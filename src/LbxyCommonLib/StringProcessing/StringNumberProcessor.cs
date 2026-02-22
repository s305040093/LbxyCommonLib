#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1402
#pragma warning disable SA1629
#pragma warning disable SA1642

namespace LbxyCommonLib.StringProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Numerics;
    using System.Text.RegularExpressions;

    /// <summary>
    /// 数值操作类型，用于指定对选中数字执行递增或递减操作。
    /// </summary>
    public enum NumberOperation
    {
        /// <summary>
        /// 递增数值。
        /// </summary>
        Increment,

        /// <summary>
        /// 递减数值。
        /// </summary>
        Decrement,
    }

    /// <summary>
    /// 溢出处理策略，当数值超出当前位宽允许范围时控制处理方式。
    /// </summary>
    public enum OverflowStrategy
    {
        /// <summary>
        /// 按模回绕，例如 99 + 1 在两位宽下回绕为 00。
        /// </summary>
        Wrap,

        /// <summary>
        /// 截断到边界值，例如小于最小值则取最小值，大于最大值则取最大值。
        /// </summary>
        Clamp,

        /// <summary>
        /// 遇到溢出时抛出异常。
        /// </summary>
        Error,
    }

    /// <summary>
    /// 数字选择模式，用于从字符串中选取要进行运算的数字片段。
    /// </summary>
    public enum NumberSelectionMode
    {
        /// <summary>
        /// 选择第一个匹配的数字片段。
        /// </summary>
        First,

        /// <summary>
        /// 选择最后一个匹配的数字片段。
        /// </summary>
        Last,

        /// <summary>
        /// 仅选择在指定范围内的数字片段。
        /// </summary>
        Range,

        /// <summary>
        /// 使用正则表达式选择数字片段。
        /// </summary>
        Regex,
    }

    /// <summary>
    /// 控制从字符串中选择哪些数字片段参与运算的选项。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class NumberSelectionOptions
    {
        /// <summary>
        /// 获取或设置数字选择模式。
        /// </summary>
        public NumberSelectionMode Mode { get; set; }

        /// <summary>
        /// 指示是否对所有匹配的数字片段应用操作；为 false 时根据 <see cref="Mode"/> 选择单个片段。
        /// </summary>
        public bool ApplyToAllMatches { get; set; }

        /// <summary>
        /// 在 <see cref="NumberSelectionMode.Range"/> 模式下，指定起始索引（基于原始字符串，从 0 开始）。
        /// </summary>
        public int? RangeStartIndex { get; set; }

        /// <summary>
        /// 在 <see cref="NumberSelectionMode.Range"/> 模式下，指定影响范围的长度。
        /// </summary>
        public int? RangeLength { get; set; }

        /// <summary>
        /// 指定以特定前缀开头的数字片段将被排除在选择之外。
        /// </summary>
        public string ExcludedPrefix { get; set; } = string.Empty;

        /// <summary>
        /// 指定以特定后缀结尾的数字片段将被排除在选择之外。
        /// </summary>
        public string ExcludedSuffix { get; set; } = string.Empty;

        /// <summary>
        /// 指定仅包含特定前缀的数字片段才会被选择。
        /// </summary>
        public string IncludedPrefix { get; set; } = string.Empty;

        /// <summary>
        /// 指定仅包含特定后缀的数字片段才会被选择。
        /// </summary>
        public string IncludedSuffix { get; set; } = string.Empty;

        /// <summary>
        /// 当 <see cref="NumberSelectionMode.Regex"/> 模式启用时使用的正则表达式模式。
        /// </summary>
        public string RegexPattern { get; set; } = string.Empty;

        /// <summary>
        /// 正则表达式选项，用于控制匹配行为（如忽略大小写、多行模式等）。
        /// </summary>
        public RegexOptions RegexOptions { get; set; }

        /// <summary>
        /// 自定义过滤委托，用于对每个候选数字片段进行额外筛选；返回 true 表示保留，false 表示排除。
        /// </summary>
        public Func<string, bool> CustomFilter { get; set; } = _ => true;
    }

    /// <summary>
    /// 记录单个数字片段在运算前后的变更信息。
    /// </summary>
    public sealed class NumberOperationLog
    {
        /// <summary>
        /// 初始化 <see cref="NumberOperationLog"/> 类型的新实例。
        /// </summary>
        /// <param name="startIndex">数字片段在原始字符串中的起始索引。</param>
        /// <param name="length">数字片段的长度。</param>
        /// <param name="originalText">运算前的数字文本。</param>
        /// <param name="newText">运算后的数字文本。</param>
        /// <param name="originalValue">运算前的数值。</param>
        /// <param name="newValue">运算后的数值。</param>
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

        /// <summary>
        /// 获取数字片段在原始字符串中的起始索引。
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// 获取数字片段的长度。
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 获取运算前的数字文本。
        /// </summary>
        public string OriginalText { get; }

        /// <summary>
        /// 获取运算后的数字文本。
        /// </summary>
        public string NewText { get; }

        /// <summary>
        /// 获取运算前的数值。
        /// </summary>
        public BigInteger OriginalValue { get; }

        /// <summary>
        /// 获取运算后的数值。
        /// </summary>
        public BigInteger NewValue { get; }
    }

    /// <summary>
    /// 表示对单个字符串执行数字运算后的结果，包括原文、变换后文本及详细日志。
    /// </summary>
    public sealed class StringNumberOperationResult
    {
        /// <summary>
        /// 初始化 <see cref="StringNumberOperationResult"/> 类型的新实例。
        /// </summary>
        /// <param name="original">原始字符串。</param>
        /// <param name="transformed">应用数字运算后的字符串。</param>
        /// <param name="logs">每个数字片段变更的详细日志集合。</param>
        public StringNumberOperationResult(string original, string transformed, IReadOnlyList<NumberOperationLog> logs)
        {
            Original = original;
            Transformed = transformed;
            Logs = logs;
        }

        /// <summary>
        /// 获取原始字符串。
        /// </summary>
        public string Original { get; }

        /// <summary>
        /// 获取应用数字运算后的字符串。
        /// </summary>
        public string Transformed { get; }

        /// <summary>
        /// 获取数字片段变更的详细日志集合。
        /// </summary>
        public IReadOnlyList<NumberOperationLog> Logs { get; }
    }

    /// <summary>
    /// 提供针对字符串中数字片段的查找与批量增减运算功能。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class StringNumberProcessor
    {
        /// <summary>
        /// 对单个字符串中选中的数字片段执行增减操作。
        /// </summary>
        /// <param name="input">要处理的输入字符串，不能为 null。</param>
        /// <param name="operation">数字操作类型（递增或递减）。</param>
        /// <param name="step">每次增减的步长，必须为非负整数。</param>
        /// <param name="options">数字选择配置，控制哪些片段被选中参与运算。</param>
        /// <param name="overflowStrategy">溢出策略，控制数值超出位宽时的处理方式。</param>
        /// <returns>包含原始字符串、变换后字符串以及操作日志的结果对象。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="input"/> 或 <paramref name="options"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="step"/> 小于 0 时抛出。</exception>
        /// <exception cref="InvalidOperationException">
        /// 当没有任何数字片段匹配选择条件，或选择模式过滤后无片段可操作时抛出。
        /// </exception>
        /// <exception cref="OverflowException">
        /// 当溢出策略为 <see cref="OverflowStrategy.Error"/> 且运算结果超出当前位宽允许范围时抛出。
        /// </exception>
        /// <example>
        /// <code>
        /// var options = new NumberSelectionOptions
        /// {
        ///     Mode = NumberSelectionMode.First,
        ///     ApplyToAllMatches = false,
        /// };
        ///
        /// var result = StringNumberProcessor.Process("Item-001", NumberOperation.Increment, 1, options, OverflowStrategy.Wrap);
        /// // result.Transformed == "Item-002"
        /// </code>
        /// </example>
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

        /// <summary>
        /// 对多个字符串执行相同的数字运算配置。
        /// </summary>
        /// <param name="inputs">要处理的字符串序列，不能为 null。</param>
        /// <param name="operation">数字操作类型。</param>
        /// <param name="step">每次增减的步长。</param>
        /// <param name="options">数字选择配置。</param>
        /// <param name="overflowStrategy">溢出策略。</param>
        /// <returns>按输入顺序返回的结果集合。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="inputs"/> 为 null 时抛出。</exception>
        /// <remarks>该方法内部逐个调用 <see cref="Process(string, NumberOperation, int, NumberSelectionOptions, OverflowStrategy)"/>。</remarks>
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

#pragma warning restore SA1649
#pragma warning restore SA1633
#pragma warning restore SA1602
#pragma warning restore SA1600
#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore SA1402
#pragma warning restore SA1629
#pragma warning restore SA1642
#pragma warning restore CS1591

