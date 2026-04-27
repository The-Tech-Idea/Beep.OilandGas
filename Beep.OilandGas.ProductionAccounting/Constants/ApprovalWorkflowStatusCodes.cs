namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Generic approval lifecycle for AFEs, internal control sign-offs, and similar workflows; aligned with
    /// <c>R_PRODUCTION_ACCOUNTING_REFERENCE_CODE</c> REFERENCE_SET <c>APPROVAL_WORKFLOW_STATUS</c>.
    /// Values overlap with <see cref="RoyaltyPaymentStatusCodes"/> where both use industry-standard uppercase tokens.
    /// </summary>
    public static class ApprovalWorkflowStatusCodes
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
    }
}
