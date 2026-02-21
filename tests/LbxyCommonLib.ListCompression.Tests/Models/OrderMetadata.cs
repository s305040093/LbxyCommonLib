using System;

namespace LbxyCommonLib.ListCompression.Tests.Models
{
    /// <summary>
    /// Represents additional metadata for an order item in tests.
    /// </summary>
    public sealed class OrderMetadata
    {
        /// <summary>
        /// Gets or sets the user name that created the order item.
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time when the order item was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
    }
}
