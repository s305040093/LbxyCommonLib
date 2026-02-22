#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1202
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1503
#pragma warning disable SA1513
#pragma warning disable SA1629
#pragma warning disable SA1402

namespace Ext.StringExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExt
    {
        /// <summary>
        /// Performs ordinal comparison with optional case-insensitivity.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="other">The other string to compare.</param>
        /// <param name="ignoreCase">If true, ignores case differences.</param>
        /// <returns>True if equal under specified rule; otherwise false. If source is null, returns false.</returns>
        /// <code>
        /// var ok = "Hello".EqualsOrdinal("hello", ignoreCase: true);
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsOrdinal(this string source, string other, bool ignoreCase)
        {
            if (source == null)
            {
                return false;
            }

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return string.Equals(source, other, comparison);
        }

        /// <summary>
        /// Performs culture-sensitive comparison using given comparison type.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="other">The other string to compare.</param>
        /// <param name="comparisonType">The comparison type indicating culture and case sensitivity.</param>
        /// <returns>True if equal under specified rule; otherwise false. If source is null, returns false.</returns>
        /// <code>
        /// var ok = "straße".EqualsCulture("strasse", StringComparison.CurrentCulture);
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsCulture(this string source, string other, StringComparison comparisonType)
        {
            if (source == null)
            {
                return false;
            }

            return string.Equals(source, other, comparisonType);
        }

        /// <summary>
        /// Compares two strings in a natural order (numeric segments are compared by numeric value).
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="other">The other string.</param>
        /// <returns>
        /// Negative if source &lt; other, zero if equal, positive if source &gt; other.
        /// If source is null, returns -1 by convention.
        /// </returns>
        /// <code>
        /// var r = "file10".CompareNatural("file2"); // r &gt; 0
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareNatural(this string source, string other)
        {
            if (source == null)
            {
                return -1;
            }

            if (other == null)
            {
                return 1;
            }

            var i = 0;
            var j = 0;
            while (i < source.Length && j < other.Length)
            {
                var c1 = source[i];
                var c2 = other[j];

                var d1 = char.IsDigit(c1);
                var d2 = char.IsDigit(c2);

                if (d1 && d2)
                {
                    var startI = i;
                    var startJ = j;
                    while (i < source.Length && char.IsDigit(source[i])) i++;
                    while (j < other.Length && char.IsDigit(other[j])) j++;

                    var num1Span = source.Substring(startI, i - startI);
                    var num2Span = other.Substring(startJ, j - startJ);

                    var trimmed1 = TrimLeadingZeros(num1Span);
                    var trimmed2 = TrimLeadingZeros(num2Span);

                    if (trimmed1.Length != trimmed2.Length)
                    {
                        return trimmed1.Length.CompareTo(trimmed2.Length);
                    }

                    var cmp = string.CompareOrdinal(trimmed1, trimmed2);
                    if (cmp != 0)
                    {
                        return cmp;
                    }

                    continue;
                }

                var ocmp = char.ToString(c1).EqualsOrdinal(char.ToString(c2), ignoreCase: true)
                    ? 0
                    : char.ToUpperInvariant(c1).CompareTo(char.ToUpperInvariant(c2));
                if (ocmp != 0)
                {
                    return ocmp;
                }

                i++;
                j++;
            }

            return (source.Length - i).CompareTo(other.Length - j);
        }

        private static string TrimLeadingZeros(string s)
        {
            var k = 0;
            while (k < s.Length - 1 && s[k] == '0') k++;
            return k == 0 ? s : s.Substring(k);
        }

        /// <summary>
        /// Determines whether the string is null or empty.
        /// </summary>
        /// <param name="source">The input string to check.</param>
        /// <returns>True if the string is null or empty; otherwise, false.</returns>
        /// <code>
        /// var ok = ((string)null).IsNullOrEmpty(); // true
        /// var no = "text".IsNullOrEmpty(); // false
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Determines whether the string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="source">The input string to check.</param>
        /// <returns>True if the string is null, empty, or white-space; otherwise, false.</returns>
        /// <code>
        /// var ok = "   ".IsNullOrWhiteSpace(); // true
        /// var no = "a b".IsNullOrWhiteSpace(); // false
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        /// Determines whether the string is not null and not empty.
        /// </summary>
        /// <param name="source">The input string to check.</param>
        /// <returns>True if the string is not null and not empty; otherwise, false.</returns>
        /// <code>
        /// var ok = "text".IsNotNullOrEmpty(); // true
        /// var no = "".IsNotNullOrEmpty(); // false
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Determines whether the string is not null, not empty, and contains non-white-space characters.
        /// </summary>
        /// <param name="source">The input string to check.</param>
        /// <returns>True if the string contains non-white-space characters; otherwise, false.</returns>
        /// <code>
        /// var ok = "a".IsNotNullOrWhiteSpace(); // true
        /// var no = "   ".IsNotNullOrWhiteSpace(); // false
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrWhiteSpace(this string source)
        {
            return !string.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        /// Splits a string by given separator string with options.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="separator">Separator string.</param>
        /// <param name="options">Split options.</param>
        /// <returns>Array of parts. If source is null, returns empty array.</returns>
        /// <code>
        /// var parts = "a--b--c".SplitBy("--", StringSplitOptions.RemoveEmptyEntries);
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] SplitBy(this string source, string separator, StringSplitOptions options = StringSplitOptions.None)
        {
            if (source == null)
            {
                return new string[0];
            }

            if (separator == null)
            {
                separator = string.Empty;
            }

            if (separator.Length == 0)
            {
                return source.Split(new[] { separator }, options);
            }

            return source.Split(new[] { separator }, options);
        }

        /// <summary>
        /// Splits text into lines handling \r, \n, and \r\n.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="removeEmpty">If true, removes empty lines.</param>
        /// <returns>Array of lines. If source is null, returns empty array.</returns>
        /// <code>
        /// var lines = "a\r\nb\nc\rd".SplitLines();
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] SplitLines(this string source, bool removeEmpty = true)
        {
            if (source == null)
            {
                return new string[0];
            }

            var list = new List<string>();
            var sb = new StringBuilder();
            for (var idx = 0; idx < source.Length; idx++)
            {
                var ch = source[idx];
                if (ch == '\r')
                {
                    if (idx + 1 < source.Length && source[idx + 1] == '\n')
                    {
                        idx++;
                    }
                    FlushLine(sb, list, removeEmpty);
                }
                else if (ch == '\n')
                {
                    FlushLine(sb, list, removeEmpty);
                }
                else
                {
                    sb.Append(ch);
                }
            }

            if (sb.Length > 0 || !removeEmpty)
            {
                var line = sb.ToString();
                if (!removeEmpty || line.Length > 0)
                {
                    list.Add(line);
                }
            }

            return list.ToArray();
        }

        private static void FlushLine(StringBuilder sb, List<string> list, bool removeEmpty)
        {
            var line = sb.ToString();
            if (!removeEmpty || line.Length > 0)
            {
                list.Add(line);
            }

            sb.Clear();
        }

        /// <summary>
        /// Splits a CSV line supporting quote escaping by doubling quotes.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="delimiter">Field delimiter (default ',').</param>
        /// <param name="quote">Quote character (default '\"').</param>
        /// <returns>Array of parsed fields. If source is null, returns empty array.</returns>
        /// <code>
        /// var fields = "a,\"b,c\",d".SplitCsv();
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] SplitCsv(this string source, char delimiter = ',', char quote = '"')
        {
            if (source == null)
            {
                return new string[0];
            }

            var list = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;
            for (var i = 0; i < source.Length; i++)
            {
                var ch = source[i];
                if (inQuotes)
                {
                    if (ch == quote)
                    {
                        if (i + 1 < source.Length && source[i + 1] == quote)
                        {
                            sb.Append(quote);
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    if (ch == quote)
                    {
                        inQuotes = true;
                    }
                    else if (ch == delimiter)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
            }

            list.Add(sb.ToString());
            return list.ToArray();
        }

        /// <summary>
        /// Joins strings with separator. Returns empty when input is null.
        /// </summary>
        /// <param name="items">Items to join.</param>
        /// <param name="separator">Separator string.</param>
        /// <returns>Joined string.</returns>
        /// <code>
        /// var s = new[]{ "a", "b" }.JoinWith("-");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string JoinWith(this IEnumerable<string> items, string separator)
        {
            if (items == null)
            {
                return string.Empty;
            }

            return string.Join(separator ?? string.Empty, items);
        }

        /// <summary>
        /// Concatenates strings in order.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="others">Other strings.</param>
        /// <returns>Concatenated string. If source is null, returns string.Empty.</returns>
        /// <code>
        /// var s = "a".ConcatWith("b", "c");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ConcatWith(this string source, params string[] others)
        {
            if (source == null)
            {
                source = string.Empty;
            }

            if (others == null || others.Length == 0)
            {
                return source;
            }

            var sb = new StringBuilder(source);
            for (var i = 0; i < others.Length; i++)
            {
                sb.Append(others[i] ?? string.Empty);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Merges lines by removing empty lines and joining with separator.
        /// </summary>
        /// <param name="lines">Source lines.</param>
        /// <param name="lineSeparator">Line separator (default CRLF).</param>
        /// <returns>Merged text.</returns>
        /// <code>
        /// var s = new[]{ "a", "", "b" }.MergeLines();
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MergeLines(this IEnumerable<string> lines, string lineSeparator = "\r\n")
        {
            if (lines == null)
            {
                return string.Empty;
            }

            var list = new List<string>();
            foreach (var l in lines)
            {
                if (!string.IsNullOrEmpty(l))
                {
                    list.Add(l);
                }
            }

            return string.Join(lineSeparator ?? string.Empty, list.ToArray());
        }

        /// <summary>
        /// Ensures the string has the given prefix; adds it if missing.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="prefix">Prefix to ensure.</param>
        /// <returns>String with prefix. If source is null, returns prefix or string.Empty.</returns>
        /// <code>
        /// var s = "world".EnsurePrefix("hello ");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsurePrefix(this string source, string prefix)
        {
            if (source == null)
            {
                return prefix ?? string.Empty;
            }

            if (string.IsNullOrEmpty(prefix))
            {
                return source;
            }

            return source.StartsWith(prefix, StringComparison.Ordinal) ? source : prefix + source;
        }

        /// <summary>
        /// Ensures the string has the given suffix; adds it if missing.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="suffix">Suffix to ensure.</param>
        /// <returns>String with suffix. If source is null, returns suffix or string.Empty.</returns>
        /// <code>
        /// var s = "hello".EnsureSuffix("!");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureSuffix(this string source, string suffix)
        {
            if (source == null)
            {
                return suffix ?? string.Empty;
            }

            if (string.IsNullOrEmpty(suffix))
            {
                return source;
            }

            return source.EndsWith(suffix, StringComparison.Ordinal) ? source : source + suffix;
        }

        /// <summary>
        /// Removes the specified prefix if present (comparison configurable).
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="prefix">Prefix to remove.</param>
        /// <param name="comparison">Comparison mode (default Ordinal).</param>
        /// <returns>String without prefix when matched, otherwise original. If source is null, returns string.Empty.</returns>
        /// <code>
        /// var s = "prefixValue".RemovePrefix("prefix");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemovePrefix(this string source, string prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (source == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(prefix))
            {
                return source;
            }

            return source.StartsWith(prefix, comparison) ? source.Substring(prefix.Length) : source;
        }

        /// <summary>
        /// Removes the specified suffix if present (comparison configurable).
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="suffix">Suffix to remove.</param>
        /// <param name="comparison">Comparison mode (default Ordinal).</param>
        /// <returns>String without suffix when matched, otherwise original. If source is null, returns string.Empty.</returns>
        /// <code>
        /// var s = "valueSuffix".RemoveSuffix("Suffix");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveSuffix(this string source, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (source == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(suffix))
            {
                return source;
            }

            return source.EndsWith(suffix, comparison) ? source.Substring(0, source.Length - suffix.Length) : source;
        }

        /// <summary>
        /// Case-insensitive prefix check.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="prefix">Prefix to check.</param>
        /// <returns>True when has prefix ignoring case; false if not or source is null.</returns>
        /// <code>
        /// var ok = "Hello".HasPrefixIgnoreCase("he");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasPrefixIgnoreCase(this string source, string prefix)
        {
            if (source == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(prefix) && source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Case-insensitive suffix check.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="suffix">Suffix to check.</param>
        /// <returns>True when has suffix ignoring case; false if not or source is null.</returns>
        /// <code>
        /// var ok = "Hello".HasSuffixIgnoreCase("LO");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSuffixIgnoreCase(this string source, string suffix)
        {
            if (source == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(suffix) && source.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Case-insensitive replace for all occurrences.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="oldValue">Old value to replace.</param>
        /// <param name="newValue">New value to insert.</param>
        /// <returns>Replaced string. If source is null, returns string.Empty.</returns>
        /// <code>
        /// var s = "Hello HELLO".ReplaceIgnoreCase("hello", "hi");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceIgnoreCase(this string source, string oldValue, string newValue)
        {
            if (source == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(oldValue))
            {
                return source;
            }

            var pattern = Regex.Escape(oldValue);
            return Regex.Replace(source, pattern, newValue ?? string.Empty, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Replaces the first occurrence of a substring.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="oldValue">Substring to replace.</param>
        /// <param name="newValue">Replacement text.</param>
        /// <param name="comparison">Comparison mode.</param>
        /// <returns>Updated string. If source is null, returns string.Empty.</returns>
        /// <code>
        /// var s = "a-b-b".ReplaceFirst("b", "X");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceFirst(this string source, string oldValue, string newValue, StringComparison comparison = StringComparison.Ordinal)
        {
            if (source == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(oldValue))
            {
                return source;
            }

            var idx = source.IndexOf(oldValue, comparison);
            if (idx < 0)
            {
                return source;
            }

            return source.Substring(0, idx) + (newValue ?? string.Empty) + source.Substring(idx + oldValue.Length);
        }

        /// <summary>
        /// Replaces the last occurrence of a substring.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="oldValue">Substring to replace.</param>
        /// <param name="newValue">Replacement text.</param>
        /// <param name="comparison">Comparison mode.</param>
        /// <returns>Updated string. If source is null, returns string.Empty.</returns>
        /// <code>
        /// var s = "b-a-b".ReplaceLast("b", "X");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceLast(this string source, string oldValue, string newValue, StringComparison comparison = StringComparison.Ordinal)
        {
            if (source == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(oldValue))
            {
                return source;
            }

            var idx = source.LastIndexOf(oldValue, comparison);
            if (idx < 0)
            {
                return source;
            }

            return source.Substring(0, idx) + (newValue ?? string.Empty) + source.Substring(idx + oldValue.Length);
        }

        /// <summary>
        /// Regex-based replace supporting options.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="pattern">Regex pattern.</param>
        /// <param name="replacement">Replacement text.</param>
        /// <param name="options">Regex options.</param>
        /// <returns>Updated string. If source is null, returns string.Empty.</returns>
        /// <code>
        /// var s = "abc123".ReplaceRegex(@"\d+", "#");
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceRegex(this string source, string pattern, string replacement, RegexOptions options = RegexOptions.None)
        {
            if (source == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(pattern))
            {
                return source;
            }

            return Regex.Replace(source, pattern, replacement ?? string.Empty, options);
        }
    }
}

#pragma warning restore SA1402
#pragma warning restore SA1649
#pragma warning restore SA1633
#pragma warning restore SA1602
#pragma warning restore SA1600
#pragma warning restore SA1202
#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore SA1629
#pragma warning restore SA1503
#pragma warning restore SA1513
#pragma warning restore CS1591
