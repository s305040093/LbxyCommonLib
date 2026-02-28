using System;
using System.Collections.Generic;
using LbxyCommonLib.Collections;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests.Collections
{
    [TestFixture]
    public class DictionaryOperationsTests
    {
        [Test]
        public void Merge_Shallow_Overwrite_ShouldUpdateExistingAndAddNew()
        {
            var target = new Dictionary<string, string> { { "A", "1" }, { "B", "2" } };
            var source = new Dictionary<string, string> { { "B", "20" }, { "C", "3" } };

            DictionaryOperations.Merge(target, source, DictionaryMergeMode.Shallow, DictionaryConflictStrategy.Overwrite);

            Assert.That(target["A"], Is.EqualTo("1"));
            Assert.That(target["B"], Is.EqualTo("20"));
            Assert.That(target["C"], Is.EqualTo("3"));
            Assert.That(target.Count, Is.EqualTo(3));
        }

        [Test]
        public void Merge_Shallow_KeepTarget_ShouldIgnoreExistingAndAddNew()
        {
            var target = new Dictionary<string, string> { { "A", "1" }, { "B", "2" } };
            var source = new Dictionary<string, string> { { "B", "20" }, { "C", "3" } };

            DictionaryOperations.Merge(target, source, DictionaryMergeMode.Shallow, DictionaryConflictStrategy.KeepTarget);

            Assert.That(target["A"], Is.EqualTo("1"));
            Assert.That(target["B"], Is.EqualTo("2"));
            Assert.That(target["C"], Is.EqualTo("3"));
        }

        [Test]
        public void Merge_Shallow_Throw_ShouldThrowOnConflict()
        {
            var target = new Dictionary<string, string> { { "A", "1" }, { "B", "2" } };
            var source = new Dictionary<string, string> { { "B", "20" }, { "C", "3" } };

            Assert.Throws<ArgumentException>(() =>
                DictionaryOperations.Merge(target, source, DictionaryMergeMode.Shallow, DictionaryConflictStrategy.Throw));
        }

        [Test]
        public void Merge_Deep_Overwrite_ShouldMergeNestedDictionaries()
        {
            var nestedTarget = new Dictionary<string, object> { { "SubA", 1 } };
            var target = new Dictionary<string, object> { { "RootA", 10 }, { "Nested", nestedTarget } };

            var nestedSource = new Dictionary<string, object> { { "SubB", 2 }, { "SubA", 100 } };
            var source = new Dictionary<string, object> { { "RootB", 20 }, { "Nested", nestedSource } };

            DictionaryOperations.Merge(target, source, DictionaryMergeMode.Deep, DictionaryConflictStrategy.Overwrite);

            Assert.That(target["RootA"], Is.EqualTo(10));
            Assert.That(target["RootB"], Is.EqualTo(20));

            var mergedNested = target["Nested"] as Dictionary<string, object>;
            Assert.That(mergedNested, Is.Not.Null);
            Assert.That(mergedNested["SubA"], Is.EqualTo(100)); // Overwritten
            Assert.That(mergedNested["SubB"], Is.EqualTo(2));   // Added
        }

        [Test]
        public void Merge_Deep_KeepTarget_ShouldMergeNestedDictionariesButKeepValues()
        {
            var nestedTarget = new Dictionary<string, object> { { "SubA", 1 } };
            var target = new Dictionary<string, object> { { "RootA", 10 }, { "Nested", nestedTarget } };

            var nestedSource = new Dictionary<string, object> { { "SubB", 2 }, { "SubA", 100 } };
            var source = new Dictionary<string, object> { { "RootB", 20 }, { "Nested", nestedSource } };

            DictionaryOperations.Merge(target, source, DictionaryMergeMode.Deep, DictionaryConflictStrategy.KeepTarget);

            var mergedNested = target["Nested"] as Dictionary<string, object>;
            Assert.That(mergedNested["SubA"], Is.EqualTo(1));   // Kept
            Assert.That(mergedNested["SubB"], Is.EqualTo(2));   // Added
        }

        [Test]
        public void Merge_Shallow_WithNested_ShouldReplaceWholeDictionary()
        {
            var nestedTarget = new Dictionary<string, object> { { "SubA", 1 } };
            var target = new Dictionary<string, object> { { "Nested", nestedTarget } };

            var nestedSource = new Dictionary<string, object> { { "SubB", 2 } };
            var source = new Dictionary<string, object> { { "Nested", nestedSource } };

            DictionaryOperations.Merge(target, source, DictionaryMergeMode.Shallow, DictionaryConflictStrategy.Overwrite);

            var mergedNested = target["Nested"] as Dictionary<string, object>;
            Assert.That(mergedNested, Is.SameAs(nestedSource));
            Assert.That(mergedNested.ContainsKey("SubA"), Is.False);
            Assert.That(mergedNested["SubB"], Is.EqualTo(2));
        }

        [Test]
        public void Replace_ShouldOnlyUpdateExistingKeys()
        {
            var target = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
            var source = new Dictionary<string, int> { { "B", 20 }, { "C", 30 } };

            DictionaryOperations.Replace(target, source);

            Assert.That(target["A"], Is.EqualTo(1));  // Unchanged
            Assert.That(target["B"], Is.EqualTo(20)); // Replaced
            Assert.That(target.ContainsKey("C"), Is.False); // Ignored
        }

        [Test]
        public void Replace_NullArgs_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => DictionaryOperations.Replace<string, int>(null, new Dictionary<string, int>()));
            Assert.Throws<ArgumentNullException>(() => DictionaryOperations.Replace<string, int>(new Dictionary<string, int>(), null));
        }

        [Test]
        public void Merge_NullArgs_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => DictionaryOperations.Merge<string, int>(null, new Dictionary<string, int>()));
            Assert.Throws<ArgumentNullException>(() => DictionaryOperations.Merge<string, int>(new Dictionary<string, int>(), null));
        }
    }
}
