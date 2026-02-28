#pragma warning disable SA1101
#pragma warning disable SA1600
#pragma warning disable SA1503
#pragma warning disable SA1204
#pragma warning disable SA1028
#pragma warning disable SA1122
#pragma warning disable SA1201
#pragma warning disable SA1513
#pragma warning disable SA1515
#pragma warning disable SA1200
#pragma warning disable SA1210
#pragma warning disable SA1309
#pragma warning disable SA1128
#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace LbxyCommonLib.Cable
{
    public class CableParser
    {
        private readonly string[] _powerKeywords;
        private readonly string[] _controlKeywords;

        // Compiled Regexes with Timeout
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

        private static readonly Regex BundleRegex = new Regex(
            @"^\s*(-?\d+)\s*\((.+)\)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            RegexTimeout);

        // Twisted Pair: m x 2 x Num (e.g. 6x2x2.5)
        // Normalized: 6x2x2.5
        private static readonly Regex TwistedPair3Regex = new Regex(
            @"^(\d+)x(2)x(\d+(?:\.\d+)?)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            RegexTimeout);

        // Twisted Pair with Bundle: n x m x 2 x Num (e.g. 8x6x2x2.5)
        private static readonly Regex TwistedPair4Regex = new Regex(
            @"^(-?\d+)x(\d+)x(2)x(\d+(?:\.\d+)?)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            RegexTimeout);

        // Standard Plus: A x S1 + B x S2
        private static readonly Regex PlusRegex = new Regex(
            @"^(\d+)x([^\+]+)\+(\d+)x([^\+]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            RegexTimeout);

        // Simple: A x S
        private static readonly Regex SimpleRegex = new Regex(
            @"^(\d+)x(.+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            RegexTimeout);

        /// <summary>
        /// Initializes a new instance of the <see cref="CableParser"/> class using the default keyword provider.
        /// </summary>
        public CableParser() : this(new DefaultCableKeywordProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CableParser"/> class with a custom keyword provider.
        /// </summary>
        /// <param name="provider">The keyword provider.</param>
        public CableParser(ICableKeywordProvider provider)
        {
            if (provider == null)
            {
                provider = new DefaultCableKeywordProvider();
            }

            // Cache keywords for performance
            _powerKeywords = provider.GetPowerKeywords().ToArray();
            _controlKeywords = provider.GetControlKeywords().ToArray();
        }

        /// <summary>
        /// 解析电缆规格字符串，返回结构化的 <see cref="CableSpec"/> 对象。
        /// 支持格式：
        /// 1. 标准格式：3x25+1x16
        /// 2. 多并格式：2(3x25+1x16) 表示2束
        /// 3. 简单格式：4x25 (自动映射为3相+1中性), 5x10 (3相+1中性+1保护)
        /// </summary>
        /// <param name="model">电缆型号 (如 YJV, KVV)</param>
        /// <param name="specString">规格字符串</param>
        /// <returns>解析后的电缆规格对象</returns>
        /// <exception cref="ArgumentNullException">当 specString 为 null 或空白时抛出</exception>
        /// <exception cref="ArgumentException">当 BundleCount &lt;= 0 时抛出</exception>
        /// <exception cref="FormatException">当规格格式无法解析时抛出</exception>
        public CableSpec Parse(string model, string specString)
        {
            var spec = new CableSpec { Model = model };

            // 1. 前置校验与类别判定
            if (!string.IsNullOrWhiteSpace(model))
            {
                bool isPower = IsPowerCable(model);
                bool isControl = IsControlCable(model);

                if (isPower && isControl)
                {
                    throw new AmbiguousCategoryException("Model contains both Power and Control cable keywords.", model);
                }

                if (isPower)
                {
                    spec.Category = "动力电缆";
                    spec.CableType = "动力电缆";
                }
                else if (isControl)
                {
                    spec.Category = "控制电缆";
                    spec.CableType = "控制电缆";
                }
                else
                {
                    // 默认归类或保持未知，这里根据习惯如果未匹配到控制电缆特征，通常视为动力或者其他
                    // 暂时保持 Category 为 null，CableType 为 "未识别"
                }
            }

            // 2. 规格解析
            ParseSpecString(spec, specString);

            return spec;
        }

        private bool IsPowerCable(string model)
        {
            foreach (var kw in _powerKeywords)
            {
                // Ensure not preceded by K (case insensitive) to avoid matching KYJV as YJV, KVV as VV
                if (Regex.IsMatch(model, $"(?<!K){Regex.Escape(kw)}", RegexOptions.IgnoreCase)) return true;
            }
            return false;
        }

        private bool IsControlCable(string model)
        {
            // K开头通常是控制电缆，除非是特殊型号
            if (model.StartsWith("K", StringComparison.OrdinalIgnoreCase) || model.StartsWith("ZR-K", StringComparison.OrdinalIgnoreCase) || model.StartsWith("NH-K", StringComparison.OrdinalIgnoreCase) || model.StartsWith("WDZ-K", StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (var kw in _controlKeywords)
            {
                if (model.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private void ParseSpecString(CableSpec spec, string specString)
        {
            if (string.IsNullOrWhiteSpace(specString))
            {
                throw new ArgumentNullException(nameof(specString));
            }

            // 预处理：归一化分隔符，去除空白，统一括号
            // 确保 "X" 和 "*" 都被归一化为 "x"
            string normalized = specString
                .Replace("×", "x").Replace("*", "x").Replace("X", "x")
                .Replace("（", "(").Replace("）", ")")
                .Trim();

            // 处理多并电缆格式: BundleCount(CoreExpression)
            // 模式: 整数 + ( + 任意内容 + )
            var bundleMatch = BundleRegex.Match(normalized);
            if (bundleMatch.Success)
            {
                int bundleCount = int.Parse(bundleMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                if (bundleCount <= 0)
                {
                    throw new ArgumentException("BundleCount must be >= 1", nameof(specString));
                }

                spec.BundleCount = bundleCount;
                string coreExpr = bundleMatch.Groups[2].Value;

                // 递归解析括号内的芯线表达式
                // 注意：为了避免无限递归（如果括号内格式仍匹配此正则），
                // 实际上标准芯线表达式（如 3x25+1x16）不应被此正则误判。
                // 3x25+1x16 不符合 ^\d+\(...\)$，除非写成 3(x25...) 这种错误格式。
                ParseSpecString(spec, coreExpr);
                return;
            }

            // 简单处理：移除空格
            normalized = normalized.Replace(" ", "");

            // 优先处理对绞电缆格式: TwistedPairCount x CoresPerPair x Section (6x2x2.5)
            // 模式: 整数 x 2 x 数字(.数字)
            var matchTwisted3 = TwistedPair3Regex.Match(normalized);
            if (matchTwisted3.Success)
            {
                spec.IsTwistedPair = true;
                spec.TwistedPairCount = int.Parse(matchTwisted3.Groups[1].Value, CultureInfo.InvariantCulture);
                spec.CoresPerPair = int.Parse(matchTwisted3.Groups[2].Value, CultureInfo.InvariantCulture);
                spec.ControlCoreSection = ParseSection(matchTwisted3.Groups[3].Value, specString);
                return;
            }

            // 优先处理带束数的对绞电缆格式: BundleCount x TwistedPairCount x CoresPerPair x Section (8x6x2x2.5)
            // 模式: 整数 x 整数 x 2 x 数字(.数字)
            var matchTwisted4 = TwistedPair4Regex.Match(normalized);
            if (matchTwisted4.Success)
            {
                int bundleCount = int.Parse(matchTwisted4.Groups[1].Value, CultureInfo.InvariantCulture);
                if (bundleCount <= 0)
                {
                    throw new ArgumentException("BundleCount must be >= 1", nameof(specString));
                }
                spec.BundleCount = bundleCount;
                spec.IsTwistedPair = true;
                spec.TwistedPairCount = int.Parse(matchTwisted4.Groups[2].Value, CultureInfo.InvariantCulture);
                spec.CoresPerPair = int.Parse(matchTwisted4.Groups[3].Value, CultureInfo.InvariantCulture);
                spec.ControlCoreSection = ParseSection(matchTwisted4.Groups[4].Value, specString);
                return;
            }

            // 优先匹配 A x S1 + B x S2 结构 (3+1, 4+1, 3+2)
            // 模式: 数字 x 数字(.数字) + 数字 x 数字(.数字)
            // 放宽正则以捕获非法字符并抛出InvalidSectionException
            var matchPlus = PlusRegex.Match(normalized);
            if (matchPlus.Success)
            {
                int count1 = int.Parse(matchPlus.Groups[1].Value, CultureInfo.InvariantCulture);
                double section1 = ParseSection(matchPlus.Groups[2].Value, specString);
                int count2 = int.Parse(matchPlus.Groups[3].Value, CultureInfo.InvariantCulture);
                double section2 = ParseSection(matchPlus.Groups[4].Value, specString);

                // 变频电缆识别: 3xS1 + 3xS2 (如 3x50+3x25)
                if (count1 == 3 && count2 == 3)
                {
                    spec.IsVariableFrequency = true;
                    spec.PhaseCoreCount = 3;
                    spec.PhaseCoreSection = section1;
                    spec.VfShieldCoreCount = 3;
                    spec.VfShieldCoreSection = section2;
                    return;
                }

                // 根据优先级解析
                if (spec.Category == "控制电缆")
                {
                    // 控制电缆通常不写成 3+1，但如果写了，视为混用或特殊控制
                    // 简单累加？或者报错？用户说“对控制电缆：依次解析到控制线属性”
                    // 如果控制电缆出现 3+1，可能是一个特殊的混合缆。
                    // 暂时将第一组视为控制线，第二组视为... 未知。
                    // 但通常控制电缆是 N x S。
                    // 这里为了健壮性，如果判定为控制电缆，仍按控制线处理？
                    // 实际上 3+1 强暗示是动力电缆。
                    // 如果 Model 是 KVV 但规格是 3x4+1x2.5，可能是地线。
                    spec.ControlCoreCount = count1;
                    spec.ControlCoreSection = section1;
                    // 忽略第二组或记录？用户需求未详述控制电缆的 + 号情况。
                    // 暂且只处理动力电缆的 + 号逻辑。
                }
                else
                {
                    // 默认为动力电缆逻辑 (Model为空或为动力电缆)
                    HandlePlusFormat(spec, count1, section1, count2, section2);
                }
                return;
            }

            // 匹配 A x S 结构
            // 放宽正则以捕获非法字符
            var matchSimple = SimpleRegex.Match(normalized);
            if (matchSimple.Success)
            {
                int count = int.Parse(matchSimple.Groups[1].Value, CultureInfo.InvariantCulture);
                double section = ParseSection(matchSimple.Groups[2].Value, specString);

                if (spec.Category == "控制电缆")
                {
                    spec.ControlCoreCount = count;
                    spec.ControlCoreSection = section;
                }
                else
                {
                    // 默认为动力电缆
                    if (count == 4)
                    {
                        // 4xS -> 3相线 + 1中性线
                        spec.PhaseCoreCount = 3;
                        spec.PhaseCoreSection = section;
                        spec.NeutralCoreCount = 1;
                        spec.NeutralCoreSection = section;
                    }
                    else if (count == 5)
                    {
                        // 5xS -> 3相线 + 1中性线 + 1保护线
                        spec.PhaseCoreCount = 3;
                        spec.PhaseCoreSection = section;
                        spec.NeutralCoreCount = 1;
                        spec.NeutralCoreSection = section;
                        spec.ProtectCoreCount = 1;
                        spec.ProtectCoreSection = section;
                    }
                    else
                    {
                        spec.PhaseCoreCount = count;
                        spec.PhaseCoreSection = section;
                    }
                }
                return;
            }

            // 如果都不匹配，可能是非法格式或复杂格式
            // 用户要求：若出现非标准写法，报错处理
            // 这里可以抛出异常或记录错误
            // 既然用户提到“报错处理”，我抛出 ArgumentException 或自定义异常？
            // 考虑到 3.b 抛出 InvalidSectionException，这里若是格式不对，抛出 FormatException
            throw new FormatException($"Unsupported spec format: {specString}");
        }

        private void HandlePlusFormat(CableSpec spec, int count1, double section1, int count2, double section2)
        {
            // 3+1 结构
            if (count1 == 3 && count2 == 1)
            {
                spec.PhaseCoreCount = 3;
                spec.PhaseCoreSection = section1;
                spec.NeutralCoreCount = 1;
                spec.NeutralCoreSection = section2;
                return;
            }

            // 3+2 结构
            if (count1 == 3 && count2 == 2)
            {
                spec.PhaseCoreCount = 3;
                spec.PhaseCoreSection = section1;
                // 通常 3+2 是 1N + 1PE
                spec.NeutralCoreCount = 1;
                spec.NeutralCoreSection = section2;
                spec.ProtectCoreCount = 1;
                spec.ProtectCoreSection = section2;
                return;
            }

            // 4+1 结构
            if (count1 == 4 && count2 == 1)
            {
                // 4大1小，通常是 3相+1中性（同大） + 1地（小）
                spec.PhaseCoreCount = 3;
                spec.PhaseCoreSection = section1;
                spec.NeutralCoreCount = 1;
                spec.NeutralCoreSection = section1; // 中性线与相线同截面
                spec.ProtectCoreCount = 1;
                spec.ProtectCoreSection = section2;
                return;
            }

            // 其他 A+B 结构
            // 默认 A 为相线，B 为中性线
            spec.PhaseCoreCount = count1;
            spec.PhaseCoreSection = section1;
            spec.NeutralCoreCount = count2;
            spec.NeutralCoreSection = section2;
        }

        private double ParseSection(string value, string fullSpec)
        {
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            throw new InvalidSectionException("Invalid section value", value);
        }
    }
}
