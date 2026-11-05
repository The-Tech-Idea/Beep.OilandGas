using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public partial class ContractorManagementService
{
    private static void RequirePersonnelReleaseSchema(IDataSource source)
    {
        const string table="WORK_ORDER_PERSONNEL_RELEASE";
        if(!source.CheckEntityExist(table))throw new InvalidOperationException("Review the existing PPDM_CORE Beep personnel release manifest; missing release schema is not an empty ledger.");
        var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=shape?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="RELEASE_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Exact personnel release key metadata is unavailable.");
        foreach(var (name,size) in new[]{("COMMAND_JSON",4*1024*1024),("RECORDED_REVISION",64)})
        {
            var fields=shape!.Fields.Where(field=>field.FieldName==name).ToArray();
            if(fields.Length!=1 || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Bounded native personnel release proof metadata is unavailable.");
        }
    }
    private async Task<WORK_ORDER_PERSONNEL_RELEASE[]> ReadPersonnelReleasesCoreAsync(FieldSelectionReview context,IReadOnlyList<WORK_ORDER_PERSONNEL_ASSIGNMENT> originals,Func<Task> guard,bool owned)
    {
        await guard();var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel release source is unavailable.");
        if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequirePersonnelReleaseSchema(source);
        if(originals.Count>4096 || originals.Select(row=>row.ASSIGNMENT_ID).Distinct(StringComparer.Ordinal).Count()!=originals.Count)throw new InvalidOperationException("Complete bounded original assignment population is unavailable.");
        var result=new List<WORK_ORDER_PERSONNEL_RELEASE>();var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_RELEASE),context.Database.ConnectionName,"WORK_ORDER_PERSONNEL_RELEASE");
        foreach(var original in originals)
        {
            if(original.OWNER_TENANT_ID!=context.Company.TenantId || original.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId)throw new UnauthorizedAccessException("Release ledger may only inspect original company assignments.");
            await guard();var rows=(await repo.GetAsync([new(){FieldName="ASSIGNMENT_ID",Operator="=",FilterValue=original.ASSIGNMENT_ID},new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId}])).ToArray();await guard();
            if(rows.Length>1 || rows.Any(row=>row is not WORK_ORDER_PERSONNEL_RELEASE))throw new InvalidOperationException("An original assignment has ambiguous cancellation evidence.");
            foreach(var value in rows)
            {
                var row=(WORK_ORDER_PERSONNEL_RELEASE)value;
                if(row.ASSIGNMENT_ID!=original.ASSIGNMENT_ID || row.OWNER_TENANT_ID!=original.OWNER_TENANT_ID || row.OWNER_COMPANY_BA_ID!=original.OWNER_COMPANY_BA_ID || row.FIELD_ID!=original.FIELD_ID || row.WORK_ORDER_ID!=original.WORK_ORDER_ID || row.PROCESS_INSTANCE_ID!=original.PROCESS_INSTANCE_ID || row.PERMIT_INTENT_ID!=original.PERMIT_INTENT_ID || row.ROW_CREATED_BY!=original.ROW_CREATED_BY || row.ACTIVE_IND!="Y")throw new UnauthorizedAccessException("Release public company/task/maker headers differ before private proof inspection.");
                CompanyWorkOrderPersonnelReleaseFacts.From(row,original);result.Add(CompanyInventorySourceReview.Copy(row));
            }
        }
        await guard();return result.OrderBy(row=>row.RELEASE_ID,StringComparer.Ordinal).ToArray();
    }
    public async Task<WORK_ORDER_PERSONNEL_RELEASE> RecordPersonnelReleaseAsync(CompanyWorkOrderPersonnelReleaseCommand original,Func<Task> requireAuthority,bool readOnly=false)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Original.Command.Review.Personnel.Context;
        await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel cancellation source is unavailable.");
        async Task Guard(bool owned=false)
        {
            await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
            if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original personnel cancellation native target changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            RequirePersonnelAssignmentSchema(source);RequirePersonnelReleaseSchema(source);
        }
        async Task<WORK_ORDER_PERSONNEL_ASSIGNMENT> ReadOriginal(bool owned=false)
        {
            await Guard(owned);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT),context.Database.ConnectionName,"WORK_ORDER_PERSONNEL_ASSIGNMENT");
            var rows=(await repo.GetAsync([new(){FieldName="ASSIGNMENT_ID",Operator="=",FilterValue=command.Original.Recorded.ASSIGNMENT_ID}])).ToArray();await Guard(owned);
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_PERSONNEL_ASSIGNMENT row)throw new InvalidOperationException("Exact original personnel assignment is unavailable.");
            if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.ROW_CREATED_BY!=context.ActorUserId || row.FIELD_ID!=context.FieldId)throw new UnauthorizedAccessException("Only the current original company assignment maker may cancel its duty.");
            command.Original.Command.RequireRecorded(row);if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(command.Original.Recorded))throw new InvalidOperationException("Complete original assignment changed before cancellation.");return CompanyInventorySourceReview.Copy(row);
        }
        async Task<WORK_ORDER_PERSONNEL_RELEASE?> Read(bool owned=false)
        {
            var assignment=await ReadOriginal(owned);var rows=await ReadPersonnelReleasesCoreAsync(context,[assignment],()=>Guard(owned),owned);
            var row=rows.SingleOrDefault();if(row is not null)command.RequireRecorded(row);return row;
        }
        await Guard();var recorded=await Read();
        if(readOnly || recorded is not null)
        {
            if(recorded is null)throw new InvalidOperationException("Original personnel release is not recorded; keep its retained intent.");
            if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(await Read()))throw new InvalidOperationException("Complete original personnel release changed during recovery.");return recorded;
        }
        WORK_ORDER_PERSONNEL_RELEASE? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);confirmed=await Read(true);if(confirmed is not null)return;
            var row=command.Prepare(DateTime.UtcNow);var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_RELEASE),context.Database.ConnectionName,"WORK_ORDER_PERSONNEL_RELEASE",rowAccess:new PersonnelReleaseInsertPolicy(row,command,()=>Guard(true)));
            await writer.InsertAsync(row,context.ActorUserId);confirmed=await Read(true);if(confirmed is null)throw new InvalidOperationException("Original personnel release append is unconfirmed.");await Guard(true);
        });
        var durable=await Read();if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed original personnel release is unconfirmed. Recover its retained original intent.");await Guard();return durable!;
    }
    public async Task<CompanyWorkOrderPersonnelReleaseRecovery> RecoverPersonnelReleaseReferenceAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelReleaseReference reference,Func<Task> requireAuthority)
    {
        reference.Require();var assignment=await RecoverPersonnelAssignmentReferenceAsync(context,reference.Assignment,requireAuthority);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel release source is unavailable.");
        async Task Guard(){await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original release native target changed.");PPDMDataSourceTransaction.RequireIdle(source);RequirePersonnelReleaseSchema(source);}
        var rows=await ReadPersonnelReleasesCoreAsync(context,[assignment.Recorded],Guard,false);var row=rows.SingleOrDefault()??throw new InvalidOperationException("Original personnel release is unavailable.");
        // Original assignment-maker headers were checked before the retained cancellation is parsed.
        var command=CompanyWorkOrderPersonnelReleaseCommand.Read(row);var result=new CompanyWorkOrderPersonnelReleaseRecovery(command,row);result.Require(context.Company,context.ActorUserId,reference);
        var durable=await RecordPersonnelReleaseAsync(command,Guard,true);if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Complete original release changed during reference recovery.");await Guard();return new(command,durable);
    }
    private sealed class PersonnelReleaseInsertPolicy(WORK_ORDER_PERSONNEL_RELEASE original,CompanyWorkOrderPersonnelReleaseCommand command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_PERSONNEL_RELEASE expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(WORK_ORDER_PERSONNEL_RELEASE) || table!="WORK_ORDER_PERSONNEL_RELEASE" || operation!=PPDMRowMutation.Insert || previous is not null || !ReferenceEquals(entity,original) || actor!=expected.ROW_CREATED_BY)throw new InvalidOperationException("Only one exact original personnel cancellation append is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Personnel cancellation append grants no general reads.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Personnel cancellation append grants no general reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Personnel cancellation append grants no general reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Personnel cancellation append grants no general access.");
    }
}
