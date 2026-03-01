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

        private class PlaceholderModel
        {
            public string Text { get; set; }
            public int Number { get; set; }
            public double Floating { get; set; }
            public bool Flag { get; set; }
            public DayOfWeek WeekDay { get; set; }
            public Guid Identifier { get; set; }
            public int? NullableInt { get; set; }
        }

        [Test]
        public void ReplacePlaceholders_BasicStringReplacement_ShouldSucceed()
        {
            var obj = new PlaceholderModel { Text = "${Name}" };
            var map = new Dictionary<string, string> { { "${Name}", "Alice" } };

            obj.ReplacePlaceholdersFromDictionary(map);

            Assert.That(obj.Text, Is.EqualTo("Alice"));
        }

        [Test]
        public void ReplacePlaceholders_NoMatch_ShouldKeepOriginal()
        {
            var obj = new PlaceholderModel { Text = "Original" };
            var map = new Dictionary<string, string> { { "${Name}", "Alice" } };

            obj.ReplacePlaceholdersFromDictionary(map);

            Assert.That(obj.Text, Is.EqualTo("Original"));
        }

        [Test]
        public void ReplacePlaceholders_ValueTypes_ShouldConvert()
        {
            // Initial values: Number=0 (default), but let's set them to placeholders?
            // Wait, value types can't hold "${Placeholder}" string.
            // Ah, the logic is: property value (as string) matches key.
            // If int Number = 100. String is "100".
            // Map: { "100", "200" } -> Number becomes 200.

            var obj = new PlaceholderModel { Number = 100, Floating = 1.5, Flag = false };
            var map = new Dictionary<string, string>
            {
                { "100", "200" },
                { "1.5", "2.5" },
                { "False", "True" } // bool.ToString() is "False"
            };

            obj.ReplacePlaceholdersFromDictionary(map);

            Assert.That(obj.Number, Is.EqualTo(200));
            Assert.That(obj.Floating, Is.EqualTo(2.5));
            Assert.That(obj.Flag, Is.True);
        }

        [Test]
        public void ReplacePlaceholders_NullableTypes_ShouldConvert()
        {
            var obj = new PlaceholderModel { NullableInt = 123 };
            var map = new Dictionary<string, string>
            {
                { "123", "456" },
                // Test setting to null?
                // If map value is empty string?
            };

            obj.ReplacePlaceholdersFromDictionary(map);

            Assert.That(obj.NullableInt, Is.EqualTo(456));
        }

        [Test]
        public void ReplacePlaceholders_NullableTypes_SetToNull_ShouldSucceed()
        {
            var obj = new PlaceholderModel { NullableInt = 123 };
            var map = new Dictionary<string, string> { { "123", "" } }; // Empty string -> null

            obj.ReplacePlaceholdersFromDictionary(map);

            Assert.That(obj.NullableInt, Is.Null);
        }

        [Test]
        public void ReplacePlaceholders_Enum_ShouldParse()
        {
            var obj = new PlaceholderModel { WeekDay = DayOfWeek.Monday };
            var map = new Dictionary<string, string> { { "Monday", "Friday" } };

            obj.ReplacePlaceholdersFromDictionary(map);

            Assert.That(obj.WeekDay, Is.EqualTo(DayOfWeek.Friday));
        }

        [Test]
        public void ReplacePlaceholders_Guid_ShouldParse()
        {
            var g1 = Guid.NewGuid();
            var g2 = Guid.NewGuid();
            var obj = new PlaceholderModel { Identifier = g1 };
            var map = new Dictionary<string, string> { { g1.ToString(), g2.ToString() } };

            obj.ReplacePlaceholdersFromDictionary(map);

            Assert.That(obj.Identifier, Is.EqualTo(g2));
        }

        [Test]
        public void ReplacePlaceholders_NullSource_ShouldThrow()
        {
            PlaceholderModel obj = null;
            Assert.Throws<ArgumentNullException>(() => obj.ReplacePlaceholdersFromDictionary(new Dictionary<string, string>()));
        }

        [Test]
        public void ReplacePlaceholders_EmptyMap_ShouldReturnOriginal()
        {
            var obj = new PlaceholderModel { Text = "${Name}" };
            var map = new Dictionary<string, string>();

            obj.ReplacePlaceholdersFromDictionary(map);

            Assert.That(obj.Text, Is.EqualTo("${Name}"));
        }

        [Test]
        public void ReplacePlaceholders_CaseSensitivity_ShouldRespectComparer()
        {
            var obj = new PlaceholderModel { Text = "${NAME}" };
            var map = new Dictionary<string, string> { { "${name}", "Alice" } }; // Lowercase key

            // Default (Ordinal) - No match
            obj.ReplacePlaceholdersFromDictionary(map);
            Assert.That(obj.Text, Is.EqualTo("${NAME}"));

            // IgnoreCase
            obj.ReplacePlaceholdersFromDictionary(map, StringComparer.OrdinalIgnoreCase);
            Assert.That(obj.Text, Is.EqualTo("Alice"));
        }

        [Test]
        public async System.Threading.Tasks.Task ReplacePlaceholdersAsync_ShouldWork()
        {
            var obj = new PlaceholderModel { Text = "${Name}" };
            var map = new Dictionary<string, string> { { "${Name}", "Alice" } };

            await obj.ReplacePlaceholdersFromDictionaryAsync(map);

            Assert.That(obj.Text, Is.EqualTo("Alice"));
        }

        [Test]
        public void ReplacePlaceholders_ThreadSafety_ConcurrentCalls_ShouldSucceed()
        {
            // PropertyAccessor cache is static. Ensure concurrent access doesn't crash.
            var map = new Dictionary<string, string> { { "${Name}", "Alice" } };

            System.Threading.Tasks.Parallel.For(0, 100, i =>
            {
                var obj = new PlaceholderModel { Text = "${Name}" };
                obj.ReplacePlaceholdersFromDictionary(map);
                Assert.That(obj.Text, Is.EqualTo("Alice"));
            });
        }

        #region ToPropertyDictionaryWithReplacement Tests

        [Test]
        public void ToPropertyDictionaryWithReplacement_ShouldReplaceValues_AndNotModifySource()
        {
            var obj = new SimpleClass { Name = "${Name}", Age = 30 };
            var map = new Dictionary<string, string> { { "${Name}", "Bob" }, { "30", "40" } };

            // Act
            var result = obj.ToPropertyDictionaryWithReplacement(map);

            // Assert Dictionary Content
            Assert.That(result["Name"], Is.EqualTo("Bob"));
            Assert.That(result["Age"], Is.EqualTo("40"));

            // Assert Source Unchanged
            Assert.That(obj.Name, Is.EqualTo("${Name}"));
            Assert.That(obj.Age, Is.EqualTo(30));
        }

        [Test]
        public void ToPropertyDictionaryWithReplacement_Equivalence_OrderIndependent()
        {
            // Verify: ToDictionary -> Replace == Clone -> Replace -> ToDictionary
            // Note: This only holds true for valid replacements compatible with property types.
            var obj = new SimpleClass { Name = "${Name}", Age = 30, BirthDate = new DateTime(2000, 1, 1) };
            var map = new Dictionary<string, string>
            {
                { "${Name}", "Bob" },
                { "30", "40" },
                { new DateTime(2000, 1, 1).ToString(), new DateTime(2001, 1, 1).ToString() }
            };

            // 1. New Method (Safe)
            var result1 = obj.ToPropertyDictionaryWithReplacement(map);

            // 2. Manual Clone -> Replace -> ToDictionary (Unsafe simulation)
            var clone = new SimpleClass
            {
                Name = obj.Name,
                Age = obj.Age,
                BirthDate = obj.BirthDate
            };
            clone.ReplacePlaceholdersFromDictionary(map);
            var result2 = clone.ToPropertyDictionary();

            Assert.That(result1, Is.EquivalentTo(result2));
        }

        [Test]
        public void ToPropertyDictionaryWithReplacement_NullSource_ShouldThrow()
        {
            SimpleClass obj = null;
            Assert.Throws<ArgumentNullException>(() => obj.ToPropertyDictionaryWithReplacement(new Dictionary<string, string>()));
        }

        [Test]
        public void ToPropertyDictionaryWithReplacement_NullMap_ShouldReturnOriginal()
        {
            var obj = new SimpleClass { Name = "Alice", Age = 30 };
            var result = obj.ToPropertyDictionaryWithReplacement(null);

            Assert.That(result["Name"], Is.EqualTo("Alice"));
            Assert.That(result["Age"], Is.EqualTo("30"));
        }

        [Test]
        public void ToPropertyDictionaryWithReplacement_ReadOnlyProp_ShouldBeReplacedInDict()
        {
            // Even though ReadOnlyProp is read-only on object, the dictionary is just strings.
            // So replacement should happen in the dictionary.
            var obj = new ReadOnlyPropClass();
            var map = new Dictionary<string, string> { { "Original", "Replaced" } };

            var result = obj.ToPropertyDictionaryWithReplacement(map);

            Assert.That(result["ReadOnly"], Is.EqualTo("Replaced"));
            Assert.That(obj.ReadOnly, Is.EqualTo("Original"));
        }

        private class ReadOnlyPropClass
        {
            public string ReadOnly { get; } = "Original";
        }

        #endregion
    }
}
