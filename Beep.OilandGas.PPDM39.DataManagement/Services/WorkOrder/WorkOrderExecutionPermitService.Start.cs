using System.Data;
using System.Text.Json;
using TheTechIdea.Beep;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

public sealed partial class WorkOrderExecutionPermitService
{
    private const string StartTable="WORK_ORDER_EXECUTION_START";
    /// <summary>Closes the original start and approved material reservation against current native order/requirements.
    /// An owned PRODUCTION transaction is also CORE-owned only when both names resolve to the same actual datasource.</summary>
    public async Task<CompanyWorkOrderMaterialUseExecutionProof?> ReadMaterialUseExecutionProofAsync(FieldSelectionReview context,
        CompanyWorkOrderExecutionStartReference reference,string productionConnection,bool ownedProduction,Func<Task> authority,
        Func<bool,Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent)
    {
        reference.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readCurrent);
        await authority();var core=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original material execution CORE is unavailable.");
        var stock=editor.GetDataSource(productionConnection)??throw new InvalidOperationException("Original material execution PRODUCTION is unavailable.");
        var shared=ReferenceEquals(core,stock);var ownedCore=ownedProduction && shared;
        if(ownedProduction && !shared)
        {
            // Beep deliberately forbids cross-source reads inside any owned command. The caller must
            // retain and compare separate CORE proof before/after PRODUCTION; null never means closed CORE proof.
            PPDMDataSourceTransaction.RequireOwned(stock,IsolationLevel.Serializable);await authority();
            if(!ReferenceEquals(core,editor.GetDataSource(context.Database.ConnectionName)) || !ReferenceEquals(stock,editor.GetDataSource(productionConnection)))
                throw new InvalidOperationException("Original separate CORE/PRODUCTION binding changed during physical use.");
            PPDMDataSourceTransaction.RequireOwned(stock,IsolationLevel.Serializable);return null;
        }
        async Task Guard()
        {
            if(ownedProduction)PPDMDataSourceTransaction.RequireOwned(stock,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(stock);
            if(ownedCore)PPDMDataSourceTransaction.RequireOwned(core,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(core);
            await authority();if(!ReferenceEquals(core,editor.GetDataSource(context.Database.ConnectionName)) || !ReferenceEquals(stock,editor.GetDataSource(productionConnection)))
                throw new InvalidOperationException("Original CORE/PRODUCTION execution boundary changed.");
            if(ownedProduction)PPDMDataSourceTransaction.RequireOwned(stock,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(stock);
            if(ownedCore)PPDMDataSourceTransaction.RequireOwned(core,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(core);
        }
        await Guard();
        var proof=await ReadExecutionRecordingProofAsync(context,reference,Guard,()=>readCurrent(ownedCore),ownedCore);
        var result=new CompanyWorkOrderMaterialUseExecutionProof(proof.Comparison,proof.Original.Command.Review.Prerequisites.Material);
        result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;
    }
    private async Task<(CompanyWorkOrderExecutionStartRecovery Original,CompanyWorkOrderSubmissionInspectionReview Comparison)> ReadExecutionRecordingProofAsync(
        FieldSelectionReview context,CompanyWorkOrderExecutionStartReference reference,Func<Task> authority,
        Func<Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,bool ownedCore)
    {
        Task Guard()=>authority();
        var row=await ReadStartAsync(context,reference.Permit,Guard,ownedCore)??throw new InvalidOperationException("Original execution start is unavailable.");
        var command=CompanyWorkOrderExecutionStartCommand.Read(row);command.RequireRecorded(row);
        var original=new CompanyWorkOrderExecutionStartRecovery(command,row);original.Require(context.Company,context.ActorUserId,reference);
        if(await ReadSubmissionAsync(context,reference,Guard,ownedCore) is not null)
            throw new InvalidOperationException("Material use is closed by the original submitted work report; recover existing issues or review the work changes independently.");
        // This native owner already pairs complete current order/results and recorded approval reads.
        // Reuse that observation directly instead of wrapping it in a second paired DTO observation.
        var current=ownedCore?await ReadExecutionRequirementsCoreAsync(context,reference.Permit,Guard,readCurrent):
            await ReadExecutionRequirementsAsync(context,reference.Permit,Guard,readCurrent);
        var closing=await ReadStartAsync(context,reference.Permit,Guard,ownedCore);
        if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(closing))
            throw new InvalidOperationException("Original start changed around material execution proof.");
        if(await ReadSubmissionAsync(context,reference,Guard,ownedCore) is not null)
            throw new InvalidOperationException("An original work report was recorded during material execution review.");
        var comparison=CompanyWorkOrderSubmissionInspectionReview.Create(original,current,DateTime.UtcNow);
        await Guard();return(original,comparison);
    }
    private void RequireStartSchema(IDataSource source)
    {
        if(!source.CheckEntityExist(StartTable))throw new InvalidOperationException("Review the explicit PPDM_CORE Beep execution-start migration before physical work.");
        var actual=source.GetEntityStructure(new EntityStructure{EntityName=StartTable,DatasourceEntityName=StartTable},true);
        var expected=editor.classCreator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_START));
        if(actual?.Fields is null || expected?.Fields is null || actual.Fields.GroupBy(row=>row.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) ||
            !actual.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName).SequenceEqual(new[]{"START_ID"},StringComparer.Ordinal))throw new InvalidOperationException("Complete physical execution-start schema and unique native key are required.");
        foreach(var field in expected.Fields)
        {
            var found=actual.Fields.SingleOrDefault(row=>row.FieldName==field.FieldName);
            if(found is null || found.Fieldtype!=field.Fieldtype || !field.AllowDBNull && found.AllowDBNull || field.Size1>0 && found.Size1<field.Size1 || field.IsKey && !found.IsKey)throw new InvalidOperationException("Complete execution-start column type/nullability/body capacity differs from its Beep manifest.");
        }
    }
    private async Task<WORK_ORDER_EXECUTION_START?> ReadStartAsync(FieldSelectionReview current,CompanyWorkOrderExecutionPermitReference permit,Func<Task> authority,bool owned,bool privateActor=true)
    {
        permit.Require();FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(current.FieldId!=permit.Task.FieldId)throw new UnauthorizedAccessException("Original execution start FIELD is required.");
        var source=editor.GetDataSource(current.Database.ConnectionName)??throw new InvalidOperationException("Original execution-start target is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,editor.GetDataSource(current.Database.ConnectionName)))throw new InvalidOperationException("Original execution-start target changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireStartSchema(source);
        }
        await Guard();var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_START),current.Database.ConnectionName,StartTable);
        var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=current.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=current.Company.BusinessAssociateId},new(){FieldName="FIELD_ID",Operator="=",FilterValue=permit.Task.FieldId},new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=permit.Task.WorkOrderId},new(){FieldName="PROCESS_INSTANCE_ID",Operator="=",FilterValue=permit.Task.InstanceId}])).ToArray();
        if(raw.Length>1 || raw.Any(value=>value is not WORK_ORDER_EXECUTION_START))throw new InvalidOperationException("One original native work-start population is required; duplicates are never adopted.");
        if(raw.Length==0){await Guard();return null;}
        var row=(WORK_ORDER_EXECUTION_START)raw[0];
        if(row.OWNER_TENANT_ID!=current.Company.TenantId || row.OWNER_COMPANY_BA_ID!=current.Company.BusinessAssociateId || row.FIELD_ID!=permit.Task.FieldId || row.WORK_ORDER_ID!=permit.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=permit.Task.InstanceId || privateActor && row.EXECUTOR_USER_ID!=current.ActorUserId)
            throw new UnauthorizedAccessException("Original execution-start company/account/FIELD/order/task closes before its private command.");
        var captured=CompanyInventorySourceReview.Copy(row);await Guard();
        var command=CompanyWorkOrderExecutionStartCommand.Read(captured);var original=command.Review.Prerequisites.Requirements;
        if(original.Context.Company!=current.Company || privateActor && original.Context.ActorUserId!=current.ActorUserId || original.Context.Database!=current.Database || original.Approval.Permit!=permit)throw new InvalidOperationException("Original execution-start command/binding differs from the requested permit/task.");return captured;
    }
    public async Task<CompanyWorkOrderRecordedStart?> ReadRecordedStartAsync(FieldSelectionReview current,CompanyWorkOrderExecutionPermitReference query,Func<Task> authority)
    {
        var first=await ReadStartAsync(current,query,authority,false,false);var closing=await ReadStartAsync(current,query,authority,false,false);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete recorded work-start population changed during public history review.");
        await authority();if(first is null)return null;var command=CompanyWorkOrderExecutionStartCommand.Read(first);
        var result=new CompanyWorkOrderRecordedStart(current.Company,query,command.IntentId,first.START_ID,first.PERMIT_ID,first.EXECUTOR_USER_ID,first.EMPLOYEE_BA_ID,command.Review.Stage.AssignedRole,command.Review.Stage.StepId,first.STARTED_AT_UTC,first.ROW_CREATED_DATE!.Value,first.PREREQUISITE_REVISION,first.RECORDED_REVISION){QualifiedWorkAssets=command.Review.QualifiedWorkAssets};
        result.Require(current.Company,query);return result;
    }
    public async Task<CompanyWorkOrderExecutionStartRecovery> RecoverExecutionStartAsync(FieldSelectionReview current,CompanyWorkOrderExecutionStartReference reference,Func<Task> authority)
    {
        reference.Require();var first=await ReadStartAsync(current,reference.Permit,authority,false)??throw new InvalidOperationException("The original work start is unconfirmed; read-only recovery cannot start work.");
        if(first.START_ID!=reference.StartId(current.Company))throw new InvalidOperationException("This task already has a different original start reference.");
        var command=CompanyWorkOrderExecutionStartCommand.Read(first);var result=new CompanyWorkOrderExecutionStartRecovery(command,first);result.Require(current.Company,current.ActorUserId,reference);
        var closing=await ReadStartAsync(current,reference.Permit,authority,false);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original recorded work-start history changed during recovery.");await authority();return result;
    }
    public Task<CompanyWorkOrderExecutionStartReceipt> StartExecutionAsync(CompanyWorkOrderExecutionStartCommand original,Func<Task> authority,
        Func<bool,Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,
        ICompanyPersonnelAcceptanceCoreProof personnel,ICompanyWorkOrderDocumentContributorCore documents,ICompanyRecordedReviewerIdentityCore identities,
        ICompanyWorkOrderExecutorIdentityOwner executorIdentities,Func<CompanyWorkOrderExecutionPermitApprovalReview,Task> currentStock,
        Func<CompanyWorkOrderExecutionPermitApprovalReview,Task<bool>> ownedStock,Func<Task<CompanyMaintenanceAssignedStageReview>> readStage)
        =>StartExecutionAsync(original,authority,readCurrent,personnel,documents,identities,executorIdentities,currentStock,ownedStock,readStage,null);
    public async Task<CompanyWorkOrderExecutionStartReceipt> StartExecutionAsync(CompanyWorkOrderExecutionStartCommand original,Func<Task> authority,
        Func<bool,Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,
        ICompanyPersonnelAcceptanceCoreProof personnel,ICompanyWorkOrderDocumentContributorCore documents,ICompanyRecordedReviewerIdentityCore identities,
        ICompanyWorkOrderExecutorIdentityOwner executorIdentities,Func<CompanyWorkOrderExecutionPermitApprovalReview,Task> currentStock,
        Func<CompanyWorkOrderExecutionPermitApprovalReview,Task<bool>> ownedStock,Func<Task<CompanyMaintenanceAssignedStageReview>> readStage,Func<bool,Task<CompanyMaintenanceQualifiedAssets>>? readQualifiedWorkAssets)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();ArgumentNullException.ThrowIfNull(executorIdentities);ArgumentNullException.ThrowIfNull(readStage);
        var context=command.Review.Prerequisites.Requirements.Context;var permit=command.Review.Prerequisites.Requirements.Approval.Permit;
        var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original execution-start source is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original execution-start source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireStartSchema(source);
        }
        var existing=await ReadStartAsync(context,permit,()=>Guard(false),false);
        if(existing is not null)
        {
            command.RequireRecorded(existing);var recovered=await RecoverExecutionStartAsync(context,new(permit,command.IntentId),()=>Guard(false));return Receipt(recovered.Recorded);
        }
        command.RequireQualifiedWorkCapture();
        if(command.Review.QualifiedWorkAssets is not null && readQualifiedWorkAssets is null)throw new InvalidOperationException("The original qualified work-scope owner is required before a new start.");
        WORK_ORDER_EXECUTION_START? recorded=null;var proofCalls=0;
        await Guard(false);command.RequireCurrentStage(await readStage());await Guard(false);
        async Task Extra(CompanyWorkOrderExecutionPrerequisites current)
        {
            await Guard(true);if(current.SourceRevision()!=command.Review.Prerequisites.SourceRevision())throw new InvalidOperationException("Original executor prerequisite source or provider boundary changed before start.");
            await executorIdentities.RequireExecutorIdentityCoreAsync(command.Review,()=>Guard(true));
            if(command.Review.QualifiedWorkAssets is {} originalAssets)
            {
                var currentAssets=await readQualifiedWorkAssets!(true);
                CompanyMaintenanceQualifiedWorkScope.RequireSnapshot(currentAssets,current.Requirements.Order,context,DateTime.UtcNow);
                CompanyMaintenanceQualifiedWorkScope.RequireAtStart(currentAssets,command.StartedAtUtc);
                if(currentAssets.IdentityRevision()!=originalAssets.IdentityRevision())throw new InvalidOperationException("Original qualified native work identity/installation/rights changed around start.");
            }
            var row=await ReadStartAsync(context,permit,()=>Guard(true),true);
            if(++proofCalls==1){if(row is not null){command.RequireRecorded(row);recorded=row;}}
            else{if(row is null || recorded is null)throw new InvalidOperationException("Original work start is unconfirmed after owned proof.");command.RequireRecorded(row);if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(recorded))throw new InvalidOperationException("Original work-start body/audit changed around owned proof.");}
            await Guard(true);
        }
        async Task Insert()
        {
            await Guard(true);if(recorded is not null)return;
            var row=command.Prepare(DateTime.UtcNow);var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_START),context.Database.ConnectionName,StartTable,rowAccess:new ExecutionStartInsertPolicy(row,command,()=>Guard(true)));
            await repo.InsertAsync(row,context.ActorUserId);recorded=await ReadStartAsync(context,permit,()=>Guard(true),true)??throw new InvalidOperationException("Original work-start insert is unconfirmed.");command.RequireRecorded(recorded);
            if(recorded.ROW_CREATED_DATE!=row.ROW_CREATED_DATE)throw new InvalidOperationException("Original work-start audit time changed during readback.");
        }
        await WithExecutionPrerequisitesAsync(context,permit,authority,readCurrent,personnel,documents,identities,currentStock,ownedStock,Extra,Insert);
        await Guard(false);command.RequireCurrentStage(await readStage());await Guard(false);
        var committed=await RecoverExecutionStartAsync(context,new(permit,command.IntentId),()=>Guard(false));
        if(recorded is null || JsonSerializer.Serialize(committed.Recorded)!=JsonSerializer.Serialize(recorded))throw new InvalidOperationException("Original work-start acknowledgement is unconfirmed. Recover its original intent without replay.");return Receipt(committed.Recorded);
        CompanyWorkOrderExecutionStartReceipt Receipt(WORK_ORDER_EXECUTION_START row){command.RequireRecorded(row);var result=new CompanyWorkOrderExecutionStartReceipt(context.Company,context.ActorUserId,command.IntentId,row.START_ID,row.PERMIT_ID,row.STARTED_AT_UTC,row.RECORDED_REVISION);result.Require(command);return result;}
    }
    private sealed class ExecutionStartInsertPolicy(WORK_ORDER_EXECUTION_START original,CompanyWorkOrderExecutionStartCommand command,Func<Task> authority):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_EXECUTION_START expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await authority();if(type!=typeof(WORK_ORDER_EXECUTION_START) || table!=StartTable || !ReferenceEquals(entity,original) || previous is not null || operation!=PPDMRowMutation.Insert || actor!=expected.EXECUTOR_USER_ID)throw new InvalidOperationException("Only the exact original owned work-start insert is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.SOURCE=expected.SOURCE;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Work start grants no general row access.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Work start grants no general row access.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Work start grants no general row access.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Work start grants no general row access.");
    }
}
