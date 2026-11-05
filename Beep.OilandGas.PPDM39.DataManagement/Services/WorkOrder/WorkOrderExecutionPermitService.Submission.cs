using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public sealed partial class WorkOrderExecutionPermitService
{
    private const string SubmissionTable="WORK_ORDER_EXECUTION_SUBMISSION";
    private void RequireSubmissionSchema(IDataSource source)
    {
        if(!source.CheckEntityExist(SubmissionTable))throw new InvalidOperationException("Review the explicit PPDM_CORE Beep work-submission migration before recording actual work.");
        var actual=source.GetEntityStructure(new EntityStructure{EntityName=SubmissionTable,DatasourceEntityName=SubmissionTable},true);var expected=editor.classCreator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_SUBMISSION));
        if(actual?.Fields is null || expected?.Fields is null || actual.Fields.GroupBy(row=>row.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) || !actual.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName).SequenceEqual(new[]{"SUBMISSION_ID"},StringComparer.Ordinal))throw new InvalidOperationException("Complete physical work-submission schema and unique native key are required.");
        foreach(var field in expected.Fields)
        {
            var found=actual.Fields.SingleOrDefault(row=>row.FieldName==field.FieldName);
            if(found is null || found.Fieldtype!=field.Fieldtype || !field.AllowDBNull && found.AllowDBNull || field.Size1>0 && found.Size1<field.Size1 || field.IsKey && !found.IsKey)throw new InvalidOperationException("Work-submission type/nullability/body capacity differs from its explicit Beep manifest.");
        }
    }
    private async Task<WORK_ORDER_EXECUTION_SUBMISSION?> ReadSubmissionAsync(FieldSelectionReview context,CompanyWorkOrderExecutionStartReference reference,Func<Task> authority,bool owned)
    {
        reference.Require();var row=await ReadSubmissionRowAsync(context,reference.Permit.Task,authority,owned,true,reference.StartId(context.Company));
        if(row is not null){var command=CompanyWorkOrderSubmissionCommand.Read(row);if(command.Review.Inspections.Start.Permit!=reference.Permit || command.Review.Inspections.Start.IntentId!=reference.IntentId)throw new InvalidOperationException("Original work submission/start reference differs.");}return row;
    }
    public async Task<CompanyWorkOrderSubmissionFacts> ReadSubmissionFactsAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<Task> authority,bool owned=false)
    {
        query.Require();async Task<WORK_ORDER_EXECUTION_SUBMISSION> Read()
        {
            var row=await ReadSubmissionRowAsync(context,query.Task,authority,owned)??throw new InvalidOperationException("Original recorded work report is unavailable.");
            if(row.SUBMISSION_ID!=query.SubmissionId)throw new InvalidOperationException("Original complete work-report population differs from the requested submission.");return row;
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original work report changed during reviewer observation.");
        var result=CompanyWorkOrderSubmissionFacts.From(closing);result.Require();await authority();return result;
    }
    public async Task<CompanyWorkOrderCompletionQuery> DiscoverSubmissionQueryAsync(FieldSelectionReview context,CompanyMaintenanceReceivingQuery task,Func<Task> authority)
    {
        task.Require();var first=await ReadSubmissionRowAsync(context,task,authority,false)??throw new InvalidOperationException("Original recorded work report is unavailable.");var closing=await ReadSubmissionRowAsync(context,task,authority,false);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original complete report population changed during discovery.");var result=CompanyWorkOrderSubmissionFacts.From(first).Query;await authority();return result;
    }
    public async Task<CompanyWorkOrderSubmissionFacts?> ReadSubmissionHandoffProofAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,string lifecycleConnection,bool ownedLifecycle,Func<Task> authority)
    {
        var core=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original report CORE source is unavailable.");var lifecycle=editor.GetDataSource(lifecycleConnection)??throw new InvalidOperationException("Original report handoff lifecycle source is unavailable.");
        if(ownedLifecycle && !ReferenceEquals(core,lifecycle))
        {
            PPDMDataSourceTransaction.RequireOwned(lifecycle,IsolationLevel.Serializable);await authority();
            if(!ReferenceEquals(core,editor.GetDataSource(context.Database.ConnectionName)) || !ReferenceEquals(lifecycle,editor.GetDataSource(lifecycleConnection)))throw new InvalidOperationException("Original separate CORE/lifecycle binding changed.");
            PPDMDataSourceTransaction.RequireOwned(lifecycle,IsolationLevel.Serializable);return null;
        }
        return await ReadSubmissionFactsAsync(context,query,authority,ownedLifecycle);
    }
    public async Task<CompanyWorkOrderCompletionReview?> ReadCompletionDecisionProofAsync(FieldSelectionReview context,string lifecycleConnection,bool ownedLifecycle,Func<Task> authority,Func<bool,Task<CompanyWorkOrderCompletionReview>> read)
    {
        var core=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original completion CORE source is unavailable.");var lifecycle=editor.GetDataSource(lifecycleConnection)??throw new InvalidOperationException("Original completion lifecycle source is unavailable.");
        async Task Guard(){await authority();if(!ReferenceEquals(core,editor.GetDataSource(context.Database.ConnectionName)) || !ReferenceEquals(lifecycle,editor.GetDataSource(lifecycleConnection)))throw new InvalidOperationException("Original completion CORE/lifecycle pointers changed.");if(ownedLifecycle)PPDMDataSourceTransaction.RequireOwned(lifecycle,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(lifecycle);}
        await Guard();if(ownedLifecycle && !ReferenceEquals(core,lifecycle))return null;var result=await read(ownedLifecycle);await Guard();return result;
    }
    private async Task<WORK_ORDER_EXECUTION_SUBMISSION?> ReadSubmissionRowAsync(FieldSelectionReview context,CompanyMaintenanceReceivingQuery task,Func<Task> authority,bool owned,bool ownExecutor=false,string? startId=null)
    {
        task.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(context.FieldId!=task.FieldId)throw new UnauthorizedAccessException("Original work-submission FIELD is required.");
        var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original work-submission source is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original work-submission source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireSubmissionSchema(source);
        }
        await Guard();var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_SUBMISSION),context.Database.ConnectionName,SubmissionTable);
        var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="FIELD_ID",Operator="=",FilterValue=task.FieldId},new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=task.WorkOrderId},new(){FieldName="PROCESS_INSTANCE_ID",Operator="=",FilterValue=task.InstanceId}])).ToArray();
        if(raw.Length>1 || raw.Any(value=>value is not WORK_ORDER_EXECUTION_SUBMISSION))throw new InvalidOperationException("One complete original work-submission population is required; replacement references cannot overwrite it.");
        if(raw.Length==0){await Guard();return null;}var row=(WORK_ORDER_EXECUTION_SUBMISSION)raw[0];
        if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || ownExecutor && row.EXECUTOR_USER_ID!=context.ActorUserId || row.FIELD_ID!=task.FieldId || row.WORK_ORDER_ID!=task.WorkOrderId || row.PROCESS_INSTANCE_ID!=task.InstanceId || startId is not null && row.START_ID!=startId)throw new UnauthorizedAccessException("Original submission company/account/FIELD/order/start closes before its private work report.");
        var captured=CompanyInventorySourceReview.Copy(row);await Guard();var command=CompanyWorkOrderSubmissionCommand.Read(captured);
        if(command.Review.Inspections.Current.Context.Database!=context.Database || command.Review.Inspections.Start.Permit.Task!=task)throw new InvalidOperationException("Original work-submission command/native binding differs.");return captured;
    }
    public async Task<CompanyWorkOrderSubmissionRecovery> RecoverSubmissionAsync(FieldSelectionReview context,CompanyWorkOrderSubmissionReference reference,Func<Task> authority)
    {
        reference.Require();var first=await ReadSubmissionAsync(context,reference.Start,authority,false)??throw new InvalidOperationException("Original work submission is unconfirmed; read-only recovery cannot submit work.");
        var result=new CompanyWorkOrderSubmissionRecovery(CompanyWorkOrderSubmissionCommand.Read(first),first);result.Require(context.Company,context.ActorUserId,reference);
        var closing=await ReadSubmissionAsync(context,reference.Start,authority,false);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original submission history changed during recovery.");await authority();return result;
    }
    public async Task<CompanyWorkOrderSubmissionReceipt> SubmitWorkAsync(CompanyWorkOrderSubmissionCommand original,Func<Task> authority,
        Func<bool,Task<CompanyWorkOrderExecutionRequirements>> readCurrent,Func<Task<CompanyMaintenanceAssignedStageReview>> readStage)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Review.Inspections.Current.Context;
        var start=new CompanyWorkOrderExecutionStartReference(command.Review.Inspections.Start.Permit,command.Review.Inspections.Start.IntentId);
        var reference=new CompanyWorkOrderSubmissionReference(start,command.IntentId);var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original submission core source is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original submission core source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireSubmissionSchema(source);
        }
        var existing=await ReadSubmissionAsync(context,start,()=>Guard(false),false);
        if(existing is not null){command.RequireRecorded(existing);return Receipt((await RecoverSubmissionAsync(context,reference,()=>Guard(false))).Recorded);}
        await Guard(false);command.RequireCurrentStage(await readStage());await Guard(false);
        WORK_ORDER_EXECUTION_SUBMISSION? recorded=null;
        async Task Proof(bool owned)
        {
            var comparison=await ReviewSubmissionInspectionsAsync(context,start,()=>Guard(owned),readCurrent,owned);
            command.RequireCurrent(new(comparison,command.Review.Stage));await Guard(owned);
        }
        await Proof(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Proof(true);var before=await ReadSubmissionAsync(context,start,()=>Guard(true),true);
            if(before is not null){command.RequireRecorded(before);recorded=before;}
            else
            {
                var row=command.Prepare(DateTime.UtcNow);var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_SUBMISSION),context.Database.ConnectionName,SubmissionTable,rowAccess:new SubmissionInsertPolicy(row,command,()=>Guard(true)));
                await repo.InsertAsync(row,context.ActorUserId);recorded=await ReadSubmissionAsync(context,start,()=>Guard(true),true)??throw new InvalidOperationException("Original work submission is unconfirmed after insert.");command.RequireRecorded(recorded);
                if(recorded.ROW_CREATED_DATE!=row.ROW_CREATED_DATE)throw new InvalidOperationException("Original work-submission audit changed during readback.");
            }
            await Proof(true);var closing=await ReadSubmissionAsync(context,start,()=>Guard(true),true);
            if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original submission body/audit changed around owned proof.");
        });
        await Proof(false);command.RequireCurrentStage(await readStage());await Guard(false);
        var recovery=await RecoverSubmissionAsync(context,reference,()=>Guard(false));
        if(recorded is null || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(recovery.Recorded))throw new InvalidOperationException("Original submission acknowledgement is unconfirmed; recover its original reference without replay.");return Receipt(recovery.Recorded);
        CompanyWorkOrderSubmissionReceipt Receipt(WORK_ORDER_EXECUTION_SUBMISSION row){command.RequireRecorded(row);var result=new CompanyWorkOrderSubmissionReceipt(context.Company,context.ActorUserId,command.IntentId,row.SUBMISSION_ID,row.START_ID,row.COMPLETED_AT_UTC,row.RECORDED_REVISION);result.Require(command);return result;}
    }
    private sealed class SubmissionInsertPolicy(WORK_ORDER_EXECUTION_SUBMISSION original,CompanyWorkOrderSubmissionCommand command,Func<Task> authority):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_EXECUTION_SUBMISSION expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await authority();if(type!=typeof(WORK_ORDER_EXECUTION_SUBMISSION) || table!=SubmissionTable || !ReferenceEquals(entity,original) || previous is not null || operation!=PPDMRowMutation.Insert || actor!=expected.EXECUTOR_USER_ID)throw new InvalidOperationException("Only the exact original owned work-submission insert is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.SOURCE=expected.SOURCE;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Work submission grants no general row access.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Work submission grants no general row access.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Work submission grants no general row access.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Work submission grants no general row access.");
    }
}
