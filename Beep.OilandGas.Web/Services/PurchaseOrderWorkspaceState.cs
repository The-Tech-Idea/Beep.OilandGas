using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Read-only UI projection of explicitly owned PO headers; no receipt, stock or approval inference.</summary>
public sealed partial class PurchaseOrderWorkspaceState:IDisposable
{
    private readonly CompanyClientContext company;
    private bool disposed;
    public int Version {get;private set;}
    public IReadOnlyList<Item>? Rows {get;private set;}
    public string? SupplierFilter {get;private set;}
    public string? StatusFilter {get;private set;}
    public IReadOnlyList<string> RecordedStatuses=>Rows?.Where(row=>!string.IsNullOrWhiteSpace(row.Status)).Select(row=>row.Status!).Distinct(StringComparer.Ordinal).OrderBy(value=>value,StringComparer.Ordinal).ToArray() ?? [];
    public IReadOnlyList<Item>? VisibleRows=>Rows is null?null:Array.AsReadOnly(Rows.Where(row=>StatusFilter is null || row.Status==StatusFilter).ToArray());
    public void SetStatusFilter(string? status){if(status is not null && !RecordedStatuses.Contains(status,StringComparer.Ordinal))throw new InvalidOperationException("This status is not in the recorded company order queue.");StatusFilter=status;}
    public Item? Detail {get;private set;}
    public IReadOnlyList<LineItem>? Lines {get;private set;}
    public IReadOnlyList<ReceiptItem>? Receipts {get;private set;}
    public enum ReceivingQueue { All, NeedsDecision, Accepted, Rejected, InactiveOrUnconfirmed }
    public ReceivingQueue ReceiptFilter {get;private set;}
    public IReadOnlyList<ReceiptItem>? VisibleReceipts=>Receipts is null?null:Array.AsReadOnly(Receipts.Where(row=>InQueue(row,ReceiptFilter)).ToArray());
    public int? ReceiptCount(ReceivingQueue queue)=>Receipts?.Count(row=>InQueue(row,queue));
    public void SetReceiptFilter(ReceivingQueue queue)
    {
        if(!Enum.IsDefined(queue))throw new ArgumentOutOfRangeException(nameof(queue));
        ReceiptFilter=queue;
    }
    private static bool InQueue(ReceiptItem row,ReceivingQueue queue)=>queue switch
    {
        ReceivingQueue.All=>true,
        ReceivingQueue.NeedsDecision=>row.ActiveIndicator=="Y" && row.Decision is null,
        ReceivingQueue.Accepted=>row.ActiveIndicator=="Y" && row.Decision?.Accepted>0,
        ReceivingQueue.Rejected=>row.ActiveIndicator=="Y" && row.Decision?.Rejected>0,
        ReceivingQueue.InactiveOrUnconfirmed=>row.ActiveIndicator!="Y",
        _=>throw new ArgumentOutOfRangeException(nameof(queue))
    };
    public string? SourceRevision {get;private set;}
    public DateTimeOffset? RetrievedAtUtc {get;private set;}
    private readonly InventoryWorkspaceState? warehouse;
    public bool WarehouseWorkBlocked=>warehouse?.StockBlocked==true;
    public string? WarehouseRecoveryRoute=>warehouse?.StockRecoveryRoute;
    public bool HasCompetingProcurementWork=>ReceiverBusy || HasUnconfirmedReceiverIntent || InventoryLinkBusy || LinkPreparationBlocked || LinkDecisionBlocked || LinkCycleBlocked;
    public event Action? WarehouseChanged;
    public PurchaseOrderWorkspaceState(CompanyClientContext company,InventoryWorkspaceState? warehouse=null)
    {
        this.company=company;this.warehouse=warehouse;
        if(warehouse is not null){warehouse.AttachProcurement(this,company);warehouse.StockChanged+=WarehouseWorkChanged;}
        company.Changed+=Clear;
    }
    private void WarehouseWorkChanged(){Clear();WarehouseChanged?.Invoke();}
    public void Clear(){ClearLinkCycleDisplay();ClearLinkDecisionDisplay();ClearLinkPreparationDisplay();ClearInventoryLink();ClearReceiverDisplay();++Version;Rows=null;SupplierFilter=null;StatusFilter=null;ReceiptFilter=ReceivingQueue.All;Detail=null;Lines=null;Receipts=null;SourceRevision=null;RetrievedAtUtc=null;}
    public async Task LoadVendorAsync(string vendor,Func<string,Task<List<PURCHASE_ORDER>>> read)
    {
        if(string.IsNullOrWhiteSpace(vendor)){Clear();return;}
        await LoadCompanyAsync(value=>read(value!),vendor);
    }
    public async Task LoadCompanyAsync(Func<string?,Task<List<PURCHASE_ORDER>>> read,string? vendor=null)
    {
        Clear();var version=Version;var scope=company.Snapshot;
        if(disposed || scope.Selection is null)return;
        if(vendor is not null && string.IsNullOrWhiteSpace(vendor))throw new InvalidOperationException("A supplied supplier restriction cannot be empty.");
        var rows=await read(vendor);if(disposed || version!=Version || scope!=company.Snapshot)return;
        PurchaseOrderOwnership.RequireRows(scope.Selection.Scope,rows);
        if(vendor is not null)PurchaseOrderOwnership.RequireVendorRows(scope.Selection.Scope,vendor,rows);
        Rows=Array.AsReadOnly(rows.Select(Project).OrderBy(row=>row.Id,StringComparer.Ordinal).ToArray());
        SupplierFilter=vendor;RetrievedAtUtc=DateTimeOffset.UtcNow;
    }
    public async Task LoadDetailAsync(string id,Func<string,Task<PurchaseOrderSourceReview?>> read)
    {
        Clear();var version=Version;var scope=company.Snapshot;
        if(disposed || scope.Selection is null || string.IsNullOrWhiteSpace(id))return;
        var row=await read(id);if(disposed || version!=Version || scope!=company.Snapshot)return;
        if(row is null)throw new InvalidOperationException("The purchase-order source review was empty.");
        row.Require(scope.Selection.Scope,scope.Selection.UserId,id);Detail=Project(row.Header);
        Lines=Array.AsReadOnly(row.Lines.Select(line=>new LineItem(line.PO_LINE_ITEM_ID,line.LINE_NUMBER,line.ITEM_DESCRIPTION,line.QUANTITY,line.UNIT_PRICE,line.LINE_TOTAL,line.ACTIVE_IND,line.QUANTITY_UOM,line.INVENTORY_ITEM_ID,Preparation(row.Company,line),line.INVENTORY_LINK_REVIEW_JSON is not null,LinkDecision(row.Company,line),line.INVENTORY_LINK_DECISION_JSON is not null)).ToArray());
        var lines=row.Lines.ToDictionary(line=>line.PO_LINE_ITEM_ID,StringComparer.Ordinal);
        Receipts=Array.AsReadOnly(row.Receipts.Select(receipt=>new ReceiptItem(receipt.PO_RECEIPT_ID,receipt.PO_LINE_ITEM_ID,lines[receipt.PO_LINE_ITEM_ID].LINE_NUMBER,
            receipt.RECEIPT_DATE,receipt.RECEIVED_QUANTITY,receipt.RECEIVED_BY,receipt.RECEIPT_STATUS,receipt.DESCRIPTION,receipt.ACTIVE_IND,
            receipt.ROW_CREATED_BY,receipt.ROW_CREATED_DATE,receipt.ROW_CHANGED_BY,receipt.ROW_CHANGED_DATE,receipt.QUANTITY_UOM,
            Decision(row.Company,receipt))).ToArray());
        SourceRevision=row.Revision;RetrievedAtUtc=row.RetrievedAtUtc;
    }
    private static Item Project(PURCHASE_ORDER row)=>new(row.PURCHASE_ORDER_ID,row.PO_NUMBER,row.VENDOR_BA_ID,row.PO_DATE,row.EXPECTED_DELIVERY_DATE,row.STATUS,row.DESCRIPTION,row.TOTAL_AMOUNT,row.ROW_CREATED_BY,row.CURRENCY_CODE);
    public void Dispose(){disposed=true;company.Changed-=Clear;if(warehouse is not null){warehouse.StockChanged-=WarehouseWorkChanged;warehouse.DetachProcurement(this);}Clear();}
    public sealed record LineItem(string Id,int? Number,string? Description,decimal? Quantity,decimal? UnitPrice,decimal? LineTotal,string? ActiveIndicator,string? QuantityUnit,string? InventoryItemId=null,LinkPreparationSummary? Preparation=null,bool HasPreparationEvidence=false,LinkDecisionSummary? InventoryDecision=null,bool HasDecisionEvidence=false);
    public sealed record LinkDecisionSummary(PurchaseOrderInventoryLinkDecision Decision,string Target,string Actor,DateTime RecordedAt,string Reason,string Intent);
    private static LinkDecisionSummary? LinkDecision(CompanyScope scope,PO_LINE_ITEM line)
    {
        if(line.INVENTORY_LINK_DECISION_JSON is null)return null;
        try{var proof=PurchaseOrderInventoryLinkDecisionProof.Require(scope,line);return new(proof.Command.Decision,proof.Command.TargetItemId,proof.Command.ActorUserId,line.ROW_CHANGED_DATE!.Value,proof.Command.Reason,proof.Command.IntentId);}
        catch(InvalidOperationException){return null;}
    }
    public sealed record LinkPreparationSummary(string Target,string Actor,DateTime RecordedAt,string Reason,string Intent);
    private static LinkPreparationSummary? Preparation(CompanyScope scope,PO_LINE_ITEM line)
    {
        if(line.INVENTORY_LINK_REVIEW_JSON is null)return null;
        try{var proof=PurchaseOrderInventoryLinkProposalProof.Require(scope,line);var original=line.INVENTORY_LINK_DECISION_JSON is null?line:PurchaseOrderInventoryLinkDecisionProof.Require(scope,line).Review.Purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);return new(proof.Command.TargetItemId,proof.Command.ActorUserId,original.ROW_CHANGED_DATE!.Value,proof.Command.Reason,proof.Command.IntentId);}
        catch(InvalidOperationException){return null;}
    }
    private static ReceiverDecision? Decision(CompanyScope scope,PO_RECEIPT row)
    {
        try{var proof=PurchaseReceiptDecisionProof.Require(scope,row);return new(proof.Command.AcceptedQuantity,proof.Command.RejectedQuantity,proof.Command.ActorUserId,row.ROW_CHANGED_DATE!.Value,proof.Command.Reason);}
        catch(InvalidOperationException){return null;}
    }
    public sealed record ReceiverDecision(decimal Accepted,decimal Rejected,string Actor,DateTime RecordedAt,string Reason);
    public sealed record ReceiptItem(string Id,string LineId,int? LineNumber,DateTime? ReceiptDate,decimal? Quantity,string? ReceivedBy,string? Status,string? Description,string? ActiveIndicator,string? RecordedBy,DateTime? RecordedAt,string? ChangedBy,DateTime? ChangedAt,string? QuantityUnit,ReceiverDecision? Decision=null);
    public sealed record Item(string Id,string? Number,string? VendorId,DateTime? OrderDate,DateTime? ExpectedDeliveryDate,string? Status,string? Description,decimal RecordedAmount,string? RecordedBy,string? CurrencyCode)
    {public string Route=>"/procurement/orders/"+Uri.EscapeDataString(Id);}
}
