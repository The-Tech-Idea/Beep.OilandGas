using System.Text.Json;
using System.Text.Json.Nodes;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;

/// <summary>What the shared records currently say. A null member means "not recorded", never "default".</summary>
public sealed record OgSharedSettingsSnapshot(
    OgInstallationSettings? Installation,
    string? ToolsEnvironmentJson,
    IReadOnlyList<string> Modules,
    string InstallationRevision,
    string ToolsRevision,
    string ModulesRevision)
{
    /// <summary>True when this database has a company record, so a workstation may adopt it.</summary>
    public bool HasInstallation => Installation is not null;
}

/// <summary>
/// The three shared records as one installation view, for the first-run wizard, the startup load and the
/// settings page. It is a thin composition over <see cref="IOgSharedSetupStore"/>; every rule about revisions,
/// identity and idempotency stays in the store and in the module's own contract.
/// </summary>
public sealed class OgSharedSetupService
{
    private readonly IOgSharedSetupStore _store;
    private readonly IFailureReporter _failures;
    private readonly IOgAuditService _audit;

    /// <summary>
    /// The audit trail is a constructor dependency, not something a page adds afterwards: a save that succeeds and
    /// a save that is refused each leave exactly one row, written in the same operation, whoever the caller is - the
    /// settings page, the first-run wizard or a test.
    /// </summary>
    public OgSharedSetupService(IOgSharedSetupStore store, IFailureReporter failures, IOgAuditService audit)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
    }

    public IOgSharedSetupStore Store => _store;

    public string ConnectionName => _store.ConnectionName;

    /// <summary>
    /// Reads the company record from its stored document. A document this build cannot read is refused rather
    /// than half-applied, so a newer server record never turns into a company with the wrong currency — and the
    /// refusal is recorded, because it means the database holds something this build does not understand.
    /// </summary>
    public OgInstallationSettings? ParseInstallation(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<OgInstallationSettings>(json);

            // A record without a company is not an installation; adopting it would blank the company name.
            return string.IsNullOrWhiteSpace(parsed?.CompanyName) ? null : parsed;
        }
        catch (JsonException ex)
        {
            _failures.ReportHandled(
                ex,
                "reading the shared company record",
                "the record is treated as not recorded",
                FailureSeverity.Degraded);

            return null;
        }
    }

    /// <summary>
    /// The module ids in a stored record. An unreadable document is empty — and recorded, because it means the
    /// database holds a module record this build does not understand.
    /// </summary>
    public IReadOnlyList<string> ParseModules(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonNode.Parse(json) is JsonObject document
                   && document["modules"] is JsonArray array
                ? array
                    .Select(node => node is JsonValue value && value.TryGetValue<string>(out var id) ? id : null)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id!)
                    .ToList()
                : [];
        }
        catch (JsonException ex)
        {
            _failures.ReportHandled(
                ex,
                "reading the shared module record",
                "no module set is shown for this database",
                FailureSeverity.Degraded);

            return [];
        }
    }

    /// <summary>
    /// Reads everything the application shares. The first record that cannot be read stops the snapshot, because
    /// adopting half an installation would give one workstation a company with another database's calendar.
    /// </summary>
    public async Task<OgResult<OgSharedSettingsSnapshot>> ReadAsync(CancellationToken cancellationToken = default)
    {
        var installation = await _store
            .ReadAsync(OgSharedSetup.CompanyId, cancellationToken)
            .ConfigureAwait(false);

        if (!installation.Success)
        {
            return OgResult<OgSharedSettingsSnapshot>.Failed(
                installation.Message ?? "The shared settings could not be read.",
                installation.Reference ?? OgSharedSetupCodes.ReadFailed);
        }

        var tools = await _store
            .ReadAsync(OgSharedSetup.ToolsEnvironmentId, cancellationToken)
            .ConfigureAwait(false);

        if (!tools.Success)
        {
            return OgResult<OgSharedSettingsSnapshot>.Failed(
                tools.Message ?? "The shared environment could not be read.",
                tools.Reference ?? OgSharedSetupCodes.ReadFailed);
        }

        var modules = await _store
            .ReadAsync(OgSharedSetup.ModulesId, cancellationToken)
            .ConfigureAwait(false);

        if (!modules.Success)
        {
            return OgResult<OgSharedSettingsSnapshot>.Failed(
                modules.Message ?? "The shared module record could not be read.",
                modules.Reference ?? OgSharedSetupCodes.ReadFailed);
        }

        return OgResult<OgSharedSettingsSnapshot>.Ok(new OgSharedSettingsSnapshot(
            ParseInstallation(installation.Value?.SettingsJson),
            tools.Value?.SettingsJson,
            ParseModules(modules.Value?.SettingsJson),
            installation.Value?.Revision ?? string.Empty,
            tools.Value?.Revision ?? string.Empty,
            modules.Value?.Revision ?? string.Empty));
    }

    /// <summary>
    /// The schema version of the settings in this database. A database written before versioning carries none and is
    /// version 1; a missing settings table means the same thing.
    /// </summary>
    public async Task<OgResult<int>> ReadSchemaVersionAsync(CancellationToken cancellationToken = default)
    {
        var read = await _store.ReadAsync(OgSharedSetup.SystemId, cancellationToken).ConfigureAwait(false);

        if (!read.Success)
        {
            // "No table" and "no row" both mean the installation predates versioning; any other failure is a failure.
            return string.Equals(read.Reference, OgSharedSetupCodes.TableMissing, StringComparison.Ordinal)
                ? OgResult<int>.Ok(OgSetupCompatibility.Unversioned)
                : OgResult<int>.Failed(
                    read.Message ?? "The settings version could not be read.",
                    read.Reference ?? OgSharedSetupCodes.ReadFailed);
        }

        return OgResult<int>.Ok(ParseSchemaVersion(read.Value?.SettingsJson));
    }

    /// <summary>Writes the schema version this build produces, so a later build can tell where it stands.</summary>
    public Task<OgResult<OgSharedSetupRecord>> WriteSchemaVersionAsync(
        string expectedRevision,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var document = new JsonObject
        {
            ["schemaVersion"] = OgSetupCompatibility.CurrentSchemaVersion,
            ["writtenUtc"] = DateTimeOffset.UtcNow.ToString("O"),
        };

        return SaveAsync(
            OgSharedSetupChange.For(OgSharedSetup.SystemId, document.ToJsonString(), expectedRevision),
            actor,
            expectedRevision,
            cancellationToken);
    }

    /// <summary>The version in a stored document; absent, unreadable or nonsense reads as version 1.</summary>
    public int ParseSchemaVersion(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return OgSetupCompatibility.Unversioned;
        }

        try
        {
            return JsonNode.Parse(json) is JsonObject document
                   && document["schemaVersion"] is JsonValue value
                   && value.TryGetValue<int>(out var version)
                   && version > 0
                ? version
                : OgSetupCompatibility.Unversioned;
        }
        catch (JsonException ex)
        {
            // A version this build cannot read is treated as the oldest, and recorded: it means the database holds
            // something this build does not understand.
            _failures.ReportHandled(
                ex,
                "reading the settings schema version",
                "the version is treated as 1",
                FailureSeverity.Degraded);

            return OgSetupCompatibility.Unversioned;
        }
    }

    /// <summary>
    /// How far an interrupted first-run setup got, or -1 when there is no record. The wizard opens at the step after
    /// it, so a person who closed the application at the Company step is not made to answer the Database step again.
    /// </summary>
    public async Task<OgResult<int>> ReadSetupProgressAsync(CancellationToken cancellationToken = default)
    {
        var read = await _store.ReadAsync(OgSharedSetup.SetupProgressId, cancellationToken).ConfigureAwait(false);

        if (!read.Success)
        {
            // No table and no row both mean "no progress to resume"; anything else is a real failure.
            return string.Equals(read.Reference, OgSharedSetupCodes.TableMissing, StringComparison.Ordinal)
                ? OgResult<int>.Ok(-1)
                : OgResult<int>.Failed(
                    read.Message ?? "The setup progress could not be read.",
                    read.Reference ?? OgSharedSetupCodes.ReadFailed);
        }

        if (read.Value is null)
        {
            return OgResult<int>.Ok(-1);
        }

        return OgResult<int>.Ok(ParseLastCompletedStep(read.Value.SettingsJson));
    }

    /// <summary>Records the last step the person accepted, so an interruption resumes there.</summary>
    public async Task<OgResult> WriteSetupProgressAsync(
        int lastCompletedStep,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var current = await _store
            .ReadAsync(OgSharedSetup.SetupProgressId, cancellationToken)
            .ConfigureAwait(false);

        if (!current.Success
            && !string.Equals(current.Reference, OgSharedSetupCodes.TableMissing, StringComparison.Ordinal))
        {
            return OgResult.Failed(
                current.Message ?? "The setup progress could not be read.",
                current.Reference ?? OgSharedSetupCodes.ReadFailed);
        }

        var document = new JsonObject
        {
            ["lastCompletedStep"] = lastCompletedStep,
            ["savedUtc"] = DateTimeOffset.UtcNow.ToString("O"),
        };

        var saved = await SaveAsync(
                OgSharedSetupChange.For(
                    OgSharedSetup.SetupProgressId,
                    document.ToJsonString(),
                    current.Value?.Revision ?? string.Empty),
                actor,
                current.Value?.Revision ?? string.Empty,
                cancellationToken)
            .ConfigureAwait(false);

        return saved.Success
            ? OgResult.Ok()
            : OgResult.Failed(
                saved.Message ?? "The setup progress was not recorded.",
                saved.Reference ?? OgSharedSetupCodes.WriteFailed);
    }

    /// <summary>Removes the progress record once the installation exists.</summary>
    public async Task<OgResult> ClearSetupProgressAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        var read = await _store
            .ReadAsync(OgSharedSetup.SetupProgressId, cancellationToken)
            .ConfigureAwait(false);

        if (!read.Success && string.Equals(read.Reference, OgSharedSetupCodes.TableMissing, StringComparison.Ordinal))
        {
            return OgResult.Ok();
        }

        return read.Value is null
            ? OgResult.Ok()
            : await DeleteDocumentAsync(OgSharedSetup.SetupProgressId, read.Value.Revision, actor, cancellationToken)
                .ConfigureAwait(false);
    }

    /// <summary>The step number in a stored progress document; an unreadable document means "no progress".</summary>
    private int ParseLastCompletedStep(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return -1;
        }

        try
        {
            return JsonNode.Parse(json) is JsonObject document
                   && document["lastCompletedStep"] is JsonValue value
                   && value.TryGetValue<int>(out var step)
                ? step
                : -1;
        }
        catch (JsonException ex)
        {
            _failures.ReportHandled(
                ex,
                "reading the setup progress",
                "the wizard starts from the beginning",
                FailureSeverity.Degraded);

            return -1;
        }
    }

    /// <summary>
    /// Saves one document and writes the audit row for it - the same row for a success and for a refusal, so the
    /// trail answers "who tried to change which document, from which revision, and did it happen".
    /// </summary>
    private async Task<OgResult<OgSharedSetupRecord>> SaveAsync(
        OgSharedSetupChange change,
        string actor,
        string expectedRevision,
        CancellationToken cancellationToken)
    {
        var saved = await _store.SaveAsync(change, actor, cancellationToken).ConfigureAwait(false);

        _audit.Add(
            "settings.save",
            "settings",
            change.SettingsId,
            saved.Success ? "ok" : "refused",
            saved.Success
                ? $"previous={(string.IsNullOrWhiteSpace(expectedRevision) ? "(none)" : expectedRevision)}; revision={saved.Value?.Revision}"
                : $"{saved.Reference}: {saved.Message}");

        return saved;
    }

    /// <summary>Removes a document (a rollback) and records who did it and which revision was removed.</summary>
    public async Task<OgResult> DeleteDocumentAsync(
        string settingsId,
        string expectedRevision,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var removed = await _store.DeleteAsync(settingsId, expectedRevision, cancellationToken).ConfigureAwait(false);

        _audit.Add(
            "settings.delete",
            "settings",
            settingsId,
            removed.Success ? "ok" : "refused",
            removed.Success
                ? $"revision={expectedRevision}"
                : $"{removed.Reference}: {removed.Message}");

        return removed;
    }

    /// <summary>
    /// Records the company on a company database. The caller passes the revision it read (empty for a database
    /// with no record yet), so this never overwrites a record another workstation wrote in between.
    /// </summary>
    public Task<OgResult<OgSharedSetupRecord>> WriteInstallationAsync(
        OgInstallationSettings settings,
        string expectedRevision,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return SaveAsync(
            OgSharedSetupChange.For(OgSharedSetup.CompanyId, settings.ToJson(), expectedRevision),
            actor,
            expectedRevision,
            cancellationToken);
    }

    /// <summary>Records the OilandGasTools environment document every user of this database shares.</summary>
    public Task<OgResult<OgSharedSetupRecord>> WriteToolsEnvironmentAsync(
        string environmentJson,
        string expectedRevision,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(environmentJson))
        {
            return Task.FromResult(OgResult<OgSharedSetupRecord>.Failed(
                "The environment document is empty.",
                OgSharedSetupCodes.Invalid));
        }

        return SaveAsync(
            OgSharedSetupChange.For(OgSharedSetup.ToolsEnvironmentId, environmentJson, expectedRevision),
            actor,
            expectedRevision,
            cancellationToken);
    }

    /// <summary>
    /// Records which PPDM modules this database contains, so a later workstation knows what it is joining. The
    /// reference lists themselves (LOV and the PPDM code tables) are the module setup's own data and are already
    /// shared by living in this database; this record names the set, it does not copy it.
    /// </summary>
    public Task<OgResult<OgSharedSetupRecord>> WriteModulesAsync(
        IReadOnlyList<string> moduleIds,
        string expectedRevision,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(moduleIds);

        var document = new JsonObject
        {
            ["modules"] = new JsonArray(moduleIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => (JsonNode?)JsonValue.Create(id.Trim()))
                .ToArray()),
            ["recordedUtc"] = DateTimeOffset.UtcNow.ToString("O"),
        };

        return SaveAsync(
            OgSharedSetupChange.For(OgSharedSetup.ModulesId, document.ToJsonString(), expectedRevision),
            actor,
            expectedRevision,
            cancellationToken);
    }
}
