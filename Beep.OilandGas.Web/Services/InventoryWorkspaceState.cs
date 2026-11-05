using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed partial class InventoryWorkspaceState:IDisposable
{
    public enum EvidenceQueue { All, MissingLocation, UnknownQuantity, NonpositiveQuantity }
    private bool disposed;
    private readonly CompanyClientContext company;
    public InventoryWorkspaceState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public int Version {get;private set;}
    public IReadOnlyList<Item>? Rows {get;private set;}
    public Item? Detail {get;private set;}
    public IReadOnlyList<Movement>? Movements {get;private set;}
    public string? Revision {get;private set;}
    public DateTimeOffset? RetrievedAtUtc {get;private set;}
    public string? WarehouseFilter {get;private set;}
    public EvidenceQueue Queue {get;private set;}
    public IReadOnlyList<string> Warehouses=>Rows?.Where(row=>row.Warehouse is not null).Select(row=>row.Warehouse!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray() ?? [];
    private IEnumerable<Item>? WarehouseRows=>Rows?.Where(row=>WarehouseFilter is null || row.Warehouse==WarehouseFilter);
    public IReadOnlyList<Item>? VisibleRows=>WarehouseRows is null?null:Array.AsReadOnly(WarehouseRows.Where(row=>Matches(row,Queue)).ToArray());
    public int? Count(EvidenceQueue queue)=>WarehouseRows?.Count(row=>Matches(row,queue));
    private static bool Matches(Item row,EvidenceQueue queue)=>queue switch
    {
        EvidenceQueue.All=>true,
        EvidenceQueue.MissingLocation=>row.Warehouse is null || row.Location is null,
        EvidenceQueue.UnknownQuantity=>row.Quantity is null,
        EvidenceQueue.NonpositiveQuantity=>row.Quantity is <=0,
        _=>throw new ArgumentOutOfRangeException(nameof(queue))
    };
    public void SetQueue(EvidenceQueue queue){if(!Enum.IsDefined(queue))throw new ArgumentOutOfRangeException(nameof(queue));Queue=queue;}
    public void SetWarehouse(string? warehouse)
    {
        if(warehouse is not null && !Warehouses.Contains(warehouse,StringComparer.Ordinal))throw new InvalidOperationException("The warehouse is outside this recorded company queue.");
        WarehouseFilter=warehouse;Queue=EvidenceQueue.All;
    }
    public void Clear(){++Version;Rows=null;Detail=null;Movements=null;Revision=null;RetrievedAtUtc=null;WarehouseFilter=null;Queue=EvidenceQueue.All;ClearStockDisplay();ClearReservations();}
    public async Task LoadQueueAsync(Func<Task<List<INVENTORY_ITEM>>> read)
    {
        Clear();var version=Version;var original=company.Snapshot;if(disposed || original.Selection is null)return;
        var rows=await read();if(disposed || version!=Version || original!=company.Snapshot)return;
        CompanyInventorySourceReview.RequireRows(original.Selection.Scope,rows);
        Rows=Array.AsReadOnly(rows.OrderBy(row=>row.INVENTORY_ITEM_ID,StringComparer.Ordinal).Select(Project).ToArray());RetrievedAtUtc=DateTimeOffset.UtcNow;
    }
    public async Task LoadDetailAsync(string id,Func<string,Task<CompanyInventorySourceReview?>> read)
    {
        Clear();var version=Version;var original=company.Snapshot;if(disposed || original.Selection is null)return;
        var source=await read(id);if(disposed || version!=Version || original!=company.Snapshot)return;
        if(source is null)throw new InvalidOperationException("The company inventory source is unavailable.");
        source.Require(original.Selection.Scope,original.Selection.UserId,id);
        PublishDetail(source);
    }
    private void PublishDetail(CompanyInventorySourceReview source)
    {
        Detail=Project(source.Item);Movements=Array.AsReadOnly(source.Transactions.Select(row=>new Movement(row.INVENTORY_TRANSACTION_ID,row.TRANSACTION_TYPE,row.TRANSACTION_DATE,row.QUANTITY,row.UNIT_COST,row.TOTAL_COST,row.REFERENCE_NUMBER,row.NOTES,row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE)).ToArray());
        Revision=source.Revision;RetrievedAtUtc=source.RetrievedAtUtc;
    }
    private static Item Project(INVENTORY_ITEM row)=>new(row.INVENTORY_ITEM_ID,row.ITEM_NUMBER,row.ITEM_NAME,row.WAREHOUSE_ID,row.LOCATION_ID,row.UNIT_OF_MEASURE,row.CURRENCY_CODE,row.QUANTITY_ON_HAND,row.UNIT_COST,row.TOTAL_VALUE,row.VALUATION_METHOD,row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE);
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
    public sealed record Item(string Id,string? Number,string? Name,string? Warehouse,string? Location,string? Unit,string? Currency,decimal? Quantity,decimal? UnitCost,decimal? Value,string? Valuation,string? Active,string? CreatedBy,DateTime? CreatedAt,string? ChangedBy,DateTime? ChangedAt)
    {public string Route=>"/warehouse/items/"+Uri.EscapeDataString(Id);}
    public sealed record Movement(string Id,string? Type,DateTime? Date,decimal? Quantity,decimal? UnitCost,decimal? Cost,string? Reference,string? Notes,string? Active,string? CreatedBy,DateTime? CreatedAt,string? ChangedBy,DateTime? ChangedAt);
}
