using System.Collections.Generic;
using LbxyCommonLib.ListCompression;
using LbxyCommonLib.ListCompression.Tests.Models;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests
{
    public class CompressionRuleTests
    {
        [Test]
        public void Default_CompressionRule_HasAdjacentOnlyFalse()
        {
            var rule = new CompressionRule<OrderItem>();
            Assert.That(rule.AdjacentOnly, Is.False);
        }

        [Test]
        public void Compress_WithExplicitDefaultRule_UsesGlobalCompression()
        {
            var input = new List<OrderItem>
            {
                new OrderItem("A", 1.0),
                new OrderItem("B", 2.0),
                new OrderItem("A", 3.0),
            }.AsReadOnly();

            var rule = new CompressionRule<OrderItem>();
            var result = ListCompressor<OrderItem>.Compress(input, rule);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Sku, Is.EqualTo("A"));
            Assert.That(result[0].Quantity, Is.EqualTo(4.0).Within(1e-9));
            Assert.That(result[1].Sku, Is.EqualTo("B"));
            Assert.That(result[1].Quantity, Is.EqualTo(2.0).Within(1e-9));
        }

        [Test]
        public void Compress_DefaultOverload_UsesGlobalCompression()
        {
            var input = new List<OrderItem>
            {
                new OrderItem("A", 1.0),
                new OrderItem("B", 2.0),
                new OrderItem("A", 3.0),
            }.AsReadOnly();

            var result = ListCompressor<OrderItem>.Compress(input);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Sku, Is.EqualTo("A"));
            Assert.That(result[0].Quantity, Is.EqualTo(4.0).Within(1e-9));
            Assert.That(result[1].Sku, Is.EqualTo("B"));
            Assert.That(result[1].Quantity, Is.EqualTo(2.0).Within(1e-9));
        }
    }
}
