using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class InventoryService
{
    private sealed class StockIntentPages(InventoryService owner,string connection):PPDMGenericRepository(owner._editor,owner._commonColumnHandler,owner._defaults,owner._metadata,typeof(INVENTORY_STOCK_INTENT),connection,"INVENTORY_STOCK_INTENT")
    {
        public async Task<(INVENTORY_STOCK_INTENT[] Entries,int Total)> ReadAsync(CompanyScope company,string actor,int page,int size)
        {
            var source=owner._editor.GetDataSource(connection) ??throw new InvalidOperationException("Original stock page datasource is unavailable.");
            var raw=await PPDMDataSourceTransaction.WithSourceAsync(source,()=>{
                var result=source.GetEntity("INVENTORY_STOCK_INTENT",[
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="ROW_CREATED_BY",Operator="=",FilterValue=actor},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}],page,size);
                BeepOperationResult.EnsureSuccess(source.ErrorObject,"Paged read","INVENTORY_STOCK_INTENT");return Task.FromResult(result);
            });
            if(raw is null || raw.PageNumber!=page || raw.PageSize!=size || raw.TotalRecords<0)throw new InvalidOperationException("Original stock provider page metadata is unavailable.");
            var totalPages=raw.TotalRecords==0?0:(int)(((long)raw.TotalRecords+size-1)/size);
            if(raw.TotalPages!=totalPages || raw.HasNextPage!=((long)page*size<raw.TotalRecords) || raw.HasPreviousPage!=(page>1) || page>Math.Max(1,totalPages))
                throw new InvalidOperationException("Original stock provider page counts are unconfirmed.");
            var rows=ConvertToTypedList(raw.Data,typeof(INVENTORY_STOCK_INTENT)).Cast<INVENTORY_STOCK_INTENT>().OrderBy(row=>row.INVENTORY_STOCK_INTENT_ID,StringComparer.Ordinal).ToArray();
            var expected=(int)Math.Min(size,Math.Max(0L,raw.TotalRecords-(long)(page-1)*size));
            if(rows.Length!=expected || rows.Any(row=>row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.ROW_CREATED_BY!=actor || row.ACTIVE_IND!="Y") || rows.Select(row=>row.INVENTORY_STOCK_INTENT_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)
                throw new InvalidOperationException("Complete owned stock page membership is unconfirmed.");
            return(rows,raw.TotalRecords);
        }
    }
    public async Task<PurchaseReceiptStockIntentPage> GetStockRecoveryPageAsync(CompanyScope company,string actor,int page,int size,PurchaseOrderService orders,Func<Task> requireAuthority)
    {
        PurchaseReceiptStockIntentPage.RequireRequest(page,size);PurchaseOrderOwnership.RequireScope(company);ArgumentNullException.ThrowIfNull(orders);ArgumentNullException.ThrowIfNull(requireAuthority);
        if(string.IsNullOrWhiteSpace(actor) || _resolveConnection is null)throw new InvalidOperationException("Original stock page account and resolver are required.");
        await requireAuthority();var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("Original stock page binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("Original stock page datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original stock page binding changed.");}
        async Task<(INVENTORY_STOCK_INTENT[] Entries,int Total)> Entries(){await Guard();_=StockIntentRepository(connection);var result=await new StockIntentPages(this,connection).ReadAsync(company,actor,page,size);await Guard();return result;}
        async Task<PurchaseReceiptStockIntentPage> Read()
        {
            var first=await Entries();var recoveries=new List<PurchaseReceiptStockRecovery>();
            foreach(var entry in first.Entries)
            {
                var recovery=await GetStockRecoveryAsync(company,entry.INVENTORY_STOCK_INTENT_ID,actor,orders,Guard);
                if(JsonSerializer.Serialize(entry)!=JsonSerializer.Serialize(recovery.Entry))throw new InvalidOperationException("Original stock page entry changed.");recoveries.Add(recovery);
            }
            var closing=await Entries();
            if(first.Total!=closing.Total || JsonSerializer.Serialize(first.Entries)!=JsonSerializer.Serialize(closing.Entries))throw new InvalidOperationException("Original stock page membership or total changed.");
            await Guard();return PurchaseReceiptStockIntentPage.Create(PurchaseReceiptStockIntentIndex.Create(company,actor,recoveries),page,size,first.Total);
        }
        var first=await Read();var final=await Read();await Guard();if(first.Revision!=final.Revision)throw new InvalidOperationException("Original stock page or current source changed.");return final;
    }
    public async Task<PurchaseReceiptStockIntentIndex> GetStockRecoveryIndexAsync(CompanyScope company,string actor,PurchaseOrderService orders,Func<Task> requireAuthority)
    {
        PurchaseOrderOwnership.RequireScope(company);ArgumentNullException.ThrowIfNull(orders);ArgumentNullException.ThrowIfNull(requireAuthority);
        if(string.IsNullOrWhiteSpace(actor) || _resolveConnection is null)throw new InvalidOperationException("Original recovery account and PRODUCTION resolver are required.");
        await requireAuthority();var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("Original recovery binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("Original recovery datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original recovery index binding changed.");}
        async Task<INVENTORY_STOCK_INTENT[]> Entries()
        {
            await Guard();var rows=(await StockIntentRepository(connection).GetAsync([
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="ROW_CREATED_BY",Operator="=",FilterValue=actor},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))?.ToArray();
            if(rows is null || rows.Length>PurchaseReceiptStockIntentIndex.MaximumEntries || rows.Any(row=>row is not INVENTORY_STOCK_INTENT))throw new InvalidOperationException("Complete bounded saved stock intent index is unavailable. No entries are truncated.");
            var entries=rows.Cast<INVENTORY_STOCK_INTENT>().OrderBy(row=>row.INVENTORY_STOCK_INTENT_ID,StringComparer.Ordinal).ToArray();
            if(entries.Any(row=>row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.ROW_CREATED_BY!=actor || row.ACTIVE_IND!="Y") || entries.Select(row=>row.INVENTORY_STOCK_INTENT_ID).Distinct(StringComparer.Ordinal).Count()!=entries.Length)
                throw new InvalidOperationException("Saved stock index ownership or identities are ambiguous.");
            return entries;
        }
        async Task<PurchaseReceiptStockIntentIndex> Read()
        {
            var entries=await Entries();var recoveries=new List<PurchaseReceiptStockRecovery>();
            foreach(var entry in entries)
            {
                var recovery=await GetStockRecoveryAsync(company,entry.INVENTORY_STOCK_INTENT_ID,actor,orders,Guard);
                if(JsonSerializer.Serialize(recovery.Entry)!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("Saved stock intent changed during discovery.");recoveries.Add(recovery);
            }
            if(JsonSerializer.Serialize(entries)!=JsonSerializer.Serialize(await Entries()))throw new InvalidOperationException("Complete saved stock intent membership changed.");
            await Guard();return PurchaseReceiptStockIntentIndex.Create(company,actor,recoveries);
        }
        var first=await Read();var final=await Read();await Guard();if(first.Revision!=final.Revision)throw new InvalidOperationException("Complete saved stock intent index or current source changed.");return final;
    }
    private sealed class StockRecoveryInsertPolicy(INVENTORY_STOCK_INTENT expected,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly INVENTORY_STOCK_INTENT original=CompanyInventorySourceReview.Copy(expected);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();
            if(type!=typeof(INVENTORY_STOCK_INTENT) || table!="INVENTORY_STOCK_INTENT" || operation!=PPDMRowMutation.Insert || previous is not null ||
                actor!=original.ROW_CREATED_BY || entity is not INVENTORY_STOCK_INTENT row || !ReferenceEquals(row,expected))
                throw new InvalidOperationException("Only original append-only stock recovery evidence is authorized.");
            row.ACTIVE_IND=original.ACTIVE_IND;row.ROW_CREATED_BY=original.ROW_CREATED_BY;row.ROW_CHANGED_BY=original.ROW_CHANGED_BY;
            row.ROW_CREATED_DATE=original.ROW_CREATED_DATE;row.ROW_CHANGED_DATE=original.ROW_CHANGED_DATE;
            if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Original stock recovery evidence changed before write.");
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Stock recovery insertion grants no reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Stock recovery insertion grants no reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Stock recovery insertion grants no reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Stock recovery insertion grants no reads.");
    }
    private PPDMGenericRepository StockIntentRepository(string connection)
    {
        const string table="INVENTORY_STOCK_INTENT";
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("Original stock recovery datasource is unavailable.");
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull)
            throw new InvalidOperationException("A single native stock recovery identity is required. Review the PRODUCTION Beep migration.");
        foreach(var (column,size) in new[]{(table+"_ID",32),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("PURCHASE_ORDER_ID",128),
            ("PO_RECEIPT_ID",128),("INVENTORY_ITEM_ID",128),("COMMAND_JSON",16384),("ORIGINAL_PURCHASE_JSON",4194304),
            ("ORIGINAL_ITEM_JSON",16384),("PRIOR_MOVEMENT_MANIFEST_JSON",1048576),("MOVEMENT_REVISION",64),("RECORDED_REVISION",64)})
        {
            var fields=schema!.Fields.Where(field=>field.FieldName==column).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || fields[0].Size1<size)
                throw new InvalidOperationException("Original stock recovery metadata is unavailable. Review the PRODUCTION Beep migration.");
        }
        return new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(INVENTORY_STOCK_INTENT),connection,table);
    }
    public async Task<PurchaseReceiptStockRecovery> GetStockRecoveryAsync(CompanyScope company,string intentId,string actor,PurchaseOrderService orders,Func<Task> requireAuthority)
    {
        PurchaseOrderOwnership.RequireScope(company);ArgumentNullException.ThrowIfNull(orders);ArgumentNullException.ThrowIfNull(requireAuthority);
        if(string.IsNullOrWhiteSpace(actor) || !Guid.TryParseExact(intentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=intentId || _resolveConnection is null)
            throw new InvalidOperationException("Original stock recovery account, intent and PRODUCTION resolver are required.");
        await requireAuthority();var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("Original stock recovery binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("Original stock recovery source is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original stock recovery binding changed.");}
        async Task<PurchaseReceiptStockRecovery> Read()
        {
            await Guard();var repo=StockIntentRepository(connection);
            var rows=(await repo.GetAsync([new(){FieldName="INVENTORY_STOCK_INTENT_ID",Operator="=",FilterValue=intentId}]))?.ToArray();
            if(rows is not {Length:1} || rows[0] is not INVENTORY_STOCK_INTENT entry)throw new InvalidOperationException("Original stock recovery entry is unavailable or ambiguous.");
            if(entry.OWNER_TENANT_ID!=company.TenantId || entry.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || entry.ROW_CREATED_BY!=actor)
                throw new InvalidOperationException("The original stock recovery entry does not establish this company and account.");
            var purchase=await orders.GetSourceReviewAsync(company,entry.PURCHASE_ORDER_ID,actor);
            var inventory=await GetCompanyInventorySourceAsync(company,entry.INVENTORY_ITEM_ID,actor,true);
            List<INVENTORY_TRANSACTION>? history=null;
            if(inventory is null)
            {
                history=[];var movements=await GetRepoAsync<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION",true);
                foreach(var id in entry.RequiredMovementIds(company,actor))
                {
                    await Guard();var retained=(await movements.GetAsync([new(){FieldName="INVENTORY_TRANSACTION_ID",Operator="=",FilterValue=id}]))?.ToArray();
                    if(retained is not {Length:1} || retained[0] is not INVENTORY_TRANSACTION movement || movement.INVENTORY_ITEM_ID!=entry.INVENTORY_ITEM_ID)throw new InvalidOperationException("Original native historical movement is unavailable or ambiguous.");
                    history.Add(CompanyInventorySourceReview.Copy(movement));
                }
            }
            var closingPurchase=await orders.GetSourceReviewAsync(company,entry.PURCHASE_ORDER_ID,actor);
            if(closingPurchase?.Revision!=purchase?.Revision)throw new InvalidOperationException("Current purchase changed during stock recovery.");
            var closingInventory=await GetCompanyInventorySourceAsync(company,entry.INVENTORY_ITEM_ID,actor,true);
            if(closingInventory?.Revision!=inventory?.Revision)throw new InvalidOperationException("Current inventory availability or source changed during stock recovery.");
            var closing=(await repo.GetAsync([new(){FieldName="INVENTORY_STOCK_INTENT_ID",Operator="=",FilterValue=intentId}]))?.ToArray();
            if(closing is not {Length:1} || JsonSerializer.Serialize(closing[0])!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("Original stock recovery entry changed.");
            var result=PurchaseReceiptStockRecovery.Create(entry,purchase,inventory,history);await Guard();return result;
        }
        var first=await Read();var final=await Read();await Guard();if(first.Revision!=final.Revision)throw new InvalidOperationException("Complete original stock recovery or current source changed.");
        return final;
    }
}
