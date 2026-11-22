using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
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
    bool AlreadyCompleted,
    IReadOnlyList<string>? ReferenceTables = null)
{
    /// <summary>True when the module loads demo data and needs an explicit confirmation (AC-5).</summary>
    public bool RequiresConfirmation => OgSetupPolicy.IsDemoModule(ModuleId);

    /// <summary>
    /// The reference ("code") tables this module will seed - the <c>R_*</c> entity types - so the person can review
    /// the LOV set before anything is applied. Never null.
    /// </summary>
    public IReadOnlyList<string> ReferenceLists => ReferenceTables ?? [];

    /// <summary>
    /// The row the wizard shows for this module: its name, its id, how many types it creates, and where it stands -
    /// already in the database, waiting for the demonstration-data confirmation, or to be applied.
    /// </summary>
    public string Describe()
    {
        var name = $"{ModuleName} ({ModuleId}) - {EntityTypeCount} type(s)";

        if (AlreadyCompleted)
        {
            return $"{name} - already applied";
        }

        return RequiresConfirmation
            ? $"{name} - demonstration data, needs confirmation"
            : $"{name} - will be applied";
    }
}

/// <summary>What an apply run will do, before it does anything.</summary>
/// <param name="ConnectionName">The database the plan applies to.</param>
/// <param name="Modules">The modules, in apply order, with what each one brings.</param>
/// <param name="EntityTypeCount">How many entity types the whole plan creates.</param>
/// <param name="IsComplete">True when every planned module is already in the database.</param>
/// <param name="ReferenceTables">The <c>R_*</c> code tables the plan will seed, distinct and ordered - the set the
/// SystemAdmin reviews before applying.</param>
public sealed record OgSetupPlan(
    string ConnectionName,
    IReadOnlyList<OgSetupModulePlan> Modules,
    int EntityTypeCount,
    bool IsComplete,
    IReadOnlyList<string>? ReferenceTables = null)
{
    /// <summary>True when a module in the plan will be skipped unless demo data is confirmed.</summary>
    public bool NeedsDemoConfirmation => Modules.Any(module => module.RequiresConfirmation && !module.AlreadyCompleted);

    /// <summary>The reference tables to seed, never null.</summary>
    public IReadOnlyList<string> ReferenceLists => ReferenceTables ?? [];
}

/// <summary>
/// Which of a module's entity types are reference ("code") tables: the ones named <c>R_*</c>, the PPDM convention the
/// API and the PPDM browser use. Selecting them here - from the module metadata that the plan already reads - is what
/// lets the wizard show the LOV set before anything is applied, rather than after.
/// </summary>
public static class OgSetupReferenceTables
{
    /// <summary>The prefix PPDM uses for a reference/code table.</summary>
    public const string Prefix = "R_";

    /// <summary>
    /// The reference-table names among <paramref name="entityTypeNames"/>: blanks and non-reference names dropped,
    /// duplicates removed, ordered so two runs show the same list.
    /// </summary>
    public static IReadOnlyList<string> From(IEnumerable<string?>? entityTypeNames) =>
        (entityTypeNames ?? [])
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!.Trim())
            .Where(name => name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>The reference-table names among <paramref name="entityTypes"/>.</summary>
    public static IReadOnlyList<string> From(IEnumerable<Type>? entityTypes) =>
        From(entityTypes?.Select(type => type.Name));
}

/// <summary>Progress for the UI: which phase, which module, how far.</summary>
public sealed record OgSetupProgress(string Phase, string ModuleId, int Completed, int Total, int Records);

/// <summary>One module's outcome.</summary>
public sealed record OgSetupModuleOutcome(
    string ModuleId,
    bool Success,
    int RecordsInserted,
    string? Error,
    bool Skipped = false);

/// <summary>The schema step summary and the modules whose schema it refused.</summary>
public sealed record OgSetupSchemaResult(string Message, IReadOnlyDictionary<string, string> FailedModules)
{
    public bool HasFailures => FailedModules.Count > 0;
}

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
    string? Reference)
{
    /// <summary>True when a demo module was left out because it was not confirmed (AC-5).</summary>
    public bool DemoDataSkipped => Modules.Any(module => module.Skipped);
}

/// <summary>
/// DAT-02: brings a database to the state the modules expect — plan, schema apply, then seeding in
/// module order, with progress and a resume that skips what already completed.
/// </summary>
public interface IOgModuleSetupRunner
{
    /// <summary>The connection this runner sets up.</summary>
    string ConnectionName { get; }

    Task<OgResult<OgSetupPlan>> PlanAsync(CancellationToken cancellationToken = default);

    /// <summary>Plans only the named modules; null or empty means every discovered module.</summary>
    Task<OgResult<OgSetupPlan>> PlanAsync(
        IReadOnlyList<string>? moduleIds,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgSetupOutcome>> ApplyAsync(
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgSetupOutcome>> ResumeAsync(
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies, with <paramref name="confirmation"/> deciding whether the demo dataset may be loaded (AC-5).
    /// Passing null confirms nothing, so demo data is skipped rather than loaded by default.
    /// </summary>
    Task<OgResult<OgSetupOutcome>> ApplyAsync(
        OgSetupConfirmation? confirmation,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Resumes, with the same confirmation rule as <see cref="ApplyAsync(OgSetupConfirmation?, IProgress{OgSetupProgress}?, CancellationToken)"/>.</summary>
    Task<OgResult<OgSetupOutcome>> ResumeAsync(
        OgSetupConfirmation? confirmation,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Applies only the named modules (the schema plan is scoped the same way).</summary>
    Task<OgResult<OgSetupOutcome>> ApplyAsync(
        OgSetupConfirmation? confirmation,
        IReadOnlyList<string>? moduleIds,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a named subset of entity types within the selected modules. Used to bisect a module that cannot be
    /// planned whole, and by the batching path internally; null or empty means every type the modules resolve to.
    /// </summary>
    Task<OgResult<OgSetupOutcome>> ApplyAsync(
        OgSetupConfirmation? confirmation,
        IReadOnlyList<string>? moduleIds,
        IReadOnlyList<string>? entityTypeNames,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Resumes only the named modules.</summary>
    Task<OgResult<OgSetupOutcome>> ResumeAsync(
        OgSetupConfirmation? confirmation,
        IReadOnlyList<string>? moduleIds,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The runner over <c>PPDM39SetupService</c> and <c>ModuleSetupOrchestrator</c>. Nothing about schema or
/// seeding is re-implemented here; this class sequences the owning services and records what completed.
/// </summary>
public sealed class OgModuleSetupRunner : IOgModuleSetupRunner
{
    /// <summary>The result code when the bound connection will not open (AC-6).</summary>
    public const string CodeUnreachable = "setup-connection-unreachable";

    /// <summary>
    /// The most entity types one migration plan may carry. PPDM_CORE resolves to the whole catalogue (2,716 types);
    /// planning that in one artifact took minutes to hash and gigabytes to persist, so the catalogue is applied in
    /// batches of this size.
    /// </summary>
    public const int SchemaBatchSize = 64;

    private readonly OgSetupComposition _composition;
    private readonly OgSetupStateStore _stateStore;
    private readonly IFailureReporter _failures;
    private readonly string _userId;
    private readonly Func<string?>? _localDatabasePath;

    public OgModuleSetupRunner(
        OgSetupComposition composition,
        OgSetupStateStore stateStore,
        IFailureReporter failures,
        string userId = "system",
        Func<string?>? localDatabasePath = null)
    {
        _composition = composition ?? throw new ArgumentNullException(nameof(composition));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _userId = string.IsNullOrWhiteSpace(userId) ? "system" : userId;
        _localDatabasePath = localDatabasePath;
    }

    public string ConnectionName => _composition.ConnectionName;

    public Task<OgResult<OgSetupPlan>> PlanAsync(CancellationToken cancellationToken = default) =>
        PlanAsync(moduleIds: null, cancellationToken);

    public Task<OgResult<OgSetupPlan>> PlanAsync(
        IReadOnlyList<string>? moduleIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // AC-6: a setup against a connection that will not open is refused, before anything is read or
            // written, with the reason in the result rather than a schema error later.
            var reachable = _composition.ProbeTarget();

            if (!reachable.Success)
            {
                return Task.FromResult(OgResult<OgSetupPlan>.Failed(
                    reachable.Message ?? $"The connection '{ConnectionName}' is not reachable.",
                    reachable.Reference ?? CodeUnreachable));
            }

            using var scope = _composition.CreateScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<ModuleSetupOrchestrator>();
            var state = _stateStore.Load();

            var completed = new HashSet<string>(
                state.Modules.Where(module => module.Succeeded).Select(module => module.ModuleId),
                StringComparer.OrdinalIgnoreCase);

            // The declared count is not what will be applied: PPDM_CORE declares a handful of types and resolves to
            // the whole catalogue. The wizard must show the truth, or a person approves 17 entity types and gets 2,716.
            var setup = scope.ServiceProvider.GetRequiredService<IPPDM39SetupService>();

            int ResolvedCount((string ModuleId, string ModuleName, int Order, IReadOnlyList<Type> EntityTypes) module)
            {
                try
                {
                    return setup.ResolvedEntityTypeNames([module.ModuleId]).Count;
                }
                catch (Exception ex)
                {
                    _failures.ReportHandled(
                        ex,
                        $"resolving the entity types of module '{module.ModuleId}'",
                        "the plan reports the module's declared count",
                        FailureSeverity.Degraded);

                    return module.EntityTypes.Count;
                }
            }

            var modules = orchestrator.GetModuleMetadata()
                .Select(module => new OgSetupModulePlan(
                    module.ModuleId,
                    module.ModuleName,
                    module.Order,
                    ResolvedCount(module),
                    completed.Contains(module.ModuleId),
                    OgSetupReferenceTables.From(module.EntityTypes)))
                .ToList();

            // A scope narrows both the reported plan and the entity count, so the schema request covers
            // exactly what will be applied.
            if (moduleIds is { Count: > 0 })
            {
                var wanted = new HashSet<string>(moduleIds, StringComparer.OrdinalIgnoreCase);
                modules = modules.Where(module => wanted.Contains(module.ModuleId)).ToList();
            }

            // The reference lists follow the scope: the person reviews what this run will seed, not the whole catalogue.
            var referenceTables = modules
                .SelectMany(module => module.ReferenceLists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var plan = new OgSetupPlan(
                _composition.ConnectionName,
                modules,
                orchestrator.GetAllEntityTypes().Count,
                modules.Count > 0 && modules.All(module => module.AlreadyCompleted),
                referenceTables);

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
        RunAsync(OgSetupConfirmation.None, moduleIds: null, progress, cancellationToken);

    public Task<OgResult<OgSetupOutcome>> ResumeAsync(
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunAsync(OgSetupConfirmation.None, moduleIds: null, progress, cancellationToken);

    public Task<OgResult<OgSetupOutcome>> ApplyAsync(
        OgSetupConfirmation? confirmation,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunAsync(confirmation, moduleIds: null, progress, cancellationToken);

    public Task<OgResult<OgSetupOutcome>> ApplyAsync(
        OgSetupConfirmation? confirmation,
        IReadOnlyList<string>? moduleIds,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunAsync(confirmation, moduleIds, progress, cancellationToken);

    public Task<OgResult<OgSetupOutcome>> ApplyAsync(
        OgSetupConfirmation? confirmation,
        IReadOnlyList<string>? moduleIds,
        IReadOnlyList<string>? entityTypeNames,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunAsync(confirmation, moduleIds, progress, cancellationToken, entityTypeNames);

    public Task<OgResult<OgSetupOutcome>> ResumeAsync(
        OgSetupConfirmation? confirmation,
        IReadOnlyList<string>? moduleIds,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunAsync(confirmation, moduleIds, progress, cancellationToken);

    public Task<OgResult<OgSetupOutcome>> ResumeAsync(
        OgSetupConfirmation? confirmation,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunAsync(confirmation, moduleIds: null, progress, cancellationToken);

    private async Task<OgResult<OgSetupOutcome>> RunAsync(
        OgSetupConfirmation? confirmation,
        IReadOnlyList<string>? moduleIds,
        IProgress<OgSetupProgress>? progress,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? entityTypeNames = null)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var planned = await PlanAsync(moduleIds, cancellationToken).ConfigureAwait(false);
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

            var schema = await ApplySchemaAsync(plan, progress, cancellationToken, entityTypeNames).ConfigureAwait(false);
            var schemaMessage = schema.Message;

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

                // AC-4: a module whose schema was refused is not seeded. Writing rows into a table whose
                // shape failed verification is exactly the damage this criterion exists to prevent.
                if (schema.FailedModules.TryGetValue(module.ModuleId, out var schemaError))
                {
                    outcomes.Add(new OgSetupModuleOutcome(
                        module.ModuleId,
                        Success: false,
                        RecordsInserted: 0,
                        Error: $"schema refused: {schemaError}"));

                    progress?.Report(new OgSetupProgress("blocked", module.ModuleId, outcomes.Count, total, 0));
                    continue;
                }

                // A module with no entity types of its own and no demonstration data has nothing to create or seed.
                // SECURITY is the case that matters: it is installed through the default EF repository, and asking the
                // orchestrator to seed it throws, which failed the whole catalogue apply at 2 h 52 m.
                if (module.EntityTypeCount == 0 && !module.RequiresConfirmation)
                {
                    outcomes.Add(new OgSetupModuleOutcome(
                        module.ModuleId,
                        Success: true,
                        RecordsInserted: 0,
                        Error: "no schema or data of its own"));

                    progress?.Report(new OgSetupProgress(
                        "nothing-to-do", module.ModuleId, outcomes.Count, total, 0));

                    RecordModule(state, outcomes[^1]);
                    _stateStore.Save(state);
                    continue;
                }

                if (!OgSetupPolicy.MayRun(module.ModuleId, confirmation, _composition.ConnectionName))
                {
                    outcomes.Add(new OgSetupModuleOutcome(
                        module.ModuleId,
                        Success: false,
                        RecordsInserted: 0,
                        Error: OgSetupPolicy.DemoSkipReason(_composition.ConnectionName),
                        Skipped: true));

                    progress?.Report(new OgSetupProgress("skipped", module.ModuleId, outcomes.Count, total, 0));
                    continue;
                }

                progress?.Report(new OgSetupProgress("seeding", module.ModuleId, outcomes.Count, total, 0));

                using var scope = _composition.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<ModuleSetupOrchestrator>();

                // The orchestrator owns per-module isolation and seeding; this runner only sequences it.
                var seedPhase = System.Diagnostics.Stopwatch.StartNew();
                var seeded = await orchestrator.RunSeedForModulesAsync(
                    [module.ModuleId],
                    _composition.ConnectionName,
                    _userId,
                    cancellationToken).ConfigureAwait(false);
                _composition.Trace?.Invoke(
                    $"seed {module.ModuleId}: {seedPhase.Elapsed.TotalSeconds:0.000} s, {seeded.TotalRecordsInserted} record(s)");

                var moduleResult = seeded.ModuleResults.FirstOrDefault();
                var succeededModule = seeded.ModulesRun == 1 && seeded.ModulesSucceeded == 1;

                var outcome = new OgSetupModuleOutcome(
                    module.ModuleId,
                    succeededModule,
                    seeded.TotalRecordsInserted,
                    succeededModule
                        ? null
                        : moduleResult is { Errors.Count: > 0 }
                            ? string.Join("; ", moduleResult.Errors)
                            : "the module reported errors");

                outcomes.Add(outcome);

                RecordModule(state, outcome);
                _stateStore.Save(state);
            }

            var skipped = outcomes.Count(outcome => outcome.Skipped);
            var attempted = outcomes.Count - skipped;
            var succeeded = outcomes.Count(outcome => outcome.Success);
            var failed = attempted - succeeded;

            var aggregate = new OgSetupOutcome(
                Success: failed == 0 && succeeded > 0,
                ConnectionName: _composition.ConnectionName,
                ModulesRun: outcomes.Count,
                ModulesSucceeded: succeeded,
                RecordsInserted: outcomes.Sum(outcome => outcome.RecordsInserted),
                Modules: outcomes,
                CanResume: failed > 0,
                SchemaMessage: schemaMessage,
                Reference: null);

            state.Completed = aggregate.Success;
            _stateStore.Save(state);

            progress?.Report(new OgSetupProgress("done", string.Empty, outcomes.Count, total, aggregate.RecordsInserted));

            // The run completed even when modules failed: the outcome carries each module's error so the page
            // and the evidence can name what went wrong, and CanResume says the rest can be retried.
            return OgResult<OgSetupOutcome>.Ok(aggregate);
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
    /// <summary>
    /// Applies the schema one module at a time, in plan order.
    /// </summary>
    /// <remarks>
    /// Planning every entity type in the catalogue as one migration does not finish in a usable time (the planner's
    /// work grows faster than the entity count), and one bad entity would stop every module. Each module is therefore planned,
    /// approved and executed on its own scope: the work is proportional to the module, a failure is recorded
    /// against that module and the rest continue, and the order is the plan's order.
    /// </remarks>
    private async Task<OgSetupSchemaResult> ApplySchemaAsync(
        OgSetupPlan plan,
        IProgress<OgSetupProgress>? progress,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? entityTypeNames = null)
    {
        var modules = plan.Modules.Where(module => !module.AlreadyCompleted).ToList();

        if (modules.Count == 0)
        {
            return new OgSetupSchemaResult(
                "Schema already complete: every module was applied by an earlier run.",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }

        progress?.Report(new OgSetupProgress("schema-open", "(schema)", 0, modules.Count, 0));
        _composition.OpenTarget();

        var applied = 0;
        var failures = new List<string>();
        var failedModules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in modules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new OgSetupProgress("schema", module.ModuleId, applied, modules.Count, 0));

            // A module that declares no entity types has no schema of its own (HSE is one: its command binds to
            // PPDM_CORE). That is not a failure - there is nothing to apply.
            if (module.EntityTypeCount == 0)
            {
                applied++;
                _composition.Trace?.Invoke($"schema {module.ModuleId}: no entity types, nothing to apply");
                continue;
            }

            var message = await ApplySchemaForModulesAsync(
                [module.ModuleId], modules.Count, progress, cancellationToken, entityTypeNames).ConfigureAwait(false);

            if (message.StartsWith("Schema applied", StringComparison.Ordinal))
            {
                applied++;
            }
            else
            {
                failures.Add($"{module.ModuleId}: {message}");
                failedModules[module.ModuleId] = message;
                _composition.Trace?.Invoke($"schema failure {module.ModuleId}: {message}");
            }
        }

        progress?.Report(new OgSetupProgress("schema-done", "(schema)", applied, modules.Count, 0));

        return new OgSetupSchemaResult(
            failures.Count == 0
                ? $"Schema applied for {applied} module(s)."
                : $"Schema applied for {applied} of {modules.Count} module(s); {failures.Count} failed: " +
                  string.Join(" | ", failures.Take(10)),
            failedModules);
    }

    private async Task<string> ApplySchemaForModulesAsync(
        IReadOnlyList<string> moduleIds,
        int total,
        IProgress<OgSetupProgress>? progress,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? entityTypeNames = null)
    {
        // PPDM_CORE resolves to the whole PPDM catalogue (thousands of entity types). One plan of that size cannot be
        // hashed, copied or persisted in a usable time, so the catalogue is applied in bounded batches by naming the
        // types of each batch; a module that fits in one batch keeps the single-plan path.
        if (entityTypeNames is null)
        {
            using var sizingScope = _composition.CreateScope();
            var sizing = sizingScope.ServiceProvider.GetRequiredService<IPPDM39SetupService>();

            var resolved = sizing.ResolvedEntityTypeNames(moduleIds);

            if (resolved.Count > SchemaBatchSize)
            {
                var batches = resolved.Chunk(SchemaBatchSize).ToList();
                var applied = 0;

                _composition.Trace?.Invoke(
                    $"runner:batched {moduleIds[0]}: {resolved.Count} type(s) in {batches.Count} batch(es)");

                for (var index = 0; index < batches.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    progress?.Report(new OgSetupProgress(
                        $"schema-batch {index + 1}/{batches.Count}", moduleIds[0], index, batches.Count, 0));

                    var message = await ApplySchemaForModulesAsync(
                            moduleIds, total, progress, cancellationToken, batches[index])
                        .ConfigureAwait(false);

                    if (!message.StartsWith("Schema applied", StringComparison.Ordinal))
                    {
                        return $"Schema apply failed in batch {index + 1} of {batches.Count}: {message}";
                    }

                    applied++;
                }

                return $"Schema applied: {applied} batch(es) over {resolved.Count} entity type(s) for {moduleIds[0]}.";
            }
        }

        // Each module is its own migration unit; the identity also keeps the manager's execution claim for one
        // module from blocking the next (see OgSetupComposition.MigrationScope).
        _composition.MigrationScope = moduleIds[0];

        progress?.Report(new OgSetupProgress("schema-plan", moduleIds[0], 0, total, 0));
        _composition.Trace?.Invoke($"runner:plan-call {moduleIds[0]}");

        // A high-risk operation needs backup/restore evidence before it may run. For the local SQLite database the
        // evidence is a real file copy taken now; for a remote connection there is nothing this process can copy,
        // and the readiness check keeps refusing - correctly, because no backup exists.
        var backupPath = BackupLocalDatabase();
        _composition.Trace?.Invoke($"schema backup: {backupPath ?? "(none)"}");

        using var scope = _composition.CreateScope();
        var setup = scope.ServiceProvider.GetRequiredService<IPPDM39SetupService>();

        var plannedSchema = await setup.PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest
        {
            ConnectionName = _composition.ConnectionName,
            ModuleIds = moduleIds.ToList(),
            EntityTypeNames = entityTypeNames?.ToList(),
            EnvironmentTier = "Development",
            PreparedBy = _userId,
            BackupConfirmed = backupPath is not null,
            RestoreTestEvidenceProvided = backupPath is not null,
            RestoreTestEvidence = backupPath,
        }).ConfigureAwait(false);

        if (!plannedSchema.Success)
        {
            var gates = plannedSchema.CiGates
                .Where(gate => !string.Equals(gate.Decision, "Pass", StringComparison.OrdinalIgnoreCase))
                .Select(gate => $"{gate.Gate}={gate.Decision}: {gate.Message}")
                .ToList();

            var checks = plannedSchema.PreflightChecks
                .Where(check => !string.Equals(check.Decision, "Pass", StringComparison.OrdinalIgnoreCase))
                .Select(check => $"{check.Code}={check.Decision}: {check.Message}")
                .ToList();

            var findings = plannedSchema.PolicyFindings
                .Where(finding => !string.Equals(finding.Decision, "Allow", StringComparison.OrdinalIgnoreCase))
                .Select(finding =>
                    $"{finding.RuleId}={finding.Decision} ({finding.RiskLevel}) {finding.Message}" +
                    (string.IsNullOrWhiteSpace(finding.EntityName) ? string.Empty : $" [{finding.EntityName}]"))
                .ToList();

            var detail = string.Concat(
                gates.Count == 0 ? string.Empty : $" Blocking gates: {string.Join("; ", gates)}.",
                checks.Count == 0 ? string.Empty : $" Failing checks: {string.Join("; ", checks)}.",
                findings.Count == 0 ? string.Empty : $" Policy findings: {string.Join("; ", findings)}.");

            return $"Schema plan not produced: {plannedSchema.Message}{detail}";
        }

        if (!plannedSchema.CanApply)
        {
            _composition.Trace?.Invoke($"schema-plan not applicable: {plannedSchema.PolicyDecision}");
            return $"Schema plan requires {plannedSchema.PendingOperationCount} operation(s) but is not applicable: {plannedSchema.PolicyDecision}";
        }

        progress?.Report(new OgSetupProgress("schema-approve", moduleIds[0], 0, total, 0));

        // The service refuses to execute an unapproved plan. The desktop is the client here: it plans, the
        // signed-in user approves, and this run executes what was approved - the same order the API enforces.
        var approved = await setup.ApproveSchemaMigrationPlanAsync(new SchemaMigrationApprovalRequest
        {
            PlanId = plannedSchema.PlanId,
            ExpectedPlanHash = plannedSchema.PlanHash,
            ExpectedManifestHash = plannedSchema.ManifestHash,
            ApprovedBy = _userId,
            Notes = "Desktop module setup: plan approved for this installation's bound connection.",
        }).ConfigureAwait(false);

        if (!approved.Success)
        {
            return $"Schema plan not approved: {approved.Message}";
        }

        progress?.Report(new OgSetupProgress("schema-execute", moduleIds[0], 0, total, 0));

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
    /// <summary>
    /// Copies the local SQLite database before a schema change, and returns the copy's path. Null when the
    /// connection is not a local file database, so the caller passes no backup evidence rather than a claim.
    /// </summary>
    private string? BackupLocalDatabase()
    {
        var source = _localDatabasePath?.Invoke();

        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
        {
            return null;
        }

        try
        {
            var directory = Path.Combine(Path.GetDirectoryName(source)!, "backups");
            Directory.CreateDirectory(directory);
            var target = Path.Combine(
                directory,
                $"{_composition.ConnectionName}-{DateTime.Now:yyyyMMdd-HHmmss}.db");
            File.Copy(source, target, overwrite: false);
            return target;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _failures.ReportHandled(
                ex,
                "copying the local database before a schema change",
                "the schema change will be refused for want of backup evidence",
                FailureSeverity.Degraded);
            return null;
        }
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
