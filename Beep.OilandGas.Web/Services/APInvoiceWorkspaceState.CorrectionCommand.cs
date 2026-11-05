using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class APInvoiceWorkspaceState
{
    private APInvoiceCorrectionSubmission? correctionIntent;
    private APInvoiceCorrectionResult? correctionResult;
    private bool correctionConfirmed;
    public bool CorrectionCommandBusy {get;private set;}
    public bool HasUnconfirmedCorrectionIntent=>correctionIntent is not null && !correctionConfirmed;
    public bool CorrectionCommandUnsettled=>CorrectionCommandBusy || HasUnconfirmedCorrectionIntent;
    public APInvoiceCorrectionSubmission? CorrectionIntent=>correctionIntent is null?null:PurchaseReceiptDecisionProof.Copy(correctionIntent);
    public APInvoiceCorrectionResult? CorrectionResult=>correctionResult is null?null:PurchaseReceiptDecisionProof.Copy(correctionResult);
    public bool CanViewCorrectionIntent=>!disposed && correctionIntent is not null && company.Selection is {} selected && selected.Scope==correctionIntent.Command.Company &&
        selected.UserId==correctionIntent.Command.ActorUserId && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    public bool CanReconcileCorrection=>CanViewCorrectionIntent && !CorrectionCommandBusy && HasUnconfirmedCorrectionIntent && Detail?.Id==correctionIntent!.Command.InvoiceId &&
        company.Selection!.Permissions.Contains("Procurement.View",StringComparer.Ordinal) && company.Selection.Permissions.Contains("Accounting.CorrectInvoice",StringComparer.Ordinal);
    public string? CorrectionRecoveryRoute=>CanViewCorrectionIntent && HasUnconfirmedCorrectionIntent?"/accounting/payables/invoices/"+Uri.EscapeDataString(correctionIntent!.Command.InvoiceId):null;
    private void ClearCorrectionOutcome()
    {
        correctionResult=null;if(correctionConfirmed && !CorrectionCommandBusy)correctionIntent=null;correctionConfirmed=false;
    }
    public async Task RecordCorrectionAsync(Func<APInvoiceCorrectionSubmission,Task<APInvoiceCorrectionResult>> record,Func<string,Task<APInvoiceSourceReview?>> readSource)
    {
        ArgumentNullException.ThrowIfNull(record);ArgumentNullException.ThrowIfNull(readSource);
        if(disposed || CorrectionCommandBusy || CorrectionReviewBusy || CorrectionCommandsUnsettled)throw new InvalidOperationException("Wait for settled source commands before correction recording.");
        var selected=company.Selection??throw new InvalidOperationException("Select the original accountant company and account.");
        if(!selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Procurement.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Accounting.CorrectInvoice",StringComparer.Ordinal))
            throw new InvalidOperationException("Current original accountant read and correction grants are required.");
        if(!HasUnconfirmedCorrectionIntent)
        {
            RequireCorrectionEditor();var review=correctionReview??throw new InvalidOperationException("Review exact current correction values before recording.");
            review.Require(selected.Scope,selected.UserId,Detail!.Id,CaptureCorrectionDraft());var history=review.Current.Invoice.Invoice.CORRECTION_HISTORY_JSON;
            var count=history is null?0:APInvoiceCorrectionHistory.Require(selected.Scope,review.Current.Invoice.Invoice,review.Current.Invoice.Lines).Versions.Count;
            var command=new APInvoiceCorrectionCommand(selected.Scope,selected.UserId,Detail.Id,review.Draft,review.Revision,review.BindingRevision,count+1,history is null?null:ProcessDefinitionRevision.FromJson(history),Guid.NewGuid().ToString("N"));
            command.Require(review);correctionIntent=PurchaseReceiptDecisionProof.Copy(new APInvoiceCorrectionSubmission(command,review));
        }
        if(!CanReconcileCorrection)throw new InvalidOperationException("Return to the original correction invoice, company and account to record or reconcile.");
        var intent=PurchaseReceiptDecisionProof.Copy(correctionIntent!);var original=company.Snapshot;var version=Version;
        CorrectionCommandBusy=true;ClearFinancialReview();var generation=correctionVersion;CorrectionChanged?.Invoke();VarianceCommandChanged?.Invoke();
        bool Current()=>!disposed && generation==correctionVersion && version==Version && original==company.Snapshot;
        try
        {
            var result=PurchaseReceiptDecisionProof.Copy(await record(PurchaseReceiptDecisionProof.Copy(intent)));if(!Current())return;result.Require(intent);
            var source=PurchaseReceiptDecisionProof.Copy(await readSource(intent.Command.InvoiceId)??throw new InvalidOperationException("Recorded corrected invoice source is unavailable."));if(!Current())return;
            source.Require(intent.Command.Company,intent.Command.ActorUserId,intent.Command.InvoiceId);
            if(JsonSerializer.Serialize(source.Invoice)!=JsonSerializer.Serialize(result.Invoice) || JsonSerializer.Serialize(source.Lines)!=JsonSerializer.Serialize(result.Lines))
                throw new InvalidOperationException("Current complete source does not confirm the exact original correction header, lines and history.");
            ClearPurchaseSource();ApplyDetail(source);correctionResult=result;correctionConfirmed=true;
        }
        finally{CorrectionCommandBusy=false;CorrectionChanged?.Invoke();VarianceCommandChanged?.Invoke();}
    }
}
