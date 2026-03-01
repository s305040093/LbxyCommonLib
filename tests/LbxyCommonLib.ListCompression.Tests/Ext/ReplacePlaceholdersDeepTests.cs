#nullable disable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LbxyCommonLib.Ext;
using NUnit.Framework;
using System.Collections;

namespace LbxyCommonLib.ListCompression.Tests.Ext
{
    [TestFixture]
    public class ReplacePlaceholdersDeepTests
    {
        #region Test Models

        private class SimpleModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Description { get; set; }
        }

        private class ComplexModel
        {
            public string Title { get; set; }
            public SimpleModel Details { get; set; }
            public List<string> Tags { get; set; }
            public List<SimpleModel> RelatedItems { get; set; }
            public Dictionary<string, string> MetaData { get; set; }
            public Dictionary<string, SimpleModel> Cache { get; set; }
        }

        private class CycleModel
        {
            public string Name { get; set; }
            public CycleModel Reference { get; set; }
        }

        private class AsyncModel
        {
            public Task<string> AsyncData { get; set; }
            public Task<int> AsyncNumber { get; set; }
        }

        private class TypeConversionModel
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public TimeSpan Duration { get; set; }
            public TestEnum Status { get; set; }
            public int? NullableInt { get; set; }
            public decimal Price { get; set; }
        }

        private enum TestEnum
        {
            None,
            Active,
            Inactive
        }

        private class ReadOnlyModel
        {
            public string ReadOnlyProp { get; } = "${ReadOnly}";
            public string WritableProp { get; set; } = "${Writable}";
        }

        #endregion

        [Test]
        public void Test01_SimpleStringReplacement()
        {
            var model = new SimpleModel { Name = "${Name}", Description = "Fixed" };
            var dict = new Dictionary<string, string> { { "${Name}", "Alice" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Name, Is.EqualTo("Alice"));
            Assert.That(model.Description, Is.EqualTo("Fixed"));
        }

        [Test]
        public void Test02_SimpleIntReplacement()
        {
            var model = new SimpleModel { Age = 0 }; // 0 is default, let's say we want to set it via string? 
            // Wait, existing logic replaces strings OR value types if their ToString matches.
            // If Age is 0, ToString is "0". If dict has "0" -> "25", it replaces.
            // But usually placeholders are like "${Age}". An int property cannot hold "${Age}".
            // So for int property, we can only replace if the current value matches a key.
            // Example: Sentinel value -1 or specific number.
            
            // Let's try replacing a string property that holds a number first.
            // Re-reading logic:
            // if (prop.PropertyType.IsValueType) ... string s = currentValue.ToString(); if (lookup.TryGetValue(s, ...))
            
            model.Age = -1; 
            var dict = new Dictionary<string, string> { { "-1", "25" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Age, Is.EqualTo(25));
        }

        [Test]
        public void Test03_NestedObjectReplacement()
        {
            var model = new ComplexModel
            {
                Title = "Root",
                Details = new SimpleModel { Name = "${NestedName}" }
            };
            var dict = new Dictionary<string, string> { { "${NestedName}", "Bob" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Details.Name, Is.EqualTo("Bob"));
        }

        [Test]
        public void Test04_ListStringReplacement()
        {
            var model = new ComplexModel
            {
                Tags = new List<string> { "Tag1", "${Tag2}", "Tag3" }
            };
            var dict = new Dictionary<string, string> { { "${Tag2}", "Urgent" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Tags[1], Is.EqualTo("Urgent"));
        }

        [Test]
        public void Test05_ListObjectReplacement()
        {
            var model = new ComplexModel
            {
                RelatedItems = new List<SimpleModel>
                {
                    new SimpleModel { Name = "Item1" },
                    new SimpleModel { Name = "${Item2}" }
                }
            };
            var dict = new Dictionary<string, string> { { "${Item2}", "FixedItem" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.RelatedItems[1].Name, Is.EqualTo("FixedItem"));
        }

        [Test]
        public void Test06_DictionaryValueStringReplacement()
        {
            var model = new ComplexModel
            {
                MetaData = new Dictionary<string, string>
                {
                    { "Key1", "Value1" },
                    { "Key2", "${Val2}" }
                }
            };
            var dict = new Dictionary<string, string> { { "${Val2}", "ReplacedValue" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.MetaData["Key2"], Is.EqualTo("ReplacedValue"));
        }

        [Test]
        public void Test07_DictionaryValueObjectReplacement()
        {
            var model = new ComplexModel
            {
                Cache = new Dictionary<string, SimpleModel>
                {
                    { "Entry1", new SimpleModel { Name = "${CacheName}" } }
                }
            };
            var dict = new Dictionary<string, string> { { "${CacheName}", "CachedObject" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Cache["Entry1"].Name, Is.EqualTo("CachedObject"));
        }

        [Test]
        public void Test08_CycleDetection_ShouldNotStackOverflow()
        {
            var a = new CycleModel { Name = "${A}" };
            var b = new CycleModel { Name = "${B}" };
            a.Reference = b;
            b.Reference = a;

            var dict = new Dictionary<string, string>
            {
                { "${A}", "Alpha" },
                { "${B}", "Beta" }
            };

            // Should finish successfully
            a.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(a.Name, Is.EqualTo("Alpha"));
            Assert.That(a.Reference.Name, Is.EqualTo("Beta"));
            Assert.That(a.Reference.Reference, Is.SameAs(a));
        }

        [Test]
        public void Test09_TypeConversion_DateTime()
        {
            // Note: DateTime is a struct. Default is MinValue.
            // We need to match MinValue.ToString() if we want to replace default, 
            // OR set a specific sentinel date.
            var sentinel = new DateTime(1900, 1, 1);
            var model = new TypeConversionModel { CreatedAt = sentinel };
            
            var targetDate = new DateTime(2023, 10, 1);
            var dict = new Dictionary<string, string> 
            { 
                { sentinel.ToString(), targetDate.ToString() } 
            };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.CreatedAt, Is.EqualTo(targetDate));
        }

        [Test]
        public void Test10_TypeConversion_Guid()
        {
            var sentinel = Guid.Empty;
            var model = new TypeConversionModel { Id = sentinel };
            
            var targetGuid = Guid.NewGuid();
            var dict = new Dictionary<string, string> 
            { 
                { sentinel.ToString(), targetGuid.ToString() } 
            };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Id, Is.EqualTo(targetGuid));
        }

        [Test]
        public void Test11_TypeConversion_Enum()
        {
            var model = new TypeConversionModel { Status = TestEnum.None };
            
            // "None" is the default (0).
            var dict = new Dictionary<string, string> 
            { 
                { "None", "Active" } 
            };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Status, Is.EqualTo(TestEnum.Active));
        }

        [Test]
        public void Test12_TypeConversion_NullableInt()
        {
            // Nullable int is null by default. Logic:
            // currentValue == null -> continue.
            // So we cannot replace null with a value.
            // We must have a non-null placeholder value.
            
            var model = new TypeConversionModel { NullableInt = -999 };
            var dict = new Dictionary<string, string> 
            { 
                { "-999", "123" } 
            };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.NullableInt, Is.EqualTo(123));
        }

        [Test]
        public void Test13_TypeConversion_TimeSpan()
        {
            var sentinel = TimeSpan.Zero;
            var model = new TypeConversionModel { Duration = sentinel };
            
            var target = TimeSpan.FromMinutes(5);
            var dict = new Dictionary<string, string> 
            { 
                { sentinel.ToString(), target.ToString() } 
            };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Duration, Is.EqualTo(target));
        }

        [Test]
        public async Task Test14_Async_TaskString_Replacement()
        {
            var model = new AsyncModel { AsyncData = Task.FromResult("${Data}") };
            var dict = new Dictionary<string, string> { { "${Data}", "Loaded" } };

            await model.ReplacePlaceholdersFromDictionaryAsync(dict);

            var result = await model.AsyncData;
            Assert.That(result, Is.EqualTo("Loaded"));
        }

        [Test]
        public async Task Test15_Async_TaskInt_Replacement()
        {
            // Task<int> returning -1
            var model = new AsyncModel { AsyncNumber = Task.FromResult(-1) };
            var dict = new Dictionary<string, string> { { "-1", "42" } };

            await model.ReplacePlaceholdersFromDictionaryAsync(dict);

            var result = await model.AsyncNumber;
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void Test16_CaseInsensitiveReplacement()
        {
            var model = new SimpleModel { Name = "${NAME}" };
            var dict = new Dictionary<string, string> { { "${name}", "Alice" } };

            // Default is case sensitive (Ordinal) usually if not specified, 
            // BUT Dictionary lookups depend on the dictionary's comparer.
            // The method creates a NEW dictionary if comparer is provided.
            
            model.ReplacePlaceholdersFromDictionary(dict, StringComparer.OrdinalIgnoreCase);

            Assert.That(model.Name, Is.EqualTo("Alice"));
        }

        [Test]
        public void Test17_ReadOnlyProperty_ShouldNotChange()
        {
            var model = new ReadOnlyModel { WritableProp = "${Writable}" };
            // ReadOnlyProp is "${ReadOnly}" but has no setter.
            
            var dict = new Dictionary<string, string> 
            { 
                { "${ReadOnly}", "Changed" },
                { "${Writable}", "Changed" }
            };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.ReadOnlyProp, Is.EqualTo("${ReadOnly}"));
            Assert.That(model.WritableProp, Is.EqualTo("Changed"));
        }

        [Test]
        public void Test18_NullSource_ThrowsException()
        {
            SimpleModel model = null;
            var dict = new Dictionary<string, string>();
            Assert.Throws<ArgumentNullException>(() => model.ReplacePlaceholdersFromDictionary(dict));
        }

        [Test]
        public void Test19_NullDictionary_ThrowsException()
        {
            var model = new SimpleModel();
            Assert.Throws<ArgumentNullException>(() => model.ReplacePlaceholdersFromDictionary(null));
        }

        [Test]
        public void Test20_EmptyDictionary_DoNothing()
        {
            var model = new SimpleModel { Name = "${Name}" };
            var dict = new Dictionary<string, string>();
            
            model.ReplacePlaceholdersFromDictionary(dict);
            
            Assert.That(model.Name, Is.EqualTo("${Name}"));
        }

        [Test]
        public void Test21_ArrayStringReplacement()
        {
            string[] arr = new[] { "${A}", "B" };
            var dict = new Dictionary<string, string> { { "${A}", "Alpha" } };
            
            // Array implements IList
            var replacer = new ObjectReplacer(dict, System.Threading.CancellationToken.None, false);
            replacer.Process(arr);

            Assert.That(arr[0], Is.EqualTo("Alpha"));
        }

        [Test]
        public void Test22_ArrayObjectReplacement()
        {
            var arr = new[] 
            { 
                new SimpleModel { Name = "${A}" } 
            };
            var dict = new Dictionary<string, string> { { "${A}", "Alpha" } };
            
            // We can't call extension method on array directly if T : class constraint issues arise or if we want to test deep traversal from an object holding array
            // But T is class, Array is class.
            arr.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(arr[0].Name, Is.EqualTo("Alpha"));
        }

        [Test]
        public void Test23_CollectionOfCollections()
        {
            var list = new List<List<string>>
            {
                new List<string> { "${A}" }
            };
            var dict = new Dictionary<string, string> { { "${A}", "Alpha" } };

            list.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(list[0][0], Is.EqualTo("Alpha"));
        }

        [Test]
        public void Test24_UnrelatedProperties_Ignored()
        {
            var model = new SimpleModel { Name = "John", Age = 30 };
            var dict = new Dictionary<string, string> { { "Jane", "Doe" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Name, Is.EqualTo("John"));
            Assert.That(model.Age, Is.EqualTo(30));
        }

        [Test]
        public void Test25_PartialReplacement_NotSupported()
        {
            // The current implementation does FULL match replacement, not substring.
            // e.g. "Hello ${Name}" with { "${Name}", "World" } -> "Hello ${Name}"
            // It only replaces if the WHOLE string matches the key.
            
            var model = new SimpleModel { Name = "Hello ${Name}" };
            var dict = new Dictionary<string, string> { { "${Name}", "World" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Name, Is.EqualTo("Hello ${Name}"));
        }
        
        [Test]
        public void Test26_DeepHierarchy()
        {
            var root = new ComplexModel
            {
                Title = "L1",
                Details = new SimpleModel { Name = "L2" },
                RelatedItems = new List<SimpleModel>
                {
                    new SimpleModel 
                    { 
                        Name = "L3",
                        Description = "${Deep}"
                    }
                }
            };
            
            var dict = new Dictionary<string, string> { { "${Deep}", "FoundIt" } };
            root.ReplacePlaceholdersFromDictionary(dict);
            
            Assert.That(root.RelatedItems[0].Description, Is.EqualTo("FoundIt"));
        }

        [Test]
        public void Test27_ConvertFailure_ShouldSkip()
        {
            // Int property, but replacement string is not a number
            var model = new SimpleModel { Age = -1 };
            var dict = new Dictionary<string, string> { { "-1", "NotANumber" } };

            model.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(model.Age, Is.EqualTo(-1));
        }

        [Test]
        public async Task Test28_Async_CollectionProcessing()
        {
            var list = new List<Task<string>>
            {
                Task.FromResult("${A}"),
                Task.FromResult("B")
            };
            
            var dict = new Dictionary<string, string> { { "${A}", "Alpha" } };

            await list.ReplacePlaceholdersFromDictionaryAsync(dict);

            Assert.That(await list[0], Is.EqualTo("Alpha"));
            Assert.That(await list[1], Is.EqualTo("B"));
        }
        
        [Test]
        public void Test29_DictionaryKeys_AreNotReplaced()
        {
            // Logic: ProcessDictionary iterates entries. 
            // If value is string/valuetype -> replace value.
            // If value is object -> recurse value.
            // Keys are NOT replaced.
            
            var dict = new Dictionary<string, string> { { "${Key}", "Value" } };
            var replacements = new Dictionary<string, string> { { "${Key}", "NewKey" } };
            
            dict.ReplacePlaceholdersFromDictionary(replacements);
            
            Assert.That(dict.ContainsKey("${Key}"), Is.True);
            Assert.That(dict.ContainsKey("NewKey"), Is.False);
        }

        [Test]
        public void Test30_MixedList_ValueTypesAndObjects()
        {
            // ArrayList or List<object>
            var list = new List<object>
            {
                "${Str}",
                -1,
                new SimpleModel { Name = "${Name}" }
            };
            
            var dict = new Dictionary<string, string> 
            { 
                { "${Str}", "StringRep" },
                { "-1", "100" },
                { "${Name}", "ObjRep" }
            };

            list.ReplacePlaceholdersFromDictionary(dict);

            Assert.That(list[0], Is.EqualTo("StringRep"));
            Assert.That(list[1], Is.EqualTo(100)); // Should convert to int? Or string "100"?
            // ConvertValue logic: targetType is item.GetType(). 
            // item is int (-1). So targetType is int. "100" -> 100.
            Assert.That(list[1], Is.InstanceOf<int>());
            Assert.That(list[1], Is.EqualTo(100));
            
            Assert.That(((SimpleModel)list[2]).Name, Is.EqualTo("ObjRep"));
        }
        
        [Test]
        public void Test31_StructInBoxedObject()
        {
            // Boxed struct in object property
            var list = new List<object> { 123 };
            var dict = new Dictionary<string, string> { { "123", "456" } };
            
            list.ReplacePlaceholdersFromDictionary(dict);
            
            Assert.That(list[0], Is.EqualTo(456));
        }
    }
}
