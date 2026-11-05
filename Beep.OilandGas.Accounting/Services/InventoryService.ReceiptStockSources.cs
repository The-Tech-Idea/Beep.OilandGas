using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class InventoryService
{
    public async Task<CompanyInventorySourceReview> GetReceiptStockInventoryAsync(PurchaseOrderSourceReview purchase,string receiptId)
    {
        purchase.Require(purchase.Company,purchase.ActorUserId,purchase.Header.PURCHASE_ORDER_ID);
        if(!purchase.Receipts.Any(row=>row.PO_RECEIPT_ID==receiptId))throw new InvalidOperationException("The receipt is outside this owned order.");
        if(_resolveConnection is null)throw new InvalidOperationException("The physical receipt PRODUCTION binding is unavailable.");
        var connection=await _resolveConnection();
        if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The physical receipt PRODUCTION binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The physical receipt datasource is unavailable.");
        async Task Guard(){if(await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The physical receipt datasource binding changed.");}
        async Task<INVENTORY_TRANSACTION[]> ReceiptRows()
        {
            await Guard();
            var repo=await GetRepoAsync<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION",true);
            var raw=(await repo.GetAsync([new(){FieldName="REFERENCE_NUMBER",Operator="=",FilterValue=receiptId}]))?.ToArray();
            if(raw is null || raw.Any(row=>row is not INVENTORY_TRANSACTION))throw new InvalidOperationException("Complete physical receipt history is unavailable.");
            return raw.Cast<INVENTORY_TRANSACTION>().OrderBy(row=>row.INVENTORY_TRANSACTION_ID,StringComparer.Ordinal).Select(CompanyInventorySourceReview.Copy).ToArray();
        }
        var rows=await ReceiptRows();var itemId=PurchaseReceiptInventoryReview.ResolveStockItemId(purchase,receiptId,rows);
        var inventory=await GetCompanyInventorySourceAsync(purchase.Company,itemId,purchase.ActorUserId,true)
            ??throw new InvalidOperationException("The recorded physical receipt inventory destination is unavailable.");
        var relevant=inventory.Transactions.Where(row=>row.REFERENCE_NUMBER==receiptId).OrderBy(row=>row.INVENTORY_TRANSACTION_ID,StringComparer.Ordinal).ToArray();
        async Task<object[]?> SavedRows(){await Guard();return (await StockIntentRepository(connection).GetAsync([
            new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=purchase.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=purchase.Company.BusinessAssociateId},
            new(){FieldName="PURCHASE_ORDER_ID",Operator="=",FilterValue=purchase.Header.PURCHASE_ORDER_ID},new(){FieldName="PO_RECEIPT_ID",Operator="=",FilterValue=receiptId}]))?.OrderBy(row=>(row as INVENTORY_STOCK_INTENT)?.INVENTORY_STOCK_INTENT_ID,StringComparer.Ordinal).Select(row=>row is INVENTORY_STOCK_INTENT entry?(object)CompanyInventorySourceReview.Copy(entry):row).ToArray();}
        var saved=await SavedRows();
        if(saved is null || saved.Any(row=>row is not INVENTORY_STOCK_INTENT) || saved.Length!=rows.Length ||
            !saved.Cast<INVENTORY_STOCK_INTENT>().Select(row=>row.INVENTORY_STOCK_INTENT_ID).Order(StringComparer.Ordinal).SequenceEqual(rows.Select(row=>row.INVENTORY_TRANSACTION_ID)))
            throw new InvalidOperationException("Complete recorded physical receipt intent membership is unconfirmed.");
        foreach(var entry in saved.Cast<INVENTORY_STOCK_INTENT>())
        {
            var original=entry.Restore(purchase.Company,entry.ROW_CREATED_BY,inventory.Transactions);
            if(original.Command.OrderId!=purchase.Header.PURCHASE_ORDER_ID || original.Command.ReceiptId!=receiptId || original.Review.Source.Inventory.Item.INVENTORY_ITEM_ID!=itemId)
                throw new InvalidOperationException("Recorded physical receipt evidence disagrees with its original destination.");
        }
        if(System.Text.Json.JsonSerializer.Serialize(rows)!=System.Text.Json.JsonSerializer.Serialize(relevant) ||
            System.Text.Json.JsonSerializer.Serialize(rows)!=System.Text.Json.JsonSerializer.Serialize(await ReceiptRows()) ||
            System.Text.Json.JsonSerializer.Serialize(saved)!=System.Text.Json.JsonSerializer.Serialize(await SavedRows()))
            throw new InvalidOperationException("Complete physical receipt membership changed while reading its destination.");
        await Guard();
        return inventory;
    }
}
