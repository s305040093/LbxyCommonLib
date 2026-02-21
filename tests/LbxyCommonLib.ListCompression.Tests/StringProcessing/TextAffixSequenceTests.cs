namespace LbxyCommonLib.StringProcessing.Tests
{
    using System.Collections.Generic;
    using LbxyCommonLib.StringProcessing;
    using NUnit.Framework;

    [TestFixture]
    public sealed class TextAffixSequenceTests
    {
        [Test]
        public void FixedPrefixAndSuffix_ArePreserved()
        {
            var options = new TextAffixSequenceOptions
            {
                UsePrefixNumber = false,
                UseSuffixNumber = false,
            };

            options.Affix.FixedPrefix = "[前缀] ";
            options.Affix.FixedSuffix = " [后缀]";

            var sequence = new TextAffixSequence(options);

            var result = sequence.Next("内容");

            Assert.That(result, Is.EqualTo("[前缀] 内容 [后缀]"));
        }

        [Test]
        public void PrefixNumber_IncrementsWithPadding()
        {
            var options = new TextAffixSequenceOptions
            {
                UsePrefixNumber = true,
                UseSuffixNumber = false,
            };

            options.Affix.FixedPrefix = "#";
            options.PrefixNumber.Start = 1;
            options.PrefixNumber.Step = 1;
            options.PrefixNumber.Width = 3;
            options.PrefixNumber.NumberBase = 10;

            var sequence = new TextAffixSequence(options);

            var first = sequence.Next("A");
            var second = sequence.Next("B");

            Assert.That(first, Is.EqualTo("#001A"));
            Assert.That(second, Is.EqualTo("#002B"));
        }

        [Test]
        public void SuffixNumber_DecrementsInHex()
        {
            var options = new TextAffixSequenceOptions
            {
                UsePrefixNumber = false,
                UseSuffixNumber = true,
            };

            options.Affix.FixedSuffix = ";";
            options.SuffixNumber.Start = 255;
            options.SuffixNumber.Step = -1;
            options.SuffixNumber.Width = 2;
            options.SuffixNumber.NumberBase = 16;
            options.SuffixNumber.Uppercase = true;

            var sequence = new TextAffixSequence(options);

            var first = sequence.Next("L1");
            var second = sequence.Next("L2");

            Assert.That(first, Is.EqualTo("L1FF;"));
            Assert.That(second, Is.EqualTo("L2FE;"));
        }

        [Test]
        public void NextMany_ProcessesAllItems()
        {
            var options = new TextAffixSequenceOptions
            {
                UsePrefixNumber = true,
                UseSuffixNumber = false,
            };

            options.PrefixNumber.Start = 10;
            options.PrefixNumber.Step = 5;
            options.PrefixNumber.Width = 0;
            options.PrefixNumber.NumberBase = 10;

            var sequence = new TextAffixSequence(options);

            var cores = new List<string> { "X", "Y", "Z" };
            var results = sequence.NextMany(cores);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results[0], Is.EqualTo("10X"));
            Assert.That(results[1], Is.EqualTo("15Y"));
            Assert.That(results[2], Is.EqualTo("20Z"));
        }
    }
}

