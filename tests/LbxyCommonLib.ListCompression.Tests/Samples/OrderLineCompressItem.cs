using System;
using LbxyCommonLib.ListCompression.Interfaces;

namespace LbxyCommonLib.ListCompression.Tests.Samples
{
    internal sealed class OrderLineCompressItem : ISummable<OrderLineCompressItem>, IEquatable<OrderLineCompressItem>
    {
        public OrderLineCompressItem(string sku, decimal unitPrice, double quantity)
        {
            Sku = sku ?? throw new ArgumentNullException(nameof(sku));
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        public string Sku { get; }

        public decimal UnitPrice { get; }

        public double Quantity { get; }

        public double GetSummableValue()
        {
            return Quantity;
        }

        public OrderLineCompressItem WithUpdatedSummableValue(double newValue)
        {
            return new OrderLineCompressItem(Sku, UnitPrice, newValue);
        }

        public bool Equals(OrderLineCompressItem other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Sku, other.Sku, StringComparison.Ordinal) && UnitPrice == other.UnitPrice;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as OrderLineCompressItem);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Sku, UnitPrice);
        }
    }
}
