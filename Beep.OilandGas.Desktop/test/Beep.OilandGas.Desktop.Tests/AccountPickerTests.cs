using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-08 AC-5: the sign-in window offers the account names of the installation's database — names only, and only
/// accounts that can actually sign in — while a typed name is still accepted.
/// </summary>
public sealed class AccountPickerTests
{
    private const string ConnectionName = "picker-probe";
    private const string Password = "Pick#2026!Typed";

    [Fact]
    public async Task The_picker_lists_the_accounts_that_can_sign_in()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);
            Assert.True((await administration.CreateAsync(
                "engineer", "Reservoir Engineer", "Engineer#2026!Pass", ["ReservoirEngineer"], "sysadmin")).Success);
            Assert.True((await administration.CreateAsync(
                "viewer", "Read Only", "Viewer#2026!Pass", ["Viewer"], "sysadmin")).Success);

            // A disabled account is not offered: it would be an entry that always fails.
            Assert.True((await administration.SetActiveAsync("viewer", active: false, actor: "sysadmin")).Success);

            var names = await identity.ListUserNamesAsync();

            Assert.True(names.Success, names.Message);
            Assert.Equal(["engineer", "sysadmin"], names.Value!);
        });
    }

    [Fact]
    public async Task A_seeded_principal_and_a_disabled_account_are_not_offered()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);

            using (var connection = new SqliteConnection($"Data Source={databaseFile}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                    "INSERT INTO USER (USER_ID, USERNAME, USER_NAME, PASSWORD_HASH, IS_ACTIVE, TENANT_ID) " +
                    "VALUES ('SYSTEM', 'SYSTEM', 'System', 'seeded-no-login', 1, '');";
                command.ExecuteNonQuery();
            }

            var names = await identity.ListUserNamesAsync();

            Assert.True(names.Success, names.Message);
            Assert.Equal(["sysadmin"], names.Value!);
        });
    }

    [Fact]
    public async Task The_names_carry_no_secret_and_no_address()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);

            var names = await identity.ListUserNamesAsync();

            Assert.True(names.Success, names.Message);
            Assert.All(names.Value!, name =>
            {
                Assert.DoesNotContain("pbkdf2", name, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("@", name, StringComparison.Ordinal);
                Assert.DoesNotContain("System Administrator", name, StringComparison.Ordinal);
            });
        });
    }

    [Fact]
    public async Task A_typed_name_still_signs_in_whether_or_not_it_was_offered()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);

            // The picker offers the names, but nothing depends on the window: the service accepts a typed name.
            var names = await identity.ListUserNamesAsync();
            Assert.Contains("sysadmin", names.Value!);

            var typed = await identity.SignInAsync("sysadmin", Password);

            Assert.True(typed.Success, typed.Message);

            // And a name the picker never offered is refused the same way as a wrong password.
            var unknown = await identity.SignInAsync("no-such-person", Password);

            Assert.False(unknown.Success);
            Assert.Equal(OgDatabaseIdentityCodes.Refused, unknown.Reference);
        });
    }

    [Fact]
    public async Task A_database_with_no_accounts_lists_nothing_rather_than_failing()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            var names = await identity.ListUserNamesAsync();

            Assert.True(names.Success, names.Message);
            Assert.Empty(names.Value!);
        });
    }

    // ------------------------------------------------------------------ fixture

    private static async Task WithDatabase(
        Func<string, RecordingFailureReporter, IOgDatabaseIdentity, IOgUserAdministration, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "picker-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPicker"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");
            var databaseFile = Path.Combine(dataDirectory, "picker.db");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "picker.db"));

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
                    INSERT INTO ROLE (ROLE_ID, ROLE_NAME, DESCRIPTION, ACTIVE_IND) VALUES
                        ('ROLE-ADMIN', 'Administrator', 'Seeded by the security module', 'Y'),
                        ('ROLE-RES', 'ReservoirEngineer', 'Seeded by the security module', 'Y'),
                        ('ROLE-VIEW', 'Viewer', 'Seeded by the security module', 'Y');
                    """;
                command.ExecuteNonQuery();
            }

            var store = new BeepDmSharedSetupStore(host, reporter, ConnectionName);
            var settings = new OgSharedSetupService(store, reporter, new SilentAudit());
            Assert.True((await settings.WriteSchemaVersionAsync(string.Empty, "test")).Success);

            var identity = new BeepDmDatabaseIdentity(host, reporter, ConnectionName);
            var administration = new BeepDmUserAdministration(identity, reporter);

            await test(databaseFile, reporter, identity, administration);
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
