using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

public sealed partial class WorkOrderExecutionPermitService
{
    private const string LaborTable="WORK_ORDER_LABOR_USAGE";
    private void RequireLaborSchema(IDataSource source,Type type,string table,string key)
    {
        if(!source.CheckEntityExist(table))throw new InvalidOperationException("Review the explicit Beep CORE labor and assignment migration before reporting time.");
        var actual=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
        var expected=editor.classCreator.ConvertToEntityStructure(type);
        if(actual?.Fields is null || expected?.Fields is null || actual.Fields.GroupBy(row=>row.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) ||
            !actual.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName).SequenceEqual(new[]{key},StringComparer.Ordinal))throw new InvalidOperationException("Complete labor source schema and unique native identity are required.");
        foreach(var field in expected.Fields)
        {
            var found=actual.Fields.SingleOrDefault(row=>row.FieldName==field.FieldName);
            if(found is null || found.Fieldtype!=field.Fieldtype || !field.AllowDBNull && found.AllowDBNull || field.Size1>0 && found.Size1<field.Size1 || field.IsKey && !found.IsKey)
                throw new InvalidOperationException("Labor source type/nullability/body capacity differs from its explicit Beep manifest.");
        }
    }
    private async Task<IReadOnlyList<T>> ReadLaborRowsAsync<T>(FieldSelectionReview context,string table,string key,IReadOnlyList<AppFilter> filters,Func<Task> authority,bool owned) where T:class
    {
        var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original labor CORE source is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original labor CORE source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            RequireLaborSchema(source,typeof(T),table,key);
        }
        await Guard();var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(T),context.Database.ConnectionName,table);
        var raw=(await repo.GetAsync(filters.ToList())).ToArray();
        if(raw.Length>CompanyWorkOrderLaborUsageReview.MaximumEntries || raw.Any(row=>row is not T))throw new InvalidOperationException("Complete bounded typed labor source membership is required.");
        var frozen=raw.Cast<T>().Select(CompanyInventorySourceReview.Copy).ToArray();await Guard();return frozen;
    }
    private static List<AppFilter> LaborCompanyFilters(FieldSelectionReview context)=>[
        new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},
        new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId}];
    public Task<CompanyWorkOrderLaborUsageFacts> ReadLaborFactsAsync(FieldSelectionReview context,string workOrderId,string laborId,Func<Task> authority)
        =>ReadLaborFactsCoreAsync(context,workOrderId,laborId,authority,false);
    public Task<CompanyWorkOrderLaborUsageFacts> ReadOwnedLaborFactsAsync(FieldSelectionReview context,string workOrderId,string laborId,Func<Task> authority)
        =>ReadLaborFactsCoreAsync(context,workOrderId,laborId,authority,true);
    private async Task<CompanyWorkOrderLaborUsageFacts> ReadLaborFactsCoreAsync(FieldSelectionReview context,string workOrderId,string laborId,Func<Task> authority,bool owned)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(workOrderId) || !LeaseSummaryValidator.Identifier(laborId))throw new ArgumentException("Original native work-order and labor identities are required.");
        async Task<CompanyWorkOrderLaborUsageFacts> Read()
        {
            var filters=LaborCompanyFilters(context);filters.Add(new(){FieldName="LABOR_ID",Operator="=",FilterValue=laborId});
            var rows=await ReadLaborRowsAsync<WORK_ORDER_LABOR_USAGE>(context,LaborTable,"LABOR_ID",filters,authority,owned);
            if(rows.Count!=1)throw new InvalidOperationException("One original native labor report is required for financial quantity evidence.");var row=rows[0];
            if(row.LABOR_ID!=laborId || row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.WORK_ORDER_ID!=workOrderId || row.FIELD_ID!=context.FieldId)
                throw new UnauthorizedAccessException("Original labor company/order/FIELD closes before its private quantity proof.");
            var result=CompanyWorkOrderLaborUsageFacts.From(row);if(result.Database!=context.Database)throw new InvalidOperationException("Original labor native CORE binding changed.");return result;
        }
        var first=await Read();var closing=await Read();if(first!=closing)throw new InvalidOperationException("Original recorded labor quantity/audit changed during financial observation.");await authority();return closing;
    }

    public async Task<CompanyWorkOrderLaborUsageReview> ReviewLaborAsync(FieldSelectionReview context,CompanyWorkOrderExecutionStartReference reference,Func<Task> authority,
        Func<bool,Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,Func<Task<CompanyMaintenanceAssignedStageReview>> readStage)
        =>await ReviewLaborCoreAsync(context,reference,authority,readCurrent,readStage,false,null);

    private async Task<CompanyWorkOrderLaborUsageReview> ReviewLaborCoreAsync(FieldSelectionReview context,CompanyWorkOrderExecutionStartReference reference,Func<Task> authority,
        Func<bool,Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,Func<Task<CompanyMaintenanceAssignedStageReview>> readStage,bool owned,string? excludeLabor)
    {
        reference.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        async Task<CompanyWorkOrderLaborUsageReview> Observe()
        {
            var proof=await ReadExecutionRecordingProofAsync(context,reference,authority,()=>readCurrent(owned),owned);
            var original=proof.Original.Command.Review.Prerequisites.Personnel;var task=reference.Permit.Task;
            var assignmentFilters=LaborCompanyFilters(context);assignmentFilters.Add(new(){FieldName="ASSIGNMENT_ID",Operator="=",FilterValue=original.AssignmentId});
            var assignments=await ReadLaborRowsAsync<WORK_ORDER_PERSONNEL_ASSIGNMENT>(context,"WORK_ORDER_PERSONNEL_ASSIGNMENT","ASSIGNMENT_ID",assignmentFilters,authority,owned);
            if(assignments.Count!=1)throw new InvalidOperationException("One original native employee assignment is required.");var assignment=assignments[0];
            if(assignment.OWNER_TENANT_ID!=context.Company.TenantId || assignment.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || assignment.FIELD_ID!=task.FieldId ||
                assignment.WORK_ORDER_ID!=task.WorkOrderId || assignment.PROCESS_INSTANCE_ID!=task.InstanceId || assignment.EMPLOYEE_BA_ID!=original.EmployeeBaId || assignment.ASSIGNMENT_ID!=original.AssignmentId ||
                assignment.PERMIT_INTENT_ID!=reference.Permit.IntentId.ToString("D"))throw new UnauthorizedAccessException("Original assignment company/task/employee closes before its private proof.");
            var facts=CompanyWorkOrderPersonnelAssignmentFacts.From(assignment);if(facts!=original || facts.Database!=context.Database)throw new InvalidOperationException("Original approved duty assignment changed.");
            var releaseFilters=LaborCompanyFilters(context);releaseFilters.Add(new(){FieldName="ASSIGNMENT_ID",Operator="=",FilterValue=original.AssignmentId});
            var releases=await ReadLaborRowsAsync<WORK_ORDER_PERSONNEL_RELEASE>(context,"WORK_ORDER_PERSONNEL_RELEASE","RELEASE_ID",releaseFilters,authority,owned);
            if(releases.Count>1)throw new InvalidOperationException("One complete original assignment cancellation population is required.");DateTime? released=null;
            foreach(var row in releases)
            {
                if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.ASSIGNMENT_ID!=original.AssignmentId || row.ROW_CREATED_BY!=original.AssignedBy)
                    throw new UnauthorizedAccessException("Original assignment cancellation owner closes before private proof.");
                released=CompanyWorkOrderPersonnelReleaseFacts.From(row,assignment).ReleasedAtUtc;
            }
            var laborFilters=LaborCompanyFilters(context);laborFilters.Add(new(){FieldName="EMPLOYEE_BA_ID",Operator="=",FilterValue=original.EmployeeBaId});
            var population=await ReadLaborRowsAsync<WORK_ORDER_LABOR_USAGE>(context,LaborTable,"LABOR_ID",laborFilters,authority,owned);
            if(population.Select(row=>row.LABOR_ID).Distinct(StringComparer.Ordinal).Count()!=population.Count)throw new InvalidOperationException("Complete unique employee labor population is required.");
            var occupancies=new List<CompanyWorkOrderLaborOccupancy>();
            foreach(var row in population)
            {
                if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.EMPLOYEE_BA_ID!=original.EmployeeBaId)
                    throw new UnauthorizedAccessException("Labor company/employee closes before its private report.");
                var command=CompanyWorkOrderLaborUsageCommand.Read(row);if(command.Review.Execution.Current.Context.Database!=context.Database)throw new InvalidOperationException("Original labor native binding differs.");
                if(row.LABOR_ID!=excludeLabor)occupancies.Add(new(row.LABOR_ID,row.RECORDED_REVISION,row.LABOR_FROM_UTC,row.LABOR_TO_UTC));
            }
            var stage=CompanyInventorySourceReview.Copy(await readStage());await authority();
            var result=new CompanyWorkOrderLaborUsageReview(proof.Comparison,stage,facts,occupancies.ToArray(),released,DateTime.UtcNow);result.Require();return result;
        }
        var first=await Observe();var closing=await Observe();
        if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original start/assignment/cancellation/employee time population changed during review.");
        await authority();return closing;
    }
    private async Task<WORK_ORDER_LABOR_USAGE?> ReadOriginalLaborAsync(FieldSelectionReview context,CompanyWorkOrderLaborUsageReference reference,Func<Task> authority,bool owned)
    {
        reference.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        var id="wo-labor-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=context.Company,reference.IntentId}));
        var filters=LaborCompanyFilters(context);filters.Add(new(){FieldName="LABOR_ID",Operator="=",FilterValue=id});
        var rows=await ReadLaborRowsAsync<WORK_ORDER_LABOR_USAGE>(context,LaborTable,"LABOR_ID",filters,authority,owned);
        if(rows.Count>1)throw new InvalidOperationException("One original native labor intent is required.");if(rows.Count==0)return null;var row=rows[0];var task=reference.Start.Permit.Task;
        if(context.FieldId!=task.FieldId || row.LABOR_ID!=id || row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId ||
            row.EXECUTOR_USER_ID!=context.ActorUserId || row.FIELD_ID!=task.FieldId || row.WORK_ORDER_ID!=task.WorkOrderId || row.PROCESS_INSTANCE_ID!=task.InstanceId || row.START_ID!=reference.Start.StartId(context.Company))
            throw new UnauthorizedAccessException("Original labor company/account/task/start closes before its private report.");
        var command=CompanyWorkOrderLaborUsageCommand.Read(row);new CompanyWorkOrderLaborUsageRecovery(command,row).Require(context.Company,context.ActorUserId,reference);
        if(command.Review.Execution.Current.Context.Database!=context.Database)throw new InvalidOperationException("Original labor native binding changed.");return row;
    }
    public async Task<CompanyWorkOrderLaborUsageRecovery> RecoverLaborAsync(FieldSelectionReview context,CompanyWorkOrderLaborUsageReference reference,Func<Task> authority)
    {
        var first=await ReadOriginalLaborAsync(context,reference,authority,false)??throw new InvalidOperationException("Original labor is unconfirmed; read-only recovery cannot report time.");
        var closing=await ReadOriginalLaborAsync(context,reference,authority,false);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original labor audit changed during recovery.");
        var result=new CompanyWorkOrderLaborUsageRecovery(CompanyWorkOrderLaborUsageCommand.Read(first),first);result.Require(context.Company,context.ActorUserId,reference);await authority();return result;
    }
    public async Task<CompanyWorkOrderLaborUsageRecovery> RecordLaborAsync(CompanyWorkOrderLaborUsageCommand original,Func<Task> authority,
        Func<bool,Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,Func<Task<CompanyMaintenanceAssignedStageReview>> readStage)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Review.Execution.Current.Context;var reference=command.Reference();
        var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original labor CORE source is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original labor CORE binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        var existing=await ReadOriginalLaborAsync(context,reference,()=>Guard(false),false);
        if(existing is not null){command.RequireRecorded(existing);return await RecoverLaborAsync(context,reference,()=>Guard(false));}
        // Lifecycle may be a separate physical datasource. Close its authoritative stage before and after
        // the CORE transaction; only the retained stage is read while CORE owns its serializable command.
        var stage=CompanyInventorySourceReview.Copy(await readStage());
        async Task Proof(bool owned,string? exclude=null)=>command.RequireCurrent(await ReviewLaborCoreAsync(context,reference.Start,()=>Guard(owned),readCurrent,
            owned?()=>Task.FromResult(stage):readStage,owned,exclude));
        await Proof(false);WORK_ORDER_LABOR_USAGE? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Proof(true);var before=await ReadOriginalLaborAsync(context,reference,()=>Guard(true),true);
            if(before is not null){command.RequireRecorded(before);recorded=before;}
            else
            {
                var row=command.Prepare(DateTime.UtcNow);var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_LABOR_USAGE),context.Database.ConnectionName,LaborTable,
                    rowAccess:new LaborInsertPolicy(row,command,()=>Guard(true)));
                await repo.InsertAsync(row,context.ActorUserId);recorded=await ReadOriginalLaborAsync(context,reference,()=>Guard(true),true)??throw new InvalidOperationException("Original labor insert is unconfirmed.");command.RequireRecorded(recorded);
                if(recorded.ROW_CREATED_DATE!=row.ROW_CREATED_DATE)throw new InvalidOperationException("Original labor audit changed during readback.");
            }
            await Proof(true,command.LaborId());
        });
        await Proof(false,command.LaborId());if(JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(await readStage()))throw new InvalidOperationException("Original lifecycle duty changed around labor reporting; recover the original intent.");
        var recovery=await RecoverLaborAsync(context,reference,()=>Guard(false));
        if(recorded is null || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(recovery.Recorded))throw new InvalidOperationException("Labor acknowledgement is unconfirmed; recover its original reference without replay.");return recovery;
    }
    private sealed class LaborInsertPolicy(WORK_ORDER_LABOR_USAGE original,CompanyWorkOrderLaborUsageCommand command,Func<Task> authority):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_LABOR_USAGE expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await authority();if(type!=typeof(WORK_ORDER_LABOR_USAGE) || table!=LaborTable || !ReferenceEquals(entity,original) || previous is not null || operation!=PPDMRowMutation.Insert || actor!=expected.EXECUTOR_USER_ID)
                throw new InvalidOperationException("Only the exact original owned labor insert is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.SOURCE=expected.SOURCE;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;
            original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Labor insert grants no general row access.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Labor insert grants no general row access.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Labor insert grants no general row access.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Labor insert grants no general row access.");
    }
}
