#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1402

namespace LbxyCommonLib.StringProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Numerics;
    using System.Text.RegularExpressions;

    public enum NumberOperation
    {
        Increment,
        Decrement,
    }

    public enum OverflowStrategy
    {
        Wrap,
        Clamp,
        Error,
    }

    public enum NumberSelectionMode
    {
        First,
        Last,
        Range,
        Regex,
    }

    public sealed class NumberSelectionOptions
    {
        public NumberSelectionMode Mode { get; set; }

        public bool ApplyToAllMatches { get; set; }

        public int? RangeStartIndex { get; set; }

        public int? RangeLength { get; set; }

        public string ExcludedPrefix { get; set; } = string.Empty;

        public string ExcludedSuffix { get; set; } = string.Empty;

        public string IncludedPrefix { get; set; } = string.Empty;

        public string IncludedSuffix { get; set; } = string.Empty;

        public string RegexPattern { get; set; } = string.Empty;

        public RegexOptions RegexOptions { get; set; }

        public Func<string, bool> CustomFilter { get; set; } = _ => true;
    }

    public sealed class NumberOperationLog
    {
        public NumberOperationLog(
            int startIndex,
            int length,
            string originalText,
            string newText,
            BigInteger originalValue,
            BigInteger newValue)
        {
            StartIndex = startIndex;
            Length = length;
            OriginalText = originalText;
            NewText = newText;
            OriginalValue = originalValue;
            NewValue = newValue;
        }

        public int StartIndex { get; }

        public int Length { get; }

        public string OriginalText { get; }

        public string NewText { get; }

        public BigInteger OriginalValue { get; }

        public BigInteger NewValue { get; }
    }

    public sealed class StringNumberOperationResult
    {
        public StringNumberOperationResult(string original, string transformed, IReadOnlyList<NumberOperationLog> logs)
        {
            Original = original;
            Transformed = transformed;
            Logs = logs;
        }

        public string Original { get; }

        public string Transformed { get; }

        public IReadOnlyList<NumberOperationLog> Logs { get; }
    }

    public static class StringNumberProcessor
    {
        public static StringNumberOperationResult Process(
            string input,
            NumberOperation operation,
            int step,
            NumberSelectionOptions options,
            OverflowStrategy overflowStrategy)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (step < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(step));
            }

            var spans = options.Mode == NumberSelectionMode.Regex
                ? FindNumberSpansByRegex(input, options)
                : FindNumberSpansByScan(input, options);

            if (spans.Count == 0)
            {
                throw new InvalidOperationException("No numeric segments match the selection options.");
            }

            var selectedSpans = SelectSpans(spans, options);
            if (selectedSpans.Count == 0)
            {
                throw new InvalidOperationException("No numeric segments selected after applying selection mode.");
            }

            var chars = input.ToCharArray();
            var logs = new List<NumberOperationLog>(selectedSpans.Count);

            foreach (var span in selectedSpans)
            {
                var originalText = new string(chars, span.Start, span.Length);
                var result = TransformDigits(originalText, operation, step, overflowStrategy);

                for (var i = 0; i < span.Length; i++)
                {
                    chars[span.Start + i] = result.NewText[i];
                }

                logs.Add(
                    new NumberOperationLog(
                        span.Start,
                        span.Length,
                        originalText,
                        result.NewText,
                        result.OriginalValue,
                        result.NewValue));
            }

            return new StringNumberOperationResult(input, new string(chars), logs);
        }

        public static IReadOnlyList<StringNumberOperationResult> ProcessMany(
            IEnumerable<string> inputs,
            NumberOperation operation,
            int step,
            NumberSelectionOptions options,
            OverflowStrategy overflowStrategy)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }

            var results = new List<StringNumberOperationResult>();
            foreach (var input in inputs)
            {
                results.Add(Process(input, operation, step, options, overflowStrategy));
            }

            return results;
        }

        private static List<NumberSpan> FindNumberSpansByScan(string input, NumberSelectionOptions options)
        {
            var spans = new List<NumberSpan>();
            var start = -1;

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c >= '0' && c <= '9')
                {
                    if (start < 0)
                    {
                        start = i;
                    }
                }
                else if (start >= 0)
                {
                    spans.Add(new NumberSpan(start, i - start));
                    start = -1;
                }
            }

            if (start >= 0)
            {
                spans.Add(new NumberSpan(start, input.Length - start));
            }

            spans = ApplyRangeFilter(spans, options, input.Length);
            spans = ApplyPrefixSuffixFilters(input, spans, options);

            return spans;
        }

        private static List<NumberSpan> FindNumberSpansByRegex(string input, NumberSelectionOptions options)
        {
            if (string.IsNullOrEmpty(options.RegexPattern))
            {
                throw new ArgumentException("RegexPattern must be provided when using Regex mode.", nameof(options));
            }

            var spans = new List<NumberSpan>();
            var regex = new Regex(options.RegexPattern, options.RegexOptions);
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (!match.Success)
                {
                    continue;
                }

                var value = match.Value;
                var isAllDigits = true;

                for (var i = 0; i < value.Length; i++)
                {
                    var c = value[i];
                    if (c < '0' || c > '9')
                    {
                        isAllDigits = false;
                        break;
                    }
                }

                if (!isAllDigits)
                {
                    continue;
                }

                spans.Add(new NumberSpan(match.Index, match.Length));
            }

            spans = ApplyRangeFilter(spans, options, input.Length);
            spans = ApplyPrefixSuffixFilters(input, spans, options);

            return spans;
        }

        private static List<NumberSpan> ApplyRangeFilter(
            List<NumberSpan> spans,
            NumberSelectionOptions options,
            int inputLength)
        {
            if (!options.RangeStartIndex.HasValue || !options.RangeLength.HasValue)
            {
                return spans;
            }

            var start = options.RangeStartIndex.Value;
            var length = options.RangeLength.Value;

            if (start < 0 || length <= 0 || start >= inputLength)
            {
                return new List<NumberSpan>();
            }

            var endExclusive = start + length;
            var result = new List<NumberSpan>();

            foreach (var span in spans)
            {
                var spanEnd = span.Start + span.Length;
                if (span.Start >= start && spanEnd <= endExclusive)
                {
                    result.Add(span);
                }
            }

            return result;
        }

        private static List<NumberSpan> ApplyPrefixSuffixFilters(
            string input,
            List<NumberSpan> spans,
            NumberSelectionOptions options)
        {
            var result = new List<NumberSpan>();

            foreach (var span in spans)
            {
                var value = input.Substring(span.Start, span.Length);

                if (!string.IsNullOrEmpty(options.IncludedPrefix) &&
                    !value.StartsWith(options.IncludedPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(options.IncludedSuffix) &&
                    !value.EndsWith(options.IncludedSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(options.ExcludedPrefix) &&
                    value.StartsWith(options.ExcludedPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(options.ExcludedSuffix) &&
                    value.EndsWith(options.ExcludedSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!options.CustomFilter(value))
                {
                    continue;
                }

                result.Add(span);
            }

            return result;
        }

        private static List<NumberSpan> SelectSpans(
            List<NumberSpan> spans,
            NumberSelectionOptions options)
        {
            if (options.ApplyToAllMatches)
            {
                return spans;
            }

            if (spans.Count == 0)
            {
                return spans;
            }

            NumberSpan selected;

            switch (options.Mode)
            {
                case NumberSelectionMode.Last:
                    selected = spans[spans.Count - 1];
                    break;
                default:
                    selected = spans[0];
                    break;
            }

            return new List<NumberSpan> { selected };
        }

        private static DigitTransformResult TransformDigits(
            string digits,
            NumberOperation operation,
            int step,
            OverflowStrategy overflowStrategy)
        {
            if (digits.Length == 0)
            {
                throw new InvalidOperationException("Cannot transform an empty digit segment.");
            }

            var width = digits.Length;
            var originalValue = BigInteger.Parse(digits, CultureInfo.InvariantCulture);
            var delta = new BigInteger(step);

            BigInteger newValue;
            if (operation == NumberOperation.Increment)
            {
                newValue = originalValue + delta;
            }
            else
            {
                newValue = originalValue - delta;
            }

            var min = BigInteger.Zero;
            var max = BigInteger.Pow(new BigInteger(10), width) - BigInteger.One;

            if (newValue < min || newValue > max)
            {
                if (overflowStrategy == OverflowStrategy.Error)
                {
                    throw new OverflowException("Numeric value overflowed the allowed range for the segment width.");
                }

                if (overflowStrategy == OverflowStrategy.Clamp)
                {
                    if (newValue < min)
                    {
                        newValue = min;
                    }
                    else if (newValue > max)
                    {
                        newValue = max;
                    }
                }
                else if (overflowStrategy == OverflowStrategy.Wrap)
                {
                    var range = max - min + BigInteger.One;
                    var normalized = (newValue - min) % range;
                    if (normalized < BigInteger.Zero)
                    {
                        normalized += range;
                    }

                    newValue = normalized + min;
                }
            }

            var format = "D" + width.ToString(CultureInfo.InvariantCulture);
            var newText = newValue.ToString(format, CultureInfo.InvariantCulture);

            return new DigitTransformResult(originalValue, newValue, newText);
        }

        private readonly struct NumberSpan
        {
            public NumberSpan(int start, int length)
            {
                Start = start;
                Length = length;
            }

            public int Start { get; }

            public int Length { get; }
        }

        private readonly struct DigitTransformResult
        {
            public DigitTransformResult(BigInteger originalValue, BigInteger newValue, string newText)
            {
                OriginalValue = originalValue;
                NewValue = newValue;
                NewText = newText;
            }

            public BigInteger OriginalValue { get; }

            public BigInteger NewValue { get; }

            public string NewText { get; }
        }
    }
}

#pragma warning restore SA1402
#pragma warning restore SA1649
#pragma warning restore SA1633
#pragma warning restore SA1602
#pragma warning restore SA1600
#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore CS1591

