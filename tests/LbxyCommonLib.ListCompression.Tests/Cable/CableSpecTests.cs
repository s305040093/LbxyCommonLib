using LbxyCommonLib.Cable;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests.Cable
{
    [TestFixture]
    public class CableSpecTests
    {
        [Test]
        public void CreatePowerCable_ShouldPopulatePropertiesCorrectly()
        {
            var spec = CableSpec.CreatePowerCable("YJV", 3, 70, 1, 35);

            Assert.That(spec.Model, Is.EqualTo("YJV"));
            Assert.That(spec.PhaseCoreCount, Is.EqualTo(3));
            Assert.That(spec.PhaseCoreSection, Is.EqualTo(70.0));
            Assert.That(spec.NeutralCoreCount, Is.EqualTo(1));
            Assert.That(spec.NeutralCoreSection, Is.EqualTo(35.0));
            Assert.That(spec.CableType, Is.EqualTo("电力电缆"));
        }

        [Test]
        public void CreateVfCable_ShouldPopulatePropertiesCorrectly()
        {
            var spec = CableSpec.CreateVfCable("BPYJV", 3, 35, 3, 6);

            Assert.That(spec.Model, Is.EqualTo("BPYJV"));
            Assert.That(spec.PhaseCoreCount, Is.EqualTo(3));
            Assert.That(spec.PhaseCoreSection, Is.EqualTo(35.0));
            Assert.That(spec.VfShieldCoreCount, Is.EqualTo(3));
            Assert.That(spec.VfShieldCoreSection, Is.EqualTo(6.0));
            Assert.That(spec.IsVariableFrequency, Is.True);
            Assert.That(spec.CableType, Is.EqualTo("变频电缆"));
        }

        [Test]
        public void CreateTwistedPairCable_ShouldPopulatePropertiesCorrectly()
        {
            var spec = CableSpec.CreateTwistedCable("DJYPV", 6, 2, 1.5);

            Assert.That(spec.Model, Is.EqualTo("DJYPV"));
            Assert.That(spec.TwistedPairCount, Is.EqualTo(6));
            Assert.That(spec.CoresPerPair, Is.EqualTo(2));
            Assert.That(spec.PhaseCoreSection, Is.EqualTo(1.5));
            Assert.That(spec.IsTwistedPair, Is.True);
        }

        [Test]
        public void GetSpecDesc_PowerCable_ShouldReturnCorrectString()
        {
            var spec = CableSpec.CreatePowerCable("YJV", 4, 70, 1, 35);
            Assert.That(spec.GetSpecDesc(), Is.EqualTo("4×70+1×35"));
        }

        [Test]
        public void GetSpecDesc_VfCable_ShouldReturnCorrectString()
        {
            var spec = CableSpec.CreateVfCable("BPYJV", 3, 35, 3, 6, 2);
            Assert.That(spec.GetSpecDesc(), Is.EqualTo("2(3×35+3×6)"));
        }

        [Test]
        public void GetSpecDesc_TwistedPairCable_ShouldReturnCorrectString()
        {
            var spec = CableSpec.CreateTwistedCable("DJYPV", 6, 2, 1.5);
            Assert.That(spec.GetSpecDesc(), Is.EqualTo("1×6×2×1.5"));
        }
    }
}
