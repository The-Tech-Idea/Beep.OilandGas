using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService:ICompanyWorkOrderCompletionDecisionOwner,ICompanyWorkOrderCompletionDecisionReads
{
    private const string CompletionDecisionKey="CompanyWorkOrderCompletionDecision";
    private sealed record CompletionDecisionEvidence(CompanyWorkOrderCompletionDecisionCommand Command,PROCESS_APPROVAL Approval,PROCESS_HISTORY History,
        [property:JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] MaintenanceGraphDelta? Delta=null,
        [property:JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingDefault)] int AuditVersion=0);
    private static CompanyWorkOrderCompletionDecisionRecovery CompletionDecisionResult(CompletionDecisionEvidence proof)
    {
        var stage=proof.Command.Review.Stage;var reference=proof.Command.Reference();return new(proof.Command,new(stage.Context.Company,stage.Context.ActorUserId,reference.IntentId,reference.Query.SubmissionId,stage.StepInstanceId,proof.Approval.PROCESS_APPROVAL_ID,proof.History.PROCESS_HISTORY_ID,proof.Command.Decision,proof.Approval.ROW_CREATED_DATE!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(proof))));
    }
    public async Task<CompanyWorkOrderCompletionDecisionFacts?> ReadCompletionDecisionFactsAsync(FieldSelectionReview lifecycle,CompanyWorkOrderCompletionQuery query,Func<Task> authority)
    {
        query.Require();var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original completion decision lifecycle source is unavailable.");
        async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original decision read lifecycle pointer changed.");PPDMDataSourceTransaction.RequireIdle(source);}
        await Guard();var first=await ReadCompletionDecisionEvidence(lifecycle,query);await Guard();var closing=await ReadCompletionDecisionEvidence(lifecycle,query);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original immutable decision approval/history population changed during public observation.");
        await Guard();if(closing is null)return null;var stage=closing.Command.Review.Stage;var reference=closing.Command.Reference();var receipt=new CompanyWorkOrderCompletionDecisionReceipt(stage.Context.Company,stage.Context.ActorUserId,reference.IntentId,reference.Query.SubmissionId,stage.StepInstanceId,closing.Approval.PROCESS_APPROVAL_ID,closing.History.PROCESS_HISTORY_ID,closing.Command.Decision,closing.Approval.ROW_CREATED_DATE!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing)));
        var result=CompanyWorkOrderCompletionDecisionFacts.From(new(closing.Command,receipt));await Guard();return result;
    }
    public async Task<CompanyMaintenanceAssignedStageReview> ReviewCompletionDecisionStageAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<string?,Task> authority)
    {
        query.Require();var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original completion lifecycle source is unavailable.");PPDMDataSourceTransaction.RequireIdle(source);
        var stage=await ReviewExecutionReviewerStageAsync(context,query.Task,authority);if(await ReadCompletionDecisionEvidence(context,query) is not null)throw new InvalidOperationException("A completion decision is already recorded; recover the original reference instead of preparing another decision.");
        await authority(stage.AssignedRole);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original decision review source changed.");PPDMDataSourceTransaction.RequireIdle(source);return stage;
    }
    private static void RemoveCompletionDecisionFromPriorDigest(ProcessInstance instance)
    {
        if(!instance.ProcessData.Data.Remove(CompletionDecisionKey,out var value))return;
        var proof=JsonSerializer.Deserialize<CompletionDecisionEvidence>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("Recorded completion decision proof is malformed.");proof.Command.Require();
        instance.History.RemoveAll(row=>row.HistoryId==proof.Command.HistoryId());
    }
    private static PROCESS_APPROVAL CompletionApproval(CompanyWorkOrderCompletionDecisionCommand command,DateTime utc,bool ownedAudit=false)
    {
        var result=new PROCESS_APPROVAL
        {
        PROCESS_APPROVAL_ID=command.ApprovalId(),PROCESS_INSTANCE_ID=command.Review.Stage.Task.InstanceId,PROCESS_STEP_INSTANCE_ID=command.Review.Stage.StepInstanceId,
        APPROVAL_TYPE="WORK_COMPLETION",APPROVAL_STATUS=command.Decision,APPROVER_USER_ID=command.Review.Stage.Context.ActorUserId,APPROVER_SEQUENCE=1,
        REQUIRED_ACTION=ownedAudit?"INDEPENDENT_VERIFICATION_V1":"INDEPENDENT_VERIFICATION",APPROVAL_DATE=utc,APPROVED_BY=command.Review.Stage.Context.ActorUserId,APPROVAL_NOTES=command.Reason,
        REQUESTED_BY=command.Review.Completion.Report.ExecutorUserId,REQUESTED_DATE=command.Review.Completion.Report.RecordedAtUtc,ACTIVE_IND="Y",ROW_CREATED_BY=command.Review.Stage.Context.ActorUserId,ROW_CREATED_DATE=utc
        };
        if(ownedAudit){result.PPDM_GUID=MaintenanceAuditGuid("PROCESS_APPROVAL",result.PROCESS_APPROVAL_ID);result.ROW_CHANGED_BY=result.ROW_CREATED_BY;result.ROW_CHANGED_DATE=utc;result.ROW_EFFECTIVE_DATE=utc;}return result;
    }
    private PROCESS_HISTORY CompletionHistory(CompanyWorkOrderCompletionDecisionCommand command,DateTime utc,bool ownedAudit=false)
    {
        var stage=command.Review.Stage;var result=ConvertToProcessHistoryEntity(new ProcessHistoryEntry{HistoryId=command.HistoryId(),InstanceId=stage.Task.InstanceId,StepInstanceId=stage.StepInstanceId,Action=command.Decision=="VERIFIED"?"WORK_VERIFIED":"WORK_REJECTED",PreviousState=stage.StepId,NewState=stage.StepId,Timestamp=utc,PerformedBy=stage.Context.ActorUserId,Notes=command.Reason});result.ROW_CREATED_BY=stage.Context.ActorUserId;result.ROW_CREATED_DATE=utc;if(ownedAudit){result.PPDM_GUID=MaintenanceAuditGuid("PROCESS_HISTORY",result.PROCESS_HISTORY_ID);result.ROW_CHANGED_BY=stage.Context.ActorUserId;result.ROW_CHANGED_DATE=utc;result.ROW_EFFECTIVE_DATE=utc;}return result;
    }
    private async Task<CompletionDecisionEvidence?> ReadCompletionDecisionEvidence(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,bool privateReviewer=false)
    {
        var owner=await ReadExecutionHandoffOwner(context,query.Task);var graph=MaintenanceBeforeReturnGraph(await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,owner.Start.ProcessId),context,query);var snapshot=JsonNode.Parse(graph)!.AsObject();var data=MaintenanceGraphInstance(graph).ProcessData.Data;
        var approvals=MaintenanceGraphRows<PROCESS_APPROVAL>(snapshot,"Approvals").Where(row=>row.APPROVAL_TYPE=="WORK_COMPLETION").ToArray();var histories=MaintenanceGraphRows<PROCESS_HISTORY>(snapshot,"History").Where(row=>row.EVENT_TYPE is "WORK_VERIFIED" or "WORK_REJECTED").ToArray();
        if(!data.TryGetValue(CompletionDecisionKey,out var value)){if(approvals.Length!=0 || histories.Length!=0)throw new InvalidOperationException("Uncorrelated completion decision rows exist; recovery cannot replace them.");return null;}
        if(privateReviewer && (approvals.Length!=1 || approvals[0].APPROVED_BY!=context.ActorUserId || approvals[0].APPROVER_USER_ID!=context.ActorUserId))throw new UnauthorizedAccessException("Original decision approval belongs to another reviewer before private proof decoding.");
        var proof=JsonSerializer.Deserialize<CompletionDecisionEvidence>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("Original completion decision proof is unavailable.");proof.Command.Require();var stage=proof.Command.Review.Stage;
        if(proof.AuditVersion is not (0 or 1) || proof.AuditVersion==1 && proof.Delta is null)throw new InvalidOperationException("Original decision audit protocol is unsupported or incomplete.");
        if(stage.Context.Company!=context.Company || stage.Context.Database!=context.Database || proof.Command.Review.Completion.Report.Query!=query || proof.Approval.ROW_CREATED_DATE is not {} utc || utc.Kind!=DateTimeKind.Utc || utc<proof.Command.Review.Completion.ComparedAtUtc ||
            approvals.Length!=1 || histories.Length!=1 || JsonSerializer.Serialize(proof.Approval)!=JsonSerializer.Serialize(CompletionApproval(proof.Command,utc,proof.AuditVersion==1)) || JsonSerializer.Serialize(proof.History)!=JsonSerializer.Serialize(CompletionHistory(proof.Command,utc,proof.AuditVersion==1)) ||
            JsonSerializer.Serialize(approvals[0])!=JsonSerializer.Serialize(proof.Approval) || JsonSerializer.Serialize(histories[0])!=JsonSerializer.Serialize(proof.History))throw new InvalidOperationException("Original completion approval/history/source/actor/audit correlations differ.");
        if(proof.Delta is {} delta)
        {
            if(delta.EvidenceKey!=CompletionDecisionKey || delta.BeforeRevision!=stage.ProcessRevision || delta.Changes.Count!=3 ||
                delta.Changes.Count(change=>change.Section=="Process" && change.Id==query.Task.InstanceId && change.Before is not null)!=1 ||
                delta.Changes.Count(change=>change.Section=="Approvals" && change.Id==proof.Command.ApprovalId() && change.Before is null)!=1 ||
                delta.Changes.Count(change=>change.Section=="History" && change.Id==proof.Command.HistoryId() && change.Before is null)!=1)throw new InvalidOperationException("Original decision delta differs from its exact process update and approval/history appends.");
            RestoreMaintenanceDelta(graph,delta);
        }
        return proof;
    }
    private async Task<CompanyWorkOrderCompletionDecisionRecovery> CompletionDecisionReceipt(FieldSelectionReview context,CompanyWorkOrderCompletionDecisionReference reference,Func<string?,Task> authority)
    {
        var first=await ReadCompletionDecisionEvidence(context,reference.Query,true)??throw new InvalidOperationException("Original completion decision is unconfirmed; recovery cannot record it.");
        if(first.Command.Reference()!=reference || first.Command.Review.Stage.Context.ActorUserId!=context.ActorUserId)throw new UnauthorizedAccessException("Original completion decision belongs to another reviewer/reference.");await authority(first.Command.Review.Stage.AssignedRole);
        var closing=await ReadCompletionDecisionEvidence(context,reference.Query,true);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original completion decision changed during recovery.");
        var receipt=new CompanyWorkOrderCompletionDecisionReceipt(context.Company,context.ActorUserId,reference.IntentId,reference.Query.SubmissionId,first.Command.Review.Stage.StepInstanceId,first.Approval.PROCESS_APPROVAL_ID,first.History.PROCESS_HISTORY_ID,first.Command.Decision,first.Approval.ROW_CREATED_DATE!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(first)));
        var result=new CompanyWorkOrderCompletionDecisionRecovery(first.Command,receipt);result.Require(context.Company,context.ActorUserId,reference);return result;
    }
    public async Task<CompanyWorkOrderCompletionDecisionRecovery> RecoverCompletionDecisionAsync(FieldSelectionReview context,CompanyWorkOrderCompletionDecisionReference reference,Func<string?,Task> authority)
    {
        reference.Require();var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original lifecycle decision source is unavailable.");PPDMDataSourceTransaction.RequireIdle(source);await authority(null);
        var result=await CompletionDecisionReceipt(context,reference,authority);await authority(result.Command.Review.Stage.AssignedRole);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original decision source changed.");PPDMDataSourceTransaction.RequireIdle(source);return result;
    }
    public async Task<CompanyWorkOrderCompletionDecisionRecovery> RecordCompletionDecisionAsync(CompanyWorkOrderCompletionDecisionCommand original,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderCompletionReview?>> readCompletion)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var stage=command.Review.Stage;var context=stage.Context;var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original decision lifecycle source is unavailable.");
        async Task Guard(bool owned){if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority(stage.AssignedRole);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original decision lifecycle binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        await Guard(false);var existing=await ReadCompletionDecisionEvidence(context,command.Reference().Query);
        if(existing is not null){if(JsonSerializer.Serialize(existing.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Original completion decision already exists with another body/reference.");return await RecoverCompletionDecisionAsync(context,command.Reference(),authority);}
        async Task Source(bool owned){var current=await readCompletion(owned);if(!owned && current is null || current is not null && current.SourceRevision()!=command.Review.Completion.SourceRevision())throw new InvalidOperationException("Original current completion/person/inspection evidence changed.");}
        async Task Stage(bool owned){await Guard(owned);var current=await ReviewExecutionReviewerStageAsync(context,stage.Task,authority);if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(stage))throw new InvalidOperationException("Original configured reviewer assignment/source changed.");}
        await Source(false);await Stage(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);await Source(true);await Stage(true);if(await ReadCompletionDecisionEvidence(context,command.Reference().Query) is not null)throw new InvalidOperationException("Original reviewer decision appeared during the owned command.");
            var beforeGraph=await MaintenanceAcceptanceNativeGraph(stage.Task.InstanceId,(await ReadExecutionHandoffOwner(context,stage.Task)).Start.ProcessId);
            if(ProcessDefinitionRevision.FromJson(beforeGraph)!=stage.ProcessRevision)throw new InvalidOperationException("Exact original configured reviewer graph changed before the decision append.");
            var utc=DateTime.UtcNow;if(utc<command.Review.Completion.ComparedAtUtc)throw new InvalidOperationException("Decision UTC precedes its original evidence.");var approval=CompletionApproval(command,utc,true);var history=CompletionHistory(command,utc,true);
            await MaintenanceDecisionWriter(approval,null,"PROCESS_APPROVAL","PROCESS_APPROVAL_ID",context.ActorUserId,PPDMRowMutation.Insert,()=>Guard(true)).InsertAsync(approval,context.ActorUserId);
            await MaintenanceDecisionWriter(history,null,"PROCESS_HISTORY","PROCESS_HISTORY_ID",context.ActorUserId,PPDMRowMutation.Insert,()=>Guard(true)).InsertAsync(history,context.ActorUserId);
            var owner=await ReadExecutionHandoffOwner(context,stage.Task);owner.Instance.ProcessData.Data[CompletionDecisionKey]=new CompletionDecisionEvidence(command,approval,history,AuditVersion:1);
            var row=CompanyInventorySourceReview.Copy((PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",stage.Task.InstanceId))).Single());var previous=CompanyInventorySourceReview.Copy(row);var createdBy=row.ROW_CREATED_BY;var created=row.ROW_CREATED_DATE;row.INSTANCE_DATA_JSON=JsonSerializer.Serialize(owner.Instance.ProcessData);row.ROW_CHANGED_BY=context.ActorUserId;row.ROW_CHANGED_DATE=utc;
            var expectedGraph=JsonNode.Parse(await MaintenanceAcceptanceNativeGraph(stage.Task.InstanceId,owner.Start.ProcessId))!.AsObject();expectedGraph["Process"]=JsonSerializer.SerializeToNode(row);
            var delta=CaptureMaintenanceDelta(CompletionDecisionKey,beforeGraph,expectedGraph.ToJsonString());owner.Instance.ProcessData.Data[CompletionDecisionKey]=new CompletionDecisionEvidence(command,approval,history,delta,1);row.INSTANCE_DATA_JSON=JsonSerializer.Serialize(owner.Instance.ProcessData);
            await MaintenanceDecisionWriter(row,previous,"PROCESS_INSTANCE","PROCESS_INSTANCE_ID",context.ActorUserId,PPDMRowMutation.Update,()=>Guard(true)).UpdateAsync(row,context.ActorUserId);var saved=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",stage.Task.InstanceId))).Single();if(row.ROW_CREATED_BY!=createdBy || row.ROW_CREATED_DATE!=created || JsonSerializer.Serialize(saved)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Exact original process decision payload/audit readback is unconfirmed.");
            await Guard(true);await Source(true);await CompletionDecisionReceipt(context,command.Reference(),authority);
        });
        await Guard(false);await Source(false);return await RecoverCompletionDecisionAsync(context,command.Reference(),authority);
    }
}
