using System;
using System.Collections.Generic;
using System.Linq;
using LbxyCommonLib.ListCompression.Tests.Samples;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests
{
    public class OrderLineCompressionServiceTests
    {
        [Test]
        public void CompressLines_GlobalCompression_GroupsBySkuAndUnitPrice()
        {
            var input = new List<OrderLineDto>
            {
                new OrderLineDto
                {
                    LineId = Guid.NewGuid(),
                    Sku = "A",
                    Quantity = 1.125m,
                    UnitPrice = 10.00m,
                    LineAmount = 11.25m,
                },
                new OrderLineDto
                {
                    LineId = Guid.NewGuid(),
                    Sku = "A",
                    Quantity = 2.375m,
                    UnitPrice = 10.00m,
                    LineAmount = 23.75m,
                },
                new OrderLineDto
                {
                    LineId = Guid.NewGuid(),
                    Sku = "B",
                    Quantity = 1.500m,
                    UnitPrice = 20.00m,
                    LineAmount = 30.00m,
                },
            }.AsReadOnly();

            var service = new OrderLineCompressionService();
            var compressed = service.CompressLines(input, globalCompression: true);

            Assert.That(compressed.Count, Is.EqualTo(2));

            var a = compressed.Single(x => x.Sku == "A");
            var b = compressed.Single(x => x.Sku == "B");

            Assert.That(a.Quantity, Is.EqualTo(3.500m));
            Assert.That(a.UnitPrice, Is.EqualTo(10.00m));
            Assert.That(a.LineAmount, Is.EqualTo(35.00m));

            Assert.That(b.Quantity, Is.EqualTo(1.500m));
            Assert.That(b.UnitPrice, Is.EqualTo(20.00m));
            Assert.That(b.LineAmount, Is.EqualTo(30.00m));
        }

        [Test]
        public void CompressLines_MatchesPureDecimalReferenceWithinRounding()
        {
            var input = new List<OrderLineDto>
            {
                new OrderLineDto
                {
                    LineId = Guid.NewGuid(),
                    Sku = "A",
                    Quantity = 1.125m,
                    UnitPrice = 10.00m,
                    LineAmount = 11.25m,
                },
                new OrderLineDto
                {
                    LineId = Guid.NewGuid(),
                    Sku = "A",
                    Quantity = 2.375m,
                    UnitPrice = 10.00m,
                    LineAmount = 23.75m,
                },
                new OrderLineDto
                {
                    LineId = Guid.NewGuid(),
                    Sku = "A",
                    Quantity = 0.500m,
                    UnitPrice = 10.00m,
                    LineAmount = 5.00m,
                },
            }.AsReadOnly();

            var service = new OrderLineCompressionService();
            var compressed = service.CompressLines(input, globalCompression: true);

            var reference = input
                .GroupBy(x => new { x.Sku, x.UnitPrice })
                .Select(g =>
                {
                    var quantity = g.Sum(x => x.Quantity);
                    quantity = Math.Round(quantity, 3, MidpointRounding.ToEven);
                    var amount = Math.Round(quantity * g.Key.UnitPrice, 2, MidpointRounding.ToEven);
                    return new
                    {
                        g.Key.Sku,
                        g.Key.UnitPrice,
                        Quantity = quantity,
                        LineAmount = amount,
                    };
                })
                .ToList();

            Assert.That(compressed.Count, Is.EqualTo(reference.Count));

            var actual = compressed.Single();
            var expected = reference.Single();

            Assert.That(actual.Sku, Is.EqualTo(expected.Sku));
            Assert.That(actual.UnitPrice, Is.EqualTo(expected.UnitPrice));
            Assert.That(actual.Quantity, Is.EqualTo(expected.Quantity));
            Assert.That(actual.LineAmount, Is.EqualTo(expected.LineAmount));
        }
    }
}
