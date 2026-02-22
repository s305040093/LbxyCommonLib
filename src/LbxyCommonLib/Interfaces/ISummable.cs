// <copyright file="ISummable.cs" company="Lbxy">
// Copyright (c) 2026 Lbxy
// </copyright>
namespace LbxyCommonLib.ListCompression.Interfaces
{
    using System;

    /// <summary>
    /// Extends <see cref="ICompressible{T}"/> with a contract for summation of a numeric property.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public interface ISummable<T> : ICompressible<T>
    {
        /// <summary>
        /// Gets the numeric value that participates in summation when elements are compressed.
        /// </summary>
        /// <returns>The value to be summed as <see cref="double"/>.</returns>
        double GetSummableValue();

        /// <summary>
        /// Returns a new element instance with the summable value updated to <paramref name="newValue"/>.
        /// Implementations must not mutate the current instance.
        /// </summary>
        /// <param name="newValue">The new summed value.</param>
        /// <returns>A new element with updated summable value.</returns>
        T WithUpdatedSummableValue(double newValue);
    }
}
