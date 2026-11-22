using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Security;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// The enterprise order: the company database is the account directory, so a user is authenticated against its
/// USER table and gets the roles its RBAC tables grant — before any company setting is read.
/// </summary>
public sealed class DatabaseIdentityTests
{
    private const string ConnectionName = "identity-probe";

    [Fact]
    public async Task An_administrator_is_created_in_the_database_and_can_sign_in_with_the_roles_it_grants()
    {
        await WithDatabase(async (databaseFile, reporter, identity) =>
        {
            // A fresh database has the tables but no login account yet.
            Assert.False(await HasAccountsAsync(identity, reporter));

            var created = await identity.CreateSystemAdministratorAsync(
                "sysadmin",
                "System Administrator",
                "Probe#2026!Desk",
                actor: "setup");

            Assert.True(created.Success, $"{created.Message} ({created.Reference})");
            Assert.Equal(["Administrator"], created.Value!.Roles);

            var accounts = await identity.HasAccountsAsync();
            Assert.True(accounts.Success);
            Assert.True(accounts.Value);

            var signedIn = await identity.SignInAsync("sysadmin", "Probe#2026!Desk");

            Assert.True(signedIn.Success, signedIn.Message);
            Assert.Equal("sysadmin", signedIn.Value!.UserName);
            Assert.Equal("System Administrator", signedIn.Value.DisplayName);
            Assert.Equal(["Administrator"], signedIn.Value.Roles);

            // The stored password is a PBKDF2 envelope, never the password and never a seeded marker.
            using var connection = new SqliteConnection($"Data Source={databaseFile}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT PASSWORD_HASH FROM USER WHERE USERNAME = 'sysadmin';";
            var stored = (string?)command.ExecuteScalar();

            Assert.NotNull(stored);
            Assert.StartsWith("pbkdf2-sha256$210000$", stored);
            Assert.DoesNotContain("Probe#2026!Desk", stored, StringComparison.Ordinal);

            Assert.Empty(reporter.Failures);
        });
    }

    [Fact]
    public async Task A_wrong_password_and_an_unknown_user_get_the_same_sentence_and_no_session()
    {
        await WithDatabase(async (databaseFile, reporter, identity) =>
        {
            var created = await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", "Probe#2026!Desk", actor: "setup");
            Assert.True(created.Success, created.Message);

            var wrong = await identity.SignInAsync("sysadmin", "Wrong#Password2026!");
            var unknown = await identity.SignInAsync("nobody", "Probe#2026!Desk");

            Assert.False(wrong.Success);
            Assert.False(unknown.Success);
            Assert.Equal(OgDatabaseIdentityCodes.Refused, wrong.Reference);
            Assert.Equal(wrong.Message, unknown.Message);
        });
    }

    [Fact]
    public async Task A_seeded_principal_is_not_a_login_account()
    {
        await WithDatabase(async (databaseFile, reporter, identity) =>
        {
            // The security module's SYSTEM principal carries 'seeded-no-login'; it must never authenticate.
            using (var connection = new SqliteConnection($"Data Source={databaseFile}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                    "INSERT INTO USER (USER_ID, USERNAME, USER_NAME, PASSWORD_HASH, IS_ACTIVE, TENANT_ID) " +
                    "VALUES ('SYSTEM', 'SYSTEM', 'System', 'seeded-no-login', 1, '');";
                command.ExecuteNonQuery();
            }

            var result = await identity.SignInAsync("SYSTEM", "anything");

            Assert.False(result.Success);
            Assert.Equal(OgDatabaseIdentityCodes.Refused, result.Reference);
        });
    }

    [Fact]
    public async Task Creating_a_second_account_with_the_same_name_is_refused()
    {
        await WithDatabase(async (databaseFile, reporter, identity) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync("sysadmin", "Admin", "Probe#2026!Desk", "setup")).Success);

            var again = await identity.CreateSystemAdministratorAsync("sysadmin", "Admin", "Probe#2026!Desk", "setup");

            Assert.False(again.Success);
            Assert.Equal(OgDatabaseIdentityCodes.AdministratorExists, again.Reference);
        });
    }

    [Fact]
    public void The_password_policy_is_one_rule_for_every_account_store()
    {
        Assert.Empty(Core.OgPasswordPolicy.Problems("Probe#2026!Desk"));
        Assert.Empty(Core.OgPasswordPolicy.Problems("correct horse battery staple"));
        Assert.NotEmpty(Core.OgPasswordPolicy.Problems("short"));
        Assert.NotEmpty(Core.OgPasswordPolicy.Problems(new string('a', 65)));
        Assert.NotEmpty(Core.OgPasswordPolicy.Problems("password123456"));
    }

    private static async Task<bool> HasAccountsAsync(IOgDatabaseIdentity identity, RecordingFailureReporter reporter)
    {
        var result = await identity.HasAccountsAsync();
        Assert.True(result.Success, result.Message);
        Assert.Empty(reporter.Failures);
        return result.Value;
    }


    private static async Task WithDatabase(
        Func<string, RecordingFailureReporter, IOgDatabaseIdentity, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "identity-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasIdentity"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");
            var databaseFile = Path.Combine(dataDirectory, "identity.db");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "identity.db"));

            Assert.True(created.Success, created.Message);

            // The security module's own tables, as the setup run would have left them.
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
                    INSERT INTO ROLE (ROLE_ID, ROLE_NAME, DESCRIPTION, ACTIVE_IND) VALUES ('ROLE-ADMIN', 'Administrator', 'Seeded by the security module', 'Y');
                    """;
                command.ExecuteNonQuery();
            }

            await test(databaseFile, reporter, new BeepDmDatabaseIdentity(host, reporter, ConnectionName));
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
