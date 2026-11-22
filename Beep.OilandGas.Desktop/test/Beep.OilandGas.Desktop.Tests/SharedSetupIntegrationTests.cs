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
/// The company settings a database shares: the company record, the module set and the Oil &amp; Gas environment,
/// stored as JSON rows in <c>BEEPSETTINGS</c>. The table is created by the first administrator's setup run and read
/// by every user who signs in against that database afterwards.
/// </summary>
public sealed class SharedSetupIntegrationTests
{
    private const string ConnectionName = "settings-probe";

    [Fact]
    public async Task The_company_record_is_created_by_the_first_run_and_a_stale_write_is_refused()
    {
        await WithDatabase(async (host, reporter, service) =>
        {
            // ---- a database that has never been set up says so; reading never creates the table --------
            var before = await service.ReadAsync();
            Assert.False(before.Success);
            Assert.Equal(OgSharedSetupCodes.TableMissing, before.Reference);
            Assert.Contains(OgSharedSetup.TableName, before.Message);

            // ---- the first administrator's run creates the table and records the company --------------
            var first = await service.WriteInstallationAsync(
                Company("Probe Energy", "PROBE", "AE", "USD", 1),
                expectedRevision: string.Empty,
                actor: "admin");

            Assert.True(first.Success, $"{first.Message} ({first.Reference})");
            Assert.True(service.Store.TableExists, $"The {OgSharedSetup.TableName} table was not created.");

            var read = await service.ReadAsync();
            Assert.True(read.Success, read.Message);
            Assert.Equal("Probe Energy", read.Value!.Installation!.CompanyName);
            Assert.Equal("PROBE", read.Value.Installation.CompanyCode);
            Assert.Equal("AE", read.Value.Installation.CountryCode);
            Assert.Equal(1, read.Value.Installation.FiscalYearStartMonth);
            Assert.NotEmpty(read.Value.InstallationRevision);
            Assert.Equal("admin", read.Value.Installation.RecordedBy);

            // ---- a second workstation that did not see the first write must not overwrite it ----------
            var stale = await service.WriteInstallationAsync(
                Company("Other Company", "OTHER", "SA", "SAR", 7),
                expectedRevision: string.Empty,
                actor: "second-admin");

            Assert.False(stale.Success);
            Assert.Equal(OgSharedSetupCodes.Stale, stale.Reference);

            var still = await service.ReadAsync();
            Assert.Equal("Probe Energy", still.Value!.Installation!.CompanyName);

            // ---- the system administrator, who did read it, may change it ----------------------------
            var updated = await service.WriteInstallationAsync(
                Company("Probe Energy", "PROBE", "SA", "SAR", 7),
                expectedRevision: read.Value.InstallationRevision,
                actor: "admin");

            Assert.True(updated.Success, updated.Message);

            var after = await service.ReadAsync();
            Assert.Equal("SA", after.Value!.Installation!.CountryCode);
            Assert.Equal("SAR", after.Value.Installation.BaseCurrencyCode);
            Assert.Equal(7, after.Value.Installation.FiscalYearStartMonth);
            Assert.NotEqual(read.Value.InstallationRevision, after.Value.InstallationRevision);

            Assert.Empty(reporter.Failures);
        });
    }

    [Fact]
    public async Task The_module_set_and_the_environment_document_round_trip()
    {
        await WithDatabase(async (host, reporter, service) =>
        {
            var modules = await service.WriteModulesAsync(
                ["WELL", "HSE"],
                expectedRevision: string.Empty,
                actor: "admin");

            Assert.True(modules.Success, modules.Message);

            var environmentJson = Setup.OgSetupCatalog.BuildEnvironmentDocument("AE", "oilfield");
            Assert.False(string.IsNullOrWhiteSpace(environmentJson));

            var environment = await service.WriteToolsEnvironmentAsync(
                environmentJson!,
                expectedRevision: string.Empty,
                actor: "admin");

            Assert.True(environment.Success, environment.Message);

            var read = await service.ReadAsync();
            Assert.True(read.Success, read.Message);
            Assert.Equal(["WELL", "HSE"], read.Value!.Modules);
            Assert.Equal(environmentJson, read.Value.ToolsEnvironmentJson);

            Assert.Empty(reporter.Failures);
        });
    }

    [Fact]
    public void An_installation_document_that_cannot_be_read_is_refused_rather_than_half_applied()
    {
        // Parsing belongs to the service so a refusal is recorded; a document this build cannot read is "not
        // recorded", never a company with empty or guessed values.
        var reporter = new RecordingFailureReporter();
        var service = new OgSharedSetupService(new NeverUsedStore(), reporter, new SilentAudit());

        Assert.Null(service.ParseInstallation(null));
        Assert.Null(service.ParseInstallation("{\"CompanyName\":\"\"}"));
        Assert.Null(service.ParseInstallation("not json at all"));

        // A document written by this build round-trips unchanged.
        var company = Company("Probe Energy", "PROBE", "AE", "USD", 4);

        Assert.Equal(company, service.ParseInstallation(company.ToJson()));

        // Exactly the malformed document was recorded; a blank company is an ordinary "not recorded" answer.
        Assert.Single(reporter.Failures);
    }

    [Fact]
    public void The_module_document_survives_a_document_a_later_build_may_write()
    {
        var reporter = new RecordingFailureReporter();
        var service = new OgSharedSetupService(new NeverUsedStore(), reporter, new SilentAudit());

        Assert.Empty(service.ParseModules(null));
        Assert.Empty(service.ParseModules("{}"));
        Assert.Empty(service.ParseModules("{\"modules\":\"WELL\"}"));
        Assert.Empty(service.ParseModules("not json"));

        Assert.Equal(
            ["WELL", "HSE"],
            service.ParseModules("{\"modules\":[\"WELL\",\"HSE\"],\"recordedUtc\":\"2026-11-09T00:00:00Z\"}"));
    }

    private static OgInstallationSettings Company(
        string name,
        string code,
        string country,
        string currency,
        int fiscalMonth) =>
        new(
            CompanyName: name,
            CompanyCode: code,
            CountryCode: country,
            CultureName: "en-US",
            TimeZoneId: "UTC",
            BaseCurrencyCode: currency,
            FiscalYearStartMonth: fiscalMonth,
            CompanyTypeCode: "OPERATOR",
            UnitSystemCode: "oilfield",
            DefaultFieldName: "North Field",
            RecordedBy: "admin",
            RecordedAtUtc: new DateTimeOffset(2026, 11, 9, 8, 0, 0, TimeSpan.Zero));

    /// <summary>A real database file, opened through the connection catalogue, with no settings table yet.</summary>
    private static async Task WithDatabase(
        Func<OgBeepHost, RecordingFailureReporter, OgSharedSetupService, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "settings-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasSettings"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "settings.db"));

            Assert.True(created.Success, created.Message);

            var store = new BeepDmSharedSetupStore(host, reporter, ConnectionName);
            await test(host, reporter, new OgSharedSetupService(store, reporter, new SilentAudit()));
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

    /// <summary>A store the parsing tests never touch: they exercise the document readers only.</summary>
    private sealed class NeverUsedStore : IOgSharedSetupStore
    {
        public string ConnectionName => "parsing-only";

        public bool TableExists => false;

        public Task<OgResult<OgSharedSetupRecord?>> ReadAsync(
            string settingsId,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("ReadAsync should not be called by a parsing test.");

        public Task<OgResult<OgSharedSetupRecord>> SaveAsync(
            OgSharedSetupChange change,
            string actor,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("SaveAsync should not be called by a parsing test.");

        public Task<OgResult> DeleteAsync(
            string settingsId,
            string expectedRevision,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("DeleteAsync should not be called by a parsing test.");
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
