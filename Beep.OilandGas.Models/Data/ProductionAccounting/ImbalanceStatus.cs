
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum ImbalanceStatus
    {
        /// <summary>
        /// Balanced (within tolerance).
        /// </summary>
        Balanced,

        /// <summary>
        /// Over-delivered (actual > nominated).
        /// </summary>
        OverDelivered,

        /// <summary>
        /// Under-delivered (actual < nominated).
        /// </summary>
        UnderDelivered,

        /// <summary>
        /// Pending reconciliation.
        /// </summary>
        PendingReconciliation,

        /// <summary>
        /// Reconciled.
        /// </summary>
        Reconciled
    }
}
