namespace Ext.StringExtensions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Ext.StringExtensions;
    using NUnit.Framework;

    [TestFixture]
    public sealed class StringExtensionsTests
    {
        [Test]
        public void EqualsOrdinal_Basic()
        {
            Assert.That("Hello".EqualsOrdinal("hello", true), Is.True);
            Assert.That("Hello".EqualsOrdinal("hello", false), Is.False);
            Assert.That(((string)null).EqualsOrdinal("x", true), Is.False);
        }

        [Test]
        public void EqualsCulture_Basic()
        {
            Assert.That("Hello".EqualsCulture("HELLO", StringComparison.CurrentCultureIgnoreCase), Is.True);
            Assert.That(((string)null).EqualsCulture("x", StringComparison.CurrentCulture), Is.False);
        }

        [Test]
        public void CompareNatural_Digits()
        {
            Assert.That("file2".CompareNatural("file10"), Is.LessThan(0));
            Assert.That("file10".CompareNatural("file2"), Is.GreaterThan(0));
            Assert.That(((string)null).CompareNatural("x"), Is.LessThan(0));
            Assert.That("x".CompareNatural(null), Is.GreaterThan(0));
            Assert.That("a001".CompareNatural("a1"), Is.EqualTo(0)); // numeric equal
        }

        [Test]
        public void SplitBy_Basic()
        {
            var parts = "a--b--c".SplitBy("--", StringSplitOptions.RemoveEmptyEntries);
            Assert.That(parts, Is.EqualTo(new[] { "a", "b", "c" }));
            var empty = ((string)null).SplitBy("--");
            Assert.That(empty.Length, Is.EqualTo(0));
        }

        [Test]
        public void SplitLines_AllNewlines()
        {
            var s = "a\r\nb\nc\rd";
            var lines = s.SplitLines();
            Assert.That(lines, Is.EqualTo(new[] { "a", "b", "c", "d" }));
        }

        [Test]
        public void SplitCsv_Quotes()
        {
            var s = "a,\"b,c\",\"d\"\"e\"";
            var fields = s.SplitCsv();
            Assert.That(fields, Is.EqualTo(new[] { "a", "b,c", "d\"e" }));
        }

        [Test]
        public void JoinWith_Basic()
        {
            var s = new[] { "x", "y" }.JoinWith("-");
            Assert.That(s, Is.EqualTo("x-y"));
            Assert.That(((IEnumerable<string>)null).JoinWith("-"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void NullEmptyWhitespace_Checks()
        {
            string n = null;
            Assert.That(n.IsNullOrEmpty(), Is.True);
            Assert.That(n.IsNullOrWhiteSpace(), Is.True);
            Assert.That(n.IsNotNullOrEmpty(), Is.False);
            Assert.That(n.IsNotNullOrWhiteSpace(), Is.False);

            Assert.That(string.Empty.IsNullOrEmpty(), Is.True);
            Assert.That(string.Empty.IsNullOrWhiteSpace(), Is.True);
            Assert.That(string.Empty.IsNotNullOrEmpty(), Is.False);
            Assert.That(string.Empty.IsNotNullOrWhiteSpace(), Is.False);

            Assert.That("   ".IsNullOrWhiteSpace(), Is.True);
            Assert.That("   ".IsNotNullOrWhiteSpace(), Is.False);
            Assert.That("a".IsNotNullOrWhiteSpace(), Is.True);
            Assert.That("a".IsNotNullOrEmpty(), Is.True);
        }

        [Test]
        public void ConcatWith_Basic()
        {
            Assert.That("a".ConcatWith("b", "c"), Is.EqualTo("abc"));
            Assert.That(((string)null).ConcatWith("x"), Is.EqualTo("x"));
        }

        [Test]
        public void MergeLines_RemovesEmpty()
        {
            var s = new[] { "a", "", "b" }.MergeLines();
            Assert.That(s, Is.EqualTo("a\r\nb"));
        }

        [Test]
        public void PrefixSuffix_EnsureAndRemove()
        {
            Assert.That("world".EnsurePrefix("hello "), Is.EqualTo("hello world"));
            Assert.That("hello".EnsureSuffix("!"), Is.EqualTo("hello!"));
            Assert.That("prefixValue".RemovePrefix("prefix"), Is.EqualTo("Value"));
            Assert.That("valueSuffix".RemoveSuffix("Suffix", StringComparison.OrdinalIgnoreCase), Is.EqualTo("value"));
            Assert.That("Hello".HasPrefixIgnoreCase("he"), Is.True);
            Assert.That("Hello".HasSuffixIgnoreCase("LO"), Is.True);
        }

        [Test]
        public void Replace_Variants()
        {
            Assert.That("Hello HELLO".ReplaceIgnoreCase("hello", "hi"), Is.EqualTo("hi hi"));
            Assert.That("a-b-b".ReplaceFirst("b", "X"), Is.EqualTo("a-X-b"));
            Assert.That("b-a-b".ReplaceLast("b", "X"), Is.EqualTo("b-a-X"));
            Assert.That("abc123".ReplaceRegex(@"\d+", "#", RegexOptions.None), Is.EqualTo("abc#"));
        }

        [Test]
        public void Performance_LargeString_10MB()
        {
            var chunk = new string('a', 1024);
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < 10240; i++)
            {
                sb.Append(chunk);
                sb.Append("\r\n");
            }

            var big = sb.ToString();
            var lines = big.SplitLines();
            Assert.That(lines.Length, Is.GreaterThan(9000));
        }
    }
}
