using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed partial class APInvoiceWorkspaceState
{
    private APInvoiceVarianceDecisionSubmission? decisionIntent;
    private APInvoiceVarianceDecisionResult? decisionResult;
    private bool decisionConfirmed;
    public bool VarianceDecisionCommandBusy {get;private set;}
    public bool HasUnconfirmedVarianceDecisionIntent=>decisionIntent is not null && !decisionConfirmed;
    public APInvoiceVarianceDecisionSubmission? VarianceDecisionIntent=>decisionIntent is null?null:PurchaseReceiptDecisionProof.Copy(decisionIntent);
    public APInvoiceVarianceDecisionResult? VarianceDecisionResult=>decisionResult is null?null:PurchaseReceiptDecisionProof.Copy(decisionResult);
    public bool CanViewVarianceDecisionIntent=>!disposed && decisionIntent is not null && company.Selection is {} selected &&
        selected.Scope==decisionIntent.Command.Company && selected.UserId==decisionIntent.Command.ActorUserId && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    public bool CanReconcileVarianceDecision=>!disposed && !VarianceDecisionCommandBusy && HasUnconfirmedVarianceDecisionIntent && company.Selection is {} selected &&
        selected.Scope==decisionIntent!.Command.Company && selected.UserId==decisionIntent.Command.ActorUserId;
    public string? VarianceDecisionRecoveryRoute=>CanReconcileVarianceDecision?"/accounting/payables/invoices/"+Uri.EscapeDataString(decisionIntent!.Command.InvoiceId):null;
    private void ClearVarianceDecisionOutcome()
    {
        decisionResult=null;if(decisionConfirmed && !VarianceDecisionCommandBusy)decisionIntent=null;decisionConfirmed=false;
    }
    public async Task RecordVarianceDecisionAsync(string disposition,string reason,Func<APInvoiceVarianceDecisionSubmission,Task<APInvoiceVarianceDecisionResult>> record,
        Func<string,Task<APInvoiceSourceReview?>> readSource)
    {
        if(CorrectionCommandUnsettled || disposed || VarianceDecisionCommandBusy || VarianceCommandBusy || AllocationCommandBusy || PolicyCommandBusy ||
            HasUnconfirmedVarianceIntent || HasUnconfirmedAllocationIntent || HasUnconfirmedPolicyIntent)throw new InvalidOperationException("Wait for settled source commands before controller submission.");
        var selected=company.Selection??throw new InvalidOperationException("Select the original controller company and account.");
        if(!selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Accounting.ReviewInvoiceVariance",StringComparer.Ordinal) ||
            !selected.Permissions.Contains("Accounting.RecordInvoiceVarianceDecision",StringComparer.Ordinal))throw new InvalidOperationException("Current controller review and recording grants are required.");
        if(!HasUnconfirmedVarianceDecisionIntent)
        {
            var review=varianceDecisionReview??throw new InvalidOperationException("Review current independent variance evidence before recording.");
            review.Require(selected.Scope,selected.UserId,review.Current.Invoice.Invoice.AP_INVOICE_ID,SourceRevision!);
            var command=new APInvoiceVarianceDecisionCommand(selected.Scope,selected.UserId,review.Current.Invoice.Invoice.AP_INVOICE_ID,review.RecordedProposal.Command.IntentId,
                disposition,reason,review.Current.Invoice.Revision,review.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"));
            command.Require(review);decisionIntent=PurchaseReceiptDecisionProof.Copy(new APInvoiceVarianceDecisionSubmission(command,review));
        }
        if(!CanReconcileVarianceDecision)throw new InvalidOperationException("Return to the original controller company and account to submit or reconcile.");
        var intent=PurchaseReceiptDecisionProof.Copy(decisionIntent!);var original=company.Snapshot;var version=Version;
        VarianceDecisionCommandBusy=true;ClearFinancialReview();var generation=varianceDecisionVersion;VarianceCommandChanged?.Invoke();
        bool Current()=>!disposed && generation==varianceDecisionVersion && version==Version && original==company.Snapshot;
        try
        {
            var result=PurchaseReceiptDecisionProof.Copy(await record(intent));if(!Current())return;result.Require(intent);
            var source=PurchaseReceiptDecisionProof.Copy(await readSource(intent.Command.InvoiceId)??throw new InvalidOperationException("Recorded controller invoice source is unavailable."));if(!Current())return;
            source.Require(intent.Command.Company,intent.Command.ActorUserId,intent.Command.InvoiceId);
            var proof=APInvoiceVarianceDecisionProof.Require(intent.Command.Company,source.Invoice);
            if(JsonSerializer.Serialize(source.Invoice)!=JsonSerializer.Serialize(result.Invoice) || !proof.Command.Same(intent.Command) ||
                APInvoiceSourceReview.Create(intent.Command.Company,intent.Command.ActorUserId,proof.OriginalInvoice,source.Lines).Revision!=intent.Command.ExpectedInvoiceRevision)
                throw new InvalidOperationException("Current source does not confirm the exact original controller decision and unchanged lines.");
            ApplyDetail(source);decisionResult=result;decisionConfirmed=true;
        }
        finally{VarianceDecisionCommandBusy=false;VarianceCommandChanged?.Invoke();}
    }
}
