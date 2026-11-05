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
        string managerRole, string managerPersona, string managerAuthorityRevision, Func<Task> authority,
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
        Task<string> Definition()=>ReportingDestinationDefinitionAsync(destination,Guard);
        async Task Destination()
        {
            await Guard(); var current = await readDestination() ?? throw new InvalidOperationException("Current reporting receiving authority is unavailable."); current.Require();
            if (JsonSerializer.Serialize(current) != JsonSerializer.Serialize(destination))
                throw new InvalidOperationException("The proposed reporting role, workspace or complete current receiving authority changed.");
            await Guard();
        }

        await Guard(); var first = await ReadRecordedExecutionAssignmentAsync(company, assignmentId, authority);
        var from = ReportingGeneration(first.Process,first.Original);
        if (first.Process.Status != ProcessStatus.IN_PROGRESS ||
            first.Process.StepInstances.Single(row=>row.StepInstanceId==from.Step).Status != StepStatus.PENDING || RecordedReportingClosure(first.Process) is not null)
            throw new InvalidOperationException("Only the qualified pending reporting generation may be reviewed for transfer.");
        if (DrillingReportingReassignmentReview.SameDestination(from.Workflow, destination))
            throw new InvalidOperationException("Choose a changed reporting workflow, role, workspace or receiving set.");
        var definition = await Definition(); await Destination();
        var execution = await readExecution() ?? throw new InvalidOperationException("Current recorded execution is unavailable."); await Guard(); first.Original.RequireSource(execution,false);
        var review = new DrillingReportingReassignmentReview(first.Original, execution, from.Number, from.Workflow,
            from.Step, from.Revision, first.Tasks.Where(task=>task.PROCESS_STEP_INSTANCE_ID==from.Step).Select(task => task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),
            ReportingDigest(ReportingModel(first.Process)), first.NativeRevision, destination,
            managerRole, managerPersona, managerAuthorityRevision, "", DateTimeOffset.UtcNow);
        review = review with { Revision = review.ContentRevision() }; review.Require();
        await Destination();
        var closing = await ReadRecordedExecutionAssignmentAsync(company, assignmentId, authority);
        if (closing.NativeRevision != first.NativeRevision || closing.Original.Revision() != first.Original.Revision() || await Definition() != definition)
            throw new InvalidOperationException("The complete pending reporting graph or destination definition changed during transfer review.");
        // External source callbacks stay outside lifecycle transactions; finish with the native graph.
        var currentExecution = await readExecution() ?? throw new InvalidOperationException("Closing recorded execution is unavailable."); await Guard(); first.Original.RequireSource(currentExecution,false);
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
