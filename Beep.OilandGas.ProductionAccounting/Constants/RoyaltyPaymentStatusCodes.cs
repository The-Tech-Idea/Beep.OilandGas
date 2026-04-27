namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// <c>ROYALTY_PAYMENT.STATUS</c> values aligned with
    /// <c>R_PRODUCTION_ACCOUNTING_REFERENCE_CODE</c> REFERENCE_SET <c>ROYALTY_PAYMENT_STATUS</c>.
    /// </summary>
    public static class RoyaltyPaymentStatusCodes
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Paid = "PAID";
        public const string OnHold = "ON_HOLD";
        public const string Disputed = "DISPUTED";
    }
}
