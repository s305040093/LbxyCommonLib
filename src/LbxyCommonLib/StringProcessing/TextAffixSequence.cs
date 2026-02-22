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

    /// <summary>
    /// 配置固定的文本前缀和后缀。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class TextAffixOptions
    {
        /// <summary>
        /// 获取或设置固定前缀文本。
        /// </summary>
        public string FixedPrefix { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置固定后缀文本。
        /// </summary>
        public string FixedSuffix { get; set; } = string.Empty;
    }

    /// <summary>
    /// 配置用于前缀或后缀的数字序列格式。
    /// </summary>
    public sealed class NumericAffixOptions
    {
        /// <summary>
        /// 获取或设置起始数值，后续调用会在此基础上按步长递增或递减。
        /// </summary>
        public BigInteger Start { get; set; } = BigInteger.One;

        /// <summary>
        /// 获取或设置每次生成后数值的增减步长，可以为负数。
        /// </summary>
        public int Step { get; set; } = 1;

        /// <summary>
        /// 获取或设置输出数字的最小宽度；大于 0 时不足位数将使用前导零填充。
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 获取或设置数字进制，默认 10。有效范围为 2 到 36。
        /// </summary>
        public int NumberBase { get; set; } = 10;

        /// <summary>
        /// 指示在非十进制输出时是否使用大写字母；为 false 时使用小写字母。
        /// </summary>
        public bool Uppercase { get; set; } = true;
    }

    /// <summary>
    /// 控制文本前后缀序列生成的整体配置。
    /// </summary>
    public sealed class TextAffixSequenceOptions
    {
        public TextAffixSequenceOptions()
        {
            Affix = new TextAffixOptions();
            PrefixNumber = new NumericAffixOptions();
            SuffixNumber = new NumericAffixOptions();
        }

        /// <summary>
        /// 获取固定前后缀配置。
        /// </summary>
        public TextAffixOptions Affix { get; }

        /// <summary>
        /// 指示是否在前缀中包含自动递增的数字。
        /// </summary>
        public bool UsePrefixNumber { get; set; }

        /// <summary>
        /// 指示是否在后缀中包含自动递增的数字。
        /// </summary>
        public bool UseSuffixNumber { get; set; }

        /// <summary>
        /// 获取前缀数字配置。
        /// </summary>
        public NumericAffixOptions PrefixNumber { get; }

        /// <summary>
        /// 获取后缀数字配置。
        /// </summary>
        public NumericAffixOptions SuffixNumber { get; }
    }

    /// <summary>
    /// 根据配置生成带有固定前后缀及可选数字序列的文本。
    /// </summary>
    public sealed class TextAffixSequence
    {
        private readonly TextAffixSequenceOptions options;

        private BigInteger prefixCurrent;

        private BigInteger suffixCurrent;

        /// <summary>
        /// 初始化 <see cref="TextAffixSequence"/> 类型的新实例。
        /// </summary>
        /// <param name="options">序列生成配置，不能为空。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="options"/> 为 null 时抛出。</exception>
        public TextAffixSequence(TextAffixSequenceOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            if (options.UsePrefixNumber)
            {
                prefixCurrent = options.PrefixNumber.Start;
            }

            if (options.UseSuffixNumber)
            {
                suffixCurrent = options.SuffixNumber.Start;
            }
        }

        /// <summary>
        /// 基于当前状态生成下一个带前后缀的字符串，并推进内部数字游标。
        /// </summary>
        /// <param name="core">核心文本内容，不能为空。</param>
        /// <returns>应用了配置前缀、核心文本和后缀的完整字符串。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="core"/> 为 null 时抛出。</exception>
        /// <example>
        /// <code>
        /// var opts = new TextAffixSequenceOptions
        /// {
        ///     Affix = { FixedPrefix = "Item-", FixedSuffix = string.Empty },
        ///     UseSuffixNumber = true,
        /// };
        /// opts.SuffixNumber.Start = 1;
        /// opts.SuffixNumber.Width = 3;
        ///
        /// var seq = new TextAffixSequence(opts);
        /// var first = seq.Next("Core");  // "Item-Core001"
        /// var second = seq.Next("Core"); // "Item-Core002"
        /// </code>
        /// </example>
        public string Next(string core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core));
            }

            var prefixText = options.Affix.FixedPrefix;
            var suffixText = options.Affix.FixedSuffix;

            if (options.UsePrefixNumber)
            {
                prefixText += FormatNumber(prefixCurrent, options.PrefixNumber);
                prefixCurrent += options.PrefixNumber.Step;
            }

            if (options.UseSuffixNumber)
            {
                suffixText = FormatNumber(suffixCurrent, options.SuffixNumber) + suffixText;
                suffixCurrent += options.SuffixNumber.Step;
            }

            return prefixText + core + suffixText;
        }

        /// <summary>
        /// 为多个核心文本依次生成带前后缀的字符串。
        /// </summary>
        /// <param name="cores">核心文本序列，不能为空。</param>
        /// <returns>与输入顺序对应的生成结果列表。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="cores"/> 为 null 时抛出。</exception>
        public IReadOnlyList<string> NextMany(IEnumerable<string> cores)
        {
            if (cores == null)
            {
                throw new ArgumentNullException(nameof(cores));
            }

            var results = new List<string>();

            foreach (var core in cores)
            {
                results.Add(Next(core));
            }

            return results;
        }

        /// <summary>
        /// 按配置将数值格式化为指定进制和宽度的字符串。
        /// </summary>
        /// <param name="value">要格式化的数值。</param>
        /// <param name="cfg">数字格式配置。</param>
        /// <returns>格式化后的字符串表示。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="cfg"/> 的 <see cref="NumericAffixOptions.NumberBase"/> 不在 2 到 36 范围内时抛出。
        /// </exception>
        private static string FormatNumber(BigInteger value, NumericAffixOptions cfg)
        {
            if (cfg.NumberBase == 10)
            {
                if (cfg.Width > 0 && value >= BigInteger.Zero)
                {
                    var format = "D" + cfg.Width.ToString(CultureInfo.InvariantCulture);
                    return value.ToString(format, CultureInfo.InvariantCulture);
                }

                return value.ToString(CultureInfo.InvariantCulture);
            }

            var negative = value < BigInteger.Zero;
            if (negative)
            {
                value = BigInteger.Negate(value);
            }

            var digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var radix = cfg.NumberBase;

            if (radix < 2 || radix > digits.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(cfg.NumberBase));
            }

            if (!cfg.Uppercase)
            {
                digits = digits.ToLowerInvariant();
            }

            if (value.IsZero)
            {
                var single = "0";
                if (cfg.Width > 1)
                {
                    return new string('0', cfg.Width - 1) + single;
                }

                return single;
            }

            var chars = new List<char>();
            while (value > BigInteger.Zero)
            {
                value = BigInteger.DivRem(value, radix, out var rem);
                chars.Add(digits[(int)rem]);
            }

            chars.Reverse();
            var s = new string(chars.ToArray());

            if (cfg.Width > 0 && s.Length < cfg.Width)
            {
                s = new string('0', cfg.Width - s.Length) + s;
            }

            return negative ? "-" + s : s;
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
