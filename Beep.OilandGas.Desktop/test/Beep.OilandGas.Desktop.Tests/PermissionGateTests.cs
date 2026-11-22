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
/// SETUP-05 AC-8: authorisation is not only a role. The permission codes a role carries are read from
/// <c>ROLE_PERMISSION</c> at sign-in, and a write path is allowed by the role <b>or</b> by the permission. A role
/// without the permission is refused; a role with it succeeds.
/// </summary>
public sealed class PermissionGateTests
{
    private const string ConnectionName = "permissions-probe";
    private const string Password = "Gate#2026!Typed";

    [Fact]
    public async Task A_role_without_the_permission_cannot_administer_accounts()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            // Two administrators exist, so demoting one is allowed by the last-administrator guard.
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "auditor", "Auditor", Password, "setup")).Success);

            // The seeded Administrator role carries Admin.ManageUsers; the auditor is moved to Viewer, which does not.
            Assert.True((await administration.RevokeRoleAsync("auditor", "Administrator", "setup")).Success);
            Assert.True((await administration.GrantRoleAsync("auditor", "Viewer", "setup")).Success);

            var signedIn = await identity.SignInAsync("auditor", Password);

            Assert.True(signedIn.Success, signedIn.Message);
            Assert.Equal(["Viewer"], signedIn.Value!.Roles);
            Assert.Empty(signedIn.Value.GrantedPermissions);

            var permissions = new OgPermissions(
                signedIn.Value.Roles,
                signedIn.Value.GrantedPermissions,
                permissionSourceAvailable: true);

            Assert.False(permissions.CanManageUsers);
            Assert.False(permissions.CanConfigureSystem);
        });
    }

    [Fact]
    public async Task A_role_with_the_permission_can_administer_accounts_without_being_an_administrator()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "hr", "HR Officer", Password, "setup")).Success);

            // An HR officer is not an administrator; the role carries only the account permission.
            Assert.True((await administration.RevokeRoleAsync("hr", "Administrator", "setup")).Success);
            Assert.True((await administration.GrantRoleAsync("hr", "HrAdmin", "setup")).Success);

            var signedIn = await identity.SignInAsync("hr", Password);

            Assert.True(signedIn.Success, signedIn.Message);

            var permissions = new OgPermissions(
                signedIn.Value!.Roles,
                signedIn.Value.GrantedPermissions,
                permissionSourceAvailable: true);

            Assert.False(permissions.IsSystemAdmin);
            Assert.True(permissions.CanManageUsers);
            Assert.False(permissions.CanConfigureSystem);
            Assert.Contains(OgPermissionNames.ManageUsers, permissions.Permissions);
        });
    }

    [Fact]
    public async Task The_administrator_role_keeps_both_gates_open()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);

            var signedIn = await identity.SignInAsync("sysadmin", Password);
            var permissions = new OgPermissions(
                signedIn.Value!.Roles,
                signedIn.Value.GrantedPermissions,
                permissionSourceAvailable: true);

            // The database's "Administrator" role carries both permission codes; the role-to-SystemAdmin mapping is
            // the application's own step and is asserted where the session is built.
            Assert.True(permissions.CanManageUsers);
            Assert.True(permissions.CanConfigureSystem);
            Assert.Contains(OgPermissionNames.ManageUsers, permissions.Permissions);
            Assert.Contains(OgPermissionNames.ConfigureSystem, permissions.Permissions);
        });
    }

    [Fact]
    public async Task An_expired_role_grant_grants_nothing()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "temp", "Temporary Admin", Password, "setup")).Success);

            // The administrator grant is given an expiry in the past: the account keeps the row but not the right.
            using (var connection = new SqliteConnection($"Data Source={databaseFile}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "UPDATE USER_ROLE SET EXPIRY_DATE = '2020-01-01T00:00:00Z';";
                command.ExecuteNonQuery();
            }

            var signedIn = await identity.SignInAsync("temp", Password);
            var permissions = new OgPermissions(
                signedIn.Value!.Roles,
                signedIn.Value.GrantedPermissions,
                permissionSourceAvailable: true);

            Assert.Empty(signedIn.Value.Roles);
            Assert.Empty(signedIn.Value.GrantedPermissions);
            Assert.False(permissions.CanManageUsers);
        });
    }

    [Fact]
    public void The_gate_names_the_permission_it_needs()
    {
        var permissions = OgPermissions.None;

        Assert.False(permissions.CanManageUsers);
        Assert.False(permissions.CanConfigureSystem);
        Assert.Equal("Admin.ManageUsers", permissions.NameOfPermissionFor(OgPermissionNames.ManageUsers));
        Assert.Equal("Admin.ConfigureSystem", permissions.NameOfPermissionFor(OgPermissionNames.ConfigureSystem));
    }

    // ------------------------------------------------------------------ fixture

    private static async Task WithDatabase(
        Func<string, RecordingFailureReporter, IOgDatabaseIdentity, IOgUserAdministration, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "permissions-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPermissions"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");
            var databaseFile = Path.Combine(dataDirectory, "permissions.db");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "permissions.db"));

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
                    CREATE TABLE PERMISSION (
                        PERMISSION_ID TEXT PRIMARY KEY, PERMISSION_CODE TEXT, DESCRIPTION TEXT,
                        ACTIVE_IND TEXT, ROW_CREATED_BY TEXT, ROW_CREATED_DATE TEXT, ROW_CHANGED_BY TEXT,
                        ROW_CHANGED_DATE TEXT, ROW_EFFECTIVE_DATE TEXT, ROW_EXPIRY_DATE TEXT, ROW_QUALITY TEXT,
                        PPDM_GUID TEXT, EXPIRY_DATE TEXT, EFFECTIVE_DATE TEXT, REMARK TEXT, SOURCE TEXT);
                    CREATE TABLE ROLE_PERMISSION (
                        ROLE_PERMISSION_ID TEXT PRIMARY KEY, ROLE_ID TEXT, PERMISSION_ID TEXT,
                        ACTIVE_IND TEXT, ROW_CREATED_BY TEXT, ROW_CREATED_DATE TEXT, ROW_CHANGED_BY TEXT,
                        ROW_CHANGED_DATE TEXT, ROW_EFFECTIVE_DATE TEXT, ROW_EXPIRY_DATE TEXT, ROW_QUALITY TEXT,
                        PPDM_GUID TEXT, EXPIRY_DATE TEXT, EFFECTIVE_DATE TEXT, REMARK TEXT, SOURCE TEXT);

                    INSERT INTO ROLE (ROLE_ID, ROLE_NAME, DESCRIPTION, ACTIVE_IND) VALUES
                        ('ROLE-ADMIN', 'Administrator', 'Seeded by the security module', 'Y'),
                        ('ROLE-VIEW', 'Viewer', 'Seeded by the security module', 'Y'),
                        ('ROLE-HR', 'HrAdmin', 'Account administration without system configuration', 'Y');

                    INSERT INTO PERMISSION (PERMISSION_ID, PERMISSION_CODE, DESCRIPTION, ACTIVE_IND) VALUES
                        ('PERM-USERS', 'Admin.ManageUsers', 'Create and administer accounts', 'Y'),
                        ('PERM-CONFIG', 'Admin.ConfigureSystem', 'Change installation settings', 'Y');

                    -- The administrator carries both; the HR role only the account permission; the viewer neither.
                    INSERT INTO ROLE_PERMISSION (ROLE_PERMISSION_ID, ROLE_ID, PERMISSION_ID, ACTIVE_IND) VALUES
                        ('RP-1', 'ROLE-ADMIN', 'PERM-USERS', 'Y'),
                        ('RP-2', 'ROLE-ADMIN', 'PERM-CONFIG', 'Y'),
                        ('RP-3', 'ROLE-HR', 'PERM-USERS', 'Y');
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
