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
            // Use useDisplayName: false to lookup by PropertyName and get the resolved DisplayName
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Code), useDisplayName: false), Is.EqualTo("Code_Xaf"));
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Name), useDisplayName: false), Is.EqualTo("Name_Display"));
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Age), useDisplayName: false), Is.EqualTo("Age_Display"));
            Assert.That(PropertyAccessor.GetDisplayName<TestModel>(nameof(TestModel.Date), useDisplayName: false), Is.EqualTo("Date"));
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
