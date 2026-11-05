using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed partial record APInvoiceSourceReview
{
    /// <summary>Stable proposal handoff source through a proven first controller decision; other header/line edits still require reconciliation.</summary>
    public APInvoiceSourceReview ForCompanyWorkflow()
    {
        Require(Company,ActorUserId,Invoice.AP_INVOICE_ID);
        var source=PurchaseReceiptDecisionProof.Copy(this);
        if(Invoice.CORRECTION_HISTORY_JSON is not null)
        {
            var history=APInvoiceCorrectionHistory.Require(Company,Invoice,Lines);
            source=Create(Company,ActorUserId,history.OriginalInvoice,history.OriginalLines);
        }
        if(source.Invoice.VARIANCE_DECISION_JSON is not null)
        {
            var decision=APInvoiceVarianceDecisionProof.Require(Company,source.Invoice);
            source=Create(Company,ActorUserId,decision.OriginalInvoice,source.Lines);
        }
        if(source.Invoice.VARIANCE_PROPOSAL_JSON is not null)
        {
            var proposal=APInvoiceVarianceProof.Require(Company,source.Invoice);
            var original=Create(Company,ActorUserId,proposal.OriginalInvoice,source.Lines);
            if(original.Revision!=proposal.Command.Proposal.ExpectedInvoiceRevision)
                throw new InvalidOperationException("The proposal's original invoice lines or source changed. Reconcile the company workflow source.");
        }
        return source;
    }
}
