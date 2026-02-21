namespace LbxyCommonLib.ListCompression.Tests.Models
{
    /// <summary>
    /// Sample type that uses custom Equals.Fody logic for tests.
    /// </summary>
    [Equals]
    public sealed class CustomEqualsSample
    {
        /// <summary>
        /// Gets or sets the primary value participating in equality.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets an auxiliary value that participates in custom equality.
        /// </summary>
        [IgnoreDuringEquals]
        public int Z { get; set; }

        /// <summary>
        /// Customizes equality behavior based on <see cref="Z"/>.
        /// </summary>
        /// <param name="other">The other instance to compare.</param>
        /// <returns><c>true</c> when the instances are considered equal.</returns>
        [CustomEqualsInternal]
        private bool CustomLogic(CustomEqualsSample other)
        {
            return Z == other.Z || Z == 0 || other.Z == 0;
        }

        /// <summary>
        /// Determines whether two <see cref="CustomEqualsSample"/> instances are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><c>true</c> if the two instances are considered equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(CustomEqualsSample left, CustomEqualsSample right) => Operator.Weave(left, right);

        /// <summary>
        /// Determines whether two <see cref="CustomEqualsSample"/> instances are not equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(CustomEqualsSample left, CustomEqualsSample right) => Operator.Weave(left, right);
    }
}
