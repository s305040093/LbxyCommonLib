using System;
using System.Diagnostics;
using LbxyCommonLib.Cable;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests.Cable
{
    [TestFixture]
    public class CableParserTests
    {
        private CableParser _parser;

        [SetUp]
        public void Setup()
        {
            _parser = new CableParser();
        }

        [Test]
        [TestCase("YJV", "1x630", 1, 630, 0, 0, 0, 0, "动力电缆", Description = "单芯动力电缆")]
        [TestCase("VV", "2x4", 2, 4, 0, 0, 0, 0, "动力电缆", Description = "两芯动力电缆")]
        [TestCase("YJV", "3x16", 3, 16, 0, 0, 0, 0, "动力电缆", Description = "三芯动力电缆")]
        [TestCase("YJV", "4x25", 3, 25, 1, 25, 0, 0, "动力电缆", Description = "四芯等截面动力电缆 -> 3相+1中性")]
        [TestCase("YJV", "5x10", 3, 10, 1, 10, 1, 10, "动力电缆", Description = "五芯等截面动力电缆 -> 3相+1中性+1保护")]
        [TestCase("YJLV", "3x240", 3, 240, 0, 0, 0, 0, "动力电缆", Description = "铝芯三芯")]
        [TestCase("YJV22", "3x95", 3, 95, 0, 0, 0, 0, "动力电缆", Description = "铠装三芯")]
        [TestCase("NH-YJV", "4x16", 3, 16, 1, 16, 0, 0, "动力电缆", Description = "耐火四芯 -> 3+1")]
        [TestCase("WDZ-YJV", "5x6", 3, 6, 1, 6, 1, 6, "动力电缆", Description = "低烟无卤五芯 -> 3+1+1")]
        [TestCase("ZR-YJV", "3x10", 3, 10, 0, 0, 0, 0, "动力电缆", Description = "阻燃三芯")]
        [TestCase("VLV22", "4x185", 3, 185, 1, 185, 0, 0, "动力电缆", Description = "铝芯铠装四芯 -> 3+1")]
        [TestCase("YJV", "3*4", 3, 4, 0, 0, 0, 0, "动力电缆", Description = "星号分隔符")]
        [TestCase("YJV", "4*6", 3, 6, 1, 6, 0, 0, "动力电缆", Description = "星号分隔符四芯")]
        [TestCase("YJV", "5*16", 3, 16, 1, 16, 1, 16, "动力电缆", Description = "星号分隔符五芯")]
        public void Parse_PowerCable_UniformSection_ShouldMapCorrectly(
            string model, string specStr,
            int expectedPhaseCount, double expectedPhaseSection,
            int expectedNeutralCount, double expectedNeutralSection,
            int expectedProtectCount, double expectedProtectSection,
            string expectedCategory)
        {
            var spec = _parser.Parse(model, specStr);

            Assert.Multiple(() =>
            {
                Assert.That(spec.Category, Is.EqualTo(expectedCategory), "Category mismatch");
                Assert.That(spec.PhaseCoreCount, Is.EqualTo(expectedPhaseCount), "PhaseCoreCount mismatch");
                Assert.That(spec.PhaseCoreSection, Is.EqualTo(expectedPhaseSection), "PhaseCoreSection mismatch");
                Assert.That(spec.NeutralCoreCount, Is.EqualTo(expectedNeutralCount), "NeutralCoreCount mismatch");
                Assert.That(spec.NeutralCoreSection, Is.EqualTo(expectedNeutralSection), "NeutralCoreSection mismatch");
                Assert.That(spec.ProtectCoreCount, Is.EqualTo(expectedProtectCount), "ProtectCoreCount mismatch");
                Assert.That(spec.ProtectCoreSection, Is.EqualTo(expectedProtectSection), "ProtectCoreSection mismatch");
            });
        }

        [Test]
        [TestCase("YJV", "3x120+1x70", 3, 120, 1, 70, 0, 0, "动力电缆", Description = "标准3+1")]
        [TestCase("VV", "3x240+1x120", 3, 240, 1, 120, 0, 0, "动力电缆", Description = "标准3+1")]
        [TestCase("YJV22", "3x50+1x25", 3, 50, 1, 25, 0, 0, "动力电缆", Description = "标准3+1")]
        [TestCase("YJLV", "3*120+1*70", 3, 120, 1, 70, 0, 0, "动力电缆", Description = "星号分隔3+1")]
        [TestCase("YJV", "3X120+1X70", 3, 120, 1, 70, 0, 0, "动力电缆", Description = "大写X分隔3+1")]
        [TestCase("YJV", "3×120+1×70", 3, 120, 1, 70, 0, 0, "动力电缆", Description = "乘号分隔3+1")]
        [TestCase("YJV", "3 x 120 + 1 x 70", 3, 120, 1, 70, 0, 0, "动力电缆", Description = "带空格3+1")]
        [TestCase("YJV", "3x120+2x70", 3, 120, 1, 70, 1, 70, "动力电缆", Description = "标准3+2 (3相+1中性+1保护)")]
        [TestCase("VV22", "3*95+2*50", 3, 95, 1, 50, 1, 50, "动力电缆", Description = "标准3+2")]
        [TestCase("YJV", "4x185+1x95", 3, 185, 1, 185, 1, 95, "动力电缆", Description = "4+1结构 (3相+1中性(大)+1保护(小))")]
        [TestCase("YJV", "4*120+1*70", 3, 120, 1, 120, 1, 70, "动力电缆", Description = "4+1结构")]
        [TestCase("WDZ-YJV", "3x120+1x70", 3, 120, 1, 70, 0, 0, "动力电缆", Description = "低烟无卤3+1")]
        [TestCase("ZR-YJV", "3x240+1x120", 3, 240, 1, 120, 0, 0, "动力电缆", Description = "阻燃3+1")]
        [TestCase("NH-YJV", "3x50+1x25", 3, 50, 1, 25, 0, 0, "动力电缆", Description = "耐火3+1")]
        [TestCase("ZA-YJV", "3x95+1x50", 3, 95, 1, 50, 0, 0, "动力电缆", Description = "ZA阻燃3+1")]
        [TestCase("ZB-YJV", "3x16+1x10", 3, 16, 1, 10, 0, 0, "动力电缆", Description = "ZB阻燃3+1")]
        [TestCase("ZC-YJV", "3x25+1x16", 3, 25, 1, 16, 0, 0, "动力电缆", Description = "ZC阻燃3+1")]
        [TestCase("YJV22", "3x35+1x16", 3, 35, 1, 16, 0, 0, "动力电缆", Description = "铠装3+1")]
        public void Parse_PowerCable_MixedSection_ShouldMapCorrectly(
            string model, string specStr,
            int expectedPhaseCount, double expectedPhaseSection,
            int expectedNeutralCount, double expectedNeutralSection,
            int expectedProtectCount, double expectedProtectSection,
            string expectedCategory)
        {
            var spec = _parser.Parse(model, specStr);

            Assert.Multiple(() =>
            {
                Assert.That(spec.Category, Is.EqualTo(expectedCategory), "Category mismatch");
                Assert.That(spec.PhaseCoreCount, Is.EqualTo(expectedPhaseCount), "PhaseCoreCount mismatch");
                Assert.That(spec.PhaseCoreSection, Is.EqualTo(expectedPhaseSection), "PhaseCoreSection mismatch");
                Assert.That(spec.NeutralCoreCount, Is.EqualTo(expectedNeutralCount), "NeutralCoreCount mismatch");
                Assert.That(spec.NeutralCoreSection, Is.EqualTo(expectedNeutralSection), "NeutralCoreSection mismatch");
                Assert.That(spec.ProtectCoreCount, Is.EqualTo(expectedProtectCount), "ProtectCoreCount mismatch");
                Assert.That(spec.ProtectCoreSection, Is.EqualTo(expectedProtectSection), "ProtectCoreSection mismatch");
            });
        }

        [Test]
        [TestCase("KVV", "4x1.5", 4, 1.5, "控制电缆")]
        [TestCase("KVVP", "10x1.5", 10, 1.5, "控制电缆")]
        [TestCase("KYJV", "14x2.5", 14, 2.5, "控制电缆")]
        [TestCase("ZR-KVV", "7x1.5", 7, 1.5, "控制电缆")]
        [TestCase("NH-KVV", "37x1.5", 37, 1.5, "控制电缆")]
        [TestCase("KVV22", "4x2.5", 4, 2.5, "控制电缆")]
        [TestCase("KYJVP", "19x1.5", 19, 1.5, "控制电缆")]
        [TestCase("WDZ-KVV", "24x1.5", 24, 1.5, "控制电缆")]
        [TestCase("KVVP2", "4x4", 4, 4, "控制电缆")]
        [TestCase("NH-KVVP", "7x2.5", 7, 2.5, "控制电缆")]
        [TestCase("ZR-KYJV", "10x2.5", 10, 2.5, "控制电缆")]
        [TestCase("KVV", "2x1.5", 2, 1.5, "控制电缆")]
        [TestCase("KVV", "3x1.5", 3, 1.5, "控制电缆")]
        [TestCase("KVV", "5x1.5", 5, 1.5, "控制电缆")]
        [TestCase("KVV", "30x1.5", 30, 1.5, "控制电缆")]
        public void Parse_ControlCable_ShouldMapCorrectly(string model, string specStr, int expectedControlCount, double expectedControlSection, string expectedCategory)
        {
            var spec = _parser.Parse(model, specStr);

            Assert.Multiple(() =>
            {
                Assert.That(spec.Category, Is.EqualTo(expectedCategory), "Category mismatch");
                Assert.That(spec.ControlCoreCount, Is.EqualTo(expectedControlCount), "ControlCoreCount mismatch");
                Assert.That(spec.ControlCoreSection, Is.EqualTo(expectedControlSection), "ControlCoreSection mismatch");
            });
        }

        [Test]
        public void Parse_AmbiguousCategory_ShouldThrow()
        {
            Assert.Throws<AmbiguousCategoryException>(() => _parser.Parse("YJV KVV", "3x10"));
        }

        [Test]
        [TestCase("RVSP", "6x2x2.5", 1, 6, 2, 2.5, Description = "标准对绞: 6对2芯2.5平方")]
        [TestCase("RVSP", "8x6x2x2.5", 8, 6, 2, 2.5, Description = "多束对绞: 8束(6对2芯2.5平方)")]
        [TestCase("RVSP", "1x2x1.5", 1, 1, 2, 1.5, Description = "单对绞: 1对2芯1.5平方")]
        [TestCase("RVSP", " 2 x 4 x 2 x 1.0 ", 2, 4, 2, 1.0, Description = "带空格多束对绞")]
        public void Parse_TwistedPair_ShouldMapCorrectly(
            string model, string specStr,
            int expectedBundleCount,
            int expectedTwistedPairCount,
            int expectedCoresPerPair,
            double expectedControlCoreSection)
        {
            var spec = _parser.Parse(model, specStr);
            Assert.Multiple(() =>
            {
                Assert.That(spec.IsTwistedPair, Is.True, "IsTwistedPair mismatch");
                Assert.That(spec.BundleCount, Is.EqualTo(expectedBundleCount), "BundleCount mismatch");
                Assert.That(spec.TwistedPairCount, Is.EqualTo(expectedTwistedPairCount), "TwistedPairCount mismatch");
                Assert.That(spec.CoresPerPair, Is.EqualTo(expectedCoresPerPair), "CoresPerPair mismatch");
                Assert.That(spec.ControlCoreSection, Is.EqualTo(expectedControlCoreSection), "ControlCoreSection mismatch");
            });
        }

        [Test]
        [TestCase("0x6x2x2.5")]
        [TestCase("-1x6x2x2.5")]
        public void Parse_InvalidTwistedPairBundleCount_ShouldThrow(string specStr)
        {
            Assert.Throws<ArgumentException>(() => _parser.Parse("RVSP", specStr));
        }

        [Test]
        public void Parse_InvalidSection_ShouldThrow()
        {
            Assert.Throws<InvalidSectionException>(() => _parser.Parse("YJV", "3xABC"));
        }

        [Test]
        [TestCase("BPYJV", "3x50+3x25", true, 1, 3, 50, 3, 25, Description = "变频标准格式")]
        [TestCase("BPYJV", "3*120+3*70", true, 1, 3, 120, 3, 70, Description = "变频星号分隔")]
        [TestCase("BPYJV", " 3 x 50 + 3 x 25 ", true, 1, 3, 50, 3, 25, Description = "变频带空格")]
        [TestCase("BPYJV", "2(3x50+3x25)", true, 2, 3, 50, 3, 25, Description = "变频多束")]
        [TestCase("BPYJV", "3x2.5+3x1.5", true, 1, 3, 2.5, 3, 1.5, Description = "变频小数")]
        public void Parse_VariableFrequency_ShouldSucceed(
            string model, string specStr,
            bool expectedIsVf, int expectedBundleCount,
            int expectedPhaseCount, double expectedPhaseSection,
            int expectedVfShieldCount, double expectedVfShieldSection)
        {
            var spec = _parser.Parse(model, specStr);
            Assert.Multiple(() =>
            {
                Assert.That(spec.IsVariableFrequency, Is.EqualTo(expectedIsVf), "IsVariableFrequency mismatch");
                Assert.That(spec.BundleCount, Is.EqualTo(expectedBundleCount), "BundleCount mismatch");
                Assert.That(spec.PhaseCoreCount, Is.EqualTo(expectedPhaseCount), "PhaseCoreCount mismatch");
                Assert.That(spec.PhaseCoreSection, Is.EqualTo(expectedPhaseSection), "PhaseCoreSection mismatch");
                Assert.That(spec.VfShieldCoreCount, Is.EqualTo(expectedVfShieldCount), "VfShieldCoreCount mismatch");
                Assert.That(spec.VfShieldCoreSection, Is.EqualTo(expectedVfShieldSection), "VfShieldCoreSection mismatch");
            });
        }

        [Test]
        [TestCase("RVSP", "6x2x2.5", true, 6, 2, 2.5, Description = "对绞标准格式")]
        [TestCase("RVSP", "6*2*2.5", true, 6, 2, 2.5, Description = "对绞星号")]
        [TestCase("RVSP", "6X2X2.5", true, 6, 2, 2.5, Description = "对绞大写X")]
        [TestCase("RVSP", "8x6x2x2.5", true, 6, 2, 2.5, Description = "对绞多束")] // BundleCount=8
        [TestCase("RVSP", "12x2x0.75", true, 12, 2, 0.75, Description = "对绞两位小数")]
        public void Parse_TwistedPair_Extended_ShouldSucceed(
            string model, string specStr,
            bool expectedIsTp,
            int expectedTwistedPairCount,
            int expectedCoresPerPair,
            double expectedSection)
        {
            var spec = _parser.Parse(model, specStr);
            Assert.Multiple(() =>
            {
                Assert.That(spec.IsTwistedPair, Is.EqualTo(expectedIsTp), "IsTwistedPair mismatch");
                Assert.That(spec.TwistedPairCount, Is.EqualTo(expectedTwistedPairCount), "TwistedPairCount mismatch");
                Assert.That(spec.CoresPerPair, Is.EqualTo(expectedCoresPerPair), "CoresPerPair mismatch");
                Assert.That(spec.ControlCoreSection, Is.EqualTo(expectedSection), "ControlCoreSection mismatch");
                if (specStr.StartsWith("8x")) // Bundle case check
                {
                    Assert.That(spec.BundleCount, Is.EqualTo(8), "BundleCount mismatch");
                }
            });
        }

        [Test]
        [TestCase("YJV", "3x50+1x25", false, false, Description = "普通动力不应识别为变频或对绞")]
        [TestCase("KVV", "4x2.5", false, false, Description = "普通控制不应识别为变频或对绞")]
        [TestCase("YJV", "3x50+2x25", false, false, Description = "非变频格式(3+2)")]
        public void Parse_NegativeCases_ShouldNotMisidentify(
            string model, string specStr,
            bool expectedIsVf, bool expectedIsTp)
        {
            var spec = _parser.Parse(model, specStr);
            Assert.Multiple(() =>
            {
                Assert.That(spec.IsVariableFrequency, Is.EqualTo(expectedIsVf), "False Positive VF");
                Assert.That(spec.IsTwistedPair, Is.EqualTo(expectedIsTp), "False Positive TP");
            });
        }

        [Test]
        [TestCase("RVSP", "3x3x2.5", Description = "非对绞格式(中间非2)")]
        public void Parse_InvalidFormat_ShouldThrow(string model, string specStr)
        {
            Assert.Throws<InvalidSectionException>(() => _parser.Parse(model, specStr));
        }

        [Test]
        [TestCase("YJV", "2(3x25+1x16)", 2, 3, 25, 1, 16, Description = "标准多并: 2束(3+1)")]
        [TestCase("YJV", "3(3x35+1x10)", 3, 3, 35, 1, 10, Description = "标准多并: 3束(3+1)")] // Changed from 3x6 (3+3) to 1x10 (3+1) to avoid VF trigger
        [TestCase("YJV", " 2 ( 3x25 + 1x16 ) ", 2, 3, 25, 1, 16, Description = "带空格多并")]
        [TestCase("YJV", "1(3x25)", 1, 3, 25, 0, 0, Description = "单束显式声明")]
        public void Parse_BundleSpec_ShouldSucceed(
            string model, string specStr,
            int expectedBundleCount,
            int expectedPhaseCount, double expectedPhaseSection,
            int expectedNeutralCount, double expectedNeutralSection)
        {
            var spec = _parser.Parse(model, specStr);
            Assert.Multiple(() =>
            {
                Assert.That(spec.BundleCount, Is.EqualTo(expectedBundleCount), "BundleCount mismatch");
                Assert.That(spec.PhaseCoreCount, Is.EqualTo(expectedPhaseCount), "PhaseCoreCount mismatch");
                Assert.That(spec.PhaseCoreSection, Is.EqualTo(expectedPhaseSection), "PhaseCoreSection mismatch");
                Assert.That(spec.NeutralCoreCount, Is.EqualTo(expectedNeutralCount), "NeutralCoreCount mismatch");
                Assert.That(spec.NeutralCoreSection, Is.EqualTo(expectedNeutralSection), "NeutralCoreSection mismatch");
            });
        }

        [Test]
        [TestCase("0(3x25)")]
        [TestCase("-1(3x25)")]
        public void Parse_InvalidBundleCount_ShouldThrow(string specStr)
        {
            Assert.Throws<ArgumentException>(() => _parser.Parse("YJV", specStr));
        }

        [Test]
        [TestCase("(3x25)")] // FormatException (no number)
        [TestCase("2(3x25")] // FormatException (missing paren)
        [TestCase(")3x25(")] // FormatException
        public void Parse_InvalidBundleFormat_ShouldThrow(string specStr)
        {
            Assert.Throws<FormatException>(() => _parser.Parse("YJV", specStr));
        }

        [Test]
        public void Parse_NullOrEmptySpec_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => _parser.Parse("YJV", null));
            Assert.Throws<ArgumentNullException>(() => _parser.Parse("YJV", "   "));
        }

        [Test]
        public void Parse_Performance_ShouldBeFast()
        {
            // Warmup
            _parser.Parse("YJV", "3x120+1x70");

            int count = 10000;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                _parser.Parse("YJV", "3x120+1x70");
            }
            sw.Stop();

            TestContext.Out.WriteLine($"Parsed {count} items in {sw.ElapsedMilliseconds} ms");
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(30000), "Should complete 10k parses within 30s");
            Assert.That(sw.ElapsedMilliseconds / (double)count, Is.LessThan(5), "Single parse should be < 5ms");
        }
    }
}
