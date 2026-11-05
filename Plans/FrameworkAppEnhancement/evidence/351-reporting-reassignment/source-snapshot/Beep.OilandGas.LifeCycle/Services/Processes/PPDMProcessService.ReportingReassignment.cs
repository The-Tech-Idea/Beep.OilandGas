using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    /// <summary>
    /// Checks a replacement for the original pending reporting generation. The app admits the
    /// management purpose and supplies fresh source/recipient authority; this method never transfers work.
    /// </summary>
    public async Task<DrillingReportingReassignmentReview> ReviewReportingReassignmentAsync(
        CompanyScope company, string assignmentId, DrillingExecutionAssignmentOption proposed,
        string managerPersona, string managerAuthorityRevision, Func<Task> authority,
        Func<Task<DrillingExecutionStatus>> readExecution, Func<Task<DrillingExecutionAssignmentOption>> readDestination)
    {
        ArgumentNullException.ThrowIfNull(proposed); ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(readExecution); ArgumentNullException.ThrowIfNull(readDestination);
        proposed.Require(); var destination = CompanyInventorySourceReview.Copy(proposed);
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The reporting reassignment source is unavailable.");
        async Task Guard()
        {
            void Target()
            {
                if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The reporting reassignment target changed.");
                PPDMDataSourceTransaction.RequireIdle(source);
            }
            Target(); await authority(); Target();
        }
        async Task<string> Definition()
        {
            await Guard();
            var rows = (await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", destination.ProcessId))).ToArray();
            if (rows.Length != 1 || rows[0] is not PROCESS_DEFINITION row || row.ACTIVE_IND != "Y" || row.IS_ACTIVE != "Y")
                throw new InvalidOperationException("One current active reporting destination definition is required.");
            var configured = ConvertToProcessDefinition(row); var step = DevelopmentReviewDefinition.RequireExecutionReporting(configured);
            if (ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured)) != destination.DefinitionRevision ||
                configured.ProcessId != destination.ProcessId || configured.ProcessName != destination.ProcessName || configured.Version != destination.Version ||
                step.StepId != destination.Step.StepId || step.StepName != destination.Step.StepName || step.SequenceNumber != destination.Step.Sequence ||
                step.RequiredRoles[0] != destination.Step.AssignedRole || step.SlaHours != destination.Step.SlaHours)
                throw new InvalidOperationException("The proposed reporting destination no longer matches its complete configured workflow/version.");
            await Guard(); return JsonSerializer.Serialize(row);
        }
        async Task Destination()
        {
            await Guard(); var current = await readDestination() ?? throw new InvalidOperationException("Current reporting receiving authority is unavailable."); current.Require();
            if (JsonSerializer.Serialize(current) != JsonSerializer.Serialize(destination))
                throw new InvalidOperationException("The proposed reporting role, workspace or complete current receiving authority changed.");
            await Guard();
        }

        await Guard(); var first = await ReadRecordedExecutionAssignmentAsync(company, assignmentId, authority);
        if (first.Process.Status != ProcessStatus.IN_PROGRESS || first.Process.CurrentState != "INITIAL" ||
            first.Process.StepInstances.Count != 1 || first.Process.StepInstances[0].Status != StepStatus.PENDING || RecordedReportingClosure(first.Process) is not null)
            throw new InvalidOperationException("Only the qualified pending reporting generation may be reviewed for transfer.");
        if (DrillingReportingReassignmentReview.SameDestination(first.Original.Workflow, destination))
            throw new InvalidOperationException("Choose a changed reporting workflow, role, workspace or receiving set.");
        var definition = await Definition(); await Destination();
        var execution = await readExecution() ?? throw new InvalidOperationException("Current recorded execution is unavailable."); await Guard(); first.Original.RequireSource(execution);
        var review = new DrillingReportingReassignmentReview(first.Original, execution, 1, first.Original.Workflow,
            first.Process.StepInstances[0].StepInstanceId, first.Original.Revision(), first.Tasks.Select(task => task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(first.Process)), first.NativeRevision, destination,
            first.Original.SenderRole, managerPersona, managerAuthorityRevision, "", DateTimeOffset.UtcNow);
        review = review with { Revision = review.ContentRevision() }; review.Require();
        await Destination();
        var closing = await ReadRecordedExecutionAssignmentAsync(company, assignmentId, authority);
        if (closing.NativeRevision != first.NativeRevision || closing.Original.Revision() != first.Original.Revision() || await Definition() != definition)
            throw new InvalidOperationException("The complete pending reporting graph or destination definition changed during transfer review.");
        // External source callbacks stay outside lifecycle transactions; finish with the native graph.
        var currentExecution = await readExecution() ?? throw new InvalidOperationException("Closing recorded execution is unavailable."); await Guard(); first.Original.RequireSource(currentExecution);
        execution.Basis.RequireSource(currentExecution.Basis, true);
        closing = await ReadRecordedExecutionAssignmentAsync(company, assignmentId, authority);
        if (closing.NativeRevision != first.NativeRevision) throw new InvalidOperationException("The reporting generation changed during closing execution proof.");
        await Destination();
        if (await Definition() != definition) throw new InvalidOperationException("The reporting destination changed before review acknowledgement.");
        closing = await ReadRecordedExecutionAssignmentAsync(company, assignmentId, authority);
        if (closing.NativeRevision != first.NativeRevision) throw new InvalidOperationException("The reporting generation changed before review acknowledgement.");
        await Guard(); return CompanyInventorySourceReview.Copy(review);
    }
}
