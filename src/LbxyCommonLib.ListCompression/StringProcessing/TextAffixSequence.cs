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

    public sealed class TextAffixOptions
    {
        public string FixedPrefix { get; set; } = string.Empty;

        public string FixedSuffix { get; set; } = string.Empty;
    }

    public sealed class NumericAffixOptions
    {
        public BigInteger Start { get; set; } = BigInteger.One;

        public int Step { get; set; } = 1;

        public int Width { get; set; }

        public int NumberBase { get; set; } = 10;

        public bool Uppercase { get; set; } = true;
    }

    public sealed class TextAffixSequenceOptions
    {
        public TextAffixSequenceOptions()
        {
            Affix = new TextAffixOptions();
            PrefixNumber = new NumericAffixOptions();
            SuffixNumber = new NumericAffixOptions();
        }

        public TextAffixOptions Affix { get; }

        public bool UsePrefixNumber { get; set; }

        public bool UseSuffixNumber { get; set; }

        public NumericAffixOptions PrefixNumber { get; }

        public NumericAffixOptions SuffixNumber { get; }
    }

    public sealed class TextAffixSequence
    {
        private readonly TextAffixSequenceOptions options;

        private BigInteger prefixCurrent;

        private BigInteger suffixCurrent;

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

