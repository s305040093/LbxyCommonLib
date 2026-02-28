#nullable disable
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LbxyCommonLib.Ext;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests.Ext
{
    [TestFixture]
    public class PropertyAccessorDisplayNameTests
    {
        private class DisplayNameTestModel
        {
            [DisplayName("Display_Name")]
            public string Name { get; set; } = "DefaultName";

            [Display(Name = "Display_Age")]
            public int Age { get; set; } = 18;

            [XafDisplayName("Display_Code")]
            public string Code { get; set; } = "C001";

            public string NoAttribute { get; set; } = "NoAttribute";

            [DisplayName("DUPLICATE")]
            public string Duplicate1 { get; set; } = "1";

            [DisplayName("DUPLICATE")]
            public string Duplicate2 { get; set; } = "2";
        }

        [Test]
        public void Access_By_PropertyName_Should_Work_With_Explicit_Flag()
        {
            var model = new DisplayNameTestModel();

            // 1. GetDisplayName by PropertyName (Explicit useDisplayName: false)
            Assert.That(PropertyAccessor.GetDisplayName<DisplayNameTestModel>(nameof(DisplayNameTestModel.Name), useDisplayName: false), Is.EqualTo("Display_Name"));

            // 2. GetValue by PropertyName (Explicit useDisplayName: false)
            Assert.That(PropertyAccessor.GetValue(model, nameof(DisplayNameTestModel.Name), useDisplayName: false), Is.EqualTo("DefaultName"));

            // 3. SetValue by PropertyName (Explicit useDisplayName: false)
            PropertyAccessor.SetValue(model, nameof(DisplayNameTestModel.Name), "NewName", useDisplayName: false);
            Assert.That(model.Name, Is.EqualTo("NewName"));

            // 4. Fallback to DisplayName (Default behavior is now strict DisplayName lookup, so this works naturally)
            Assert.That(PropertyAccessor.GetValue(model, "Display_Name"), Is.EqualTo("NewName"));
        }

        [Test]
        public void UseDisplayName_True_ShouldFindByDisplayName()
        {
            var model = new DisplayNameTestModel();

            // GetValue
            Assert.That(PropertyAccessor.GetValue(model, "Display_Name", useDisplayName: true), Is.EqualTo("DefaultName"));
            Assert.That(PropertyAccessor.GetValue(model, "Display_Age", useDisplayName: true), Is.EqualTo(18));
            Assert.That(PropertyAccessor.GetValue(model, "Display_Code", useDisplayName: true), Is.EqualTo("C001"));

            // SetValue
            PropertyAccessor.SetValue(model, "Display_Name", "UpdatedName", useDisplayName: true);
            Assert.That(model.Name, Is.EqualTo("UpdatedName"));
        }

        [Test]
        public void UseDisplayName_True_ShouldFailForPropertyName()
        {
            var model = new DisplayNameTestModel();

            // When useDisplayName is true, PropertyName "Name" should not be found (unless it happens to be the DisplayName, which is "Display_Name" here)
            var ex = Assert.Throws<ArgumentException>(() =>
                PropertyAccessor.GetValue(model, nameof(DisplayNameTestModel.Name), useDisplayName: true));

            Assert.That(ex.Message, Does.Contain("Property with DisplayName 'Name' not found"));
            Assert.That(ex.Message, Does.Contain("Available DisplayNames"));
            Assert.That(ex.Message, Does.Contain("Display_Name"));
        }

        [Test]
        public void UseDisplayName_True_CaseInsensitive_Default()
        {
            var model = new DisplayNameTestModel();

            // Default comparison is OrdinalIgnoreCase
            Assert.That(PropertyAccessor.GetValue(model, "display_name", useDisplayName: true), Is.EqualTo("DefaultName"));
            Assert.That(PropertyAccessor.GetValue(model, "DISPLAY_AGE", useDisplayName: true), Is.EqualTo(18));
        }

        [Test]
        public void UseDisplayName_True_CaseSensitive_ShouldFail()
        {
            var model = new DisplayNameTestModel();

            // Explicit Ordinal comparison
            var ex = Assert.Throws<ArgumentException>(() =>
                PropertyAccessor.GetValue(model, "display_name", useDisplayName: true, comparison: StringComparison.Ordinal));

            Assert.That(ex.Message, Does.Contain("not found"));
        }

        [Test]
        public void UseDisplayName_True_DuplicateDisplayNames_ShouldUseFirstOne()
        {
            var model = new DisplayNameTestModel { Duplicate1 = "1", Duplicate2 = "2" };

            // Both Duplicate1 and Duplicate2 have DisplayName "DUPLICATE".
            // PropertyAccessor<T> constructor logic should register the first one encountered.
            // Usually reflection returns properties in declaration order, so Duplicate1 comes first.

            var value = PropertyAccessor.GetValue(model, "DUPLICATE", useDisplayName: true);
            Assert.That(value, Is.EqualTo("1"));

            PropertyAccessor.SetValue(model, "DUPLICATE", "Updated1", useDisplayName: true);
            Assert.That(model.Duplicate1, Is.EqualTo("Updated1"));
            Assert.That(model.Duplicate2, Is.EqualTo("2")); // Should remain unchanged
        }

        [Test]
        public void GetDisplayName_UseDisplayName_True_ShouldReturnDisplayName()
        {
            // When querying by DisplayName, it should return the DisplayName itself (trivial, but verifies lookup works)
            var dn = PropertyAccessor.GetDisplayName<DisplayNameTestModel>("Display_Name", useDisplayName: true);
            Assert.That(dn, Is.EqualTo("Display_Name"));
        }

        [Test]
        public void GetDisplayName_UseDisplayName_True_ShouldFailForPropertyName()
        {
            // GetDisplayName usually returns input if not found? 
            // Wait, let's check PropertyAccessor<T>.GetDisplayName logic.
            // It calls GetMetadata. If GetMetadata returns null, it returns propertyName.
            // But GetMetadata throws exception? No, GetMetadata returns null.
            // GetValue/SetValue throw exception if GetMetadata returns null.
            // GetDisplayName implementation:
            // var meta = GetMetadata(propertyName, useDisplayName, comparison);
            // return meta != null ? meta.DisplayName : propertyName;

            // So if useDisplayName=true and we pass "Name" (which is not a DisplayName), GetMetadata returns null.
            // Then it returns "Name".

            var result = PropertyAccessor.GetDisplayName<DisplayNameTestModel>("Name", useDisplayName: true);
            Assert.That(result, Is.EqualTo("Name"));
        }

        [Test]
        public void ExceptionMessage_ShouldListAvailableNames()
        {
            var model = new DisplayNameTestModel();
            var ex = Assert.Throws<ArgumentException>(() =>
               PropertyAccessor.GetValue(model, "NonExistent", useDisplayName: true));

            // Verify the list contains at least one known display name
            Assert.That(ex.Message, Does.Contain("Display_Name"));
            Assert.That(ex.Message, Does.Contain("Display_Age"));
        }
    }
}
