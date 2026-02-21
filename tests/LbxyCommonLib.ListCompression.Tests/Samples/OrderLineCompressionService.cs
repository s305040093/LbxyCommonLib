using System;
using System.Collections.Generic;
using System.Linq;
using LbxyCommonLib.ListCompression;

namespace LbxyCommonLib.ListCompression.Tests.Samples
{
    internal sealed class OrderLineCompressionService
    {
        private const int QuantityScale = 3;
        private const int AmountScale = 2;

        public IReadOnlyList<OrderLineDto> CompressLines(
            IReadOnlyList<OrderLineDto> input,
            bool globalCompression)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var internalList = input
                .Select(dto => new OrderLineCompressItem(
                    dto.Sku,
                    dto.UnitPrice,
                    QuantityDecimalToDouble(dto.Quantity)))
                .ToList()
                .AsReadOnly();

            var rule = new CompressionRule<OrderLineCompressItem>
            {
                AdjacentOnly = !globalCompression,
            };

            var compressedInternal = ListCompressor<OrderLineCompressItem>.Compress(
                internalList,
                rule);

            var result = compressedInternal
                .Select(item =>
                {
                    var quantityDec = QuantityDoubleToDecimal(item.Quantity);
                    var amount = ComputeLineAmount(quantityDec, item.UnitPrice);

                    return new OrderLineDto
                    {
                        LineId = Guid.NewGuid(),
                        Sku = item.Sku,
                        Quantity = quantityDec,
                        UnitPrice = item.UnitPrice,
                        LineAmount = amount,
                    };
                })
                .ToList();

            return result;
        }

        private static double QuantityDecimalToDouble(decimal quantity)
        {
            var rounded = Math.Round(quantity, QuantityScale, MidpointRounding.ToEven);
            return (double)rounded;
        }

        private static decimal QuantityDoubleToDecimal(double quantity)
        {
            var dec = (decimal)quantity;
            return Math.Round(dec, QuantityScale, MidpointRounding.ToEven);
        }

        private static decimal ComputeLineAmount(decimal quantity, decimal unitPrice)
        {
            var amount = quantity * unitPrice;
            return Math.Round(amount, AmountScale, MidpointRounding.ToEven);
        }
    }
}
