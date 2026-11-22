using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
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
/// DAT-02 AC-1 / REL-04 <c>module-setup</c>: measures the unscoped apply (every discovered module) and records
/// where the time goes, module by module.
/// </summary>
/// <remarks>
/// Opt-in: <c>OG_RUN_SETUP_PERF=1</c>. The run stops itself after <c>OG_SETUP_GUARD_MINUTES</c> (default 8) so a
/// slow pipeline cannot occupy a machine indefinitely; the report says how far it got. The output is written to
/// <c>%TEMP%/og-setup-perf.txt</c>, one line per module in completion order plus the plan and schema totals.
/// </remarks>
public sealed class ModuleSetupPerformanceTests
{
    private const string ConnectionName = "dat02-perf";

    [Fact]
    public async Task Unscoped_apply_records_where_the_time_goes()
    {
        if (Environment.GetEnvironmentVariable("OG_RUN_SETUP_PERF") != "1")
        {
            return;
        }

        var guard = double.TryParse(
            Environment.GetEnvironmentVariable("OG_SETUP_GUARD_MINUTES"), out var minutes)
            ? minutes
            : 8;

        // OG_SETUP_REUSE_ROOT points the run at an existing test database, so a module can be planned a second
        // time against the schema an earlier run created.
        var reuse = Environment.GetEnvironmentVariable("OG_SETUP_REUSE_ROOT");
        var root = string.IsNullOrWhiteSpace(reuse)
            ? Path.Combine(OgPaths.DataRoot, "perf-" + Guid.NewGuid().ToString("n")[..8])
            : Path.Combine(OgPaths.DataRoot, reuse);
        var reporter = new RecordingFailureReporter();
        var report = new List<string>();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPerf"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var catalog = new BeepDmConnectionCatalog(host, reporter);
        var created = await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "perf.db"));

        Assert.True(created.Success, created.Message);

        // Diagnostic: how the cost of applying one module changes with the number of tables already present. The
        // tables are empty and plainly named, so nothing about the module itself changes between runs.
        var prefill = int.TryParse(
            Environment.GetEnvironmentVariable("OG_SETUP_PREFILL_TABLES"), out var tablesToPreCreate)
            ? tablesToPreCreate
            : 0;

        var prefillColumns = int.TryParse(
            Environment.GetEnvironmentVariable("OG_SETUP_PREFILL_COLUMNS"), out var columnsToPreCreate)
            ? Math.Clamp(columnsToPreCreate, 2, 60)
            : 2;

        if (prefill > 0)
        {
            var database = Path.Combine(root, "Data", "perf.db");

            using var connection = new SqliteConnection($"Data Source={database}");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            for (var index = 1; index <= prefill; index++)
            {
                using var create = connection.CreateCommand();
                create.Transaction = transaction;
                create.CommandText =
                    $"CREATE TABLE PREFILL_{index:0000} (ID INTEGER NOT NULL PRIMARY KEY, NOTE TEXT NULL);";
                create.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        // Diagnostic: an existing table with a shape the module cannot accept, without the cost of a full core
        // apply. It reproduces the "populated database" the policy gate appears on.
        var extraTable = Environment.GetEnvironmentVariable("OG_SETUP_PREFILL_EXTRA_TABLE");

        if (!string.IsNullOrWhiteSpace(extraTable)
            && System.Text.RegularExpressions.Regex.IsMatch(extraTable, "^[A-Za-z_][A-Za-z0-9_]*$"))
        {
            var database = Path.Combine(root, "Data", "perf.db");
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={database}");
            connection.Open();
            using var create = connection.CreateCommand();
            create.CommandText = $"CREATE TABLE IF NOT EXISTS {extraTable} (JUNK TEXT NULL);";
            create.ExecuteNonQuery();
        }

        var liveTrace = Path.Combine(Path.GetTempPath(), "og-setup-perf-live.txt");

        // The setup service reports its planning phases here, so a slow plan names the engine call that is slow.
        global::Beep.OilandGas.PPDM39.DataManagement.Services.PPDM39SetupService.Trace =
            line => File.AppendAllText(liveTrace, $"{DateTime.Now:HH:mm:ss.fff}  {line}{Environment.NewLine}");
        using var composition = new OgSetupComposition(host, reporter, ConnectionName)
        {
            Trace = line => File.AppendAllText(liveTrace, $"{DateTime.Now:HH:mm:ss.fff}  {line}{Environment.NewLine}"),
        };
        var runner = new OgModuleSetupRunner(
            composition,
            new OgSetupStateStore(Path.Combine(root, "state.json")),
            reporter,
            "perf",
            () => Path.Combine(root, "Data", "perf.db"));

        var scope = (Environment.GetEnvironmentVariable("OG_SETUP_PERF_SCOPE") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        IReadOnlyList<string>? moduleIds = scope.Length == 0 ? null : scope;

        // A named subset of entity types, so a module that cannot be planned whole can be bisected:
        //   OG_SETUP_PERF_TYPES="TYPE_A,TYPE_B"     explicit names
        //   OG_SETUP_PERF_TYPE_RANGE="0:64"         start:count into the module's resolved types
        var typeNames = (Environment.GetEnvironmentVariable("OG_SETUP_PERF_TYPES") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        IReadOnlyList<string>? entityTypeNames = typeNames.Count > 0 ? typeNames : null;

        var typeRange = Environment.GetEnvironmentVariable("OG_SETUP_PERF_TYPE_RANGE");

        if (entityTypeNames is null && !string.IsNullOrWhiteSpace(typeRange))
        {
            var parts = typeRange.Split(':');

            if (parts.Length == 2 && int.TryParse(parts[0], out var start) && int.TryParse(parts[1], out var count))
            {
                using var probe = composition.CreateScope();
                var setup = probe.ServiceProvider.GetRequiredService<
                    global::Beep.OilandGas.Models.Core.Interfaces.IPPDM39SetupService>();

                var resolved = setup.ResolvedEntityTypeNames(moduleIds ?? ["PPDM_CORE"]);
                entityTypeNames = resolved.Skip(Math.Max(0, start)).Take(Math.Max(1, count)).ToList();

                report.Add($"types: {entityTypeNames.Count} of {resolved.Count} (range {start}:{count})");
            }
        }

        if (entityTypeNames is not null)
        {
            report.Add($"types: {entityTypeNames.Count} named");
        }

        var planClock = Stopwatch.StartNew();
        var planned = await runner.PlanAsync(moduleIds);
        planClock.Stop();

        Assert.True(planned.Success, planned.Message);
        var plan = planned.Value!;

        report.Add($"scope: {(moduleIds is null ? "(all)" : string.Join(",", moduleIds))}");
        report.Add($"plan: {planClock.Elapsed.TotalSeconds:0.0} s, {plan.Modules.Count} module(s), {plan.Modules.Sum(m => m.EntityTypeCount)} entity type(s) in scope");
        report.Add($"prefill: {prefill} table(s) x {prefillColumns} column(s) already in the database");

        foreach (var module in plan.Modules)
        {
            report.Add($"plan-module {module.ModuleId}: {module.EntityTypeCount} entity type(s)");
        }

        // Per-module timing from the progress stream: a module's time is the gap between its first report and
        // the next module's first report.
        var moduleTimes = new List<(string Module, double Seconds, int Records)>();
        var overall = Stopwatch.StartNew();
        var current = (Module: string.Empty, StartedAt: 0d, Records: 0);
        var currentPhase = string.Empty;

        void CloseCurrent()
        {
            if (current.Module.Length == 0)
            {
                return;
            }

            moduleTimes.Add((current.Module, overall.Elapsed.TotalSeconds - current.StartedAt, current.Records));
        }

        // Written immediately, so a run that is killed (or stalls) still shows how far it got and where.
        var live = Path.Combine(Path.GetTempPath(), "og-setup-perf-live.txt");
        File.WriteAllText(live, $"started {DateTime.Now:HH:mm:ss}{Environment.NewLine}");

        var progress = new LiveProgress(update =>
        {
            if (!string.Equals(update.ModuleId, current.Module, StringComparison.Ordinal))
            {
                CloseCurrent();
                current = (update.ModuleId, overall.Elapsed.TotalSeconds, update.Records);

                File.AppendAllText(
                    live,
                    $"{DateTime.Now:HH:mm:ss}  {overall.Elapsed.TotalSeconds:0.0} s  {update.Phase} {update.ModuleId} ({update.Completed}/{update.Total}){Environment.NewLine}");
            }
        });

        using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(guard));
        var stoppedEarly = false;

        try
        {
            var applied = await runner.ApplyAsync(
                OgSetupConfirmation.None, moduleIds, entityTypeNames, progress, cancellation.Token);

            if (applied.Value is not null)
            {
                report.Add($"schema: {applied.Value.SchemaMessage}");
            }
            else
            {
                report.Add($"apply: {applied.Message} ({applied.Reference})");
            }
        }
        catch (OperationCanceledException)
        {
            stoppedEarly = true;
        }

        overall.Stop();
        CloseCurrent();

        report.Add($"guard: {guard:0.#} min, stopped early: {stoppedEarly}");
        report.Add($"total: {overall.Elapsed.TotalMinutes:0.0} min over {moduleTimes.Count} module(s)");

        foreach (var (module, seconds, records) in moduleTimes.OrderByDescending(entry => entry.Seconds).Take(15))
        {
            report.Add($"module {module}: {seconds:0.0} s, {records} record(s)");
        }

        foreach (var failure in reporter.Failures.Take(5))
        {
            report.Add($"reported: {failure[..Math.Min(300, failure.Length)]}");
        }

        File.WriteAllLines(Path.Combine(Path.GetTempPath(), "og-setup-perf.txt"), report);
    }

    /// <summary>Reports on the calling thread, so the live file is accurate while the run is in progress.</summary>
    private sealed class LiveProgress : IProgress<OgSetupProgress>
    {
        private readonly Action<OgSetupProgress> _report;

        public LiveProgress(Action<OgSetupProgress> report) => _report = report;

        public void Report(OgSetupProgress value) => _report(value);
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
