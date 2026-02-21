using System;
using System.Collections.Generic;
using LbxyCommonLib.ListCompression.Tests.Models;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests
{
    public class FodyEqualityTests
    {
        [Test]
        public void OrderItem_Equality_IgnoresNonKeyProperties()
        {
            var item1 = new OrderItem("A", 1.0)
            {
                Id = Guid.NewGuid(),
                Priority = 1,
                Status = OrderStatus.Paid,
                CreatedAt = DateTime.UtcNow,
                Tags = new List<string> { "x", "y" },
            };

            var item2 = new OrderItem("A", 2.0)
            {
                Id = Guid.NewGuid(),
                Priority = 5,
                Status = OrderStatus.Cancelled,
                CreatedAt = null,
                Tags = new List<string> { "other" },
            };

            Assert.That(item1, Is.EqualTo(item2));
            Assert.That(item1 == item2, Is.True);
            Assert.That(item1 != item2, Is.False);
            Assert.That(item1.GetHashCode(), Is.EqualTo(item2.GetHashCode()));
        }

        [Test]
        public void OrderItem_Equality_UsesSkuAsKey()
        {
            var item1 = new OrderItem("A", 1.0);
            var item2 = new OrderItem("B", 1.0);

            Assert.That(item1, Is.Not.EqualTo(item2));
            Assert.That(item1 == item2, Is.False);
            Assert.That(item1 != item2, Is.True);
        }

        [Test]
        public void OrderItem_Equality_HandlesNullCorrectly()
        {
            OrderItem left = new OrderItem("A", 1.0);
            OrderItem right = null;

            Assert.That(left == right, Is.False);
            Assert.That(right == left, Is.False);
            Assert.That(left != right, Is.True);
            Assert.That(right != left, Is.True);
        }

        [Test]
        public void CustomEqualsSample_UsesCustomLogic()
        {
            var a = new CustomEqualsSample { X = 1, Z = 0 };
            var b = new CustomEqualsSample { X = 1, Z = 5 };
            var c = new CustomEqualsSample { X = 1, Z = 7 };

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b, Is.True);
            Assert.That(a != b, Is.False);

            Assert.That(b, Is.Not.EqualTo(c));
            Assert.That(b == c, Is.False);
            Assert.That(b != c, Is.True);
        }
    }
}
