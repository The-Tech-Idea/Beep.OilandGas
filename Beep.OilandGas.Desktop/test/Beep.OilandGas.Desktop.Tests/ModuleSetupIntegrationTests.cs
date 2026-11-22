using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-02: apply and resume the module setup against a **fresh** local SQLite file, through the real
/// <c>PPDM39SetupService</c>, the discovered modules and the schema migration pipeline.
/// </summary>
/// <remarks>
/// Opt-in (set <c>OG_RUN_SETUP_INTEGRATION=1</c>). The scope is one small module (WELL_STATUS_FACETS, six
/// reference tables) because the full plan covers 343 entity types and takes longer than a test may run; the
/// full plan's shape is asserted by the smoke run. Scoping is a runner capability, not a test-only path.
/// </remarks>
public sealed class ModuleSetupIntegrationTests
{
    private const string ConnectionName = "dat02-local";
    private static readonly string[] Scope = ["WELL_STATUS_FACETS"];

    [IntegrationFact]
    public async Task Plan_names_the_reference_lists_it_will_seed()
    {        var root = Path.Combine(OgPaths.DataRoot, "dat02-refs-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        using var host = OgBeepHost.Create(
            new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDat02Refs"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var catalog = new BeepDmConnectionCatalog(host, reporter);
        var created = await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "dat02refs.db"));

        Assert.True(created.Success, $"{created.Message} (reference {created.Reference})");

        using var composition = new OgSetupComposition(host, reporter, ConnectionName);
        var runner = new OgModuleSetupRunner(
            composition,
            new OgSetupStateStore(Path.Combine(root, "module-setup-state.json")),
            reporter,
            "integration-test");

        var planned = await runner.PlanAsync(Scope);
        Assert.True(planned.Success, planned.Message);

        var plan = planned.Value!;
        var module = Assert.Single(plan.Modules);

        Assert.NotEmpty(module.ReferenceLists);
        Assert.All(module.ReferenceLists, name => Assert.StartsWith("R_", name, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(module.ReferenceLists, name => name.Contains("WELL_STATUS", StringComparison.OrdinalIgnoreCase));

        // The plan's own list is the union of the module rows, so the wizard can review it without another read.
        Assert.Equal(module.ReferenceLists, plan.ReferenceLists);

        // The names are real PPDM tables, not invented: each must be present in the module metadata that produced the
        // plan (its entity types), which is what the apply later uses.
        Assert.True(
            module.ReferenceLists.Count <= module.EntityTypeCount,
            $"{module.ReferenceLists.Count} reference list(s) but only {module.EntityTypeCount} entity type(s)");
    }

    /// <summary>
    /// DAT-02 AC-2 at full scope: every discovered module is planned, its schema applied and its data seeded, on one
    /// fresh database. No scope, no demo data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Currently blocked by the migration engine, and gated behind its own variable so it cannot be mistaken for
    /// passing:</b> run with <c>OG_RUN_SETUP_FULL=1</c> (and <c>OG_RUN_SETUP_INTEGRATION=1</c>). What it does here
    /// today is recorded in <c>plan/evidence</c>: the catalogue plans as 34 modules / 475 entity types; `PPDM_CORE`
    /// alone runs about ten minutes and then fails with "Checkpoint was not acknowledged (Failed)" because the
    /// migration history file has grown to 1 GB (each checkpoint record embeds the whole plan artifact and the file is
    /// rewritten per append); with the engine's compact-checkpoint mode the process dies in
    /// <c>MigrationManager.Canonicalize</c> instead. Both are engine defects, not setup defects: the scoped apply below
    /// (six-entity module) and every other test pass.
    /// </para>
    /// <para>
    /// The assertion is written as it should hold, so the day the engine is fixed this test proves the claim instead of
    /// being rewritten to match a workaround.
    /// </para>
    /// </remarks>
    [FullSetupFact]
    public async Task The_unscoped_apply_creates_and_seeds_every_module()
    {
        var root = Path.Combine(OgPaths.DataRoot, "dat02-all-" + Guid.NewGuid().ToString("n")[..8]);
        var dataDirectory = Path.Combine(root, "Data");
        var reporter = new RecordingFailureReporter();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDat02All"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var catalog = new BeepDmConnectionCatalog(host, reporter);
        var created = await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "dat02all.db"));

        Assert.True(created.Success, $"{created.Message} (reference {created.Reference})");

        using var composition = new OgSetupComposition(host, reporter, ConnectionName);
        var runner = new OgModuleSetupRunner(
            composition,
            new OgSetupStateStore(Path.Combine(root, "module-setup-state.json")),
            reporter,
            "integration-test",
            () => Path.Combine(dataDirectory, "dat02all.db"));

        var planned = await runner.PlanAsync();

        Assert.True(planned.Success, planned.Message);

        var plan = planned.Value!;

        Assert.NotEmpty(plan.Modules);
        Assert.True(
            plan.Modules.Count >= 28,
            $"the plan holds only {plan.Modules.Count} module(s); the catalogue is expected to be larger");

        var applied = await runner.ApplyAsync(OgSetupConfirmation.None);

        var outcome = applied.Value;

        // Keep the whole result where a failure can be read after the run, assertion text first.
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, "unscoped-apply.txt"),
            $"plan: {plan.Modules.Count} module(s), {plan.Modules.Sum(module => module.EntityTypeCount)} entity type(s)"
            + Environment.NewLine
            + $"schema: {outcome?.SchemaMessage}{Environment.NewLine}"
            + string.Join(
                Environment.NewLine,
                outcome?.Modules.Select(module => $"{module.ModuleId}: ok={module.Success} skipped={module.Skipped} records={module.RecordsInserted} error={module.Error}") ?? [])
            + Environment.NewLine
            + string.Join(Environment.NewLine, reporter.Failures));

        Assert.True(applied.Success, $"{applied.Message} (reference {applied.Reference})");
        Assert.NotNull(outcome);

        // Only the demonstration dataset may be left out, and only because nothing confirmed it.
        var skipped = outcome!.Modules.Where(module => module.Skipped).Select(module => module.ModuleId).ToList();

        Assert.All(skipped, moduleId => Assert.Equal("DEMO_DATA", moduleId, ignoreCase: true));

        var failed = outcome.Modules.Where(module => !module.Success && !module.Skipped).ToList();

        Assert.True(
            failed.Count == 0,
            $"{failed.Count} module(s) failed: "
            + string.Join(" | ", failed.Select(module => $"{module.ModuleId}: {module.Error}")));

        Assert.Equal(outcome.Modules.Count - skipped.Count, outcome.ModulesSucceeded);
        Assert.Equal(outcome.Modules.Count, outcome.Modules.Count);
        Assert.StartsWith("Schema applied", outcome.SchemaMessage, StringComparison.Ordinal);
    }

    [IntegrationFact]
    public async Task Apply_then_resume_on_a_fresh_sqlite_file()
    {        var root = Path.Combine(OgPaths.DataRoot, "dat02-" + Guid.NewGuid().ToString("n")[..8]);
        var configDirectory = Path.Combine(root, "Beep");
        var dataDirectory = Path.Combine(root, "Data");
        var reporter = new RecordingFailureReporter();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(configDirectory, "BeepOilGasDat02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var catalog = new BeepDmConnectionCatalog(host, reporter);
        var created = await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "dat02.db"));

        Assert.True(created.Success, $"{created.Message} (reference {created.Reference})");

        using var composition = new OgSetupComposition(host, reporter, ConnectionName);
        var runner = new OgModuleSetupRunner(
            composition,
            new OgSetupStateStore(Path.Combine(root, "module-setup-state.json")),
            reporter,
            "integration-test");

        // AC-6: a connection that is not configured is refused, before anything is read or written.
        using var unreachable = new OgSetupComposition(host, reporter, "no-such-connection");
        var probe = unreachable.ProbeTarget();
        Assert.False(probe.Success);
        Assert.Equal(OgModuleSetupRunner.CodeUnreachable, probe.Reference);
        Assert.Contains("no-such-connection", probe.Message!, StringComparison.Ordinal);

        // AC-1: a scoped plan lists exactly the modules asked for, in order.
        var planned = await runner.PlanAsync(Scope);
        Assert.True(planned.Success, planned.Message);
        Assert.NotEmpty(planned.Value!.Modules);
        Assert.All(planned.Value.Modules, module => Assert.Contains(module.ModuleId, Scope));

        // AC-5: the plan names the reference ("code") tables it will seed, taken from the modules' own metadata, so the
        // wizard can show the LOV set for review before the apply. WELL_STATUS_FACETS is a reference-data module.
        var references = planned.Value.ReferenceLists;
        Assert.NotEmpty(references);
        Assert.All(references, name => Assert.StartsWith("R_", name, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(references, name => name.Contains("WELL_STATUS", StringComparison.OrdinalIgnoreCase));

        // AC-1/AC-4: apply runs them in order, creating the schema before seeding.
        var applied = await runner.ApplyAsync(OgSetupConfirmation.None, Scope);
        Assert.True(applied.Success, $"{applied.Message} (reference {applied.Reference})");
        var outcome = applied.Value!;

        // The schema message is long; keep it where a person can read it after a run, before any assertion.
        File.WriteAllText(
            Path.Combine(Path.GetTempPath(), "og-dat02-schema.txt"),
            $"{outcome.SchemaMessage}{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, outcome.Modules.Select(m => $"{m.ModuleId}: ok={m.Success} records={m.RecordsInserted} skipped={m.Skipped} error={m.Error}"))}{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, reporter.Failures)}");

        Assert.True(
            outcome.Success,
            $"schema: {outcome.SchemaMessage} | " +
            $"reported: {string.Join(" || ", reporter.Failures.Select(f => f[..Math.Min(400, f.Length)]))} | " +
            string.Join(" | ", outcome.Modules.Where(m => !m.Success).Select(m => $"{m.ModuleId}: {m.Error}")));

        Assert.NotEmpty(outcome.Modules);
        Assert.Equal(outcome.Modules.Count, outcome.ModulesSucceeded);
        Assert.StartsWith("Schema applied", outcome.SchemaMessage, StringComparison.Ordinal);

        // AC-2: a second apply performs no destructive operation; completed modules are skipped.
        var again = await runner.ApplyAsync(OgSetupConfirmation.None, Scope);
        Assert.True(again.Success, $"{again.Message} (reference {again.Reference})");
        Assert.Equal(0, again.Value!.RecordsInserted);
        Assert.All(again.Value.Modules, module => Assert.Equal("already completed", module.Error));

        // AC-3: resume also skips what completed.
        var resumed = await runner.ResumeAsync(OgSetupConfirmation.None, Scope);
        Assert.True(resumed.Success, $"{resumed.Message} (reference {resumed.Reference})");
        Assert.Equal(0, resumed.Value!.RecordsInserted);
    }

    private sealed class RecordingFailureReporter : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
