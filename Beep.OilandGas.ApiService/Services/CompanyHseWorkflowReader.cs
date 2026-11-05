using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.ApiService.Services;

public interface ICompanyHseWorkflowReader
{
    Task<ProcessInstance?> ReadAsync(ClaimsPrincipal user, string incidentId);
}

/// <summary>Resolves an incident workflow only from its recorded company, FIELD and database ownership.</summary>
public sealed class CompanyHseWorkflowReader(CompanyModuleConnection company, ModuleConnectionResolver modules,
    ICompanyFieldSelectionService selections, IFieldOrchestrator field, IProcessService processes) : ICompanyHseWorkflowReader
{
    public async Task<ProcessInstance?> ReadAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        var actor = user.ActingUserId();
        async Task<(FieldSelectionReview Context, AssetDatabaseScope Lifecycle)> CaptureAsync()
        {
            var access = await company.RefreshCompanyAsync();
            var selected = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
            if (user.Identity?.IsAuthenticated != true || access.UserId != actor || !access.Roles.Any(user.IsInRole) ||
                !selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId || selected.SelectionContext!.Database != await company.ResolveAsync())
                throw RefusalException.Forbidden("The current company and FIELD cannot be confirmed.");
            var connection = await modules.ResolveAsync("LIFECYCLE");
            return (selected.SelectionContext, new AssetDatabaseScope(connection, await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"], connection)));
        }
        var captured = await CaptureAsync();
        var incident = await field.GetHSEService().GetIncidentAsync(incidentId);
        if (incident is null || incident.IncidentId != incidentId || incident.FieldId != captured.Context.FieldId)
            throw RefusalException.NotFound("The incident is unavailable in the current FIELD.");
        var instances = await processes.GetProcessInstancesForEntityAsync(incidentId, CompanyHseWorkflowStart.EntityType)
            ?? throw new InvalidOperationException("The incident workflow population is unavailable.");
        var owned = new List<ProcessInstance>();
        foreach (var instance in instances)
        {
            if (instance is null || instance.ProcessId != CompanyHseWorkflowStart.ProcessId || instance.EntityId != incidentId ||
                instance.EntityType != CompanyHseWorkflowStart.EntityType || instance.ProcessData is null)
                throw new InvalidOperationException("The incident workflow ownership cannot be confirmed.");
            var original = CompanyHseWorkflowStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData), instance.InstanceId, instance.FieldId, incidentId);
            if (original.Context.Company != captured.Context.Company || original.Context.FieldId != captured.Context.FieldId) continue;
            if (original.Context.Database != captured.Context.Database || original.LifecycleDatabase != captured.Lifecycle)
                throw RefusalException.Conflict("The incident workflow belongs to a different database binding.");
            owned.Add(instance);
        }
        if (owned.Count > 1) throw RefusalException.Conflict("More than one workflow belongs to this incident. Review the recorded workflows.");
        if (await CaptureAsync() != captured) throw RefusalException.Conflict("The current company, FIELD or workflow binding changed.");
        incident = await field.GetHSEService().GetIncidentAsync(incidentId);
        if (incident is null || incident.IncidentId != incidentId || incident.FieldId != captured.Context.FieldId || await CaptureAsync() != captured)
            throw RefusalException.Conflict("The incident or current FIELD changed during workflow review.");
        return owned.SingleOrDefault();
    }
}
