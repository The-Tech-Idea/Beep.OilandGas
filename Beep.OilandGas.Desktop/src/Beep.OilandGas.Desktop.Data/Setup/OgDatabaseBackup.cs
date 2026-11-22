using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;

/// <summary>One backup file the installation holds.</summary>
/// <param name="Path">The file, under the data root.</param>
/// <param name="TakenUtc">When it was taken, from its name and its timestamp.</param>
/// <param name="Bytes">Its size, so a person can tell a real copy from an empty one.</param>
public sealed record OgBackupFile(string Path, DateTimeOffset TakenUtc, long Bytes);

/// <summary>
/// Copies the installation's database to a dated file and restores from one.
/// </summary>
/// <remarks>
/// <para>
/// A file database is the whole installation — the company record, the settings, the accounts and the module data —
/// so a copy of the file taken with the connection closed is a complete backup. A server database is the DBA's to
/// back up: this service refuses rather than copying something it cannot see.
/// </para>
/// <para>
/// The connection is closed around both operations. SQLite keeps the file open while connected, and copying a file
/// somebody is writing produces a corrupt backup that looks successful.
/// </para>
/// <para>
/// Restoring is a destructive act and says so: it overwrites the live database with the file named, records the
/// operation in the audit trail, and refuses a file that is not a SQLite database.
/// </para>
/// </remarks>
public interface IOgDatabaseBackup
{
    /// <summary>The directory backups are written to and restored from.</summary>
    string BackupDirectory { get; }

    /// <summary>True when the installation's database is a file this machine can copy.</summary>
    bool CanBackUp { get; }

    Task<OgResult<OgBackupFile>> BackupAsync(string actor, CancellationToken cancellationToken = default);

    Task<OgResult<IReadOnlyList<OgBackupFile>>> ListAsync(CancellationToken cancellationToken = default);

    Task<OgResult> RestoreAsync(
        string backupPath,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refuses an upgrade that has no backup to fall back on. Pure, so the rule is tested without a database.
    /// </summary>
    OgResult RequireBackupBeforeUpgrade(int installedVersion, int targetVersion, string? backupPath);
}

/// <summary>The result codes the settings page and the upgrade path branch on.</summary>
public static class OgBackupCodes
{
    public const string NotAFileDatabase = "backup-not-a-file-database";
    public const string BackupFailed = "backup-failed";
    public const string BackupNotFound = "backup-not-found";
    public const string BackupInvalid = "backup-invalid";
    public const string RestoreFailed = "restore-failed";
    public const string NoBackupBeforeUpgrade = "upgrade-no-backup";
    public const string ListFailed = "backup-list-failed";
}

/// <inheritdoc />
public sealed class OgDatabaseBackup : IOgDatabaseBackup
{
    /// <summary>The signature every SQLite file begins with.</summary>
    private static readonly byte[] SqliteHeader = "SQLite format 3\0"u8.ToArray();

    private readonly OgBeepHost _host;
    private readonly IFailureReporter _failures;
    private readonly IOgAuditService _audit;
    private readonly string _connectionName;

    public OgDatabaseBackup(
        OgBeepHost host,
        IFailureReporter failures,
        IOgAuditService audit,
        string backupDirectory,
        string connectionName)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        ArgumentException.ThrowIfNullOrWhiteSpace(backupDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);

        BackupDirectory = Path.GetFullPath(backupDirectory);
        _connectionName = connectionName;
    }

    public string BackupDirectory { get; }

    public bool CanBackUp => DatabaseFile() is not null;

    public Task<OgResult<OgBackupFile>> BackupAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var file = DatabaseFile();

            if (file is null)
            {
                return Task.FromResult(OgResult<OgBackupFile>.Failed(
                    "This installation's database is on a server; its backup is the database administrator's to take.",
                    OgBackupCodes.NotAFileDatabase));
            }

            if (!File.Exists(file))
            {
                return Task.FromResult(OgResult<OgBackupFile>.Failed(
                    $"The database file is missing: {file}",
                    OgBackupCodes.BackupFailed));
            }

            Directory.CreateDirectory(BackupDirectory);

            var name = $"{_connectionName}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}{Path.GetExtension(file)}";
            var target = Path.Combine(BackupDirectory, name);

            Close();

            try
            {
                File.Copy(file, target, overwrite: false);
            }
            finally
            {
                Open();
            }

            var taken = DateTimeOffset.UtcNow;
            var bytes = new FileInfo(target).Length;

            _audit.Add(
                "backup.create",
                "database",
                Path.GetFileName(target),
                "ok",
                $"{bytes} byte(s) copied from '{Path.GetFileName(file)}'");

            return Task.FromResult(OgResult<OgBackupFile>.Ok(new OgBackupFile(target, taken, bytes)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "taking a backup of the installation's database",
                "no backup was taken",
                FailureSeverity.Error);

            _audit.Add("backup.create", "database", _connectionName, "refused", reference);

            return Task.FromResult(OgResult<OgBackupFile>.Failed(
                $"The backup could not be taken. Reference: {reference}",
                OgBackupCodes.BackupFailed));
        }
    }

    public Task<OgResult<IReadOnlyList<OgBackupFile>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(BackupDirectory))
            {
                return Task.FromResult(OgResult<IReadOnlyList<OgBackupFile>>.Ok([]));
            }

            IReadOnlyList<OgBackupFile> files = Directory
                .GetFiles(BackupDirectory, "*.db")
                .Select(path => new FileInfo(path))
                .OrderByDescending(info => info.LastWriteTimeUtc)
                .Select(info => new OgBackupFile(
                    info.FullName,
                    new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero),
                    info.Length))
                .ToList();

            return Task.FromResult(OgResult<IReadOnlyList<OgBackupFile>>.Ok(files));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "listing the installation's backups",
                "no backups are shown",
                FailureSeverity.Degraded);

            return Task.FromResult(OgResult<IReadOnlyList<OgBackupFile>>.Failed(
                $"The backups could not be listed. Reference: {reference}",
                OgBackupCodes.ListFailed));
        }
    }

    public Task<OgResult> RestoreAsync(
        string backupPath,
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(backupPath))
            {
                return Task.FromResult(OgResult.Failed(
                    "Name the backup file to restore.",
                    OgBackupCodes.BackupNotFound));
            }

            var source = Path.GetFullPath(backupPath);

            if (!File.Exists(source))
            {
                _audit.Add("backup.restore", "database", Path.GetFileName(source), "refused", "the file does not exist");

                return Task.FromResult(OgResult.Failed(
                    $"No backup file exists at '{source}'.",
                    OgBackupCodes.BackupNotFound));
            }

            if (!LooksLikeSqlite(source))
            {
                _audit.Add("backup.restore", "database", Path.GetFileName(source), "refused", "not a SQLite database");

                return Task.FromResult(OgResult.Failed(
                    "That file is not a database backup.",
                    OgBackupCodes.BackupInvalid));
            }

            var file = DatabaseFile();

            if (file is null)
            {
                return Task.FromResult(OgResult.Failed(
                    "This installation's database is on a server; its restore is the database administrator's to perform.",
                    OgBackupCodes.NotAFileDatabase));
            }

            Close();

            try
            {
                File.Copy(source, file, overwrite: true);
            }
            finally
            {
                Open();
            }

            _audit.Add(
                "backup.restore",
                "database",
                Path.GetFileName(source),
                "ok",
                $"restored over '{Path.GetFileName(file)}' by '{actor}'");

            return Task.FromResult(OgResult.Ok(
                $"Restored '{Path.GetFileName(source)}' over the installation's database."));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "restoring the installation's database",
                "the database was not restored",
                FailureSeverity.Critical);

            _audit.Add("backup.restore", "database", Path.GetFileName(backupPath ?? string.Empty), "failed", reference);

            return Task.FromResult(OgResult.Failed(
                $"The database could not be restored. Reference: {reference}",
                OgBackupCodes.RestoreFailed));
        }
    }

    public OgResult RequireBackupBeforeUpgrade(int installedVersion, int targetVersion, string? backupPath)
    {
        if (targetVersion <= installedVersion)
        {
            return OgResult.Ok("No upgrade is being performed.");
        }

        if (string.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath))
        {
            return OgResult.Failed(
                $"Upgrading from version {installedVersion} to {targetVersion} needs a backup first. Take one, then run the upgrade again.",
                OgBackupCodes.NoBackupBeforeUpgrade);
        }

        return OgResult.Ok($"Backup '{Path.GetFileName(backupPath)}' confirmed before the upgrade.");
    }

    /// <summary>
    /// The database file, when the installation's connection is a file. A server connection has no file to copy.
    /// </summary>
    private string? DatabaseFile()
    {
        var connection = _host.Editor.ConfigEditor?.DataConnections?
            .FirstOrDefault(candidate =>
                string.Equals(candidate.ConnectionName, _connectionName, StringComparison.OrdinalIgnoreCase));

        if (connection is null || !connection.IsFile)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(connection.FilePath) || string.IsNullOrWhiteSpace(connection.FileName))
        {
            return null;
        }

        return Path.Combine(connection.FilePath, connection.FileName);
    }

    /// <summary>Closes and unregisters the datasource so the file is not held while it is copied.</summary>
    private void Close()
    {
        var editor = _host.Editor;

        if (editor.GetDataSource(_connectionName) is null)
        {
            return;
        }

        editor.CloseDataSource(_connectionName);
        editor.RemoveDataDource(_connectionName);
    }

    /// <summary>Reopens the datasource after the file has been replaced.</summary>
    private void Open() => _host.Editor.OpenDataSource(_connectionName);

    private static bool LooksLikeSqlite(string path)
    {
        using var stream = File.OpenRead(path);

        if (stream.Length < SqliteHeader.Length)
        {
            return false;
        }

        var header = new byte[SqliteHeader.Length];
        var read = stream.Read(header, 0, header.Length);

        return read == header.Length && header.SequenceEqual(SqliteHeader);
    }
}
