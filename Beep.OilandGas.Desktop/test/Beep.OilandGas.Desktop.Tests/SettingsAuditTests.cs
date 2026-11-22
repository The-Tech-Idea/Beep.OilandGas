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
/// SETUP-10 AC-5: the audit row belongs to the settings save, not to a page's notification. A successful save and a
/// stale-refused save each leave exactly one row, written by the same operation that attempted the write.
/// </summary>
public sealed class SettingsAuditTests
{
    private const string ConnectionName = "settings-audit-probe";

    [Fact]
    public async Task A_successful_save_writes_one_audit_row_with_the_previous_revision()
    {
        await WithDatabase(async (reporter, settings, audit) =>
        {
            var saved = await settings.WriteInstallationAsync(Company("Probe Energy"), string.Empty, "admin");

            Assert.True(saved.Success, saved.Message);

            var row = Assert.Single(audit.Rows);

            Assert.Equal("settings.save", row.Verb);
            Assert.Equal("settings", row.SubjectKind);
            Assert.Equal(OgSharedSetup.CompanyId, row.SubjectId);
            Assert.Equal("ok", row.Outcome);
            Assert.Contains("previous=(none)", row.Detail!, StringComparison.Ordinal);
            Assert.Contains(saved.Value!.Revision, row.Detail!, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task A_stale_save_writes_one_refused_row_and_nothing_changes()
    {
        await WithDatabase(async (reporter, settings, audit) =>
        {
            var first = await settings.WriteInstallationAsync(Company("Probe Energy"), string.Empty, "admin");
            Assert.True(first.Success, first.Message);

            audit.Rows.Clear();

            // The same expected revision again: another workstation saved in between.
            var stale = await settings.WriteInstallationAsync(Company("Other Company"), string.Empty, "second-admin");

            Assert.False(stale.Success);
            Assert.Equal(OgSharedSetupCodes.Stale, stale.Reference);

            var row = Assert.Single(audit.Rows);

            Assert.Equal("settings.save", row.Verb);
            Assert.Equal("refused", row.Outcome);
            Assert.Equal(OgSharedSetup.CompanyId, row.SubjectId);
            Assert.Contains(OgSharedSetupCodes.Stale, row.Detail!, StringComparison.Ordinal);

            var stored = await settings.ReadAsync();
            Assert.Equal("Probe Energy", stored.Value!.Installation!.CompanyName);
        });
    }

    [Fact]
    public async Task Each_document_records_its_own_row()
    {
        await WithDatabase(async (reporter, settings, audit) =>
        {
            Assert.True((await settings.WriteInstallationAsync(Company("Probe Energy"), string.Empty, "admin")).Success);

            Assert.True((await settings.WriteToolsEnvironmentAsync(
                Setup.OgSetupCatalog.BuildEnvironmentDocument("AE", "oilfield")!, string.Empty, "admin")).Success);

            Assert.True((await settings.WriteModulesAsync(["WELL"], string.Empty, "admin")).Success);

            Assert.Equal(3, audit.Rows.Count);
            Assert.Equal(
                [OgSharedSetup.CompanyId, OgSharedSetup.ToolsEnvironmentId, OgSharedSetup.ModulesId],
                audit.Rows.Select(row => row.SubjectId));
            Assert.All(audit.Rows, row =>
            {
                Assert.Equal("settings.save", row.Verb);
                Assert.Equal("ok", row.Outcome);
            });
        });
    }

    [Fact]
    public async Task A_removed_document_records_its_own_row()
    {
        await WithDatabase(async (reporter, settings, audit) =>
        {
            var saved = await settings.WriteInstallationAsync(Company("Probe Energy"), string.Empty, "admin");
            Assert.True(saved.Success, saved.Message);

            audit.Rows.Clear();

            var removed = await settings.DeleteDocumentAsync(
                OgSharedSetup.CompanyId,
                saved.Value!.Revision,
                "rollback");

            Assert.True(removed.Success, removed.Message);

            var row = Assert.Single(audit.Rows);

            Assert.Equal("settings.delete", row.Verb);
            Assert.Equal("ok", row.Outcome);
            Assert.Contains(saved.Value.Revision, row.Detail!, StringComparison.Ordinal);
        });
    }

    // ------------------------------------------------------------------ fixture

    private static OgInstallationSettings Company(string name) =>
        new(
            CompanyName: name,
            CompanyCode: "PROBE",
            CountryCode: "AE",
            CultureName: "en-US",
            TimeZoneId: "UTC",
            BaseCurrencyCode: "USD",
            FiscalYearStartMonth: 1,
            CompanyTypeCode: "OPERATOR",
            UnitSystemCode: "oilfield",
            DefaultFieldName: "North Field",
            RecordedBy: "admin",
            RecordedAtUtc: new DateTimeOffset(2026, 11, 9, 8, 0, 0, TimeSpan.Zero));

    private static async Task WithDatabase(
        Func<RecordingFailureReporter, OgSharedSetupService, RecordingAudit, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "settings-audit-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();
        var audit = new RecordingAudit();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasSettingsAudit"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "audit.db"));

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
