using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Coordinates the reviewed incident source and the fixed company lifecycle owner.</summary>
public sealed partial class CompanyHseWorkflowService(CompanyModuleConnection company, ModuleConnectionResolver modules,
    ICompanyFieldSelectionService selections, ICompanyFieldSelectionAuthority preferences, IFieldOrchestrator field,
    ICompanyIncidentCommands incidents, IProcessService processes, ICompanyCorrectiveActionCommands corrective)
{
    private async Task<AssetDatabaseScope> RequireAuthorityAsync(ClaimsPrincipal user, FieldSelectionReview original)
    {
        var actor = user.ActingUserId();
        await preferences.RequireStoredSelectionAsync(original);
        var access = await company.RefreshCompanyAsync();
        if (user.Identity?.IsAuthenticated != true || actor != original.ActorUserId || access.UserId != actor || access.Scope != original.Company ||
            !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains(PermissionConstants.HSE.ManageIncidents, StringComparer.Ordinal) ||
            field.CurrentFieldId != original.FieldId || await company.ResolveAsync() != original.Database)
            throw new UnauthorizedAccessException("Current HSE company, account and FIELD authority is required.");
        var connection = await modules.ResolveAsync("LIFECYCLE");
        var lifecycle = new AssetDatabaseScope(connection, await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"], connection));
        await preferences.RequireStoredSelectionAsync(original);
        if (field.CurrentFieldId != original.FieldId) throw new UnauthorizedAccessException("The selected FIELD changed.");
        return lifecycle;
    }

    public async Task<CompanyHseWorkflowReview> ReviewAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("The incident identity is required.", nameof(incidentId));
        var actor = user.ActingUserId(); var access = await company.RefreshCompanyAsync();
        var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
        if (!selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId) throw new UnauthorizedAccessException("A current FIELD is required.");
        var context = selected.SelectionContext!;
        var lifecycle = await RequireAuthorityAsync(user, context);
        if (processes is not BoundProcessService bound) throw new InvalidOperationException("The fixed lifecycle process owner is required.");
        var process = await bound.CreateBoundServiceAsync(lifecycle.ConnectionName);
        var definition = await process.GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId) ?? throw new InvalidOperationException("The HSE workflow definition is unavailable.");
        if (!definition.IsActive || definition.ProcessId != CompanyHseWorkflowStart.ProcessId || definition.ProcessType != "HSE" ||
            definition.EntityType != CompanyHseWorkflowStart.EntityType || definition.Steps is not { Count: > 0 and <= 64 } ||
            definition.Steps.Any(step => step is null || !LeaseSummaryValidator.Identifier(step.StepId) || step.SequenceNumber < 0 || step.SlaHours < 0 ||
                step.RequiredRoles is not { Count: 1 } || !LeaseSummaryValidator.Identifier(step.RequiredRoles[0])) ||
            definition.Steps.Select(step => step.StepId).Distinct(StringComparer.Ordinal).Count() != definition.Steps.Count ||
            definition.Steps.Select(step => step.SequenceNumber).Distinct().Count() != definition.Steps.Count)
            throw new InvalidOperationException("A complete HSE workflow with explicit role assignments is required.");
        var definitionJson = JsonSerializer.Serialize(definition);
        var incidentRevision = await incidents.ReviewWorkflowSourceRevisionAsync(context, incidentId);
        if (await RequireAuthorityAsync(user, context) != lifecycle || JsonSerializer.Serialize(await process.GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId)) != definitionJson)
            throw new InvalidOperationException("The company, lifecycle binding or workflow definition changed during review.");
        var result = new CompanyHseWorkflowReview(context, lifecycle, incidentId, incidentRevision,
            ProcessDefinitionRevision.FromJson(definitionJson), definition.ProcessName, definition.Steps.OrderBy(step => step.SequenceNumber).First().RequiredRoles[0]);
        _ = result.CreateIntent(Guid.NewGuid());
        return result;
    }

    /// <summary>Review recorded causes and root-cause options for the current company and FIELD.</summary>
    public async Task<IncidentRcaSourceReview> ReviewRcaAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("The incident identity is required.", nameof(incidentId));
        var actor = user.ActingUserId();
        var access = await company.RefreshCompanyAsync();
        var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
        if (!selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId)
            throw new UnauthorizedAccessException("A current FIELD is required.");
        var context = selected.SelectionContext!;
        var lifecycle = await RequireAuthorityAsync(user, context);
        var review = await incidents.ReviewRcaSourceAsync(context, incidentId);
        if (review is null) throw new InvalidOperationException("The incident cause review is unavailable.");
        review.Require();
        if (review.Context != context || review.IncidentId != incidentId)
            throw new InvalidOperationException("The incident cause review does not match the selected company, FIELD and incident.");
        if (await RequireAuthorityAsync(user, context) != lifecycle)
            throw new UnauthorizedAccessException("The incident company, FIELD or lifecycle binding changed during review.");
        return review;
    }

    /// <summary>Record the original reviewed incident cause without advancing the lifecycle.</summary>
    public async Task<IncidentRcaReceipt> RecordRcaAsync(ClaimsPrincipal user, IncidentRcaIntent original)
    {
        ArgumentNullException.ThrowIfNull(original);
        original = JsonSerializer.Deserialize<IncidentRcaIntent>(JsonSerializer.Serialize(original))!;
        original.Require();
        var context = original.Source.Context;
        var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, context.Company, context.ActorUserId);
        if (!selected.SelectionAvailable || selected.SelectionContext != context)
            throw new UnauthorizedAccessException("The original incident company or FIELD selection changed.");
        var lifecycle = await RequireAuthorityAsync(user, context);
        var receipt = await incidents.RecordRcaAsync(original);
        receipt.Require(original);
        if (await RequireAuthorityAsync(user, context) != lifecycle)
            throw new UnauthorizedAccessException("The incident company, FIELD or lifecycle binding changed during recording.");
        return receipt;
    }

    public async Task<IReadOnlyList<IncidentRcaIntent>> ReadRecordedRcaAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("The incident identity is required.", nameof(incidentId));
        var actor = user.ActingUserId(); var access = await company.RefreshCompanyAsync();
        var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
        if (!selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId) throw new UnauthorizedAccessException("A current FIELD is required.");
        var context = selected.SelectionContext!; var lifecycle = await RequireAuthorityAsync(user, context);
        var originals = await incidents.ReadRecordedRcaAsync(context, incidentId) ?? throw new InvalidOperationException("Recorded cause recovery is unavailable.");
        if (originals.Count > 256 || originals.Any(original => original is null)) throw new InvalidOperationException("Recorded causes are unavailable or ambiguous.");
        foreach (var original in originals)
        {
            original.Require(); var source = original.Source;
            if (source.Context.Company != context.Company || source.Context.ActorUserId != actor || source.Context.Database != context.Database ||
                source.Context.FieldId != context.FieldId || source.IncidentId != incidentId)
                throw new InvalidOperationException("A recorded cause belongs to another company, account, FIELD or incident.");
        }
        if (originals.Select(original => original.IntentId).Distinct().Count() != originals.Count || await RequireAuthorityAsync(user, context) != lifecycle)
            throw new InvalidOperationException("Recorded causes or current authority changed during recovery.");
        return originals;
    }

    public async Task<IReadOnlyList<IncidentRcaRecordedCause>> ReadSharedRcaAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("The incident identity is required.", nameof(incidentId));
        var actor = user.ActingUserId(); var access = await company.RefreshCompanyAsync();
        var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
        if (!selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId) throw new UnauthorizedAccessException("A current FIELD is required.");
        var context = selected.SelectionContext!; var lifecycle = await RequireAuthorityAsync(user, context);
        var causes = await incidents.ReadSharedRcaAsync(context, incidentId) ?? throw new InvalidOperationException("Shared causes are unavailable.");
        if (causes.Count > 256 || causes.Any(cause => cause is null)) throw new InvalidOperationException("Shared causes are unavailable or ambiguous.");
        foreach (var cause in causes)
        {
            cause.Require();
            if (cause.Company != context.Company || cause.Database != context.Database || cause.FieldId != context.FieldId || cause.IncidentId != incidentId)
                throw new InvalidOperationException("A cause belongs to another company, FIELD or incident.");
        }
        if (causes.Select(cause => cause.IntentId).Distinct().Count() != causes.Count ||
            causes.Select(cause => cause.Observation).Distinct().Count() != causes.Count || await RequireAuthorityAsync(user, context) != lifecycle)
            throw new InvalidOperationException("Shared causes or current authority changed during review.");
        return causes;
    }

    public async Task<CompanyHseRcaHandoffReview> ReviewRcaHandoffAsync(ClaimsPrincipal user, string incidentId, Guid intentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId) || intentId == Guid.Empty) throw new ArgumentException("The incident and recorded cause are required.");
        var actor = user.ActingUserId(); var access = await company.RefreshCompanyAsync();
        var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
        if (!selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId) throw new UnauthorizedAccessException("A current FIELD is required.");
        var context = selected.SelectionContext!; var lifecycle = await RequireAuthorityAsync(user, context);
        async Task<IncidentRcaRecordedCause> ReadCause()
        {
            var matches = (await incidents.ReadSharedRcaAsync(context, incidentId)).Where(item => item.IntentId == intentId).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("The recorded cause is unavailable or ambiguous.");
            var original = matches[0]; original.Require();
            if (original.Company != context.Company || original.Database != context.Database || original.FieldId != context.FieldId || original.IncidentId != incidentId)
                throw new InvalidOperationException("The recorded cause belongs to another incident or FIELD.");
            return original;
        }
        async Task Guard(string role)
        {
            if (await RequireAuthorityAsync(user, context) != lifecycle) throw new UnauthorizedAccessException("The lifecycle binding changed.");
            var current = await company.RefreshCompanyAsync();
            if (!current.Roles.Contains(role, StringComparer.Ordinal) || !user.IsInRole(role)) throw new UnauthorizedAccessException("The current RCA step role is required.");
        }
        var cause = await ReadCause(); var causeJson = JsonSerializer.Serialize(cause);
        if (processes is not BoundProcessService bound) throw new InvalidOperationException("The fixed lifecycle process owner is required.");
        var process = await bound.CreateBoundServiceAsync(lifecycle.ConnectionName);
        if (process is not ICompanyHseWorkflowCommands owner) throw new InvalidOperationException("The reviewed HSE lifecycle owner is unavailable.");
        var review = await owner.ReviewRcaHandoffAsync(context, lifecycle, cause, Guard);
        review.Require();
        if (review.Context != context || review.LifecycleDatabase != lifecycle || JsonSerializer.Serialize(review.Cause) != causeJson)
            throw new InvalidOperationException("The RCA handoff review differs from the selected recorded cause.");
        await Guard(review.AssignedRole);
        if (JsonSerializer.Serialize(await ReadCause()) != causeJson) throw new InvalidOperationException("The recorded cause changed during handoff review.");
        await Guard(review.AssignedRole); return review;
    }

    public async Task<CompanyHseRcaHandoffReceipt> CompleteRcaHandoffAsync(ClaimsPrincipal user, CompanyHseRcaHandoffCommand original)
    {
        ArgumentNullException.ThrowIfNull(original); original = JsonSerializer.Deserialize<CompanyHseRcaHandoffCommand>(JsonSerializer.Serialize(original))!; original.Require();
        var review = original.Review; var context = review.Context; var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, context.Company, context.ActorUserId);
        if (!selected.SelectionAvailable || selected.SelectionContext != context) throw new UnauthorizedAccessException("The original company or FIELD selection changed.");
        async Task Guard(string role)
        {
            if (role != review.AssignedRole || await RequireAuthorityAsync(user, context) != review.LifecycleDatabase) throw new UnauthorizedAccessException("The RCA lifecycle binding changed.");
            var current = await company.RefreshCompanyAsync();
            if (!current.Roles.Contains(role, StringComparer.Ordinal) || !user.IsInRole(role)) throw new UnauthorizedAccessException("The assigned RCA role is required.");
        }
        async Task RequireCause()
        {
            var rows = (await incidents.ReadSharedRcaAsync(context, review.Cause.IncidentId)).Where(item => item.IntentId == review.Cause.IntentId).ToArray();
            if (rows.Length != 1 || JsonSerializer.Serialize(rows[0]) != JsonSerializer.Serialize(review.Cause)) throw new InvalidOperationException("The recorded cause changed or is unavailable.");
        }
        await Guard(review.AssignedRole); await RequireCause(); await Guard(review.AssignedRole);
        if (processes is not BoundProcessService bound) throw new InvalidOperationException("The fixed lifecycle process owner is required.");
        var process = await bound.CreateBoundServiceAsync(review.LifecycleDatabase.ConnectionName);
        if (process is not ICompanyHseWorkflowCommands owner) throw new InvalidOperationException("The RCA lifecycle owner is unavailable.");
        var receipt = await owner.CompleteRcaHandoffAsync(original, Guard, RequireCause);
        receipt.Require(original); await Guard(review.AssignedRole); await RequireCause(); await Guard(review.AssignedRole); return receipt;
    }

    public async Task<CompanyHseRcaRecoveredHandoff?> RecoverRcaHandoffAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("The incident identity is required.", nameof(incidentId));
        var actor = user.ActingUserId(); var access = await company.RefreshCompanyAsync(); var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
        if (!selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId) throw new UnauthorizedAccessException("A current FIELD is required.");
        var context = selected.SelectionContext!; var lifecycle = await RequireAuthorityAsync(user, context);
        async Task Guard(string role)
        {
            if (await RequireAuthorityAsync(user, context) != lifecycle) throw new UnauthorizedAccessException("The RCA lifecycle binding changed.");
            var current = await company.RefreshCompanyAsync();
            if (!current.Roles.Contains(role, StringComparer.Ordinal) || !user.IsInRole(role)) throw new UnauthorizedAccessException("The assigned RCA role is required.");
        }
        async Task RequireCause(IncidentRcaRecordedCause original)
        {
            var rows = (await incidents.ReadSharedRcaAsync(context, incidentId)).Where(item => item.IntentId == original.IntentId).ToArray();
            if (rows.Length != 1 || JsonSerializer.Serialize(rows[0]) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The recorded cause changed or is unavailable.");
        }
        if (processes is not BoundProcessService bound) throw new InvalidOperationException("The fixed lifecycle process owner is required.");
        var process = await bound.CreateBoundServiceAsync(lifecycle.ConnectionName);
        if (process is not ICompanyHseWorkflowCommands owner) throw new InvalidOperationException("The RCA lifecycle owner is unavailable.");
        var result = await owner.RecoverRcaHandoffAsync(context, lifecycle, incidentId, Guard, RequireCause);
        if (result is not null)
        {
            result.Require(); var original = result.Original; var prior = original.Review.Context;
            if (prior.Company != context.Company || prior.ActorUserId != actor || prior.Database != context.Database || prior.FieldId != context.FieldId ||
                original.Review.LifecycleDatabase != lifecycle || original.Review.Cause.IncidentId != incidentId)
                throw new InvalidOperationException("The recovered RCA handoff belongs to another account, FIELD or incident.");
            await Guard(original.Review.AssignedRole); await RequireCause(original.Review.Cause); await Guard(original.Review.AssignedRole);
        }
        else if (await RequireAuthorityAsync(user, context) != lifecycle) throw new UnauthorizedAccessException("The RCA lifecycle binding changed.");
        return result;
    }

    public async Task<CompanyHseWorkflowReceipt> StartAsync(ClaimsPrincipal user, CompanyHseWorkflowStart original)
    {
        ArgumentNullException.ThrowIfNull(original); original = JsonSerializer.Deserialize<CompanyHseWorkflowStart>(JsonSerializer.Serialize(original))!; original.Require();
        var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, original.Context.Company, original.Context.ActorUserId);
        if (!selected.SelectionAvailable || selected.SelectionContext != original.Context || await RequireAuthorityAsync(user, original.Context) != original.LifecycleDatabase)
            throw new UnauthorizedAccessException("The original incident company, FIELD or lifecycle binding changed.");
        if (processes is not BoundProcessService bound) throw new InvalidOperationException("The fixed lifecycle process owner is required.");
        var process = await bound.CreateBoundServiceAsync(original.LifecycleDatabase.ConnectionName);
        if (process is not ICompanyHseWorkflowCommands owner) throw new InvalidOperationException("The reviewed HSE lifecycle owner is unavailable.");
        async Task Guard()
        {
            if (await RequireAuthorityAsync(user, original.Context) != original.LifecycleDatabase)
                throw new UnauthorizedAccessException("The original HSE authority or lifecycle binding changed.");
        }
        Task<string> Read() => incidents.ReviewWorkflowSourceRevisionAsync(original.Context, original.IncidentId);
        var receipt = await owner.StartHseWorkflowAsync(original, Guard, Read);
        receipt.Require(original);
        await Guard();
        return receipt;
    }
}
