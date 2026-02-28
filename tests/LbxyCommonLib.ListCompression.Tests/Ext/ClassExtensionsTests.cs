#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LbxyCommonLib.Ext;
using NUnit.Framework;

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
        public void Map_ToExistingDictionary_ShouldOverwrite()
        {
            var obj = new SimpleClass { Name = "Bob", Age = 25 };
            var dict = new Dictionary<string, string>
            {
                { "Name", "OldName" },
                { "ExistingKey", "ExistingValue" }
            };

            obj.ToPropertyDictionary(dict);

            Assert.That(dict["Name"], Is.EqualTo("Bob")); // Overwritten
            Assert.That(dict["Age"], Is.EqualTo("25"));   // Added
            Assert.That(dict["ExistingKey"], Is.EqualTo("ExistingValue")); // Preserved
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
        public void Map_WithDisplayAttributes_ShouldUseAttributeName()
        {
            var obj = new DisplayAttributeClass();
            var dict = obj.ToPropertyDictionary(options: ClassExtensions.PropertyNameOptions.UseDisplayAttributes);

            // DisplayName
            Assert.That(dict.ContainsKey("DN_Name"), Is.True);
            Assert.That(dict.ContainsKey("Name"), Is.False);

            // Display(Name=...)
            Assert.That(dict.ContainsKey("D_Description"), Is.True);
            Assert.That(dict.ContainsKey("Description"), Is.False);

            // XafDisplayName
            Assert.That(dict.ContainsKey("Xaf_Code"), Is.True);
            Assert.That(dict.ContainsKey("Code"), Is.False);

            // Priority: Xaf > DisplayName > Display
            Assert.That(dict.ContainsKey("Xaf_Priority"), Is.True);
            Assert.That(dict.ContainsKey("DN_Priority"), Is.False);
            Assert.That(dict.ContainsKey("D_Priority"), Is.False);

            // No Attribute fallback
            Assert.That(dict.ContainsKey("NoAttribute"), Is.True);
        }

        [Test]
        public void Map_WithoutDisplayAttributesOption_ShouldUsePropertyName()
        {
            var obj = new DisplayAttributeClass();
            var dict = obj.ToPropertyDictionary(options: ClassExtensions.PropertyNameOptions.Default);

            Assert.That(dict.ContainsKey("Name"), Is.True);
            Assert.That(dict.ContainsKey("DN_Name"), Is.False);
        }

        [Test]
        public void Map_NullObject_ShouldReturnTarget()
        {
            SimpleClass obj = null;
            var target = new Dictionary<string, string>();
            var result = obj.ToPropertyDictionary(target);

            Assert.That(result, Is.SameAs(target));
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Map_NullObject_NoTarget_ShouldReturnNewEmpty()
        {
            SimpleClass obj = null;
            var result = obj.ToPropertyDictionary();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Map_OnlyUpdateExisting_True_ShouldIgnoreNewKeys()
        {
            var obj = new SimpleClass { Name = "UpdatedName", Age = 99 };
            var dict = new Dictionary<string, string>
            {
                { "Name", "OldName" }
            };

            // Age is in obj but not in dict. Should be ignored.
            obj.ToPropertyDictionary(dict, options: ClassExtensions.PropertyNameOptions.Default, onlyUpdateExisting: true);

            Assert.That(dict["Name"], Is.EqualTo("UpdatedName"));
            Assert.That(dict.ContainsKey("Age"), Is.False);
        }

        [Test]
        public void Map_OnlyUpdateExisting_False_ShouldAddNewKeys()
        {
            var obj = new SimpleClass { Name = "UpdatedName", Age = 99 };
            var dict = new Dictionary<string, string>
            {
                { "Name", "OldName" }
            };

            // Default behavior (false) -> Add new keys
            obj.ToPropertyDictionary(dict, options: ClassExtensions.PropertyNameOptions.Default, onlyUpdateExisting: false);

            Assert.That(dict["Name"], Is.EqualTo("UpdatedName"));
            Assert.That(dict["Age"], Is.EqualTo("99"));
        }

        [Test]
        public void Map_OnlyUpdateExisting_True_WithNullTarget_ShouldReturnEmpty()
        {
            var obj = new SimpleClass { Name = "Test", Age = 10 };

            // Null target -> New empty dict created -> No keys existing -> Nothing added
            var result = obj.ToPropertyDictionary(null, options: ClassExtensions.PropertyNameOptions.Default, onlyUpdateExisting: true);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Map_OnlyUpdateExisting_True_WithNestedObjectProp_ShouldIgnoreIfKeyMissing()
        {
            // Although implementation treats values as strings, checking logic holds
            var obj = new { Nested = new SimpleClass { Name = "Inner" } };
            var dict = new Dictionary<string, string>();

            // Nested prop "Nested" not in dict -> Should be ignored
            obj.ToPropertyDictionary(dict, onlyUpdateExisting: true);

            Assert.That(dict, Is.Empty);
        }

        [Test]
        public void Map_OnlyUpdateExisting_True_WithNestedObjectProp_ShouldUpdateIfKeyExists()
        {
            var obj = new { Nested = new SimpleClass { Name = "Inner" } };
            var dict = new Dictionary<string, string>
             {
                 { "Nested", "OldValue" }
             };

            // Key exists -> Should update (ToString called on object)
            obj.ToPropertyDictionary(dict, onlyUpdateExisting: true);

            Assert.That(dict["Nested"], Is.EqualTo(obj.Nested.ToString()));
        }

        [Test]
        public void Map_ComplexAndCollectionTypes_ShouldCallToString()
        {
            var obj = new
            {
                Complex = new SimpleClass { Name = "Inner" },
                List = new List<int> { 1, 2, 3 },
                MyEnum = ClassExtensions.PropertyNameOptions.UseDisplayAttributes
            };

            var dict = obj.ToPropertyDictionary();

            Assert.That(dict["Complex"], Is.EqualTo(obj.Complex.ToString()));
            Assert.That(dict["List"], Is.EqualTo(obj.List.ToString()));
            Assert.That(dict["MyEnum"], Is.EqualTo("UseDisplayAttributes"));
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
    }
}
