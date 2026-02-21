using System;
using System.Collections.Generic;
using LbxyCommonLib.ListCompression.Interfaces;

namespace LbxyCommonLib.ListCompression.Tests.Models
{
    /// <summary>
    /// Represents an order line used in list compression tests.
    /// </summary>
    [Equals]
    public sealed class OrderItem : ISummable<OrderItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderItem"/> class.
        /// </summary>
        /// <param name="sku">The stock keeping unit identifier.</param>
        /// <param name="quantity">The quantity associated with this order item.</param>
        public OrderItem(string sku, double quantity)
        {
            Sku = sku ?? throw new ArgumentNullException(nameof(sku));
            Quantity = quantity;
            Id = Guid.Empty;
            Priority = 0;
            Status = OrderStatus.Pending;
            CreatedAt = null;
            Tags = new List<string>();
            Metadata = new OrderMetadata();
        }

        /// <summary>
        /// Gets or sets the stock keeping unit identifier.
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the order item.
        /// </summary>
        [IgnoreDuringEquals]
        public double Quantity { get; set; }

        /// <summary>
        /// Gets or sets the internal identifier used only for testing.
        /// </summary>
        [IgnoreDuringEquals]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the order item.
        /// </summary>
        [IgnoreDuringEquals]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the processing status of the order item.
        /// </summary>
        [IgnoreDuringEquals]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the order item.
        /// </summary>
        [IgnoreDuringEquals]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the order item.
        /// </summary>
        [IgnoreDuringEquals]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the additional metadata of the order item.
        /// </summary>
        [IgnoreDuringEquals]
        public OrderMetadata Metadata { get; set; }

        /// <summary>
        /// Gets the numeric value used for summation in compression.
        /// </summary>
        /// <returns>The quantity of this order item.</returns>
        public double GetSummableValue()
        {
            return Quantity;
        }

        /// <summary>
        /// Creates a new instance with the updated summable value.
        /// </summary>
        /// <param name="newValue">The new quantity to apply.</param>
        /// <returns>A new <see cref="OrderItem"/> instance with updated quantity.</returns>
        public OrderItem WithUpdatedSummableValue(double newValue)
        {
            return new OrderItem(Sku, newValue)
            {
                Id = Id,
                Priority = Priority,
                Status = Status,
                CreatedAt = CreatedAt,
                Tags = new List<string>(Tags),
                Metadata = new OrderMetadata
                {
                    CreatedBy = Metadata.CreatedBy,
                    CreatedAt = Metadata.CreatedAt,
                },
            };
        }

        /// <summary>
        /// Determines whether two <see cref="OrderItem"/> instances are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><c>true</c> if the two instances are considered equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(OrderItem left, OrderItem right) => Operator.Weave(left, right);

        /// <summary>
        /// Determines whether two <see cref="OrderItem"/> instances are not equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(OrderItem left, OrderItem right) => Operator.Weave(left, right);
    }
}
