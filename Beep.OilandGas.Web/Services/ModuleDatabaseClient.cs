using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services;

public sealed class ModuleDatabaseClient(ApiClient api)
{
    private static string Scoped(string path, string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        return $"{path}?tenantId={Uri.EscapeDataString(tenantId)}";
    }
    public async Task<List<AppTenant>> TenantsAsync() =>
        await api.GetAsync<List<AppTenant>>("/api/setup/modules/tenants") ?? [];
    public async Task<List<ModuleDatabaseSummary>> ListAsync(string tenantId) =>
        await api.GetAsync<List<ModuleDatabaseSummary>>(Scoped("/api/setup/modules", tenantId)) ?? [];
    public async Task<List<string>> ConnectionsAsync() =>
        await api.GetAsync<List<string>>("/api/setup/modules/connections") ?? [];
    public async Task<ModuleSeedSummary> SeedAsync(string tenantId, string moduleId, ModuleSeedRequest request) =>
        await api.PostAsync<ModuleSeedRequest, ModuleSeedSummary>(
            Scoped($"/api/setup/modules/{Uri.EscapeDataString(moduleId)}/seed", tenantId), request)
        ?? throw new InvalidOperationException("The seeding response was empty.");
    public async Task<ModuleDatabaseBinding> BindAsync(string tenantId, string moduleId, ModuleBindingRequest request) =>
        await api.PutAsync<ModuleBindingRequest, ModuleDatabaseBinding>(
            Scoped($"/api/setup/modules/{Uri.EscapeDataString(moduleId)}/connection", tenantId), request)
        ?? throw new InvalidOperationException("The binding response was empty.");
    public async Task<SchemaMigrationPlanResult> PlanAsync(string tenantId, string moduleId, ModulePlanRequest request) =>
        await api.PostAsync<ModulePlanRequest, SchemaMigrationPlanResult>(
            Scoped($"/api/setup/modules/{Uri.EscapeDataString(moduleId)}/plan", tenantId), request)
        ?? throw new InvalidOperationException("The migration plan response was empty.");
    public async Task<SchemaMigrationApprovalResult> ApproveAsync(string tenantId, SchemaMigrationPlanResult plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.PlanId);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.PlanHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.ManifestHash);
        var result = await api.PostAsync<SchemaMigrationApprovalRequest, SchemaMigrationApprovalResult>(
            Scoped("/api/ppdm39/setup/schema/approve", tenantId), new() { PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash })
        ?? throw new InvalidOperationException("The approval response was empty.");
        if (result.Success && (!string.Equals(result.PlanId, plan.PlanId, StringComparison.Ordinal) ||
            !string.Equals(result.PlanHash, plan.PlanHash, StringComparison.Ordinal) || !string.Equals(result.ManifestHash, plan.ManifestHash, StringComparison.Ordinal)))
            throw new InvalidOperationException("The approval response does not identify the reviewed plan and manifest.");
        return result;
    }
    public async Task<SchemaMigrationExecuteResult> ExecuteAsync(string tenantId, SchemaMigrationPlanResult plan, bool acknowledgeHighRisk)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.PlanId);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.PlanHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.ManifestHash);
        var result = await api.PostAsync<SchemaMigrationExecuteRequest, SchemaMigrationExecuteResult>(
            Scoped("/api/ppdm39/setup/schema/execute", tenantId), new()
            {
                PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash,
                ExpectedManifestHash = plan.ManifestHash, AcknowledgeHighRisk = acknowledgeHighRisk
            }) ?? throw new InvalidOperationException("The execution response was empty.");
        if (result.Success && (!string.Equals(result.PlanId, plan.PlanId, StringComparison.Ordinal)
            || !string.Equals(result.PlanHash, plan.PlanHash, StringComparison.Ordinal)
            || !string.Equals(result.ManifestHash, plan.ManifestHash, StringComparison.Ordinal)))
            throw new InvalidOperationException("The execution response does not match the reviewed plan. Verify server status before retrying.");
        return result;
    }
}
