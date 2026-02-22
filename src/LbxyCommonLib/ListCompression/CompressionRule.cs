// <copyright file="CompressionRule.cs" company="Lbxy">
// Copyright (c) 2026 Lbxy
// </copyright>
namespace LbxyCommonLib.ListCompression
{
    using System;
    using LbxyCommonLib.ListCompression.Interfaces;

    /// <summary>
    /// Encapsulates customization for list compression behavior.
    /// By default, <see cref="AdjacentOnly"/> is false so the default rule performs global compression,
    /// and the rule-less overloads of <see cref="ListCompressor{T}"/> use the same behavior.
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// When no custom equality or merge delegates are supplied, equality falls back to <see cref="object.Equals(object, object)"/>,
    /// and merge behavior prefers <see cref="ISummable{T}"/> when available.
    /// </remarks>
    /// <typeparam name="T">Element type.</typeparam>
    public sealed class CompressionRule<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionRule{T}"/> class.
        /// </summary>
        public CompressionRule()
        {
#if NETSTANDARD2_0
#pragma warning disable CS8618
            this.AreEqual = null!;
            this.Merge = null!;
            this.SumSelector = null!;
            this.UpdateSum = null!;
#pragma warning restore CS8618
#endif
        }

        /// <summary>
        /// Gets or sets an optional equality predicate. If not provided, <see cref="object.Equals(object, object)"/> is used.
        /// </summary>
        public Func<T, T, bool> AreEqual { get; set; }

        /// <summary>
        /// Gets or sets an optional merge function to combine two equal elements into a single one.
        /// If not provided and <typeparamref name="T"/> implements <see cref="ISummable{T}"/>,
        /// a default summation-based merge is applied.
        /// </summary>
        public Func<T, T, T> Merge { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only adjacent equal elements are compressed (run-length compression).
        /// When false, compression occurs globally by key while preserving first-seen order. Default is false.
        /// </summary>
        public bool AdjacentOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets an optional selector for the summable value when <typeparamref name="T"/> does not implement <see cref="ISummable{T}"/>.
        /// </summary>
        public Func<T, double> SumSelector { get; set; }

        /// <summary>
        /// Gets or sets an optional factory to update an element's summable value when <typeparamref name="T"/> does not implement <see cref="ISummable{T}"/>.
        /// </summary>
        public Func<T, double, T> UpdateSum { get; set; }
    }
}
