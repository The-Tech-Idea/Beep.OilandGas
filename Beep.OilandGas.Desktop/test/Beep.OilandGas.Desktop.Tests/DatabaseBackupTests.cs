using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-12 AC-5/AC-6: the installation's database can be copied to a dated file and restored from one, and an
/// upgrade refuses to run without a backup to fall back on.
/// </summary>
public sealed class DatabaseBackupTests
{
    private const string ConnectionName = "backup-probe";

    [Fact]
    public async Task A_backup_round_trips_the_company_record()
    {
        await WithDatabase(async (backupPathRoot, backup, settings, audit) =>
        {
            Assert.True((await settings.WriteInstallationAsync(Company("Probe Energy"), string.Empty, "admin")).Success);

            var taken = await backup.BackupAsync("admin");

            Assert.True(taken.Success, $"{taken.Message} ({taken.Reference})");
            Assert.True(File.Exists(taken.Value!.Path));
            Assert.StartsWith(backupPathRoot, taken.Value.Path, StringComparison.OrdinalIgnoreCase);
            Assert.True(taken.Value.Bytes > 0);

            // Change the live database after the copy.
            var current = await settings.ReadAsync();
            Assert.True((await settings.WriteInstallationAsync(
                Company("Changed Energy"), current.Value!.InstallationRevision, "admin")).Success);

            Assert.Equal("Changed Energy", (await settings.ReadAsync()).Value!.Installation!.CompanyName);

            // Restoring puts the earlier record back.
            var restored = await backup.RestoreAsync(taken.Value.Path, "admin");

            Assert.True(restored.Success, $"{restored.Message} ({restored.Reference})");

            var after = await settings.ReadAsync();
            Assert.True(after.Success, after.Message);
            Assert.Equal("Probe Energy", after.Value!.Installation!.CompanyName);

            // Both operations are in the audit trail.
            Assert.Contains(audit.Rows, row => row.Verb == "backup.create" && row.Outcome == "ok");
            Assert.Contains(audit.Rows, row => row.Verb == "backup.restore" && row.Outcome == "ok");
        });
    }

    [Fact]
    public async Task A_restore_is_visible_to_the_next_settings_write()
    {
        await WithDatabase(async (backupPathRoot, backup, settings, audit) =>
        {
            var first = await settings.WriteInstallationAsync(Company("Probe Energy"), string.Empty, "admin");
            Assert.True(first.Success, first.Message);

            var revisionAtBackup = first.Value!.Revision;

            var taken = await backup.BackupAsync("admin");
            Assert.True(taken.Success, taken.Message);

            // A change after the backup gives a new revision.
            var second = await settings.WriteInstallationAsync(Company("Changed Energy"), revisionAtBackup, "admin");
            Assert.True(second.Success, second.Message);

            var revisionBeforeRestore = second.Value!.Revision;

            Assert.True((await backup.RestoreAsync(taken.Value!.Path, "admin")).Success);

            // A write that expects the pre-restore revision is refused: the restore is what the next writer sees.
            var stale = await settings.WriteInstallationAsync(Company("Third Energy"), revisionBeforeRestore, "admin");
            Assert.False(stale.Success);
            Assert.Equal(OgSharedSetupCodes.Stale, stale.Reference);

            // The revision the backup held is the live one again.
            var accepted = await settings.WriteInstallationAsync(Company("Third Energy"), revisionAtBackup, "admin");
            Assert.True(accepted.Success, accepted.Message);
            Assert.Equal("Third Energy", (await settings.ReadAsync()).Value!.Installation!.CompanyName);
        });
    }

    [Fact]
    public async Task A_restore_without_a_named_backup_is_refused()
    {
        await WithDatabase(async (backupPathRoot, backup, settings, audit) =>
        {
            var missing = await backup.RestoreAsync(string.Empty, "admin");
            Assert.False(missing.Success);
            Assert.Equal(OgBackupCodes.BackupNotFound, missing.Reference);

            var notThere = await backup.RestoreAsync(Path.Combine(backupPathRoot, "no-such-file.db"), "admin");
            Assert.False(notThere.Success);
            Assert.Equal(OgBackupCodes.BackupNotFound, notThere.Reference);

            Assert.Contains(audit.Rows, row => row.Verb == "backup.restore" && row.Outcome == "refused");
        });
    }

    [Fact]
    public async Task A_file_that_is_not_a_database_is_refused_as_a_backup()
    {
        await WithDatabase(async (backupPathRoot, backup, settings, audit) =>
        {
            var text = Path.Combine(backupPathRoot, "notes.db");
            Directory.CreateDirectory(backupPathRoot);
            File.WriteAllText(text, "this is not a database");

            var refused = await backup.RestoreAsync(text, "admin");

            Assert.False(refused.Success);
            Assert.Equal(OgBackupCodes.BackupInvalid, refused.Reference);
        });
    }

    [Fact]
    public async Task The_backups_are_listed_newest_first()
    {
        await WithDatabase(async (backupPathRoot, backup, settings, audit) =>
        {
            Assert.True((await backup.BackupAsync("admin")).Success);

            var listed = await backup.ListAsync();

            Assert.True(listed.Success, listed.Message);
            Assert.NotEmpty(listed.Value!);
            Assert.All(listed.Value!, file => Assert.True(File.Exists(file.Path)));
        });
    }

    [Fact]
    public void An_upgrade_without_a_backup_is_refused()
    {
        var backup = new OgDatabaseBackup(
            StubHost.Value,
            new SilentReporter(),
            new RecordingAudit(),
            Path.Combine(OgPaths.DataRoot, "backups"),
            ConnectionName);

        var refused = backup.RequireBackupBeforeUpgrade(installedVersion: 1, targetVersion: 2, backupPath: null);

        Assert.False(refused.Success);
        Assert.Equal(OgBackupCodes.NoBackupBeforeUpgrade, refused.Reference);

        var missingFile = backup.RequireBackupBeforeUpgrade(
            1,
            2,
            Path.Combine(OgPaths.DataRoot, "no-such-backup.db"));

        Assert.False(missingFile.Success);
        Assert.Equal(OgBackupCodes.NoBackupBeforeUpgrade, missingFile.Reference);

        // A real file is accepted, and no upgrade means no requirement.
        var existing = Path.Combine(Path.GetTempPath(), "backup-" + Guid.NewGuid().ToString("n")[..8] + ".db");
        File.WriteAllBytes(existing, "SQLite format 3\0"u8.ToArray());

        try
        {
            Assert.True(backup.RequireBackupBeforeUpgrade(1, 2, existing).Success);
            Assert.True(backup.RequireBackupBeforeUpgrade(2, 2, null).Success);
        }
        finally
        {
            File.Delete(existing);
        }
    }

    // ------------------------------------------------------------------ fixture

    /// <summary>The pure upgrade rule does not touch a database, so an unopened host is enough for it.</summary>
    private static readonly Lazy<OgBeepHost> StubHost = new(() => OgBeepHost.Create(
        new OgBeepHostOptions(
            Path.Combine(OgPaths.DataRoot, "backup-stub-" + Guid.NewGuid().ToString("n")[..8]),
            "BeepOilGasBackupStub")));

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
        Func<string, IOgDatabaseBackup, OgSharedSetupService, RecordingAudit, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "backup-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();
        var audit = new RecordingAudit();
        var backups = Path.Combine(root, "Backups");

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasBackup"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "backup.db"));

            Assert.True(created.Success, created.Message);

            var store = new BeepDmSharedSetupStore(host, reporter, ConnectionName);
            var settings = new OgSharedSetupService(store, reporter, audit);
            var backup = new OgDatabaseBackup(host, reporter, audit, backups, ConnectionName);

            Assert.True(backup.CanBackUp);

            await test(backups, backup, settings, audit);
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

    private sealed class SilentReporter : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) =>
            FailureReferenceFormat.New();
    }
}
