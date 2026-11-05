using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingCompletionReviewReadiness> ReadCompletionRoutingAsync(ClaimsPrincipal user, string reviewId)
    {
        var access = await CaptureExecutionReadAsync(user, reviewId); var scope = access.Basis.Scope();
        await RequireReportViewAsync(user, scope);
        var sender = await CaptureExecutionRecorderAsync(user, scope, reporting: true);
        var selected = await company.RefreshCompanyAsync();
        var canPropose = sender.Roles.Length > 0 && selected.Permissions.Contains("Workflow.Start", StringComparer.Ordinal) &&
            (await resources.CheckAssetAccessAsync(scope.ActorUserId, scope.FieldId, "FIELD", "WRITE")).HasAccess;
        async Task Guard()
        {
            await access.Check(); await RequireReportViewAsync(user, scope); await sender.Check();
            if (canPropose && !(await company.RefreshCompanyAsync()).Permissions.Contains("Workflow.Start", StringComparer.Ordinal))
                throw new UnauthorizedAccessException("Current configured report sender, workflow-start and original FIELD WRITE access are required for private completion routing.");
        }
        async Task ConfirmResources()
        {
            await Guard();
            if(canPropose && !(await resources.CheckAssetAccessAsync(scope.ActorUserId,scope.FieldId,"FIELD","WRITE")).HasAccess)
                throw new UnauthorizedAccessException("Current original FIELD WRITE access is required for private completion routing.");
            await Guard();
        }
        async Task<DrillingDailyReportHistory> ReadSource()
        {
            await ConfirmResources();
            var execution = await native.ReadExecutionStatusAsync(access.Basis, Guard);
            var history = await native.ReadDailyReportsAsync(execution, Guard); history.Require();
            if (history.ReportedCompletion is null)
                throw new InvalidOperationException("Record and recover completion against the latest observed period before inspecting independent review routing.");
            if (canPropose)
            {
                var current = await native.ReadProgramWellAsync(scope.CoreDatabase, scope.FieldId, execution.Basis.Source.Basis.Well.Uwi, true, Guard);
                if (current != execution.Basis.Source.Basis.Well)
                    throw new InvalidOperationException("The original active completion-review well changed.");
            }
            await ConfirmResources();return history;
        }
        var source = await ReadSource();
        var owner = await ExecutionAssignmentOwner(source.Execution);
        async Task<DrillingCompletionReviewReadiness> Observe()
        {
            await ConfirmResources();
            var existing=await owner.FindRecordedCompletionReviewAsync(scope.Company,scope.FieldId,source.ReportedCompletion!.ReportId,Guard);
            var definitions = canPropose && existing is null ? CompanyInventorySourceReview.Copy(await owner.GetProcessDefinitionsByTypeAsync("DRILLING")
                ?? throw new InvalidOperationException("The complete configured completion-review catalog is unavailable.")) : new List<ProcessDefinition>();
            if (definitions.Count > 128 || definitions.Any(row => row is null || !row.IsActive || row.ProcessType != "DRILLING" || !LeaseSummaryValidator.Identifier(row.ProcessId)) ||
                definitions.Select(row => row.ProcessId).Distinct(StringComparer.Ordinal).Count() != definitions.Count)
                throw new InvalidOperationException("The complete configured drilling catalog is ambiguous or exceeds its bound.");
            var options = new List<DrillingCompletionReviewOption>();
            var authorities = new Dictionary<string, DevelopmentReviewRecipientStep>(StringComparer.Ordinal); var stageCount = 0;
            foreach (var definition in definitions.Where(row => row.EntityType == "DRILLING_DAILY_REPORT").OrderBy(row => row.ProcessId, StringComparer.Ordinal))
            {
                var configured = DevelopmentReviewDefinition.RequireCompletion(definition);
                stageCount += configured.Count;
                if (stageCount > 256) throw new InvalidOperationException("The complete completion-review catalog exceeds 256 configured stages.");
                var stages = new List<DrillingCompletionReviewStage>();
                foreach (var step in configured)
                {
                    await Guard(); var role = step.RequiredRoles[0];
                    if (!authorities.TryGetValue(role, out var authority))
                    {
                        authority = await recipients.ReadCompletionAsync(source, step.StepId, role); authorities.Add(role, authority);
                    }
                    var current = authority with { StepId = step.StepId };
                    stages.Add(new(new(step.StepId, step.StepName, step.SequenceNumber, role,
                        current.Recipients.Select(row => row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(), step.SlaHours), current));
                }
                var option = new DrillingCompletionReviewOption(definition.ProcessId, definition.ProcessName, definition.Version,
                    ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)), stages);
                option.Require(source); options.Add(option);
            }
            var result = new DrillingCompletionReviewReadiness(source, options, canPropose ? sender.Roles : [],
                canPropose ? sender.Persona : null, canPropose ? sender.Revision : null, "", DateTimeOffset.UtcNow){ExistingReviewInstanceId=existing};
            result = result with { Revision = result.ContentRevision() }; result.Require(scope.Company, scope.ActorUserId, reviewId);
            await ConfirmResources(); return result;
        }
        var opening = await Observe(); var closing = await Observe();
        if (opening.Revision != closing.Revision)
            throw new InvalidOperationException("Configured completion approval stages or independent recipients changed during routing inspection.");
        var after = await ReadSource();
        if (after.Revision != source.Revision)
            throw new InvalidOperationException("The original execution, current reported completion or complete history changed during routing inspection.");
        await access.Confirm(); await Guard(); return CompanyInventorySourceReview.Copy(closing);
    }
}
