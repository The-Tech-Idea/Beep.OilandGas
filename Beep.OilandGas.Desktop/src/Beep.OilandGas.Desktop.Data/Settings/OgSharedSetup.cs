using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;

/// <summary>The settings documents a company database holds, by id.</summary>
public static class OgSharedSetup
{
    /// <summary>The company record: name, code, region, calendar, currency and the oil-and-gas defaults.</summary>
    public const string CompanyId = BEEPSETTINGS.Company;

    /// <summary>The shared OilandGasTools environment (the document that used to be a per-workstation file).</summary>
    public const string ToolsEnvironmentId = BEEPSETTINGS.ToolsEnvironment;

    /// <summary>Which PPDM modules — and therefore which reference lists — this database contains.</summary>
    public const string ModulesId = BEEPSETTINGS.Modules;

    /// <summary>The schema version of the settings this database holds, so an older build refuses to write it.</summary>
    public const string SystemId = BEEPSETTINGS.System;

    /// <summary>The account a setup created and the password it typed, until its owner replaces it.</summary>
    public const string BootstrapId = BEEPSETTINGS.Bootstrap;

    /// <summary>How far an interrupted setup got, so it resumes instead of starting again.</summary>
    public const string SetupProgressId = BEEPSETTINGS.SetupProgress;

    /// <summary>The table every deployment of this product reads its settings from.</summary>
    public const string TableName = BEEPSETTINGS.TableName;
}

/// <summary>
/// The company settings every user of a company database shares: what the first run recorded, and what an
/// administrator may later change.
/// </summary>
/// <param name="CompanyName">The legal or trading name shown in the shell.</param>
/// <param name="CompanyCode">The short code used on documents; unique per installation.</param>
/// <param name="CountryCode">The ISO 3166-1 alpha-2 country, as the environment presets use it.</param>
/// <param name="CultureName">The interface culture recorded for the installation.</param>
/// <param name="TimeZoneId">The <see cref="TimeZoneInfo"/> id the installation works in.</param>
/// <param name="BaseCurrencyCode">The ISO 4217 code money is reported in.</param>
/// <param name="FiscalYearStartMonth">1..12; the month the fiscal year begins.</param>
/// <param name="CompanyTypeCode">Operator, national oil company, service company and so on.</param>
/// <param name="UnitSystemCode">The measurement system the installation works in.</param>
/// <param name="DefaultFieldName">The field or asset the shell starts on; empty when none is recorded.</param>
/// <param name="RecordedBy">The account that recorded the company (the first administrator).</param>
/// <param name="RecordedAtUtc">When it was recorded, so a later reader can say how old the record is.</param>
public sealed record OgInstallationSettings(
    string CompanyName,
    string CompanyCode,
    string CountryCode,
    string CultureName,
    string TimeZoneId,
    string BaseCurrencyCode,
    int FiscalYearStartMonth,
    string CompanyTypeCode,
    string UnitSystemCode,
    string? DefaultFieldName,
    string RecordedBy,
    DateTimeOffset RecordedAtUtc)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    public string ToJson() => JsonSerializer.Serialize(this, Json);
}

/// <summary>One settings row as stored, with the revision a later write must match.</summary>
public sealed record OgSharedSetupRecord(
    string SettingsId,
    string SettingsJson,
    string Revision,
    string PreviousRevision,
    string IntentId,
    string UpdatedBy,
    DateTimeOffset? UpdatedUtc);

/// <summary>
/// A change to one settings document. <paramref name="IntentId"/> makes a retry the same change, not a second one;
/// <paramref name="ExpectedRevision"/> is the revision the writer read, so a stale writer is refused.
/// </summary>
public sealed record OgSharedSetupChange(
    string SettingsId,
    string SettingsJson,
    string ExpectedRevision,
    string IntentId)
{
    /// <summary>A change to a document the caller has just read (empty revision when there is no row yet).</summary>
    public static OgSharedSetupChange For(string settingsId, string settingsJson, string expectedRevision) =>
        new(settingsId, settingsJson, expectedRevision, Guid.NewGuid().ToString("N"));
}

/// <summary>
/// Reads and writes the company settings table in the connected database.
/// </summary>
/// <remarks>
/// <para>
/// One table, one row per document, JSON in the row: that is the whole design, so an administrator can see and
/// change a company's settings in the database without a schema of its own. The table is created by the first
/// administrator's setup run (<see cref="BEEPSETTINGS"/>'s shape comes from the entity class, so a column rename
/// breaks the build); a reader never creates it, because reading must not change a server.
/// </para>
/// <para>
/// The store is provider-agnostic: it binds the entity to the connection's table through the same unit of work
/// every other desktop store uses, so no statement text exists here.
/// </para>
/// </remarks>
public interface IOgSharedSetupStore
{
    /// <summary>The connection the settings are read from.</summary>
    string ConnectionName { get; }

    /// <summary>True when this database has the settings table at all.</summary>
    bool TableExists { get; }

    /// <summary>
    /// One document, or null when the table exists and the row does not. A missing table is a failure, not null.
    /// </summary>
    Task<OgResult<OgSharedSetupRecord?>> ReadAsync(
        string settingsId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes one document, creating the table when this is the first write. A write whose
    /// <see cref="OgSharedSetupChange.ExpectedRevision"/> does not match the stored revision is refused.
    /// </summary>
    Task<OgResult<OgSharedSetupRecord>> SaveAsync(
        OgSharedSetupChange change,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes one document. Used to undo a company creation that failed part-way; the revision is required, so a
    /// rollback can never remove a row another workstation wrote in the meantime.
    /// </summary>
    Task<OgResult> DeleteAsync(
        string settingsId,
        string expectedRevision,
        CancellationToken cancellationToken = default);
}

/// <summary>The result codes the setup screens branch on.</summary>
public static class OgSharedSetupCodes
{
    public const string TableMissing = "settings-table-missing";
    public const string TableNotCreated = "settings-table-not-created";
    public const string Unreachable = "settings-database-unreachable";
    public const string Invalid = "settings-record-invalid";
    public const string Stale = "settings-record-changed";
    public const string ReadFailed = "settings-read-failed";
    public const string WriteFailed = "settings-write-failed";
}

/// <inheritdoc />
public sealed class BeepDmSharedSetupStore : IOgSharedSetupStore
{
    private readonly OgBeepHost _host;
    private readonly IFailureReporter _failures;

    public BeepDmSharedSetupStore(OgBeepHost host, IFailureReporter failures, string connectionName)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);

        ConnectionName = connectionName;
    }

    public string ConnectionName { get; }

    public bool TableExists =>
        _host.Editor.GetDataSource(ConnectionName) is { } source && HasTable(source);

    public async Task<OgResult<OgSharedSetupRecord?>> ReadAsync(
        string settingsId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsIdentifier(settingsId))
            {
                return OgResult<OgSharedSetupRecord?>.Failed(
                    "That is not one of this installation's settings documents.",
                    OgSharedSetupCodes.Invalid);
            }

            var source = OpenSource();
            if (source is null)
            {
                return OgResult<OgSharedSetupRecord?>.Failed(
                    $"The connection '{ConnectionName}' is not open, so the company settings cannot be read.",
                    OgSharedSetupCodes.Unreachable);
            }

            if (!HasTable(source))
            {
                return OgResult<OgSharedSetupRecord?>.Failed(
                    $"The database has no {OgSharedSetup.TableName} table yet, so no company has been created in it.",
                    OgSharedSetupCodes.TableMissing);
            }

            var row = await ReadRowAsync(source, settingsId, cancellationToken).ConfigureAwait(false);

            return OgResult<OgSharedSetupRecord?>.Ok(row is null ? null : ToRecord(row));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading the company settings '{settingsId}'",
                "the company settings could not be read",
                FailureSeverity.Error);

            return OgResult<OgSharedSetupRecord?>.Failed(
                $"The company settings could not be read. Reference: {reference}",
                OgSharedSetupCodes.ReadFailed);
        }
    }

    public async Task<OgResult<OgSharedSetupRecord>> SaveAsync(
        OgSharedSetupChange change,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(change);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsIdentifier(change.SettingsId)
                || string.IsNullOrWhiteSpace(change.SettingsJson)
                || change.SettingsJson.Length > 1048576
                || !Guid.TryParseExact(change.IntentId, "N", out var intent)
                || intent == Guid.Empty
                || (change.ExpectedRevision.Length != 0 && !IsRevision(change.ExpectedRevision)))
            {
                return OgResult<OgSharedSetupRecord>.Failed(
                    "The settings change is incomplete or not in the form this database accepts.",
                    OgSharedSetupCodes.Invalid);
            }

            var source = OpenSource();
            if (source is null)
            {
                return OgResult<OgSharedSetupRecord>.Failed(
                    $"The connection '{ConnectionName}' is not open, so the settings could not be saved.",
                    OgSharedSetupCodes.Unreachable);
            }

            if (!HasTable(source))
            {
                // The first administrator's run creates the table; a later save only finds it.
                var created = TryCreateTable(source);

                if (!created.Success)
                {
                    return OgResult<OgSharedSetupRecord>.Failed(
                        created.Message ?? $"The {OgSharedSetup.TableName} table could not be created.",
                        created.Reference ?? OgSharedSetupCodes.TableNotCreated);
                }
            }

            var unit = OgUow.For<BEEPSETTINGS>(source);
            unit.PrimaryKey = nameof(BEEPSETTINGS.SETTINGS_ID);

            var existing = await OgUow
                .ReadAsync(unit, [OgUow.Equal(unit.PrimaryKey, change.SettingsId)], cancellationToken)
                .ConfigureAwait(false);

            var current = existing.Count == 1 ? existing[0] : null;

            if (existing.Count > 1)
            {
                return OgResult<OgSharedSetupRecord>.Failed(
                    $"More than one {OgSharedSetup.TableName} row has the id '{change.SettingsId}'.",
                    OgSharedSetupCodes.Invalid);
            }

            var storedRevision = current?.REVISION ?? string.Empty;

            // A revision that no longer matches is a refusal, not an overwrite: another administrator saved
            // while this screen was open, and the person is asked to reload rather than lose that change.
            if (!string.Equals(storedRevision, change.ExpectedRevision, StringComparison.Ordinal))
            {
                return OgResult<OgSharedSetupRecord>.Failed(
                    "The company settings changed since they were read. Reload them before saving.",
                    OgSharedSetupCodes.Stale);
            }

            var revision = Revision(change.SettingsId, change.SettingsJson, change.ExpectedRevision, change.IntentId);
            var now = DateTime.UtcNow;

            var row = current ?? new BEEPSETTINGS
            {
                SETTINGS_ID = change.SettingsId,
                ROW_CREATED_BY = actor,
                ROW_CREATED_DATE = now,
            };

            row.SETTINGS_JSON = change.SettingsJson;
            row.REVISION = revision;
            row.PREVIOUS_REVISION = string.IsNullOrWhiteSpace(change.ExpectedRevision) ? null : change.ExpectedRevision;
            row.INTENT_ID = change.IntentId;
            row.UPDATED_BY = actor;
            row.UPDATED_UTC = now;
            row.ACTIVE_IND = "Y";
            row.ROW_CHANGED_BY = actor;
            row.ROW_CHANGED_DATE = now;

            if (current is null)
            {
                // Add() only buffers the row; Commit() below reports whether it reached the database.
                unit.Add(row);
            }
            else
            {
                var updated = unit.Update(row);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgSharedSetupRecord>.Failed(
                        OgUow.Message(updated, "The company settings were not saved."),
                        OgSharedSetupCodes.WriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgSharedSetupRecord>.Failed(
                    OgUow.Message(committed, "The company settings were not saved."),
                    OgSharedSetupCodes.WriteFailed);
            }

            var saved = await ReadRowAsync(source, change.SettingsId, cancellationToken).ConfigureAwait(false);

            if (saved is null)
            {
                return OgResult<OgSharedSetupRecord>.Failed(
                    "The settings were written but could not be read back, so they are not confirmed.",
                    OgSharedSetupCodes.WriteFailed);
            }

            return OgResult<OgSharedSetupRecord>.Ok(ToRecord(saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving the company settings '{change.SettingsId}'",
                "the company settings were not saved",
                FailureSeverity.Error);

            return OgResult<OgSharedSetupRecord>.Failed(
                $"The company settings could not be saved. Reference: {reference}",
                OgSharedSetupCodes.WriteFailed);
        }
    }

    public async Task<OgResult> DeleteAsync(
        string settingsId,
        string expectedRevision,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsIdentifier(settingsId)
                || (expectedRevision.Length != 0 && !IsRevision(expectedRevision)))
            {
                return OgResult.Failed(
                    "That is not one of this installation's settings documents.",
                    OgSharedSetupCodes.Invalid);
            }

            var source = OpenSource();
            if (source is null)
            {
                return OgResult.Failed(
                    $"The connection '{ConnectionName}' is not open, so the document could not be removed.",
                    OgSharedSetupCodes.Unreachable);
            }

            if (!HasTable(source))
            {
                // Nothing was ever created, so there is nothing to undo.
                return OgResult.Ok();
            }

            var unit = OgUow.For<BEEPSETTINGS>(source);
            unit.PrimaryKey = nameof(BEEPSETTINGS.SETTINGS_ID);

            var rows = await OgUow
                .ReadAsync(unit, [OgUow.Equal(unit.PrimaryKey, settingsId)], cancellationToken)
                .ConfigureAwait(false);

            if (rows.Count == 0)
            {
                // Already gone: a retried rollback is the same rollback.
                return OgResult.Ok();
            }

            var row = rows[0];

            if (!string.Equals(row.REVISION, expectedRevision, StringComparison.Ordinal))
            {
                return OgResult.Failed(
                    "The company settings changed since the rollback was prepared, so nothing was removed.",
                    OgSharedSetupCodes.Stale);
            }

            var deleted = unit.Delete(row);

            if (!OgUow.IsOk(deleted))
            {
                return OgResult.Failed(
                    OgUow.Message(deleted, "The settings row was not removed."),
                    OgSharedSetupCodes.WriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok()
                : OgResult.Failed(
                    OgUow.Message(committed, "The settings row was not removed."),
                    OgSharedSetupCodes.WriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"removing the company settings '{settingsId}'",
                "the settings row was not removed",
                FailureSeverity.Error);

            return OgResult.Failed(
                $"The settings row could not be removed. Reference: {reference}",
                OgSharedSetupCodes.WriteFailed);
        }
    }

    /// <summary>
    /// Creates <see cref="OgSharedSetup.TableName"/> from the entity class through the engine's schema provider, so
    /// the table matches the columns the code binds and no statement text exists here.
    /// </summary>
    private OgResult TryCreateTable(IDataSource source)
    {
        var structure = _host.Editor.classCreator?.ConvertToEntityStructure(typeof(BEEPSETTINGS));

        if (structure is null)
        {
            return OgResult.Failed(
                "The engine could not describe the settings table, so it was not created.",
                OgSharedSetupCodes.TableNotCreated);
        }

        structure.EntityName = OgSharedSetup.TableName;
        structure.DatasourceEntityName = OgSharedSetup.TableName;

        var created = _host.Editor.GetMigrationProvider(source).CreateEntity(structure);

        if (!OgUow.IsOk(created))
        {
            return OgResult.Failed(
                created?.Message ?? $"The {OgSharedSetup.TableName} table was not created.",
                OgSharedSetupCodes.TableNotCreated);
        }

        return OgResult.Ok();
    }

    private IDataSource? OpenSource()
    {
        var editor = _host.Editor;

        var configured = editor.ConfigEditor?.DataConnections?
            .FirstOrDefault(connection =>
                string.Equals(connection.ConnectionName, ConnectionName, StringComparison.OrdinalIgnoreCase));

        if (configured is null)
        {
            return null;
        }

        var source = editor.GetDataSource(ConnectionName);

        if (source is null)
        {
            source = editor.CreateNewDataSourceConnection(configured, ConnectionName);
        }

        if (source?.Openconnection() == System.Data.ConnectionState.Open)
        {
            return source;
        }

        // A source created before its definition was written keeps the old definition. Rebuilding it from the
        // configured entry is the same recovery the module setup uses, and it is tried once before refusing.
        if (source is not null && editor.CheckDataSourceExist(ConnectionName))
        {
            editor.CloseDataSource(ConnectionName);
            editor.RemoveDataDource(ConnectionName);
        }

        source = editor.CreateNewDataSourceConnection(configured, ConnectionName);

        return source?.Openconnection() == System.Data.ConnectionState.Open ? source : null;
    }

    private bool HasTable(IDataSource source)
    {
        // A connection that cannot list its tables has no settings this build can bind, so the caller is told the
        // table is missing rather than that the settings are empty. The listing failure is still recorded.
        try
        {
            return source.GetEntitesList()
                .Any(name => string.Equals(name, OgSharedSetup.TableName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _failures.ReportHandled(
                ex,
                $"listing the tables of '{ConnectionName}'",
                "the company settings table cannot be found, so the installation falls back to its own record",
                FailureSeverity.Degraded);

            return false;
        }
    }

    private static async Task<BEEPSETTINGS?> ReadRowAsync(
        IDataSource source,
        string settingsId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<BEEPSETTINGS>(source);
        unit.PrimaryKey = nameof(BEEPSETTINGS.SETTINGS_ID);

        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal(unit.PrimaryKey, settingsId)], cancellationToken)
            .ConfigureAwait(false);

        return rows.Count switch
        {
            0 => null,
            1 => rows[0],
            _ => throw new InvalidOperationException(
                $"More than one {OgSharedSetup.TableName} row has the id '{settingsId}'."),
        };
    }

    /// <summary>
    /// The revision of one document: a hash of exactly what is being written and of the revision it replaces, so
    /// the same change twice produces the same revision and a different change never does.
    /// </summary>
    private static string Revision(string settingsId, string json, string expectedRevision, string intentId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{settingsId}\u001f{expectedRevision}\u001f{intentId}\u001f{json}")));

    private static bool IsIdentifier(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length <= 64
        && value.All(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_');

    private static bool IsRevision(string? value) =>
        value is { Length: 64 } && value.All(character => character is >= '0' and <= '9' or >= 'A' and <= 'F');

    private static OgSharedSetupRecord ToRecord(BEEPSETTINGS row) =>
        new(
            row.SETTINGS_ID,
            row.SETTINGS_JSON,
            row.REVISION,
            row.PREVIOUS_REVISION ?? string.Empty,
            row.INTENT_ID,
            row.UPDATED_BY,
            row.UPDATED_UTC is { } utc ? new DateTimeOffset(utc, TimeSpan.Zero) : null);
}
