using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

public sealed partial class WorkOrderExecutionPermitService
{
    private static readonly System.Diagnostics.ActivitySource DecisionActivities=new("Beep.OilandGas.WorkOrderPermitDecision");
    public async Task<CompanyWorkOrderExecutionPermitDecisionReceipt> DecideAsync(CompanyWorkOrderExecutionPermitDecision original,
        Func<Task> authority,Func<Task<CompanyMaintenanceInspectionReview>> readSource,bool readOnly=false,
        Func<Task>? requireCurrentPrerequisiteProof=null,Func<Task>? requireCorePrerequisiteProof=null,Func<Task>? requireCommittedPrerequisiteProof=null)
    {
        using var decisionActivity=DecisionActivities.StartActivity("permit.decision");
        ArgumentNullException.ThrowIfNull(original);ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readSource);
        var decision=CompanyInventorySourceReview.Copy(original);decision.Require();var context=decision.Review.Context;
        var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("The original permit decision source is unavailable.");
        var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),context.Database.ConnectionName,Table);
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("The original permit decision database changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task CurrentSource()
        {
            PPDMDataSourceTransaction.RequireIdle(source);var observed=await readSource();observed.Require(context.Company,context.ActorUserId,decision.Review.Proposal.Task);
            if(observed.SnapshotRevision!=decision.Review.CurrentSource.SnapshotRevision || observed.InspectionDatabase!=context.Database)throw new InvalidOperationException("The original independent decision source/assignment changed; re-review rather than adopt another body.");
        }
        async Task PrerequisiteProof(bool owned,bool committed=false)
        {
            if(decision.Outcome!="Approve")return;
            using var proofActivity=DecisionActivities.StartActivity(owned?"permit.proof.owned":committed?"permit.proof.committed":"permit.proof.current");
            decision.Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);
            if(requireCurrentPrerequisiteProof is null || requireCorePrerequisiteProof is null)throw new InvalidOperationException("Both current and owned native prerequisite proof owners are required before approval.");
            var proof=owned?requireCorePrerequisiteProof:committed?requireCommittedPrerequisiteProof??requireCurrentPrerequisiteProof:requireCurrentPrerequisiteProof;
            if(proof is null)throw new InvalidOperationException("Approval requires complete current and owned native prerequisite proof owners.");
            await Guard(owned);await proof();await Guard(owned);
            decision.Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);
        }
        async Task<WORK_ORDER_EXECUTION_PERMIT> Read()
        {
            var rows=(await repository.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=decision.Review.Proposal.PermitId()}])).ToArray();
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_EXECUTION_PERMIT row)throw new InvalidOperationException("The original permit decision row is unavailable or ambiguous.");return row;
        }
        await Guard(false);RequireSchema(source);WORK_ORDER_EXECUTION_PERMIT? recorded=null;
        var initial=await Read();var fresh=initial.DECISION_DATA_JSON is null;
        var completeSourceProof=fresh && !readOnly && decision.Outcome=="Approve";
        // A fresh approval's complete prerequisite reader already closes this exact order/inspection/stage snapshot.
        if(!completeSourceProof)await CurrentSource();await Guard(false);
        if(fresh && !readOnly)await PrerequisiteProof(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);RequireSchema(source);var current=await Read();
            if(current.DECISION_DATA_JSON is not null){decision.RequireRecorded(current);recorded=current;}
            else
            {
                decision.RequirePending(current);if(readOnly)throw new InvalidOperationException("The original independent decision is not recorded; read-only recovery cannot create it.");
                await PrerequisiteProof(true);
                var intended=decision.Prepare(current,DateTime.UtcNow);
                using(DecisionActivities.StartActivity("permit.decision.write-readback")){await repository.UpdateAsync(intended,context.ActorUserId);recorded=await Read();decision.RequireRecorded(recorded);}
                if(recorded.DECIDED_AT_UTC!=intended.DECIDED_AT_UTC)throw new InvalidOperationException("The original independent decision time changed during readback.");
                await PrerequisiteProof(true);
            }
            await Guard(true);
        });
        if(recorded is null)throw new InvalidOperationException("The original independent decision is unconfirmed.");var frozen=JsonSerializer.Serialize(recorded);
        // Owned core evidence closed before/after mutation. An explicit committed proof closes separate providers;
        // the complete current order/inspection/stage and native recorded body still close after commit.
        await Guard(false);if(!completeSourceProof || requireCommittedPrerequisiteProof is not null)await CurrentSource();await Guard(false);if(fresh && !readOnly)await PrerequisiteProof(false,true);RequireSchema(source);var committed=await Read();decision.RequireRecorded(committed);
        if(frozen!=JsonSerializer.Serialize(committed))throw new InvalidOperationException("The independent decision changed after authority/source closure.");await Guard(false);
        var receipt=new CompanyWorkOrderExecutionPermitDecisionReceipt(context.Company,context.ActorUserId,decision.IntentId,decision.Review.Proposal.PermitId(),decision.Outcome,ProcessDefinitionRevision.FromJson(frozen));receipt.Require(decision);return receipt;
    }
    public async Task<CompanyWorkOrderExecutionPermitDecision> ReadOriginalDecisionAsync(FieldSelectionReview current,
        CompanyWorkOrderExecutionPermitDecisionReference query,Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(query);ArgumentNullException.ThrowIfNull(authority);query.Require();FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(current.FieldId!=query.Proposal.Task.FieldId)throw new UnauthorizedAccessException("The original decision reviewer/FIELD is required.");
        var source=editor.GetDataSource(current.Database.ConnectionName)??throw new InvalidOperationException("The original decision datasource is unavailable.");
        async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,editor.GetDataSource(current.Database.ConnectionName)))throw new InvalidOperationException("The original decision source changed.");PPDMDataSourceTransaction.RequireIdle(source);}
        await Guard();RequireSchema(source);var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),current.Database.ConnectionName,Table);
        var id=CompanyWorkOrderExecutionPermitIntent.PermitIdFor(current.Company,query.Proposal.IntentId);
        async Task<WORK_ORDER_EXECUTION_PERMIT> Read()
        {
            var rows=(await repository.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=id}])).ToArray();
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_EXECUTION_PERMIT row)throw new InvalidOperationException("The original independent decision reference is unavailable or ambiguous.");
            if(row.TENANT_ID!=current.Company.TenantId || row.BUSINESS_ASSOCIATE_ID!=current.Company.BusinessAssociateId || row.DECIDED_BY!=current.ActorUserId || row.FIELD_ID!=query.Proposal.Task.FieldId || row.WORK_ORDER_ID!=query.Proposal.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=query.Proposal.Task.InstanceId || row.DECISION_INTENT_ID!=query.IntentId.ToString("D"))throw new UnauthorizedAccessException("The original decision is private to its reviewer/company/task/reference.");return row;
        }
        var recorded=await Read();var decision=CompanyWorkOrderExecutionPermitDecision.FromRecorded(recorded);
        if(decision.Review.Context!=current || decision.ProposalReference()!=query.Proposal || decision.IntentId!=query.IntentId)throw new UnauthorizedAccessException("The original decision authority/reference changed.");
        var frozen=JsonSerializer.Serialize(recorded);await Guard();if(frozen!=JsonSerializer.Serialize(await Read()))throw new InvalidOperationException("The original decision changed during private reference review.");await Guard();return CompanyInventorySourceReview.Copy(decision);
    }
}
