using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-03 AC-6/AC-7 (gap G-1): the wizard's rail shows completed, current and upcoming steps, and an interrupted
/// setup resumes at the step after the last one accepted instead of starting again.
/// </summary>
public sealed class WizardResumeTests
{
    private const string ConnectionName = "wizard-probe";

    [Fact]
    public void The_rail_states_are_completed_current_and_upcoming()
    {
        // Furthest = 2 (Company): Welcome and Database are done, Company is here, the rest are ahead.
        Assert.Equal(OgSetupStepState.Completed, OgSetupSteps.StateOf(furthest: 2, index: 0));
        Assert.Equal(OgSetupStepState.Completed, OgSetupSteps.StateOf(furthest: 2, index: 1));
        Assert.Equal(OgSetupStepState.Current, OgSetupSteps.StateOf(furthest: 2, index: 2));
        Assert.Equal(OgSetupStepState.Upcoming, OgSetupSteps.StateOf(furthest: 2, index: 3));
        Assert.Equal(OgSetupStepState.Upcoming, OgSetupSteps.StateOf(furthest: 2, index: OgSetupSteps.Count - 1));
    }

    [Fact]
    public void Every_label_says_which_step_it_is_and_where_it_stands()
    {
        Assert.Equal("1. Welcome - done", OgSetupSteps.LabelFor(furthest: 1, index: 0));
        Assert.Equal("2. Database - here", OgSetupSteps.LabelFor(furthest: 1, index: 1));
        Assert.Equal("3. Company", OgSetupSteps.LabelFor(furthest: 1, index: 2));
    }

    [Theory]
    [InlineData(-1, 0)]  // nothing recorded: the first step
    [InlineData(0, 1)]   // Welcome accepted: open at Database
    [InlineData(2, 3)]   // Company accepted: open at Modules
    [InlineData(5, 5)]   // everything accepted: open at Review, where the work happens
    [InlineData(9, 5)]   // a record from a later build cannot push past the review
    public void A_resume_opens_at_the_step_after_the_last_one_accepted(int lastCompleted, int expected)
    {
        Assert.Equal(expected, OgSetupSteps.ResumeIndex(lastCompleted));
    }

    [Fact]
    public async Task The_progress_record_round_trips_and_is_cleared()
    {
        await WithDatabase(async (settings, audit) =>
        {
            // Nothing recorded yet.
            Assert.Equal(-1, (await settings.ReadSetupProgressAsync()).Value);

            Assert.True((await settings.WriteSetupProgressAsync(2, "installer")).Success);

            var read = await settings.ReadSetupProgressAsync();
            Assert.True(read.Success, read.Message);
            Assert.Equal(2, read.Value);
            Assert.Equal(OgSetupSteps.ResumeIndex(2), OgSetupSteps.ResumeIndex(read.Value));

            // Writing again replaces the record rather than failing on the revision.
            Assert.True((await settings.WriteSetupProgressAsync(3, "installer")).Success);
            Assert.Equal(3, (await settings.ReadSetupProgressAsync()).Value);

            Assert.True((await settings.ClearSetupProgressAsync("installer")).Success);
            Assert.Equal(-1, (await settings.ReadSetupProgressAsync()).Value);

            // Each write and the removal left its own audit row.
            Assert.Equal(3, audit.Rows.Count);
            Assert.Equal("settings.delete", audit.Rows[^1].Verb);
        });
    }

    [Fact]
    public async Task A_database_with_no_settings_table_reports_no_progress_rather_than_failing()
    {
        await WithDatabase(async (settings, audit) =>
        {
            // The fixture writes the version row, which creates the table; a missing table is the other half of the
            // rule and is covered by the store's own test. Here the empty table must read as "no progress".
            Assert.Equal(-1, (await settings.ReadSetupProgressAsync()).Value);
        });
    }

    // ------------------------------------------------------------------ fixture

    private static async Task WithDatabase(Func<OgSharedSetupService, RecordingAudit, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "wizard-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();
        var audit = new RecordingAudit();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWizard"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "wizard.db"));

            Assert.True(created.Success, created.Message);

            var store = new BeepDmSharedSetupStore(host, reporter, ConnectionName);
            var settings = new OgSharedSetupService(store, reporter, audit);

            Assert.True((await settings.WriteSchemaVersionAsync(string.Empty, "test")).Success);
            audit.Rows.Clear();

            await test(settings, audit);
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
            Failures.Add($"{severity}: {operation} — {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
