
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum ExchangeCommitmentStatus
    {
        /// <summary>
        /// Pending fulfillment.
        /// </summary>
        Pending,

        /// <summary>
        /// Partially fulfilled.
        /// </summary>
        PartiallyFulfilled,

        /// <summary>
        /// Fully fulfilled.
        /// </summary>
        Fulfilled,

        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled
    }
}
