using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;

/// <summary>Everything one company creation needs. The company document is already validated by the wizard.</summary>
public sealed record OgCompanyCreationRequest(
    string ConnectionName,
    OgInstallationSettings Company,
    string EnvironmentDocument,
    IReadOnlyList<string> ModuleIds,
    bool IncludeDemoData,
    string AdministratorUserName,
    string AdministratorDisplayName,
    string AdministratorPassword,
    string Actor,
    bool RequirePasswordChange = false);

/// <summary>
/// A company that was created (or adopted). A failure is an <see cref="OgResult"/> failure whose message names what
/// was undone and what could not be, and whose audit row carries the same detail in structured form.
/// </summary>
public sealed record OgCompanyCreationOutcome(
    bool Adopted,
    string? AdministratorUserName,
    IReadOnlyList<string> Completed,
    string Message);

/// <summary>
/// Creates a company in one all-or-nothing run: apply the module schema, write the company settings, the environment
/// and the module set, then create the system administrator in the database.
/// </summary>
/// <remarks>
/// <para>
/// OWASP's multi-tenant guidance is that onboarding is all-or-nothing — a failed step removes what was partly
/// created and records the failure. The wizard used to write the settings row and then create the administrator, so
/// a refusal at the last step left a company with no account; this type owns the sequence and its compensations.
/// </para>
/// <para>
/// Compensations are revision-checked deletes. A rollback therefore cannot remove a row another workstation wrote in
/// the meantime: it reports the conflict and leaves the row, which is the safe direction.
/// </para>
/// <para>
/// Schema creation is the one thing that cannot be undone. Module tables are not dropped by a rollback — that is
/// recorded in <see cref="OgCompanyCreationOutcome.LeftBehind"/> instead of being hidden.
/// </para>
/// </remarks>
public sealed class OgCompanyCreation
{
    private readonly OgSharedSetupService _settings;
    private readonly IOgDatabaseIdentity _identity;
    private readonly Func<IReadOnlyList<string>, OgSetupConfirmation, IProgress<OgSetupProgress>?, Task<OgResult<OgSetupOutcome>>> _applyModules;
    private readonly IFailureReporter _failures;
    private readonly IOgAuditService _audit;

    public OgCompanyCreation(
        OgSharedSetupService settings,
        IOgDatabaseIdentity identity,
        Func<IReadOnlyList<string>, OgSetupConfirmation, IProgress<OgSetupProgress>?, Task<OgResult<OgSetupOutcome>>> applyModules,
        IFailureReporter failures,
        IOgAuditService audit)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        _applyModules = applyModules ?? throw new ArgumentNullException(nameof(applyModules));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
    }

    public async Task<OgResult<OgCompanyCreationOutcome>> CreateAsync(
        OgCompanyCreationRequest request,
        IProgress<OgSetupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var completed = new List<string>();
        var undo = new List<(string Step, Func<Task<OgResult>> Reverse)>();

        try
        {
            var current = await _settings.ReadAsync(cancellationToken).ConfigureAwait(false);

            OgSharedSettingsSnapshot snapshot;

            if (current.Success && current.Value is not null)
            {
                snapshot = current.Value;
            }
            else if (string.Equals(current.Reference, OgSharedSetupCodes.TableMissing, StringComparison.Ordinal))
            {
                // The first administrator's run creates the table; "no table" means "no company yet", not an error.
                snapshot = new OgSharedSettingsSnapshot(
                    Installation: null,
                    ToolsEnvironmentJson: null,
                    Modules: [],
                    InstallationRevision: string.Empty,
                    ToolsRevision: string.Empty,
                    ModulesRevision: string.Empty);
            }
            else
            {
                var failure = current.Message ?? "The company database could not be read.";

                _audit.Add("company.setup", "company", request.Company.CompanyCode, "refused", failure);

                return OgResult<OgCompanyCreationOutcome>.Failed(
                    failure,
                    current.Reference ?? OgSharedSetupCodes.ReadFailed);
            }

            // ---- joining an existing company: adopt, write nothing --------------------------------
            if (snapshot.HasInstallation)
            {
                var version = await _settings.ReadSchemaVersionAsync(cancellationToken).ConfigureAwait(false);

                if (version.Success
                    && OgSetupCompatibility.Verdict(version.Value) == OgSetupCompatibilityVerdict.WrittenByNewerBuild)
                {
                    // A newer build wrote this database. An older one must not adopt it, let alone write it.
                    var refusal = $"This database was set up by a newer build (settings version {version.Value}; this build understands {OgSetupCompatibility.CurrentSchemaVersion}). Install the newer build.";

                    _audit.Add("company.setup", "company", request.Company.CompanyCode, "refused", refusal);

                    return OgResult<OgCompanyCreationOutcome>.Failed(
                        refusal,
                        OgSetupCompatibility.CodeDatabaseNewer);
                }

                var adopted = await EnsureAdministratorAsync(request, cancellationToken).ConfigureAwait(false);

                if (!adopted.Success || adopted.Value is null)
                {
                    _audit.Add(
                        "company.setup",
                        "company",
                        request.Company.CompanyCode,
                        "refused",
                        adopted.Message ?? "the existing company was adopted but its account directory refused the sign-in account");

                    return OgResult<OgCompanyCreationOutcome>.Failed(
                        adopted.Message ?? "The existing company was adopted but no account could be confirmed.",
                        adopted.Reference ?? OgDatabaseIdentityCodes.WriteFailed);
                }

                _audit.Add(
                    "company.setup",
                    "company",
                    request.Company.CompanyCode,
                    "adopted",
                    "the database already had a company record; nothing about the company was written");

                return OgResult<OgCompanyCreationOutcome>.Ok(new OgCompanyCreationOutcome(
                    Adopted: true,
                    AdministratorUserName: adopted.Value.UserName,
                    Completed: ["adopted existing company record"],
                    Message: "This database already has a company; it was adopted and nothing was rewritten."));
            }

            // ---- modules first: the settings and the account are rows in tables they create -------
            if (request.ModuleIds.Count > 0)
            {
                var applied = await _applyModules(
                        request.ModuleIds,
                        new OgSetupConfirmation(request.ConnectionName, request.IncludeDemoData),
                        progress)
                    .ConfigureAwait(false);

                if (!applied.Success || applied.Value is null)
                {
                    // Nothing of ours was written yet; the schema may be partially applied and that is recorded.
                    var leftBehind = new List<string>
                    {
                        "the module schema may be partly applied; module tables are not dropped by a failed setup",
                    };

                    var message = applied.Message ?? "The modules were not applied.";

                    _audit.Add(
                        "company.setup",
                        "company",
                        request.Company.CompanyCode,
                        "refused",
                        $"{message}; left: {string.Join("; ", leftBehind)}");

                    return OgResult<OgCompanyCreationOutcome>.Failed(
                        $"{message} Nothing of the company was written, but {string.Join("; ", leftBehind)}.",
                        applied.Reference ?? "company-setup-modules-failed");
                }

                completed.Add($"{applied.Value.ModulesSucceeded} module(s) applied");
            }

            // ---- the company record --------------------------------------------------------------
            var company = await _settings
                .WriteInstallationAsync(request.Company, snapshot.InstallationRevision, request.Actor, cancellationToken)
                .ConfigureAwait(false);

            if (!company.Success || company.Value is null)
            {
                return await RollBackAsync(
                        company.Message ?? "The company record was not written.",
                        company.Reference ?? OgSharedSetupCodes.WriteFailed,
                        request,
                        completed,
                        undo,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            completed.Add("company record written");
            undo.Add(("company record", () => _settings.Store.DeleteAsync(OgSharedSetup.CompanyId, company.Value.Revision, cancellationToken)));

            // ---- the shared environment ----------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(request.EnvironmentDocument))
            {
                var environment = await _settings
                    .WriteToolsEnvironmentAsync(request.EnvironmentDocument, snapshot.ToolsRevision, request.Actor, cancellationToken)
                    .ConfigureAwait(false);

                if (!environment.Success || environment.Value is null)
                {
                    return await RollBackAsync(
                            environment.Message ?? "The shared environment was not written.",
                            environment.Reference ?? OgSharedSetupCodes.WriteFailed,
                            request,
                            completed,
                            undo,
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                completed.Add("environment written");
                undo.Add(("environment", () => _settings.Store.DeleteAsync(OgSharedSetup.ToolsEnvironmentId, environment.Value.Revision, cancellationToken)));
            }

            // ---- the module set ------------------------------------------------------------------
            if (request.ModuleIds.Count > 0)
            {
                var modules = await _settings
                    .WriteModulesAsync(request.ModuleIds, snapshot.ModulesRevision, request.Actor, cancellationToken)
                    .ConfigureAwait(false);

                if (!modules.Success || modules.Value is null)
                {
                    return await RollBackAsync(
                            modules.Message ?? "The module set was not recorded.",
                            modules.Reference ?? OgSharedSetupCodes.WriteFailed,
                            request,
                            completed,
                            undo,
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                completed.Add("module set recorded");
                undo.Add(("module set", () => _settings.Store.DeleteAsync(OgSharedSetup.ModulesId, modules.Value.Revision, cancellationToken)));
            }

            // ---- the schema version ---------------------------------------------------------------
            var existingSystem = await _settings.Store
                .ReadAsync(OgSharedSetup.SystemId, cancellationToken)
                .ConfigureAwait(false);

            if (!existingSystem.Success
                && !string.Equals(existingSystem.Reference, OgSharedSetupCodes.TableMissing, StringComparison.Ordinal))
            {
                return await RollBackAsync(
                        existingSystem.Message ?? "The settings version could not be read.",
                        existingSystem.Reference ?? OgSharedSetupCodes.ReadFailed,
                        request,
                        completed,
                        undo,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var system = await _settings
                .WriteSchemaVersionAsync(existingSystem.Value?.Revision ?? string.Empty, request.Actor, cancellationToken)
                .ConfigureAwait(false);

            if (!system.Success || system.Value is null)
            {
                return await RollBackAsync(
                        system.Message ?? "The settings version was not written.",
                        system.Reference ?? OgSharedSetupCodes.WriteFailed,
                        request,
                        completed,
                        undo,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            completed.Add($"settings version {OgSetupCompatibility.CurrentSchemaVersion} recorded");
            undo.Add(("settings version", () => _settings.Store.DeleteAsync(OgSharedSetup.SystemId, system.Value.Revision, cancellationToken)));

            // ---- the system administrator --------------------------------------------------------
            var administrator = await EnsureAdministratorAsync(request, cancellationToken).ConfigureAwait(false);

            if (!administrator.Success)
            {
                return await RollBackAsync(
                        administrator.Message ?? "The administrator was not created.",
                        administrator.Reference ?? OgDatabaseIdentityCodes.WriteFailed,
                        request,
                        completed,
                        undo,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (!administrator.Value.Adopted)
            {
                completed.Add("administrator created");
                var userId = administrator.Value.UserId;
                undo.Add(("administrator", () => _identity.DeleteAccountAsync(userId, cancellationToken)));

                // The password was typed by whoever ran the setup, so the account is provisional until its owner
                // replaces it (SETUP-03 AC-4). An adopted account is not touched: it has its owner's password.
                if (request.RequirePasswordChange)
                {
                    var marked = await _identity
                        .MarkPasswordChangeRequiredAsync(administrator.Value.UserName, request.Actor, cancellationToken)
                        .ConfigureAwait(false);

                    if (!marked.Success)
                    {
                        return await RollBackAsync(
                                marked.Message ?? "The administrator was created but not marked for a password change.",
                                marked.Reference ?? OgDatabaseIdentityCodes.WriteFailed,
                                request,
                                completed,
                                undo,
                                cancellationToken)
                            .ConfigureAwait(false);
                    }

                    completed.Add("password change required at first sign-in");

                    // The marker carries a revision, so the rollback reads it before deleting: an empty revision
                    // would be refused and the marker would survive a failed setup.
                    undo.Add(("password-change marker", async () =>
                    {
                        var marker = await _settings.Store
                            .ReadAsync(OgSharedSetup.BootstrapId, cancellationToken)
                            .ConfigureAwait(false);

                        return marker.Success && marker.Value is not null
                            ? await _settings.Store
                                .DeleteAsync(OgSharedSetup.BootstrapId, marker.Value.Revision, cancellationToken)
                                .ConfigureAwait(false)
                            : OgResult.Ok();
                    }));
                }
            }
            else
            {
                completed.Add("administrator adopted (the name already existed)");
            }

            _audit.Add(
                "company.setup",
                "company",
                request.Company.CompanyCode,
                "ok",
                string.Join("; ", completed));

            return OgResult<OgCompanyCreationOutcome>.Ok(new OgCompanyCreationOutcome(
                Adopted: false,
                AdministratorUserName: administrator.Value.UserName,
                Completed: completed,
                Message: "The company was created."));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating the company '{request.Company.CompanyName}'",
                "the company was not created",
                FailureSeverity.Critical);

            return await RollBackAsync(
                    $"The company could not be created. Reference: {reference}",
                    reference,
                    request,
                    completed,
                    undo,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Creates the administrator, or adopts an existing account with that name. The role grant is only written for a
    /// new account; an adopted account keeps the roles it already has.
    /// </summary>
    private async Task<OgResult<AdministratorResult>> EnsureAdministratorAsync(
        OgCompanyCreationRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _identity
            .CreateSystemAdministratorAsync(
                request.AdministratorUserName,
                request.AdministratorDisplayName,
                request.AdministratorPassword,
                request.Actor,
                cancellationToken)
            .ConfigureAwait(false);

        if (created.Success && created.Value is not null)
        {
            return OgResult<AdministratorResult>.Ok(new AdministratorResult(created.Value.UserId, created.Value.UserName, Adopted: false));
        }

        if (string.Equals(created.Reference, OgDatabaseIdentityCodes.AdministratorExists, StringComparison.Ordinal))
        {
            // Joining an existing company: the person's own account is the one they will sign in with.
            return OgResult<AdministratorResult>.Ok(new AdministratorResult(string.Empty, request.AdministratorUserName.Trim(), Adopted: true));
        }

        return OgResult<AdministratorResult>.Failed(
            created.Message ?? "The administrator was not created.",
            created.Reference ?? OgDatabaseIdentityCodes.WriteFailed);
    }

    /// <summary>The account the setup produced, and whether it already existed.</summary>
    private sealed record AdministratorResult(string UserId, string UserName, bool Adopted);

    /// <summary>
    /// Runs the compensations newest-first and reports honestly: what was undone, and what could not be.
    /// </summary>
    private async Task<OgResult<OgCompanyCreationOutcome>> RollBackAsync(
        string message,
        string reference,
        OgCompanyCreationRequest request,
        IReadOnlyList<string> completed,
        IReadOnlyList<(string Step, Func<Task<OgResult>> Reverse)> undo,
        CancellationToken cancellationToken)
    {
        var compensated = new List<string>();
        var leftBehind = new List<string>();

        for (var index = undo.Count - 1; index >= 0; index--)
        {
            var (step, reverse) = undo[index];

            OgResult reversed;

            try
            {
                reversed = await reverse().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var failureReference = _failures.ReportHandled(
                    ex,
                    $"undoing '{step}' after a failed company creation",
                    $"'{step}' was left in the company database",
                    FailureSeverity.Error);

                reversed = OgResult.Failed(
                    $"'{step}' could not be undone. Reference: {failureReference}",
                    OgSharedSetupCodes.WriteFailed);
            }

            if (reversed.Success)
            {
                compensated.Add(step);
            }
            else
            {
                leftBehind.Add($"{step}: {reversed.Message}");
            }
        }

        if (undo.Count > 0 && compensated.Count == undo.Count)
        {
            message += " Nothing was left behind: the partly created company was removed.";
        }
        else if (leftBehind.Count > 0)
        {
            message += $" These were left in place and need attention: {string.Join("; ", leftBehind)}.";
        }

        _audit.Add(
            "company.setup",
            "company",
            request.Company.CompanyCode,
            "refused",
            $"rolled back: {(compensated.Count == 0 ? "nothing to undo" : string.Join(", ", compensated))}" +
            (leftBehind.Count == 0 ? string.Empty : $"; left: {string.Join("; ", leftBehind)}"));

        return OgResult<OgCompanyCreationOutcome>.Failed(message, reference ?? OgSharedSetupCodes.WriteFailed);
    }
}
