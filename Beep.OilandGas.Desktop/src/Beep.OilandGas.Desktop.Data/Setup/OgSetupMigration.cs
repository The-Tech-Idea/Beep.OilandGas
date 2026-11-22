using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;

/// <summary>What a migration run did.</summary>
/// <param name="FromVersion">The version the database was at.</param>
/// <param name="ToVersion">The version it is at now.</param>
/// <param name="Rewritten">The documents this run rewrote.</param>
/// <param name="Message">One sentence for the person or the log.</param>
public sealed record OgSetupMigrationOutcome(
    int FromVersion,
    int ToVersion,
    IReadOnlyList<string> Rewritten,
    string Message);

/// <summary>
/// Brings an installation created by an earlier build to the shape this build expects.
/// </summary>
/// <remarks>
/// <para>
/// There is <b>one</b> version marker: the <c>SYSTEM</c> row says which shape the database is in. A database written
/// by a <b>newer</b> build is refused before anything is read. An older one is migrated by re-saving each document
/// the database already holds through the ordinary service — which validates it, writes it with the revision it read
/// and leaves the ordinary audit rows — and then stamping the version last, so a failed step leaves the old version
/// in place.
/// </para>
/// <para>
/// No document carries its own version. Two markers would be two answers to the same question, and the document one
/// buys nothing: a document is only ever read through a database whose version has already been checked.
/// </para>
/// </remarks>
public interface IOgSetupMigration
{
    /// <summary>Runs the migration; a database already current is reported as nothing to do.</summary>
    Task<OgResult<OgSetupMigrationOutcome>> MigrateAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class OgSetupMigration : IOgSetupMigration
{
    private readonly OgSharedSetupService _settings;
    private readonly IFailureReporter _failures;
    private readonly IOgAuditService _audit;

    public OgSetupMigration(
        OgSharedSetupService settings,
        IFailureReporter failures,
        IOgAuditService audit)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
    }

    public async Task<OgResult<OgSetupMigrationOutcome>> MigrateAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = await _settings.ReadSchemaVersionAsync(cancellationToken).ConfigureAwait(false);

            if (!current.Success)
            {
                return OgResult<OgSetupMigrationOutcome>.Failed(
                    current.Message ?? "The database version could not be read.",
                    current.Reference ?? OgSharedSetupCodes.ReadFailed);
            }

            var from = current.Value;

            if (OgSetupCompatibility.Verdict(from) == OgSetupCompatibilityVerdict.WrittenByNewerBuild)
            {
                var refusal =
                    $"This database was written by a newer build (version {from}; this build understands "
                    + $"{OgSetupCompatibility.CurrentSchemaVersion}). Install the newer build; nothing was changed.";

                _audit.Add("setup.migrate", "database", _settings.ConnectionName, "refused", refusal);

                return OgResult<OgSetupMigrationOutcome>.Failed(refusal, OgSetupCompatibility.CodeDatabaseNewer);
            }

            if (from >= OgSetupCompatibility.CurrentSchemaVersion)
            {
                var nothing = new OgSetupMigrationOutcome(
                    from,
                    from,
                    [],
                    $"The database is already at version {from}; there is nothing to migrate.");

                _audit.Add("setup.migrate", "database", _settings.ConnectionName, "ok", nothing.Message);

                return OgResult<OgSetupMigrationOutcome>.Ok(nothing);
            }

            var snapshot = await _settings.ReadAsync(cancellationToken).ConfigureAwait(false);

            if (!snapshot.Success || snapshot.Value is null)
            {
                return Failed(
                    snapshot.Message ?? "The settings could not be read.",
                    snapshot.Reference,
                    from);
            }

            var rewritten = new List<string>();

            // The company record: re-saved only when this build can read it, so an unreadable record is left alone
            // rather than replaced with an empty one.
            if (snapshot.Value.Installation is { } company)
            {
                var saved = await _settings
                    .WriteInstallationAsync(company, snapshot.Value.InstallationRevision, actor, cancellationToken)
                    .ConfigureAwait(false);

                if (!saved.Success)
                {
                    return Failed(saved.Message, saved.Reference, from, rewritten);
                }

                rewritten.Add(OgSharedSetup.CompanyId);
            }

            if (snapshot.Value.ToolsEnvironmentJson is { Length: > 0 } environment)
            {
                var saved = await _settings
                    .WriteToolsEnvironmentAsync(environment, snapshot.Value.ToolsRevision, actor, cancellationToken)
                    .ConfigureAwait(false);

                if (!saved.Success)
                {
                    return Failed(saved.Message, saved.Reference, from, rewritten);
                }

                rewritten.Add(OgSharedSetup.ToolsEnvironmentId);
            }

            if (snapshot.Value.Modules.Count > 0)
            {
                var saved = await _settings
                    .WriteModulesAsync(snapshot.Value.Modules, snapshot.Value.ModulesRevision, actor, cancellationToken)
                    .ConfigureAwait(false);

                if (!saved.Success)
                {
                    return Failed(saved.Message, saved.Reference, from, rewritten);
                }

                rewritten.Add(OgSharedSetup.ModulesId);
            }

            // The version last: a failure above leaves the database saying it is still the old shape.
            var stamped = await _settings
                .WriteSchemaVersionAsync(string.Empty, actor, cancellationToken)
                .ConfigureAwait(false);

            if (!stamped.Success)
            {
                return Failed(stamped.Message, stamped.Reference, from, rewritten);
            }

            var outcome = new OgSetupMigrationOutcome(
                from,
                OgSetupCompatibility.CurrentSchemaVersion,
                rewritten,
                rewritten.Count == 0
                    ? $"Migrated from version {from} to {OgSetupCompatibility.CurrentSchemaVersion}; there was no document to rewrite."
                    : $"Migrated from version {from} to {OgSetupCompatibility.CurrentSchemaVersion}: {string.Join(", ", rewritten)}.");

            _audit.Add("setup.migrate", "database", _settings.ConnectionName, "ok", outcome.Message);

            return OgResult<OgSetupMigrationOutcome>.Ok(outcome);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                "migrating the installation's settings",
                "the database still holds its previous shape",
                FailureSeverity.Error);

            _audit.Add("setup.migrate", "database", _settings.ConnectionName, "refused", reference);

            return OgResult<OgSetupMigrationOutcome>.Failed(
                $"The settings could not be migrated. Reference: {reference}",
                reference);
        }

    }

    private OgResult<OgSetupMigrationOutcome> Failed(
        string? message,
        string? reference,
        int from,
        IReadOnlyList<string>? rewritten = null)
    {
        var list = rewritten ?? [];
        var failure = message ?? "The settings could not be migrated.";

        _audit.Add(
            "setup.migrate",
            "database",
            _settings.ConnectionName,
            "refused",
            list.Count == 0 ? failure : $"{failure} (already rewritten: {string.Join(", ", list)})");

        return OgResult<OgSetupMigrationOutcome>.Failed(
            list.Count == 0 ? failure : $"{failure} These were already rewritten: {string.Join(", ", list)}.",
            reference ?? OgSharedSetupCodes.WriteFailed);
    }
}
