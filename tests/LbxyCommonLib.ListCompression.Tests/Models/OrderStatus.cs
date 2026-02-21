namespace LbxyCommonLib.ListCompression.Tests.Models
{
    /// <summary>
    /// Represents the processing status of an order item in tests.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// The order item is pending and not yet processed.
        /// </summary>
        Pending,

        /// <summary>
        /// The order item has been confirmed and paid.
        /// </summary>
        Paid,

        /// <summary>
        /// The order item has been shipped.
        /// </summary>
        Shipped,

        /// <summary>
        /// The order item has been cancelled.
        /// </summary>
        Cancelled,
    }
}
