// <copyright file="NumericalEqualityUtil.cs" company="Lbxy">
// Copyright (c) 2026 Lbxy
// </copyright>
#pragma warning disable SA1402 // File may only contain a single type

namespace LbxyCommonLib.Numerics
{
    using System;

    /// <summary>
    /// Provides numerical equality helpers for double, float and decimal types.
    /// </summary>
    public static class NumericalEqualityUtil
    {
        private const double DefaultDoubleEpsilon = 1e-9;

        private const float DefaultFloatEpsilon = 1e-6f;

        private const decimal DefaultDecimalEpsilon = 1e-12m;

        /// <summary>
        /// Determines whether two <see cref="double"/> values are equal within the default epsilon of 1e-9.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(double value1, double value2)
        {
            return AreEqual(value1, value2, DefaultDoubleEpsilon);
        }

        /// <summary>
        /// Determines whether two <see cref="double"/> values are equal within the specified epsilon.
        /// NaN values are equal only to NaN, and infinities are equal only if both are the same infinity.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="epsilon">The maximum allowed absolute difference. When less than or equal to zero, exact equality is used.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(double value1, double value2, double epsilon)
        {
            if (double.IsNaN(value1) || double.IsNaN(value2))
            {
                return double.IsNaN(value1) && double.IsNaN(value2);
            }

            if (double.IsInfinity(value1) || double.IsInfinity(value2))
            {
                if (double.IsInfinity(value1) && double.IsInfinity(value2))
                {
                    return value1.Equals(value2);
                }

                return false;
            }

            if (epsilon <= 0d)
            {
                return value1.Equals(value2);
            }

            return Math.Abs(value1 - value2) <= epsilon;
        }

        /// <summary>
        /// Determines whether two nullable <see cref="double"/> values are equal within the default epsilon of 1e-9.
        /// Two null values are considered equal.
        /// </summary>
        /// <param name="value1">The first nullable value.</param>
        /// <param name="value2">The second nullable value.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(double? value1, double? value2)
        {
            return AreEqual(value1, value2, DefaultDoubleEpsilon);
        }

        /// <summary>
        /// Determines whether two nullable <see cref="double"/> values are equal within the specified epsilon.
        /// Two null values are considered equal.
        /// </summary>
        /// <param name="value1">The first nullable value.</param>
        /// <param name="value2">The second nullable value.</param>
        /// <param name="epsilon">The maximum allowed absolute difference. When less than or equal to zero, exact equality is used for non-null values.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(double? value1, double? value2, double epsilon)
        {
            if (!value1.HasValue || !value2.HasValue)
            {
                return value1.HasValue == value2.HasValue;
            }

            return AreEqual(value1.Value, value2.Value, epsilon);
        }

        /// <summary>
        /// Determines whether two <see cref="float"/> values are equal within the default epsilon of 1e-6.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(float value1, float value2)
        {
            return AreEqual(value1, value2, DefaultFloatEpsilon);
        }

        /// <summary>
        /// Determines whether two <see cref="float"/> values are equal within the specified epsilon.
        /// NaN values are equal only to NaN, and infinities are equal only if both are the same infinity.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="epsilon">The maximum allowed absolute difference. When less than or equal to zero, exact equality is used.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(float value1, float value2, float epsilon)
        {
            if (float.IsNaN(value1) || float.IsNaN(value2))
            {
                return float.IsNaN(value1) && float.IsNaN(value2);
            }

            if (float.IsInfinity(value1) || float.IsInfinity(value2))
            {
                if (float.IsInfinity(value1) && float.IsInfinity(value2))
                {
                    return value1.Equals(value2);
                }

                return false;
            }

            if (epsilon <= 0f)
            {
                return value1.Equals(value2);
            }

            return Math.Abs(value1 - value2) <= epsilon;
        }

        /// <summary>
        /// Determines whether two nullable <see cref="float"/> values are equal within the default epsilon of 1e-6.
        /// Two null values are considered equal.
        /// </summary>
        /// <param name="value1">The first nullable value.</param>
        /// <param name="value2">The second nullable value.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(float? value1, float? value2)
        {
            return AreEqual(value1, value2, DefaultFloatEpsilon);
        }

        /// <summary>
        /// Determines whether two nullable <see cref="float"/> values are equal within the specified epsilon.
        /// Two null values are considered equal.
        /// </summary>
        /// <param name="value1">The first nullable value.</param>
        /// <param name="value2">The second nullable value.</param>
        /// <param name="epsilon">The maximum allowed absolute difference. When less than or equal to zero, exact equality is used for non-null values.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(float? value1, float? value2, float epsilon)
        {
            if (!value1.HasValue || !value2.HasValue)
            {
                return value1.HasValue == value2.HasValue;
            }

            return AreEqual(value1.Value, value2.Value, epsilon);
        }

        /// <summary>
        /// Determines whether two <see cref="decimal"/> values are equal within the default epsilon of 1e-12.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(decimal value1, decimal value2)
        {
            return AreEqual(value1, value2, DefaultDecimalEpsilon);
        }

        /// <summary>
        /// Determines whether two <see cref="decimal"/> values are equal within the specified epsilon.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="epsilon">The maximum allowed absolute difference. When less than or equal to zero, exact equality is used.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(decimal value1, decimal value2, decimal epsilon)
        {
            if (epsilon <= 0m)
            {
                return value1.Equals(value2);
            }

            return Math.Abs(value1 - value2) <= epsilon;
        }

        /// <summary>
        /// Determines whether two nullable <see cref="decimal"/> values are equal within the default epsilon of 1e-12.
        /// Two null values are considered equal.
        /// </summary>
        /// <param name="value1">The first nullable value.</param>
        /// <param name="value2">The second nullable value.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(decimal? value1, decimal? value2)
        {
            return AreEqual(value1, value2, DefaultDecimalEpsilon);
        }

        /// <summary>
        /// Determines whether two nullable <see cref="decimal"/> values are equal within the specified epsilon.
        /// Two null values are considered equal.
        /// </summary>
        /// <param name="value1">The first nullable value.</param>
        /// <param name="value2">The second nullable value.</param>
        /// <param name="epsilon">The maximum allowed absolute difference. When less than or equal to zero, exact equality is used for non-null values.</param>
        /// <returns><c>true</c> if the two values are considered equal; otherwise, <c>false</c>.</returns>
        public static bool AreEqual(decimal? value1, decimal? value2, decimal epsilon)
        {
            if (!value1.HasValue || !value2.HasValue)
            {
                return value1.HasValue == value2.HasValue;
            }

            return AreEqual(value1.Value, value2.Value, epsilon);
        }
    }

    /// <summary>
    /// Provides extension methods for more fluent numerical equality checks.
    /// </summary>
    public static class NumericalEqualityExtensions
    {
        /// <summary>
        /// Determines whether the current <see cref="double"/> value is within the specified absolute tolerance of the target value.
        /// </summary>
        /// <param name="value">The current value (left-hand side of the comparison).</param>
        /// <param name="target">The target value (right-hand side of the comparison).</param>
        /// <param name="tolerance">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns><c>true</c> if <c>|value - target| &lt;= tolerance</c>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is less than zero.</exception>
        /// <remarks>
        /// <para>Special floating-point values follow IEEE-754 semantics:</para>
        /// <list type="bullet">
        /// <item><description>If either operand is <see cref="double.NaN"/>, the method returns <c>false</c>.</description></item>
        /// <item><description>Positive and negative infinity are equal only when both operands are the same infinity.</description></item>
        /// </list>
        /// <para>Example:</para>
        /// <code>
        /// using LbxyCommonLib.Numerics;
        ///
        /// double actual = 1.000000001;
        /// double expected = 1.0;
        /// bool equal = actual.WithinTolerance(expected, 1e-8);
        /// </code>
        /// </remarks>
        public static bool WithinTolerance(this double value, double target, double tolerance)
        {
            if (tolerance < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be greater than or equal to zero.");
            }

            if (double.IsNaN(value) || double.IsNaN(target))
            {
                return false;
            }

            if (double.IsInfinity(value) || double.IsInfinity(target))
            {
                return value.Equals(target);
            }

            return Math.Abs(value - target) <= tolerance;
        }

        /// <summary>
        /// Determines whether the current <see cref="double"/> value is equal to the specified target value within a relative error bound.
        /// </summary>
        /// <param name="value">The current value (left-hand side of the comparison).</param>
        /// <param name="target">The target value (right-hand side of the comparison).</param>
        /// <param name="relativeError">The maximum allowed relative error, expressed as a fraction. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if the absolute difference is less than or equal to <c>relativeError * max(|value|, |target|)</c>;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="relativeError"/> is less than zero.</exception>
        /// <remarks>
        /// <para>Special floating-point values follow IEEE-754 semantics:</para>
        /// <list type="bullet">
        /// <item><description>If either operand is <see cref="double.NaN"/>, the method returns <c>false</c>.</description></item>
        /// <item><description>Positive and negative infinity are equal only when both operands are the same infinity.</description></item>
        /// </list>
        /// <para>When both values are zero, they are considered equal for any non-negative relative error.</para>
        /// <para>Example:</para>
        /// <code>
        /// using LbxyCommonLib.Numerics;
        ///
        /// double baseline = 100.0;
        /// double candidate = 100.0000001;
        /// bool equal = candidate.EqualsRelatively(baseline, 1e-6);
        /// </code>
        /// </remarks>
        public static bool EqualsRelatively(this double value, double target, double relativeError = 1e-9)
        {
            if (relativeError < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(relativeError), "Relative error must be greater than or equal to zero.");
            }

            if (double.IsNaN(value) || double.IsNaN(target))
            {
                return false;
            }

            if (double.IsInfinity(value) || double.IsInfinity(target))
            {
                return value.Equals(target);
            }

            if (value.Equals(target))
            {
                return true;
            }

            var diff = Math.Abs(value - target);

            if (relativeError == 0d)
            {
                return diff == 0d;
            }

            var scale = Math.Max(Math.Abs(value), Math.Abs(target));
            if (scale == 0d)
            {
                return diff == 0d;
            }

            return diff <= relativeError * scale;
        }

        /// <summary>
        /// Determines whether the current <see cref="double"/> value is equal to the specified target value within an absolute error bound.
        /// </summary>
        /// <param name="value">The current value (left-hand side of the comparison).</param>
        /// <param name="target">The target value (right-hand side of the comparison).</param>
        /// <param name="absoluteError">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns><c>true</c> if <c>|value - target| &lt;= absoluteError</c>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="absoluteError"/> is less than zero.</exception>
        /// <remarks>
        /// <para>This method is similar to <see cref="WithinTolerance(double,double,double)"/> but uses a default absolute error of 1e-12.</para>
        /// </remarks>
        public static bool EqualsAbsolutely(this double value, double target, double absoluteError = 1e-12)
        {
            return WithinTolerance(value, target, absoluteError);
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="double"/> value is within the specified absolute tolerance of the target value.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="tolerance">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and within the specified tolerance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is less than zero.</exception>
        public static bool WithinTolerance(this double? value, double? target, double tolerance)
        {
            if (tolerance < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return value.Value.WithinTolerance(target.Value, tolerance);
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="double"/> value is equal to the specified target value within a relative error bound.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="relativeError">The maximum allowed relative error, expressed as a fraction. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and satisfy the relative error constraint; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="relativeError"/> is less than zero.</exception>
        public static bool EqualsRelatively(this double? value, double? target, double relativeError = 1e-9)
        {
            if (relativeError < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(relativeError), "Relative error must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return value.Value.EqualsRelatively(target.Value, relativeError);
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="double"/> value is equal to the specified target value within an absolute error bound.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="absoluteError">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and satisfy the absolute error constraint; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="absoluteError"/> is less than zero.</exception>
        public static bool EqualsAbsolutely(this double? value, double? target, double absoluteError = 1e-12)
        {
            if (absoluteError < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(absoluteError), "Absolute error must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return value.Value.EqualsAbsolutely(target.Value, absoluteError);
        }

        /// <summary>
        /// Determines whether the current <see cref="float"/> value is within the specified absolute tolerance of the target value.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="tolerance">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns><c>true</c> if <c>|value - target| &lt;= tolerance</c>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is less than zero.</exception>
        public static bool WithinTolerance(this float value, float target, double tolerance)
        {
            return ((double)value).WithinTolerance(target, tolerance);
        }

        /// <summary>
        /// Determines whether the current <see cref="float"/> value is equal to the specified target value within a relative error bound.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="relativeError">The maximum allowed relative error, expressed as a fraction. Must be greater than or equal to zero.</param>
        /// <returns><c>true</c> if the relative error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="relativeError"/> is less than zero.</exception>
        public static bool EqualsRelatively(this float value, float target, double relativeError = 1e-9)
        {
            return ((double)value).EqualsRelatively(target, relativeError);
        }

        /// <summary>
        /// Determines whether the current <see cref="float"/> value is equal to the specified target value within an absolute error bound.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="absoluteError">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns><c>true</c> if the absolute error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="absoluteError"/> is less than zero.</exception>
        public static bool EqualsAbsolutely(this float value, float target, double absoluteError = 1e-12)
        {
            return ((double)value).EqualsAbsolutely(target, absoluteError);
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="float"/> value is within the specified absolute tolerance of the target value.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="tolerance">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and within the specified tolerance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is less than zero.</exception>
        public static bool WithinTolerance(this float? value, float? target, double tolerance)
        {
            if (tolerance < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return ((double)value.Value).WithinTolerance(target.Value, tolerance);
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="float"/> value is equal to the specified target value within a relative error bound.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="relativeError">The maximum allowed relative error, expressed as a fraction. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and satisfy the relative error constraint; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="relativeError"/> is less than zero.</exception>
        public static bool EqualsRelatively(this float? value, float? target, double relativeError = 1e-9)
        {
            if (relativeError < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(relativeError), "Relative error must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return ((double)value.Value).EqualsRelatively(target.Value, relativeError);
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="float"/> value is equal to the specified target value within an absolute error bound.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="absoluteError">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and satisfy the absolute error constraint; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="absoluteError"/> is less than zero.</exception>
        public static bool EqualsAbsolutely(this float? value, float? target, double absoluteError = 1e-12)
        {
            if (absoluteError < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(absoluteError), "Absolute error must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return ((double)value.Value).EqualsAbsolutely(target.Value, absoluteError);
        }

        /// <summary>
        /// Determines whether the current <see cref="decimal"/> value is within the specified absolute tolerance of the target value.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="tolerance">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns><c>true</c> if <c>|value - target| &lt;= tolerance</c>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is less than zero.</exception>
        public static bool WithinTolerance(this decimal value, decimal target, decimal tolerance)
        {
            if (tolerance < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be greater than or equal to zero.");
            }

            return Math.Abs(value - target) <= tolerance;
        }

        /// <summary>
        /// Determines whether the current <see cref="decimal"/> value is equal to the specified target value within a relative error bound.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="relativeError">The maximum allowed relative error, expressed as a fraction. Must be greater than or equal to zero.</param>
        /// <returns><c>true</c> if the relative error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="relativeError"/> is less than zero.</exception>
        public static bool EqualsRelatively(this decimal value, decimal target, decimal relativeError = 1e-9m)
        {
            if (relativeError < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(relativeError), "Relative error must be greater than or equal to zero.");
            }

            if (value.Equals(target))
            {
                return true;
            }

            var diff = Math.Abs(value - target);

            if (relativeError == 0m)
            {
                return diff == 0m;
            }

            var scale = Math.Max(Math.Abs(value), Math.Abs(target));
            if (scale == 0m)
            {
                return diff == 0m;
            }

            return diff <= relativeError * scale;
        }

        /// <summary>
        /// Determines whether the current <see cref="decimal"/> value is equal to the specified target value within an absolute error bound.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="absoluteError">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns><c>true</c> if the absolute error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="absoluteError"/> is less than zero.</exception>
        public static bool EqualsAbsolutely(this decimal value, decimal target, decimal absoluteError = 1e-12m)
        {
            if (absoluteError < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(absoluteError), "Absolute error must be greater than or equal to zero.");
            }

            return Math.Abs(value - target) <= absoluteError;
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="decimal"/> value is within the specified absolute tolerance of the target value.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="tolerance">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and within the specified tolerance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is less than zero.</exception>
        public static bool WithinTolerance(this decimal? value, decimal? target, decimal tolerance)
        {
            if (tolerance < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return value.Value.WithinTolerance(target.Value, tolerance);
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="decimal"/> value is equal to the specified target value within a relative error bound.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="relativeError">The maximum allowed relative error, expressed as a fraction. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and satisfy the relative error constraint; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="relativeError"/> is less than zero.</exception>
        public static bool EqualsRelatively(this decimal? value, decimal? target, decimal relativeError = 1e-9m)
        {
            if (relativeError < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(relativeError), "Relative error must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return value.Value.EqualsRelatively(target.Value, relativeError);
        }

        /// <summary>
        /// Determines whether the current nullable <see cref="decimal"/> value is equal to the specified target value within an absolute error bound.
        /// </summary>
        /// <param name="value">The current nullable value.</param>
        /// <param name="target">The target nullable value.</param>
        /// <param name="absoluteError">The maximum allowed absolute difference. Must be greater than or equal to zero.</param>
        /// <returns>
        /// <c>true</c> if both values are <c>null</c>, or both are non-null and satisfy the absolute error constraint; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="absoluteError"/> is less than zero.</exception>
        public static bool EqualsAbsolutely(this decimal? value, decimal? target, decimal absoluteError = 1e-12m)
        {
            if (absoluteError < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(absoluteError), "Absolute error must be greater than or equal to zero.");
            }

            if (!value.HasValue || !target.HasValue)
            {
                return value.HasValue == target.HasValue;
            }

            return value.Value.EqualsAbsolutely(target.Value, absoluteError);
        }

        /// <summary>
        /// Determines whether the current <see cref="int"/> value is within the specified absolute tolerance of the target value.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="tolerance">The maximum allowed absolute difference, interpreted in double precision.</param>
        /// <returns><c>true</c> if the absolute error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is less than zero.</exception>
        public static bool WithinTolerance(this int value, int target, double tolerance)
        {
            return ((double)value).WithinTolerance(target, tolerance);
        }

        /// <summary>
        /// Determines whether the current <see cref="int"/> value is equal to the specified target value within a relative error bound.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="relativeError">The maximum allowed relative error, expressed as a fraction.</param>
        /// <returns><c>true</c> if the relative error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="relativeError"/> is less than zero.</exception>
        public static bool EqualsRelatively(this int value, int target, double relativeError = 1e-9)
        {
            return ((double)value).EqualsRelatively(target, relativeError);
        }

        /// <summary>
        /// Determines whether the current <see cref="int"/> value is equal to the specified target value within an absolute error bound.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="absoluteError">The maximum allowed absolute difference.</param>
        /// <returns><c>true</c> if the absolute error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="absoluteError"/> is less than zero.</exception>
        public static bool EqualsAbsolutely(this int value, int target, double absoluteError = 1e-12)
        {
            return ((double)value).EqualsAbsolutely(target, absoluteError);
        }

        /// <summary>
        /// Determines whether the current <see cref="long"/> value is within the specified absolute tolerance of the target value.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="tolerance">The maximum allowed absolute difference, interpreted in double precision.</param>
        /// <returns><c>true</c> if the absolute error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is less than zero.</exception>
        public static bool WithinTolerance(this long value, long target, double tolerance)
        {
            return ((double)value).WithinTolerance(target, tolerance);
        }

        /// <summary>
        /// Determines whether the current <see cref="long"/> value is equal to the specified target value within a relative error bound.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="relativeError">The maximum allowed relative error, expressed as a fraction.</param>
        /// <returns><c>true</c> if the relative error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="relativeError"/> is less than zero.</exception>
        public static bool EqualsRelatively(this long value, long target, double relativeError = 1e-9)
        {
            return ((double)value).EqualsRelatively(target, relativeError);
        }

        /// <summary>
        /// Determines whether the current <see cref="long"/> value is equal to the specified target value within an absolute error bound.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="absoluteError">The maximum allowed absolute difference.</param>
        /// <returns><c>true</c> if the absolute error constraint is satisfied; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="absoluteError"/> is less than zero.</exception>
        public static bool EqualsAbsolutely(this long value, long target, double absoluteError = 1e-12)
        {
            return ((double)value).EqualsAbsolutely(target, absoluteError);
        }
    }
}

#pragma warning restore SA1402 // File may only contain a single type
