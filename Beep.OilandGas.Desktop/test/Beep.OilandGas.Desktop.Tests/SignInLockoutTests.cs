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
/// SETUP-05 AC-7: failed sign-ins are counted against the account, the account locks at the threshold, a correct
/// password from its owner is told it is locked, the lock expires, and an administrator can clear it.
/// </summary>
public sealed class SignInLockoutTests
{
    private const string ConnectionName = "lockout-probe";
    private const string Password = "Lock#2026!Typed";
    private const string Wrong = "Wrong#Password2026!";

    [Fact]
    public void The_rule_counts_within_a_window_and_locks_at_the_threshold()
    {
        var now = new DateTimeOffset(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

        // Four failures inside the window: still open.
        var attempt = OgSignInPolicy.NextFailure(0, null, now);
        Assert.False(attempt.Locked);
        Assert.Equal(1, attempt.Count);

        for (var count = 2; count < OgSignInPolicy.MaximumFailures; count++)
        {
            attempt = OgSignInPolicy.NextFailure(attempt.Count, now, now);
            Assert.False(attempt.Locked);
        }

        // The fifth locks it, for the configured duration.
        var locked = OgSignInPolicy.NextFailure(attempt.Count, now, now);
        Assert.True(locked.Locked);
        Assert.Equal(OgSignInPolicy.MaximumFailures, locked.Count);
        Assert.Equal(now + OgSignInPolicy.LockoutDuration, locked.LockedUntil);

        // A failure outside the window starts again: an occasional mistype never accumulates.
        var stale = OgSignInPolicy.NextFailure(4, now - OgSignInPolicy.FailureWindow - TimeSpan.FromMinutes(1), now);
        Assert.False(stale.Locked);
        Assert.Equal(1, stale.Count);

        // A lock in the past no longer applies.
        Assert.True(OgSignInPolicy.IsLocked(now + TimeSpan.FromMinutes(1), now));
        Assert.False(OgSignInPolicy.IsLocked(now - TimeSpan.FromMinutes(1), now));
    }

    [Fact]
    public async Task The_configured_number_of_failures_locks_the_account()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);

            // One failure fewer than the threshold leaves the account open — a single mistype is not a lockout.
            for (var attempt = 1; attempt < OgSignInPolicy.MaximumFailures; attempt++)
            {
                var below = await identity.SignInAsync("sysadmin", Wrong);
                Assert.False(below.Success);
                Assert.Equal(OgDatabaseIdentityCodes.Refused, below.Reference);
            }

            var (openBefore, _, _) = ReadLock(databaseFile, "sysadmin");
            Assert.False(openBefore);

            // The threshold failure locks it.
            var locking = await identity.SignInAsync("sysadmin", Wrong);
            Assert.False(locking.Success);

            var (locked, until, count) = ReadLock(databaseFile, "sysadmin");

            Assert.True(locked);
            Assert.NotNull(until);
            Assert.True(until > DateTime.UtcNow);
            Assert.Equal(OgSignInPolicy.MaximumFailures, count);

            // The owner's correct password is refused with the lock explained, not with the generic sentence.
            var withCorrect = await identity.SignInAsync("sysadmin", Password);

            Assert.False(withCorrect.Success);
            Assert.Equal(OgDatabaseIdentityCodes.Locked, withCorrect.Reference);
            Assert.Contains("locked", withCorrect.Message!, StringComparison.OrdinalIgnoreCase);

            // A guess at a locked account is refused generically and does not extend the lock.
            var guess = await identity.SignInAsync("sysadmin", Wrong);
            Assert.Equal(OgDatabaseIdentityCodes.Refused, guess.Reference);

            var (_, stillUntil, _) = ReadLock(databaseFile, "sysadmin");
            Assert.Equal(until, stillUntil);
        });
    }

    [Fact]
    public async Task A_successful_sign_in_clears_the_count()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);

            for (var attempt = 1; attempt < OgSignInPolicy.MaximumFailures; attempt++)
            {
                Assert.False((await identity.SignInAsync("sysadmin", Wrong)).Success);
            }

            var accepted = await identity.SignInAsync("sysadmin", Password);
            Assert.True(accepted.Success, accepted.Message);

            var (locked, until, count) = ReadLock(databaseFile, "sysadmin");

            Assert.False(locked);
            Assert.Null(until);
            Assert.Equal(0, count);
        });
    }

    [Fact]
    public async Task An_expired_lock_opens_with_the_owners_password_and_is_cleared()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);

            // Lock it, then move the lock into the past — exactly what waiting does.
            for (var attempt = 0; attempt < OgSignInPolicy.MaximumFailures; attempt++)
            {
                Assert.False((await identity.SignInAsync("sysadmin", Wrong)).Success);
            }

            WriteLock(databaseFile, "sysadmin", DateTime.UtcNow.AddMinutes(-1), locked: "Y", count: OgSignInPolicy.MaximumFailures);

            var accepted = await identity.SignInAsync("sysadmin", Password);

            Assert.True(accepted.Success, accepted.Message);

            var (locked, until, count) = ReadLock(databaseFile, "sysadmin");

            Assert.False(locked);
            Assert.Null(until);
            Assert.Equal(0, count);
        });
    }

    [Fact]
    public async Task An_administrator_can_clear_the_lock_before_it_expires()
    {
        await WithDatabase(async (databaseFile, reporter, identity, administration) =>
        {
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "sysadmin", "System Administrator", Password, "setup")).Success);
            Assert.True((await identity.CreateSystemAdministratorAsync(
                "operator", "Operator", Password, "setup")).Success);

            for (var attempt = 0; attempt < OgSignInPolicy.MaximumFailures; attempt++)
            {
                Assert.False((await identity.SignInAsync("operator", Wrong)).Success);
            }

            var listed = await administration.ListAsync();
            var operatorRow = listed.Value!.Single(user => user.UserName == "operator");
            Assert.True(operatorRow.IsLocked);

            var cleared = await administration.ClearLockoutAsync("operator", "sysadmin");

            Assert.True(cleared.Success, cleared.Message);

            var accepted = await identity.SignInAsync("operator", Password);
            Assert.True(accepted.Success, accepted.Message);

            var after = await administration.ListAsync();
            Assert.False(after.Value!.Single(user => user.UserName == "operator").IsLocked);
        });
    }

    // ------------------------------------------------------------------ fixture

    private static (bool Locked, DateTimeOffset? Until, int Count) ReadLock(string databaseFile, string userName)
    {
        using var connection = new SqliteConnection($"Data Source={databaseFile}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT LOCKED_IND, LOCKOUT_UNTIL_UTC, FAILED_LOGIN_COUNT FROM USER WHERE USERNAME = $name;";
        command.Parameters.AddWithValue("$name", userName);

        using var reader = command.ExecuteReader();
        Assert.True(reader.Read(), $"no row for '{userName}'");

        var locked = reader.GetString(0) == "Y";
        var until = reader.IsDBNull(1) ? (DateTimeOffset?)null : new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(1), DateTimeKind.Utc));
        var count = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

        return (locked, until, count);
    }

    private static void WriteLock(string databaseFile, string userName, DateTime until, string locked, int count)
    {
        using var connection = new SqliteConnection($"Data Source={databaseFile}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "UPDATE USER SET LOCKED_IND = $locked, LOCKOUT_UNTIL_UTC = $until, FAILED_LOGIN_COUNT = $count " +
            "WHERE USERNAME = $name;";
        command.Parameters.AddWithValue("$locked", locked);
        command.Parameters.AddWithValue("$until", until);
        command.Parameters.AddWithValue("$count", count);
        command.Parameters.AddWithValue("$name", userName);

        Assert.Equal(1, command.ExecuteNonQuery());
    }

    private static async Task WithDatabase(
        Func<string, RecordingFailureReporter, IOgDatabaseIdentity, IOgUserAdministration, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "lockout-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLockout"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");
            var databaseFile = Path.Combine(dataDirectory, "lockout.db");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "lockout.db"));

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
                        ('ROLE-ADMIN', 'Administrator', 'Seeded by the security module', 'Y');
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
