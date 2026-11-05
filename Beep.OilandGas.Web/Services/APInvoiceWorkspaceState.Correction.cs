using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class APInvoiceWorkspaceState
{
    private APInvoiceFinancialReview? correctionSource;
    private APInvoiceCorrectionReview? correctionReview;
    private (long Version,CompanyClientSelection? Selection) correctionContext;
    private int correctionVersion,correctionRead,correctionSourceVersion;
    private readonly Dictionary<string,(decimal? Quantity,decimal? UnitPrice,decimal? LineTotal,string? Description)> correctionInputs=new(StringComparer.Ordinal);
    public decimal? CorrectionTotal {get;private set;}
    public string CorrectionReason {get;private set;}="";
    public bool CorrectionEditing=>correctionSource is not null;
    public bool CorrectionReviewBusy {get;private set;}
    public APInvoiceCorrectionReview? CorrectionReview=>correctionReview is null?null:PurchaseReceiptDecisionProof.Copy(correctionReview);
    public IReadOnlyList<(string Id,decimal? Quantity,decimal? UnitPrice,decimal? LineTotal,string? Description)> CorrectionLines=>correctionInputs.OrderBy(row=>row.Key,StringComparer.Ordinal).Select(row=>(row.Key,row.Value.Quantity,row.Value.UnitPrice,row.Value.LineTotal,row.Value.Description)).ToArray();
    public event Action? CorrectionChanged;
    private bool CorrectionCommandsUnsettled=>VarianceCommandBusy || VarianceDecisionCommandBusy || AllocationCommandBusy || PolicyCommandBusy ||
        HasUnconfirmedVarianceIntent || HasUnconfirmedVarianceDecisionIntent || HasUnconfirmedAllocationIntent || HasUnconfirmedPolicyIntent;
    public bool CanViewCorrection=>!disposed && company.Selection is {} selected && Detail is not null && SourceRevision is not null &&
        selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) && selected.Permissions.Contains("Procurement.View",StringComparer.Ordinal) &&
        selected.Permissions.Contains("Accounting.CorrectInvoice",StringComparer.Ordinal) && recordedVariance?.Command.ActorUserId==selected.UserId &&
        recordedVarianceDecision?.Command.Disposition=="REQUEST_CORRECTION" && recordedVarianceDecision.Command.Company==selected.Scope;
    public void ClearCorrection()
    {
        ++correctionVersion;++correctionRead;ClearCorrectionOutcome();correctionSource=null;correctionReview=null;correctionInputs.Clear();CorrectionTotal=null;CorrectionReason="";CorrectionReviewBusy=false;CorrectionChanged?.Invoke();
    }
    private void InvalidateCorrectionReview(){++correctionVersion;correctionReview=null;CorrectionChanged?.Invoke();}
    public void BeginCorrection()
    {
        ClearCorrection();
        if(!CanViewCorrection || CorrectionCommandUnsettled || CorrectionCommandsUnsettled || financial is null)throw new InvalidOperationException("Load current complete financial evidence under the original accountant correction authority before editing.");
        var selected=company.Selection!;financial.Require(selected.Scope,selected.UserId,Detail!.Id,SourceRevision!);
        var proof=APInvoiceVarianceDecisionProof.Require(selected.Scope,financial.Invoice.Invoice);
        if(proof.Command.Disposition!="REQUEST_CORRECTION" || financial.Invoice.Invoice.TOTAL_AMOUNT is not >0 || financial.Invoice.Lines.Count==0 ||
            financial.Invoice.Lines.Any(line=>line.QUANTITY is null || line.UNIT_PRICE is null || line.LINE_TOTAL is null))
            throw new InvalidOperationException("Explicit recorded invoice and line amounts and a controller correction request are required; missing values cannot be inferred.");
        correctionSource=PurchaseReceiptDecisionProof.Copy(financial);correctionContext=company.Snapshot;correctionSourceVersion=Version;
        CorrectionTotal=correctionSource.Invoice.Invoice.TOTAL_AMOUNT;
        foreach(var line in correctionSource.Invoice.Lines)correctionInputs.Add(line.AP_INVOICE_LINE_ID,(line.QUANTITY,line.UNIT_PRICE,line.LINE_TOTAL,line.ITEM_DESCRIPTION));
        CorrectionChanged?.Invoke();
    }
    private void RequireCorrectionEditor()
    {
        if(!CanViewCorrection || CorrectionCommandUnsettled || CorrectionCommandsUnsettled || correctionSource is null || correctionContext!=company.Snapshot || correctionSourceVersion!=Version || correctionSource.Invoice.Revision!=SourceRevision)
        {ClearCorrection();throw new InvalidOperationException("The original accountant correction source or authority changed. Reload before editing.");}
    }
    public void SetCorrectionTotal(decimal? amount){RequireCorrectionEditor();CorrectionTotal=amount;InvalidateCorrectionReview();}
    public void SetCorrectionReason(string? reason){RequireCorrectionEditor();CorrectionReason=reason??"";InvalidateCorrectionReview();}
    public void SetCorrectionAmount(string id,string field,decimal? amount)
    {
        RequireCorrectionEditor();if(!correctionInputs.TryGetValue(id,out var row))throw new InvalidOperationException("Choose a recorded correction line.");
        switch(field){case "QUANTITY":row.Quantity=amount;break;case "UNIT_PRICE":row.UnitPrice=amount;break;case "LINE_TOTAL":row.LineTotal=amount;break;default:throw new ArgumentException("Choose an editable correction amount.");}
        correctionInputs[id]=row;InvalidateCorrectionReview();
    }
    public void SetCorrectionDescription(string id,string? text)
    {RequireCorrectionEditor();if(!correctionInputs.TryGetValue(id,out var row))throw new InvalidOperationException("Choose a recorded correction line.");row.Description=text;correctionInputs[id]=row;InvalidateCorrectionReview();}
    private APInvoiceCorrectionDraft CaptureCorrectionDraft()
    {
        RequireCorrectionEditor();if(CorrectionTotal is null || correctionInputs.Values.Any(row=>row.Quantity is null || row.UnitPrice is null || row.LineTotal is null))
            throw new InvalidOperationException("Enter explicit invoice and line amounts before review.");
        var source=correctionSource!;var proof=APInvoiceVarianceDecisionProof.Require(source.Invoice.Company,source.Invoice.Invoice);
        var draft=new APInvoiceCorrectionDraft(source.Invoice.Revision,source.Revision,proof.Command.IntentId,APInvoiceCorrectionReview.DecisionRevision(source.Invoice.Invoice.VARIANCE_DECISION_JSON!),
            CorrectionTotal.Value,CorrectionReason,CorrectionInputs());draft.RequireShape();return draft;
    }
    private APInvoiceCorrectionLine[] CorrectionInputs()=>correctionInputs.OrderBy(row=>row.Key,StringComparer.Ordinal).Select(row=>new APInvoiceCorrectionLine(row.Key,row.Value.Quantity!.Value,row.Value.UnitPrice!.Value,row.Value.LineTotal!.Value,row.Value.Description)).ToArray();
    public async Task LoadCorrectionReviewAsync(Func<string,APInvoiceCorrectionDraft,Task<APInvoiceCorrectionReview>> read)
    {
        ArgumentNullException.ThrowIfNull(read);if(CorrectionReviewBusy)throw new InvalidOperationException("Wait for the current correction review.");
        InvalidateCorrectionReview();var draft=CaptureCorrectionDraft();var source=correctionSource!;var generation=correctionVersion;var operation=++correctionRead;var original=company.Snapshot;var version=Version;
        CorrectionReviewBusy=true;CorrectionChanged?.Invoke();
        try
        {
            var result=PurchaseReceiptDecisionProof.Copy(await read(source.Invoice.Invoice.AP_INVOICE_ID,PurchaseReceiptDecisionProof.Copy(draft)));
            if(disposed || generation!=correctionVersion || operation!=correctionRead || version!=Version || original!=company.Snapshot || !CanViewCorrection)return;
            result.Require(original.Selection!.Scope,original.Selection.UserId,source.Invoice.Invoice.AP_INVOICE_ID,draft);correctionReview=result;
        }
        finally{if(operation==correctionRead){CorrectionReviewBusy=false;CorrectionChanged?.Invoke();}}
    }
}
