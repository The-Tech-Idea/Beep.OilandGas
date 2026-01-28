
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum PaymentMethod
    {
        /// <summary>
        /// Check payment.
        /// </summary>
        Check,

        /// <summary>
        /// Wire transfer.
        /// </summary>
        WireTransfer,

        /// <summary>
        /// ACH (Automated Clearing House).
        /// </summary>
        ACH,

        /// <summary>
        /// Direct deposit.
        /// </summary>
        DirectDeposit
    }
}
