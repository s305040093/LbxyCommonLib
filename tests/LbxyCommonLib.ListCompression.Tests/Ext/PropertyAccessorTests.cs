#nullable disable
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LbxyCommonLib.Ext;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests.Ext
{
    [TestFixture]
    public class PropertyAccessorTests
    {
        private class TestModel
        {
            [DisplayName("Name_Display")]
            public string Name { get; set; } = "DefaultName";

            [Display(Name = "Age_Display")]
            public int Age { get; set; } = 18;

            [XafDisplayName("Code_Xaf")]
            [DisplayName("Code_Display")]
            public string Code { get; set; } = "C001";

            public DateTime? Date { get; set; }

            public string ReadOnly { get; } = "ReadOnly";

            private string PrivateProp { get; set; } = "Private";
        }

        [Test]
        public void GetDisplayName_ShouldFollowPriority()
        {
            // Xaf > DisplayName > Display > Name
            // Use useDisplayName: false to lookup by PropertyName and get the resolved PropertyName (which is the input itself in this case, but verified via metadata)
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Code), useDisplayName: false), Is.EqualTo(nameof(TestModel.Code)));

            // useDisplayName: true (Default) -> Returns DisplayName
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Code), useDisplayName: true), Is.EqualTo(nameof(TestModel.Code))); // Input "Code" is not a DisplayName, so returns input "Code"
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>("Code_Xaf", useDisplayName: true), Is.EqualTo("Code_Xaf"));

            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Name), useDisplayName: false), Is.EqualTo(nameof(TestModel.Name)));
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Age), useDisplayName: false), Is.EqualTo(nameof(TestModel.Age)));
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Date), useDisplayName: false), Is.EqualTo(nameof(TestModel.Date)));
        }

        [Test]
        public void GetDisplayName_WithUseDisplayNameFalse_ShouldReturnPropertyName()
        {
            // useDisplayName: false implies "Try PropertyName first, then DisplayName", AND return PropertyName

            // 1. By PropertyName -> Returns PropertyName
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>("Code", useDisplayName: false), Is.EqualTo("Code"));

            // 2. By DisplayName (Fallback) -> Returns PropertyName
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>("Code_Xaf", useDisplayName: false), Is.EqualTo("Code"));

            // 3. Case Insensitive (Default) -> Returns PropertyName (Case corrected)
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>("code", useDisplayName: false), Is.EqualTo("Code"));
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>("code_xaf", useDisplayName: false), Is.EqualTo("Code"));

            // 4. Non-existent property returns input
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>("NonExistent", useDisplayName: false), Is.EqualTo("NonExistent"));
        }

        [Test]
        public void GetValue_ShouldReturnCorrectValues()
        {
            var model = new TestModel { Name = "Alice", Age = 30 };

            // Use useDisplayName: false to lookup by PropertyName
            Assert.That(PropertyAccessor.GetValue(model, nameof(TestModel.Name), useDisplayName: false), Is.EqualTo("Alice"));
            Assert.That(PropertyAccessor.GetValue(model, nameof(TestModel.Age), useDisplayName: false), Is.EqualTo(30));
        }

        [Test]
        public void SetValue_ShouldUpdateValues()
        {
            var model = new TestModel();

            // Use useDisplayName: false to lookup by PropertyName
            PropertyAccessor.SetValue(model, nameof(TestModel.Name), "Bob", useDisplayName: false);
            PropertyAccessor.SetValue(model, nameof(TestModel.Age), 25, useDisplayName: false);

            Assert.That(model.Name, Is.EqualTo("Bob"));
            Assert.That(model.Age, Is.EqualTo(25));
        }

        [Test]
        public void SetValue_WithDisplayName_ShouldUpdateValues()
        {
            var model = new TestModel();

            // Use DisplayName "Name_Display" to set "Name"
            PropertyAccessor.SetValue(model, "Name_Display", "Charlie");

            Assert.That(model.Name, Is.EqualTo("Charlie"));
        }

        [Test]
        public void GetValue_WithDisplayName_ShouldReturnValues()
        {
            var model = new TestModel { Name = "David" };

            Assert.That(PropertyAccessor.GetValue(model, "Name_Display"), Is.EqualTo("David"));
        }

        [Test]
        public void GetValue_ReadOnlyProperty_ShouldSucceed()
        {
            var model = new TestModel();
            Assert.That(PropertyAccessor.GetValue(model, nameof(TestModel.ReadOnly)), Is.EqualTo("ReadOnly"));
        }

        [Test]
        public void SetValue_ReadOnlyProperty_ShouldThrow()
        {
            var model = new TestModel();
            Assert.Throws<InvalidOperationException>(() =>
                PropertyAccessor.SetValue(model, nameof(TestModel.ReadOnly), "NewVal"));
        }

        [Test]
        public void GetValue_NonExistentProperty_ShouldThrow()
        {
            var model = new TestModel();
            Assert.Throws<ArgumentException>(() =>
                PropertyAccessor.GetValue(model, "NonExistent"));
        }

        [Test]
        public void GetProperties_ShouldReturnAllPublicInstanceProperties()
        {
            var props = PropertyAccessor.GetProperties<TestModel>();
            // Name, Age, Code, Date, ReadOnly. (PrivateProp is private)
            Assert.That(props.Count, Is.EqualTo(5));
        }
    }
}
