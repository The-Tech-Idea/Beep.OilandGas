using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// A migration plan survives a restart: the artifact is persisted when it is built, and a **new
/// <see cref="MigrationManager"/></b> over the same datasource can load it back by its plan id with the same identity.
/// </summary>
/// <remarks>
/// <para>
/// This is the test that a change to the checkpoint payload must satisfy before it ships: the checkpoint record is how a
/// restarted process recovers the plan, so any design that stops carrying the plan in the record has to leave the plan
/// recoverable through the artifact. It needs a real database (the history is written through BeepDM's config storage),
/// so it is opt-in like the other gateway tests.
/// </para>
/// <para>
/// It deliberately does not execute the plan: execution is covered by the scoped apply test, and this test's subject is
/// the persisted identity, not DDL.
/// </para>
/// </remarks>
public sealed class MigrationResumeTests
{
    private const string ConnectionName = "resume-probe";

    [IntegrationFact]
    public async Task A_plan_built_by_one_manager_is_loaded_by_a_second_one_after_a_restart()
    {
        var root = Path.Combine(OgPaths.DataRoot, "resume-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasResume"));

        try
        {
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "resume.db"));

            Assert.True(created.Success, $"{created.Message} ({created.Reference})");

            var source = host.Editor.GetDataSource(ConnectionName);
            Assert.NotNull(source);

            // ---- first process: build the plan (which persists its artifact) -------------------------
            var first = new MigrationManager(host.Editor, source);
            var plan = first.BuildMigrationPlanForTypes([typeof(R_WELL_STATUS_TYPE)]);

            Assert.NotNull(plan);
            Assert.False(string.IsNullOrWhiteSpace(plan.PlanId));
            Assert.False(string.IsNullOrWhiteSpace(plan.PlanHash));
            Assert.NotEmpty(plan.Operations);

            // ---- new manager over the same datasource: the restart -----------------------------------
            var restarted = new MigrationManager(host.Editor, source);

            var reloaded = restarted.LoadMigrationPlan(plan.PlanId);

            Assert.NotNull(reloaded);
            Assert.Equal(plan.PlanId, reloaded.PlanId);
            Assert.Equal(plan.PlanHash, reloaded.PlanHash);
            Assert.Equal(plan.PlanHashVersion, reloaded.PlanHashVersion);
            Assert.Equal(plan.Operations.Count, reloaded.Operations.Count);
            Assert.Equal(plan.DataSourceName, reloaded.DataSourceName);
        }
        finally
        {
            TryDelete(root);
        }
    }

    private static void TryDelete(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (IOException)
        {
            // A provider may still hold the file; the test root is disposable either way.
        }
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
            Failures.Add($"{severity}: {operation} - {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
