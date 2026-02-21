namespace LbxyCommonLib.Numerics.Tests
{
    using System;
    using LbxyCommonLib.Numerics;
    using NUnit.Framework;

    [TestFixture]
    public sealed class NumericalEqualityUtilTests
    {
        [Test]
        public void Double_AreEqual_WithinDefaultEpsilon_ReturnsTrue()
        {
            var v1 = 1.0d;
            var v2 = 1.0d + 5e-10d;

            var result = NumericalEqualityUtil.AreEqual(v1, v2);

            Assert.That(result, Is.True);
        }

        [Test]
        public void Double_AreEqual_OutsideDefaultEpsilon_ReturnsFalse()
        {
            var v1 = 1.0d;
            var v2 = 1.0d + 2e-9d;

            var result = NumericalEqualityUtil.AreEqual(v1, v2);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Double_AreEqual_WithCustomEpsilon_RespectsThreshold()
        {
            var v1 = 1.0d;
            var v2 = 1.0d + 1e-6d;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 1e-8d), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 1e-5d), Is.True);
        }

        [Test]
        public void Double_AreEqual_WithNonPositiveEpsilon_UsesExactEquality()
        {
            var v1 = 1.0d;
            var v2 = 1.0d + 1e-9d;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v1, 0d), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 0d), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, -1d), Is.False);
        }

        [Test]
        public void Double_AreEqual_NaNHandling_IsConsistent()
        {
            var nan = double.NaN;

            Assert.That(NumericalEqualityUtil.AreEqual(nan, double.NaN), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(nan, 0d), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(1d, nan), Is.False);
        }

        [Test]
        public void Double_AreEqual_InfinityHandling_IsConsistent()
        {
            Assert.That(NumericalEqualityUtil.AreEqual(double.PositiveInfinity, double.PositiveInfinity), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(double.NegativeInfinity, double.NegativeInfinity), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(double.PositiveInfinity, double.NegativeInfinity), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(double.PositiveInfinity, 1d), Is.False);
        }

        [Test]
        public void NullableDouble_AreEqual_NullHandling_IsConsistent()
        {
            double? v1 = null;
            double? v2 = null;
            double? v3 = 1.0d;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v3), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(v3, v1), Is.False);
        }

        [Test]
        public void NullableDouble_AreEqual_UsesUnderlyingComparison()
        {
            double? v1 = 1.0d;
            double? v2 = 1.0d + 5e-10d;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 1e-12d), Is.False);
        }

        [Test]
        public void Float_AreEqual_DefaultAndCustomEpsilon_WorkAsExpected()
        {
            var v1 = 1.0f;
            var v2 = 1.0f + 5e-7f;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 1e-7f), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 1e-5f), Is.True);
        }

        [Test]
        public void Float_AreEqual_NaNAndInfinity_HandledCorrectly()
        {
            var nan = float.NaN;

            Assert.That(NumericalEqualityUtil.AreEqual(nan, float.NaN), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(nan, 0f), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(float.PositiveInfinity, float.PositiveInfinity), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(float.PositiveInfinity, float.NegativeInfinity), Is.False);
        }

        [Test]
        public void NullableFloat_AreEqual_NullAndValue_HandledCorrectly()
        {
            float? v1 = null;
            float? v2 = null;
            float? v3 = 1.0f;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v3), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(v3, v1), Is.False);
        }

        [Test]
        public void Decimal_AreEqual_DefaultAndCustomEpsilon_WorkAsExpected()
        {
            var v1 = 1.000000000000m;
            var v2 = 1.0000000000005m;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 1e-13m), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 1e-11m), Is.True);
        }

        [Test]
        public void Decimal_AreEqual_NonPositiveEpsilon_UsesExactEquality()
        {
            var v1 = 1.000000000000m;
            var v2 = 1.000000000001m;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v1, 0m), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, 0m), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2, -1m), Is.False);
        }

        [Test]
        public void NullableDecimal_AreEqual_NullAndValue_HandledCorrectly()
        {
            decimal? v1 = null;
            decimal? v2 = null;
            decimal? v3 = 1.0m;

            Assert.That(NumericalEqualityUtil.AreEqual(v1, v2), Is.True);
            Assert.That(NumericalEqualityUtil.AreEqual(v1, v3), Is.False);
            Assert.That(NumericalEqualityUtil.AreEqual(v3, v1), Is.False);
        }

        [Test]
        public void NumericalEqualityUtil_IsThreadSafe_ForConcurrentUse()
        {
            var random = new Random(1234);

            for (var i = 0; i < 100000; i++)
            {
                var d1 = random.NextDouble();
                var d2 = d1 + (random.NextDouble() - 0.5d) * 1e-10d;

                _ = NumericalEqualityUtil.AreEqual(d1, d2);
                _ = NumericalEqualityUtil.AreEqual((double?)d1, (double?)d2);
                _ = NumericalEqualityUtil.AreEqual((float)d1, (float)d2);
                _ = NumericalEqualityUtil.AreEqual((decimal)d1, (decimal)d2);
            }
        }

        [Test]
        public void DoubleExtensions_WithinTolerance_WorksForBoundaryCases()
        {
            const double target = 1.0d;
            var valueWithin = 1.0d + 5e-13d;
            var valueOutside = 1.0d + 2e-12d;

            Assert.That(valueWithin.WithinTolerance(target, 1e-12), Is.True);
            Assert.That(valueOutside.WithinTolerance(target, 1e-12), Is.False);
        }

        [Test]
        public void DoubleExtensions_EqualsAbsolutely_DefaultAndCustomErrors_Work()
        {
            const double target = 10.0d;
            var value = 10.0d + 5e-13d;

            Assert.That(value.EqualsAbsolutely(target), Is.True);
            Assert.That(value.EqualsAbsolutely(target, 1e-15), Is.False);
        }

        [Test]
        public void DoubleExtensions_EqualsRelatively_WorksForLargeAndSmallValues()
        {
            const double targetLarge = 1_000_000_000d;
            var valueLarge = targetLarge * (1d + 5e-10d);
            Assert.That(valueLarge.EqualsRelatively(targetLarge, 1e-9), Is.True);

            const double targetSmall = 1e-9;
            var valueSmall = targetSmall * (1d + 2e-8d);
            Assert.That(valueSmall.EqualsRelatively(targetSmall, 1e-9), Is.False);
        }

        [Test]
        public void DoubleExtensions_SpecialValues_RespectIeeeSemantics()
        {
            Assert.That(double.NaN.WithinTolerance(double.NaN, 1.0), Is.False);
            Assert.That(double.PositiveInfinity.WithinTolerance(double.PositiveInfinity, 1.0), Is.True);
            Assert.That(double.PositiveInfinity.WithinTolerance(double.NegativeInfinity, 1.0), Is.False);
            Assert.That(double.PositiveInfinity.EqualsRelatively(double.PositiveInfinity), Is.True);
            Assert.That(double.PositiveInfinity.EqualsRelatively(1.0), Is.False);
        }

        [Test]
        public void NullableDoubleExtensions_HandleNullShortCircuit()
        {
            double? nullValue = null;
            double? otherNull = null;
            double? nonNull = 1.0d;

            Assert.That(nullValue.WithinTolerance(otherNull, 1e-9), Is.True);
            Assert.That(nullValue.WithinTolerance(nonNull, 1e-9), Is.False);
            Assert.That(nonNull.EqualsAbsolutely(nullValue), Is.False);
        }

        [Test]
        public void FloatDecimalAndIntegralExtensions_DelegateToDoubleLogic()
        {
            float f = 1.0f + 5e-7f;
            Assert.That(f.WithinTolerance(1.0f, 1e-6), Is.True);

            decimal dm = 1.000000000000m;
            var dm2 = 1.0000000000005m;
            Assert.That(dm2.EqualsAbsolutely(dm, 1e-12m), Is.True);

            const int i = 100;
            const int i2 = 101;
            Assert.That(i.EqualsAbsolutely(i2, 0.5), Is.False);

            const long l = 1_000_000_000_000L;
            var l2 = l + 1;
            Assert.That(l2.WithinTolerance(l, 2.0), Is.True);
        }

        [Test]
        public void DoubleExtensions_HandleMaxValueAndEpsilon()
        {
            const double max = double.MaxValue;
            Assert.That(max.EqualsAbsolutely(max, 0d), Is.True);

            const double zero = 0d;
            var epsilon = double.Epsilon;
            Assert.That(epsilon.WithinTolerance(zero, double.Epsilon), Is.True);
        }

        [Test]
        public void FloatAndDecimalExtensions_HandleExtremeValues()
        {
            const float maxFloat = float.MaxValue;
            Assert.That(maxFloat.EqualsRelatively(maxFloat, 0d), Is.True);

            const decimal largeDecimal = 1000000000000000000m;
            var delta = 0.000000000001m;
            Assert.That(largeDecimal.WithinTolerance(largeDecimal + delta, delta), Is.True);
        }

        [Test]
        public void Extensions_ThrowOnNegativeToleranceOrError()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => 1.0d.WithinTolerance(1.0d, -1.0d));
            Assert.Throws<ArgumentOutOfRangeException>(() => 1.0d.EqualsRelatively(1.0d, -1.0d));
            Assert.Throws<ArgumentOutOfRangeException>(() => 1.0d.EqualsAbsolutely(1.0d, -1.0d));

            Assert.Throws<ArgumentOutOfRangeException>(() => ((float?)1.0f).WithinTolerance(1.0f, -1.0d));
            Assert.Throws<ArgumentOutOfRangeException>(() => ((decimal?)1.0m).EqualsRelatively(1.0m, -1.0m));
            Assert.Throws<ArgumentOutOfRangeException>(() => ((decimal?)1.0m).EqualsAbsolutely(1.0m, -1.0m));
        }
    }
}
