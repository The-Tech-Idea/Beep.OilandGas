using System.Text.Json;
using System.Reflection;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    private sealed record AppliedReviewStage(ProcessInstance Process,PROCESS_INSTANCE Row,DateTime At,PROCESS_APPROVAL Approval,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Routed,string? NextStep);
    private static void CopyBusiness<T>(T target,T converted) where T:ModelEntityBase
    {foreach(var property in typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))if(property.CanWrite)property.SetValue(target,property.GetValue(converted));}
    // Domain owners must admit the exact source/intent and hold the canonical Serializable command before entering.
    private async Task<AppliedReviewStage> ApplyOwnedReviewStageAsync(ProcessInstance instance,ProcessStepInstance current,IReadOnlyList<ProcessStepDefinition> templates,
        CrossPersonaTaskRouter router,string actor,string decision,string notes,Guid intentId,string subject,string approvalType,Func<Task> guard,string? entityDescription=null,Dictionary<string,object>? actionData=null)
    {
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original review datasource is unavailable.");
        PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);await guard();
            var closed=await router.CompleteTaskSetInOwnedTransactionAsync(instance.InstanceId,current.StepInstanceId,current.RequiredRole!,instance.EntityType,instance.EntityId,actor,_connectionName);
            var now=DateTime.UtcNow;var rejected=decision=="REJECT";var index=Array.FindIndex(templates.ToArray(),template=>template.StepId==current.StepId);
            current.Status=rejected?StepStatus.FAILED:StepStatus.COMPLETED;current.StartDate??=now;current.CompletionDate=now;current.CompletedBy=actor;current.Outcome=rejected?"REJECTED":"APPROVED";current.Notes=notes;
            ProcessStepInstance? next=null;
            if(!rejected && index+1<templates.Count){next=instance.StepInstances.Single(step=>step.StepId==templates[index+1].StepId);next.Status=StepStatus.PENDING;next.StartDate=now;instance.CurrentStepId=next.StepId;instance.CurrentState=next.StepId;}
            else{instance.Status=rejected?ProcessStatus.FAILED:ProcessStatus.COMPLETED;instance.CurrentState=rejected?"REVIEW_REJECTED":"REVIEW_APPROVED";instance.CompletionDate=now;instance.CompletedBy=actor;}
            instance.ProcessData.LastUpdated=now;
            var approval=new PROCESS_APPROVAL{PROCESS_APPROVAL_ID=subject+"-approval-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=instance.InstanceId,IntentId=intentId})),PROCESS_INSTANCE_ID=instance.InstanceId,PROCESS_STEP_INSTANCE_ID=current.StepInstanceId,
                APPROVAL_TYPE=approvalType,APPROVAL_STATUS=rejected?"REJECTED":"APPROVED",REQUIRED_ACTION=decision,APPROVER_USER_ID=actor,APPROVED_BY=actor,APPROVAL_DATE=now,APPROVAL_NOTES=notes,
                APPROVER_SEQUENCE=current.SequenceNumber,REQUESTED_BY=instance.StartedBy,REQUESTED_DATE=instance.StartDate,ROW_CREATED_BY=actor,ROW_CREATED_DATE=now};
            var history=ConvertToProcessHistoryEntity(new ProcessHistoryEntry{HistoryId=subject+"-decision-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=instance.InstanceId,IntentId=intentId})),InstanceId=instance.InstanceId,StepInstanceId=current.StepInstanceId,
                Action=rejected?approvalType+"_REJECTED":approvalType+"_APPROVED",PreviousState=current.StepId,NewState=instance.CurrentState,Timestamp=now,PerformedBy=actor,Notes=notes,ActionData=actionData??new()});history.ROW_CREATED_BY=actor;history.ROW_CREATED_DATE=now;
            await GetProcessApprovalRepository().InsertAsync(approval,actor);await guard();await GetProcessHistoryRepository().InsertAsync(history,actor);
            var processRow=(PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",instance.InstanceId))).Single();
            CopyBusiness(processRow,ConvertToProcessInstanceEntity(instance));processRow.ROW_CHANGED_BY=actor;processRow.ROW_CHANGED_DATE=now;await GetProcessInstanceRepository().UpdateAsync(processRow,actor);
            foreach(var changed in new[]{current,next}.OfType<ProcessStepInstance>())
            {
                var row=(PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",changed.StepInstanceId))).Single();
                CopyBusiness(row,ConvertToProcessStepInstanceEntity(changed));row.ROW_CHANGED_BY=actor;row.ROW_CHANGED_DATE=now;await GetProcessStepInstanceRepository().UpdateAsync(row,actor);
            }
            var routed=next is null?new List<CROSS_PERSONA_TASK>():await router.RouteTaskInOwnedTransactionAsync(instance.InstanceId,next.StepInstanceId,next.RequiredRole!,instance.EntityType,instance.EntityId,entityDescription,actor);
            instance=await LoadProcessInstanceAsync(instance.InstanceId)??throw new InvalidOperationException("The updated source-bound review could not be read back.");
            await guard();PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);
            return new(instance,processRow,now,approval,history,closed,routed,next?.StepInstanceId);
    }
}
