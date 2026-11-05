using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class APInvoiceWorkspaceState
{
    private APInvoiceVarianceProposalReview? variance;
    private int varianceVersion;
    private APInvoiceVarianceSubmission? varianceIntent;
    private APInvoiceVarianceResult? varianceResult;
    private APInvoiceVarianceProof? recordedVariance;
    private APInvoiceVarianceDecisionProof? recordedVarianceDecision;
    public APInvoiceVarianceDecisionProof? RecordedVarianceDecision=>recordedVarianceDecision is null?null:PurchaseReceiptDecisionProof.Copy(recordedVarianceDecision);
    public bool HasRecordedVarianceDecisionSource {get;private set;}
    private bool varianceConfirmed;
    private APInvoiceVarianceDecisionReview? varianceDecisionReview;
    private int varianceDecisionVersion;
    public APInvoiceVarianceDecisionReview? VarianceDecisionReview=>varianceDecisionReview is null?null:PurchaseReceiptDecisionProof.Copy(varianceDecisionReview);
    public void ClearVarianceDecisionReview(){++varianceDecisionVersion;varianceDecisionReview=null;ClearVarianceDecisionOutcome();}
    public async Task LoadVarianceDecisionReviewAsync(Func<string,string,Task<APInvoiceVarianceDecisionReview>> read)
    {
        if(CorrectionCommandUnsettled || VarianceDecisionCommandBusy || HasUnconfirmedVarianceDecisionIntent || HasUnconfirmedVarianceIntent || HasUnconfirmedAllocationIntent || HasUnconfirmedPolicyIntent || VarianceCommandBusy || AllocationCommandBusy || PolicyCommandBusy)throw new InvalidOperationException("Wait for settled source commands before independent variance review.");
        ClearVarianceDecisionReview();var original=company.Snapshot;var version=Version;var generation=varianceDecisionVersion;var id=Detail?.Id;var revision=SourceRevision;
        if(disposed || original.Selection is null || id is null || revision is null)return;
        if(!original.Selection.Permissions.Contains("Accounting.ReviewInvoiceVariance",StringComparer.Ordinal))throw new InvalidOperationException("Current independent variance review grant is required.");
        var review=PurchaseReceiptDecisionProof.Copy(await read(id,revision));
        if(disposed || generation!=varianceDecisionVersion || version!=Version || original!=company.Snapshot)return;
        review.Require(original.Selection.Scope,original.Selection.UserId,id,revision);varianceDecisionReview=review;
    }
    public event Action? VarianceCommandChanged;
    public bool VarianceCommandBusy {get;private set;}
    public bool HasUnconfirmedVarianceIntent=>varianceIntent is not null && !varianceConfirmed;
    public APInvoiceVarianceSubmission? VarianceIntent=>varianceIntent is null?null:PurchaseReceiptDecisionProof.Copy(varianceIntent);
    public APInvoiceVarianceResult? VarianceResult=>varianceResult is null?null:PurchaseReceiptDecisionProof.Copy(varianceResult);
    public APInvoiceVarianceProof? RecordedVariance=>recordedVariance is null?null:PurchaseReceiptDecisionProof.Copy(recordedVariance);
    public bool HasRecordedVarianceSource {get;private set;}
    public bool CanViewVarianceIntent=>!disposed && varianceIntent is not null && company.Selection is {} selected && selected.Scope==varianceIntent.Command.Company &&
        selected.UserId==varianceIntent.Command.ActorUserId && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    public bool CanReconcileVariance=>!disposed && !VarianceCommandBusy && HasUnconfirmedVarianceIntent && company.Selection is {} selected &&
        selected.Scope==varianceIntent!.Command.Company && selected.UserId==varianceIntent.Command.ActorUserId;
    public string? VarianceRecoveryRoute=>CanReconcileVariance?"/accounting/payables/invoices/"+Uri.EscapeDataString(varianceIntent!.Command.InvoiceId):null;
    public APInvoiceVarianceProposalReview? VarianceProposalReview=>variance is null?null:PurchaseReceiptDecisionProof.Copy(variance);
    public IReadOnlyList<APSourceComparison> RecordedVariances=>financial?.Checks.Where(row=>row.Outcome is APSourceComparisonOutcome.RecordedDifference or APSourceComparisonOutcome.ExceedsRecordedLimit).ToArray()??[];
    public void ClearVarianceProposal()
    {
        ++varianceVersion;variance=null;varianceResult=null;
        if(varianceConfirmed && !VarianceCommandBusy)varianceIntent=null;
        varianceConfirmed=false;
    }
    public async Task ReviewVarianceProposalAsync(string disposition,IReadOnlyList<APInvoiceVarianceExplanation> explanations,string reason,
        Func<string,APInvoiceVarianceProposal,Task<APInvoiceVarianceProposalReview>> read)
    {
        if(CorrectionCommandUnsettled || VarianceDecisionCommandBusy || HasUnconfirmedVarianceDecisionIntent || VarianceCommandBusy || HasUnconfirmedVarianceIntent)throw new InvalidOperationException("Reconcile the original variance intent before preparing another proposal.");
        ClearVarianceProposal();var generation=varianceVersion;var version=Version;var financialGeneration=financialVersion;var original=company.Snapshot;
        if(disposed || original.Selection is null || financial is null)throw new InvalidOperationException("Reload complete financial evidence before variance preparation.");
        if(!original.Selection.Permissions.Contains("Accounting.PrepareInvoiceVariance",StringComparer.Ordinal) || AllocationCommandBusy || PolicyCommandBusy || HasUnconfirmedAllocationIntent || HasUnconfirmedPolicyIntent)
            throw new InvalidOperationException("Current variance preparation authority and settled source commands are required.");
        var id=financial.Invoice.Invoice.AP_INVOICE_ID;
        var proposal=new APInvoiceVarianceProposal(disposition,financial.Invoice.Revision,financial.Revision,PurchaseReceiptDecisionProof.Copy(explanations.ToArray()),reason);
        proposal.Require(financial);var review=await read(id,proposal);
        if(disposed || generation!=varianceVersion || version!=Version || financialGeneration!=financialVersion || original!=company.Snapshot)return;
        var frozen=PurchaseReceiptDecisionProof.Copy(review);frozen.Require(original.Selection.Scope,original.Selection.UserId,id,proposal);variance=frozen;
    }
    public async Task RecordVarianceAsync(Func<APInvoiceVarianceSubmission,Task<APInvoiceVarianceResult>> record,Func<string,Task<APInvoiceSourceReview?>> readSource)
    {
        if(CorrectionCommandUnsettled || disposed || VarianceDecisionCommandBusy || HasUnconfirmedVarianceDecisionIntent || VarianceCommandBusy || AllocationCommandBusy || PolicyCommandBusy || HasUnconfirmedAllocationIntent || HasUnconfirmedPolicyIntent)
            throw new InvalidOperationException("Wait for settled source commands before variance submission.");
        var selected=company.Selection??throw new InvalidOperationException("Select the original variance company and account.");
        if(!selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Accounting.PrepareInvoiceVariance",StringComparer.Ordinal) ||
            !selected.Permissions.Contains("Accounting.RecordInvoiceVariance",StringComparer.Ordinal))throw new InvalidOperationException("Current variance preparation and recording grants are required.");
        if(!HasUnconfirmedVarianceIntent)
        {
            var review=variance??throw new InvalidOperationException("Review an explicit current variance preparation before submission.");
            review.Require(selected.Scope,selected.UserId,review.Financial.Invoice.Invoice.AP_INVOICE_ID,review.Proposal);
            var command=new APInvoiceVarianceCommand(selected.Scope,selected.UserId,review.Financial.Invoice.Invoice.AP_INVOICE_ID,review.Proposal,review.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"));
            command.Require(review);varianceIntent=PurchaseReceiptDecisionProof.Copy(new APInvoiceVarianceSubmission(command,review));
        }
        if(!CanReconcileVariance)throw new InvalidOperationException("Return to the original variance company and account to submit or reconcile.");
        var intent=PurchaseReceiptDecisionProof.Copy(varianceIntent!);var original=company.Snapshot;var version=Version;
        VarianceCommandBusy=true;ClearFinancialReview();var generation=varianceVersion;VarianceCommandChanged?.Invoke();
        bool Current()=>!disposed && generation==varianceVersion && version==Version && original==company.Snapshot;
        try
        {
            var result=PurchaseReceiptDecisionProof.Copy(await record(intent));if(!Current())return;result.Require(intent);
            var source=PurchaseReceiptDecisionProof.Copy(await readSource(intent.Command.InvoiceId)??throw new InvalidOperationException("Recorded variance invoice source is unavailable."));if(!Current())return;
            source.Require(intent.Command.Company,intent.Command.ActorUserId,intent.Command.InvoiceId);
            if(System.Text.Json.JsonSerializer.Serialize(source.Invoice)!=System.Text.Json.JsonSerializer.Serialize(result.Invoice) ||
                APInvoiceSourceReview.Create(intent.Command.Company,intent.Command.ActorUserId,APInvoiceVarianceProof.Require(intent.Command.Company,source.Invoice).OriginalInvoice,source.Lines).Revision!=intent.Command.Proposal.ExpectedInvoiceRevision)
                throw new InvalidOperationException("Current source does not confirm the complete original variance outcome and unchanged lines.");
            ApplyDetail(source);varianceResult=result;varianceConfirmed=true;
        }
        finally{VarianceCommandBusy=false;VarianceCommandChanged?.Invoke();}
    }
    private void ReadRecordedVariance(APInvoiceSourceReview source)
    {
        HasRecordedVarianceSource=source.Invoice.VARIANCE_PROPOSAL_JSON is not null;recordedVariance=null;
        HasRecordedVarianceDecisionSource=source.Invoice.VARIANCE_DECISION_JSON is not null;recordedVarianceDecision=null;
        if(HasRecordedVarianceDecisionSource)try{recordedVarianceDecision=APInvoiceVarianceDecisionProof.Require(source.Company,source.Invoice);}catch(InvalidOperationException){}
        if(!HasRecordedVarianceSource)return;
        try{recordedVariance=APInvoiceVarianceProof.Require(source.Company,source.Invoice);}catch(InvalidOperationException){}
    }
}
