using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public partial class ContractorManagementService
{
    public async Task<CompanyWorkOrderPersonnelAssignmentRoster> ReadPersonnelAssignmentsAsync(FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference permit,Func<Task> requireAuthority)
    {
        permit.Require();ArgumentNullException.ThrowIfNull(requireAuthority);FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(context.FieldId!=permit.Task.FieldId)throw new UnauthorizedAccessException("Original personnel roster FIELD differs.");
        await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel roster source is unavailable.");
        async Task Guard()
        {
            await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
            if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original personnel roster native target changed.");
            PPDMDataSourceTransaction.RequireIdle(source);RequirePersonnelAssignmentSchema(source);
        }
        async Task<WORK_ORDER_PERSONNEL_ASSIGNMENT[]> Read()
        {
            await Guard();var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT),context.Database.ConnectionName,"WORK_ORDER_PERSONNEL_ASSIGNMENT");
            var rows=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},
                new(){FieldName="FIELD_ID",Operator="=",FilterValue=context.FieldId},new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=permit.Task.WorkOrderId},new(){FieldName="PROCESS_INSTANCE_ID",Operator="=",FilterValue=permit.Task.InstanceId},
                new(){FieldName="PERMIT_INTENT_ID",Operator="=",FilterValue=permit.IntentId.ToString("D")},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
            if(rows.Length>4096 || rows.Any(row=>row is not WORK_ORDER_PERSONNEL_ASSIGNMENT))throw new InvalidOperationException("Complete bounded original personnel assignment roster is unavailable.");
            var typed=rows.Cast<WORK_ORDER_PERSONNEL_ASSIGNMENT>().ToArray();
            // Close every public owner/task header before deserializing any retained private proof.
            if(typed.Any(row=>row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.FIELD_ID!=context.FieldId || row.WORK_ORDER_ID!=permit.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=permit.Task.InstanceId || row.PERMIT_INTENT_ID!=permit.IntentId.ToString("D") || row.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Original personnel roster contains another company/permit/task.");
            if(typed.Select(row=>row.ASSIGNMENT_ID).Distinct(StringComparer.Ordinal).Count()!=typed.Length)throw new InvalidOperationException("Original personnel roster assignment identities are ambiguous.");
            return typed.Select(CompanyInventorySourceReview.Copy).OrderBy(row=>row.ASSIGNMENT_ID,StringComparer.Ordinal).ToArray();
        }
        var first=await Read();var firstReleases=await ReadPersonnelReleasesCoreAsync(context,first,Guard,false);var closing=await Read();var closingReleases=await ReadPersonnelReleasesCoreAsync(context,closing,Guard,false);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(firstReleases)!=JsonSerializer.Serialize(closingReleases))throw new InvalidOperationException("Complete original personnel roster/cancellation evidence changed during review.");
        var facts=closing.Select(CompanyWorkOrderPersonnelAssignmentFacts.From).ToArray();await Guard();
        var releases=closingReleases.Select(row=>CompanyWorkOrderPersonnelReleaseFacts.From(row,closing.Single(original=>original.ASSIGNMENT_ID==row.ASSIGNMENT_ID))).ToArray();
        return CompanyWorkOrderPersonnelAssignmentRoster.Create(context,permit,facts,DateTime.UtcNow,releases);
    }
    public async Task<CompanyWorkOrderPersonnelAssignmentRecovery> RecoverPersonnelAssignmentReferenceAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelAssignmentReference reference,Func<Task> requireAuthority)
    {
        reference.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(reference.Permit.Task.FieldId!=context.FieldId)throw new UnauthorizedAccessException("Original personnel recovery FIELD differs.");
        await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel recovery source is unavailable.");
        async Task Guard()
        {
            await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
            if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original personnel recovery native target changed.");
            PPDMDataSourceTransaction.RequireIdle(source);RequirePersonnelAssignmentSchema(source);
        }
        await Guard();
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT),context.Database.ConnectionName,"WORK_ORDER_PERSONNEL_ASSIGNMENT");
        var rows=(await repo.GetAsync([new(){FieldName="ASSIGNMENT_ID",Operator="=",FilterValue=reference.AssignmentId(context.Company)}])).ToArray();await Guard();
        if(rows.Length!=1 || rows[0] is not WORK_ORDER_PERSONNEL_ASSIGNMENT row)throw new InvalidOperationException("Exact original personnel assignment is unavailable.");
        if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.ROW_CREATED_BY!=context.ActorUserId || row.FIELD_ID!=context.FieldId || row.WORK_ORDER_ID!=reference.Permit.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=reference.Permit.Task.InstanceId)throw new UnauthorizedAccessException("Original recorded assignment actor/company/task must close before private evidence is read.");
        if(string.IsNullOrEmpty(row.COMMAND_JSON) || row.COMMAND_JSON.Length>2*1024*1024)throw new InvalidOperationException("Original bounded personnel assignment body is unavailable.");
        CompanyWorkOrderPersonnelAssignmentCommand command;
        try{command=JsonSerializer.Deserialize<CompanyWorkOrderPersonnelAssignmentCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original command is unavailable.");}
        catch(JsonException){throw new InvalidOperationException("Original command is malformed.");}
        var result=new CompanyWorkOrderPersonnelAssignmentRecovery(command,row);result.Require(context.Company,context.ActorUserId,reference);
        var saved=await RecordPersonnelAssignmentAsync(command,Guard,()=>throw new InvalidOperationException("Private reference recovery cannot create a new assignment."),true);
        if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(saved))throw new InvalidOperationException("Complete original assignment changed during reference recovery.");
        await Guard();return new(command,saved);
    }
    public async Task<WORK_ORDER_PERSONNEL_ASSIGNMENT> RecordPersonnelAssignmentAsync(CompanyWorkOrderPersonnelAssignmentCommand original,Func<Task> requireAuthority,Func<Task> requireCurrentSource,bool readOnly=false,Func<Task>? requireCoreSource=null)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();
        ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireCurrentSource);
        var context=command.Review.Personnel.Context;var query=command.Review.Personnel.Query;
        await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel assignment source is unavailable.");
        const string table="WORK_ORDER_PERSONNEL_ASSIGNMENT";
        async Task Guard(bool owned=false)
        {
            await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
            if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)) || !source.CheckEntityExist(table))throw new InvalidOperationException("Review the PPDM_CORE Beep assignment manifest and original binding.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            RequirePersonnelAssignmentSchema(source);
        }
        PPDMGenericRepository Repo()=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT),context.Database.ConnectionName,table);
        async Task<WORK_ORDER_PERSONNEL_ASSIGNMENT?> Read(bool owned=false)
        {
            await Guard(owned);var rows=(await Repo().GetAsync([new(){FieldName="ASSIGNMENT_ID",Operator="=",FilterValue=command.AssignmentId()}])).ToArray();await Guard(owned);
            if(rows.Length>1 || rows.Any(row=>row is not WORK_ORDER_PERSONNEL_ASSIGNMENT))throw new InvalidOperationException("Original assignment is ambiguous.");
            if(rows.Length==0)return null;var row=(WORK_ORDER_PERSONNEL_ASSIGNMENT)rows[0];
            // Public owner and actor close before the retained private body is inspected.
            if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.ROW_CREATED_BY!=context.ActorUserId || row.FIELD_ID!=context.FieldId || row.WORK_ORDER_ID!=query.Permit.Task.WorkOrderId)throw new UnauthorizedAccessException("Only the original company/task assignment maker may recover private personnel proof.");
            command.RequireRecorded(row);return CompanyInventorySourceReview.Copy(row);
        }
        await Guard();var recorded=await Read();
        if(readOnly || recorded is not null)
        {
            if(recorded is null)throw new InvalidOperationException("Original assignment is not recorded; retain its intent before retrying.");
            if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(await Read()))throw new InvalidOperationException("Complete original assignment changed during recovery.");
            return recorded;
        }
        async Task LocalProof()
        {
            var current=await ReadPersonnelSourceCoreAsync(context,query,()=>Guard(true),true);
            if(current.Employment!=command.Review.Personnel.Employment || current.License!=command.Review.Personnel.License)throw new InvalidOperationException("Original employee membership/licence changed inside assignment.");
            var permit=command.Review.PermitReview;
            if(!source.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT"))throw new InvalidOperationException("Original local pending permit schema is unavailable.");
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),context.Database.ConnectionName,"WORK_ORDER_EXECUTION_PERMIT");
            var rows=(await repo.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=permit.Proposal.PermitId()}])).ToArray();
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_EXECUTION_PERMIT row || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row))!=permit.RecordedRevision)throw new InvalidOperationException("Complete original pending permit changed inside assignment.");
            permit.Proposal.RequireRecorded(row);await (requireCoreSource??requireCurrentSource)();await Guard(true);
        }
        async Task RequireNoOverlap(string? ownAssignmentId=null)
        {
            // Complete bounded native population, never a first-page assumption.
            var rows=(await Repo().GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="EMPLOYEE_BA_ID",Operator="=",FilterValue=query.EmployeeBaId}])).ToArray();
            if(rows.Length>4096 || rows.Any(row=>row is not WORK_ORDER_PERSONNEL_ASSIGNMENT))throw new InvalidOperationException("Complete bounded personnel assignment population is unavailable.");
            var assignments=rows.Cast<WORK_ORDER_PERSONNEL_ASSIGNMENT>().ToArray();
            if(assignments.Select(row=>row.ASSIGNMENT_ID).Distinct(StringComparer.Ordinal).Count()!=assignments.Length)throw new InvalidOperationException("Native employee assignment identities are ambiguous.");
            var released=await ReadPersonnelReleasesCoreAsync(context,assignments,()=>Guard(true),true);
            foreach(var item in assignments)
            {
                if(item.OWNER_TENANT_ID!=context.Company.TenantId || item.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || item.EMPLOYEE_BA_ID!=query.EmployeeBaId || item.VALID_FROM_UTC==default || item.VALID_FROM_UTC.Kind!=DateTimeKind.Utc || item.VALID_TO_UTC.Kind!=DateTimeKind.Utc || item.VALID_TO_UTC<=item.VALID_FROM_UTC || item.ACTIVE_IND!="Y" || item.RECORDED_REVISION!=CompanyWorkOrderPersonnelAssignmentCommand.Digest(item))throw new InvalidOperationException("Complete original active employee assignment population is foreign or unconfirmed.");
                if(item.ASSIGNMENT_ID!=ownAssignmentId && !released.Any(row=>row.ASSIGNMENT_ID==item.ASSIGNMENT_ID) && item.VALID_FROM_UTC<command.ValidToUtc && command.ValidFromUtc<item.VALID_TO_UTC)
                    throw new InvalidOperationException("An overlapping current employee assignment requires an explicit reviewed resolution.");
            }
            await Guard(true);
        }
        await requireCurrentSource();await Guard();WORK_ORDER_PERSONNEL_ASSIGNMENT? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);confirmed=await Read(true);if(confirmed is not null)return;
            await LocalProof();await RequireNoOverlap();var row=command.Prepare(DateTime.UtcNow);
            var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT),context.Database.ConnectionName,table,rowAccess:new PersonnelAssignmentInsertPolicy(row,command,()=>Guard(true)));
            await writer.InsertAsync(row,context.ActorUserId);confirmed=await Read(true);
            if(confirmed is null)throw new InvalidOperationException("Original personnel assignment insert is unconfirmed.");
            await LocalProof();await RequireNoOverlap(row.ASSIGNMENT_ID);await Guard(true);
        });
        var durable=await Read();if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed original personnel assignment is unconfirmed. Recover the retained intent.");
        await requireCurrentSource();await Guard();return durable!;
    }
    private static void RequirePersonnelAssignmentSchema(IDataSource source)
    {
        const string table="WORK_ORDER_PERSONNEL_ASSIGNMENT";
        if(!source.CheckEntityExist(table))throw new InvalidOperationException("Review the existing PPDM_CORE Beep personnel assignment manifest.");
        var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=shape?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="ASSIGNMENT_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Exact assignment key metadata is unavailable.");
        foreach(var (name,size) in new[]{("COMMAND_JSON",2*1024*1024),("RECORDED_REVISION",64)})
        {
            var fields=shape!.Fields.Where(field=>field.FieldName==name).ToArray();
            if(fields.Length!=1 || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Bounded native assignment proof metadata is unavailable.");
        }
    }
    private sealed class PersonnelAssignmentInsertPolicy(WORK_ORDER_PERSONNEL_ASSIGNMENT original,CompanyWorkOrderPersonnelAssignmentCommand command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_PERSONNEL_ASSIGNMENT expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT) || table!="WORK_ORDER_PERSONNEL_ASSIGNMENT" || operation!=PPDMRowMutation.Insert || previous is not null || actor!=expected.ROW_CREATED_BY || !ReferenceEquals(entity,original))throw new InvalidOperationException("Only one original native personnel assignment append is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;
            command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Personnel append grants no general reads.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Personnel append grants no general reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Personnel append grants no general reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Personnel append grants no general access.");
    }
}
