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
/// SETUP-03 AC-4: the administrator a setup creates is provisional. Its password was typed by whoever ran the setup,
/// so the first sign-in must return "change it", a weak replacement is refused, and once it is replaced the old
/// password stops working and the marker is gone.
/// </summary>
public sealed class BootstrapPasswordTests
{
    private const string ConnectionName = "bootstrap-probe";

    [Fact]
    public async Task A_setup_created_administrator_is_asked_to_change_its_password()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity) =>
        {
            var creation = NewCreation(settings, identity, reporter);

            var created = await creation.CreateAsync(Request(OldPassword));

            Assert.True(created.Success, $"{created.Message} ({created.Reference})");
            Assert.Contains("password change required at first sign-in", created.Value!.Completed);

            var signedIn = await identity.SignInAsync("sysadmin", OldPassword);

            Assert.True(signedIn.Success, signedIn.Message);
            Assert.True(signedIn.Value!.MustChangePassword);
        });
    }

    [Fact]
    public async Task An_adopted_administrator_is_not_marked()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity) =>
        {
            // A first workstation creates the company without asking for a password change.
            var first = await NewCreation(settings, identity, reporter)
                .CreateAsync(Request(OldPassword, requirePasswordChange: false));

            Assert.True(first.Success, first.Message);
            Assert.False(first.Value!.Adopted);

            // A second one joins the same company; it adopts the record and the account, so it marks nothing.
            var joined = await NewCreation(settings, identity, reporter)
                .CreateAsync(Request(OldPassword));

            Assert.True(joined.Success, joined.Message);
            Assert.True(joined.Value!.Adopted);

            var signedIn = await identity.SignInAsync("sysadmin", OldPassword);

            Assert.True(signedIn.Success, signedIn.Message);
            Assert.False(signedIn.Value!.MustChangePassword);
        });
    }

    [Fact]
    public async Task A_weak_new_password_is_refused_and_the_marker_stays()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity) =>
        {
            Assert.True((await NewCreation(settings, identity, reporter)
                .CreateAsync(Request(OldPassword))).Success);

            var refused = await identity.ChangePasswordAsync("sysadmin", "short");

            Assert.False(refused.Success);
            Assert.Equal(OgDatabaseIdentityCodes.PasswordRefused, refused.Reference);

            // Still provisional, and the setup password still opens the database.
            var signedIn = await identity.SignInAsync("sysadmin", OldPassword);
            Assert.True(signedIn.Success, signedIn.Message);
            Assert.True(signedIn.Value!.MustChangePassword);
        });
    }

    [Fact]
    public async Task After_the_change_the_old_password_stops_working_and_the_marker_is_gone()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity) =>
        {
            Assert.True((await NewCreation(settings, identity, reporter)
                .CreateAsync(Request(OldPassword))).Success);

            string HashOf()
            {
                using var c = new SqliteConnection($"Data Source={databaseFile}");
                c.Open();
                using var cmd = c.CreateCommand();
                cmd.CommandText = "SELECT PASSWORD_HASH FROM USER WHERE USERNAME = 'sysadmin';";
                return (string?)cmd.ExecuteScalar() ?? "(none)";
            }

            var before = HashOf();
            var changed = await identity.ChangePasswordAsync("sysadmin", NewPassword);

            Assert.True(changed.Success, changed.Message);
            Assert.NotEqual(before, HashOf());

            var withOld = await identity.SignInAsync("sysadmin", OldPassword);
            Assert.False(withOld.Success);
            Assert.Equal(OgDatabaseIdentityCodes.Refused, withOld.Reference);

            var withNew = await identity.SignInAsync("sysadmin", NewPassword);
            Assert.True(withNew.Success, withNew.Message);
            Assert.False(withNew.Value!.MustChangePassword);

            // The marker row is deleted, not merely ignored.
            using var connection = new SqliteConnection($"Data Source={databaseFile}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM BEEPSETTINGS WHERE SETTINGS_ID = 'BOOTSTRAP';";
            Assert.Equal(0L, (long)command.ExecuteScalar()!);
        });
    }

    [Fact]
    public async Task An_existing_company_without_a_marker_signs_in_normally()
    {
        await WithDatabase(async (databaseFile, reporter, settings, identity) =>
        {
            // No creation ran, so there is no marker at all.
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", OldPassword, "setup")).Success);

            var signedIn = await identity.SignInAsync("sysadmin", OldPassword);

            Assert.True(signedIn.Success, signedIn.Message);
            Assert.False(signedIn.Value!.MustChangePassword);
        });
    }

    // ------------------------------------------------------------------ fixtures

    private const string OldPassword = "Setup#2026!Typed";
    private const string NewPassword = "a long sentence the owner chose";

    private static OgCompanyCreation NewCreation(
        OgSharedSetupService settings,
        IOgDatabaseIdentity identity,
        IFailureReporter reporter) =>
        new(
            settings,
            identity,
            (modules, confirmation, progress) => Task.FromResult(OgResult<OgSetupOutcome>.Ok(
                new OgSetupOutcome(
                    Success: true,
                    ConnectionName: ConnectionName,
                    ModulesRun: modules.Count,
                    ModulesSucceeded: modules.Count,
                    RecordsInserted: 0,
                    Modules: [],
                    CanResume: false,
                    SchemaMessage: "applied",
                    Reference: null))),
            reporter,
            new SilentAudit());

    private static OgCompanyCreationRequest Request(string password, bool requirePasswordChange = true) =>
        new(
            ConnectionName: ConnectionName,
            Company: new OgInstallationSettings(
                "Probe Energy", "PROBE", "AE", "en-US", "UTC", "USD", 1, "OPERATOR", "oilfield", "North Field",
                "setup", new DateTimeOffset(2026, 11, 9, 8, 0, 0, TimeSpan.Zero)),
            EnvironmentDocument: Setup.OgSetupCatalog.BuildEnvironmentDocument("AE", "oilfield")!,
            ModuleIds: ["WELL"],
            IncludeDemoData: false,
            AdministratorUserName: "sysadmin",
            AdministratorDisplayName: "System Administrator",
            AdministratorPassword: password,
            Actor: "setup",
            RequirePasswordChange: requirePasswordChange);

    private static async Task WithDatabase(
        Func<string, RecordingFailureReporter, OgSharedSetupService, IOgDatabaseIdentity, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "bootstrap-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasBootstrap"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");
            var databaseFile = Path.Combine(dataDirectory, "bootstrap.db");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "bootstrap.db"));

            Assert.True(created.Success, created.Message);

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

            await test(databaseFile, reporter, settings, identity);
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

    private sealed class SilentAudit : IOgAuditService
    {
        public void Add(
            string verb,
            string subjectKind,
            string subjectId,
            string outcome,
            string? detail = null,
            string? fieldId = null)
        {
        }
    }
}
