using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ProductionAccounting.Services;

public partial class ProductionAccountingService
{
    public void RequireCommercialTermsSchema(string connection)
    {
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The commercial PRODUCTION datasource is unavailable.");
        var schema=source.GetEntityStructure(new EntityStructure{EntityName="SALES_CONTRACT",DatasourceEntityName="SALES_CONTRACT"},true);
        var fields=schema?.Fields?.Where(field=>field.FieldName=="TERMS_WORKFLOW_JSON").ToArray();
        if(fields is not {Length:1} || !fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || fields[0].Size<SalesContractTermsNativeProof.MaximumLength)
            throw new InvalidOperationException("Review the nullable commercial evidence column through the PRODUCTION Beep migration.");
    }
    public Task<SalesContractTermsRecord> PrepareSalesContractTermsAsync(SalesContractTermsPreparation preparation,string connection,
        Func<Task<RevenueSalesContractReview>> readCurrent,Func<Task> requireAuthority)
    {
        preparation=CompanyInventorySourceReview.Copy(preparation);preparation.Require();
        if(preparation.Source.Contract.TERMS_WORKFLOW_JSON is not null)throw new InvalidOperationException("Review the original recorded commercial intent rather than replacing its preparation.");
        return RecordCommercialTermsAsync(preparation,null,null,connection,readCurrent,requireAuthority);
    }
    public Task<SalesContractTermsRecord> DecideSalesContractTermsAsync(SalesContractTermsRecord preparation,SalesContractTermsDecisionCommand command,string connection,
        Func<Task<RevenueSalesContractReview>> readCurrent,Func<Task> requireAuthority)
    {
        preparation=CompanyInventorySourceReview.Copy(preparation);command=CompanyInventorySourceReview.Copy(command);command.Require(preparation);
        return RecordCommercialTermsAsync(preparation.Preparation,preparation,command,connection,readCurrent,requireAuthority);
    }
    public Task<SalesContractTermsRecord> AmendSalesContractTermsAsync(SalesContractTermsAmendment amendment,string connection,
        Func<Task<RevenueSalesContractReview>> readCurrent,Func<Task> requireAuthority)
    {
        amendment=CompanyInventorySourceReview.Copy(amendment);amendment.Require();
        return RecordCommercialTermsAsync(amendment.Preparation,null,null,connection,readCurrent,requireAuthority,amendment);
    }
    private async Task<SalesContractTermsRecord> RecordCommercialTermsAsync(SalesContractTermsPreparation preparation,SalesContractTermsRecord? expected,SalesContractTermsDecisionCommand? decision,
        string connection,Func<Task<RevenueSalesContractReview>> readCurrent,Func<Task> requireAuthority,SalesContractTermsAmendment? amendment=null)
    {
        ArgumentNullException.ThrowIfNull(readCurrent);ArgumentNullException.ThrowIfNull(requireAuthority);
        var company=preparation.Company;var id=preparation.Source.Contract.SALES_CONTRACT_ID;var actor=decision?.ActorUserId ??preparation.ActorUserId;
        await requireAuthority();var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The commercial PRODUCTION datasource is unavailable.");
        async Task Guard(bool owned)
        {await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The commercial datasource changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        async Task<(SALES_CONTRACT Row,SalesContractTermsRecord? Record,RevenueSalesContractReview Review,SalesContractTermsNativeProof? Proof)> Read(bool owned)
        {
            await Guard(owned);
            if(amendment?.Process is {} origin)
            {
                var saved=await GetCommercialAmendmentIntentAsync(company,actor,origin.InstanceId,connection,()=>Guard(owned));
                if(saved is null || JsonSerializer.Serialize(saved.Restore(company,actor,origin.InstanceId).Amendment)!=JsonSerializer.Serialize(amendment))
                    throw new InvalidOperationException("Reconcile the original durable amendment proposal before another mutation.");
            }
            RequireCommercialTermsSchema(connection);
            var row=await GetRecordedSalesContractAsync(id,company,connection);
            var fresh=CompanyInventorySourceReview.Copy(await readCurrent());
            if(JsonSerializer.Serialize(fresh.Contract)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Native contract and reviewed source disagree.");
            SalesContractTermsRecord? recorded=null;SalesContractTermsNativeProof? proof=null;
            if(row.TERMS_WORKFLOW_JSON is not null)
            {
                proof=SalesContractTermsNativeProof.Require(company,row);recorded=proof.Record;
                var samePreparation=JsonSerializer.Serialize(recorded.Preparation)==JsonSerializer.Serialize(preparation);
                if(!samePreparation && (amendment is null || proof.Revision!=amendment.PreviousNativeRevision || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(amendment.Previous)))
                    throw new InvalidOperationException("A different commercial preparation is already recorded.");
                if(amendment is not null && samePreparation && (proof.Amendment!=new SalesContractTermsAmendmentOrigin(amendment.Previous.Revision,amendment.PreviousNativeRevision,amendment.Reason,amendment.Process) ||
                    proof.PriorVersions is not {Count:>0} || JsonSerializer.Serialize(proof.PriorVersions[^1].Record)!=JsonSerializer.Serialize(amendment.Previous)))
                    throw new InvalidOperationException("The recorded amendment differs from its original predecessor or reason.");
                fresh=SalesContractTermsNativeProof.RestoreOriginalReview(fresh,recorded);
                if(decision is not null)
                {
                    if(recorded.Decision is null && JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(expected) ||
                        recorded.Decision is not null && JsonSerializer.Serialize(recorded.Decision)!=JsonSerializer.Serialize(decision))
                        throw new InvalidOperationException("The commercial decision conflicts with its original preparation or recorded intent.");
                }
            }
            else if(decision is not null || amendment is not null)throw new InvalidOperationException("The original commercial preparation is not recorded.");
            preparation.RequireCurrentSource(fresh);await Guard(owned);return(row,recorded,fresh,proof);
        }
        SalesContractTermsRecord? confirmed=null;string? native=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            var current=await Read(true);var next=current.Record;
            var history=current.Proof?.PriorVersions;var origin=current.Proof?.Amendment;
            if(amendment is not null && current.Record is not null && JsonSerializer.Serialize(current.Record.Preparation)!=JsonSerializer.Serialize(preparation))
            {
                var previous=current.Proof!;
                history=(previous.PriorVersions ?? []).Append(new SalesContractTermsPriorVersion(previous.Record,previous.NativeRowJson,previous.Revision,previous.Amendment)).ToArray();
                origin=new(amendment.Previous.Revision,amendment.PreviousNativeRevision,amendment.Reason,amendment.Process);
                next=SalesContractTermsRecord.Create(preparation,DateTime.UtcNow);
            }
            if(next is null)next=SalesContractTermsRecord.Create(preparation,DateTime.UtcNow);
            else if(decision is not null && next.Decision is null)next=next.Decide(decision,current.Review,DateTime.UtcNow);
            if(current.Record is null || current.Record.Revision!=next.Revision)
            {
                var original=CompanyInventorySourceReview.Copy(current.Row);
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_CONTRACT),connection,"SALES_CONTRACT",rowAccess:new CommercialTermsPolicy(company,id,actor,original,next,()=>Guard(true),history,origin));
                await writer.UpdateAsync(current.Row,actor);
                var written=await GetRecordedSalesContractAsync(id,company,connection);
                if(JsonSerializer.Serialize(written)!=JsonSerializer.Serialize(current.Row) || SalesContractTermsNativeProof.Require(company,written).Record.Revision!=next.Revision)
                    throw new InvalidOperationException("Native commercial write/audit read-back is unconfirmed.");
            }
            var closing=await Read(true);confirmed=closing.Record ??throw new InvalidOperationException("Commercial evidence was not recorded.");
            if(confirmed.Revision!=next.Revision)throw new InvalidOperationException("The commercial recorded outcome changed.");native=JsonSerializer.Serialize(closing.Row);await Guard(true);
        });
        var durable=await Read(false);
        if(confirmed is null || durable.Record?.Revision!=confirmed.Revision || JsonSerializer.Serialize(durable.Row)!=native)
            throw new InvalidOperationException("Committed commercial evidence is uncertain. Reconcile the original intent.");
        return CompanyInventorySourceReview.Copy(durable.Record);
    }
    private sealed class CommercialTermsPolicy(CompanyScope company,string id,string actor,SALES_CONTRACT original,SalesContractTermsRecord record,Func<Task> guard,IReadOnlyList<SalesContractTermsPriorVersion>? history=null,SalesContractTermsAmendmentOrigin? origin=null):IPPDMRowAccessPolicy
    {
        private async Task Table(Type type,string table)
        {await guard();if(type!=typeof(SALES_CONTRACT) || table!="SALES_CONTRACT")throw new InvalidOperationException("Commercial mutation is restricted to the exact native contract.");}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? user)
        {
            await Table(type,table);
            if(operation!=PPDMRowMutation.Update || user!=actor || entity is not SALES_CONTRACT row || previous is not SALES_CONTRACT before || JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The original commercial mutation changed.");
            if(row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE is null)throw new InvalidOperationException("Native commercial audit stamping is required.");
            row.TERMS_WORKFLOW_JSON=SalesContractTermsNativeProof.Create(record,row,history,origin);_=SalesContractTermsNativeProof.Require(company,row);
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await Table(type,table);return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="SALES_CONTRACT_ID",Operator="=",FilterValue=id}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await Table(type,table);return new List<AppFilter>{new(){FieldName="SELLER_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await Table(type,table);if(rows.Count!=1 || rows[0] is not SALES_CONTRACT row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Original native commercial contract changed before mutation.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Commercial mutation policy grants no reads.");
    }
}
