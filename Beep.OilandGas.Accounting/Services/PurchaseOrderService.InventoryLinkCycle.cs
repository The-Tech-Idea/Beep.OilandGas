using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class PurchaseOrderService
{
    public async Task<PurchaseOrderInventoryLinkCycleResult> ReopenInventoryLinkAsync(PurchaseOrderInventoryLinkCycleCommand command,PurchaseOrderInventoryLinkCycleReview review,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(review);ArgumentNullException.ThrowIfNull(requireAuthority);
        command=CompanyInventorySourceReview.Copy(command);review=CompanyInventorySourceReview.Copy(review);command.Require(review);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original cycle binding is unavailable.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The original cycle datasource is unavailable.");
        async Task Guard(bool owned)
        {
            await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The original cycle binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        async Task<(PurchaseOrderInventoryLinkCycleResult Result,bool Recorded)> Capture(bool owned)
        {
            await Guard(owned);
            var columns=source.GetEntityStructure(new EntityStructure{EntityName="PO_LINE_ITEM",DatasourceEntityName="PO_LINE_ITEM"},true);
            foreach(var required in new[]{("INVENTORY_LINK_REVIEW_JSON",PurchaseOrderInventoryLinkProposalProof.MaximumLength),("INVENTORY_LINK_DECISION_JSON",PurchaseOrderInventoryLinkDecisionProof.MaximumLength)})
            {
                var fields=columns?.Fields?.Where(field=>field.FieldName==required.Item1).ToArray();
                if(fields is not {Length:1} || !fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || fields[0].Size<required.Item2)throw new InvalidOperationException("The active cycle proof columns require reviewed Beep migration metadata.");
            }
            var history=await GetInventoryLinkHistoryAsync(command.Company,command.OrderId,command.LineId,command.ActorUserId,()=>Guard(owned));
            var purchase=await GetSourceReviewAsync(command.Company,command.OrderId,command.ActorUserId) ?? throw new InvalidOperationException("The original owned order is unavailable.");
            var line=purchase.Lines.Single(value=>value.PO_LINE_ITEM_ID==command.LineId);history.Require(command.Company,command.ActorUserId,command.OrderId,command.LineId,line.INVENTORY_LINK_HISTORY_REVISION);
            var lines=await GetRepoAsync<PO_LINE_ITEM>("PO_LINE_ITEM");
            var unique=(await lines.GetAsync([new(){FieldName="PO_LINE_ITEM_ID",Operator="=",FilterValue=command.LineId}]))?.ToArray();
            if(unique is not {Length:1} || unique[0] is not PO_LINE_ITEM uniqueLine || JsonSerializer.Serialize(uniqueLine)!=JsonSerializer.Serialize(line))throw new InvalidOperationException("The original cycle line identity is ambiguous.");
            var archives=await GetRepoAsync<PO_INVENTORY_LINK_HISTORY>("PO_INVENTORY_LINK_HISTORY");
            var byKey=(await archives.GetAsync([new(){FieldName="PO_INVENTORY_LINK_HISTORY_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
            var byIntent=(await archives.GetAsync([new(){FieldName="CYCLE_INTENT_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
            var entry=history.Entries.SingleOrDefault(value=>value.CYCLE_INTENT_ID==command.IntentId);
            var result=new PurchaseOrderInventoryLinkCycleResult(purchase,history);
            if(entry is null)
            {
                if(byKey is not {Length:0} || byIntent is not {Length:0})throw new InvalidOperationException("The original cycle intent is already used or ambiguous.");
                var fresh=PurchaseOrderInventoryLinkCycleReview.Create(purchase,history,command.LineId,command.Action,command.BindingRevision);
                command.Require(fresh);
                if(JsonSerializer.Serialize(line)!=JsonSerializer.Serialize(review.Purchase.Lines.Single(value=>value.PO_LINE_ITEM_ID==command.LineId)) || JsonSerializer.Serialize(history.Entries)!=JsonSerializer.Serialize(review.History.Entries))
                    throw new InvalidOperationException("The original reviewed cycle or predecessor evidence changed.");
            }
            else
            {
                if(byKey is not {Length:1} || byIntent is not {Length:1} || byKey[0] is not PO_INVENTORY_LINK_HISTORY || byIntent[0] is not PO_INVENTORY_LINK_HISTORY ||
                    JsonSerializer.Serialize(byKey[0])!=JsonSerializer.Serialize(entry) || JsonSerializer.Serialize(byIntent[0])!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("The retained cycle identity is ambiguous.");
                new PurchaseOrderInventoryLinkCycleSubmission(command,review).RequireResult(result);
            }
            await Guard(owned);return(result,entry is not null);
        }
        PurchaseOrderInventoryLinkCycleResult? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            var current=await Capture(true);
            if(!current.Recorded)
            {
                var original=CompanyInventorySourceReview.Copy(current.Result.Purchase.Lines.Single(value=>value.PO_LINE_ITEM_ID==command.LineId));
                var predecessor=current.Result.History.Entries.LastOrDefault();var utc=DateTime.UtcNow;
                var entry=new PO_INVENTORY_LINK_HISTORY{PO_INVENTORY_LINK_HISTORY_ID=command.IntentId,OWNER_TENANT_ID=command.Company.TenantId,OWNER_COMPANY_BA_ID=command.Company.BusinessAssociateId,
                    PURCHASE_ORDER_ID=command.OrderId,PO_LINE_ITEM_ID=command.LineId,SEQUENCE_NUMBER=current.Result.History.Entries.Count+1,PREVIOUS_ENTRY_ID=predecessor?.PO_INVENTORY_LINK_HISTORY_ID,PREVIOUS_REVISION=original.INVENTORY_LINK_HISTORY_REVISION,
                    CYCLE_INTENT_ID=command.IntentId,SOURCE_REVISION=review.Purchase.Revision,BINDING_REVISION=command.BindingRevision,REASON=command.Reason,ARCHIVED_LINE_JSON=JsonSerializer.Serialize(original),
                    ACTIVE_IND="Y",ROW_CREATED_BY=command.ActorUserId,ROW_CHANGED_BY=command.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};
                entry.RECORDED_REVISION=PurchaseOrderInventoryLinkHistoryReview.EntryRevision(entry);command.RequireEntry(review,entry);
                var archiveWriter=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(PO_INVENTORY_LINK_HISTORY),connection,"PO_INVENTORY_LINK_HISTORY",rowAccess:new InventoryLinkCyclePolicy(command,review,original,entry,true,()=>Guard(true)));
                await archiveWriter.InsertAsync(entry,command.ActorUserId);await Guard(true);
                var archives=await GetRepoAsync<PO_INVENTORY_LINK_HISTORY>("PO_INVENTORY_LINK_HISTORY");
                var inserted=(await archives.GetAsync([new(){FieldName="PO_INVENTORY_LINK_HISTORY_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
                var intentRows=(await archives.GetAsync([new(){FieldName="CYCLE_INTENT_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
                if(inserted is not {Length:1} || intentRows is not {Length:1} || JsonSerializer.Serialize(inserted[0])!=JsonSerializer.Serialize(entry) || JsonSerializer.Serialize(intentRows[0])!=JsonSerializer.Serialize(entry))
                    throw new InvalidOperationException("The exact original cycle archive was not stored uniquely.");
                await Guard(true);
                var lineWriter=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(PO_LINE_ITEM),connection,"PO_LINE_ITEM",rowAccess:new InventoryLinkCyclePolicy(command,review,original,entry,false,()=>Guard(true)));
                var reopened=CompanyInventorySourceReview.Copy(original);await lineWriter.UpdateAsync(reopened,command.ActorUserId);
            }
            var stored=await Capture(true);if(!stored.Recorded)throw new InvalidOperationException("The cycle archive was not recorded.");confirmed=stored.Result;await Guard(true);
        });
        var durable=await Capture(false);
        if(!durable.Recorded || confirmed is null || durable.Result.Purchase.Revision!=confirmed.Purchase.Revision || durable.Result.History.Revision!=confirmed.History.Revision)
            throw new InvalidOperationException("Committed cycle is unconfirmed. Reconcile the original intent.");
        return durable.Result;
    }
    private sealed class InventoryLinkCyclePolicy(PurchaseOrderInventoryLinkCycleCommand command,PurchaseOrderInventoryLinkCycleReview review,PO_LINE_ITEM original,PO_INVENTORY_LINK_HISTORY entry,bool archive,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private async Task RequireTable(Type type,string table)
        {
            await guard();if(type!=(archive?typeof(PO_INVENTORY_LINK_HISTORY):typeof(PO_LINE_ITEM)) || table!=(archive?"PO_INVENTORY_LINK_HISTORY":"PO_LINE_ITEM"))throw new InvalidOperationException("Cycle policy is restricted to the original archive or line.");
        }
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await RequireTable(type,table);if(actor!=command.ActorUserId)throw new InvalidOperationException("The original cycle actor changed.");
            if(archive)
            {
                if(operation!=PPDMRowMutation.Insert || previous is not null || entity is not PO_INVENTORY_LINK_HISTORY row || !ReferenceEquals(row,entry))throw new InvalidOperationException("Only the original append-only cycle archive is authorized.");
                row.ACTIVE_IND="Y";row.ROW_CREATED_BY=command.ActorUserId;row.ROW_CHANGED_BY=command.ActorUserId;row.ROW_CHANGED_DATE=row.ROW_CREATED_DATE=entry.ROW_CREATED_DATE;
                row.RECORDED_REVISION=PurchaseOrderInventoryLinkHistoryReview.EntryRevision(row);command.RequireEntry(review,row);
            }
            else
            {
                if(operation!=PPDMRowMutation.Update || entity is not PO_LINE_ITEM row || previous is not PO_LINE_ITEM before || JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Only the original reviewed line can reopen.");
                row.INVENTORY_LINK_REVIEW_JSON=null;row.INVENTORY_LINK_DECISION_JSON=null;row.INVENTORY_LINK_HISTORY_REVISION=entry.RECORDED_REVISION;row.ROW_CHANGED_BY=command.ActorUserId;row.ROW_CHANGED_DATE=entry.ROW_CREATED_DATE;
                if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(command.ReopenedLine(review,entry)))throw new InvalidOperationException("The cycle write changed unapproved line data.");
            }
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await RequireTable(type,table);if(archive)throw new InvalidOperationException("Archive policy cannot grant reads.");return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="PO_LINE_ITEM_ID",Operator="=",FilterValue=command.LineId}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await RequireTable(type,table);if(archive)throw new InvalidOperationException("Archive policy cannot grant reads.");return new List<AppFilter>{new(){FieldName="PURCHASE_ORDER_ID",Operator="=",FilterValue=command.OrderId}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await RequireTable(type,table);if(archive || rows.Count!=1 || rows[0] is not PO_LINE_ITEM row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The original cycle line changed before write.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Cycle policy cannot grant reads.");
    }
}
