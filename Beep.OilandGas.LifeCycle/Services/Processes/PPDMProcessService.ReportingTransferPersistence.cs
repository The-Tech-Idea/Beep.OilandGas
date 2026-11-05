using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private sealed class ReportingTransferPersonas(Func<DrillingExecutionAssignmentOption> workflow, Func<Task> guard) : IRolePersonaReader
    {
        public async Task<List<string>> GetActivePersonasAsync(string role)
        {
            await guard(); var current = workflow();
            if (role != current.Step.AssignedRole) throw new InvalidOperationException("The reporting generation role changed during routing.");
            return current.Step.PersonaCodes.Order(StringComparer.Ordinal).ToList();
        }
    }

    private async Task<string> ReportingDestinationDefinitionAsync(DrillingExecutionAssignmentOption destination, Func<Task> guard)
    {
        await guard();
        var rows = (await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", destination.ProcessId))).ToArray();
        if (rows.Length != 1 || rows[0] is not PROCESS_DEFINITION row || row.ACTIVE_IND != "Y" || row.IS_ACTIVE != "Y")
            throw new InvalidOperationException("One current active reporting destination definition is required.");
        var configured = ConvertToProcessDefinition(row); var step = DevelopmentReviewDefinition.RequireExecutionReporting(configured);
        if (ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured)) != destination.DefinitionRevision ||
            configured.ProcessId != destination.ProcessId || configured.ProcessName != destination.ProcessName || configured.Version != destination.Version ||
            step.StepId != destination.Step.StepId || step.StepName != destination.Step.StepName || step.SequenceNumber != destination.Step.Sequence ||
            step.RequiredRoles[0] != destination.Step.AssignedRole || step.SlaHours != destination.Step.SlaHours)
            throw new InvalidOperationException("The proposed reporting destination no longer matches its complete configured workflow/version.");
        await guard(); return JsonSerializer.Serialize(row);
    }

    /// <summary>
    /// The app must admit current management and complete receiving authority in every guard.
    /// Source/recipient callbacks run outside the owned transaction; recovery reads only recorded proof.
    /// </summary>
    public async Task<DrillingReportingReassignmentReceipt> RecordReportingReassignmentAsync(DrillingReportingReassignment input,
        Func<Task> authority, Func<Task<DrillingExecutionStatus>> readExecution,
        Func<Task<DrillingExecutionAssignmentOption>> readDestination, bool recover = false)
    {
        ArgumentNullException.ThrowIfNull(input); ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(readExecution); ArgumentNullException.ThrowIfNull(readDestination);
        input.Require(); var command = CompanyInventorySourceReview.Copy(input); var review = command.Review;
        var original = review.Responsibility.Root; var id = original.InstanceId(); var scope = command.Scope(); var actor = scope.ActorUserId;
        if (scope.LifecycleDatabase.ConnectionName != _connectionName) throw new InvalidOperationException("The original reporting transfer lifecycle target is required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The reporting transfer source is unavailable.");
        async Task Guard(bool owned)
        {
            void Target()
            {
                if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The reporting transfer target changed.");
                if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            }
            Target(); await authority(); Target();
        }
        async Task Evidence()
        {
            if (recover) return;
            await Guard(false);
            var execution = await readExecution() ?? throw new InvalidOperationException("Current transfer execution is unavailable.");
            original.RequireSource(execution,false); review.Execution.Basis.RequireSource(execution.Basis, true);
            var destination = await readDestination() ?? throw new InvalidOperationException("Current transfer receiving authority is unavailable."); destination.Require();
            if (JsonSerializer.Serialize(destination) != JsonSerializer.Serialize(review.Destination))
                throw new InvalidOperationException("The frozen reporting destination or receiving authority changed.");
            await Guard(false);
        }
        var routing = review.Responsibility.Workflow;
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName),
            personas: new ReportingTransferPersonas(() => routing, () => Guard(true)));
        await Guard(false); await Evidence(); string? graph = null; ReportingTransferProof? recorded = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var process = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The original reporting responsibility is unavailable.");
            if (DrillingExecutionAssignment.OwnerCompany(JsonSerializer.Serialize(process.ProcessData)) != scope.Company ||
                DrillingExecutionAssignment.FromPayload(JsonSerializer.Serialize(process.ProcessData), id, scope.FieldId, original.OperationId).Revision() != original.Revision())
                throw new UnauthorizedAccessException("The reporting transfer belongs to another original company or responsibility.");
            if ((await FindExecutionAssignmentAsync(scope.Company, scope.FieldId, original.OperationId, router))?.PROCESS_INSTANCE_ID != id)
                throw new InvalidOperationException("The complete reporting source population changed.");
            graph = await ConfirmExecutionAssignmentAsync(process, original, router);
            var proofs = ReportingTransfers(process);
            var matching = proofs.Where(row => row.Command.HistoryId() == command.HistoryId()).ToArray();
            if (matching.Length > 1) throw new InvalidOperationException("The original reporting transfer identity is ambiguous.");
            if (matching.Length == 1)
            {
                if (matching[0].Command.Revision() != command.Revision()) throw new InvalidOperationException("This reporting transfer identity belongs to a different original.");
                recorded = matching[0]; await Guard(true); return;
            }
            if ((await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", command.HistoryId()))).Any() ||
                (await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID", command.StepInstanceId()))).Any())
                throw new InvalidOperationException("Partial or retagged reporting transfer evidence cannot become absence.");
            await router.RequireAbsentReportingTransferTasksAsync(command, _connectionName);
            var current = ReportingGeneration(process, original);
            if (process.Status != ProcessStatus.IN_PROGRESS || ReportingDigest(current)!=ReportingDigest(review.Responsibility) || ProcessDefinitionRevision.FromJson(graph) != review.NativeRevision ||
                ReportingDigest(ReportingModel(process)) != review.ProcessRevision)
                throw new InvalidOperationException("The exact displayed pending reporting generation changed or was already transferred by another original.");
            if (recover) { await Guard(true); return; }
            if (review.Responsibility.Number >= 256) throw new InvalidOperationException("The reporting generation history exceeds its supported bound.");
            var definition = await ReportingDestinationDefinitionAsync(review.Destination, () => Guard(true));
            var before = await ReportingSnapshotAsync(process, router);
            await router.CompleteTaskSetInOwnedTransactionAsync(id, current.StepInstanceId, current.Workflow.Step.AssignedRole, "DRILLING_OPERATION", original.OperationId, actor, _connectionName, proofs.LastOrDefault()?.Command);
            await Guard(true);
            var at = DateTime.UtcNow;
            if (at < command.ReassignedAtUtc.UtcDateTime) throw new InvalidOperationException("Transfer recording UTC precedes the explicit actual UTC.");
            var former = process.StepInstances.Single(row => row.StepInstanceId == current.StepInstanceId);
            former.Status = StepStatus.COMPLETED; former.StartDate = at; former.CompletionDate = at; former.CompletedBy = actor; former.Outcome = "REPORTING_TRANSFERRED"; former.Notes = command.Reason;
            var formerRow = CompanyInventorySourceReview.Copy(before.Steps.Single(row => row.PROCESS_STEP_INSTANCE_ID == current.StepInstanceId));
            CopyBusiness(formerRow, ConvertToProcessStepInstanceEntity(former)); await GetProcessStepInstanceRepository().UpdateAsync(formerRow, actor);
            await Guard(true);
            var next = TransferNextStep(before, command); process.StepInstances.Add(next);
            var newRow = ConvertToProcessStepInstanceEntity(next); newRow.ROW_CREATED_BY = actor; newRow.ROW_CREATED_DATE = at;
            await GetProcessStepInstanceRepository().InsertAsync(newRow, actor);
            await Guard(true);
            recorded = new(command, at, before); proofs.Add(recorded); process.ProcessData.Data[DrillingReportingReassignment.PayloadKey] = proofs;
            process.CurrentStepId = next.StepId; process.CurrentState = "REPORTING_REASSIGNED"; process.ProcessData.LastUpdated = at;
            var row = CompanyInventorySourceReview.Copy(before.Row); CopyBusiness(row, ConvertToProcessInstanceEntity(process));
            if (row.INSTANCE_DATA_JSON?.Length > DrillingExecutionAssignment.MaximumBodyLength + 65536) throw new InvalidOperationException("The complete reporting transfer proof exceeds its supported bound.");
            await GetProcessInstanceRepository().UpdateAsync(row, actor);
            await Guard(true);
            routing = review.Destination;
            await router.RouteTaskInOwnedTransactionAsync(id, next.StepInstanceId, routing.Step.AssignedRole, "DRILLING_OPERATION", original.OperationId, original.Source.Basis.Source.Program.PROGRAM_NAME, actor, command);
            await Guard(true);
            process = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The transferred reporting responsibility is unavailable.");
            var after = await ReportingSnapshotAsync(process, router); RequireReportingTransferredBusiness(after, recorded);
            var history = ConvertToProcessHistoryEntity(new ProcessHistoryEntry { HistoryId = command.HistoryId(), InstanceId = id, StepInstanceId = current.StepInstanceId,
                Action = "REPORTING_TRANSFERRED", PreviousState = before.Model.CurrentState, NewState = "REPORTING_REASSIGNED", Timestamp = at, PerformedBy = actor, Notes = command.Reason });
            history.EVENT_DATA_JSON = TransferHistoryData(recorded, after); await GetProcessHistoryRepository().InsertAsync(history, actor);
            await Guard(true);
            if (await ReportingDestinationDefinitionAsync(review.Destination, () => Guard(true)) != definition)
                throw new InvalidOperationException("The reporting destination definition changed before transfer commit.");
            await Guard(true);
            process = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The transferred reporting responsibility disappeared.");
            graph = await ConfirmExecutionAssignmentAsync(process, original, router); await Guard(true);
        });
        await Guard(false);
        var closing = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The original reporting transfer commit is unconfirmed.");
        if (graph != await ConfirmExecutionAssignmentAsync(closing, original, router)) throw new InvalidOperationException("The reporting transfer changed around commit. Recover its retained original.");
        await Evidence(); await Guard(false);
        closing = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The reporting transfer disappeared during closing source proof.");
        if (graph != await ConfirmExecutionAssignmentAsync(closing, original, router) ||
            (await FindExecutionAssignmentAsync(scope.Company, scope.FieldId, original.OperationId, router))?.PROCESS_INSTANCE_ID != id)
            throw new InvalidOperationException("The reporting transfer or complete source population changed after closing evidence.");
        await Guard(false);
        var receipt = new DrillingReportingReassignmentReceipt(command.Revision(), id, recorded is not null, recorded?.Command.HistoryId(), recorded?.Command.ReassignedAtUtc,
            recorded is null ? null : review.Responsibility.Number + 1, recorded?.Command.StepInstanceId(), recorded is null ? [] : command.TaskIds(), ProcessDefinitionRevision.FromJson(graph!), DateTimeOffset.UtcNow);
        receipt.Require(command); return receipt;
    }
}
