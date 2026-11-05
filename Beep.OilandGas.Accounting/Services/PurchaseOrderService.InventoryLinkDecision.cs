using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class PurchaseOrderService
{
    public async Task<PO_LINE_ITEM> RecordInventoryLinkDecisionAsync(PurchaseOrderInventoryLinkDecisionCommand command,PurchaseOrderInventoryLinkDecisionReview review,
        Func<Task<CompanyInventorySourceReview>> readTarget,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(review);ArgumentNullException.ThrowIfNull(readTarget);ArgumentNullException.ThrowIfNull(requireAuthority);
        command=CompanyInventorySourceReview.Copy(command);review=CompanyInventorySourceReview.Copy(review);command.Require(review);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original production binding is unavailable.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The original production source is unavailable.");
        async Task Guard(bool owned)
        {
            await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original decision binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        async Task<PO_LINE_ITEM> Confirm(bool owned)
        {
            await Guard(owned);
            var repo=await GetRepoAsync<PO_LINE_ITEM>("PO_LINE_ITEM");
            var schema=source.GetEntityStructure(new EntityStructure{EntityName="PO_LINE_ITEM",DatasourceEntityName="PO_LINE_ITEM"},true);
            var fields=schema?.Fields?.Where(field=>field.FieldName=="INVENTORY_LINK_DECISION_JSON").ToArray();
            if(fields is not {Length:1} || !fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || fields[0].Size<PurchaseOrderInventoryLinkDecisionProof.MaximumLength)
                throw new InvalidOperationException("Review the nullable decision column in the PRODUCTION Beep migration.");
            var current=await GetSourceReviewAsync(command.Company,command.OrderId,command.ActorUserId) ?? throw new InvalidOperationException("The original owned PO source is unavailable.");
            var raw=(await repo.GetAsync([new(){FieldName="PO_LINE_ITEM_ID",Operator="=",FilterValue=command.LineId}]))?.ToArray();
            if(raw is not {Length:1} || raw[0] is not PO_LINE_ITEM row || JsonSerializer.Serialize(current.Lines.SingleOrDefault(line=>line.PO_LINE_ITEM_ID==command.LineId))!=JsonSerializer.Serialize(row))
                throw new InvalidOperationException("The original line identity is ambiguous or changed.");
            var original=row;
            if(row.INVENTORY_LINK_DECISION_JSON is not null)
            {
                var proof=PurchaseOrderInventoryLinkDecisionProof.Require(command.Company,row);
                if(proof.Command!=command || JsonSerializer.Serialize(proof.Review)!=JsonSerializer.Serialize(review))throw new InvalidOperationException("A different decision is already recorded. Reconcile the original intent.");
                original=proof.Review.Purchase.Lines.Single(line=>line.PO_LINE_ITEM_ID==command.LineId);
            }
            var recreated=PurchaseOrderSourceReview.Create(command.Company,command.ActorUserId,current.Header,current.Lines.Select(line=>line.PO_LINE_ITEM_ID==command.LineId?original:line).ToArray(),current.Receipts);
            var target=CompanyInventorySourceReview.Copy(await readTarget() ?? throw new InvalidOperationException("The reviewed target source is unavailable."));target.Require(command.Company,command.ActorUserId,command.TargetItemId);
            var fresh=PurchaseOrderInventoryLinkDecisionReview.Create(recreated,target,command.LineId,command.BindingRevision);
            if(fresh.Revision!=review.Revision)throw new InvalidOperationException("The original order or target inventory evidence changed.");
            command.Require(fresh);await Guard(owned);return CompanyInventorySourceReview.Copy(row);
        }
        PO_LINE_ITEM? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            var row=await Confirm(true);
            if(row.INVENTORY_LINK_DECISION_JSON is null)
            {
                var original=CompanyInventorySourceReview.Copy(row);
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(PO_LINE_ITEM),connection,"PO_LINE_ITEM",rowAccess:new InventoryLinkDecisionPolicy(command,review,original,()=>Guard(true)));
                await writer.UpdateAsync(row,command.ActorUserId);
                var stored=await Confirm(true);if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The stored decision differs from the reviewed write.");
            }
            confirmed=await Confirm(true);await Guard(true);
        });
        var durable=await Confirm(false);
        if(JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(confirmed))throw new InvalidOperationException("Committed decision is unconfirmed. Reconcile the original intent.");
        return durable;
    }
    private sealed class InventoryLinkDecisionPolicy(PurchaseOrderInventoryLinkDecisionCommand command,PurchaseOrderInventoryLinkDecisionReview review,PO_LINE_ITEM original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(PO_LINE_ITEM) || table!="PO_LINE_ITEM" || operation!=PPDMRowMutation.Update || actor!=command.ActorUserId || entity is not PO_LINE_ITEM row || previous is not PO_LINE_ITEM before || JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(original))
                throw new InvalidOperationException("Only the original reviewed PO-line decision is authorized.");
            if(command.Decision==PurchaseOrderInventoryLinkDecision.Approve)row.INVENTORY_ITEM_ID=command.TargetItemId;
            row.INVENTORY_LINK_DECISION_JSON=PurchaseOrderInventoryLinkDecisionProof.Create(command,review,row);_=PurchaseOrderInventoryLinkDecisionProof.Require(command.Company,row);
        }
        private async Task RequireTable(Type type,string table){await guard();if(type!=typeof(PO_LINE_ITEM) || table!="PO_LINE_ITEM")throw new InvalidOperationException("Decision policy is restricted to the original line.");}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await RequireTable(type,table);return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="PO_LINE_ITEM_ID",Operator="=",FilterValue=command.LineId}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await RequireTable(type,table);return new List<AppFilter>{new(){FieldName="PURCHASE_ORDER_ID",Operator="=",FilterValue=command.OrderId}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await RequireTable(type,table);if(rows.Count!=1 || rows[0] is not PO_LINE_ITEM row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The original line changed before decision.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Decision policy cannot grant reads.");
    }
}
