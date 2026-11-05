using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    private async Task<PPDMProcessService> ExecutionAssignmentOwner(DrillingExecutionStatus source)
    {
        var binding = source.Basis.Scope().LifecycleDatabase;
        if (processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(binding.ConnectionName) is not PPDMProcessService owner)
            throw new InvalidOperationException("The canonical original lifecycle owner is required for execution reporting assignment.");
        return owner;
    }

    public async Task<DrillingExecutionAssignmentReview> ReadExecutionAssignmentAsync(ClaimsPrincipal user, string reviewId)
    {
        var access = await CaptureExecutionReadAsync(user, reviewId); var scope = access.Basis.Scope();
        await RequireReportViewAsync(user, scope);
        var sender = await CaptureExecutionRecorderAsync(user, scope, reporting: true);
        var selected = await company.RefreshCompanyAsync();
        var rights = await grants.ReadAsync(scope.ActorUserId, scope.CoreDatabase.Fingerprint, scope.Company.BusinessAssociateId);
        var canPropose = sender.Roles.Length > 0 && selected.Permissions.Contains("Workflow.Start", StringComparer.Ordinal) && rights.Any(row =>
            row.UserId == scope.ActorUserId && row.DatabaseScope == scope.CoreDatabase.Fingerprint && row.OrganizationId == scope.Company.BusinessAssociateId &&
            row.AssetType == "FIELD" && row.AssetId == scope.FieldId && row.FacilityType is null && row.IsActive && row.AccessLevel is "WRITE" or "DELETE");
        async Task Guard()
        {
            await access.Check(); await RequireReportViewAsync(user, scope); await sender.Check();
            if (canPropose && !(await company.RefreshCompanyAsync()).Permissions.Contains("Workflow.Start", StringComparer.Ordinal))
                throw new UnauthorizedAccessException("Current workflow-start authority is required for private execution recipient review.");
        }
        var source = await native.ReadExecutionStatusAsync(access.Basis, Guard);
        if (source.Recorded is null) throw new InvalidOperationException("Record and recover the original execution start before assigning reporting responsibility.");
        if (canPropose)
        {
            var well = await native.ReadProgramWellAsync(scope.CoreDatabase, scope.FieldId, source.Basis.Source.Basis.Well.Uwi, true, Guard);
            if (well != source.Basis.Source.Basis.Well || !(await resources.CheckAssetAccessAsync(scope.ActorUserId, scope.FieldId, "FIELD", "WRITE")).HasAccess)
                throw new UnauthorizedAccessException("The original active well and current FIELD WRITE authority are required for execution assignment.");
        }
        var owner = await ExecutionAssignmentOwner(source);
        async Task<DrillingExecutionAssignmentReview> Observe()
        {
            await Guard();
            var existing = await owner.FindRecordedExecutionAssignmentAsync(scope.Company, scope.FieldId, source.Recorded.DRILLING_OPERATION_ID, Guard);
            var definitions = existing is null && canPropose ? CompanyInventorySourceReview.Copy(await owner.GetProcessDefinitionsByTypeAsync("DRILLING")
                ?? throw new InvalidOperationException("The complete configured execution reporting catalog is unavailable.")) : new List<ProcessDefinition>();
            if (definitions.Count > 128 || definitions.Any(row => row is null || !row.IsActive || row.ProcessType != "DRILLING" || !LeaseSummaryValidator.Identifier(row.ProcessId)) ||
                definitions.Select(row => row.ProcessId).Distinct(StringComparer.Ordinal).Count() != definitions.Count)
                throw new InvalidOperationException("The complete configured execution reporting catalog is ambiguous or exceeds its bound.");
            var options = new List<DrillingExecutionAssignmentOption>();
            foreach (var definition in definitions.Where(row => row.EntityType == "DRILLING_OPERATION").OrderBy(row => row.ProcessId, StringComparer.Ordinal))
            {
                var step = DevelopmentReviewDefinition.RequireExecutionReporting(definition);
                var authority = await recipients.ReadExecutionReportingAsync(source, step.StepId, step.RequiredRoles[0]);
                var option = new DrillingExecutionAssignmentOption(definition.ProcessId, definition.ProcessName, definition.Version,
                    ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),
                    new(step.StepId, step.StepName, step.SequenceNumber, step.RequiredRoles[0], authority.Recipients.Select(row => row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(), step.SlaHours), authority);
                option.Require(); options.Add(option);
            }
            var result = new DrillingExecutionAssignmentReview(source, options, canPropose ? sender.Roles : [],
                canPropose ? sender.Persona : null, canPropose ? sender.Revision : null, existing, "", DateTimeOffset.UtcNow);
            result = result with { Revision = result.ContentRevision() }; result.Require(scope.Company, scope.ActorUserId, reviewId); await Guard(); return result;
        }
        var first = await Observe(); var closing = await Observe();
        if (first.Revision != closing.Revision) throw new InvalidOperationException("The original reporting workflow, recipients or assignment changed during review.");
        var current = await native.ReadExecutionStatusAsync(access.Basis, Guard);
        if (source.Revision != current.Revision) throw new InvalidOperationException("The original recorded execution changed during assignment review.");
        await access.Confirm(); await Guard(); return CompanyInventorySourceReview.Copy(closing);
    }

    public async Task<DrillingExecutionAssignmentReceipt> AssignExecutionAsync(ClaimsPrincipal user, DrillingExecutionAssignment input, bool recover = false)
    {
        input.Require(); var original = CompanyInventorySourceReview.Copy(input); var scope = original.Scope();
        if (user.ActingUserId() != scope.ActorUserId || await account.RequireAsync() != scope.ActorUserId || (await company.RefreshCompanyAsync()).Scope != scope.Company)
            throw new UnauthorizedAccessException("Only the original execution assignment sender in the original company may record or recover this intent.");
        await RequireReportViewAsync(user, scope);
        var basis = original.Source.Basis.Source.Basis; var fundingSource = basis.Funding.Source;
        var access = await Capture(user, fundingSource.Current.Source.Review.InstanceId, fundingSource.AfeId, basis.Well.Uwi, !recover, recover, writePermission: "Drilling.Edit");
        var observed = await access.Read();
        if (observed.Scope() != scope || observed.ExecutionDatabase != basis.ExecutionDatabase)
            throw new UnauthorizedAccessException("The assignment belongs to another original account/company/FIELD/well/binding.");
        var sender = recover ? null : await CaptureExecutionRecorderAsync(user, scope, true, original.SenderRole, true);
        var bindings = await CaptureExecutionBindingsAsync(original.Source.Basis);
        async Task Guard()
        {
            await access.Check(); await bindings(); await RequireReportViewAsync(user, scope);
            if (sender is not null)
            {
                await sender.Check();
                if (sender.Persona != original.SenderPersonaCode || sender.Revision != original.SenderAuthorityRevision ||
                    !(await company.RefreshCompanyAsync()).Permissions.Contains("Workflow.Start", StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("The original assignment sender role/persona/authority changed.");
                var authority = await recipients.ReadExecutionReportingAsync(original.Source, original.Workflow.Step.StepId, original.Workflow.Step.AssignedRole);
                if (JsonSerializer.Serialize(authority) != JsonSerializer.Serialize(original.Workflow.Authority))
                    throw new InvalidOperationException("The frozen configured reporting recipient authority changed. Recover the original assignment before preparing another.");
            }
        }
        async Task<DrillingExecutionStatus> Read()
        {
            await Guard(); var currentBasis = await ReadExecutionBasisCoreAsync(user, original.Source.Basis.ReviewInstanceId, false);
            if (!recover)
            {
                var well = await native.ReadProgramWellAsync(scope.CoreDatabase, scope.FieldId, basis.Well.Uwi, true, Guard);
                if (well != basis.Well) throw new InvalidOperationException("The original active assignment well changed.");
            }
            var result = await native.ReadExecutionStatusAsync(currentBasis, Guard); original.RequireSource(result);
            await access.Confirm(); await Guard(); return result;
        }
        await Guard(); var owner = await ExecutionAssignmentOwner(original.Source);
        var receipt = await owner.RecordExecutionAssignmentAsync(original, Guard, Read, recover); receipt.Require(original);
        if (!recover && !receipt.Recorded) throw new InvalidOperationException("The original execution reporting assignment is unconfirmed. Recover its retained input.");
        await access.Confirm(); await Guard(); return receipt;
    }

    private static void RequireExecutionRecipient(ClaimsPrincipal user, ProgramReviewer reader, DrillingExecutionAssignment original)
    {
        var scope = original.Scope(); var role = original.Workflow.Step.AssignedRole;
        if (scope.Company != reader.Company || scope.CoreDatabase != reader.Core || scope.LifecycleDatabase != reader.Lifecycle ||
            !reader.Roles.Contains(role, StringComparer.Ordinal) || !user.IsInRole(role) ||
            !original.Workflow.Authority.Recipients.Any(row => row.UserId == reader.Actor && row.PersonaCode == reader.Persona) ||
            !reader.Grants.Any(row => row.UserId == reader.Actor && row.DatabaseScope == reader.Core.Fingerprint && row.OrganizationId == reader.Company.BusinessAssociateId &&
                row.AssetType == "FIELD" && row.AssetId == scope.FieldId && row.FacilityType is null && row.IsActive && row.AccessLevel is "READ" or "WRITE" or "DELETE"))
            throw new UnauthorizedAccessException("Only an original reporting recipient with the current configured role/workspace and original FIELD READ rights may open this responsibility.");
    }

    public async Task<DrillingExecutionAssignedWork> ReadAssignedExecutionAsync(ClaimsPrincipal user, string processId)
    {
        if (!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingExecutionAssignment.InstancePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException("Choose an exact assigned reporting responsibility from the company inbox.");
        var reader = await CaptureProgramReviewerAsync(user, reportingAssignment: true);
        var first = await reader.Native.ReadRecordedExecutionAssignmentAsync(reader.Company, processId, reader.Check);
        RequireExecutionRecipient(user, reader, first.Original);
        var access = await CaptureExecutionReadAsync(user, first.Original.Source.Basis.ReviewInstanceId);
        async Task Guard() { await reader.Check(); await access.Check(); await RequireReportViewAsync(user, access.Basis.Scope()); }
        var current = await native.ReadExecutionStatusAsync(access.Basis, Guard); first.Original.RequireSource(current, false);
        var closing = await reader.Native.ReadRecordedExecutionAssignmentAsync(reader.Company, processId, Guard);
        RequireExecutionRecipient(user, reader, closing.Original);
        if (closing.NativeRevision != first.NativeRevision || closing.Original.Revision() != first.Original.Revision())
            throw new InvalidOperationException("The original reporting responsibility or complete task graph changed during readback.");
        var matches = closing.Tasks.Where(task => task.TARGET_PERSONA_CODE == reader.Persona && task.ASSIGNED_ROLE == closing.Original.Workflow.Step.AssignedRole &&
            task.PROCESS_STEP_INSTANCE_ID == DrillingExecutionAssignmentReceipt.StepId(processId, closing.Original.Workflow.Step.StepId) && task.TASK_STATUS == "PENDING").ToArray();
        if (matches.Length != 1) throw new InvalidOperationException("The original individual reporting task is missing or ambiguous.");
        var task = matches[0]; var finalExecution = await native.ReadExecutionStatusAsync(access.Basis, Guard);
        if (finalExecution.Revision != current.Revision) throw new InvalidOperationException("The original assigned execution changed during readback.");
        var result = new DrillingExecutionAssignedWork(closing.Original, current, task.CROSS_TASK_ID, task.PROCESS_STEP_INSTANCE_ID, task.ASSIGNED_ROLE, reader.Persona,
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing.Process)), ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task)), closing.NativeRevision, "", DateTimeOffset.UtcNow);
        result = result with { Revision = result.ContentRevision() }; result.Require(reader.Company, reader.Actor, processId);
        await access.Confirm(); await Guard(); return CompanyInventorySourceReview.Copy(result);
    }
}
