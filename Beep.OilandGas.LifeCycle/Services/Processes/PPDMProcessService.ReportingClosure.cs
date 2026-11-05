using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    // Only project a graph admitted by ReadRecordedExecutionAssignmentAsync. Public API authority remains with its caller.
    public static DrillingReportingClosure? RecordedReportingClosure(ProcessInstance process)
    {
        var proof=ReportingClosure(process);if(proof is null)return null;
        proof.Command.Require();
        if(proof.Command.Review.Responsibility.Root.InstanceId()!=process.InstanceId || process.Status!=ProcessStatus.COMPLETED || process.CurrentState!="REPORTING_CLOSED")
            throw new InvalidOperationException("The original reporting closure does not belong to this qualified responsibility.");
        return CompanyInventorySourceReview.Copy(proof.Command);
    }
    private sealed record ReportingInitialSnapshot(ProcessInstance Model,PROCESS_INSTANCE Row,List<PROCESS_STEP_INSTANCE> Steps,PROCESS_HISTORY Start,List<CROSS_PERSONA_TASK> Tasks);
    private sealed record ReportingClosureProof(DrillingReportingClosure Command,DateTime At,ReportingInitialSnapshot Before);
    private static string ReportingDigest(object value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value));
    private static ReportingClosureProof? ReportingClosure(ProcessInstance process)=>process.ProcessData.Data.TryGetValue(DrillingReportingClosure.PayloadKey,out var value)?
        JsonSerializer.Deserialize<ReportingClosureProof>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("The original reporting closure proof is unavailable."):null;
    private async Task<ReportingInitialSnapshot> ReportingSnapshotAsync(ProcessInstance process,CrossPersonaTaskRouter router)
    {
        var id=process.InstanceId;var rows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var steps=(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();var histories=(await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        if(rows.Length!=1 || rows[0] is not PROCESS_INSTANCE row || steps.Length is <1 or >256 || steps.Any(item=>item is not PROCESS_STEP_INSTANCE) || histories.Any(item=>item is not PROCESS_HISTORY) ||
            histories.OfType<PROCESS_HISTORY>().Count(item=>item.EVENT_TYPE=="PROCESS_STARTED")!=1 || histories.OfType<PROCESS_HISTORY>().Any(item=>item.EVENT_TYPE is not ("PROCESS_STARTED" or "REPORTING_CLOSED" or "REPORTING_TRANSFERRED")) ||
            histories.Length is <1 or >257 || (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any())throw new InvalidOperationException("The complete original reporting process/step/history population is ambiguous.");
        if(!ReportingJsonEquivalent(row.INSTANCE_DATA_JSON,process.ProcessData))throw new InvalidOperationException("The stored reporting envelope changed during native snapshot loading.");
        var model=ReportingModel(process);
        var normalized=CompanyInventorySourceReview.Copy(row);normalized.INSTANCE_DATA_JSON=JsonSerializer.Serialize(model.ProcessData);
        return new(model,normalized,steps.Cast<PROCESS_STEP_INSTANCE>().OrderBy(item=>item.STEP_SEQUENCE).Select(CompanyInventorySourceReview.Copy).ToList(),CompanyInventorySourceReview.Copy(histories.OfType<PROCESS_HISTORY>().Single(item=>item.EVENT_TYPE=="PROCESS_STARTED")),
            (await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).Select(CompanyInventorySourceReview.Copy).OrderBy(item=>item.CROSS_TASK_ID,StringComparer.Ordinal).ToList());
    }
    private static void RequireReportingChanged(Beep.OilandGas.Models.Data.ModelEntityBase row,string actor,DateTime minimum)
    {
        if(row.ACTIVE_IND!="Y" || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE is not {} at || at.Kind==DateTimeKind.Local || at<minimum || at>DateTime.UtcNow)
            throw new InvalidOperationException("The reporting closure differs from its actual common-column change audit.");
    }
    private void RequireReportingClosedBusiness(ReportingInitialSnapshot actual,ReportingClosureProof proof)
    {
        var command=proof.Command;var before=proof.Before;var actor=command.Scope().ActorUserId;var at=proof.At;
        var expected=CompanyInventorySourceReview.Copy(before.Model);expected.Status=ProcessStatus.COMPLETED;expected.CurrentState="REPORTING_CLOSED";expected.CompletionDate=at;expected.CompletedBy=actor;expected.ProcessData.LastUpdated=at;
        var stepId=command.Review.Responsibility.StepInstanceId;
        var step=expected.StepInstances.Single(row=>row.StepInstanceId==stepId);step.Status=StepStatus.COMPLETED;step.StartDate=at;step.CompletionDate=at;step.CompletedBy=actor;step.Outcome="REPORTING_CLOSED";step.Notes=command.Reason;
        if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(actual.Model))throw new InvalidOperationException("Reporting closure changed undeclared process or step fields.");
        RequireReportingChanged(actual.Row,actor,at);var row=CompanyInventorySourceReview.Copy(before.Row);CopyBusiness(row,ConvertToProcessInstanceEntity(expected));row.ROW_CHANGED_BY=actor;row.ROW_CHANGED_DATE=actual.Row.ROW_CHANGED_DATE;
        if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(actual.Row))throw new InvalidOperationException("Reporting closure changed undeclared native process fields or original audit.");
        if(actual.Steps.Count!=before.Steps.Count)throw new InvalidOperationException("The complete reporting stage population changed during closure.");
        foreach(var originalStep in before.Steps)
        {
            var actualStep=actual.Steps.Single(row=>row.PROCESS_STEP_INSTANCE_ID==originalStep.PROCESS_STEP_INSTANCE_ID);
            var expectedStep=CompanyInventorySourceReview.Copy(originalStep);
            if(originalStep.PROCESS_STEP_INSTANCE_ID==stepId)
            {CopyBusiness(expectedStep,ConvertToProcessStepInstanceEntity(step));RequireReportingChanged(actualStep,actor,at);expectedStep.ROW_CHANGED_BY=actor;expectedStep.ROW_CHANGED_DATE=actualStep.ROW_CHANGED_DATE;}
            if(ReportingDigest(expectedStep)!=ReportingDigest(actualStep))throw new InvalidOperationException("Reporting closure changed undeclared native step fields or historical audit.");
        }
        if(actual.Tasks.Count!=before.Tasks.Count)throw new InvalidOperationException("The complete reporting task population changed during closure.");
        foreach(var task in actual.Tasks)
        {
            var original=before.Tasks.SingleOrDefault(item=>item.CROSS_TASK_ID==task.CROSS_TASK_ID)??throw new InvalidOperationException("A reporting closure task is unowned.");
            var closed=CompanyInventorySourceReview.Copy(original);
            if(command.Review.Responsibility.TaskIds().Contains(task.CROSS_TASK_ID,StringComparer.Ordinal))
            {
                RequireReportingChanged(task,actor,original.ROW_CHANGED_DATE!.Value);
                if(task.ROW_CHANGED_DATE>at)throw new InvalidOperationException("Reporting task closure audit exceeds its recorded decision UTC.");
                closed.TASK_STATUS="COMPLETED";closed.ROW_CHANGED_BY=actor;closed.ROW_CHANGED_DATE=task.ROW_CHANGED_DATE;
            }
            if(JsonSerializer.Serialize(task)!=JsonSerializer.Serialize(closed))throw new InvalidOperationException("Reporting closure changed another task field or its original accountable audit.");
        }
    }
    private async Task<string> ConfirmExecutionAssignmentAsync(ProcessInstance process,DrillingExecutionAssignment original,CrossPersonaTaskRouter router)
    {
        var transfers=ReportingTransfers(process);
        var proof=ReportingClosure(process);if(proof is null)return transfers.Count>0?await ConfirmReportingTransfersAsync(process,original,router):await ConfirmExecutionAssignmentInitialAsync(process,original,router);
        proof.Command.Require();var command=proof.Command;var id=original.InstanceId();var actor=command.Scope().ActorUserId;
        if(command.Review.Responsibility.Root.Revision()!=original.Revision() || process.InstanceId!=id || process.ProcessData.Data.Count!=(transfers.Count==0?2:3) ||
            ReportingDigest(command.Review.Responsibility)!=ReportingDigest(ReportingGeneration(process,original)) ||
            proof.At.Kind==DateTimeKind.Local || proof.At<command.ClosedAtUtc.UtcDateTime || proof.At>DateTime.UtcNow ||
            ReportingDigest(proof.Before.Model)!=command.Review.ProcessRevision)throw new InvalidOperationException("The retained original reporting closure/source/UTC changed.");
        var histories=(await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).OfType<PROCESS_HISTORY>().ToArray();
        var pendingHistories=histories.Where(row=>row.EVENT_TYPE!="REPORTING_CLOSED").ToArray();
        var initial=await ConfirmReportingPendingAsync(proof.Before,original,transfers,pendingHistories,router);
        if(ProcessDefinitionRevision.FromJson(initial)!=command.Review.NativeRevision)throw new InvalidOperationException("The reporting closure no longer matches its complete original pending graph.");
        var actual=await ReportingSnapshotAsync(process,router);RequireReportingClosedBusiness(actual,proof);
        var closed=histories.Where(item=>item.PROCESS_HISTORY_ID==command.HistoryId()).ToArray();
        var data=JsonSerializer.Serialize(new{IntentRevision=command.Revision(),BeforeRevision=ReportingDigest(proof.Before),AfterRevision=ReportingDigest(actual)});
        if(histories.Length!=transfers.Count+2 || closed.Length!=1 || process.History.Count!=histories.Length || closed[0].EVENT_TYPE!="REPORTING_CLOSED" || closed[0].EVENT_DATE!=proof.At || closed[0].USER_ID!=actor ||
            closed[0].PROCESS_STEP_INSTANCE_ID!=command.Review.Responsibility.StepInstanceId || closed[0].FROM_STATUS!=proof.Before.Model.CurrentState || closed[0].TO_STATUS!="REPORTING_CLOSED" || closed[0].DETAILS!=command.Reason || closed[0].EVENT_DATA_JSON!=data ||
            closed[0].IP_ADDRESS is not null || closed[0].ROW_CREATED_BY!=actor || closed[0].ROW_CREATED_DATE is not {} created || created.Kind==DateTimeKind.Local || created<proof.At)
            throw new InvalidOperationException("The reporting closure canonical audit or exact command/before/after digest changed.");
        RequireReportingChanged(closed[0],actor,created);return JsonSerializer.Serialize(new{Current=actual,Closure=closed[0],Proof=proof,Transfers=transfers,TransferHistory=pendingHistories});
    }
    private async Task ConfirmReportingAcceptanceAsync(DrillingReportingClosure command)
    {
        var progress=command.Review.Evidence.Acceptance;var process=await LoadProcessInstanceAsync(progress.InstanceId)??throw new InvalidOperationException("The original accepted completion review is unavailable.");
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName));
        if(DrillingCompletionReviewStart.OwnerCompany(JsonSerializer.Serialize(process.ProcessData))!=command.Scope().Company ||
            (await FindCompletionReviewAsync(command.Scope().Company,command.Scope().FieldId,progress.Original.ClaimId,router))?.PROCESS_INSTANCE_ID!=progress.InstanceId)
            throw new InvalidOperationException("The complete original company accepted-review population changed.");
        var original=DrillingCompletionReviewStart.FromPayload(JsonSerializer.Serialize(process.ProcessData),progress.InstanceId,command.Scope().FieldId,progress.Original.ClaimId);
        if(original.Revision()!=progress.Original.Revision() || process.Status!=ProcessStatus.COMPLETED || CompletionProofs(process).LastOrDefault()?.Command.Revision()!=progress.Stages.Last().AcceptedDecision!.Revision() ||
            ProcessDefinitionRevision.FromJson(await ConfirmCompletionReviewGraphAsync(process,original))!=progress.NativeRevision)
            throw new InvalidOperationException("The complete accepted completion review differs from its current closure evidence.");
    }
    public async Task<DrillingReportingClosureReceipt> RecordReportingClosureAsync(DrillingReportingClosure input,Func<Task> authority,Func<Task<DrillingAcceptedCompletionTests>> readEvidence,bool recover=false)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var original=command.Review.Responsibility.Root;var id=original.InstanceId();var actor=command.Scope().ActorUserId;
        if(command.Scope().LifecycleDatabase.ConnectionName!=_connectionName)throw new InvalidOperationException("The original reporting closure lifecycle binding is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The reporting closure source is unavailable.");
        async Task Guard(bool owned)
        {
            void Target()
            {
                if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The reporting closure target changed.");
                if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            }
            Target();await authority();Target();
        }
        async Task Evidence(){if(recover)return;await Guard(false);var current=await readEvidence();current.Require(command.Scope().Company,actor,command.Review.Evidence.Acceptance.InstanceId);if(current.Revision!=command.Review.Evidence.Revision)throw new InvalidOperationException("The exact accepted claim, current tests or source changed. Reload reporting closure.");await Guard(false);}
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new ReportingTransferPersonas(()=>command.Review.Responsibility.Workflow,()=>Guard(true)));
        await Guard(false);await Evidence();ReportingClosureProof? recorded=null;string? graph=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var process=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original reporting responsibility is unavailable.");
            if(DrillingExecutionAssignment.OwnerCompany(JsonSerializer.Serialize(process.ProcessData))!=command.Scope().Company || DrillingExecutionAssignment.FromPayload(JsonSerializer.Serialize(process.ProcessData),id,command.Scope().FieldId,original.OperationId).Revision()!=original.Revision())throw new UnauthorizedAccessException("The reporting closure belongs to another original company or assignment.");
            if((await FindExecutionAssignmentAsync(command.Scope().Company,command.Scope().FieldId,original.OperationId,router))?.PROCESS_INSTANCE_ID!=id)throw new InvalidOperationException("The complete reporting responsibility source population changed.");
            graph=await ConfirmExecutionAssignmentAsync(process,original,router);recorded=ReportingClosure(process);
            if(recorded is not null){if(recorded.Command.Revision()!=command.Revision())throw new InvalidOperationException("Another exact original closure already owns this responsibility.");await Guard(true);return;}
            if((await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID",command.HistoryId()))).Any())throw new InvalidOperationException("Partial or retagged reporting closure audit cannot become absence.");
            if(recover)return;
            if(ProcessDefinitionRevision.FromJson(graph)!=command.Review.NativeRevision || ReportingDigest(ReportingModel(process))!=command.Review.ProcessRevision ||
                ReportingDigest(ReportingGeneration(process,original))!=ReportingDigest(command.Review.Responsibility))throw new InvalidOperationException("The displayed current reporting generation changed before closure.");
            await ConfirmReportingAcceptanceAsync(command);var before=await ReportingSnapshotAsync(process,router);
            await router.CompleteTaskSetInOwnedTransactionAsync(id,command.Review.Responsibility.StepInstanceId,command.Review.Responsibility.Workflow.Step.AssignedRole,"DRILLING_OPERATION",original.OperationId,actor,_connectionName,ReportingTransfers(process).LastOrDefault()?.Command);
            await Guard(true);
            var at=DateTime.UtcNow;if(at<command.ClosedAtUtc.UtcDateTime)throw new InvalidOperationException("Closure recording UTC precedes its explicit actual UTC.");
            process.Status=ProcessStatus.COMPLETED;process.CurrentState="REPORTING_CLOSED";process.CompletionDate=at;process.CompletedBy=actor;process.ProcessData.LastUpdated=at;
            var step=process.StepInstances.Single(row=>row.StepInstanceId==command.Review.Responsibility.StepInstanceId);step.Status=StepStatus.COMPLETED;step.StartDate=at;step.CompletionDate=at;step.CompletedBy=actor;step.Outcome="REPORTING_CLOSED";step.Notes=command.Reason;
            var stepRow=CompanyInventorySourceReview.Copy(before.Steps.Single(row=>row.PROCESS_STEP_INSTANCE_ID==step.StepInstanceId));CopyBusiness(stepRow,ConvertToProcessStepInstanceEntity(step));await GetProcessStepInstanceRepository().UpdateAsync(stepRow,actor);await Guard(true);
            recorded=new(command,at,before);process.ProcessData.Data[DrillingReportingClosure.PayloadKey]=recorded;
            var row=CompanyInventorySourceReview.Copy(before.Row);CopyBusiness(row,ConvertToProcessInstanceEntity(process));if(row.INSTANCE_DATA_JSON?.Length>DrillingExecutionAssignment.MaximumBodyLength+65536)throw new InvalidOperationException("The complete original reporting closure proof exceeds its supported bound.");await GetProcessInstanceRepository().UpdateAsync(row,actor);await Guard(true);
            var afterProcess=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded reporting closure is unavailable.");var after=await ReportingSnapshotAsync(afterProcess,router);RequireReportingClosedBusiness(after,recorded);
            var history=ConvertToProcessHistoryEntity(new ProcessHistoryEntry{HistoryId=command.HistoryId(),InstanceId=id,StepInstanceId=step.StepInstanceId,Action="REPORTING_CLOSED",PreviousState=before.Model.CurrentState,NewState="REPORTING_CLOSED",Timestamp=at,PerformedBy=actor,Notes=command.Reason});
            history.EVENT_DATA_JSON=JsonSerializer.Serialize(new{IntentRevision=command.Revision(),BeforeRevision=ReportingDigest(before),AfterRevision=ReportingDigest(after)});await GetProcessHistoryRepository().InsertAsync(history,actor);
            await ConfirmReportingAcceptanceAsync(command);await Guard(true);process=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded reporting closure disappeared.");graph=await ConfirmExecutionAssignmentAsync(process,original,router);await Guard(true);
        });
        await Guard(false);var closing=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original reporting closure commit is unconfirmed.");if(graph!=await ConfirmExecutionAssignmentAsync(closing,original,router))throw new InvalidOperationException("The original reporting closure graph changed around commit. Recover its retained input.");await Evidence();await Guard(false);
        closing=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original reporting closure disappeared around closing source proof.");
        if(graph!=await ConfirmExecutionAssignmentAsync(closing,original,router) || (await FindExecutionAssignmentAsync(command.Scope().Company,command.Scope().FieldId,original.OperationId,router))?.PROCESS_INSTANCE_ID!=id)throw new InvalidOperationException("The complete reporting closure or source population changed after closing evidence.");await Guard(false);
        var result=new DrillingReportingClosureReceipt(command.Revision(),id,recorded is not null,recorded?.Command.HistoryId(),recorded?.Command.ClosedAtUtc,command.Review.Responsibility.TaskIds(),ProcessDefinitionRevision.FromJson(graph!),DateTimeOffset.UtcNow);result.Require(command);return result;
    }
}
