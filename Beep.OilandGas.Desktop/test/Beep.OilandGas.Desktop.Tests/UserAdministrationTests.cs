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
/// SETUP-05 AC-9 and SETUP-10 AC-6: the SystemAdmin's account administration — create with a role, reset a password
/// (which makes the account provisional again), disable and enable, grant and revoke a role — with the guard that the
/// last active administrator cannot be locked out.
/// </summary>
public sealed class UserAdministrationTests
{
    private const string ConnectionName = "users-probe";
    private const string InitialPassword = "Root#2026!Typed";
    private const string NewPassword = "a sentence chosen by its owner";

    [Fact]
    public async Task A_created_user_signs_in_with_the_role_it_was_given()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", InitialPassword, "setup")).Success);

            var created = await administration.CreateAsync(
                "engineer", "Reservoir Engineer", NewPassword, ["ReservoirEngineer"], "sysadmin");

            Assert.True(created.Success, $"{created.Message} ({created.Reference})");
            Assert.True(created.Value!.MustChangePassword);
            Assert.Contains("ReservoirEngineer", created.Value.Roles);

            var signedIn = await identity.SignInAsync("engineer", NewPassword);

            Assert.True(signedIn.Success, signedIn.Message);
            Assert.Equal(["ReservoirEngineer"], signedIn.Value!.Roles);
            Assert.True(signedIn.Value.MustChangePassword);
        });
    }

    [Fact]
    public async Task A_reset_password_is_provisional_again_and_the_old_one_stops_working()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", InitialPassword, "setup")).Success);

            Assert.True((await administration.CreateAsync(
                "engineer", "Reservoir Engineer", NewPassword, ["ReservoirEngineer"], "sysadmin")).Success);

            // The person replaced their own password in the meantime.
            Assert.True((await identity.ChangePasswordAsync("engineer", "their own long sentence")).Success);

            var beforeReset = await identity.SignInAsync("engineer", "their own long sentence");
            Assert.True(beforeReset.Success, beforeReset.Message);
            Assert.False(beforeReset.Value!.MustChangePassword);

            var reset = await administration.ResetPasswordAsync("engineer", "Reset#2026!ByAdmin", "sysadmin");

            Assert.True(reset.Success, reset.Message);

            // The administrator's password is provisional, and the person's own password is gone.
            var afterReset = await identity.SignInAsync("engineer", "Reset#2026!ByAdmin");
            Assert.True(afterReset.Success, afterReset.Message);
            Assert.True(afterReset.Value!.MustChangePassword);

            var withOld = await identity.SignInAsync("engineer", "their own long sentence");
            Assert.False(withOld.Success);
        });
    }

    [Fact]
    public async Task A_disabled_account_is_refused()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", InitialPassword, "setup")).Success);

            Assert.True((await administration.CreateAsync(
                "engineer", "Reservoir Engineer", NewPassword, ["ReservoirEngineer"], "sysadmin")).Success);

            var disabled = await administration.SetActiveAsync("engineer", active: false, actor: "sysadmin");

            Assert.True(disabled.Success, disabled.Message);

            var refused = await identity.SignInAsync("engineer", NewPassword);

            Assert.False(refused.Success);
            Assert.Equal(OgDatabaseIdentityCodes.Refused, refused.Reference);

            var enabled = await administration.SetActiveAsync("engineer", active: true, actor: "sysadmin");

            Assert.True(enabled.Success, enabled.Message);
            Assert.True((await identity.SignInAsync("engineer", NewPassword)).Success);
        });
    }

    [Fact]
    public async Task A_role_can_be_granted_and_revoked()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", InitialPassword, "setup")).Success);

            Assert.True((await administration.CreateAsync(
                "engineer", "Reservoir Engineer", NewPassword, ["ReservoirEngineer"], "sysadmin")).Success);

            var granted = await administration.GrantRoleAsync("engineer", "Viewer", "sysadmin");
            Assert.True(granted.Success, granted.Message);

            var afterGrant = await identity.SignInAsync("engineer", NewPassword);
            Assert.Equal(["ReservoirEngineer", "Viewer"], afterGrant.Value!.Roles.OrderBy(role => role));

            var revoked = await administration.RevokeRoleAsync("engineer", "Viewer", "sysadmin");
            Assert.True(revoked.Success, revoked.Message);

            var afterRevoke = await identity.SignInAsync("engineer", NewPassword);
            Assert.Equal(["ReservoirEngineer"], afterRevoke.Value!.Roles);

            var roles = await administration.ListRolesAsync();
            Assert.True(roles.Success);
            Assert.Contains("Administrator", roles.Value!);
        });
    }

    [Fact]
    public async Task The_last_active_administrator_cannot_be_disabled_or_lose_the_role()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", InitialPassword, "setup")).Success);

            var disabled = await administration.SetActiveAsync("sysadmin", active: false, actor: "sysadmin");

            Assert.False(disabled.Success);
            Assert.Equal(OgUserAdministrationCodes.LastAdministrator, disabled.Reference);

            var revoked = await administration.RevokeRoleAsync("sysadmin", "Administrator", "sysadmin");

            Assert.False(revoked.Success);
            Assert.Equal(OgUserAdministrationCodes.LastAdministrator, revoked.Reference);

            // Still an administrator, so somebody can administer the database.
            Assert.True((await identity.SignInAsync("sysadmin", InitialPassword)).Success);
        });
    }

    [Fact]
    public async Task The_list_shows_every_account_and_flags_the_provisional_ones()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", InitialPassword, "setup")).Success);

            Assert.True((await administration.CreateAsync(
                "engineer", "Reservoir Engineer", NewPassword, ["ReservoirEngineer"], "sysadmin")).Success);

            var listed = await administration.ListAsync();

            Assert.True(listed.Success, listed.Message);
            Assert.Equal(2, listed.Value!.Count);

            var sysadmin = listed.Value.Single(user => user.UserName == "sysadmin");
            Assert.Contains("Administrator", sysadmin.Roles);
            Assert.False(sysadmin.MustChangePassword);   // created directly, not by this administration call

            var engineer = listed.Value.Single(user => user.UserName == "engineer");
            Assert.True(engineer.MustChangePassword);
            Assert.True(engineer.IsActive);
        });
    }

    // ------------------------------------------------------------------ fixture

    private static async Task WithDatabase(
        Func<string, RecordingFailureReporter, IOgDatabaseIdentity, IOgUserAdministration, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "users-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasUsers"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");
            var databaseFile = Path.Combine(dataDirectory, "users.db");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "users.db"));

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

            // An account directory only exists inside a company, so the settings table is there too; the version row
            // creates it exactly as a company creation does.
            var store = new BeepDmSharedSetupStore(host, reporter, ConnectionName);
            var settings = new OgSharedSetupService(store, reporter, new SilentAudit());
            Assert.True((await settings
                .WriteSchemaVersionAsync(expectedRevision: string.Empty, actor: "test"))
                .Success);

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
