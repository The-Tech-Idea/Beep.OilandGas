namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// <c>AFE.STATUS</c> values used by <see cref="Services.AfeService"/>.
    /// Tokens match <see cref="DocumentWorkflowStatusCodes.Draft"/> and <see cref="ApprovalWorkflowStatusCodes.Approved"/>;
    /// also seeded under <c>AFE_STATUS</c> in <see cref="ProductionAccountingReferenceCodeSeed"/>.
    /// </summary>
    public static class AfeStatusCodes
    {
        public const string Draft = DocumentWorkflowStatusCodes.Draft;
        public const string Approved = ApprovalWorkflowStatusCodes.Approved;
    }
}
