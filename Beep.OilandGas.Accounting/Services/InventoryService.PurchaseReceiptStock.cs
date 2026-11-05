using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;
public partial class InventoryService
{
    public async Task<PurchaseReceiptInventoryReview> RecordPurchaseReceiptStockAsync(PurchaseReceiptStockSubmission submission,PurchaseOrderService orders,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(orders);ArgumentNullException.ThrowIfNull(requireAuthority);
        submission=CompanyInventorySourceReview.Copy(submission);submission.Require();var command=submission.Command;await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original PRODUCTION stock binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original stock datasource is unavailable.");
        async Task Guard(bool owned)
        {
            await requireAuthority();if(string.IsNullOrWhiteSpace(connection) || await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))
                throw new InvalidOperationException("Original stock binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        async Task<PurchaseReceiptInventoryReview> Read(bool owned)
        {
            await Guard(owned);RequireCompanySourceSchema(connection,"INVENTORY_ITEM");RequireCompanySourceSchema(connection,"INVENTORY_TRANSACTION",true);
            var schema=source.GetEntityStructure(new EntityStructure{EntityName="INVENTORY_ITEM",DatasourceEntityName="INVENTORY_ITEM"},true);
            var quantity=schema?.Fields?.Where(value=>value.FieldName=="QUANTITY_ON_HAND").ToArray();
            if(quantity is not {Length:1} || quantity[0].Fieldtype!=typeof(decimal).FullName)throw new InvalidOperationException("Native decimal stock quantity metadata is required.");
            var purchase=await orders.GetSourceReviewAsync(command.Company,command.OrderId,command.ActorUserId) ??throw new InvalidOperationException("Original receipt order is unavailable.");
            var itemId=submission.Review.Source.Inventory.Item.INVENTORY_ITEM_ID;
            var inventory=await GetReceiptStockInventoryAsync(purchase,command.ReceiptId);
            if(inventory.Item.INVENTORY_ITEM_ID!=itemId)throw new InvalidOperationException("The original physical receipt destination changed.");
            var finalPurchase=await orders.GetSourceReviewAsync(command.Company,command.OrderId,command.ActorUserId);
            if(finalPurchase?.Revision!=purchase.Revision)throw new InvalidOperationException("Original purchase changed while reading stock evidence.");
            var items=await GetRepoAsync<INVENTORY_ITEM>("INVENTORY_ITEM");
            var unique=(await items.GetAsync([new(){FieldName="INVENTORY_ITEM_ID",Operator="=",FilterValue=itemId}]))?.ToArray();
            if(unique is not {Length:1} || unique[0] is not INVENTORY_ITEM || JsonSerializer.Serialize(unique[0])!=JsonSerializer.Serialize(inventory.Item))
                throw new InvalidOperationException("Original stock item identity is ambiguous.");
            var transactions=await GetRepoAsync<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION",true);
            var byIntent=(await transactions.GetAsync([new(){FieldName="INVENTORY_TRANSACTION_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
            var recorded=inventory.Transactions.SingleOrDefault(value=>value.INVENTORY_TRANSACTION_ID==command.IntentId);
            if(recorded is null?byIntent is not {Length:0}:byIntent is not {Length:1} || JsonSerializer.Serialize(byIntent[0])!=JsonSerializer.Serialize(recorded))
                throw new InvalidOperationException("Original stock intent is used or ambiguous.");
            var result=PurchaseReceiptInventoryReview.Create(purchase,inventory,command.ReceiptId,command.BindingRevision);
            if(recorded is null)
            {
                var fresh=PurchaseReceiptStockReview.Create(result);command.Require(fresh);
                if(JsonSerializer.Serialize(result.Inventory.Item)!=JsonSerializer.Serialize(submission.Review.Source.Inventory.Item) ||
                    JsonSerializer.Serialize(result.Inventory.Transactions)!=JsonSerializer.Serialize(submission.Review.Source.Inventory.Transactions))
                    throw new InvalidOperationException("Original reviewed stock preimage changed.");
            }
            else submission.RequireResult(result);
            var recoveryRows=(await StockIntentRepository(connection).GetAsync([new(){FieldName="INVENTORY_STOCK_INTENT_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
            if(recorded is null)
            {
                if(recoveryRows is not {Length:0})throw new InvalidOperationException("Original stock recovery intent is already used.");
            }
            else
            {
                var expected=INVENTORY_STOCK_INTENT.Create(submission,recorded);
                if(recoveryRows is not {Length:1} || recoveryRows[0] is not INVENTORY_STOCK_INTENT entry || JsonSerializer.Serialize(entry)!=JsonSerializer.Serialize(expected))
                    throw new InvalidOperationException("Exact original stock recovery evidence is unavailable or ambiguous.");
                _=entry.Restore(command.Company,command.ActorUserId,inventory.Transactions);
            }
            await Guard(owned);return result;
        }
        PurchaseReceiptInventoryReview? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            var current=await Read(true);
            if(!current.Inventory.Transactions.Any(value=>value.INVENTORY_TRANSACTION_ID==command.IntentId))
            {
                var utc=DateTime.UtcNow;
                var row=new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID=command.IntentId,INVENTORY_ITEM_ID=current.Inventory.Item.INVENTORY_ITEM_ID,
                    TRANSACTION_TYPE=command.Kind==PurchaseReceiptStockMovementKind.Receive?"PURCHASE_RECEIPT":"SUPPLIER_RETURN",
                    QUANTITY=command.Kind==PurchaseReceiptStockMovementKind.Receive?command.Quantity:-command.Quantity,TRANSACTION_DATE=utc,
                    REFERENCE_NUMBER=command.ReceiptId,NOTES=command.Reason,ACTIVE_IND="Y",ROW_CREATED_BY=command.ActorUserId,ROW_CHANGED_BY=command.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};
                var movementWriter=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(INVENTORY_TRANSACTION),connection,"INVENTORY_TRANSACTION",
                    rowAccess:new ReceiptStockPolicy(submission,current.Inventory.Item,row,true,()=>Guard(true)));
                await movementWriter.InsertAsync(row,command.ActorUserId);await Guard(true);
                var reader=await GetRepoAsync<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION",true);
                var inserted=(await reader.GetAsync([new(){FieldName="INVENTORY_TRANSACTION_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
                if(inserted is not {Length:1} || JsonSerializer.Serialize(inserted[0])!=JsonSerializer.Serialize(row))
                    throw new InvalidOperationException("Exact original stock movement was not stored uniquely.");
                await Guard(true);
                var recovery=INVENTORY_STOCK_INTENT.Create(submission,row);
                var recoveryWriter=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(INVENTORY_STOCK_INTENT),connection,"INVENTORY_STOCK_INTENT",
                    rowAccess:new StockRecoveryInsertPolicy(recovery,()=>Guard(true)));
                await recoveryWriter.InsertAsync(recovery,command.ActorUserId);await Guard(true);
                var recoveryRows=(await StockIntentRepository(connection).GetAsync([new(){FieldName="INVENTORY_STOCK_INTENT_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
                if(recoveryRows is not {Length:1} || JsonSerializer.Serialize(recoveryRows[0])!=JsonSerializer.Serialize(recovery))
                    throw new InvalidOperationException("Exact original stock recovery evidence was not stored uniquely.");
                await Guard(true);
                var itemWriter=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(INVENTORY_ITEM),connection,"INVENTORY_ITEM",
                    rowAccess:new ReceiptStockPolicy(submission,current.Inventory.Item,row,false,()=>Guard(true)));
                await itemWriter.UpdateAsync(CompanyInventorySourceReview.Copy(current.Inventory.Item),command.ActorUserId);
            }
            confirmed=await Read(true);submission.RequireResult(confirmed);await Guard(true);
        });
        var durable=await Read(false);submission.RequireResult(durable);
        if(confirmed is null || confirmed.Revision!=durable.Revision)throw new InvalidOperationException("Committed stock result is unconfirmed. Reconcile the original intent.");
        return CompanyInventorySourceReview.Copy(durable);
    }
    private sealed class ReceiptStockPolicy(PurchaseReceiptStockSubmission submission,INVENTORY_ITEM original,INVENTORY_TRANSACTION movement,bool inserting,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private async Task Table(Type type,string table)
        {
            await guard();if(type!=(inserting?typeof(INVENTORY_TRANSACTION):typeof(INVENTORY_ITEM)) || table!=(inserting?"INVENTORY_TRANSACTION":"INVENTORY_ITEM"))
                throw new InvalidOperationException("Stock policy is restricted to the original movement or item.");
        }
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await Table(type,table);var command=submission.Command;
            if(actor!=command.ActorUserId)throw new InvalidOperationException("Original stock actor changed.");
            if(inserting)
            {
                if(operation!=PPDMRowMutation.Insert || previous is not null || entity is not INVENTORY_TRANSACTION row || !ReferenceEquals(row,movement) ||
                    row.INVENTORY_TRANSACTION_ID!=command.IntentId || row.INVENTORY_ITEM_ID!=original.INVENTORY_ITEM_ID || row.UNIT_COST is not null || row.TOTAL_COST is not null ||
                    row.REFERENCE_NUMBER!=command.ReceiptId || row.NOTES!=command.Reason)
                    throw new InvalidOperationException("Only the original physical stock movement can be appended.");
                row.ACTIVE_IND="Y";row.ROW_CREATED_BY=command.ActorUserId;row.ROW_CHANGED_BY=command.ActorUserId;
                row.ROW_CHANGED_DATE=row.ROW_CREATED_DATE=row.TRANSACTION_DATE;
                var proof=command.Movement(submission.Review,row);proof.Require(command.Company,row);
                row.PURCHASE_RECEIPT_STOCK_JSON=JsonSerializer.Serialize(proof);_=PurchaseReceiptStockMovement.Read(command.Company,row);
            }
            else
            {
                if(operation!=PPDMRowMutation.Update || entity is not INVENTORY_ITEM row || previous is not INVENTORY_ITEM before ||
                    JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Only the original reviewed stock item can change.");
                var expected=command.ResultItem(submission.Review,movement);row.QUANTITY_ON_HAND=expected.QUANTITY_ON_HAND;row.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;row.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;
                if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Stock mutation changed unapproved item fields.");
            }
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await Table(type,table);if(inserting)throw new InvalidOperationException("Movement policy grants no reads.");return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="INVENTORY_ITEM_ID",Operator="=",FilterValue=original.INVENTORY_ITEM_ID}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await Table(type,table);if(inserting)throw new InvalidOperationException("Movement policy grants no reads.");return new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=submission.Command.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=submission.Command.Company.BusinessAssociateId}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await Table(type,table);if(inserting || rows.Count!=1 || rows[0] is not INVENTORY_ITEM || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Original stock item changed before write.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Stock policy grants no reads.");
    }
}
