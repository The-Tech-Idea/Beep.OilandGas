using Beep.OilandGas.Models.Data;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed class ModuleDatabaseClient(ApiClient api)
{
    public async Task<List<ModuleDatabaseSummary>> ListAsync() =>
        await api.GetAsync<List<ModuleDatabaseSummary>>("/api/setup/modules") ?? [];
    public async Task<List<string>> ConnectionsAsync() =>
        await api.GetAsync<List<string>>("/api/setup/modules/connections") ?? [];
    public async Task<ModuleSeedSummary> SeedAsync(string moduleId, ModuleSeedRequest request) =>
        await api.PostAsync<ModuleSeedRequest, ModuleSeedSummary>(
            $"/api/setup/modules/{Uri.EscapeDataString(moduleId)}/seed", request)
        ?? throw new InvalidOperationException("The seeding response was empty.");
    public async Task<ModuleDatabaseBinding> BindAsync(string moduleId, ModuleBindingRequest request) =>
        await api.PutAsync<ModuleBindingRequest, ModuleDatabaseBinding>(
            $"/api/setup/modules/{Uri.EscapeDataString(moduleId)}/connection", request)
        ?? throw new InvalidOperationException("The binding response was empty.");
    public async Task<SchemaMigrationPlanResult> PlanAsync(string moduleId, ModulePlanRequest request) =>
        await api.PostAsync<ModulePlanRequest, SchemaMigrationPlanResult>(
            $"/api/setup/modules/{Uri.EscapeDataString(moduleId)}/plan", request)
        ?? throw new InvalidOperationException("The migration plan response was empty.");
    public async Task<SchemaMigrationApprovalResult> ApproveAsync(string planId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);
        var result = await api.PostAsync<SchemaMigrationApprovalRequest, SchemaMigrationApprovalResult>(
            "/api/ppdm39/setup/schema/approve", new() { PlanId = planId })
        ?? throw new InvalidOperationException("The approval response was empty.");
        if (result.Success && !string.Equals(result.PlanId, planId, StringComparison.Ordinal))
            throw new InvalidOperationException("The approval response does not identify the reviewed plan.");
        return result;
    }
    public async Task<SchemaMigrationExecuteResult> ExecuteAsync(SchemaMigrationPlanResult plan, bool acknowledgeHighRisk)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.PlanId);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.PlanHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.ManifestHash);
        var result = await api.PostAsync<SchemaMigrationExecuteRequest, SchemaMigrationExecuteResult>(
            "/api/ppdm39/setup/schema/execute", new()
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
