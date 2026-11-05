using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class PurchaseOrderWorkspaceState
{
    private PurchaseOrderInventoryLinkCycleResult? linkCycleHistory,linkCycleResult;
    private PurchaseOrderInventoryLinkCycleReview? linkCycleReview;
    private PurchaseOrderInventoryLinkCycleSubmission? linkCycleIntent;
    private bool linkCycleConfirmed;
    private int linkCycleGeneration;
    public bool LinkCycleBusy {get;private set;}
    public event Action? LinkCycleChanged;
    public bool HasUnconfirmedLinkCycleIntent=>linkCycleIntent is not null && !linkCycleConfirmed;
    public bool LinkCycleBlocked=>LinkCycleBusy || HasUnconfirmedLinkCycleIntent;
    public PurchaseOrderInventoryLinkCycleResult? LinkCycleHistory=>linkCycleHistory is null?null:CompanyInventorySourceReview.Copy(linkCycleHistory);
    public PurchaseOrderInventoryLinkCycleReview? LinkCycleReview=>linkCycleReview is null?null:CompanyInventorySourceReview.Copy(linkCycleReview);
    public PurchaseOrderInventoryLinkCycleSubmission? LinkCycleIntent=>linkCycleIntent is null?null:CompanyInventorySourceReview.Copy(linkCycleIntent);
    public PurchaseOrderInventoryLinkCycleResult? LinkCycleResult=>linkCycleResult is null?null:CompanyInventorySourceReview.Copy(linkCycleResult);
    public string? LinkCycleRecoveryRoute=>HasUnconfirmedLinkCycleIntent && company.Selection is {} selected && selected.Scope==linkCycleIntent!.Command.Company && selected.UserId==linkCycleIntent.Command.ActorUserId?
        "/procurement/orders/"+Uri.EscapeDataString(linkCycleIntent.Command.OrderId):null;
    public bool CanReconcileLinkCycle=>!disposed && !WarehouseWorkBlocked && !LinkCycleBusy && !LinkDecisionBlocked && !LinkPreparationBlocked && !ReceiverBusy && !HasUnconfirmedReceiverIntent && !InventoryLinkBusy && HasUnconfirmedLinkCycleIntent &&
        company.Selection is {} selected && selected.Scope==linkCycleIntent!.Command.Company && selected.UserId==linkCycleIntent.Command.ActorUserId && Detail?.Id==linkCycleIntent.Command.OrderId;
    private void ClearLinkCycleDisplay()
    {
        ++linkCycleGeneration;linkCycleHistory=null;linkCycleReview=null;linkCycleResult=null;
        if(linkCycleConfirmed && !LinkCycleBusy)linkCycleIntent=null;linkCycleConfirmed=false;
    }
    private void RequireCycleIdle()
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || LinkDecisionBlocked || LinkPreparationBlocked || ReceiverBusy || HasUnconfirmedReceiverIntent || InventoryLinkBusy)
            throw new InvalidOperationException("Wait or reconcile the original company intent before reviewing a cycle.");
    }
    public void ClearLinkCycleReview(){RequireCycleIdle();ClearLinkCycleDisplay();}
    public async Task LoadLinkCycleHistoryAsync(string lineId,Func<string,string,Task<PurchaseOrderInventoryLinkCycleResult>> read)
    {
        RequireCycleIdle();ClearLinkCycleDisplay();var original=company.Snapshot;var version=Version;var generation=linkCycleGeneration;var order=Detail?.Id;var revision=SourceRevision;
        if(original.Selection is null || order is null || revision is null || Lines?.Any(row=>row.Id==lineId)!=true)throw new InvalidOperationException("Select an original owned line for history.");
        LinkCycleBusy=true;LinkCycleChanged?.Invoke();
        try
        {
            var result=CompanyInventorySourceReview.Copy(await read(order,lineId) ?? throw new InvalidOperationException("Complete cycle history is unavailable."));
            if(disposed || WarehouseWorkBlocked || version!=Version || generation!=linkCycleGeneration || original!=company.Snapshot)return;
            result.Require(original.Selection.Scope,original.Selection.UserId,order,lineId);
            if(result.Purchase.Revision!=revision || SourceRevision!=revision)throw new InvalidOperationException("The displayed order changed. Reload before reviewing history.");
            linkCycleHistory=result;
        }
        finally{LinkCycleBusy=false;LinkCycleChanged?.Invoke();}
    }
    public async Task LoadLinkCycleReviewAsync(string lineId,PurchaseOrderInventoryLinkCycleAction action,Func<string,string,PurchaseOrderInventoryLinkCycleAction,Task<PurchaseOrderInventoryLinkCycleReview>> read)
    {
        RequireCycleIdle();ClearLinkCycleDisplay();ClearLinkDecisionReview();ClearLinkPreparationReview();ClearReceiverDisplay();var original=company.Snapshot;var version=Version;var generation=linkCycleGeneration;var order=Detail?.Id;var revision=SourceRevision;
        if(original.Selection is null || order is null || revision is null || Lines?.Any(row=>row.Id==lineId && row.ActiveIndicator=="Y" && row.Preparation is not null)!=true || !Enum.IsDefined(action))throw new InvalidOperationException("Select an active verified prepared line and explicit cycle action.");
        LinkCycleBusy=true;LinkCycleChanged?.Invoke();
        try
        {
            var review=CompanyInventorySourceReview.Copy(await read(order,lineId,action) ?? throw new InvalidOperationException("Cycle review is unavailable."));
            if(disposed || WarehouseWorkBlocked || version!=Version || generation!=linkCycleGeneration || original!=company.Snapshot)return;
            review.Require(original.Selection.Scope,original.Selection.UserId,order,lineId);
            if(review.Action!=action || review.Purchase.Revision!=revision || SourceRevision!=revision)throw new InvalidOperationException("Original displayed cycle evidence changed.");
            linkCycleReview=review;linkCycleHistory=new(review.Purchase,review.History);
        }
        finally{LinkCycleBusy=false;LinkCycleChanged?.Invoke();}
    }
    public async Task RecordLinkCycleAsync(string reason,Func<PurchaseOrderInventoryLinkCycleSubmission,Task<PurchaseOrderInventoryLinkCycleResult>> record,Func<string,string,Task<PurchaseOrderInventoryLinkCycleResult>> read)
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBusy || LinkDecisionBlocked || LinkPreparationBlocked || ReceiverBusy || HasUnconfirmedReceiverIntent || InventoryLinkBusy)throw new InvalidOperationException("Wait or reconcile competing company work.");
        if(linkCycleIntent is null || linkCycleConfirmed)
        {
            var review=linkCycleReview ?? throw new InvalidOperationException("Review the original cycle first.");var selected=company.Selection ?? throw new InvalidOperationException("Select the original company account.");
            var command=new PurchaseOrderInventoryLinkCycleCommand(selected.Scope,selected.UserId,review.Purchase.Header.PURCHASE_ORDER_ID,review.LineId,review.Action,review.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),reason);
            var intent=new PurchaseOrderInventoryLinkCycleSubmission(command,review);intent.Require(selected.Scope,selected.UserId,command.OrderId,command.LineId);
            if(Detail?.Id!=command.OrderId || SourceRevision!=review.Purchase.Revision)throw new InvalidOperationException("Reload the original reviewed order.");
            linkCycleIntent=CompanyInventorySourceReview.Copy(intent);linkCycleConfirmed=false;
        }
        if(!CanReconcileLinkCycle)throw new InvalidOperationException("Return to the original company, account and order to reconcile this cycle.");
        var submission=CompanyInventorySourceReview.Copy(linkCycleIntent!);var snapshot=company.Snapshot;var version=Version;var generation=linkCycleGeneration;
        LinkCycleBusy=true;linkCycleHistory=null;linkCycleReview=null;linkCycleResult=null;ClearInventoryLink();LinkCycleChanged?.Invoke();
        bool Current()=>!disposed && !WarehouseWorkBlocked && snapshot==company.Snapshot && version==Version && generation==linkCycleGeneration;
        try
        {
            var result=CompanyInventorySourceReview.Copy(await record(CompanyInventorySourceReview.Copy(submission)) ?? throw new InvalidOperationException("Cycle result is unavailable."));submission.RequireResult(result);if(!Current())return;
            var fresh=CompanyInventorySourceReview.Copy(await read(submission.Command.OrderId,submission.Command.LineId) ?? throw new InvalidOperationException("Cycle read-back is unavailable."));submission.RequireResult(fresh);if(!Current())return;
            if(result.Purchase.Revision!=fresh.Purchase.Revision || result.History.Revision!=fresh.History.Revision)throw new InvalidOperationException("The committed cycle read-back changed.");
            using var staged=new PurchaseOrderWorkspaceState(company);await staged.LoadDetailAsync(submission.Command.OrderId,_=>Task.FromResult<PurchaseOrderSourceReview?>(fresh.Purchase));if(!Current())return;
            Detail=staged.Detail;Lines=staged.Lines;Receipts=staged.Receipts;SourceRevision=staged.SourceRevision;RetrievedAtUtc=staged.RetrievedAtUtc;Rows=null;
            linkCycleHistory=fresh;linkCycleResult=result;linkCycleConfirmed=true;
        }
        catch{if(Current()){linkCycleHistory=null;linkCycleReview=null;linkCycleResult=null;}throw;}
        finally{LinkCycleBusy=false;LinkCycleChanged?.Invoke();}
    }
}
