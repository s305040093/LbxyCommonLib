namespace LbxyCommonLib.StringProcessing.Tests
{
    using System;
    using System.Collections.Generic;
    using LbxyCommonLib.StringProcessing;
    using NUnit.Framework;

    [TestFixture]
    public sealed class StringNumberProcessorTests
    {
        [Test]
        public void Increment_LastNumber_KeepsLeadingZeros()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.Last,
            };

            var result = StringNumberProcessor.Process(
                "item-00123",
                NumberOperation.Increment,
                1,
                options,
                OverflowStrategy.Clamp);

            Assert.That(result.Transformed, Is.EqualTo("item-00124"));
            Assert.That(result.Logs.Count, Is.EqualTo(1));
            Assert.That(result.Logs[0].OriginalText, Is.EqualTo("00123"));
            Assert.That(result.Logs[0].NewText, Is.EqualTo("00124"));
        }

        [Test]
        public void Decrement_FirstNumber_InRange()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.Range,
                RangeStartIndex = 0,
                RangeLength = 5,
            };

            var result = StringNumberProcessor.Process(
                "123-456-789",
                NumberOperation.Decrement,
                1,
                options,
                OverflowStrategy.Clamp);

            Assert.That(result.Transformed, Is.EqualTo("122-456-789"));
        }

        [Test]
        public void ExcludedPrefix_IsRespected()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.First,
                ExcludedPrefix = "00",
            };

            var result = StringNumberProcessor.Process(
                "id-001 id-123",
                NumberOperation.Increment,
                1,
                options,
                OverflowStrategy.Clamp);

            Assert.That(result.Transformed, Is.EqualTo("id-001 id-124"));
        }

        [Test]
        public void IncludedSuffix_IsRespected()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.First,
                IncludedSuffix = "99",
            };

            var result = StringNumberProcessor.Process(
                "code100 code099",
                NumberOperation.Increment,
                1,
                options,
                OverflowStrategy.Clamp);

            Assert.That(result.Transformed, Is.EqualTo("code100 code100"));
        }

        [Test]
        public void RegexSelection_Works()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.Regex,
                RegexPattern = @"(?<=ORD-)\d+",
            };

            var result = StringNumberProcessor.Process(
                "ORD-00042-DESC",
                NumberOperation.Increment,
                5,
                options,
                OverflowStrategy.Clamp);

            Assert.That(result.Transformed, Is.EqualTo("ORD-00047-DESC"));
        }

        [Test]
        public void WrapOverflow_WrapsWithinWidth()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.Last,
            };

            var result = StringNumberProcessor.Process(
                "value=999",
                NumberOperation.Increment,
                2,
                options,
                OverflowStrategy.Wrap);

            Assert.That(result.Transformed, Is.EqualTo("value=001"));
        }

        [Test]
        public void ClampOverflow_ClampsToBounds()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.Last,
            };

            var result = StringNumberProcessor.Process(
                "value=000",
                NumberOperation.Decrement,
                5,
                options,
                OverflowStrategy.Clamp);

            Assert.That(result.Transformed, Is.EqualTo("value=000"));
        }

        [Test]
        public void ErrorOverflow_Throws()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.Last,
            };

            Assert.Throws<OverflowException>(
                () => StringNumberProcessor.Process(
                    "value=999",
                    NumberOperation.Increment,
                    1,
                    options,
                    OverflowStrategy.Error));
        }

        [Test]
        public void ProcessMany_ProcessesAllInputs()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.Last,
                ApplyToAllMatches = false,
            };

            var inputs = new List<string> { "a1", "b2", "c3" };

            var results = StringNumberProcessor.ProcessMany(
                inputs,
                NumberOperation.Increment,
                1,
                options,
                OverflowStrategy.Clamp);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results[0].Transformed, Is.EqualTo("a2"));
            Assert.That(results[1].Transformed, Is.EqualTo("b3"));
            Assert.That(results[2].Transformed, Is.EqualTo("c4"));
        }

        [Test]
        public void ApplyToAllMatches_TransformsAllNumbers()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.First,
                ApplyToAllMatches = true,
            };

            var result = StringNumberProcessor.Process(
                "a1 b2 c3",
                NumberOperation.Increment,
                1,
                options,
                OverflowStrategy.Clamp);

            Assert.That(result.Transformed, Is.EqualTo("a2 b3 c4"));
            Assert.That(result.Logs.Count, Is.EqualTo(3));
        }

        [Test]
        public void CustomFilter_CanExcludeCombinedPrefixAndSuffix()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.First,
                ApplyToAllMatches = true,
                CustomFilter = digits =>
                {
                    if (digits.Length < 5)
                    {
                        return true;
                    }

                    var hasExcludedPrefix = digits.StartsWith("99", StringComparison.Ordinal);
                    var hasExcludedSuffix = digits.EndsWith("000", StringComparison.Ordinal);

                    return !(hasExcludedPrefix && hasExcludedSuffix);
                },
            };

            var result = StringNumberProcessor.Process(
                "99abc 99000 12345 99001",
                NumberOperation.Increment,
                1,
                options,
                OverflowStrategy.Clamp);

            Assert.That(result.Transformed, Is.EqualTo("99abc 99000 12346 99002"));
        }

        [Test]
        public void NoDigits_Throws()
        {
            var options = new NumberSelectionOptions
            {
                Mode = NumberSelectionMode.First,
            };

            Assert.Throws<InvalidOperationException>(
                () => StringNumberProcessor.Process(
                    "no-digits-here",
                    NumberOperation.Increment,
                    1,
                    options,
                    OverflowStrategy.Clamp));
        }
    }
}
