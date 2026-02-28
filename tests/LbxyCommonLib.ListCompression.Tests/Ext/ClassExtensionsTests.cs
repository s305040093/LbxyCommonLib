#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LbxyCommonLib.Ext;
using NUnit.Framework;

using System.Runtime.Serialization;

namespace LbxyCommonLib.ListCompression.Tests.Ext
{
    // 模拟 DevExpress 的 XafDisplayNameAttribute
    [AttributeUsage(AttributeTargets.Property)]
    public class XafDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }
        public XafDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    [TestFixture]
    public class ClassExtensionsTests
    {
        private class SimpleClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public DateTime? BirthDate { get; set; }
        }

        private class PrivatePropsClass
        {
            public string PublicProp { get; set; } = "Public";
            private string PrivateProp { get; set; } = "Private";
            protected string ProtectedProp { get; set; } = "Protected";
            internal string InternalProp { get; set; } = "Internal";
            public static string StaticProp { get; set; } = "Static";
        }

        private class DisplayAttributeClass
        {
            [DisplayName("DN_Name")]
            public string Name { get; set; } = "Value1";

            [Display(Name = "D_Description")]
            public string Description { get; set; } = "Value2";

            [XafDisplayName("Xaf_Code")]
            public string Code { get; set; } = "Value3";

            [XafDisplayName("Xaf_Priority")]
            [DisplayName("DN_Priority")]
            [Display(Name = "D_Priority")]
            public string Priority { get; set; } = "Value4";

            public string NoAttribute { get; set; } = "Value5";
        }

        [Test]
        public void Map_ToNewDictionary_ShouldSucceed()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30, BirthDate = new DateTime(2000, 1, 1) };
            var dict = obj.ToPropertyDictionary();

            Assert.That(dict, Is.Not.Null);
            Assert.That(dict["Name"], Is.EqualTo("Alice"));
            Assert.That(dict["Age"], Is.EqualTo("30"));
            Assert.That(dict["BirthDate"], Is.EqualTo(new DateTime(2000, 1, 1).ToString()));
        }

        [Test]
        public void Map_NullValues_ShouldBeEmptyString()
        {
            var obj = new SimpleClass { Name = null, BirthDate = null };
            var dict = obj.ToPropertyDictionary();

            Assert.That(dict["Name"], Is.EqualTo(string.Empty));
            Assert.That(dict["BirthDate"], Is.EqualTo(string.Empty));
        }

        [Test]
        public void Map_PrivateProperties_ShouldBeIgnored()
        {
            var obj = new PrivatePropsClass();
            var dict = obj.ToPropertyDictionary();

            Assert.That(dict.ContainsKey("PublicProp"), Is.True);
            Assert.That(dict.ContainsKey("PrivateProp"), Is.False);
            Assert.That(dict.ContainsKey("ProtectedProp"), Is.False);
            Assert.That(dict.ContainsKey("InternalProp"), Is.False); // Internal is not public
            Assert.That(dict.ContainsKey("StaticProp"), Is.False);
        }

        [Test]
        public void Map_WithUseDisplayNameFalse_ShouldIgnoreDisplayAttributes()
        {
            var obj = new DisplayAttributeClass();
            var dict = obj.ToPropertyDictionary(useDisplayName: false);

            Assert.That(dict.ContainsKey("Name"), Is.True);
            Assert.That(dict.ContainsKey("Description"), Is.True);
            Assert.That(dict.ContainsKey("Code"), Is.True);
            Assert.That(dict.ContainsKey("Priority"), Is.True);
            Assert.That(dict.ContainsKey("NoAttribute"), Is.True);

            Assert.That(dict.ContainsKey("DN_Name"), Is.False);
            Assert.That(dict.ContainsKey("D_Description"), Is.False);
            Assert.That(dict.ContainsKey("Xaf_Code"), Is.False);
        }

        [Test]
        public void Map_Default_ShouldUseDisplayAttributes()
        {
            var obj = new DisplayAttributeClass();
            // Default useDisplayName is now true
            var dict = obj.ToPropertyDictionary();

            Assert.That(dict.ContainsKey("DN_Name"), Is.True);
            Assert.That(dict["DN_Name"], Is.EqualTo("Value1"));

            Assert.That(dict.ContainsKey("D_Description"), Is.True);
            Assert.That(dict["D_Description"], Is.EqualTo("Value2"));

            Assert.That(dict.ContainsKey("Xaf_Code"), Is.True);
            Assert.That(dict["Xaf_Code"], Is.EqualTo("Value3"));

            // Priority: Xaf > DisplayName > Display
            Assert.That(dict.ContainsKey("Xaf_Priority"), Is.True);
            Assert.That(dict["Xaf_Priority"], Is.EqualTo("Value4"));

            // No attribute -> fallback to property name
            Assert.That(dict.ContainsKey("NoAttribute"), Is.True);
            Assert.That(dict["NoAttribute"], Is.EqualTo("Value5"));
        }

        [Test]
        public void Map_WithUseDisplayNameTrue_ShouldUseDisplayAttributes()
        {
            var obj = new DisplayAttributeClass();
            var dict = obj.ToPropertyDictionary(useDisplayName: true);

            Assert.That(dict.ContainsKey("DN_Name"), Is.True);
            Assert.That(dict["DN_Name"], Is.EqualTo("Value1"));

            Assert.That(dict.ContainsKey("D_Description"), Is.True);
            Assert.That(dict["D_Description"], Is.EqualTo("Value2"));

            Assert.That(dict.ContainsKey("Xaf_Code"), Is.True);
            Assert.That(dict["Xaf_Code"], Is.EqualTo("Value3"));

            // Priority: Xaf > DisplayName > Display
            Assert.That(dict.ContainsKey("Xaf_Priority"), Is.True);
            Assert.That(dict["Xaf_Priority"], Is.EqualTo("Value4"));

            // No attribute -> fallback to property name
            Assert.That(dict.ContainsKey("NoAttribute"), Is.True);
            Assert.That(dict["NoAttribute"], Is.EqualTo("Value5"));
        }

        [Test]
        public void Map_NullObject_ShouldReturnEmpty()
        {
            SimpleClass obj = null;
            var result = obj.ToPropertyDictionary();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Map_ComplexAndCollectionTypes_ShouldCallToString()
        {
            var obj = new
            {
                Complex = new SimpleClass { Name = "Inner" },
                List = new List<int> { 1, 2, 3 },
                MyEnum = DayOfWeek.Monday
            };

            var dict = obj.ToPropertyDictionary();

            Assert.That(dict["Complex"], Is.EqualTo(obj.Complex.ToString()));
            Assert.That(dict["List"], Is.EqualTo(obj.List.ToString()));
            Assert.That(dict["MyEnum"], Is.EqualTo("Monday"));
        }

        [Test]
        public void Map_ObjectVariable_ShouldMapObjectPropertiesOnly()
        {
            object obj = new SimpleClass { Name = "Hidden" };
            // T is object
            var dict = obj.ToPropertyDictionary();

            // object has no public instance properties
            Assert.That(dict, Is.Empty);
        }



        [Test]
        public void MergeOrReplace_NullSource_ShouldThrowArgumentNullException()
        {
            SimpleClass obj = null;
            var src = new Dictionary<string, string>();
            Assert.Throws<ArgumentNullException>(() => obj.MergeOrReplace(src));
        }

        [Test]
        public void MergeOrReplace_NullSrc_ShouldReturnPropertyDictionary()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30 };
            var result = obj.MergeOrReplace(null);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3)); // Name, Age, BirthDate
            Assert.That(result["Name"], Is.EqualTo("Alice"));
        }

        [Test]
        public void MergeOrReplace_EmptySrc_ShouldReturnPropertyDictionary()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30 };
            var result = obj.MergeOrReplace(new Dictionary<string, string>());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result["Name"], Is.EqualTo("Alice"));
        }

        [Test]
        public void MergeOrReplace_Merge_DuplicateKeys_Overwrite_ShouldUpdateValue()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30 };
            var src = new Dictionary<string, string> { { "Name", "Bob" } };

            var result = obj.MergeOrReplace(src, LbxyCommonLib.Collections.DictionaryConflictStrategy.Overwrite);

            Assert.That(result["Name"], Is.EqualTo("Bob"));
            Assert.That(result["Age"], Is.EqualTo("30"));
        }

        [Test]
        public void MergeOrReplace_Merge_DuplicateKeys_KeepTarget_ShouldKeepOriginalValue()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30 };
            var src = new Dictionary<string, string> { { "Name", "Bob" } };

            var result = obj.MergeOrReplace(src, LbxyCommonLib.Collections.DictionaryConflictStrategy.KeepTarget);

            Assert.That(result["Name"], Is.EqualTo("Alice"));
            Assert.That(result["Age"], Is.EqualTo("30"));
        }

        [Test]
        public void MergeOrReplace_Merge_NewKeys_ShouldAdd()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30 };
            var src = new Dictionary<string, string> { { "City", "New York" } };

            var result = obj.MergeOrReplace(src);

            Assert.That(result.ContainsKey("City"), Is.True);
            Assert.That(result["City"], Is.EqualTo("New York"));
            Assert.That(result["Name"], Is.EqualTo("Alice"));
        }

        [Test]
        public void MergeOrReplace_Replace_DuplicateKeys_ShouldUpdate()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30 };
            var src = new Dictionary<string, string> { { "Name", "Bob" } };

            var result = obj.MergeOrReplace(src, isReplace: true);

            Assert.That(result["Name"], Is.EqualTo("Bob"));
            Assert.That(result["Age"], Is.EqualTo("30"));
        }

        [Test]
        public void MergeOrReplace_Replace_NewKeys_ShouldIgnore()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30 };
            var src = new Dictionary<string, string> { { "City", "New York" } };

            var result = obj.MergeOrReplace(src, isReplace: true);

            Assert.That(result.ContainsKey("City"), Is.False);
            Assert.That(result["Name"], Is.EqualTo("Alice"));
        }

        [Test]
        public void MergeOrReplace_WithUseDisplayName_ShouldRespectParameter()
        {
            var obj = new DisplayAttributeClass { Name = "Value1" };
            // useDisplayName=true by default: key should be "DN_Name"
            // src key matches "DN_Name"
            var src = new Dictionary<string, string> { { "DN_Name", "Overridden" } };

            var result = obj.MergeOrReplace(src);

            Assert.That(result.ContainsKey("DN_Name"), Is.True);
            Assert.That(result["DN_Name"], Is.EqualTo("Overridden"));
            Assert.That(result.ContainsKey("Name"), Is.False);
        }

        [Test]
        public void MergeOrReplace_WithUseDisplayNameFalse_ShouldUsePropertyNames()
        {
            var obj = new DisplayAttributeClass { Name = "Value1" };
            // useDisplayName=false: key should be "Name"
            var src = new Dictionary<string, string> { { "Name", "Overridden" } };

            var result = obj.MergeOrReplace(src, useDisplayName: false);

            Assert.That(result.ContainsKey("Name"), Is.True);
            Assert.That(result["Name"], Is.EqualTo("Overridden"));
            Assert.That(result.ContainsKey("DN_Name"), Is.False);
        }

        private class ComplexModel
        {
            public string Title { get; set; }
            public SimpleClass Child { get; set; }
        }

        private class DeepModel
        {
            public int Level { get; set; }
            public DeepModel Next { get; set; }
        }

        private class CircularModel
        {
            public string Name { get; set; }
            public CircularModel Child { get; set; }
        }

        private class IgnoredModel
        {
            public string Visible { get; set; }
            [IgnoreDataMember]
            public string Hidden1 { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public string Hidden2 { get; set; }
        }

        private class NullableModel
        {
            public int? Value { get; set; }
            public DateTime? Date { get; set; }
        }
    }
}
