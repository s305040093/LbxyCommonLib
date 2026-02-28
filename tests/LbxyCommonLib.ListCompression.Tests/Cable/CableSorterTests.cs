using System.Collections.Generic;
using System.Linq;
using LbxyCommonLib.Cable;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests.Cable
{
    [TestFixture]
    public class CableSorterTests
    {
        [Test]
        public void Sort_NullOrEmptyList_ShouldReturnEmpty()
        {
            var result1 = CableSorter.Sort(null);
            Assert.That(result1, Is.Empty);

            var result2 = CableSorter.Sort(new List<CableSpec>());
            Assert.That(result2, Is.Empty);
        }

        [Test]
        public void Sort_PowerCables_ShouldSortByPriorities()
        {
            var cables = new List<CableSpec>
            {
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10, NeutralCoreSection = 5, ProtectCoreCount = 1 },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 25, NeutralCoreSection = 10, ProtectCoreCount = 1 },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10, NeutralCoreSection = 10, ProtectCoreCount = 1 },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10, NeutralCoreSection = 5, ProtectCoreCount = 2 }
            };

            var sorted = CableSorter.Sort(cables);

            // Expected Order:
            // 1. PhaseCoreSection=25
            // 2. PhaseCoreSection=10, NeutralCoreSection=10
            // 3. PhaseCoreSection=10, NeutralCoreSection=5, ProtectCoreCount=2
            // 4. PhaseCoreSection=10, NeutralCoreSection=5, ProtectCoreCount=1

            Assert.Multiple(() =>
            {
                Assert.That(sorted[0].PhaseCoreSection, Is.EqualTo(25));
                Assert.That(sorted[1].PhaseCoreSection, Is.EqualTo(10));
                Assert.That(sorted[1].NeutralCoreSection, Is.EqualTo(10));
                Assert.That(sorted[2].PhaseCoreSection, Is.EqualTo(10));
                Assert.That(sorted[2].NeutralCoreSection, Is.EqualTo(5));
                Assert.That(sorted[2].ProtectCoreCount, Is.EqualTo(2));
                Assert.That(sorted[3].PhaseCoreSection, Is.EqualTo(10));
                Assert.That(sorted[3].NeutralCoreSection, Is.EqualTo(5));
                Assert.That(sorted[3].ProtectCoreCount, Is.EqualTo(1));
            });
        }

        [Test]
        public void Sort_ControlCables_ShouldSortByPriorities()
        {
            var cables = new List<CableSpec>
            {
                new CableSpec { Category = "控制电缆", ControlCoreSection = 1.5, ControlCoreCount = 4, TwistedPairCount = 0 },
                new CableSpec { Category = "控制电缆", ControlCoreSection = 2.5, ControlCoreCount = 4, TwistedPairCount = 0 },
                new CableSpec { Category = "控制电缆", ControlCoreSection = 1.5, ControlCoreCount = 10, TwistedPairCount = 0 },
                new CableSpec { Category = "控制电缆", ControlCoreSection = 1.5, ControlCoreCount = 4, TwistedPairCount = 2 }
            };

            var sorted = CableSorter.Sort(cables);

            // Expected Order:
            // 1. ControlCoreSection=2.5
            // 2. ControlCoreSection=1.5, ControlCoreCount=10
            // 3. ControlCoreSection=1.5, ControlCoreCount=4, TwistedPairCount=2
            // 4. ControlCoreSection=1.5, ControlCoreCount=4, TwistedPairCount=0

            Assert.Multiple(() =>
            {
                Assert.That(sorted[0].ControlCoreSection, Is.EqualTo(2.5));
                Assert.That(sorted[1].ControlCoreSection, Is.EqualTo(1.5));
                Assert.That(sorted[1].ControlCoreCount, Is.EqualTo(10));
                Assert.That(sorted[2].ControlCoreSection, Is.EqualTo(1.5));
                Assert.That(sorted[2].ControlCoreCount, Is.EqualTo(4));
                Assert.That(sorted[2].TwistedPairCount, Is.EqualTo(2));
                Assert.That(sorted[3].ControlCoreSection, Is.EqualTo(1.5));
                Assert.That(sorted[3].ControlCoreCount, Is.EqualTo(4));
                Assert.That(sorted[3].TwistedPairCount, Is.EqualTo(0));
            });
        }

        [Test]
        public void Sort_MixedCategories_ShouldGroupPowerFirst()
        {
            var cables = new List<CableSpec>
            {
                new CableSpec { Category = "控制电缆", ControlCoreSection = 1.5 },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10 },
                new CableSpec { Category = "未知类型", PhaseCoreSection = 50 }, // Should be last
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 25 }
            };

            var sorted = CableSorter.Sort(cables);

            Assert.Multiple(() =>
            {
                Assert.That(sorted[0].Category, Is.EqualTo("动力电缆"));
                Assert.That(sorted[0].PhaseCoreSection, Is.EqualTo(25));
                Assert.That(sorted[1].Category, Is.EqualTo("动力电缆"));
                Assert.That(sorted[1].PhaseCoreSection, Is.EqualTo(10));
                Assert.That(sorted[2].Category, Is.EqualTo("控制电缆"));
                Assert.That(sorted[3].Category, Is.EqualTo("未知类型"));
            });
        }

        [Test]
        public void Sort_ShouldBeStable()
        {
            var cables = new List<CableSpec>
            {
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10, Model = "A" },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10, Model = "B" },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10, Model = "C" }
            };

            var sorted = CableSorter.Sort(cables);

            Assert.Multiple(() =>
            {
                Assert.That(sorted[0].Model, Is.EqualTo("A"));
                Assert.That(sorted[1].Model, Is.EqualTo("B"));
                Assert.That(sorted[2].Model, Is.EqualTo("C"));
            });
        }
        [Test]
        public void Sort_Demo_PrintComparison()
        {
            var cables = new List<CableSpec>
            {
                new CableSpec { Category = "控制电缆", ControlCoreSection = 1.5, ControlCoreCount = 4, Model = "KVV-4x1.5" },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10, NeutralCoreSection = 10, ProtectCoreCount = 1, Model = "YJV-3x10+1x10+1x6" }, // 假设Protect是6mm2，这里只用Count
                new CableSpec { Category = "未知类型", PhaseCoreSection = 50, Model = "Unknown-50" },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 25, Model = "YJV-3x25" },
                new CableSpec { Category = "控制电缆", ControlCoreSection = 2.5, ControlCoreCount = 4, Model = "KVV-4x2.5" },
                new CableSpec { Category = "动力电缆", PhaseCoreSection = 10, NeutralCoreSection = 5, ProtectCoreCount = 1, Model = "YJV-3x10+1x6+1x6" }
            };

            System.Console.WriteLine("Before Sorting:");
            foreach (var c in cables)
            {
                System.Console.WriteLine($"[{c.Category}] {c.Model} (Phase:{c.PhaseCoreSection}, Neutral:{c.NeutralCoreSection}, Protect:{c.ProtectCoreCount}, ControlSec:{c.ControlCoreSection}, ControlCnt:{c.ControlCoreCount})");
            }

            var sorted = CableSorter.Sort(cables);

            System.Console.WriteLine("\nAfter Sorting:");
            foreach (var c in sorted)
            {
                System.Console.WriteLine($"[{c.Category}] {c.Model} (Phase:{c.PhaseCoreSection}, Neutral:{c.NeutralCoreSection}, Protect:{c.ProtectCoreCount}, ControlSec:{c.ControlCoreSection}, ControlCnt:{c.ControlCoreCount})");
            }

            // Basic assertions to ensure test passes
            Assert.That(sorted[0].Model, Is.EqualTo("YJV-3x25"));
            Assert.That(sorted.Last().Model, Is.EqualTo("Unknown-50"));
        }
    }
}
