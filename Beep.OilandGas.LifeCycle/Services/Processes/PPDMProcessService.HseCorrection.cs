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
    private sealed record HseCorrectionProof(CompanyHseCorrectionReceipt Receipt, ProcessInstance Snapshot, PROCESS_HISTORY History,
        PROCESS_APPROVAL Approval, List<CROSS_PERSONA_TASK> Closed, List<CROSS_PERSONA_TASK> Routed);
    private static List<HseCorrectionProof> HseCorrectionProofs(ProcessInstance process) => process.ProcessData.Data.TryGetValue(HseCorrectionKey, out var value)
        ? JsonSerializer.Deserialize<List<HseCorrectionProof>>(JsonSerializer.Serialize(value)) ?? throw new InvalidOperationException("The incident correction history is invalid.") : [];

    private static void RequireCorrectionProof(HseCorrectionProof proof)
    {
        proof.Receipt.Require(); var r = proof.Receipt.Original.Review; var round = proof.Receipt.Round; var actor = r.Context.ActorUserId;
        var recorded = CompanyHseCorrectionRound.FromPayload(JsonSerializer.Serialize(proof.Snapshot.ProcessData), r.InstanceId, r.Context.FieldId!, r.Evidence.Actions.IncidentId);
        if (recorded != round || HseClosureDigest(proof.Snapshot) != proof.Receipt.GraphRevision ||
            proof.History.PROCESS_HISTORY_ID != proof.Receipt.HistoryId || proof.History.PROCESS_INSTANCE_ID != r.InstanceId || proof.History.PROCESS_STEP_INSTANCE_ID != r.StepInstanceId ||
            proof.History.EVENT_TYPE != "HSE_RETURNED" || proof.History.USER_ID != actor || proof.History.DETAILS != proof.Receipt.Original.Reason ||
            proof.Approval.PROCESS_APPROVAL_ID != proof.Receipt.ApprovalId || proof.Approval.PROCESS_INSTANCE_ID != r.InstanceId || proof.Approval.PROCESS_STEP_INSTANCE_ID != r.StepInstanceId ||
            proof.Approval.APPROVAL_STATUS != "REJECTED" || proof.Approval.APPROVED_BY != actor || proof.Approval.APPROVER_USER_ID != actor || proof.Approval.ROW_CREATED_BY != actor ||
            proof.Approval.REQUIRED_ACTION != "HSE_CORRECTION_RETURN" || proof.Approval.APPROVAL_NOTES != proof.Receipt.Original.Reason || proof.Approval.APPROVAL_DATE != proof.History.EVENT_DATE ||
            proof.Snapshot.CurrentStepId != "INC_CORRECTIVE_ACTIONS" || proof.Snapshot.Status != ProcessStatus.IN_PROGRESS ||
            proof.Snapshot.StepInstances.Single(s => s.StepInstanceId == round.ActionStepId).Status != StepStatus.PENDING ||
            proof.Snapshot.StepInstances.Single(s => s.StepInstanceId == round.VerificationStepId).Status != StepStatus.BLOCKED ||
            proof.Snapshot.StepInstances.Single(s => s.StepInstanceId == round.ClosureStepId).Status != StepStatus.BLOCKED ||
            proof.Closed.Any(t => t.PROCESS_INSTANCE_ID != r.InstanceId || t.PROCESS_STEP_INSTANCE_ID != r.StepInstanceId || t.ASSIGNED_ROLE != r.AssignedRole || t.ROW_CHANGED_BY != actor || t.TASK_STATUS != "COMPLETED") ||
            proof.Routed.Any(t => t.PROCESS_INSTANCE_ID != r.InstanceId || t.PROCESS_STEP_INSTANCE_ID != round.ActionStepId || t.ROW_CREATED_BY != actor || t.TASK_STATUS != "PENDING" || CompanyAccountingCorrectionCycle.TaskRound(t.TASK_CONTEXT_JSON) != round.IntentId.ToString("N")))
            throw new InvalidOperationException("The original incident correction proof changed.");
    }

    private async Task ConfirmCorrectionHistoryAsync(HseCorrectionProof proof, CrossPersonaTaskRouter router)
    {
        RequireCorrectionProof(proof);
        var history = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", proof.Receipt.HistoryId))).ToArray();
        var approval = (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID", proof.Receipt.ApprovalId))).ToArray();
        if (history.Length != 1 || JsonSerializer.Serialize(history[0]) != JsonSerializer.Serialize(proof.History) || approval.Length != 1 || JsonSerializer.Serialize(approval[0]) != JsonSerializer.Serialize(proof.Approval))
            throw new InvalidOperationException("The recorded correction audit changed.");
        await router.ConfirmRecordedTaskSetAsync(proof.Closed, _connectionName); await router.ConfirmProgressedTaskSetAsync(proof.Routed, _connectionName);
    }

    public Task<CompanyHseClosureReview> ReviewCorrectionReturnAsync(IncidentClosureReview evidence, AssetDatabaseScope lifecycle, string nativeRevision, Func<string, Task> requireRoleAuthority)
        => ReviewClosureStageCoreAsync(evidence, lifecycle, nativeRevision, requireRoleAuthority, false, true);

    public async Task<CompanyHseCorrectionReceipt> ReturnForCorrectionAsync(CompanyHseCorrectionCommand original, Func<string, Task> requireRoleAuthority, Func<Task> requireRecordedSource, Func<Task> requireRecordedRejection)
    {
        original.Require(); var frozen = JsonSerializer.Deserialize<CompanyHseCorrectionCommand>(JsonSerializer.Serialize(original))!;
        var review = frozen.Review; var actor = review.Context.ActorUserId;
        if (_personaReader is null || review.LifecycleDatabase.ConnectionName != _connectionName) throw new InvalidOperationException("The fixed HSE lifecycle and persona owners are required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The lifecycle datasource is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName), personas: _personaReader);
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The lifecycle datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await requireRoleAuthority(review.AssignedRole);
        }
        static void Copy<T>(T row, T converted) where T : ModelEntityBase
        { foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) if (property.CanWrite) property.SetValue(row, property.GetValue(converted)); }
        async Task Update(ModelEntityBase row, PPDMGenericRepository repository, string key, string id)
        {
            var created = row.ROW_CREATED_DATE; var creator = row.ROW_CREATED_BY;
            await repository.UpdateAsync(row, actor);
            var saved = (await repository.GetAsync(Key(key, id))).ToArray();
            if (saved.Length != 1 || JsonSerializer.Serialize(saved[0]) != JsonSerializer.Serialize((object)row) || row.ROW_CREATED_DATE != created || row.ROW_CREATED_BY != creator)
                throw new InvalidOperationException("The correction return update is unconfirmed.");
        }
        async Task Confirm(HseCorrectionProof proof)
        {
            RequireCorrectionProof(proof);
            if (JsonSerializer.Serialize(proof.Receipt.Original) != JsonSerializer.Serialize(frozen) || HseClosureDigest(proof.Snapshot) != proof.Receipt.GraphRevision ||
                proof.History.PROCESS_INSTANCE_ID != review.InstanceId || proof.History.PROCESS_STEP_INSTANCE_ID != review.StepInstanceId || proof.History.EVENT_TYPE != "HSE_RETURNED" ||
                proof.History.USER_ID != actor || proof.History.DETAILS != frozen.Reason || proof.Approval.PROCESS_INSTANCE_ID != review.InstanceId ||
                proof.Approval.PROCESS_STEP_INSTANCE_ID != review.StepInstanceId || proof.Approval.APPROVAL_STATUS != "REJECTED" || proof.Approval.APPROVED_BY != actor || proof.Approval.APPROVAL_NOTES != frozen.Reason ||
                proof.Snapshot.CurrentStepId != "INC_CORRECTIVE_ACTIONS" || proof.Snapshot.Status != ProcessStatus.IN_PROGRESS ||
                proof.Closed.Any(t => t.ROW_CHANGED_BY != actor || t.TASK_STATUS != "COMPLETED") ||
                !proof.Closed.Select(t => t.CROSS_TASK_ID).Order(StringComparer.Ordinal).SequenceEqual(proof.Receipt.CompletedTasks) ||
                !proof.Routed.Select(t => t.CROSS_TASK_ID).Order(StringComparer.Ordinal).SequenceEqual(proof.Receipt.CorrectionTasks))
                throw new InvalidOperationException("The recorded correction return evidence is unconfirmed.");
            var process = await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The recorded incident is unavailable.");
            var start = CompanyHseWorkflowStart.FromPayload(JsonSerializer.Serialize(process.ProcessData), process.InstanceId, process.FieldId, review.Evidence.Actions.IncidentId);
            if (process.ProcessId != CompanyHseWorkflowStart.ProcessId || process.EntityType != CompanyHseWorkflowStart.EntityType || process.EntityId != review.Evidence.Actions.IncidentId || process.FieldId != review.Context.FieldId ||
                proof.Snapshot.InstanceId != process.InstanceId || proof.Snapshot.ProcessId != process.ProcessId || proof.Snapshot.EntityType != process.EntityType || proof.Snapshot.EntityId != process.EntityId || proof.Snapshot.FieldId != process.FieldId ||
                start.Context.Company != review.Context.Company || start.Context.Database != review.Context.Database || start.Context.FieldId != review.Context.FieldId || start.LifecycleDatabase != review.LifecycleDatabase)
                throw new InvalidOperationException("The correction return ownership changed.");
            var histories = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", proof.Receipt.HistoryId))).ToArray();
            var approvals = (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID", proof.Receipt.ApprovalId))).ToArray();
            if (histories.Length != 1 || JsonSerializer.Serialize(histories[0]) != JsonSerializer.Serialize(proof.History) || approvals.Length != 1 || JsonSerializer.Serialize(approvals[0]) != JsonSerializer.Serialize(proof.Approval))
                throw new InvalidOperationException("The recorded correction audit changed.");
            await router.ConfirmRecordedTaskSetAsync(proof.Closed, _connectionName); await router.ConfirmProgressedTaskSetAsync(proof.Routed, _connectionName);
        }
        HseCorrectionProof? result = null; await Guard(false); await requireRecordedRejection(); await Guard(false);
        var before = await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The incident workflow is unavailable.");
        var retained = HseCorrectionProofs(before).Where(p => p.Receipt.Original.IntentId == frozen.IntentId).ToArray();
        if (retained.Length > 1) throw new InvalidOperationException("The recorded correction return is ambiguous.");
        if (retained.Length == 1) await Confirm(retained[0]);
        else await requireRecordedSource();
        await Guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var process = await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The incident workflow is unavailable.");
            var returns = HseCorrectionProofs(process);
            if (returns.Count > CompanyHseCorrectionRound.MaximumRounds || returns.Select(p => p.Receipt.Original.IntentId).Distinct().Count() != returns.Count)
                throw new InvalidOperationException("The correction return history is ambiguous.");
            result = returns.SingleOrDefault(p => p.Receipt.Original.IntentId == frozen.IntentId);
            if (result is not null) { await Confirm(result); await Guard(true); return; }
            if (retained.Length != 0) throw new InvalidOperationException("The previously recorded return disappeared; source evidence cannot authorize a replacement.");
            if (returns.Count == CompanyHseCorrectionRound.MaximumRounds) throw new InvalidOperationException("The incident correction round limit requires escalation.");
            var current = await ReviewClosureStageCoreAsync(review.Evidence, review.LifecycleDatabase, review.NativeRevision, requireRoleAuthority, true, true);
            if (JsonSerializer.Serialize(current) != JsonSerializer.Serialize(review)) throw new InvalidOperationException("The reviewed rejection or lifecycle graph changed.");
            foreach (var previous in returns)
            {
                RequireCorrectionProof(previous);
                var audit = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", previous.Receipt.HistoryId))).ToArray();
                var approval = (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID", previous.Receipt.ApprovalId))).ToArray();
                if (audit.Length != 1 || JsonSerializer.Serialize(audit[0]) != JsonSerializer.Serialize(previous.History) || approval.Length != 1 || JsonSerializer.Serialize(approval[0]) != JsonSerializer.Serialize(previous.Approval)) throw new InvalidOperationException("Prior correction audit changed.");
                await router.ConfirmRecordedTaskSetAsync(previous.Closed, _connectionName);
            }
            var handoffs = HseClosureProofs(process);
            if (handoffs.Count > 3 * (CompanyHseCorrectionRound.MaximumRounds + 1) || handoffs.Select(p => p.Command.IntentId).Distinct().Count() != handoffs.Count || handoffs.Select(p => (p.Command.Review.Stage, p.Command.Review.CorrectionRound)).Distinct().Count() != handoffs.Count) throw new InvalidOperationException("The incident handoff history is ambiguous.");
            foreach (var handoff in handoffs)
            {
                var audit = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", handoff.History.PROCESS_HISTORY_ID))).ToArray();
                var approval = handoff.Approval is null ? [] : (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID", handoff.Approval.PROCESS_APPROVAL_ID))).ToArray();
                if (HseClosureDigest(handoff.Snapshot) != handoff.Digest || audit.Length != 1 || JsonSerializer.Serialize(audit[0]) != JsonSerializer.Serialize(handoff.History) || handoff.Approval is not null && (approval.Length != 1 || JsonSerializer.Serialize(approval[0]) != JsonSerializer.Serialize(handoff.Approval))) throw new InvalidOperationException("Prior incident handoff evidence changed.");
                await router.ConfirmRecordedTaskSetAsync(handoff.Closed, _connectionName);
            }
            var action = process.StepInstances.Single(s => s.StepId == "INC_CORRECTIVE_ACTIONS");
            var verification = process.StepInstances.Single(s => s.StepInstanceId == review.StepInstanceId);
            var closure = process.StepInstances.Single(s => s.StepId == "INC_CLOSURE");
            if (action.Status != StepStatus.COMPLETED || closure.Status != StepStatus.BLOCKED) throw new InvalidOperationException("The incident correction destination changed.");
            var destinations = await _personaReader.GetActivePersonasAsync(action.RequiredRole!);
            if (destinations.Count == 0) throw new InvalidOperationException("The corrective-action role has no active persona destination.");
            var closed = await router.CompleteTaskSetInOwnedTransactionAsync(review.InstanceId, review.StepInstanceId, review.AssignedRole, CompanyHseWorkflowStart.EntityType, review.Evidence.Actions.IncidentId, actor, _connectionName);
            var now = DateTime.UtcNow; var id = "hse-return-" + frozen.IntentId.ToString("N");
            var rejected = new PROCESS_APPROVAL { PROCESS_APPROVAL_ID = id, PROCESS_INSTANCE_ID = review.InstanceId, PROCESS_STEP_INSTANCE_ID = review.StepInstanceId,
                APPROVAL_TYPE = "INC_VERIFICATION", APPROVAL_STATUS = "REJECTED", APPROVER_USER_ID = actor, APPROVER_SEQUENCE = returns.Count + 1,
                REQUIRED_ACTION = "HSE_CORRECTION_RETURN", APPROVAL_DATE = now, APPROVED_BY = actor, APPROVAL_NOTES = frozen.Reason, ACTIVE_IND = "Y", ROW_CREATED_BY = actor, ROW_CREATED_DATE = now };
            var history = new ProcessHistoryEntry { HistoryId = id, InstanceId = review.InstanceId, StepInstanceId = review.StepInstanceId, Action = "HSE_RETURNED", PreviousState = "INC_VERIFICATION", NewState = "INC_CORRECTIVE_ACTIONS", Timestamp = now, PerformedBy = actor, Notes = frozen.Reason };
            var auditRow = ConvertToProcessHistoryEntity(history); auditRow.ROW_CREATED_BY = actor; auditRow.ROW_CREATED_DATE = now;
            if ((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID", id))).Any() || (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", id))).Any()) throw new InvalidOperationException("The correction return identity is already owned.");
            await Guard(true);
            await GetProcessApprovalRepository().InsertAsync(rejected, actor); await GetProcessHistoryRepository().InsertAsync(auditRow, actor); process.History.Add(history);
            foreach (var changed in new[] { action, verification, closure })
            {
                changed.Status = changed == action ? StepStatus.PENDING : StepStatus.BLOCKED; changed.StartDate = null; changed.CompletionDate = null; changed.CompletedBy = null; changed.Outcome = null;
                var row = (PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID", changed.StepInstanceId))).Single();
                Copy(row, ConvertToProcessStepInstanceEntity(changed)); row.ROW_CHANGED_BY = actor; row.ROW_CHANGED_DATE = now;
                await Update(row, GetProcessStepInstanceRepository(), "PROCESS_STEP_INSTANCE_ID", changed.StepInstanceId);
            }
            var round = new CompanyHseCorrectionRound(frozen.IntentId, returns.Count + 1, review.Context.Company, review.InstanceId, action.StepInstanceId, verification.StepInstanceId, closure.StepInstanceId, review.Evidence.Actions.Revision); round.Require();
            process.ProcessData.Data[CompanyHseCorrectionRound.PayloadKey] = round; process.ProcessData.LastUpdated = now; process.CurrentStepId = action.StepId; process.CurrentState = action.StepId;
            var processRow = (PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", review.InstanceId))).Single();
            Copy(processRow, ConvertToProcessInstanceEntity(process)); processRow.ROW_CHANGED_BY = actor; processRow.ROW_CHANGED_DATE = now;
            await Update(processRow, GetProcessInstanceRepository(), "PROCESS_INSTANCE_ID", review.InstanceId);
            var routed = await router.RouteTaskInOwnedTransactionAsync(review.InstanceId, action.StepInstanceId, action.RequiredRole!, CompanyHseWorkflowStart.EntityType, review.Evidence.Actions.IncidentId, null, actor);
            var snapshot = JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(process))!; snapshot.ProcessData.Data.Remove(HseClosureKey); snapshot.ProcessData.Data.Remove(HseCorrectionKey);
            var receipt = new CompanyHseCorrectionReceipt(frozen, round, id, id, closed.Select(t => t.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(), routed.Select(t => t.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(), HseClosureDigest(snapshot)); receipt.Require();
            result = new(receipt, snapshot, auditRow, rejected, closed, routed); returns.Add(result); process.ProcessData.Data[HseCorrectionKey] = returns;
            if (JsonSerializer.Serialize(process.ProcessData).Length > 262144) throw new InvalidOperationException("The incident correction evidence exceeds bounded lifecycle storage.");
            Copy(processRow, ConvertToProcessInstanceEntity(process)); processRow.ROW_CHANGED_BY = actor; processRow.ROW_CHANGED_DATE = now;
            await Update(processRow, GetProcessInstanceRepository(), "PROCESS_INSTANCE_ID", review.InstanceId); await Confirm(result); await Guard(true);
        });
        if (result is null) throw new InvalidOperationException("The correction return result is unconfirmed.");
        await Confirm(result); await Guard(false); await requireRecordedRejection();
        if (retained.Length == 0) await requireRecordedSource();
        await Guard(false); await Confirm(result); return result.Receipt;
    }

}
