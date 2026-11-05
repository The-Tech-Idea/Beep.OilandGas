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
    /// <summary>Requires an independently enforced original company/receiver write grant and binding on every guard.</summary>
    public async Task<PO_RECEIPT> RecordReceiptDecisionAsync(PurchaseReceiptDecisionCommand command,PurchaseOrderSourceReview displayed,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(displayed);ArgumentNullException.ThrowIfNull(requireAuthority);
        command=PurchaseReceiptDecisionProof.Copy(command);displayed=PurchaseReceiptDecisionProof.Copy(displayed);command.Require(displayed);
        await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required.");
        var connection=await _resolveConnection();
        if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The production binding is unavailable.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The procurement datasource is unavailable.");
        async Task Guard(bool owned)
        {
            await requireAuthority();
            if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))
                throw new InvalidOperationException("The original receiving binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        PO_RECEIPT? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var repo=await GetRepoAsync<PO_RECEIPT>("PO_RECEIPT");
            var schema=source.GetEntityStructure(new EntityStructure{EntityName="PO_RECEIPT",DatasourceEntityName="PO_RECEIPT"},true);
            foreach(var name in new[]{"ACCEPTED_QUANTITY","REJECTED_QUANTITY","RECEIVER_DECISION_JSON"})
            {
                var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
                if(fields is not {Length:1} || !fields[0].AllowDBNull ||
                    (name=="RECEIVER_DECISION_JSON"?(fields[0].Fieldtype!=typeof(string).FullName || fields[0].Size<PurchaseReceiptDecisionProof.MaximumLength):
                    fields[0].Fieldtype!=typeof(decimal).FullName))
                    throw new InvalidOperationException("Review the nullable receipt decision columns in the PRODUCTION Beep migration before receiving.");
            }
            async Task<PO_RECEIPT> Review()
            {
                await Guard(true);
                var current=await GetSourceReviewAsync(command.Company,command.OrderId,command.ActorUserId)
                    ?? throw new InvalidOperationException("The original owned order is unavailable.");
                // Global identity read also refuses a duplicate/foreign receipt hidden by the order predicate.
                var raw=(await repo.GetAsync([new(){FieldName="PO_RECEIPT_ID",Operator="=",FilterValue=command.ReceiptId}]))?.ToArray();
                if(raw is not {Length:1} || raw[0] is not PO_RECEIPT row ||
                    JsonSerializer.Serialize(current.Receipts.SingleOrDefault(item=>item.PO_RECEIPT_ID==command.ReceiptId))!=JsonSerializer.Serialize(row))
                    throw new InvalidOperationException("The original receipt identity or ownership is unconfirmed.");
                var original=row;
                if(row.RECEIVER_DECISION_JSON is not null || row.ACCEPTED_QUANTITY is not null || row.REJECTED_QUANTITY is not null)
                {
                    var proof=PurchaseReceiptDecisionProof.Require(command.Company,row);
                    if(proof.Command!=command)throw new InvalidOperationException("Another receiver intent is already recorded. Do not replace or repair it.");
                    original=proof.OriginalReceipt;
                    if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(displayed.Receipts.Single(item=>item.PO_RECEIPT_ID==command.ReceiptId)))
                        throw new InvalidOperationException("The original receiving evidence changed.");
                }
                var recreated=PurchaseOrderSourceReview.Create(command.Company,command.ActorUserId,current.Header,current.Lines,
                    current.Receipts.Select(item=>item.PO_RECEIPT_ID==command.ReceiptId?original:item).ToArray());
                command.Require(recreated);await Guard(true);return PurchaseReceiptDecisionProof.Copy(row);
            }
            var row=await Review();
            if(row.RECEIVER_DECISION_JSON is null)
            {
                var original=PurchaseReceiptDecisionProof.Copy(row);
                row.ACCEPTED_QUANTITY=command.AcceptedQuantity;row.REJECTED_QUANTITY=command.RejectedQuantity;
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(PO_RECEIPT),connection,"PO_RECEIPT",
                    rowAccess:new ReceiptDecisionPolicy(command,original,()=>Guard(true)));
                await writer.UpdateAsync(row,command.ActorUserId);
                var stored=await Review();
                if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The stored receiver decision differs from the reviewed write.");
                row=stored;
            }
            confirmed=await Review();await Guard(true);
        });
        await Guard(false);
        var finalRepo=await GetRepoAsync<PO_RECEIPT>("PO_RECEIPT");
        var durable=(await finalRepo.GetAsync([new(){FieldName="PO_RECEIPT_ID",Operator="=",FilterValue=command.ReceiptId}]))?.ToArray();
        if(durable is not {Length:1} || durable[0] is not PO_RECEIPT stored || JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(confirmed))
            throw new InvalidOperationException("The committed receiver decision is unconfirmed. Reconcile the original intent.");
        var result=PurchaseReceiptDecisionProof.Require(command.Company,stored);
        if(result.Command!=command)throw new InvalidOperationException("The receiver intent changed after commit.");
        await Guard(false);return PurchaseReceiptDecisionProof.Copy(stored);
    }
    private sealed class ReceiptDecisionPolicy(PurchaseReceiptDecisionCommand command,PO_RECEIPT original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();
            if(type!=typeof(PO_RECEIPT) || table!="PO_RECEIPT" || operation!=PPDMRowMutation.Update || actor!=command.ActorUserId ||
                entity is not PO_RECEIPT row || previous is not PO_RECEIPT before || JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(original))
                throw new InvalidOperationException("Only the original reviewed receipt decision update is authorized.");
            row.RECEIVER_DECISION_JSON=PurchaseReceiptDecisionProof.Create(command,original,row);
            _=PurchaseReceiptDecisionProof.Require(command.Company,row);
        }
        private async Task RequireTable(Type type,string table)
        {
            await guard();if(type!=typeof(PO_RECEIPT) || table!="PO_RECEIPT")throw new InvalidOperationException("Receipt decision policy is restricted to the original receipt.");
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {
            await RequireTable(type,table);return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="PO_RECEIPT_ID",Operator="=",FilterValue=command.ReceiptId}}};
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {
            await RequireTable(type,table);return new List<AppFilter>{new(){FieldName="PURCHASE_ORDER_ID",Operator="=",FilterValue=command.OrderId}};
        }
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {
            await RequireTable(type,table);
            if(rows.Count!=1 || rows[0] is not PO_RECEIPT row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(original))
                throw new InvalidOperationException("The mutation's original receipt changed before writing.");
        }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Mutation policy cannot grant reads.");
    }
}
