using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using LbxyCommonLib.ListCompression;
using LbxyCommonLib.ListCompression.Tests.Models;

namespace LbxyCommonLib.ListCompression.Tests
{
    public class CompressorTests
    {
        [Test]
        public void Compress_EmptyList_ReturnsEmpty()
        {
            var input = new List<OrderItem>().AsReadOnly();
            var result = ListCompressor<OrderItem>.Compress(input);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Compress_SingleElement_ReturnsSameElement()
        {
            var input = new List<OrderItem> { new OrderItem("A", 1.0) }.AsReadOnly();
            var result = ListCompressor<OrderItem>.Compress(input);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Sku, Is.EqualTo("A"));
            Assert.That(result[0].Quantity, Is.EqualTo(1.0).Within(1e-9));
        }

        [Test]
        public void Compress_AdjacentEquals_SumsQuantities_WithAdjacentRule()
        {
            var input = new List<OrderItem>
            {
                new OrderItem("A", 1.0),
                new OrderItem("A", 2.0),
                new OrderItem("B", 3.0),
                new OrderItem("B", 4.0),
                new OrderItem("B", 5.0),
            }.AsReadOnly();

            var result = ListCompressor<OrderItem>.Compress(
                input,
                new CompressionRule<OrderItem> { AdjacentOnly = true });
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Sku, Is.EqualTo("A"));
            Assert.That(result[0].Quantity, Is.EqualTo(3.0).Within(1e-9));
            Assert.That(result[1].Sku, Is.EqualTo("B"));
            Assert.That(result[1].Quantity, Is.EqualTo(12.0).Within(1e-9));
        }

        [Test]
        public void Compress_GlobalEquals_SumsAcrossListPreservingOrder()
        {
            var input = new List<OrderItem>
            {
                new OrderItem("A", 1.0),
                new OrderItem("B", 2.0),
                new OrderItem("A", 3.0),
                new OrderItem("B", 4.0),
            }.AsReadOnly();

            var rule = new CompressionRule<OrderItem> { AdjacentOnly = false };
            var result = ListCompressor<OrderItem>.Compress(input, rule);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Sku, Is.EqualTo("A"));
            Assert.That(result[0].Quantity, Is.EqualTo(4.0).Within(1e-9));
            Assert.That(result[1].Sku, Is.EqualTo("B"));
            Assert.That(result[1].Quantity, Is.EqualTo(6.0).Within(1e-9));
        }

        private struct ValueWithCount
        {
            public ValueWithCount(string value, int count)
            {
                Value = value;
                Count = count;
            }

            public string Value { get; }
            public int Count { get; }
        }

        [Test]
        public void Compress_CustomRule_SumsUsingSelectors()
        {
            var input = new List<ValueWithCount>
            {
                new ValueWithCount("X", 1),
                new ValueWithCount("X", 2),
                new ValueWithCount("Y", 3),
            }.AsReadOnly();

            var rule = new CompressionRule<ValueWithCount>
            {
                AreEqual = (a, b) => string.Equals(a.Value, b.Value, StringComparison.Ordinal),
                SumSelector = v => v.Count,
                UpdateSum = (v, sum) => new ValueWithCount(v.Value, (int)sum),
            };

            var result = ListCompressor<ValueWithCount>.Compress(input, rule);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Value, Is.EqualTo("X"));
            Assert.That(result[0].Count, Is.EqualTo(3));
            Assert.That(result[1].Value, Is.EqualTo("Y"));
            Assert.That(result[1].Count, Is.EqualTo(3));
        }

        [Test]
        public void Compress_DoesNotMutateOriginalList()
        {
            var original = new List<OrderItem>
            {
                new OrderItem("A", 1.0),
                new OrderItem("A", 2.0),
            };
            var input = original.AsReadOnly();
            var result = ListCompressor<OrderItem>.Compress(input);

            // Original remains with two elements
            Assert.That(original.Count, Is.EqualTo(2));
            // Result is compressed
            Assert.That(result.Count, Is.EqualTo(1));
        }
    }
}
