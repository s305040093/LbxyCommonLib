#pragma warning disable CS1591
#pragma warning disable CS8618
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649

namespace LbxyCommonLib.Cable
{
    using System.Globalization;

    /// <summary>
    /// 精简电缆规格模型，用于统一表达电力电缆、变频电缆与对绞电缆的核心结构参数。
    /// </summary>
    public class CableSpec
    {
        /// <summary>
        /// 电缆型号，例如 YJV、BPYJV（变频）、KVV（控制）。
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 束数，例如 2(3×35+3×6) 中的 2；默认 1 表示单束。
        /// </summary>
        public int BundleCount { get; set; } = 1;

        /// <summary>
        /// 相线（火线）芯数。
        /// </summary>
        public int PhaseCoreCount { get; set; }

        /// <summary>
        /// 相线截面积，以平方毫米为单位。
        /// </summary>
        public double PhaseCoreSection { get; set; }

        /// <summary>
        /// 中性线（N 线）芯数，无则为 0。
        /// </summary>
        public int NeutralCoreCount { get; set; } = 0;

        /// <summary>
        /// 中性线截面积，以平方毫米为单位；无则为 0。
        /// </summary>
        public double NeutralCoreSection { get; set; } = 0;

        /// <summary>
        /// 保护线（PE 线）芯数，无则为 0。
        /// </summary>
        public int ProtectCoreCount { get; set; } = 0;

        /// <summary>
        /// 保护线截面积，以平方毫米为单位；无则为 0。
        /// </summary>
        public double ProtectCoreSection { get; set; } = 0;

        /// <summary>
        /// 指示是否为变频电缆。
        /// </summary>
        public bool IsVariableFrequency { get; set; } = false;

        /// <summary>
        /// 变频屏蔽或控制芯数量，仅对变频电缆有效。
        /// </summary>
        public int VfShieldCoreCount { get; set; } = 0;

        /// <summary>
        /// 变频屏蔽或控制芯截面积，以平方毫米为单位。
        /// </summary>
        public double VfShieldCoreSection { get; set; } = 0;

        /// <summary>
        /// 指示是否为对绞结构电缆，例如 6×2×2.5。
        /// </summary>
        public bool IsTwistedPair { get; set; } = false;

        /// <summary>
        /// 对绞对数，例如 6×2×2.5 中的 6。
        /// </summary>
        public int TwistedPairCount { get; set; } = 0;

        /// <summary>
        /// 每对芯数，例如 6×2×2.5 中的 2。
        /// </summary>
        public int CoresPerPair { get; set; } = 0;

        /// <summary>
        /// 控制线芯数，例如 7×1.5 中的 7。
        /// </summary>
        public int ControlCoreCount { get; set; } = 0;

        /// <summary>
        /// 控制线截面积，以平方毫米为单位，例如 7×1.5 中的 1.5。
        /// </summary>
        public double ControlCoreSection { get; set; } = 0;

        /// <summary>
        /// 电缆类型，例如“电力电缆”“控制电缆”“变频电缆”或“未识别”。
        /// </summary>
        public string CableType { get; set; } = "未识别";

        /// <summary>
        /// 电缆类别，用于区分动力电缆与控制电缆。
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 生成标准化规格描述，用于作为电缆规格的唯一文本表示。
        /// </summary>
        /// <returns>规格描述字符串，例如 4×70+1×35 或 2×6×2×2.5。</returns>
        public string GetSpecDesc()
        {
            if (this.IsTwistedPair && this.TwistedPairCount > 0 && this.CoresPerPair > 0)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}×{1}×{2}×{3}",
                    this.BundleCount,
                    this.TwistedPairCount,
                    this.CoresPerPair,
                    this.PhaseCoreSection);
            }

            var bundleDesc = this.BundleCount > 1 ? string.Format(CultureInfo.InvariantCulture, "{0}(", this.BundleCount) : string.Empty;
            var coreDesc = string.Format(CultureInfo.InvariantCulture, "{0}×{1}", this.PhaseCoreCount, this.PhaseCoreSection);

            if (this.NeutralCoreCount > 0)
            {
                coreDesc += string.Format(CultureInfo.InvariantCulture, "+{0}×{1}", this.NeutralCoreCount, this.NeutralCoreSection);
            }

            if (this.ProtectCoreCount > 0)
            {
                coreDesc += string.Format(CultureInfo.InvariantCulture, "+{0}×{1}", this.ProtectCoreCount, this.ProtectCoreSection);
            }

            if (this.IsVariableFrequency && this.VfShieldCoreCount > 0)
            {
                coreDesc += string.Format(CultureInfo.InvariantCulture, "+{0}×{1}", this.VfShieldCoreCount, this.VfShieldCoreSection);
            }

            var bundleEnd = this.BundleCount > 1 ? ")" : string.Empty;

            return bundleDesc + coreDesc + bundleEnd;
        }

        /// <summary>
        /// 快速构造普通电力电缆规格，例如 4×70+1×35。
        /// </summary>
        /// <param name="model">电缆型号。</param>
        /// <param name="phaseCount">相线芯数。</param>
        /// <param name="phaseSection">相线截面积。</param>
        /// <param name="neutralCount">中性线芯数。</param>
        /// <param name="neutralSection">中性线截面积。</param>
        /// <param name="protectCount">保护线芯数。</param>
        /// <param name="protectSection">保护线截面积。</param>
        /// <returns>构造好的 <see cref="CableSpec"/> 实例。</returns>
        public static CableSpec CreatePowerCable(string model, int phaseCount, double phaseSection, int neutralCount = 0, double neutralSection = 0, int protectCount = 0, double protectSection = 0)
        {
            return new CableSpec
            {
                Model = model,
                PhaseCoreCount = phaseCount,
                PhaseCoreSection = phaseSection,
                NeutralCoreCount = neutralCount,
                NeutralCoreSection = neutralSection,
                ProtectCoreCount = protectCount,
                ProtectCoreSection = protectSection,
                CableType = "电力电缆",
            };
        }

        /// <summary>
        /// 快速构造变频电缆规格，例如 3×35+3×6 或 2(3×35+3×6)。
        /// </summary>
        /// <param name="model">电缆型号。</param>
        /// <param name="phaseCount">相线芯数。</param>
        /// <param name="phaseSection">相线截面积。</param>
        /// <param name="shieldCount">屏蔽或控制芯数量。</param>
        /// <param name="shieldSection">屏蔽或控制芯截面积。</param>
        /// <param name="bundleCount">束数，默认 1。</param>
        /// <returns>构造好的 <see cref="CableSpec"/> 实例。</returns>
        public static CableSpec CreateVfCable(string model, int phaseCount, double phaseSection, int shieldCount, double shieldSection, int bundleCount = 1)
        {
            return new CableSpec
            {
                Model = model,
                BundleCount = bundleCount,
                PhaseCoreCount = phaseCount,
                PhaseCoreSection = phaseSection,
                IsVariableFrequency = true,
                VfShieldCoreCount = shieldCount,
                VfShieldCoreSection = shieldSection,
                CableType = "变频电缆",
            };
        }

        /// <summary>
        /// 快速构造对绞电缆规格，例如 6×2×2.5 或 2×6×2×2.5。
        /// </summary>
        /// <param name="model">电缆型号。</param>
        /// <param name="pairCount">对绞对数。</param>
        /// <param name="coresPerPair">每对芯数。</param>
        /// <param name="singleCoreSection">单芯截面积。</param>
        /// <param name="isVf">是否为变频对绞电缆。</param>
        /// <returns>构造好的 <see cref="CableSpec"/> 实例。</returns>
        public static CableSpec CreateTwistedCable(string model, int pairCount, int coresPerPair, double singleCoreSection, bool isVf = false)
        {
            return new CableSpec
            {
                Model = model,
                PhaseCoreSection = singleCoreSection,
                IsTwistedPair = true,
                TwistedPairCount = pairCount,
                CoresPerPair = coresPerPair,
                IsVariableFrequency = isVf,
            };
        }
    }
}

#pragma warning restore SA1649
#pragma warning restore SA1633
#pragma warning restore SA1602
#pragma warning restore SA1600
#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore CS1591
