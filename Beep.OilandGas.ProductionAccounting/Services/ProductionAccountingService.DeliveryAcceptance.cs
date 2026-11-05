using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
namespace Beep.OilandGas.ProductionAccounting.Services;
public partial class ProductionAccountingService
{
    public void RequireDeliveryAcceptanceSchema(string connection)
    {
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery PRODUCTION datasource is unavailable.");
        var fields=source.GetEntityStructure(new EntityStructure{EntityName="RUN_TICKET",DatasourceEntityName="RUN_TICKET"},true)?.Fields?.Where(field=>field.FieldName=="DELIVERY_WORKFLOW_JSON").ToArray();
        if(fields is not {Length:1} || !fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || fields[0].Size<RevenueDeliveryAcceptanceProof.MaximumLength)
            throw new InvalidOperationException("Review the nullable delivery evidence column through the PRODUCTION Beep migration.");
    }
    public Task<RevenueDeliveryAcceptanceRecord> PrepareDeliveryAcceptanceAsync(RevenueDeliveryAcceptancePreparation preparation,string connection,Func<Task<RevenueDeliverySourceReview>> readCurrent,Func<Task> requireAuthority)
    {preparation=CompanyInventorySourceReview.Copy(preparation);preparation.Require();preparation.Facts.RequireQuantity(preparation.Source);return RecordDeliveryAcceptanceAsync(preparation,null,connection,readCurrent,requireAuthority);}
    public Task<RevenueDeliveryAcceptanceRecord> DecideDeliveryAcceptanceAsync(RevenueDeliveryAcceptanceRecord preparation,RevenueDeliveryAcceptanceDecisionCommand command,string connection,Func<Task<RevenueDeliverySourceReview>> readCurrent,Func<Task> requireAuthority)
    {preparation=CompanyInventorySourceReview.Copy(preparation);command=CompanyInventorySourceReview.Copy(command);command.RequireExecution(preparation);return RecordDeliveryAcceptanceAsync(preparation.Preparation,command,connection,readCurrent,requireAuthority);}
    public Task<RevenueDeliveryAcceptanceRecord> CorrectDeliveryAcceptanceAsync(RevenueDeliveryAcceptanceCorrection correction,string connection,Func<Task<RevenueDeliverySourceReview>> readCurrent,Func<Task> requireAuthority)
    {correction=CompanyInventorySourceReview.Copy(correction);correction.Require();return RecordDeliveryAcceptanceAsync(correction.Preparation,null,connection,readCurrent,requireAuthority,correction);}
    private async Task<RevenueDeliveryAcceptanceRecord> RecordDeliveryAcceptanceAsync(RevenueDeliveryAcceptancePreparation preparation,RevenueDeliveryAcceptanceDecisionCommand? decision,string connection,Func<Task<RevenueDeliverySourceReview>> readCurrent,Func<Task> requireAuthority,RevenueDeliveryAcceptanceCorrection? correction=null)
    {
        ArgumentNullException.ThrowIfNull(readCurrent);ArgumentNullException.ThrowIfNull(requireAuthority);RequireDeliveryAcceptanceSchema(connection);
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery datasource is unavailable.");var actor=decision?.ActorUserId ??preparation.ActorUserId;
        async Task Guard(bool owned){await requireAuthority();RequireDeliveryAcceptanceSchema(connection);if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The delivery datasource changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        async Task<(RevenueDeliverySourceReview Review,RevenueDeliveryAcceptanceRecord? Record,RevenueDeliveryAcceptanceProof? Proof)> Read(bool owned)
        {
            await Guard(owned);var current=await readCurrent();preparation.RequireCurrent(current);
            var proof=current.Ticket.DELIVERY_WORKFLOW_JSON is null?null:RevenueDeliveryAcceptanceProof.RequireEnvelope(preparation.Company,current.Ticket);var record=proof?.Record;
            var same=record is not null && JsonSerializer.Serialize(record.Preparation)==JsonSerializer.Serialize(preparation);
            if(record is not null && !same && (correction is null || proof!.Revision!=correction.PreviousNativeRevision || JsonSerializer.Serialize(record)!=JsonSerializer.Serialize(correction.Previous)))throw new InvalidOperationException("A different original delivery preparation is already recorded.");
            if(correction is not null)
            {
                if(record is null)throw new InvalidOperationException("The original rejected delivery preparation is unavailable.");
                if(same && (proof!.Correction!=new RevenueDeliveryAcceptanceCorrectionOrigin(correction.Previous.Revision,correction.PreviousNativeRevision,correction.Reason) ||
                    proof.PriorVersions is not {Count:>0} || JsonSerializer.Serialize(proof.PriorVersions[^1].Record)!=JsonSerializer.Serialize(correction.Previous)))throw new InvalidOperationException("The recorded receiving correction differs from its original predecessor or reason.");
            }
            if(decision is not null)
            {
                if(record is null)throw new InvalidOperationException("The original delivery preparation is absent.");
                if(record.Decision is null)decision.Require(record);
                else if(JsonSerializer.Serialize(record.Decision)!=JsonSerializer.Serialize(decision))throw new InvalidOperationException("The delivery decision conflicts with its original intent.");
            }
            await Guard(owned);return(current,record,proof);
        }
        RevenueDeliveryAcceptanceRecord? confirmed=null;string? native=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            var current=await Read(true);var next=current.Record ??RevenueDeliveryAcceptanceRecord.Create(preparation,DateTime.UtcNow);
            var history=current.Proof?.PriorVersions;var origin=current.Proof?.Correction;
            if(correction is not null && current.Record is not null && JsonSerializer.Serialize(current.Record.Preparation)!=JsonSerializer.Serialize(preparation))
            {
                var previous=current.Proof!;history=(previous.PriorVersions ??[]).Append(new RevenueDeliveryAcceptancePriorVersion(previous.Record,previous.NativeRowJson,previous.Revision){Correction=previous.Correction}).ToArray();
                origin=new(correction.Previous.Revision,correction.PreviousNativeRevision,correction.Reason);next=RevenueDeliveryAcceptanceRecord.Create(preparation,DateTime.UtcNow);
            }
            if(decision is not null && next.Decision is null)next=next.Decide(decision,current.Review,DateTime.UtcNow);
            if(current.Record is null || current.Record.Revision!=next.Revision)
            {
                var row=CompanyInventorySourceReview.Copy(current.Review.Ticket);var original=CompanyInventorySourceReview.Copy(row);
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(RUN_TICKET),connection,"RUN_TICKET",rowAccess:new DeliveryAcceptancePolicy(original,next,actor,()=>Guard(true),history,origin));
                await writer.UpdateAsync(row,actor);
                var written=await Read(true);
                if(written.Record?.Revision!=next.Revision || JsonSerializer.Serialize(written.Review.Ticket)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Native delivery audit/read-back is unconfirmed.");
            }
            var closing=await Read(true);confirmed=closing.Record;if(confirmed?.Revision!=next.Revision)throw new InvalidOperationException("The recorded delivery outcome changed.");native=JsonSerializer.Serialize(closing.Review.Ticket);await Guard(true);
        });
        var durable=await Read(false);
        if(confirmed is null || durable.Record?.Revision!=confirmed.Revision || JsonSerializer.Serialize(durable.Review.Ticket)!=native)throw new InvalidOperationException("Committed delivery evidence is uncertain. Reconcile the original intent.");
        return CompanyInventorySourceReview.Copy(durable.Record);
    }
    private sealed class DeliveryAcceptancePolicy(RUN_TICKET original,RevenueDeliveryAcceptanceRecord record,string actor,Func<Task> guard,IReadOnlyList<RevenueDeliveryAcceptancePriorVersion>? history=null,RevenueDeliveryAcceptanceCorrectionOrigin? origin=null):IPPDMRowAccessPolicy
    {
        private async Task Table(Type type,string table){await guard();if(type!=typeof(RUN_TICKET) || table!="RUN_TICKET")throw new InvalidOperationException("Delivery mutation is restricted to the original native ticket.");}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? user)
        {
            await Table(type,table);
            if(operation!=PPDMRowMutation.Update || user!=actor || entity is not RUN_TICKET row || previous is not RUN_TICKET before || JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(original) || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE is null)
                throw new InvalidOperationException("Original delivery mutation/audit changed.");
            row.DELIVERY_WORKFLOW_JSON=RevenueDeliveryAcceptanceProof.Create(record,row,history,origin);_=RevenueDeliveryAcceptanceProof.Require(record.Preparation.Company,row);
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await Table(type,table);return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="RUN_TICKET_ID",Operator="=",FilterValue=original.RUN_TICKET_ID}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await Table(type,table);return new List<AppFilter>{new(){FieldName="FIELD_ID",Operator="=",FilterValue=original.FIELD_ID}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await Table(type,table);if(rows.Count!=1 || rows[0] is not RUN_TICKET row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Original native delivery changed before mutation.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Delivery mutation policy grants no reads.");
    }
}
