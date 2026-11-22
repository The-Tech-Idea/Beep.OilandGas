using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-03 AC-5: company creation is all-or-nothing. A step that fails after the first write removes what this run
/// wrote, reports what could not be removed, and leaves the database as it was before the run.
/// </summary>
public sealed class CompanyCreationTests
{
    private const string ConnectionName = "creation-probe";
    private const string Strong = "Probe#2026!Desk";

    [Fact]
    public async Task A_successful_creation_writes_the_company_the_environment_the_modules_and_the_administrator()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity, audits) =>
        {
            var creation = NewCreation(settings, identity, reporter, audits);

            var result = await creation.CreateAsync(Request(Strong));

            Assert.True(result.Success, $"{result.Message} ({result.Reference})");
            Assert.False(result.Value!.Adopted);
            Assert.Contains("company record written", result.Value.Completed);
            Assert.Contains("environment written", result.Value.Completed);
            Assert.Contains("module set recorded", result.Value.Completed);
            Assert.Contains("administrator created", result.Value.Completed);

            var stored = await settings.ReadAsync();
            Assert.True(stored.Success, stored.Message);
            Assert.Equal("Probe Energy", stored.Value!.Installation!.CompanyName);
            Assert.Equal(["WELL"], stored.Value.Modules);
            Assert.False(string.IsNullOrWhiteSpace(stored.Value.ToolsEnvironmentJson));

            Assert.Single(audits.Rows.Where(row => row.Verb == "company.setup" && row.Outcome == "ok"));
        });
    }

    [Fact]
    public async Task A_failure_after_the_settings_were_written_removes_them_again()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity, audits) =>
        {
            var creation = NewCreation(settings, identity, reporter, audits);

            // The password is refused by the account policy, which happens after the company record, the environment
            // and the module set have been written — exactly the state that used to be left behind.
            var result = await creation.CreateAsync(Request(password: "short"));

            Assert.False(result.Success);
            Assert.Contains("Nothing was left behind", result.Message, StringComparison.Ordinal);

            var stored = await settings.ReadAsync();
            Assert.True(stored.Success, stored.Message);
            Assert.Null(stored.Value!.Installation);
            Assert.Null(stored.Value.ToolsEnvironmentJson);
            Assert.Empty(stored.Value.Modules);

            using var connection = new SqliteConnection($"Data Source={databaseFile}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM USER WHERE USERNAME = 'sysadmin';";
            Assert.Equal(0L, (long)command.ExecuteScalar()!);

            Assert.Contains(audits.Rows, row => row.Verb == "company.setup" && row.Outcome == "refused");
        });
    }

    [Fact]
    public async Task A_module_failure_writes_no_company_at_all()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity, audits) =>
        {
            var creation = new OgCompanyCreation(
                settings,
                identity,
                (_, _, _) => Task.FromResult(OgResult<OgSetupOutcome>.Failed(
                    "Schema apply failed for module WELL.",
                    "company-setup-modules-failed")),
                reporter,
                audits);

            var result = await creation.CreateAsync(Request(Strong));

            Assert.False(result.Success);
            Assert.Contains("partly applied", result.Message, StringComparison.Ordinal);

            var stored = await settings.ReadAsync();

            // Nothing was written at all — not even the settings table, because the schema step failed first.
            Assert.False(stored.Success);
            Assert.Equal(OgSharedSetupCodes.TableMissing, stored.Reference);

            using var connection = new SqliteConnection($"Data Source={databaseFile}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM USER;";
            Assert.Equal(0L, (long)command.ExecuteScalar()!);
        });
    }

    [Fact]
    public async Task An_existing_company_is_adopted_and_nothing_is_rewritten()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity, audits) =>
        {
            // First run creates it, and the module plan is applied.
            var applied = new List<string>();
            var first = await NewCreation(settings, identity, reporter, audits, applied)
                .CreateAsync(Request(Strong));

            Assert.True(first.Success, first.Message);
            Assert.Equal(["WELL"], applied);

            var before = await settings.ReadAsync();
            var revision = before.Value!.InstallationRevision;

            // A second workstation joins with another company name: the record must not change, and no module may be
            // applied again - a join writes nothing to the company database (AC-3).
            var joined = await NewCreation(settings, identity, reporter, audits, applied)
                .CreateAsync(Request(Strong) with { Company = Company("Other Company", "OTHER") });

            Assert.True(joined.Success, joined.Message);
            Assert.True(joined.Value!.Adopted);
            Assert.Equal(["WELL"], applied);

            var after = await settings.ReadAsync();
            Assert.Equal("Probe Energy", after.Value!.Installation!.CompanyName);
            Assert.Equal(revision, after.Value.InstallationRevision);
        });
    }

    private static OgCompanyCreationRequest Request(string password) =>
        new(
            ConnectionName: ConnectionName,
            Company: Company("Probe Energy", "PROBE"),
            EnvironmentDocument: Setup.OgSetupCatalog.BuildEnvironmentDocument("AE", "oilfield")!,
            ModuleIds: ["WELL"],
            IncludeDemoData: false,
            AdministratorUserName: "sysadmin",
            AdministratorDisplayName: "System Administrator",
            AdministratorPassword: password,
            Actor: "setup");

    private static OgInstallationSettings Company(string name, string code) =>
        new(
            CompanyName: name,
            CompanyCode: code,
            CountryCode: "AE",
            CultureName: "en-US",
            TimeZoneId: "UTC",
            BaseCurrencyCode: "USD",
            FiscalYearStartMonth: 1,
            CompanyTypeCode: "OPERATOR",
            UnitSystemCode: "oilfield",
            DefaultFieldName: "North Field",
            RecordedBy: "setup",
            RecordedAtUtc: new DateTimeOffset(2026, 11, 9, 8, 0, 0, TimeSpan.Zero));

    private static OgCompanyCreation NewCreation(
        OgSharedSetupService settings,
        IOgDatabaseIdentity identity,
        IFailureReporter reporter,
        RecordingAuditService audits,
        ICollection<string>? applied = null) =>
        new(
            settings,
            identity,
            (modules, confirmation, progress) =>
            {
                foreach (var module in modules)
                {
                    applied?.Add(module);
                }

                return Task.FromResult(OgResult<OgSetupOutcome>.Ok(
                new OgSetupOutcome(
                    Success: true,
                    ConnectionName: ConnectionName,
                    ModulesRun: modules.Count,
                    ModulesSucceeded: modules.Count,
                    RecordsInserted: 3,
                    Modules: [],
                    CanResume: false,
                    SchemaMessage: "applied",
                    Reference: null)));
            },
            reporter,
            audits);

    private static async Task WithDatabase(
        Func<string, RecordingFailureReporter, OgSharedSetupService, IOgDatabaseIdentity, RecordingAuditService, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "creation-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();
        var audits = new RecordingAuditService();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasCreation"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");
            var databaseFile = Path.Combine(dataDirectory, "creation.db");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "creation.db"));

            Assert.True(created.Success, created.Message);

            // The security module's tables, as the setup run would have left them.
            using (var seed = new SqliteConnection($"Data Source={databaseFile}"))
            {
                seed.Open();
                using var command = seed.CreateCommand();
                command.CommandText =
                    """
                    CREATE TABLE ROLE (
                        ROLE_ID TEXT PRIMARY KEY, ROLE_NAME TEXT, DESCRIPTION TEXT,
                        ACTIVE_IND TEXT, ROW_CREATED_BY TEXT, ROW_CREATED_DATE TEXT, ROW_CHANGED_BY TEXT,
                        ROW_CHANGED_DATE TEXT, ROW_EFFECTIVE_DATE TEXT, ROW_EXPIRY_DATE TEXT, ROW_QUALITY TEXT,
                        PPDM_GUID TEXT, EXPIRY_DATE TEXT, EFFECTIVE_DATE TEXT, REMARK TEXT, SOURCE TEXT);
                    CREATE TABLE USER (
                        USER_ID TEXT PRIMARY KEY, USERNAME TEXT, USER_NAME TEXT, FULL_NAME TEXT, EMAIL TEXT,
                        PASSWORD_HASH TEXT, IS_ACTIVE INTEGER, TENANT_ID TEXT, BUSINESS_ASSOCIATE_ID TEXT,
                        FAILED_LOGIN_COUNT INTEGER, LOCKED_IND TEXT, LOCKOUT_UNTIL_UTC TEXT, LAST_LOGIN_UTC TEXT,
                        LAST_PASSWORD_CHANGE_UTC TEXT, MAX_PASSWORD_AGE_DAYS INTEGER, PASSWORD_RESET_TOKEN TEXT,
                        PASSWORD_RESET_TOKEN_EXPIRY_UTC TEXT, MFA_ENABLED_IND TEXT,
                        ACTIVE_IND TEXT, ROW_CREATED_BY TEXT, ROW_CREATED_DATE TEXT, ROW_CHANGED_BY TEXT,
                        ROW_CHANGED_DATE TEXT, ROW_EFFECTIVE_DATE TEXT, ROW_EXPIRY_DATE TEXT, ROW_QUALITY TEXT,
                        PPDM_GUID TEXT, EXPIRY_DATE TEXT, EFFECTIVE_DATE TEXT, REMARK TEXT, SOURCE TEXT);
                    CREATE TABLE USER_ROLE (
                        USER_ROLE_ID TEXT PRIMARY KEY, USER_ID TEXT, ROLE_ID TEXT, BUSINESS_ASSOCIATE_ID TEXT,
                        EFFECTIVE_DATE TEXT, EXPIRY_DATE TEXT,
                        ACTIVE_IND TEXT, ROW_CREATED_BY TEXT, ROW_CREATED_DATE TEXT, ROW_CHANGED_BY TEXT,
                        ROW_CHANGED_DATE TEXT, ROW_EFFECTIVE_DATE TEXT, ROW_EXPIRY_DATE TEXT, ROW_QUALITY TEXT,
                        PPDM_GUID TEXT, REMARK TEXT, SOURCE TEXT);
                    INSERT INTO ROLE (ROLE_ID, ROLE_NAME, DESCRIPTION, ACTIVE_IND)
                        VALUES ('ROLE-ADMIN', 'Administrator', 'Seeded by the security module', 'Y');
                    """;
                command.ExecuteNonQuery();
            }

            var store = new BeepDmSharedSetupStore(host, reporter, ConnectionName);
            var settings = new OgSharedSetupService(store, reporter, new SilentAudit());
            var identity = new BeepDmDatabaseIdentity(host, reporter, ConnectionName);

            await test(databaseFile, reporter, settings, identity, audits);
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

    private sealed class RecordingAuditService : IOgAuditService
    {
        public List<(string Verb, string Outcome, string? Detail)> Rows { get; } = [];

        public void Add(
            string verb,
            string subjectKind,
            string subjectId,
            string outcome,
            string? detail = null,
            string? fieldId = null) =>
            Rows.Add((verb, outcome, detail));
    }
}
