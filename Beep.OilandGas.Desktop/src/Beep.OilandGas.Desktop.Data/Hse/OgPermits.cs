using System.Globalization;
using System.Text;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PermitsAndApplications.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;

/// <summary>One recorded status change (HSE-07, AC-6).</summary>
public sealed record OgPermitHistoryEntry(DateTime At, string State, string? Remarks, string? By)
{
    public string Describe() =>
        At.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " · " +
        State.ToLowerInvariant() +
        (string.IsNullOrWhiteSpace(By) ? string.Empty : " · " + By) +
        (string.IsNullOrWhiteSpace(Remarks) ? string.Empty : " · " + Remarks);
}

/// <summary>
/// One permit application (HSE-07), matching the feature's <c>OgPermitApplication</c>. The state is the latest recorded
/// history entry, which is the append-only record of the application's life.
/// </summary>
public sealed record OgPermitApplication(
    string Id,
    string Number,
    string Type,
    string Jurisdiction,
    string? AssetId,
    string? FieldId,
    string Scope,
    string State,
    DateTime? SubmittedAt,
    DateTime? DecidedAt,
    string? Decision,
    string? Applicant,
    IReadOnlyList<OgPermitHistoryEntry> History)
{
    /// <summary>AC-1: the transitions the owner's own rules allow from this state, and nothing else.</summary>
    public IReadOnlyList<string> LegalNextStates => PermitRules.NextStates(State);

    public bool IsDecided => string.Equals(State, PermitApplicationStatusCodes.Approved, StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(State, PermitApplicationStatusCodes.Rejected, StringComparison.OrdinalIgnoreCase);

    public string JurisdictionText => string.IsNullOrWhiteSpace(Jurisdiction) ? "not specified" : Jurisdiction;

    public string Describe() =>
        Number + " · " + Type + " · " + JurisdictionText + " · " + State.ToLowerInvariant() +
        (string.IsNullOrWhiteSpace(AssetId) ? string.Empty : " · asset " + AssetId) +
        (SubmittedAt is { } submitted
            ? " · submitted " + submitted.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : " · not submitted") +
        (DecidedAt is { } decided ? " · decided " + decided.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty);

    /// <summary>
    /// AC-5's content: the document a PDF would render, written from the record's own fields. The screen shows this same text,
    /// so the two cannot disagree.
    /// </summary>
    public string Document()
    {
        var text = new StringBuilder();

        text.AppendLine("PERMIT APPLICATION");
        text.AppendLine("Reference: " + Number);
        text.AppendLine("Application id: " + Id);
        text.AppendLine("Type: " + Type);
        text.AppendLine("Jurisdiction: " + JurisdictionText);
        text.AppendLine("Asset: " + (AssetId ?? "(none recorded)"));
        text.AppendLine("Field: " + (FieldId ?? "(none recorded)"));
        text.AppendLine("State: " + State.ToLowerInvariant());
        text.AppendLine("Applicant: " + (Applicant ?? "(none recorded)"));
        text.AppendLine("Submitted: " + (SubmittedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "not submitted"));
        text.AppendLine("Decided: " + (DecidedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "not decided"));
        text.AppendLine("Decision: " + (Decision ?? "(none recorded)"));
        text.AppendLine("Scope: " + Scope);
        text.AppendLine();
        text.AppendLine("STATUS HISTORY");

        foreach (var entry in History)
        {
            text.AppendLine("  " + entry.Describe());
        }

        return text.ToString();
    }
}

/// <summary>The permits workspace (HSE-07).</summary>
public sealed record OgPermitWorkspace(
    IReadOnlyList<OgPermitApplication> Applications,
    OgPermitApplication? Selected,
    IReadOnlyList<string> States,
    string Notice,
    DateTime ReadAt)
{
    public int Pending => Applications.Count(application =>
        !string.Equals(application.State, PermitApplicationStatusCodes.Draft, StringComparison.OrdinalIgnoreCase) &&
        !application.IsDecided &&
        !string.Equals(application.State, PermitApplicationStatusCodes.Withdrawn, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(application.State, PermitApplicationStatusCodes.Expired, StringComparison.OrdinalIgnoreCase));

    public string ListText => Applications.Count == 0
        ? "No permit application is recorded."
        : string.Join(Environment.NewLine, Applications.Select(application => application.Describe()));

    public string CountsText => Applications.Count.ToString(CultureInfo.InvariantCulture) + " application(s) · " +
                                Pending.ToString(CultureInfo.InvariantCulture) + " awaiting a decision";

    /// <summary>The feature's own state: the renderer is unavailable, so the export is disabled and says why.</summary>
    public const string RendererNotice =
        "PDF export is disabled: it needs the wkhtmltopdf native binary, which this build does not carry (REL-01 owns its " +
        "packaging). The document below is the application's content and is what the PDF would render.";

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The permit rules HSE-07 states once.</summary>
public static class PermitRules
{
    public const string CodeNoConnection = "permit-no-connection";
    public const string CodeReadFailed = "permit-read-failed";
    public const string CodeWriteFailed = "permit-write-failed";
    public const string CodeActorRequired = "permit-actor-required";
    public const string CodeInputRequired = "permit-input-required";
    public const string CodeJurisdictionRequired = "permit-jurisdiction-required";
    public const string CodeAssetRequired = "permit-asset-required";
    public const string CodeScopeRequired = "permit-scope-required";
    public const string CodeForeignAsset = "permit-foreign-asset";
    public const string CodeUnknownApplication = "permit-unknown-application";
    public const string CodeTransitionIllegal = "permit-transition-illegal";
    public const string CodeRationaleRequired = "permit-rationale-required";
    public const string CodeConditionsRequired = "permit-conditions-required";
    public const string CodeSelfDecision = "permit-self-decision";
    public const string CodeIntentConflict = "permit-intent-conflict";
    public const string CodeRendererUnavailable = "permit-pdf-renderer-unavailable";
    public const string CodeTypeInvalid = "permit-type-invalid";

    /// <summary>The nine recorded states, as the owner's storage keys.</summary>
    public static IReadOnlyList<string> States { get; } =
    [
        PermitApplicationStatusCodes.Draft,
        PermitApplicationStatusCodes.Submitted,
        PermitApplicationStatusCodes.UnderReview,
        PermitApplicationStatusCodes.AdditionalInformationRequired,
        PermitApplicationStatusCodes.Approved,
        PermitApplicationStatusCodes.Rejected,
        PermitApplicationStatusCodes.Withdrawn,
        PermitApplicationStatusCodes.Expired,
        PermitApplicationStatusCodes.Renewed,
    ];

    /// <summary>The decision states, where the applicant may not decide their own application.</summary>
    public static IReadOnlyList<string> DecisionStates { get; } =
        [PermitApplicationStatusCodes.Approved, PermitApplicationStatusCodes.Rejected];

    /// <summary>
    /// AC-1: the legal next states, decided by the owner's <see cref="PermitStatusTransitionRules"/>. The desktop does not
    /// restate the table, so a rule the owner tightens is tightened here too.
    /// </summary>
    public static IReadOnlyList<string> NextStates(string? state) =>
        States.Where(candidate => PermitStatusTransitionRules.IsTransitionAllowed(state, candidate)).ToList();

    /// <summary>The enum member for a storage key, so a header can be written without guessing at a name.</summary>
    public static PermitApplicationStatus? MemberFor(string? state)
    {
        var code = PermitStatusTransitionRules.Normalize(state);

        foreach (var member in Enum.GetValues<PermitApplicationStatus>())
        {
            if (string.Equals(PermitApplicationStatusCodes.ToStorageKey(member), code,
                    StringComparison.OrdinalIgnoreCase))
            {
                return member;
            }
        }

        return null;
    }

    /// <summary>AC-2 and AC-3: a new application needs its type, jurisdiction, asset and scope, in this field.</summary>
    public static (string Code, string Message)? ValidateCreate(OgPermitInputs inputs, OgHseScope scope)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(inputs.IntentId) || inputs.IntentId.Trim().Length < 4)
        {
            return (CodeInputRequired,
                "An application needs an intent key of at least four characters, so a retry can find the application it " +
                "already wrote.");
        }

        if (string.IsNullOrWhiteSpace(inputs.Type) ||
            !Enum.TryParse<PermitApplicationType>(inputs.Type.Trim(), ignoreCase: true, out _))
        {
            return (CodeTypeInvalid,
                "'" + (inputs.Type ?? "(none)") + "' is not a permit application type; use " +
                string.Join(", ", Enum.GetNames<PermitApplicationType>()) + ".");
        }

        // AC-2: the jurisdiction is explicit and never defaulted.
        if (string.IsNullOrWhiteSpace(inputs.Country) ||
            !Enum.TryParse<Country>(inputs.Country.Trim(), ignoreCase: true, out _))
        {
            return (CodeJurisdictionRequired,
                "An application needs its jurisdiction (country). None is assumed from the field, the company or a browser " +
                "default.");
        }

        // AC-3: an asset must be named, and it must be this field's.
        if (string.IsNullOrWhiteSpace(inputs.AssetId))
        {
            return (CodeAssetRequired, "An application needs the asset it authorises.");
        }

        if (string.IsNullOrWhiteSpace(scope.FieldId))
        {
            return (CodeForeignAsset,
                "An application is filed for one field; none is selected, so no asset can be shown to belong to it.");
        }

        if (!string.IsNullOrWhiteSpace(inputs.FieldId) &&
            !string.Equals(inputs.FieldId.Trim(), scope.FieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (CodeForeignAsset,
                "Asset '" + inputs.AssetId.Trim() + "' is recorded against field '" + inputs.FieldId.Trim() +
                "', not the current field '" + scope.FieldId.Trim() + "'. A foreign asset is refused.");
        }

        return string.IsNullOrWhiteSpace(inputs.Scope)
            ? (CodeScopeRequired, "An application needs the scope it applies for.")
            : null;
    }

    /// <summary>AC-1: a transition must be one the owner's rules allow, and it must say why.</summary>
    public static (string Code, string Message)? CheckTransition(string? current, string? target)
    {
        var normalized = PermitStatusTransitionRules.Normalize(target);
        var legal = NextStates(current);

        return legal.Contains(normalized, StringComparer.OrdinalIgnoreCase)
            ? null
            : (CodeTransitionIllegal,
                "A permit application in " + PermitStatusTransitionRules.Normalize(current) + " may move to " +
                (legal.Count == 0 ? "nothing - it is closed" : string.Join(", ", legal)) + ", not " +
                PermitStatusTransitionRules.Normalize(target) + ".");
    }

    /// <summary>AC-4: a decision records its rationale, and an approval records the conditions it authorises under.</summary>
    public static (string Code, string Message)? ValidateDecision(string? target, string? rationale, string? conditions)
    {
        if (string.IsNullOrWhiteSpace(rationale))
        {
            return (CodeRationaleRequired,
                "A decision needs its rationale, so the record shows why the state changed.");
        }

        return string.Equals(PermitStatusTransitionRules.Normalize(target), PermitApplicationStatusCodes.Approved,
                   StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(conditions)
            ? (CodeConditionsRequired,
                "An approval needs the conditions the permit is granted under; an approval without them authorises nothing " +
                "in particular.")
            : null;
    }

    /// <summary>AC-4's SoD: the applicant cannot decide their own application.</summary>
    public static (string Code, string Message)? CheckSeparateDuties(
        string? applicant,
        string? actor,
        string? target)
    {
        if (string.IsNullOrWhiteSpace(applicant) || string.IsNullOrWhiteSpace(actor))
        {
            return null;
        }

        var isDecision = DecisionStates.Contains(PermitStatusTransitionRules.Normalize(target),
            StringComparer.OrdinalIgnoreCase);

        return isDecision && string.Equals(applicant.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase)
            ? (CodeSelfDecision, applicant + " applied for this permit and therefore cannot decide it.")
            : null;
    }

    /// <summary>The application id for an intent key, so a retry lands on the record it already wrote.</summary>
    public static string IdFor(string intent) => "PERMIT-" + intent.Trim();
}

/// <summary>The inputs of a new permit application (HSE-07).</summary>
public sealed record OgPermitInputs(
    string? IntentId,
    string? Type,
    string? Country,
    string? StateProvince,
    string? AssetKind,
    string? AssetId,
    string? FieldId,
    string? Scope);

/// <summary>HSE-07: the permit inbox, the application command and the decision.</summary>
public interface IOgPermits
{
    /// <summary>The applications and their recorded histories.</summary>
    Task<OgResult<OgPermitWorkspace>> LoadAsync(
        OgHseScope scope,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a draft application; a retry with the same intent key adopts the one it already wrote.</summary>
    Task<OgResult<OgPermitWorkspace>> CreateAsync(
        OgPermitInputs inputs,
        OgHseScope scope,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves an application to a legal next state, recording the actor, date, rationale and conditions in the append-only
    /// history (AC-1, AC-4, AC-6).
    /// </summary>
    Task<OgResult<OgPermitWorkspace>> DecideAsync(
        string applicationId,
        string target,
        string rationale,
        string? conditions,
        OgHseScope scope,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The PDF the application would render. Refused in this build: the renderer needs a native binary that is not carried
    /// (HSE-07's own "renderer unavailable" state), while the content it would render is on the record.
    /// </summary>
    OgResult<string> RenderPdf(string applicationId);
}

/// <summary>
/// HSE-07 over the owner's <c>PERMIT_APPLICATION</c> and <c>PERMIT_STATUS_HISTORY</c>, with the owner's own
/// <see cref="PermitStatusTransitionRules"/> deciding which transitions are legal.
/// </summary>
/// <remarks>
/// <para>
/// The history table is the authoritative record: the state is its latest entry, every change appends a row and never edits
/// one, and the header's own status is updated afterwards. That ordering means a decision is never lost from the record even
/// if the header cannot be written, and it is why the register reads the state from history rather than from the enum column.
/// </para>
/// <para>
/// The header is still read for the application's type, jurisdiction and asset. It carries enum columns, and
/// <c>ADDITIONAL_INFO_REQUIRED</c> is a storage key whose enum member is <c>AdditionalInformationRequired</c>, so a row written
/// with the key cannot be materialised; the register is therefore built from history and the header read is degraded with a
/// stated reason rather than failing the whole inbox.
/// </para>
/// </remarks>
public sealed class OgPermits : IOgPermits
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgPermits(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgPermitWorkspace>> LoadAsync(
        OgHseScope scope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPermitWorkspace>.Failed(
                "No connection is open, so the permit inbox cannot be read.", PermitRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, scope, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the permit inbox", "the permit inbox is not shown", FailureSeverity.Error);

            return OgResult<OgPermitWorkspace>.Failed(
                "The permit inbox could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? PermitRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPermitWorkspace>> CreateAsync(
        OgPermitInputs inputs,
        OgHseScope scope,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("An application must record who filed it.", PermitRules.CodeActorRequired);
        }

        if (PermitRules.ValidateCreate(inputs, scope) is { } refusal)
        {
            return Failure(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the application cannot be filed.", PermitRules.CodeNoConnection);
        }

        try
        {
            var id = PermitRules.IdFor(inputs.IntentId!);
            var now = DateTime.UtcNow;

            var headers = await OgUow.ReadAllAsync<PERMIT_APPLICATION>(source).ConfigureAwait(false);
            var existing = headers.FirstOrDefault(row =>
                string.Equals(row.PERMIT_APPLICATION_ID, id, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                // Retained original: the same intent key lands on the application it already wrote.
                if (!string.Equals(existing.FIELD_ID?.Trim(), scope.FieldId!.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existing.SUBMISSION_DESCRIPTION, inputs.Scope!.Trim(), StringComparison.Ordinal))
                {
                    return Failure(
                        "Intent '" + inputs.IntentId + "' was already used to file " + id + " with different content. " +
                        "Use a new intent key for a different application.",
                        PermitRules.CodeIntentConflict);
                }
            }
            else
            {
                var unit = OgUow.For<PERMIT_APPLICATION>(source);
                unit.PrimaryKey = nameof(PERMIT_APPLICATION.PERMIT_APPLICATION_ID);

                var header = new PERMIT_APPLICATION
                {
                    PERMIT_APPLICATION_ID = id,
                    OWNER_TENANT_ID = scope.CompanyId,
                    OWNER_COMPANY_ID = scope.CompanyId,
                    APPLICANT_ID = actor.Trim(),
                    APPLICATION_TYPE = Enum.Parse<PermitApplicationType>(inputs.Type!.Trim(), true),
                    STATUS = PermitApplicationStatus.Draft,
                    COUNTRY = Enum.Parse<Country>(inputs.Country!.Trim(), true),
                    STATE_PROVINCE = string.IsNullOrWhiteSpace(inputs.StateProvince)
                        ? default
                        : Enum.Parse<StateProvince>(inputs.StateProvince.Trim(), true),
                    CREATED_DATE = now,
                    FIELD_ID = scope.FieldId!.Trim(),
                    SUBMISSION_DESCRIPTION = inputs.Scope!.Trim(),
                    SUBMISSION_COMPLETE = false,
                    FEES_PAID = false,
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                    ACTIVE_IND = "Y",
                };

                switch (inputs.AssetKind?.Trim().ToUpperInvariant())
                {
                    case "WELL":
                        header.WELL_ID = inputs.AssetId!.Trim();
                        break;
                    case "FACILITY":
                        header.FACILITY_ID = inputs.AssetId!.Trim();
                        break;
                    default:
                        header.RELATED_WELL_UWI = inputs.AssetId!.Trim();
                        break;
                }

                unit.Add(header);

                var committed = await unit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(committed))
                {
                    return Failure(OgUow.Message(committed, "The application was not saved."),
                        PermitRules.CodeWriteFailed);
                }

                var opened = await AppendAsync(source, id, PermitApplicationStatusCodes.Draft,
                    "Application filed.", actor).ConfigureAwait(false);

                if (opened is not null)
                {
                    return Failure(opened, PermitRules.CodeWriteFailed);
                }
            }

            return await AfterAsync(source, scope, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "filing a permit application", "the application was not filed", FailureSeverity.Error);

            return Failure(
                "The application could not be filed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? PermitRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPermitWorkspace>> DecideAsync(
        string applicationId,
        string target,
        string rationale,
        string? conditions,
        OgHseScope scope,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A decision must record who made it.", PermitRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the application cannot change state.", PermitRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, scope, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var workspace = read.Value;

            var application = workspace.Applications.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, applicationId, StringComparison.OrdinalIgnoreCase));

            if (application is null)
            {
                return Failure("Application '" + applicationId + "' is not recorded.",
                    PermitRules.CodeUnknownApplication);
            }

            if (PermitRules.CheckTransition(application.State, target) is { } illegal)
            {
                return Failure(illegal.Message, illegal.Code);
            }

            if (PermitRules.ValidateDecision(target, rationale, conditions) is { } incomplete)
            {
                return Failure(incomplete.Message, incomplete.Code);
            }

            if (PermitRules.CheckSeparateDuties(application.Applicant, actor, target) is { } self)
            {
                return Failure(self.Message, self.Code);
            }

            var code = PermitStatusTransitionRules.Normalize(target);
            var remarks = string.IsNullOrWhiteSpace(conditions)
                ? rationale.Trim()
                : rationale.Trim() + " · conditions: " + conditions.Trim();

            // AC-6: the history is appended first, because it is the record the register is built from.
            var appended = await AppendAsync(source, applicationId, code, remarks, actor).ConfigureAwait(false);

            if (appended is not null)
            {
                return Failure(appended, PermitRules.CodeWriteFailed);
            }

            // The header's own status follows, so a reader of the header sees the state too. A header that cannot be
            // updated does not lose the decision: the history already carries it, and the register reads the history.
            await UpdateHeaderAsync(source, applicationId, code, rationale, conditions, actor, cancellationToken)
                .ConfigureAwait(false);

            return await AfterAsync(source, scope, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "deciding permit application " + applicationId, "the decision was not saved",
                FailureSeverity.Error);

            return Failure(
                "The decision could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? PermitRules.CodeWriteFailed);
        }
    }

    public OgResult<string> RenderPdf(string applicationId) =>
        // HSE-07's own state: the renderer is unavailable, so the export is disabled with the reason rather than producing
        // a file that is not the application.
        OgResult<string>.Failed(
            OgPermitWorkspace.RendererNotice + " (application " + applicationId + ")",
            PermitRules.CodeRendererUnavailable);

    // ------------------------------------------------------------------ the write helpers

    private static async Task<string?> AppendAsync(
        IDataSource source,
        string applicationId,
        string state,
        string? remarks,
        string actor)
    {
        var now = DateTime.UtcNow;

        var unit = OgUow.For<PERMIT_STATUS_HISTORY>(source);
        unit.PrimaryKey = nameof(PERMIT_STATUS_HISTORY.PERMIT_STATUS_HISTORY_ID);

        unit.Add(new PERMIT_STATUS_HISTORY
        {
            PERMIT_STATUS_HISTORY_ID = Guid.NewGuid().ToString("N"),
            PERMIT_APPLICATION_ID = applicationId,
            STATUS = PermitStatusTransitionRules.Normalize(state),
            STATUS_DATE = now,
            STATUS_REMARKS = remarks,
            UPDATED_BY = actor.Trim(),
        });

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The status history was not written.");
    }

    private async Task UpdateHeaderAsync(
        IDataSource source,
        string applicationId,
        string state,
        string rationale,
        string? conditions,
        string actor,
        CancellationToken cancellationToken)
    {
        try
        {
            var unit = OgUow.For<PERMIT_APPLICATION>(source);
            unit.PrimaryKey = nameof(PERMIT_APPLICATION.PERMIT_APPLICATION_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(PERMIT_APPLICATION.PERMIT_APPLICATION_ID), applicationId)],
                cancellationToken).ConfigureAwait(false);

            var header = rows.FirstOrDefault();

            if (header is null)
            {
                return;
            }

            var member = PermitRules.MemberFor(state);

            if (member is null)
            {
                return;
            }

            header.STATUS = member.Value;

            if (string.Equals(state, PermitApplicationStatusCodes.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                header.SUBMITTED_DATE ??= DateTime.UtcNow;
                header.SUBMITTED_BY_USER_ID = actor.Trim();
            }

            if (PermitRules.DecisionStates.Contains(state, StringComparer.OrdinalIgnoreCase))
            {
                header.DECISION_DATE = DateTime.UtcNow.Date;
                header.DECISION = string.IsNullOrWhiteSpace(conditions)
                    ? rationale.Trim()
                    : rationale.Trim() + " · conditions: " + conditions.Trim();
            }

            header.ROW_CHANGED_BY = actor.Trim();
            header.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(header);

            if (!OgUow.IsOk(updated))
            {
                _failures.ReportHandled(
                    new InvalidOperationException(OgUow.Message(updated, "The application header was not updated.")),
                    "updating the permit application header after " + state,
                    "the history carries the change; the header still shows its previous status",
                    FailureSeverity.Degraded);

                return;
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                _failures.ReportHandled(
                    new InvalidOperationException(OgUow.Message(committed, "The application header was not committed.")),
                    "committing the permit application header after " + state,
                    "the history carries the change; the header still shows its previous status",
                    FailureSeverity.Degraded);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // The decision is already in the append-only history; a header that cannot follow is degraded, not lost.
            _failures.ReportHandled(
                ex, "updating the permit application header after " + state,
                "the history carries the change; the header still shows its previous status",
                FailureSeverity.Degraded);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgPermitWorkspace>> AfterAsync(
        IDataSource source,
        OgHseScope scope,
        CancellationToken cancellationToken) =>
        await ReadAsync(source, scope, cancellationToken).ConfigureAwait(false);

    private async Task<OgResult<OgPermitWorkspace>> ReadAsync(
        IDataSource source,
        OgHseScope scope,
        CancellationToken cancellationToken)
    {
        var history = await OgUow.ReadAllAsync<PERMIT_STATUS_HISTORY>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        // The header carries enum columns, and the status among them has a storage key (ADDITIONAL_INFO_REQUIRED) whose
        // enum member is AdditionalInformationRequired. The register therefore never takes the state from that column - it
        // reads the append-only history - so a header whose status cannot be materialised cannot misreport an application's
        // state. If the header rows cannot be read at all the inbox is refused rather than listed without its field filter,
        // because an unfiltered list would show another field's permit as this field's.
        IReadOnlyList<PERMIT_APPLICATION> headers;

        try
        {
            headers = await OgUow.ReadAllAsync<PERMIT_APPLICATION>(source).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the permit application headers",
                "the permit inbox is refused rather than listing applications whose field cannot be verified",
                FailureSeverity.Error);

            return OgResult<OgPermitWorkspace>.Failed(
                "The permit applications could not be read, so the inbox is refused rather than shown as empty" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? PermitRules.CodeReadFailed);
        }

        var byApplication = history
            .GroupBy(row => row.PERMIT_APPLICATION_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var applications = new List<OgPermitApplication>();

        var ids = headers
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.IsNullOrWhiteSpace(scope.FieldId) ||
                          string.Equals(row.FIELD_ID?.Trim(), scope.FieldId.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(row => row.PERMIT_APPLICATION_ID)
            .Concat(byApplication.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var id in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var header = headers.FirstOrDefault(row =>
                string.Equals(row.PERMIT_APPLICATION_ID, id, StringComparison.OrdinalIgnoreCase));

            // A header from another field is not this field's application.
            if (header is not null && !string.IsNullOrWhiteSpace(scope.FieldId) &&
                !string.Equals(header.FIELD_ID?.Trim(), scope.FieldId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var entries = byApplication.TryGetValue(id, out var rows)
                ? rows.OrderBy(row => row.STATUS_DATE).ThenBy(row => row.PERMIT_STATUS_HISTORY_ID,
                        StringComparer.Ordinal)
                    .Select(row => new OgPermitHistoryEntry(
                        row.STATUS_DATE,
                        PermitStatusTransitionRules.Normalize(row.STATUS),
                        row.STATUS_REMARKS,
                        row.UPDATED_BY))
                    .ToList()
                : [];

            var state = entries.Count > 0
                ? entries[^1].State
                : header is null
                    ? string.Empty
                    : PermitStatusTransitionRules.Normalize(header.STATUS.ToString());

            applications.Add(new OgPermitApplication(
                id,
                header?.REFERENCE_NUMBER ?? id,
                header?.APPLICATION_TYPE.ToString() ?? "(header not shown)",
                header is null ? string.Empty : Country(header),
                AssetOf(header),
                header?.FIELD_ID,
                header?.SUBMISSION_DESCRIPTION ?? header?.REMARKS ?? string.Empty,
                state,
                header?.SUBMITTED_DATE,
                header?.DECISION_DATE,
                header?.DECISION,
                header?.APPLICANT_ID,
                entries));
        }

        var ordered = applications
            .Where(application => application.History.Count > 0 || !string.IsNullOrWhiteSpace(application.Type))
            .OrderByDescending(application => application.History.Count > 0 ? application.History[0].At : DateTime.MinValue)
            .ThenBy(application => application.Number, StringComparer.Ordinal)
            .ToList();

        return OgResult<OgPermitWorkspace>.Ok(new OgPermitWorkspace(
            ordered, ordered.FirstOrDefault(), PermitRules.States, string.Empty, DateTime.UtcNow));
    }

    private static string Country(PERMIT_APPLICATION header)
    {
        var country = header.COUNTRY.ToString();
        var state = header.STATE_PROVINCE.ToString();

        return string.Equals(state, "0", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(state)
            ? country
            : country + " / " + state;
    }

    private static string? AssetOf(PERMIT_APPLICATION? header) =>
        header is null
            ? null
            : !string.IsNullOrWhiteSpace(header.WELL_ID)
                ? header.WELL_ID
                : !string.IsNullOrWhiteSpace(header.FACILITY_ID)
                    ? header.FACILITY_ID
                    : header.RELATED_WELL_UWI;

    private static OgResult<OgPermitWorkspace> Failure(string message, string code) =>
        OgResult<OgPermitWorkspace>.Failed(message, code);
}
