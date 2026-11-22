using Beep.OilandGas.Models.Data.PPDM39Setup;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;

/// <summary>One module in the plan.</summary>
public sealed record OgSetupModulePlan(
    string ModuleId,
    string ModuleName,
    int Order,
    int EntityTypeCount,
    bool AlreadyCompleted);

/// <summary>What an apply run will do, before it does anything.</summary>
public sealed record OgSetupPlan(
    string ConnectionName,
    IReadOnlyList<OgSetupModulePlan> Modules,
    int EntityTypeCount,
    bool IsComplete);

/// <summary>Progress for the UI: which phase, which module, how far.</summary>
public sealed record OgSetupProgress(string Phase, string ModuleId, int Completed, int Total, int Records);

/// <summary>One module's outcome.</summary>
public sealed record OgSetupModuleOutcome(string ModuleId, bool Success, int RecordsInserted, string? Error);

/// <summary>The result of an apply or resume run.</summary>
public sealed record OgSetupOutcome(
    bool Success,
    string ConnectionName,
    int ModulesRun,
    int ModulesSucceeded,
    int RecordsInserted,
    IReadOnlyList<OgSetupModuleOutcome> Modules,
    bool CanResume,
    string? SchemaMessage,
    string? Reference);

/// <summary>
/// DAT-02: brings a database to the state the modules expect — plan, schema apply, then seeding in
/// module order, with progress and a resume that skips what already completed.
/// </summary>
public interface IOgModuleSetupRunner
{
    Task<OgResult<OgSetupPlan>> PlanAsync(CancellationToken cancellationToken = default);

    Task<OgResult<OgSetupOutcome>> ApplyAsync(
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgSetupOutcome>> ResumeAsync(
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The runner over <c>PPDM39SetupService</c> and <c>ModuleSetupOrchestrator</c>. Nothing about schema or
/// seeding is re-implemented here; this class sequences the owning services and records what completed.
/// </summary>
public sealed class OgModuleSetupRunner : IOgModuleSetupRunner
{
    private readonly OgSetupComposition _composition;
    private readonly OgSetupStateStore _stateStore;
    private readonly IFailureReporter _failures;
    private readonly string _userId;

    public OgModuleSetupRunner(
        OgSetupComposition composition,
        OgSetupStateStore stateStore,
        IFailureReporter failures,
        string userId = "system")
    {
        _composition = composition ?? throw new ArgumentNullException(nameof(composition));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _userId = string.IsNullOrWhiteSpace(userId) ? "system" : userId;
    }

    public string ConnectionName => _composition.ConnectionName;

    public Task<OgResult<OgSetupPlan>> PlanAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = _composition.CreateScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<ModuleSetupOrchestrator>();
            var state = _stateStore.Load();

            var completed = new HashSet<string>(
                state.Modules.Where(module => module.Succeeded).Select(module => module.ModuleId),
                StringComparer.OrdinalIgnoreCase);

            var modules = orchestrator.GetModuleMetadata()
                .Select(module => new OgSetupModulePlan(
                    module.ModuleId,
                    module.ModuleName,
                    module.Order,
                    module.EntityTypes.Count,
                    completed.Contains(module.ModuleId)))
                .ToList();

            var plan = new OgSetupPlan(
                _composition.ConnectionName,
                modules,
                orchestrator.GetAllEntityTypes().Count,
                modules.Count > 0 && modules.All(module => module.AlreadyCompleted));

            return Task.FromResult(OgResult<OgSetupPlan>.Ok(plan));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                "planning the database module setup",
                "no plan was produced",
                FailureSeverity.Error);

            return Task.FromResult(OgResult<OgSetupPlan>.Failed("The setup plan could not be produced.", reference));
        }
    }

    public Task<OgResult<OgSetupOutcome>> ApplyAsync(
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunAsync(progress, cancellationToken);

    public Task<OgResult<OgSetupOutcome>> ResumeAsync(
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunAsync(progress, cancellationToken);

    private async Task<OgResult<OgSetupOutcome>> RunAsync(
        IProgress<OgSetupProgress>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var planned = await PlanAsync(cancellationToken).ConfigureAwait(false);
            if (!planned.Success || planned.Value is null)
            {
                return OgResult<OgSetupOutcome>.Failed(
                    planned.Message ?? "The setup plan could not be produced.",
                    planned.Reference ?? "setup-plan-failed");
            }

            var plan = planned.Value;
            var state = _stateStore.Load();
            state.ConnectionName = _composition.ConnectionName;
            state.LastRunUtc = DateTimeOffset.UtcNow;

            var schemaMessage = await ApplySchemaAsync(plan, cancellationToken).ConfigureAwait(false);

            var outcomes = new List<OgSetupModuleOutcome>();
            var total = plan.Modules.Count;

            foreach (var module in plan.Modules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (module.AlreadyCompleted)
                {
                    outcomes.Add(new OgSetupModuleOutcome(module.ModuleId, true, 0, "already completed"));
                    continue;
                }

                progress?.Report(new OgSetupProgress("seeding", module.ModuleId, outcomes.Count, total, 0));

                using var scope = _composition.CreateScope();
                var setup = scope.ServiceProvider.GetRequiredService<IPPDM39SetupService>();

                var seeded = await setup.SeedSelectedModulesAsync(
                    [module.ModuleId],
                    _composition.ConnectionName,
                    _userId,
                    cancellationToken).ConfigureAwait(false);

                var outcome = new OgSetupModuleOutcome(
                    module.ModuleId,
                    seeded.Success,
                    seeded.TotalInserted,
                    seeded.Success ? null : (seeded.Message.Length > 0 ? seeded.Message : "the module reported errors"));

                outcomes.Add(outcome);

                RecordModule(state, outcome);
                _stateStore.Save(state);
            }

            var succeeded = outcomes.Count(outcome => outcome.Success);
            var aggregate = new OgSetupOutcome(
                Success: succeeded == outcomes.Count && outcomes.Count > 0,
                ConnectionName: _composition.ConnectionName,
                ModulesRun: outcomes.Count,
                ModulesSucceeded: succeeded,
                RecordsInserted: outcomes.Sum(outcome => outcome.RecordsInserted),
                Modules: outcomes,
                CanResume: succeeded < outcomes.Count,
                SchemaMessage: schemaMessage,
                Reference: null);

            state.Completed = aggregate.Success;
            _stateStore.Save(state);

            progress?.Report(new OgSetupProgress("done", string.Empty, outcomes.Count, total, aggregate.RecordsInserted));

            return aggregate.Success
                ? OgResult<OgSetupOutcome>.Ok(aggregate)
                : OgResult<OgSetupOutcome>.Failed(
                    $"{outcomes.Count - succeeded} of {outcomes.Count} modules did not complete.",
                    "module-setup-incomplete");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                "running the database module setup",
                "the database is not fully set up",
                FailureSeverity.Error);

            return OgResult<OgSetupOutcome>.Failed("The module setup run failed.", reference);
        }
    }

    /// <summary>Plans and applies the schema. A schema problem is reported, not hidden, and seeding still runs.</summary>
    private async Task<string?> ApplySchemaAsync(OgSetupPlan plan, CancellationToken cancellationToken)
    {
        using var scope = _composition.CreateScope();
        var setup = scope.ServiceProvider.GetRequiredService<IPPDM39SetupService>();

        var plannedSchema = await setup.PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest
        {
            ConnectionName = _composition.ConnectionName,
            ModuleIds = plan.Modules.Select(module => module.ModuleId).ToList(),
            EnvironmentTier = "Development",
        }).ConfigureAwait(false);

        if (!plannedSchema.Success)
        {
            return $"Schema plan not produced: {plannedSchema.Message}";
        }

        if (!plannedSchema.CanApply)
        {
            return $"Schema plan requires {plannedSchema.PendingOperationCount} operation(s) but is not applicable: {plannedSchema.PolicyDecision}";
        }

        var executed = await setup.ExecuteSchemaMigrationPlanAsync(new SchemaMigrationExecuteRequest
        {
            PlanId = plannedSchema.PlanId,
            ExecutedBy = _userId,
            ExpectedPlanHash = plannedSchema.PlanHash,
            ExpectedManifestHash = plannedSchema.ManifestHash,
            ResumeIfCheckpointExists = true,
            AcknowledgeHighRisk = false,
        }).ConfigureAwait(false);

        return executed.Success
            ? $"Schema applied: {plannedSchema.TablesToCreate} table(s) to create, plan {plannedSchema.PlanId}."
            : $"Schema apply failed: {executed.Message}";
    }

    private static void RecordModule(OgSetupState state, OgSetupModuleOutcome outcome)
    {
        var existing = state.Modules.FirstOrDefault(module =>
            string.Equals(module.ModuleId, outcome.ModuleId, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            existing = new OgSetupModuleState { ModuleId = outcome.ModuleId };
            state.Modules.Add(existing);
        }

        existing.Succeeded = outcome.Success;
        existing.RecordsInserted = outcome.RecordsInserted;
        existing.CompletedAtUtc = DateTimeOffset.UtcNow;
    }
}
