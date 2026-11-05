using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;
public sealed partial class APInvoiceWorkspaceState
{
    private APInvoiceCorrectionHistory? recordedCorrectionHistory;
    private APInvoiceCorrectionDecisionReview? correctionDecisionReview;
    private int correctionDecisionGeneration;
    public bool CorrectionDecisionReviewBusy {get;private set;}
    public event Action? CorrectionDecisionChanged;
    public APInvoiceCorrectionDecisionReview? CorrectionDecisionReview=>CanViewCorrectionDecision && correctionDecisionReview is not null?PurchaseReceiptDecisionProof.Copy(correctionDecisionReview):null;
    public bool CanViewCorrectionDecision=>!disposed && recordedCorrectionHistory is {Versions.Count:>0} && Detail is not null && SourceRevision is not null && company.Selection is {} selected &&
        selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) && selected.Permissions.Contains("Procurement.View",StringComparer.Ordinal) &&
        selected.Permissions.Contains("Accounting.ReviewInvoiceVariance",StringComparer.Ordinal) && recordedVariance is not null &&
        selected.UserId!=recordedCorrectionHistory.Versions[^1].Command.ActorUserId && selected.UserId!=recordedVariance.Command.ActorUserId &&
        selected.UserId!=recordedVariance.OriginalInvoice.ROW_CREATED_BY;
    public void ClearCorrectionDecisionReview()
    {++correctionDecisionGeneration;correctionDecisionReview=null;CorrectionDecisionReviewBusy=false;CorrectionDecisionChanged?.Invoke();}
    public async Task LoadCorrectionDecisionReviewAsync(Func<string,string,Task<APInvoiceCorrectionDecisionReview>> read)
    {
        ArgumentNullException.ThrowIfNull(read);
        if(CorrectionDecisionReviewBusy)throw new InvalidOperationException("Wait for the current independent correction review.");
        ClearCorrectionDecisionReview();
        if(!CanViewCorrectionDecision || CorrectionCommandUnsettled || CorrectionCommandsUnsettled || CorrectionReviewBusy)
            throw new InvalidOperationException("Current independent reviewer authority, recorded correction and settled source commands are required.");
        var original=company.Snapshot;var version=Version;var generation=correctionDecisionGeneration;var id=Detail!.Id;var revision=SourceRevision!;
        CorrectionDecisionReviewBusy=true;CorrectionDecisionChanged?.Invoke();
        try
        {
            var result=PurchaseReceiptDecisionProof.Copy(await read(id,revision));
            if(disposed || generation!=correctionDecisionGeneration || version!=Version || original!=company.Snapshot || !CanViewCorrectionDecision)return;
            result.Require(original.Selection!.Scope,original.Selection.UserId,id,revision);correctionDecisionReview=result;
        }
        finally{if(generation==correctionDecisionGeneration){CorrectionDecisionReviewBusy=false;CorrectionDecisionChanged?.Invoke();}}
    }
}
