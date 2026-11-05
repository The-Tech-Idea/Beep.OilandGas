using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    public async Task<string?> FindDevelopmentReviewAsync(CompanyScope company,string fieldId,string planId,Func<Task> requireCurrentRead)
    {
        if(company is null || !LeaseSummaryValidator.Identifier(company.TenantId) || !LeaseSummaryValidator.Identifier(company.BusinessAssociateId))throw new ArgumentException("A selected company is required.");
        if(!LeaseSummaryValidator.Identifier(fieldId) || !LeaseSummaryValidator.Identifier(planId))throw new ArgumentException("A recorded development source is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original lifecycle datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original lifecycle target changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await requireCurrentRead();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<string?> Find()
        {
            var rows=await GetProcessInstanceRepository().GetAsync([new(){FieldName="ENTITY_TYPE",FilterValue="FIELD_DEVELOPMENT_PLAN"},new(){FieldName="ENTITY_ID",FilterValue=planId},new(){FieldName="FIELD_ID",FilterValue=fieldId}]);
            var ids=new List<string>();
            foreach(var value in rows)
            {
                if(value is not PROCESS_INSTANCE row || row.ENTITY_TYPE!="FIELD_DEVELOPMENT_PLAN" || row.ENTITY_ID!=planId || row.FIELD_ID!=fieldId)
                    throw new InvalidOperationException("The recorded draft review lookup returned ambiguous source ownership.");
                var original=DevelopmentReviewStart.FromPayload(row.INSTANCE_DATA_JSON!,row.PROCESS_INSTANCE_ID!,fieldId,planId);
                if(original.Source.Context.Company!=company)continue;
                if(row.ACTIVE_IND!="Y" || original.LifecycleDatabase.ConnectionName!=_connectionName)throw new InvalidOperationException("The original draft review is inactive or belongs to another target.");
                ids.Add(row.PROCESS_INSTANCE_ID!);
            }
            if(ids.Count>1)throw new InvalidOperationException("The recorded draft has multiple review processes and needs reconciliation.");
            return ids.SingleOrDefault();
        }
        await Guard(false);string? id=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{await Guard(true);id=await Find();await Guard(true);if(id!=await Find())throw new InvalidOperationException("The draft review lookup changed during its read.");await Guard(true);});
        await Guard(false);if(id!=await Find())throw new InvalidOperationException("The recorded draft review or its absence changed after the read.");await Guard(false);return id;
    }
    public async Task<(ProcessInstance Process,IReadOnlyList<DevelopmentReviewStageProgress> Stages,IReadOnlyList<DevelopmentReviewHistory> History)> ReadDevelopmentReviewOutcomeAsync(string id,Func<Task> requireCurrentRead)
    {
        var process=await ReadDevelopmentReviewProgressAsync(id,requireCurrentRead);var proofs=DevelopmentProofs(process);
        static DateTimeOffset Utc(DateTime value){if(value.Kind==DateTimeKind.Local)throw new InvalidOperationException("A native review audit time must be UTC.");return new(DateTime.SpecifyKind(value,DateTimeKind.Utc));}
        var stages=process.StepInstances.OrderBy(step=>step.SequenceNumber).Select(step=>{
            var proof=proofs.SingleOrDefault(item=>item.Command.Review.StepInstanceId==step.StepInstanceId);
            return new DevelopmentReviewStageProgress(step.StepId,step.StepInstanceId,step.SequenceNumber,step.StepName,step.RequiredRole!,step.Status.ToString(),
                proof?.Command.Decision,proof?.Command.Review.Scope.ActorUserId,proof?.Command.Notes,proof?.Approval.PROCESS_APPROVAL_ID,proof?.History.PROCESS_HISTORY_ID,proof is null?null:Utc(proof.At));
        }).ToArray();
        var history=process.History.OrderBy(row=>row.Timestamp).ThenBy(row=>row.HistoryId,StringComparer.Ordinal).Select(row=>new DevelopmentReviewHistory(row.HistoryId,string.IsNullOrEmpty(row.StepInstanceId)?null:row.StepInstanceId,row.Action,row.PerformedBy,row.Notes,Utc(row.Timestamp))).ToArray();
        await requireCurrentRead();return(process,stages,history);
    }
    public async Task<ProcessInstance> ReadDevelopmentReviewProgressAsync(string id,Func<Task> requireCurrentRead)
    {
        if(!LeaseSummaryValidator.Identifier(id))throw new ArgumentException("A recorded development review identity is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original development review datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original development review source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await requireCurrentRead();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await Guard(false);ProcessInstance? result=null;DevelopmentReviewStart? original=null;string? graph=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);result=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded development review is unavailable.");
            original=DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(result.ProcessData),id,result.FieldId,result.EntityId);
            if(original.LifecycleDatabase.ConnectionName!=_connectionName)throw new InvalidOperationException("The original review lifecycle target differs from its current reader.");
            var definition=await GetProcessDefinitionAsync(original.Workflow.ProcessId)??throw new InvalidOperationException("The original review definition is unavailable.");
            graph=await ConfirmDevelopmentGraphAsync(result,original,DevelopmentReviewDefinition.Require(definition));await Guard(true);
            if(graph!=await ConfirmDevelopmentGraphAsync(result,original,DevelopmentReviewDefinition.Require(definition)))throw new InvalidOperationException("The original development review changed during its read.");
        });
        await Guard(false);var closingDefinition=await GetProcessDefinitionAsync(original!.Workflow.ProcessId)??throw new InvalidOperationException("The original review definition is unavailable.");
        if(result is null || graph!=await ConfirmDevelopmentGraphAsync(result,original,DevelopmentReviewDefinition.Require(closingDefinition)))throw new InvalidOperationException("The complete development review graph changed after its read.");
        await Guard(false);return JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(result))!;
    }
    /// <summary>A confirmed pending assignment after zero or more canonical review decisions; the API admits the individual.</summary>
    public async Task<(ProcessInstance Process,IReadOnlyList<CROSS_PERSONA_TASK> Tasks)> ReadDevelopmentReviewAssignmentAsync(string id,Func<Task> requireCurrentRead)
    {
        if(!LeaseSummaryValidator.Identifier(id))throw new ArgumentException("A recorded development review identity is required.");
        ArgumentNullException.ThrowIfNull(requireCurrentRead);
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The development review datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The development review datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await requireCurrentRead();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await Guard(false);ProcessInstance? instance=null;List<CROSS_PERSONA_TASK> tasks=[];string? graph=null;CrossPersonaTaskRouter? router=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);
            instance=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded development review is unavailable.");
            if(instance.EntityType!="FIELD_DEVELOPMENT_PLAN" || instance.InstanceId!=id || string.IsNullOrWhiteSpace(instance.FieldId))
                throw new InvalidOperationException("The recorded process is not a source-bound development review.");
            var original=DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData),id,instance.FieldId,instance.EntityId);
            if(original.LifecycleDatabase.ConnectionName!=_connectionName)throw new InvalidOperationException("The original development lifecycle binding differs from this reader.");
            var definition=await GetProcessDefinitionAsync(original.Workflow.ProcessId)??throw new InvalidOperationException("The recorded review definition is unavailable.");
            var steps=DevelopmentReviewDefinition.Require(definition);
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=original.Workflow.DefinitionRevision)
                throw new InvalidOperationException("The original development review definition changed.");
            graph=await ConfirmDevelopmentGraphAsync(instance,original,steps);
            if(instance.Status!=ProcessStatus.IN_PROGRESS)throw new InvalidOperationException("This development review has no pending assignment. Recover the original decision outcome or inspect recorded review history.");
            var active=instance.StepInstances.Single(step=>step.StepId==instance.CurrentStepId);
            router=new(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new DevelopmentStartPersonas(original,()=>Guard(true)));
            var creator=DevelopmentProofs(instance).LastOrDefault()?.Command.Review.Scope.ActorUserId??original.Source.Context.ActorUserId;
            tasks=await router.ReadTaskSetInOwnedTransactionAsync(id,active.StepInstanceId,active.RequiredRole!,instance.EntityType,instance.EntityId,creator);
            await router.ConfirmRecordedTaskSetAsync(tasks,_connectionName);await Guard(true);
            var closing=await GetProcessDefinitionAsync(original.Workflow.ProcessId);
            if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(definition) || await ConfirmDevelopmentGraphAsync(instance,original,steps)!=graph)
                throw new InvalidOperationException("The original development definition or assignment graph changed during its read.");
        });
        await Guard(false);
        if(instance is null || router is null)throw new InvalidOperationException("The recorded development assignment could not be confirmed.");
        var closingOwner=DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData),id,instance.FieldId,instance.EntityId);
        var closingDefinition=await GetProcessDefinitionAsync(closingOwner.Workflow.ProcessId)??throw new InvalidOperationException("The original development definition is unavailable.");
        if(await ConfirmDevelopmentGraphAsync(instance,closingOwner,DevelopmentReviewDefinition.Require(closingDefinition))!=graph)throw new InvalidOperationException("The recorded development assignment changed after its read.");
        await router.ConfirmRecordedTaskSetAsync(tasks,_connectionName);await Guard(false);
        return(JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(instance))!,tasks);
    }
}
