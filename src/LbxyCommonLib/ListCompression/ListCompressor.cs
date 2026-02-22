// <copyright file="ListCompressor.cs" company="Lbxy">
// Copyright (c) 2026 Lbxy
// </copyright>
namespace LbxyCommonLib.ListCompression
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using LbxyCommonLib.ListCompression.Interfaces;

    /// <summary>
    /// Provides high-performance, extensible list compression operations.
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// The static methods in this class are thread-safe as they do not mutate shared state and only operate on provided inputs.
    /// </remarks>
    /// <typeparam name="T">Element type.</typeparam>
    public static class ListCompressor<T>
    {
        /// <summary>
        /// Compresses the input list using the default compression rule configured for global compression.
        /// Runs in O(n) time and produces a new list without mutating <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The input list to compress.</param>
        /// <returns>A newly created compressed list.</returns>
        public static List<T> Compress(IReadOnlyList<T> input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var rule = new CompressionRule<T>();
            return Compress(input, rule);
        }

        /// <summary>
        /// Compresses the input list according to the provided <paramref name="rule"/>.
        /// </summary>
        /// <param name="input">The input list to compress.</param>
        /// <param name="rule">The compression rule describing equality and merge behavior.</param>
        /// <returns>A newly created compressed list.</returns>
        public static List<T> Compress(IReadOnlyList<T> input, CompressionRule<T> rule)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            return rule.AdjacentOnly ? CompressAdjacent(input, rule) : CompressGlobal(input, rule);
        }

        /// <summary>
        /// Asynchronously compresses the input list according to <paramref name="rule"/>.
        /// </summary>
        /// <param name="input">The input list to compress.</param>
        /// <param name="rule">Compression behavior.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task yielding a newly created compressed list.</returns>
        public static Task<List<T>> CompressAsync(
            IReadOnlyList<T> input,
            CompressionRule<T> rule,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Use Task.Run to maintain compatibility with net45.
            return Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return Compress(input, rule);
                },
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously compresses the input list with the default compression rule configured for global compression.
        /// </summary>
        /// <param name="input">The input list to compress.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task yielding a newly created compressed list.</returns>
        public static Task<List<T>> CompressAsync(
            IReadOnlyList<T> input,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return CompressAsync(input, new CompressionRule<T>(), cancellationToken);
        }

        private static List<T> CompressAdjacent(IReadOnlyList<T> input, CompressionRule<T> rule)
        {
            var result = new List<T>(input.Count);
            if (input.Count == 0)
            {
                return result;
            }

            T current = input[0];
            for (int i = 1; i < input.Count; i++)
            {
                var next = input[i];
                if (AreEqual(current, next, rule))
                {
                    current = Merge(current, next, rule);
                }
                else
                {
                    result.Add(current);
                    current = next;
                }
            }

            result.Add(current);
            return result;
        }

        private static List<T> CompressGlobal(IReadOnlyList<T> input, CompressionRule<T> rule)
        {
            var result = new List<T>(input.Count);
            var indexByKey = new Dictionary<CompressionKey<T>, int>(input.Count);
            for (int i = 0; i < input.Count; i++)
            {
                var item = input[i];
                var key = new CompressionKey<T>(item, rule.AreEqual);
                int existingIndex;
                if (indexByKey.TryGetValue(key, out existingIndex))
                {
                    var merged = Merge(result[existingIndex], item, rule);
                    result[existingIndex] = merged;
                }
                else
                {
                    indexByKey[key] = result.Count;
                    result.Add(item);
                }
            }

            return result;
        }

        private static bool AreEqual(T a, T b, CompressionRule<T> rule)
        {
            if (rule.AreEqual != null)
            {
                return rule.AreEqual(a, b);
            }

            return object.Equals(a, b);
        }

        private static T Merge(T a, T b, CompressionRule<T> rule)
        {
            if (rule.Merge != null)
            {
                return rule.Merge(a, b);
            }

            var summableA = a as ISummable<T>;
            var summableB = b as ISummable<T>;
            if (summableA != null && summableB != null)
            {
                double sum = summableA.GetSummableValue() + summableB.GetSummableValue();
                return summableA.WithUpdatedSummableValue(sum);
            }

            if (rule.SumSelector != null && rule.UpdateSum != null)
            {
                double sum = rule.SumSelector(a) + rule.SumSelector(b);
                return rule.UpdateSum(a, sum);
            }

            return a;
        }

        private struct CompressionKey<TElement>
        {
            private readonly TElement element;
            private readonly Func<TElement, TElement, bool> equality;

            public CompressionKey(TElement element, Func<TElement, TElement, bool> equality)
            {
                this.element = element;
                this.equality = equality;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CompressionKey<TElement>))
                {
                    return false;
                }

                var other = (CompressionKey<TElement>)obj;

                if (this.equality != null)
                {
                    return this.equality(this.element, other.element);
                }

                return object.Equals(this.element, other.element);
            }

            public override int GetHashCode()
            {
                // Use default object's hash code to keep O(1) expected behavior, even when a custom equality is supplied.
                return this.element == null ? 0 : this.element.GetHashCode();
            }
        }
    }
}
