using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService:ICompanyMaintenanceExecutionHandoffOwner
{
    private const string ExecutionHandoffKey="CompanyMaintenanceExecutionHandoff";
    private sealed record ExecutionHandoffEvidence(CompanyMaintenanceExecutionHandoffCommand Command,string ProcessDigest,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Next);
    private static string ExecutionHandoffBusinessDigest(ProcessInstance instance)
    {var copy=CompanyInventorySourceReview.Copy(instance);copy.ProcessData.Data.Remove(ExecutionHandoffKey);RemoveCompletionDecisionFromPriorDigest(copy);return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    private async Task<(ProcessInstance Instance,CompanyMaintenanceProcessStart Start,ProcessDefinition Definition)> ReadExecutionHandoffOwner(FieldSelectionReview context,CompanyMaintenanceReceivingQuery task)
    {
        task.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(context.Database.ConnectionName!=_connectionName || context.FieldId!=task.FieldId || _personaReader is null)throw new UnauthorizedAccessException("Original company/FIELD/lifecycle/persona owner is required.");
        var rows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",task.InstanceId))).ToArray();
        if(rows.Length!=1 || rows[0] is not PROCESS_INSTANCE row || row.ACTIVE_IND!="Y" || row.FIELD_ID!=task.FieldId || row.ENTITY_TYPE!="WORK_ORDER" || row.ENTITY_ID!=task.WorkOrderId)throw new InvalidOperationException("Original work-order process is unavailable.");
        var start=CompanyMaintenanceProcessStart.FromPayload(row.INSTANCE_DATA_JSON!,task.InstanceId,task.FieldId,task.WorkOrderId);
        if(start.Context.Company!=context.Company || start.Context.Database!=context.Database)throw new UnauthorizedAccessException("Original lifecycle ownership/native binding differs.");
        return await ReadReceivingOwner(context,new(start.IntentId,start.Source.DecisionId,task.FieldId,start.Source.WellId,task.WorkOrderId));
    }
    private async Task<CompanyMaintenanceExecutionHandoffReview> BuildExecutionHandoffReview(CompanyMaintenanceAssignedStageReview stage,CompanyWorkOrderSubmissionFacts report,Func<string?,Task> authority)
    {
        stage.Require(stage.Context.Company,stage.Context.ActorUserId,report.Query.Task);report.Require();await authority(stage.AssignedRole);
        var owner=await ReadExecutionHandoffOwner(stage.Context,report.Query.Task);var instance=owner.Instance;
        if(instance.ProcessData.Data.ContainsKey(ExecutionHandoffKey))throw new InvalidOperationException("Original execution handoff already exists; recover its original reference.");
        var graph=await MaintenanceAcceptanceNativeGraph(stage.Task.InstanceId,owner.Start.ProcessId);
        if(ProcessDefinitionRevision.FromJson(graph)!=stage.ProcessRevision || owner.Start.DefinitionRevision!=stage.DefinitionRevision)throw new InvalidOperationException("Original configured executor graph changed.");
        var templates=owner.Definition.Steps.OrderBy(row=>row.SequenceNumber).ToArray();var index=Array.FindIndex(templates,row=>row.StepId==stage.StepId);
        if(index!=1 || index+1>=templates.Length || templates[index+1].StepType!="APPROVAL" || !templates[index+1].RequiresApproval || templates[index+1].RequiredRoles.Count!=1 || templates[index+1].RequiredRoles[0]==stage.AssignedRole)
            throw new InvalidOperationException("Configure an explicit immediately following independent review approval role before submitting the report handoff.");
        var current=instance.StepInstances.Single(row=>row.StepInstanceId==stage.StepInstanceId);var next=instance.StepInstances.Single(row=>row.StepId==templates[index+1].StepId);
        if(instance.Status!=ProcessStatus.IN_PROGRESS || instance.CurrentStepId!=stage.StepId || current.RequiredRole!=stage.AssignedRole || current.Status is not (StepStatus.PENDING or StepStatus.IN_PROGRESS) ||
            !string.IsNullOrEmpty(current.AssignedTo) && current.AssignedTo!=stage.Context.ActorUserId && current.AssignedTo!=stage.AssignedRole || next.Status!=StepStatus.BLOCKED || next.StartDate is not null || next.CompletionDate is not null || !string.IsNullOrEmpty(next.CompletedBy))
            throw new InvalidOperationException("Original executor or future configured reviewer step changed.");
        var result=new CompanyMaintenanceExecutionHandoffReview(stage,report,next.StepId,next.StepInstanceId,next.RequiredRole!);result.Require();await authority(stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceExecutionHandoffReview> ReviewExecutionHandoffAsync(CompanyMaintenanceAssignedStageReview stage,CompanyWorkOrderSubmissionFacts report,Func<string?,Task> authority)
    {
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original lifecycle source is unavailable.");
        PPDMDataSourceTransaction.RequireIdle(source);var first=await BuildExecutionHandoffReview(stage,report,authority);var closing=await BuildExecutionHandoffReview(stage,report,authority);
        if(first!=closing || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original report handoff changed during review.");PPDMDataSourceTransaction.RequireIdle(source);return first;
    }
    private async Task<ExecutionHandoffEvidence> ConfirmExecutionHandoff(FieldSelectionReview context,CompanyMaintenanceExecutionHandoffReference reference,Func<string?,Task> authority,bool privateRead)
    {
        var owner=await ReadExecutionHandoffOwner(context,reference.Query.Task);var nativeGraph=await MaintenanceAcceptanceNativeGraph(reference.Query.Task.InstanceId,owner.Start.ProcessId);var graph=MaintenanceBeforeReturnGraph(nativeGraph,context,reference.Query);var snapshot=JsonNode.Parse(graph)!.AsObject();var instance=graph==nativeGraph?owner.Instance:MaintenanceGraphInstance(graph);
        var closed=instance.StepInstances.SingleOrDefault(row=>row.Outcome=="WORK_REPORTED")??throw new InvalidOperationException("Original recorded executor handoff is unavailable.");
        if(privateRead && closed.CompletedBy!=context.ActorUserId)throw new UnauthorizedAccessException("Original handoff belongs to another executor account.");
        if(!instance.ProcessData.Data.TryGetValue(ExecutionHandoffKey,out var value))throw new InvalidOperationException("Original execution handoff is unconfirmed; recovery cannot create it.");
        ExecutionHandoffEvidence proof;try{proof=JsonSerializer.Deserialize<ExecutionHandoffEvidence>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("Original handoff proof is unavailable.");}catch(JsonException error){throw new InvalidOperationException("Original handoff proof is malformed.",error);}
        proof.Command.Require();var original=proof.Command.Review;var stage=original.Stage;
        if(proof.Command.Reference()!=reference || stage.Context.Company!=context.Company || stage.Context.Database!=context.Database || stage.Task!=reference.Query.Task || stage.DefinitionRevision!=owner.Start.DefinitionRevision ||
            privateRead && stage.Context.ActorUserId!=context.ActorUserId || closed.StepInstanceId!=stage.StepInstanceId || closed.CompletedBy!=stage.Context.ActorUserId || closed.Notes!=proof.Command.Reason || closed.CompletionDate is not {} utc || utc.Kind!=DateTimeKind.Utc || utc<original.Report.RecordedAtUtc ||
            closed.Status!=StepStatus.COMPLETED || instance.Status!=ProcessStatus.IN_PROGRESS || instance.CurrentStepId!=original.NextStepId || ExecutionHandoffBusinessDigest(instance)!=proof.ProcessDigest)
            throw new InvalidOperationException("Original report/executor/configured handoff/native graph proof differs.");
        if(instance.ProcessData.Data.ContainsKey(CompletionDecisionKey))await ReadCompletionDecisionEvidence(context,reference.Query);
        var next=instance.StepInstances.Single(row=>row.StepInstanceId==original.NextStepInstanceId);
        if(next.StepId!=original.NextStepId || next.RequiredRole!=original.NextRole || next.Status!=StepStatus.PENDING || next.StartDate!=utc || next.CompletionDate is not null)throw new InvalidOperationException("Original configured review assignment changed.");
        await authority(privateRead?stage.AssignedRole:original.NextRole);
        var identity=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=stage.Task.InstanceId,proof.Command.IntentId}));
        var expected=ConvertToProcessHistoryEntity(new ProcessHistoryEntry{HistoryId="work-report-handoff-"+identity,InstanceId=stage.Task.InstanceId,StepInstanceId=stage.StepInstanceId,Action="WORK_REPORTED",PreviousState=stage.StepId,NewState=original.NextStepId,Timestamp=utc,PerformedBy=stage.Context.ActorUserId,Notes=proof.Command.Reason});
        expected.ROW_CREATED_BY=stage.Context.ActorUserId;expected.ROW_CREATED_DATE=utc;
        if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(proof.History))throw new InvalidOperationException("Original report handoff history audit differs.");
        var history=MaintenanceGraphRows<PROCESS_HISTORY>(snapshot,"History").Where(row=>row.PROCESS_HISTORY_ID==expected.PROCESS_HISTORY_ID).ToArray();
        if(history.Length!=1 || JsonSerializer.Serialize(history[0])!=JsonSerializer.Serialize(proof.History) || proof.Closed is not {Count:>0 and <=128} || proof.Next is not {Count:>0 and <=128} ||
            proof.Closed.Any(row=>row.PROCESS_INSTANCE_ID!=stage.Task.InstanceId || row.PROCESS_STEP_INSTANCE_ID!=stage.StepInstanceId || row.ASSIGNED_ROLE!=stage.AssignedRole || row.TASK_STATUS!="COMPLETED" || row.ROW_CHANGED_BY!=stage.Context.ActorUserId) ||
            proof.Next.Any(row=>row.PROCESS_INSTANCE_ID!=stage.Task.InstanceId || row.PROCESS_STEP_INSTANCE_ID!=next.StepInstanceId || row.ASSIGNED_ROLE!=original.NextRole || row.TASK_STATUS!="PENDING" || row.ROW_CREATED_BY!=stage.Context.ActorUserId))throw new InvalidOperationException("Original report history and complete executor/reviewer task correlations differ.");
        RequireGraphTasks(snapshot,proof.Closed);RequireGraphTasks(snapshot,proof.Next);
        if(!proof.Next.Select(row=>row.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual((await _personaReader!.GetActivePersonasAsync(original.NextRole)).Order(StringComparer.Ordinal)))throw new InvalidOperationException("Original reviewer persona population changed.");return proof;
    }
    private async Task<CompanyMaintenanceExecutionHandoffRecovery> HandoffReceipt(FieldSelectionReview context,CompanyMaintenanceExecutionHandoffReference reference,Func<string?,Task> authority)
    {
        var first=await ConfirmExecutionHandoff(context,reference,authority,true);var closing=await ConfirmExecutionHandoff(context,reference,authority,true);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original handoff history changed during recovery.");
        var stage=first.Command.Review.Stage;var graph=MaintenanceBeforeReturnGraph(await MaintenanceAcceptanceNativeGraph(stage.Task.InstanceId,(await ReadExecutionHandoffOwner(context,reference.Query.Task)).Start.ProcessId),context,reference.Query);
        var decision=await ReadCompletionDecisionEvidence(context,reference.Query);if(decision?.Delta is {} delta)graph=RestoreMaintenanceDelta(graph,delta);
        var result=new CompanyMaintenanceExecutionHandoffRecovery(first.Command,new(context.Company,context.ActorUserId,reference.IntentId,stage.Task.InstanceId,reference.Query.SubmissionId,stage.StepInstanceId,first.Command.Review.NextStepInstanceId,first.Command.Review.NextRole,first.History.PROCESS_HISTORY_ID,
            first.Closed.Select(row=>row.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),first.Next.Select(row=>row.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),ProcessDefinitionRevision.FromJson(graph)));result.Require(context.Company,context.ActorUserId,reference);return result;
    }
    public async Task<CompanyMaintenanceExecutionHandoffRecovery> RecoverExecutionHandoffAsync(FieldSelectionReview context,CompanyMaintenanceExecutionHandoffReference reference,Func<string?,Task> authority)
    {
        reference.Require();var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original lifecycle source is unavailable.");PPDMDataSourceTransaction.RequireIdle(source);await authority(null);
        var result=await HandoffReceipt(context,reference,authority);await authority(result.Command.Review.Stage.AssignedRole);PPDMDataSourceTransaction.RequireIdle(source);return result;
    }
    public async Task<CompanyMaintenanceExecutionHandoffRecovery> RecordExecutionHandoffAsync(CompanyMaintenanceExecutionHandoffCommand original,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderSubmissionFacts?>> readReport)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var review=command.Review;var context=review.Stage.Context;
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original report handoff lifecycle source is unavailable.");
        async Task Guard(bool owned)
        {if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority(review.Stage.AssignedRole);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original lifecycle source changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        await Guard(false);var owner=await ReadExecutionHandoffOwner(context,review.Report.Query.Task);
        if(owner.Instance.ProcessData.Data.ContainsKey(ExecutionHandoffKey)){var recovered=await RecoverExecutionHandoffAsync(context,command.Reference(),authority);if(JsonSerializer.Serialize(recovered.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Original handoff intent/body differs.");return recovered;}
        async Task Report(bool owned){var current=await readReport(owned);if(!owned && current is null || current is not null && JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(review.Report))throw new InvalidOperationException("Original recorded work report changed before handoff.");}
        await Report(false);await Guard(false);var current=await BuildExecutionHandoffReview(review.Stage,review.Report,authority);if(current!=review)throw new InvalidOperationException("Original configured handoff review changed.");
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);await Report(true);var closing=await BuildExecutionHandoffReview(review.Stage,review.Report,authority);if(closing!=review)throw new InvalidOperationException("Original handoff graph changed inside owned command.");
            var loaded=await ReadExecutionHandoffOwner(context,review.Report.Query.Task);var instance=loaded.Instance;var executor=instance.StepInstances.Single(row=>row.StepInstanceId==review.Stage.StepInstanceId);var next=instance.StepInstances.Single(row=>row.StepInstanceId==review.NextStepInstanceId);
            var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:_personaReader);
            var closed=await router.CompleteTaskSetInOwnedTransactionAsync(instance.InstanceId,executor.StepInstanceId,review.Stage.AssignedRole,"WORK_ORDER",review.Stage.Task.WorkOrderId,context.ActorUserId,_connectionName);
            var utc=DateTime.UtcNow;if(utc<review.Report.RecordedAtUtc)throw new InvalidOperationException("Handoff audit must follow recorded actual work.");executor.Status=StepStatus.COMPLETED;executor.StartDate??=utc;executor.CompletionDate=utc;executor.CompletedBy=context.ActorUserId;executor.Outcome="WORK_REPORTED";executor.Notes=command.Reason;
            next.Status=StepStatus.PENDING;next.StartDate=utc;instance.CurrentStepId=next.StepId;instance.CurrentState=next.StepId;instance.ProcessData.LastUpdated=utc;
            var identity=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=instance.InstanceId,command.IntentId}));var history=new ProcessHistoryEntry{HistoryId="work-report-handoff-"+identity,InstanceId=instance.InstanceId,StepInstanceId=executor.StepInstanceId,Action="WORK_REPORTED",PreviousState=executor.StepId,NewState=next.StepId,Timestamp=utc,PerformedBy=context.ActorUserId,Notes=command.Reason};
            var historyRow=ConvertToProcessHistoryEntity(history);historyRow.ROW_CREATED_BY=context.ActorUserId;historyRow.ROW_CREATED_DATE=utc;await GetProcessHistoryRepository().InsertAsync(historyRow,context.ActorUserId);instance.History.Add(history);
            async Task Update<T>(T row,T prepared,PPDMGenericRepository repository,string key,string id) where T:ModelEntityBase
            {
                var creator=row.ROW_CREATED_BY;var created=row.ROW_CREATED_DATE;foreach(var property in typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))if(property.CanWrite)property.SetValue(row,property.GetValue(prepared));row.ROW_CHANGED_BY=context.ActorUserId;row.ROW_CHANGED_DATE=utc;
                await repository.UpdateAsync(row,context.ActorUserId);var saved=(await repository.GetAsync(Key(key,id))).ToArray();if(row.ROW_CREATED_BY!=creator || row.ROW_CREATED_DATE!=created || saved.Length!=1 || JsonSerializer.Serialize(saved[0])!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Exact report handoff business/audit readback is unconfirmed.");
            }
            foreach(var step in new[]{executor,next}){var row=(PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",step.StepInstanceId))).Single();await Update(row,ConvertToProcessStepInstanceEntity(step),GetProcessStepInstanceRepository(),"PROCESS_STEP_INSTANCE_ID",step.StepInstanceId);}
            var processRow=(PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",instance.InstanceId))).Single();await Update(processRow,ConvertToProcessInstanceEntity(instance),GetProcessInstanceRepository(),"PROCESS_INSTANCE_ID",instance.InstanceId);
            var routed=await router.RouteTaskInOwnedTransactionAsync(instance.InstanceId,next.StepInstanceId,review.NextRole,"WORK_ORDER",review.Stage.Task.WorkOrderId,null,context.ActorUserId);
            instance.ProcessData.Data[ExecutionHandoffKey]=new ExecutionHandoffEvidence(command,ExecutionHandoffBusinessDigest(instance),historyRow,closed,routed);
            await Update(processRow,ConvertToProcessInstanceEntity(instance),GetProcessInstanceRepository(),"PROCESS_INSTANCE_ID",instance.InstanceId);await Guard(true);await ConfirmExecutionHandoff(context,command.Reference(),authority,true);await Report(true);
        });
        await Guard(false);await Report(false);return await RecoverExecutionHandoffAsync(context,command.Reference(),authority);
    }
    private async Task<CompanyMaintenanceAssignedStageReview> ReviewExecutionReviewerStageAsync(FieldSelectionReview current,CompanyMaintenanceReceivingQuery task,Func<string?,Task> authority)
    {
        var owner=await ReadExecutionHandoffOwner(current,task);var step=owner.Instance.StepInstances.Single(row=>row.StepId==owner.Instance.CurrentStepId);
        if(!LeaseSummaryValidator.Identifier(step.RequiredRole) || !string.IsNullOrEmpty(step.AssignedTo) && step.AssignedTo!=current.ActorUserId && step.AssignedTo!=step.RequiredRole)throw new UnauthorizedAccessException("Original configured reviewer assignment is required.");await authority(step.RequiredRole);
        var proof=JsonSerializer.Deserialize<ExecutionHandoffEvidence>(JsonSerializer.Serialize(owner.Instance.ProcessData.Data[ExecutionHandoffKey]))??throw new InvalidOperationException("Original report handoff is unavailable.");
        if(proof.Command.Review.Report.ExecutorUserId==current.ActorUserId)throw new UnauthorizedAccessException("Original executor cannot take the independent review assignment.");
        await ConfirmExecutionHandoff(current,proof.Command.Reference(),authority,false);var first=await MaintenanceAcceptanceNativeGraph(task.InstanceId,owner.Start.ProcessId);await authority(step.RequiredRole);
        if(first!=await MaintenanceAcceptanceNativeGraph(task.InstanceId,owner.Start.ProcessId))throw new InvalidOperationException("Original configured review graph changed.");
        var initial=JsonSerializer.Deserialize<MaintenanceAcceptanceEvidence>(JsonSerializer.Serialize(owner.Instance.ProcessData.Data[MaintenanceAcceptanceKey]))??throw new InvalidOperationException("Original receiving audit is unavailable.");
        var result=new CompanyMaintenanceAssignedStageReview(current,task,step.StepInstanceId,step.StepId,step.StepName,step.RequiredRole!,initial.Command.Review.Context.ActorUserId,initial.History.PROCESS_HISTORY_ID,initial.Approval.PROCESS_APPROVAL_ID,owner.Start.DefinitionRevision,ProcessDefinitionRevision.FromJson(first));result.Require(current.Company,current.ActorUserId,task);return result;
    }
}
