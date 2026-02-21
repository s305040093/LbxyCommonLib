using System;

namespace LbxyCommonLib.ListCompression.Tests.Samples
{
    public sealed class OrderLineDto
    {
        public Guid LineId { get; init; }

        public string Sku { get; init; }

        public decimal Quantity { get; init; }

        public decimal UnitPrice { get; init; }

        public decimal LineAmount { get; init; }
    }
}
