using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Web.Services;
public sealed partial class PurchaseOrderWorkspaceState
{
    private PurchaseOrderInventoryLinkProposalReview? linkPreparationReview;
    private PurchaseOrderInventoryLinkProposalSubmission? linkPreparationIntent;
    private PO_LINE_ITEM? linkPreparationResult;
    private bool linkPreparationConfirmed;
    private int linkPreparationGeneration;
    private PurchaseOrderInventoryLinkResolution? linkPreparationResolution;
    public PurchaseOrderInventoryLinkResolution? LinkPreparationResolution=>linkPreparationResolution is null?null:CompanyInventorySourceReview.Copy(linkPreparationResolution);
    public bool LinkPreparationBusy {get;private set;}
    public event Action? LinkPreparationChanged;
    public IReadOnlyList<LinkTarget>? LinkTargets {get;private set;}
    public sealed record LinkTarget(string Id,string? Description,string? Warehouse,string? Location,string? Unit);
    public bool HasUnconfirmedLinkPreparationIntent=>linkPreparationIntent is not null && !linkPreparationConfirmed;
    public bool LinkPreparationBlocked=>LinkPreparationBusy || HasUnconfirmedLinkPreparationIntent;
    public PurchaseOrderInventoryLinkProposalReview? LinkPreparationReview=>linkPreparationReview is null?null:CompanyInventorySourceReview.Copy(linkPreparationReview);
    public PurchaseOrderInventoryLinkProposalSubmission? LinkPreparationIntent=>linkPreparationIntent is null?null:CompanyInventorySourceReview.Copy(linkPreparationIntent);
    public PO_LINE_ITEM? LinkPreparationResult=>linkPreparationResult is null?null:CompanyInventorySourceReview.Copy(linkPreparationResult);
    public string? LinkPreparationRecoveryRoute=>HasUnconfirmedLinkPreparationIntent && company.Selection is {} selected && selected.Scope==linkPreparationIntent!.Command.Company && selected.UserId==linkPreparationIntent.Command.ActorUserId?
        "/procurement/orders/"+Uri.EscapeDataString(linkPreparationIntent.Command.OrderId):null;
    public bool CanReconcileLinkPreparation=>!disposed && !WarehouseWorkBlocked && !LinkCycleBlocked && !LinkPreparationBusy && !LinkDecisionBlocked && !ReceiverBusy && !HasUnconfirmedReceiverIntent && HasUnconfirmedLinkPreparationIntent &&
        company.Selection is {} selected && selected.Scope==linkPreparationIntent!.Command.Company && selected.UserId==linkPreparationIntent.Command.ActorUserId && Detail?.Id==linkPreparationIntent.Command.OrderId;
    private void ClearLinkPreparationDisplay()
    {
        ++linkPreparationGeneration;linkPreparationReview=null;linkPreparationResult=null;linkPreparationResolution=null;LinkTargets=null;
        if(linkPreparationConfirmed && !LinkPreparationBusy)linkPreparationIntent=null;linkPreparationConfirmed=false;
    }
    public void ClearLinkPreparationReview()
    {
        if(LinkCycleBlocked || LinkPreparationBlocked || LinkDecisionBlocked)throw new InvalidOperationException("Wait or reconcile the original inventory-link preparation first.");
        ++linkPreparationGeneration;linkPreparationReview=null;linkPreparationResult=null;linkPreparationResolution=null;
    }
    private void RequirePreparationIdle()
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || LinkDecisionBlocked || LinkPreparationBlocked || ReceiverBusy || HasUnconfirmedReceiverIntent || InventoryLinkBusy)
            throw new InvalidOperationException("Wait or reconcile the original preparation/receiver intent before another review.");
    }
    public async Task LoadLinkTargetsAsync(Func<Task<List<INVENTORY_ITEM>>> read)
    {
        RequirePreparationIdle();ClearLinkDecisionReview();ClearLinkPreparationReview();LinkTargets=null;var original=company.Snapshot;var version=Version;var generation=linkPreparationGeneration;
        if(original.Selection is null || Detail is null)throw new InvalidOperationException("Reload the original company order first.");
        LinkPreparationBusy=true;LinkPreparationChanged?.Invoke();
        try
        {
            var rows=await read() ?? throw new InvalidOperationException("Owned inventory targets are unavailable.");
            if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || version!=Version || generation!=linkPreparationGeneration || original!=company.Snapshot)return;
            CompanyInventorySourceReview.RequireRows(original.Selection.Scope,rows);
            LinkTargets=Array.AsReadOnly(rows.Where(row=>row.ACTIVE_IND=="Y").OrderBy(row=>row.INVENTORY_ITEM_ID,StringComparer.Ordinal)
                .Select(row=>new LinkTarget(row.INVENTORY_ITEM_ID,row.DESCRIPTION,row.WAREHOUSE_ID,row.LOCATION_ID,row.UNIT_OF_MEASURE)).ToArray());
        }
        finally{LinkPreparationBusy=false;LinkPreparationChanged?.Invoke();}
    }
    public async Task LoadLinkPreparationReviewAsync(string lineId,string targetId,Func<string,string,string,Task<PurchaseOrderInventoryLinkProposalReview>> read)
    {
        RequirePreparationIdle();ClearLinkDecisionReview();ClearLinkPreparationReview();ClearReceiverDisplay();var original=company.Snapshot;var version=Version;var generation=linkPreparationGeneration;var order=Detail?.Id;var revision=SourceRevision;
        if(original.Selection is null || order is null || revision is null || Lines?.Any(row=>row.Id==lineId && row.ActiveIndicator=="Y")!=true || LinkTargets?.Any(row=>row.Id==targetId)!=true)
            throw new InvalidOperationException("Select an active owned line and a loaded owned inventory target.");
        LinkPreparationBusy=true;LinkPreparationChanged?.Invoke();
        try
        {
            var review=CompanyInventorySourceReview.Copy(await read(order,lineId,targetId) ?? throw new InvalidOperationException("Preparation source review is unavailable."));
            if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || version!=Version || generation!=linkPreparationGeneration || original!=company.Snapshot)return;
            review.Require(original.Selection.Scope,original.Selection.UserId,order,lineId,targetId);
            if(review.Purchase.Revision!=revision || SourceRevision!=revision)throw new InvalidOperationException("The displayed order source changed. Reload before preparing.");
            linkPreparationReview=review;
        }
        finally{LinkPreparationBusy=false;LinkPreparationChanged?.Invoke();}
    }
    public async Task RecordLinkPreparationAsync(string reason,Func<PurchaseOrderInventoryLinkProposalSubmission,Task<PO_LINE_ITEM>> record,Func<string,Task<PurchaseOrderSourceReview?>> read)
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || LinkDecisionBlocked || LinkPreparationBusy || ReceiverBusy || HasUnconfirmedReceiverIntent || InventoryLinkBusy)throw new InvalidOperationException("Wait for the current preparation or receiving operation.");
        if(linkPreparationIntent is null || linkPreparationConfirmed)
        {
            var review=linkPreparationReview ?? throw new InvalidOperationException("Review the original line and inventory target first.");
            var selected=company.Selection ?? throw new InvalidOperationException("Select the original company and account.");
            var command=new PurchaseOrderInventoryLinkProposalCommand(selected.Scope,selected.UserId,review.Purchase.Header.PURCHASE_ORDER_ID,review.LineId,review.Target.Item.INVENTORY_ITEM_ID,review.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),reason);
            var submission=new PurchaseOrderInventoryLinkProposalSubmission(command,review);submission.Require(selected.Scope,selected.UserId,command.OrderId,command.LineId);
            if(Detail?.Id!=command.OrderId || SourceRevision!=review.Purchase.Revision)throw new InvalidOperationException("Reload the original preparation source.");
            linkPreparationIntent=CompanyInventorySourceReview.Copy(submission);linkPreparationConfirmed=false;
        }
        if(!CanReconcileLinkPreparation)throw new InvalidOperationException("Return to the original company, account and order to reconcile preparation.");
        var intent=CompanyInventorySourceReview.Copy(linkPreparationIntent!);var original=company.Snapshot;var version=Version;var generation=linkPreparationGeneration;
        LinkPreparationBusy=true;linkPreparationReview=null;linkPreparationResult=null;ClearInventoryLink();LinkPreparationChanged?.Invoke();
        bool Current()=>!disposed && !WarehouseWorkBlocked && !LinkCycleBlocked && version==Version && generation==linkPreparationGeneration && original==company.Snapshot;
        try
        {
            var result=CompanyInventorySourceReview.Copy(await record(CompanyInventorySourceReview.Copy(intent)) ?? throw new InvalidOperationException("Preparation outcome is unavailable."));intent.RequireResult(result);if(!Current())return;
            PurchaseOrderSourceReview? fresh=null;using var staged=new PurchaseOrderWorkspaceState(company);
            await staged.LoadDetailAsync(intent.Command.OrderId,async id=>{fresh=CompanyInventorySourceReview.Copy(await read(id) ?? throw new InvalidOperationException("The preparation read-back is unavailable."));return fresh;});
            if(!Current())return;
            var stored=fresh!.Lines.Single(row=>row.PO_LINE_ITEM_ID==intent.Command.LineId);intent.RequireResult(stored);
            if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("Recorded preparation read-back changed.");
            var originalSource=PurchaseOrderSourceReview.Create(intent.Command.Company,intent.Command.ActorUserId,fresh.Header,
                fresh.Lines.Select(row=>row.PO_LINE_ITEM_ID==intent.Command.LineId?intent.Review.Purchase.Lines.Single(line=>line.PO_LINE_ITEM_ID==intent.Command.LineId):row).ToArray(),fresh.Receipts);
            if(originalSource.Revision!=intent.Review.Purchase.Revision)throw new InvalidOperationException("Original purchase source changed during preparation confirmation.");
            Detail=staged.Detail;Lines=staged.Lines;Receipts=staged.Receipts;SourceRevision=staged.SourceRevision;RetrievedAtUtc=staged.RetrievedAtUtc;Rows=null;
            linkPreparationResult=result;linkPreparationConfirmed=true;LinkTargets=null;
        }
        catch{if(Current()){linkPreparationResult=null;linkPreparationReview=null;}throw;}
        finally{LinkPreparationBusy=false;LinkPreparationChanged?.Invoke();}
    }
    public async Task ReconcileLinkPreparationResolutionAsync(Func<PurchaseOrderInventoryLinkProposalSubmission,Task<PurchaseOrderInventoryLinkResolution>> read)
    {
        if(!CanReconcileLinkPreparation)throw new InvalidOperationException("Return to the original preparer company, account and order to review resolution.");
        var intent=CompanyInventorySourceReview.Copy(linkPreparationIntent!);var original=company.Snapshot;var version=Version;var generation=linkPreparationGeneration;
        LinkPreparationBusy=true;linkPreparationReview=null;linkPreparationResult=null;linkPreparationResolution=null;LinkPreparationChanged?.Invoke();
        try
        {
            var resolution=CompanyInventorySourceReview.Copy(await read(CompanyInventorySourceReview.Copy(intent)) ?? throw new InvalidOperationException("Independent resolution is unavailable."));resolution.Require(intent);
            if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || version!=Version || generation!=linkPreparationGeneration || original!=company.Snapshot)return;
            using var staged=new PurchaseOrderWorkspaceState(company);await staged.LoadDetailAsync(intent.Command.OrderId,_=>Task.FromResult<PurchaseOrderSourceReview?>(resolution.Purchase));
            if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || version!=Version || generation!=linkPreparationGeneration || original!=company.Snapshot)return;
            Detail=staged.Detail;Lines=staged.Lines;Receipts=staged.Receipts;SourceRevision=staged.SourceRevision;RetrievedAtUtc=staged.RetrievedAtUtc;Rows=null;
            linkPreparationResolution=resolution;linkPreparationConfirmed=true;LinkTargets=null;
        }
        finally{LinkPreparationBusy=false;LinkPreparationChanged?.Invoke();}
    }
}
