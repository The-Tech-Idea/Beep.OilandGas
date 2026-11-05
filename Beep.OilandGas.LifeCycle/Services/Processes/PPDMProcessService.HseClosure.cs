using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Accounting.Models;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private const string HseClosureKey = "CompanyHseClosureStages";
    private const string HseCorrectionKey = "CompanyHseCorrectionReturns";
    private static void RequireReviewedHseMutation(ProcessInstance? process, string? stepId = null, bool approval = false)
    {
        if (process is not null && (process.ProcessId == CompanyHseWorkflowStart.ProcessId || process.ProcessData?.Data.ContainsKey(CompanyHseWorkflowStart.PayloadKey) == true) &&
            (approval || stepId is "INC_CORRECTIVE_ACTIONS" or "INC_VERIFICATION" or "INC_CLOSURE" || process.CurrentStepId is "INC_CORRECTIVE_ACTIONS" or "INC_VERIFICATION" or "INC_CLOSURE" || process.ProcessData?.Data.ContainsKey(HseClosureKey) == true))
            throw new InvalidOperationException("Incident verification and closure require the reviewed native lifecycle owner.");
    }
    public override async Task<bool> CompleteStepAsync(string instanceId, string stepId, string outcome, string userId)
    {
        RequireReviewedHseMutation(await LoadProcessInstanceAsync(instanceId), stepId);
        return await base.CompleteStepAsync(instanceId, stepId, outcome, userId);
    }
    public override async Task<bool> ExecuteStepAsync(string instanceId, string stepId, PROCESS_STEP_DATA stepData, string userId)
    {
        RequireReviewedHseMutation(await LoadProcessInstanceAsync(instanceId), stepId);
        return await base.ExecuteStepAsync(instanceId, stepId, stepData, userId);
    }
    private sealed record HseClosureProof(CompanyHseClosureCommand Command, ProcessInstance Snapshot, string Digest, PROCESS_HISTORY History,
        PROCESS_APPROVAL? Approval, List<CROSS_PERSONA_TASK> Closed, List<CROSS_PERSONA_TASK> Next, string? NextStep);
    private static string HseClosureDigest(ProcessInstance process)
    {
        var copy = JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(process))!;
        copy.ProcessData.Data.Remove(HseClosureKey); copy.ProcessData.Data.Remove(HseCorrectionKey); return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    private static List<HseClosureProof> HseClosureProofs(ProcessInstance process) => process.ProcessData.Data.TryGetValue(HseClosureKey, out var value)
        ? JsonSerializer.Deserialize<List<HseClosureProof>>(JsonSerializer.Serialize(value)) ?? throw new InvalidOperationException("Recorded closure proofs are invalid.") : [];

    public async Task<CompanyHseClosureReview> ReviewClosureStageAsync(IncidentClosureReview evidence, AssetDatabaseScope lifecycle, string nativeRevision, Func<string, Task> requireRoleAuthority)
        => await ReviewClosureStageCoreAsync(evidence, lifecycle, nativeRevision, requireRoleAuthority, false);

    private async Task<CompanyHseClosureReview> ReviewClosureStageCoreAsync(IncidentClosureReview evidence, AssetDatabaseScope lifecycle, string nativeRevision, Func<string, Task> requireRoleAuthority, bool owned, bool correction = false)
    {
        evidence.Require(); var context = evidence.Actions.Context;
        if (_personaReader is null || lifecycle.ConnectionName != _connectionName) throw new InvalidOperationException("The fixed lifecycle owner and personas are required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The lifecycle datasource is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName), personas: _personaReader);
        async Task<CompanyHseClosureReview> Capture()
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The lifecycle datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            var definition = await GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId) ?? throw new InvalidOperationException("The incident definition is unavailable.");
            if (!definition.IsActive || definition.ProcessType != "HSE" || definition.EntityType != CompanyHseWorkflowStart.EntityType || definition.Steps is not { Count: > 0 and <= 64 } ||
                definition.Steps.Any(t => t is null || !LeaseSummaryValidator.Identifier(t.StepId) || t.RequiredRoles is not { Count: 1 } || !LeaseSummaryValidator.Identifier(t.RequiredRoles[0])) ||
                definition.Steps.Select(t => t.StepId).Distinct(StringComparer.Ordinal).Count() != definition.Steps.Count || definition.Steps.Select(t => t.SequenceNumber).Distinct().Count() != definition.Steps.Count)
                throw new InvalidOperationException("The incident definition requires exact stages and roles.");
            var definitionRevision = ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition));
            var candidates = await GetProcessInstancesForEntityAsync(evidence.Actions.IncidentId, CompanyHseWorkflowStart.EntityType) ?? throw new InvalidOperationException("The incident workflow population is unavailable.");
            var matches = new List<ProcessInstance>();
            foreach (var candidate in candidates)
            {
                if (candidate is null || candidate.ProcessId != CompanyHseWorkflowStart.ProcessId || candidate.EntityType != CompanyHseWorkflowStart.EntityType || candidate.EntityId != evidence.Actions.IncidentId)
                    throw new InvalidOperationException("The incident workflow ownership is ambiguous.");
                var start = CompanyHseWorkflowStart.FromPayload(JsonSerializer.Serialize(candidate.ProcessData), candidate.InstanceId, candidate.FieldId, evidence.Actions.IncidentId);
                if (start.Context.Company != context.Company || start.Context.FieldId != context.FieldId) continue;
                if (start.Context.Database != context.Database || start.LifecycleDatabase != lifecycle || start.DefinitionRevision != definitionRevision) throw new InvalidOperationException("The incident workflow binding or definition changed.");
                matches.Add(candidate);
            }
            if (matches.Count != 1) throw new InvalidOperationException("Exactly one owned incident workflow is required.");
            var process = matches[0]; var stage = process.CurrentStepId;
            if (process.Status != ProcessStatus.IN_PROGRESS || stage is not ("INC_CORRECTIVE_ACTIONS" or "INC_VERIFICATION" or "INC_CLOSURE"))
                throw new InvalidOperationException("Complete the earlier assigned incident stages before closure review.");
            var ordered = definition.Steps.OrderBy(t => t.SequenceNumber).ToArray(); var index = Array.FindIndex(ordered, t => t.StepId == stage); var template = ordered[index];
            if (process.StepInstances.Count != ordered.Length || process.StepInstances.Select(s => s.StepId).Distinct(StringComparer.Ordinal).Count() != ordered.Length ||
                ordered.Any(t => process.StepInstances.Count(s => s.StepId == t.StepId && s.SequenceNumber == t.SequenceNumber && s.InstanceId == process.InstanceId && s.RequiredRole == t.RequiredRoles[0] && s.ApprovalRequired == t.RequiresApproval) != 1))
                throw new InvalidOperationException("The recorded incident assignments differ from their definition.");
            var step = process.StepInstances.Single(s => s.StepId == stage); var role = step.RequiredRole!;
            var nextName = stage == "INC_CORRECTIVE_ACTIONS" ? "INC_VERIFICATION" : stage == "INC_VERIFICATION" ? "INC_CLOSURE" : null;
            if (step.Status is not (StepStatus.PENDING or StepStatus.IN_PROGRESS) || !LeaseSummaryValidator.Identifier(step.StepInstanceId) ||
                !string.IsNullOrEmpty(step.AssignedTo) && step.AssignedTo != context.ActorUserId ||
                process.StepInstances.Any(s => s.SequenceNumber < step.SequenceNumber && s.Status != StepStatus.COMPLETED) ||
                template.RequiresApproval != (stage != "INC_CORRECTIVE_ACTIONS") || template.StepType != (stage == "INC_CORRECTIVE_ACTIONS" ? "ACTION" : "APPROVAL") ||
                template.ConditionalNextSteps is not { Count: 0 } || template.ValidationRules is not { Count: 0 } || template.StepConfiguration is not { Count: 0 } ||
                (nextName is null ? index != ordered.Length - 1 || !string.IsNullOrEmpty(template.NextStepId) : index + 1 >= ordered.Length || ordered[index + 1].StepId != nextName || template.NextStepId != nextName || process.StepInstances.Single(s => s.StepId == nextName).Status != StepStatus.BLOCKED))
                throw new InvalidOperationException("The assigned incident stage, earlier steps or approval policy changed.");
            var round = CompanyHseCorrectionRound.FromPayload(JsonSerializer.Serialize(process.ProcessData), process.InstanceId, process.FieldId, evidence.Actions.IncidentId);
            var priorProofs = HseClosureProofs(process).Where(p => p.Command.Review.CorrectionRound == round?.IntentId).ToList();
            if (!correction && round is not null)
            {
                var returned = HseCorrectionProofs(process).Single(p => p.Receipt.Original.IntentId == round.IntentId); RequireCorrectionProof(returned);
                var rejectedActions = returned.Receipt.Original.Review.Evidence.Actions.Actions;
                var additional = evidence.Actions.Actions.Where(a => rejectedActions.All(old => old.StepId != a.StepId)).ToArray();
                if (evidence.Actions.Revision == round.RejectedRevision || rejectedActions.Any(old => evidence.Actions.Actions.Count(a => a == old) != 1) ||
                    additional.Length == 0 || additional.Any(a => a.Status != "COMPLETED" || a.CompletedAt < returned.History.EVENT_DATE))
                    throw new InvalidOperationException("Preserve the rejected evidence and complete new corrective work after its recorded return before another handoff.");
            }
            if (stage != "INC_CORRECTIVE_ACTIONS" && (priorProofs.Count(p => p.Command.Review.Stage == "INC_CORRECTIVE_ACTIONS") != 1 ||
                priorProofs.Single(p => p.Command.Review.Stage == "INC_CORRECTIVE_ACTIONS").Command.Review.Evidence.Actions.Revision != evidence.Actions.Revision ||
                JsonSerializer.Serialize(priorProofs.Single(p => p.Command.Review.Stage == "INC_CORRECTIVE_ACTIONS").Command.Review.Evidence.Actions.Actions) != JsonSerializer.Serialize(evidence.Actions.Actions)))
                throw new InvalidOperationException("The verification actions differ from their recorded corrective-action handoff.");
            IncidentRcaRecordedCause? recordedCause = null;
            if (ordered.Any(t => t.StepId == "INC_RCA"))
            {
                if (!process.ProcessData.Data.TryGetValue(RcaHandoffKey, out var rcaValue)) throw new InvalidOperationException("Recorded RCA handoff evidence is required before corrective-action closure.");
                var rca = JsonSerializer.Deserialize<HseRcaHandoffEvidence>(JsonSerializer.Serialize(rcaValue)) ?? throw new InvalidOperationException("Recorded RCA handoff evidence is invalid."); rca.Command.Require();
                var r = rca.Command.Review; var recordedStep = rca.Snapshot.StepInstances.Single(s => s.StepInstanceId == r.StepInstanceId);
                if (r.Context.Company != context.Company || r.Context.Database != context.Database || r.Context.FieldId != context.FieldId || r.LifecycleDatabase != lifecycle || r.InstanceId != process.InstanceId || r.Cause.IncidentId != evidence.Actions.IncidentId ||
                    HseRcaBusinessDigest(rca.Snapshot) != rca.BusinessDigest || recordedStep.StepId != "INC_RCA" || recordedStep.Status != StepStatus.COMPLETED || recordedStep.CompletedBy != r.Context.ActorUserId || recordedStep.Outcome != "RCA_RECORDED" ||
                    JsonSerializer.Serialize(process.StepInstances.Single(s => s.StepInstanceId == r.StepInstanceId)) != JsonSerializer.Serialize(recordedStep)) throw new InvalidOperationException("The recorded RCA handoff step or ownership changed.");
                var history = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", rca.History.PROCESS_HISTORY_ID))).ToArray();
                if (history.Length != 1 || JsonSerializer.Serialize(history[0]) != JsonSerializer.Serialize(rca.History) || rca.History.EVENT_TYPE != "RCA_HANDED_OFF" || rca.History.USER_ID != r.Context.ActorUserId || rca.History.PROCESS_STEP_INSTANCE_ID != r.StepInstanceId || rca.Closed.Count == 0)
                    throw new InvalidOperationException("The recorded RCA audit evidence changed.");
                await router.ConfirmRecordedTaskSetAsync(rca.Closed, _connectionName); recordedCause = r.Cause;
            }
            if (stage == "INC_CLOSURE" && (priorProofs.Count(p => p.Command.Review.Stage == "INC_VERIFICATION") != 1 ||
                JsonSerializer.Serialize(priorProofs.Single(p => p.Command.Review.Stage == "INC_VERIFICATION").Command.Review.Evidence.Actions.Actions) != JsonSerializer.Serialize(evidence.Actions.Actions) ||
                priorProofs.Single(p => p.Command.Review.Stage == "INC_VERIFICATION").Command.Review.Evidence.Actions.Revision != evidence.Actions.Revision ||
                JsonSerializer.Serialize(priorProofs.Single(p => p.Command.Review.Stage == "INC_VERIFICATION").Command.Review.Evidence.Decisions) != JsonSerializer.Serialize(evidence.Decisions)))
                throw new InvalidOperationException("Closure requires the same recorded independent verification approved by this workflow.");
            await requireRoleAuthority(role);
            var tasks = await router.GetAllTasksForDevelopmentEvidenceAsync(process.InstanceId, _connectionName);
            var current = tasks.Where(t => t.PROCESS_STEP_INSTANCE_ID == step.StepInstanceId && CompanyAccountingCorrectionCycle.TaskRound(t.TASK_CONTEXT_JSON) == round?.IntentId.ToString("N")).ToArray();
            var personas = await _personaReader.GetActivePersonasAsync(role);
            var nextPersonas = nextName is null ? Array.Empty<string>() : (await _personaReader.GetActivePersonasAsync(process.StepInstances.Single(s => s.StepId == (correction ? "INC_CORRECTIVE_ACTIONS" : nextName)).RequiredRole!)).ToArray();
            if (current.Length == 0 || current.Any(t => t.ACTIVE_IND != "Y" || t.TASK_STATUS is not ("PENDING" or "IN_PROGRESS") || t.ASSIGNED_ROLE != role || t.ENTITY_TYPE != CompanyHseWorkflowStart.EntityType || t.ENTITY_ID != evidence.Actions.IncidentId) ||
                personas.Count == 0 || current.Select(t => t.TARGET_PERSONA_CODE).Distinct(StringComparer.Ordinal).Count() != current.Length ||
                !current.Select(t => t.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(personas.Order(StringComparer.Ordinal)) ||
                nextName is not null && (nextPersonas.Length == 0 || nextPersonas.Distinct(StringComparer.Ordinal).Count() != nextPersonas.Length || nextPersonas.Any(p => !LeaseSummaryValidator.Identifier(p))))
                throw new InvalidOperationException("The current and next persona task assignments are unavailable.");
            var priorApprovalIds = HseClosureProofs(process).Where(p => p.Command.Review.CorrectionRound != round?.IntentId && p.Approval is not null).Select(p => p.Approval!.PROCESS_APPROVAL_ID).Concat(HseCorrectionProofs(process).Select(p => p.Approval.PROCESS_APPROVAL_ID)).ToHashSet(StringComparer.Ordinal);
            if ((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID", step.StepInstanceId))).Any(a => !priorApprovalIds.Contains(((PROCESS_APPROVAL)a).PROCESS_APPROVAL_ID))) throw new InvalidOperationException("The current incident stage already has approval evidence.");
            await requireRoleAuthority(role);
            var graph = ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Definition = definition, Process = process, Tasks = tasks.OrderBy(t => t.CROSS_TASK_ID, StringComparer.Ordinal), NextPersonas = nextPersonas.Order(StringComparer.Ordinal) }));
            var result = new CompanyHseClosureReview(evidence, lifecycle, process.InstanceId, step.StepInstanceId, stage, role, graph, nativeRevision) { RecordedCause = recordedCause, CorrectionRound = round?.IntentId }; if (correction) result.RequireCorrectionReview(); else result.Require();
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The lifecycle datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            return result;
        }
        var first = await Capture(); var second = await Capture();
        if (JsonSerializer.Serialize(first) != JsonSerializer.Serialize(second)) throw new InvalidOperationException("The incident closure graph changed during review.");
        return second;
    }

    public async Task<CompanyHseClosureReceipt> CompleteClosureStageAsync(CompanyHseClosureCommand original, Func<string, Task> requireRoleAuthority, Func<Task> requireRecordedSource)
        => await CompleteClosureStageCoreAsync(original, requireRoleAuthority, requireRecordedSource, false);

    private async Task<CompanyHseClosureReceipt> CompleteClosureStageCoreAsync(CompanyHseClosureCommand original, Func<string, Task> requireRoleAuthority, Func<Task> requireRecordedSource, bool readOnly)
    {
        original.Require(); var frozen = JsonSerializer.Deserialize<CompanyHseClosureCommand>(JsonSerializer.Serialize(original))!; var review = frozen.Review; var actor = review.Context.ActorUserId;
        if (_personaReader is null || review.LifecycleDatabase.ConnectionName != _connectionName) throw new InvalidOperationException("The fixed lifecycle owner is required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The lifecycle datasource is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName), personas: _personaReader);
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The lifecycle datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await requireRoleAuthority(review.AssignedRole);
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task Confirm(HseClosureProof proof)
        {
            if (JsonSerializer.Serialize(proof.Command) != JsonSerializer.Serialize(frozen) || HseClosureDigest(proof.Snapshot) != proof.Digest)
                throw new InvalidOperationException("The recorded incident closure original or snapshot changed.");
            var process = await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The recorded incident workflow is unavailable.");
            var start = CompanyHseWorkflowStart.FromPayload(JsonSerializer.Serialize(process.ProcessData), process.InstanceId, process.FieldId, review.Evidence.Actions.IncidentId);
            var snapshotStep = proof.Snapshot.StepInstances.Single(s => s.StepInstanceId == review.StepInstanceId);
            var step = process.StepInstances.Single(s => s.StepInstanceId == review.StepInstanceId);
            var currentRound = CompanyHseCorrectionRound.FromPayload(JsonSerializer.Serialize(process.ProcessData), process.InstanceId, process.FieldId, review.Evidence.Actions.IncidentId);
            if (currentRound?.IntentId != review.CorrectionRound) throw new InvalidOperationException("This incident approval belongs to a superseded correction round.");
            if (process.ProcessId != CompanyHseWorkflowStart.ProcessId || process.EntityType != CompanyHseWorkflowStart.EntityType || process.EntityId != review.Evidence.Actions.IncidentId ||
                proof.Snapshot.ProcessId != process.ProcessId || proof.Snapshot.EntityType != process.EntityType || proof.Snapshot.EntityId != process.EntityId || proof.Snapshot.FieldId != process.FieldId ||
                start.Context.Company != review.Context.Company || start.Context.Database != review.Context.Database || start.Context.FieldId != review.Context.FieldId || start.LifecycleDatabase != review.LifecycleDatabase ||
                step.StepId != review.Stage || step.Status != StepStatus.COMPLETED || step.CompletedBy != actor || step.Outcome != "HSE_REVIEWED" || JsonSerializer.Serialize(step) != JsonSerializer.Serialize(snapshotStep) ||
                proof.History.PROCESS_INSTANCE_ID != review.InstanceId || proof.History.PROCESS_STEP_INSTANCE_ID != review.StepInstanceId || proof.History.EVENT_TYPE != "HSE_REVIEWED" || proof.History.USER_ID != actor || proof.History.DETAILS != frozen.Reason ||
                proof.Closed.Count == 0 || proof.Closed.Any(t => t.TASK_STATUS != "COMPLETED" || t.ASSIGNED_ROLE != review.AssignedRole || t.ROW_CHANGED_BY != actor))
                throw new InvalidOperationException("The incident closure graph and history are unconfirmed.");
            if (review.Stage == "INC_CLOSURE" && (process.Status != ProcessStatus.COMPLETED || process.CompletionDate != proof.Snapshot.CompletionDate || process.CompletedBy != actor || HseClosureDigest(process) != proof.Digest || proof.Next.Count != 0 || proof.NextStep is not null))
                throw new InvalidOperationException("The terminal incident lifecycle is unconfirmed.");
            await router.ConfirmRecordedTaskSetAsync(proof.Closed, _connectionName);
            if (proof.Next.Count > 0) await router.ConfirmProgressedTaskSetAsync(proof.Next, _connectionName);
            var history = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", proof.History.PROCESS_HISTORY_ID))).ToArray();
            if (history.Length != 1 || JsonSerializer.Serialize(history[0]) != JsonSerializer.Serialize(proof.History)) throw new InvalidOperationException("The incident history read-back is unconfirmed.");
            var allApprovals = (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID", review.StepInstanceId))).Cast<PROCESS_APPROVAL>().ToArray();
            var historicalApprovalIds = HseClosureProofs(process).Where(p => p.Command.Review.CorrectionRound != review.CorrectionRound && p.Approval is not null).Select(p => p.Approval!.PROCESS_APPROVAL_ID).Concat(HseCorrectionProofs(process).Select(p => p.Approval.PROCESS_APPROVAL_ID)).ToHashSet(StringComparer.Ordinal);
            var approvals = allApprovals.Where(a => !historicalApprovalIds.Contains(a.PROCESS_APPROVAL_ID)).ToArray();
            if (proof.Approval is null ? approvals.Length != 0 : approvals.Length != 1 || JsonSerializer.Serialize(approvals[0]) != JsonSerializer.Serialize(proof.Approval))
                throw new InvalidOperationException("The incident approval read-back is unconfirmed.");
            if (proof.Approval is { } approval && (approval.PROCESS_INSTANCE_ID != review.InstanceId || approval.APPROVAL_STATUS != "APPROVED" || approval.APPROVED_BY != actor || approval.APPROVER_USER_ID != actor || approval.APPROVAL_NOTES != frozen.Reason || approval.REQUIRED_ACTION != "INDEPENDENT_HSE_VERIFICATION" || approval.ROW_CREATED_BY != actor || approval.APPROVAL_DATE != step.CompletionDate))
                throw new InvalidOperationException("The independent incident approval is unconfirmed.");
        }
        static void CopyBusiness<T>(T target, T converted) where T : ModelEntityBase
        { foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) if (p.CanWrite) p.SetValue(target, p.GetValue(converted)); }
        async Task Update(ModelEntityBase row, PPDMGenericRepository repository, string key, string id, string? creator, DateTime? created)
        {
            await repository.UpdateAsync(row, actor); var rows = (await repository.GetAsync(Key(key, id))).ToArray();
            if (row.ROW_CREATED_BY != creator || row.ROW_CREATED_DATE != created || rows.Length != 1 || JsonSerializer.Serialize(rows[0]) != JsonSerializer.Serialize((object)row)) throw new InvalidOperationException("The incident lifecycle update is unconfirmed.");
        }
        HseClosureProof? proof = null; await Guard(false); await requireRecordedSource(); await Guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var process = await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The incident workflow is unavailable.");
            var proofs = HseClosureProofs(process);
            if (proofs.Count > 3 * (CompanyHseCorrectionRound.MaximumRounds + 1) || proofs.Select(p => p.Command.IntentId).Distinct().Count() != proofs.Count || proofs.Select(p => (p.Command.Review.Stage, p.Command.Review.CorrectionRound)).Distinct().Count() != proofs.Count)
                throw new InvalidOperationException("The incident closure proof population is ambiguous.");
            proof = proofs.SingleOrDefault(p => p.Command.IntentId == frozen.IntentId);
            if (proof is not null) { await Confirm(proof); await Guard(true); return; }
            if (readOnly) throw new InvalidOperationException("No recorded closure matches the original request.");
            if (proofs.Any(p => p.Command.Review.Stage == review.Stage && p.Command.Review.CorrectionRound == review.CorrectionRound)) throw new InvalidOperationException("This stage already has another recorded original.");
            var currentReview = await ReviewClosureStageCoreAsync(review.Evidence, review.LifecycleDatabase, review.NativeRevision, requireRoleAuthority, true);
            if (JsonSerializer.Serialize(currentReview) != JsonSerializer.Serialize(review)) throw new InvalidOperationException("The reviewed incident stage changed.");
            foreach (var returned in HseCorrectionProofs(process)) await ConfirmCorrectionHistoryAsync(returned, router);
            foreach (var prior in proofs)
            {
                // Revalidate each prior approval/history/task proof through its original command, without nesting a transaction.
                var priorHistory = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", prior.History.PROCESS_HISTORY_ID))).ToArray();
                var priorApprovals = prior.Approval is null ? [] : (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID", prior.Approval.PROCESS_APPROVAL_ID))).ToArray();
                if (HseClosureDigest(prior.Snapshot) != prior.Digest || priorHistory.Length != 1 || JsonSerializer.Serialize(priorHistory[0]) != JsonSerializer.Serialize(prior.History) ||
                    prior.Approval is not null && (priorApprovals.Length != 1 || JsonSerializer.Serialize(priorApprovals[0]) != JsonSerializer.Serialize(prior.Approval))) throw new InvalidOperationException("Prior incident approval evidence changed.");
                await router.ConfirmRecordedTaskSetAsync(prior.Closed, _connectionName);
            }
            var step = process.StepInstances.Single(s => s.StepInstanceId == review.StepInstanceId);
            var nextName = review.Stage == "INC_CORRECTIVE_ACTIONS" ? "INC_VERIFICATION" : review.Stage == "INC_VERIFICATION" ? "INC_CLOSURE" : null;
            var next = nextName is null ? null : process.StepInstances.Single(s => s.StepId == nextName);
            var closed = await router.CompleteTaskSetInOwnedTransactionAsync(review.InstanceId, review.StepInstanceId, review.AssignedRole, CompanyHseWorkflowStart.EntityType, review.Evidence.Actions.IncidentId, actor, _connectionName);
            var now = DateTime.UtcNow; step.Status = StepStatus.COMPLETED; step.StartDate ??= now; step.CompletionDate = now; step.CompletedBy = actor; step.Outcome = "HSE_REVIEWED";
            if (next is not null) { next.Status = StepStatus.PENDING; process.CurrentStepId = next.StepId; process.CurrentState = next.StepId; }
            else { process.Status = ProcessStatus.COMPLETED; process.CompletionDate = now; process.CompletedBy = actor; process.CurrentState = "COMPLETED"; }
            process.ProcessData.LastUpdated = now;
            var id = "hse-close-" + frozen.IntentId.ToString("N");
            PROCESS_APPROVAL? approval = null;
            if (review.Stage != "INC_CORRECTIVE_ACTIONS")
            {
                approval = new() { PROCESS_APPROVAL_ID = id, PROCESS_INSTANCE_ID = review.InstanceId, PROCESS_STEP_INSTANCE_ID = review.StepInstanceId, APPROVAL_TYPE = review.Stage,
                    APPROVAL_STATUS = "APPROVED", APPROVER_USER_ID = actor, APPROVER_SEQUENCE = 1, REQUIRED_ACTION = "INDEPENDENT_HSE_VERIFICATION", APPROVAL_DATE = now, APPROVED_BY = actor, APPROVAL_NOTES = frozen.Reason,
                    ACTIVE_IND = "Y", ROW_CREATED_BY = actor, ROW_CREATED_DATE = now };
                if ((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID", id))).Any()) throw new InvalidOperationException("The approval identity already exists.");
                await GetProcessApprovalRepository().InsertAsync(approval, actor);
            }
            var history = new ProcessHistoryEntry { HistoryId = id, InstanceId = review.InstanceId, StepInstanceId = review.StepInstanceId, Action = "HSE_REVIEWED", PreviousState = review.Stage, NewState = nextName ?? "COMPLETED", Timestamp = now, PerformedBy = actor, Notes = frozen.Reason };
            var historyRow = ConvertToProcessHistoryEntity(history); historyRow.ROW_CREATED_BY = actor; historyRow.ROW_CREATED_DATE = now;
            if ((await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", id))).Any()) throw new InvalidOperationException("The history identity already exists.");
            await GetProcessHistoryRepository().InsertAsync(historyRow, actor); process.History.Add(history);
            foreach (var changed in next is null ? new[] { step } : new[] { step, next })
            {
                var row = (PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID", changed.StepInstanceId))).Single(); var creator = row.ROW_CREATED_BY; var created = row.ROW_CREATED_DATE;
                CopyBusiness(row, ConvertToProcessStepInstanceEntity(changed)); row.ROW_CHANGED_BY = actor; row.ROW_CHANGED_DATE = now;
                await Update(row, GetProcessStepInstanceRepository(), "PROCESS_STEP_INSTANCE_ID", changed.StepInstanceId, creator, created);
            }
            var processRow = (PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", review.InstanceId))).Single(); var owner = processRow.ROW_CREATED_BY; var at = processRow.ROW_CREATED_DATE;
            CopyBusiness(processRow, ConvertToProcessInstanceEntity(process)); processRow.ROW_CHANGED_BY = actor; processRow.ROW_CHANGED_DATE = now;
            await Update(processRow, GetProcessInstanceRepository(), "PROCESS_INSTANCE_ID", review.InstanceId, owner, at);
            var routed = next is null ? new List<CROSS_PERSONA_TASK>() : await router.RouteTaskInOwnedTransactionAsync(review.InstanceId, next.StepInstanceId, next.RequiredRole!, CompanyHseWorkflowStart.EntityType, review.Evidence.Actions.IncidentId, null, actor);
            var snapshot = JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(process))!;
            snapshot.ProcessData.Data.Remove(HseClosureKey); snapshot.ProcessData.Data.Remove(HseCorrectionKey);
            proof = new(frozen, snapshot, HseClosureDigest(snapshot), historyRow, approval, closed, routed, next?.StepInstanceId); proofs.Add(proof);
            process.ProcessData.Data[HseClosureKey] = proofs;
            if (JsonSerializer.Serialize(process.ProcessData).Length > 262144) throw new InvalidOperationException("The incident closure evidence exceeds its bounded lifecycle storage.");
            CopyBusiness(processRow, ConvertToProcessInstanceEntity(process)); processRow.ROW_CHANGED_BY = actor; processRow.ROW_CHANGED_DATE = now;
            await Update(processRow, GetProcessInstanceRepository(), "PROCESS_INSTANCE_ID", review.InstanceId, owner, at);
            await Confirm(proof); await Guard(true);
        });
        if (proof is null) throw new InvalidOperationException("The incident lifecycle result is unconfirmed.");
        await Confirm(proof); await Guard(false); await requireRecordedSource(); await Guard(false); await Confirm(proof);
        var receipt = new CompanyHseClosureReceipt(frozen.IntentId, review.Context.Company, actor, review.InstanceId, review.StepInstanceId, review.Stage,
            proof.History.PROCESS_HISTORY_ID, proof.Approval?.PROCESS_APPROVAL_ID, proof.NextStep, proof.Closed.Select(t => t.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(), proof.Next.Select(t => t.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(), proof.Digest);
        receipt.Require(frozen); return receipt;
    }

    public async Task<CompanyHseClosureRecovery?> RecoverClosureStageAsync(FieldSelectionReview context, AssetDatabaseScope lifecycle, string incidentId, Func<string, Task> requireRoleAuthority, Func<CompanyHseClosureCommand, Task> requireRecordedSource)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        var candidates = await GetProcessInstancesForEntityAsync(incidentId, CompanyHseWorkflowStart.EntityType) ?? throw new InvalidOperationException("The workflow population is unavailable.");
        var originals = new List<CompanyHseClosureCommand>(); var owned = 0;
        foreach (var process in candidates)
        {
            var start = CompanyHseWorkflowStart.FromPayload(JsonSerializer.Serialize(process.ProcessData), process.InstanceId, process.FieldId, incidentId);
            if (start.Context.Company != context.Company || start.Context.FieldId != context.FieldId) continue;
            if (++owned != 1 || process.ProcessId != CompanyHseWorkflowStart.ProcessId || process.EntityType != CompanyHseWorkflowStart.EntityType || process.EntityId != incidentId) throw new InvalidOperationException("The recorded incident closure population is ambiguous or retagged.");
            if (start.Context.Database != context.Database || start.LifecycleDatabase != lifecycle) throw new InvalidOperationException("The lifecycle binding changed.");
            var proofs = HseClosureProofs(process);
            if (proofs.Count > 3 * (CompanyHseCorrectionRound.MaximumRounds + 1)) throw new InvalidOperationException("The closure history is ambiguous.");
            var round = CompanyHseCorrectionRound.FromPayload(JsonSerializer.Serialize(process.ProcessData), process.InstanceId, process.FieldId, incidentId);
            originals.AddRange(proofs.Where(p => p.Command.Review.CorrectionRound == round?.IntentId && p.Command.Review.Context.ActorUserId == context.ActorUserId).Select(p => p.Command));
        }
        if (originals.Count == 0) return null;
        var original = originals.OrderBy(c => c.Review.Stage == "INC_CLOSURE" ? 3 : c.Review.Stage == "INC_VERIFICATION" ? 2 : 1).Last(); original.Require();
        var r = original.Review;
        if (r.Context.Company != context.Company || r.Context.Database != context.Database || r.Context.FieldId != context.FieldId || r.LifecycleDatabase != lifecycle || r.Evidence.Actions.IncidentId != incidentId) throw new InvalidOperationException("The retained original belongs to another scope.");
        var receipt = await CompleteClosureStageCoreAsync(original, requireRoleAuthority, () => requireRecordedSource(original), true);
        return new(original, receipt);
    }
}
