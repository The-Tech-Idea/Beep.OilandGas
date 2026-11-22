using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-10 AC-7: the shared environment is composed from a country, a unit system and the company's own overrides;
/// a saved override round-trips, the engine can read the document, and a stale write is refused.
/// </summary>
public sealed class EnvironmentEditorTests
{
    private const string ConnectionName = "environment-probe";

    [Fact]
    public void An_override_wins_over_the_preset_and_the_unit_system()
    {
        var composed = OgEnvironmentDocument.Compose(
            "AE",
            "oilfield",
            "{\"units\":{\"system\":\"metric-kpa\"},\"printing\":{\"paper\":\"A3\"}}");

        Assert.True(composed.Success, composed.Message);

        var (country, unitSystem, overrides) = OgEnvironmentDocument.Read(composed.Value);

        Assert.Equal("AE", country);
        Assert.Equal("metric-kpa", unitSystem);
        Assert.NotNull(overrides);
        Assert.Contains("paper", overrides!, StringComparison.Ordinal);
        Assert.DoesNotContain("\"system\"", overrides!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_document_is_one_the_engine_can_read()
    {
        var composed = OgEnvironmentDocument.Compose("AE", "oilfield", null);
        Assert.True(composed.Success, composed.Message);

        var environment = WellboreEnvironment.FromJson(composed.Value!, out var warnings);

        Assert.Empty(warnings);
        Assert.Equal("AE", environment.Region.Country);
        Assert.Equal("oilfield", environment.Units.System);
    }

    [Fact]
    public void Overrides_that_are_not_a_json_object_are_refused_with_a_sentence()
    {
        var notAnObject = OgEnvironmentDocument.Compose("AE", "oilfield", "[1,2,3]");
        Assert.False(notAnObject.Success);
        Assert.Equal(OgEnvironmentDocument.CodeOverridesInvalid, notAnObject.Reference);

        var malformed = OgEnvironmentDocument.Compose("AE", "oilfield", "{ not json");
        Assert.False(malformed.Success);
        Assert.Equal(OgEnvironmentDocument.CodeOverridesInvalid, malformed.Reference);

        var noCountry = OgEnvironmentDocument.Compose(null, "oilfield", null);
        Assert.False(noCountry.Success);
        Assert.Equal(OgEnvironmentDocument.CodeCountryMissing, noCountry.Reference);
    }

    [Fact]
    public async Task A_saved_override_round_trips_through_the_database()
    {
        await WithDatabase(async (reporter, settings, audit) =>
        {
            var composed = OgEnvironmentDocument.Compose(
                "AE",
                "oilfield",
                "{\"printing\":{\"paper\":\"A3\"},\"units\":{\"system\":\"metric-bar\"}}");

            Assert.True(composed.Success, composed.Message);

            var saved = await settings.WriteToolsEnvironmentAsync(composed.Value!, string.Empty, "sysadmin");

            Assert.True(saved.Success, saved.Message);

            var read = await settings.ReadAsync();
            Assert.True(read.Success, read.Message);
            Assert.Equal(composed.Value, read.Value!.ToolsEnvironmentJson);

            var (country, unitSystem, overrides) = OgEnvironmentDocument.Read(read.Value.ToolsEnvironmentJson);

            Assert.Equal("AE", country);
            Assert.Equal("metric-bar", unitSystem);
            Assert.Contains("paper", overrides!, StringComparison.Ordinal);

            // The engine reads what was stored, with the override applied.
            var environment = WellboreEnvironment.FromJson(read.Value.ToolsEnvironmentJson!, out var warnings);

            Assert.Empty(warnings);
            Assert.Equal("metric-bar", environment.Units.System);

            // The save wrote its own audit row (SETUP-10 AC-5), so the editor does not need to.
            Assert.Contains(audit.Rows, row =>
                row.Verb == "settings.save"
                && row.SubjectId == OgSharedSetup.ToolsEnvironmentId
                && row.Outcome == "ok");
        });
    }

    [Fact]
    public async Task A_stale_environment_write_is_refused_and_the_stored_document_is_unchanged()
    {
        await WithDatabase(async (reporter, settings, audit) =>
        {
            var first = OgEnvironmentDocument.Compose("AE", "oilfield", null);
            Assert.True(first.Success, first.Message);

            var saved = await settings.WriteToolsEnvironmentAsync(first.Value!, string.Empty, "sysadmin");
            Assert.True(saved.Success, saved.Message);

            var second = OgEnvironmentDocument.Compose("SA", "metric-mpa", null);
            Assert.True(second.Success, second.Message);

            // The same expected revision again: another administrator saved in between.
            var stale = await settings.WriteToolsEnvironmentAsync(second.Value!, string.Empty, "second-admin");

            Assert.False(stale.Success);
            Assert.Equal(OgSharedSetupCodes.Stale, stale.Reference);

            var read = await settings.ReadAsync();
            var (country, _, _) = OgEnvironmentDocument.Read(read.Value!.ToolsEnvironmentJson);

            Assert.Equal("AE", country);
            Assert.Equal(first.Value, read.Value.ToolsEnvironmentJson);
        });
    }

    // ------------------------------------------------------------------ fixture

    private static async Task WithDatabase(
        Func<RecordingFailureReporter, OgSharedSetupService, RecordingAudit, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "environment-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();
        var audit = new RecordingAudit();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEnvironment"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "environment.db"));

            Assert.True(created.Success, created.Message);

            var store = new BeepDmSharedSetupStore(host, reporter, ConnectionName);
            var settings = new OgSharedSetupService(store, reporter, audit);

            await test(reporter, settings, audit);
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
