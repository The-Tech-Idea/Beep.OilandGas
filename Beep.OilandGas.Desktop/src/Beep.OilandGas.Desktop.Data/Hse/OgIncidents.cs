using System.Globalization;
using System.Text.RegularExpressions;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;

/// <summary>The company and field a read or a report belongs to (HSE-02, AC-3).</summary>
/// <param name="CompanyId">The company the records belong to.</param>
/// <param name="FieldId">The field the records belong to, or null when the session has none.</param>
public sealed record OgHseScope(string CompanyId, string? FieldId)
{
    public static OgHseScope FromSession(OgSession session) =>
        new(
            string.IsNullOrWhiteSpace(session.CompanyId) ? "APP" : session.CompanyId,
            string.IsNullOrWhiteSpace(session.FieldId) ? null : session.FieldId);
}

/// <summary>One incident as the register shows it (HSE-02).</summary>
public sealed record OgIncident(
    string Id,
    string Number,
    DateTime OccurredUtc,
    string OccurredText,
    string Type,
    string Tier,
    string Severity,
    string? FieldId,
    string? FacilityId,
    string Jurisdiction,
    string State,
    string? Investigator,
    int OpenActions,
    string? Description,
    string? Location)
{
    public string JurisdictionText => string.IsNullOrWhiteSpace(Jurisdiction)
        ? "not specified"
        : Jurisdiction;

    /// <summary>Whether the incident is in one of the five active states.</summary>
    public bool IsOpen => HseRules.IsActive(State);

    public string Describe() =>
        Number + " · " + OccurredText + " · " + Type + " · " + Tier + " · " + State.ToLowerInvariant() +
        " · " + JurisdictionText +
        (string.IsNullOrWhiteSpace(FieldId) ? string.Empty : " · field " + FieldId) +
        (string.IsNullOrWhiteSpace(FacilityId) ? string.Empty : " · facility " + FacilityId) +
        (string.IsNullOrWhiteSpace(Investigator) ? string.Empty : " · investigator " + Investigator) +
        " · " + OpenActions.ToString(CultureInfo.InvariantCulture) + " open action(s)";
}

/// <summary>What a person records when they report an incident (HSE-02, AC-1, AC-2).</summary>
/// <param name="IntentId">
/// The client's own key for this report. The incident's id is derived from it, so a report retried after a lost
/// acknowledgement finds the incident it already wrote instead of creating a second one.
/// </param>
/// <param name="OccurredAtIso">An ISO 8601 timestamp <b>with an explicit offset</b>; a local time without one is refused.</param>
/// <param name="Jurisdiction">Required and never defaulted to a country.</param>
public sealed record OgIncidentReport(
    string? IntentId,
    string? Type,
    string? Tier,
    string? Jurisdiction,
    string? OccurredAtIso,
    string? FieldId,
    string? FacilityId,
    string Description,
    string? Location = null,
    string? ImmediateActions = null,
    string? PeopleInvolved = null);

/// <summary>The filters the register offers (HSE-02). Only the state list is restricted to recorded values (AC-6).</summary>
public sealed record OgIncidentFilter(
    string? State = null,
    string? Tier = null,
    string? Type = null,
    DateTime? From = null,
    DateTime? To = null,
    string? FieldId = null)
{
    public static OgIncidentFilter None { get; } = new();
}

/// <summary>The incident register for one company and field (HSE-02).</summary>
public sealed record OgIncidentWorkspace(
    OgHseScope Scope,
    IReadOnlyList<OgIncident> Incidents,
    OgIncident? Selected,
    IReadOnlyList<string> States,
    IReadOnlyList<string> Tiers,
    IReadOnlyList<string> Types,
    DateTime ReadAt)
{
    public string RowsText => Incidents.Count == 0
        ? "No incident is recorded for this filter."
        : string.Join(Environment.NewLine, Incidents.Select(incident => incident.Describe()));

    public string ScopeText => "company " + Scope.CompanyId +
                               (string.IsNullOrWhiteSpace(Scope.FieldId) ? " · no field selected" : " · field " + Scope.FieldId);

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public string FiltersText => "states " + string.Join(", ", States) +
                                 " · tiers " + string.Join(", ", Tiers);

    /// <summary>AC-5: this build has no process engine, and the page says so rather than offering a write.</summary>
    public const string TransitionNotice =
        "A state change goes through the workflow engine (WFL-01/WFL-02), which this build does not have; the register " +
        "will not change an incident's state directly.";
}

/// <summary>The rules HSE-02 states once, so the service, the page and the tests agree.</summary>
public static class IncidentRules
{
    public const string CodeNoConnection = "incident-no-connection";
    public const string CodeReadFailed = "incident-read-failed";
    public const string CodeWriteFailed = "incident-write-failed";
    public const string CodeScopeChanged = "incident-scope-changed";
    public const string CodePopulationNull = "incident-population-null";
    public const string CodeActorRequired = "incident-actor-required";
    public const string CodeInputRequired = "incident-input-required";
    public const string CodeInputInvalid = "incident-input-invalid";
    public const string CodeJurisdictionRequired = "incident-jurisdiction-required";
    public const string CodeTimestampNeedsOffset = "incident-timestamp-needs-offset";
    public const string CodeFieldChanged = "incident-field-changed";
    public const string CodeUnknownIncident = "incident-unknown";
    public const string CodeIntentConflict = "incident-intent-conflict";
    public const string CodeWorkflowNotWired = "incident-workflow-not-wired";
    public const string CodeFilterInvalid = "incident-filter-invalid";

    /// <summary>AC-6: the state filter offers exactly the owner's recorded states.</summary>
    public static IReadOnlyList<string> States { get; } =
    [
        IncidentState.Reported,
        IncidentState.UnderInvestigation,
        IncidentState.PendingRCA,
        IncidentState.PendingCorrectiveAction,
        IncidentState.PendingCloseOut,
        IncidentState.Closed,
        IncidentState.Cancelled,
    ];

    /// <summary>The owner's four severity tiers.</summary>
    public static IReadOnlyList<string> Tiers => HseReferenceCodes.Severity.All;

    /// <summary>An ISO 8601 timestamp that carries an explicit offset - 'Z' or ±hh:mm (AC-1).</summary>
    private static readonly Regex IsoWithOffset = new(
        @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2}(\.\d+)?)?(Z|[+-]\d{2}:?\d{2})$",
        RegexOptions.CultureInvariant);

    /// <summary>The intent key must be usable inside an incident id.</summary>
    private static readonly Regex IntentKey = new(@"^[A-Za-z0-9_-]{4,40}$", RegexOptions.CultureInvariant);

    /// <summary>
    /// AC-1 and AC-2: a report needs a type, a tier, an explicit jurisdiction and an ISO timestamp with an offset.
    /// </summary>
    /// <remarks>
    /// The jurisdiction is checked before anything else is written and there is <b>no default</b>: an empty one is refused
    /// with a sentence asking for it, so no country is ever assumed (the Web removed exactly that defect).
    /// </remarks>
    public static (string Code, string Message)? ValidateReport(OgIncidentReport report, OgHseScope scope)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(scope);

        if (report.IntentId is not { Length: > 0 } intent || !IntentKey.IsMatch(intent))
        {
            return (CodeInputRequired,
                "A report needs an intent key of 4-40 letters, digits, hyphens or underscores, so a retry can find the " +
                "incident it already wrote.");
        }

        if (string.IsNullOrWhiteSpace(report.Type))
        {
            return (CodeInputRequired, "A report needs the incident type.");
        }

        if (string.IsNullOrWhiteSpace(report.Tier))
        {
            return (CodeInputRequired, "A report needs the incident tier.");
        }

        if (!Tiers.Contains(report.Tier.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeInputInvalid,
                $"'{report.Tier}' is not a recorded tier; use {string.Join(", ", Tiers)}.");
        }

        // AC-2: no jurisdiction defaults to any country.
        if (string.IsNullOrWhiteSpace(report.Jurisdiction))
        {
            return (CodeJurisdictionRequired,
                "A report needs its jurisdiction. None is assumed from the field, the company or the browser.");
        }

        if (!TryParseOccurred(report.OccurredAtIso, out _))
        {
            return (CodeTimestampNeedsOffset,
                "The incident's date and time must be an ISO 8601 timestamp with an explicit offset, for example " +
                "2026-11-08T14:30:00Z or 2026-11-08T17:30:00+03:00. A local time without an offset is refused.");
        }

        if (string.IsNullOrWhiteSpace(report.FieldId))
        {
            return (CodeInputRequired, "A report needs the field it happened in.");
        }

        // The report dialog shows the field the reviewer saw; if the session's field has moved on, the reviewed report
        // no longer describes the field it would be filed against.
        if (!string.Equals(report.FieldId.Trim(), scope.FieldId, StringComparison.OrdinalIgnoreCase))
        {
            return (CodeFieldChanged,
                $"This report was reviewed against field '{report.FieldId}', but the current field is " +
                $"'{scope.FieldId ?? "(none)"}'. Review the report again before filing it.");
        }

        if (string.IsNullOrWhiteSpace(report.Description))
        {
            return (CodeInputRequired, "A report needs its description.");
        }

        return null;
    }

    /// <summary>AC-1: parses an ISO 8601 timestamp only when it carries an offset.</summary>
    public static bool TryParseOccurred(string? iso, out DateTimeOffset occurred)
    {
        occurred = default;

        if (string.IsNullOrWhiteSpace(iso) || !IsoWithOffset.IsMatch(iso.Trim()))
        {
            return false;
        }

        return DateTimeOffset.TryParse(
            iso.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out occurred);
    }

    /// <summary>AC-4: a null population is a refusal, not an empty list.</summary>
    public static (string Code, string Message)? CheckPopulation<T>(IReadOnlyList<T>? rows, string what)
    {
        return rows is null
            ? (CodePopulationNull,
                "The " + what + " could not be read (the result was null), so the register is refused rather than shown " +
                "as empty.")
            : null;
    }

    /// <summary>Refuses a filter value the register does not offer - only recorded states are accepted (AC-6).</summary>
    public static (string Code, string Message)? ValidateFilter(OgIncidentFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.State is { Length: > 0 } state &&
            !States.Contains(state.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeFilterInvalid,
                $"'{state}' is not a recorded incident state; the filter offers {string.Join(", ", States)}.");
        }

        if (filter.Tier is { Length: > 0 } tier &&
            !Tiers.Contains(tier.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeFilterInvalid, $"'{tier}' is not a recorded tier; the filter offers {string.Join(", ", Tiers)}.");
        }

        if (filter.From is { } from && filter.To is { } to && to.Date < from.Date)
        {
            return (CodeFilterInvalid, "The date range ends before it starts.");
        }

        return null;
    }

    /// <summary>The incident id for an intent key, so a retry lands on the same row.</summary>
    public static string IdFor(string intent) => "INC-" + intent.Trim();

    /// <summary>A severity id's name for a person, from the owner's tier comments.</summary>
    public static string DescribeSeverity(string? severity) => severity?.Trim().ToUpperInvariant() switch
    {
        HseReferenceCodes.Severity.Tier1 => "Major / fatal",
        HseReferenceCodes.Severity.Tier2 => "Serious / recordable",
        HseReferenceCodes.Severity.Tier3 => "Minor / first aid",
        HseReferenceCodes.Severity.Tier4 => "Near miss / observation",
        null or "" => "not recorded",
        _ => severity!,
    };
}

/// <summary>HSE-02: the incident register and the report command.</summary>
public interface IOgIncidents
{
    /// <summary>Starts a load; a later load invalidates this one, so a late response is discarded (AC-3).</summary>
    long BeginLoad();

    /// <summary>False when a newer load has started, so this response must not be rendered (AC-3).</summary>
    bool IsCurrent(long version);

    /// <param name="currentScope">
    /// The session's company and field <b>now</b>. It is compared with the scope the load started under before the rows are
    /// returned, and a difference discards them with a stated reason (AC-3).
    /// </param>
    Task<OgResult<OgIncidentWorkspace>> LoadAsync(
        long version,
        OgHseScope scope,
        Func<OgHseScope> currentScope,
        OgIncidentFilter filter,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports an incident; a retry with the same intent key adopts the incident it already wrote.</summary>
    Task<OgResult<OgIncidentWorkspace>> ReportAsync(
        OgIncidentReport report,
        OgHseScope scope,
        string actor,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// A state change. This build has no workflow engine, so it is refused rather than written directly (AC-5).
    /// </summary>
    Task<OgResult<OgIncidentWorkspace>> TransitionAsync(
        string incidentId,
        string trigger,
        string? reason,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HSE-02 over the owner's <c>HSE_INCIDENT</c>, <c>HSE_INCIDENT_DETAIL</c>, <c>HSE_INCIDENT_COMPONENT</c> and
/// <c>HSE_INCIDENT_RESPONSE</c>, plus the <c>PROJECT</c> family for the incident's open actions.
/// </summary>
/// <remarks>
/// The owner's schema puts the type and severity on a detail row, the field, facility, jurisdiction and location on a
/// component row, and the state on a history of status responses - so a report writes four rows and the register reads them
/// back. The incident id is derived from the caller's intent key, which exists before anything is written, so a report retried
/// after a lost acknowledgement finds its own rows instead of creating a second incident.
/// </remarks>
public sealed class OgIncidents : IOgIncidents
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;
    private long _version;

    public OgIncidents(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public long BeginLoad() => Interlocked.Increment(ref _version);

    public bool IsCurrent(long version) => version == Volatile.Read(ref _version);

    public async Task<OgResult<OgIncidentWorkspace>> LoadAsync(
        long version,
        OgHseScope scope,
        Func<OgHseScope> currentScope,
        OgIncidentFilter filter,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(currentScope);
        ArgumentNullException.ThrowIfNull(filter);

        if (IncidentRules.ValidateFilter(filter) is { } filterRefusal)
        {
            return OgResult<OgIncidentWorkspace>.Failed(filterRefusal.Message, filterRefusal.Code);
        }

        var start = currentScope();

        if (start != scope)
        {
            return ScopeChanged(scope, start);
        }

        if (!IsCurrent(version))
        {
            return ScopeChanged(scope, start);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgIncidentWorkspace>.Failed(
                "No connection is open, so the incident register cannot be read.", IncidentRules.CodeNoConnection);
        }

        try
        {
            var incidents = await OgUow.ReadAllAsync<HSE_INCIDENT>(source).ConfigureAwait(false);
            var details = await OgUow.ReadAllAsync<HSE_INCIDENT_DETAIL>(source).ConfigureAwait(false);
            var components = await OgUow.ReadAllAsync<HSE_INCIDENT_COMPONENT>(source).ConfigureAwait(false);
            var responses = await OgUow.ReadAllAsync<HSE_INCIDENT_RESPONSE>(source).ConfigureAwait(false);
            var projects = await OgUow.ReadAllAsync<PROJECT>(source).ConfigureAwait(false);
            var steps = await OgUow.ReadAllAsync<PROJECT_STEP>(source).ConfigureAwait(false);
            var statuses = await OgUow.ReadAllAsync<PROJECT_STATUS>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            // AC-4: a null population is refused rather than shown as an empty register.
            foreach (var (rows, what) in new (object?, string)[]
                     {
                         (incidents, "incident register"),
                         (details, "incident details"),
                         (components, "incident components"),
                         (responses, "incident status history"),
                     })
            {
                if (rows is null)
                {
                    return OgResult<OgIncidentWorkspace>.Failed(
                        "The " + what + " came back null, so the register is refused rather than shown as empty.",
                        IncidentRules.CodePopulationNull);
                }
            }

            // AC-3: the company or field moved while the rows were being read - discard them.
            var end = currentScope();

            if (end != start || !IsCurrent(version))
            {
                return ScopeChanged(scope, end);
            }

            var register = Build(incidents!, details!, components!, responses!, projects!, steps!, statuses!,
                filter, timeZoneId, cancellationToken);

            return OgResult<OgIncidentWorkspace>.Ok(new OgIncidentWorkspace(
                scope,
                register,
                register.FirstOrDefault(),
                IncidentRules.States,
                IncidentRules.Tiers,
                register.Select(incident => incident.Type)
                    .Where(type => !string.IsNullOrWhiteSpace(type))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(type => type, StringComparer.Ordinal)
                    .ToList(),
                DateTime.UtcNow));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the incident register", "the incident register is not shown; no row is inferred",
                FailureSeverity.Error);

            return OgResult<OgIncidentWorkspace>.Failed(
                "The incident register could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? IncidentRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgIncidentWorkspace>> ReportAsync(
        OgIncidentReport report,
        OgHseScope scope,
        string actor,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgIncidentWorkspace>.Failed(
                "An incident report must record who filed it.", IncidentRules.CodeActorRequired);
        }

        if (IncidentRules.ValidateReport(report, scope) is { } refusal)
        {
            return OgResult<OgIncidentWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgIncidentWorkspace>.Failed(
                "No connection is open, so the incident cannot be reported.", IncidentRules.CodeNoConnection);
        }

        try
        {
            IncidentRules.TryParseOccurred(report.OccurredAtIso, out var occurred);

            var incidentId = IncidentRules.IdFor(report.IntentId!);
            var now = DateTime.UtcNow;

            var header = await OgUow.ReadAllAsync<HSE_INCIDENT>(source).ConfigureAwait(false);
            var existing = header.FirstOrDefault(row =>
                string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                // Retained-original: the same intent key lands on the incident it already wrote. A different report under
                // the same key is a conflict, not an overwrite.
                if (!string.Equals(existing.REMARK, report.Description, StringComparison.Ordinal) ||
                    (existing.INCIDENT_DATE ?? DateTime.MinValue) != occurred.UtcDateTime)
                {
                    return OgResult<OgIncidentWorkspace>.Failed(
                        $"Intent '{report.IntentId}' was already used to report incident {incidentId} with different " +
                        "content. Use a new intent key for a different incident.",
                        IncidentRules.CodeIntentConflict);
                }
            }
            else
            {
                var unit = OgUow.For<HSE_INCIDENT>(source);
                unit.PrimaryKey = "INCIDENT_ID";

                unit.Add(new HSE_INCIDENT
                {
                    INCIDENT_ID = incidentId,
                    INCIDENT_DATE = occurred.UtcDateTime,
                    REMARK = report.Description.Trim(),
                    ACTIVE_IND = "Y",
                    SOURCE = "BeepOilGasDesktop",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var committed = await unit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(committed))
                {
                    return OgResult<OgIncidentWorkspace>.Failed(
                        OgUow.Message(committed, "The incident was not saved."), IncidentRules.CodeWriteFailed);
                }
            }

            // The detail, component and status rows are ensured in turn, so a retry after a partial write completes the
            // report instead of duplicating it.
            var detailRows = await OgUow.ReadAllAsync<HSE_INCIDENT_DETAIL>(source).ConfigureAwait(false);

            var detail = OgUow.For<HSE_INCIDENT_DETAIL>(source);
            detail.PrimaryKey = "INCIDENT_ID";

            void EnsureDetail(decimal observation, string detailType, string? remark)
            {
                if (string.IsNullOrWhiteSpace(remark))
                {
                    return;
                }

                var present = detailRows.Any(row =>
                    string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase) &&
                    row.DETAIL_OBS_NO == observation);

                if (present)
                {
                    return;
                }

                detail.Add(new HSE_INCIDENT_DETAIL
                {
                    INCIDENT_ID = incidentId,
                    DETAIL_OBS_NO = observation,
                    DETAIL_TYPE = detailType,
                    INCIDENT_TYPE_ID = observation == 1m ? report.Type!.Trim() : null,
                    INCIDENT_SEVERITY_ID = observation == 1m ? report.Tier!.Trim().ToUpperInvariant() : null,
                    REMARK = remark.Trim(),
                    INCIDENT_DATE = occurred.UtcDateTime,
                    ACTIVE_IND = "Y",
                    SOURCE = "BeepOilGasDesktop",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            EnsureDetail(1m, "REPORT", report.Description);
            EnsureDetail(2m, "ACTION", report.ImmediateActions);
            EnsureDetail(3m, "PEOPLE", report.PeopleInvolved);

            var detailsCommitted = await detail.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(detailsCommitted))
            {
                return OgResult<OgIncidentWorkspace>.Failed(
                    OgUow.Message(detailsCommitted, "The incident's detail was not saved."),
                    IncidentRules.CodeWriteFailed);
            }

            var componentRows = await OgUow.ReadAllAsync<HSE_INCIDENT_COMPONENT>(source).ConfigureAwait(false);

            var componentPresent = componentRows.Any(row =>
                string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase) &&
                row.COMPONENT_OBS_NO == 1m);

            if (!componentPresent)
            {
                var component = OgUow.For<HSE_INCIDENT_COMPONENT>(source);
                component.PrimaryKey = "INCIDENT_ID";

                component.Add(new HSE_INCIDENT_COMPONENT
                {
                    INCIDENT_ID = incidentId,
                    COMPONENT_OBS_NO = 1m,
                    FIELD_ID = report.FieldId!.Trim(),
                    FACILITY_ID = string.IsNullOrWhiteSpace(report.FacilityId) ? null : report.FacilityId.Trim(),
                    COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole,
                    COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType,
                    JURISDICTION = report.Jurisdiction!.Trim(),
                    REMARK = string.IsNullOrWhiteSpace(report.Location) ? null : report.Location.Trim(),
                    ACTIVE_IND = "Y",
                    SOURCE = "BeepOilGasDesktop",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var componentCommitted = await component.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(componentCommitted))
                {
                    return OgResult<OgIncidentWorkspace>.Failed(
                        OgUow.Message(componentCommitted, "The incident's field, jurisdiction and facility were not saved."),
                        IncidentRules.CodeWriteFailed);
                }
            }

            var responseRows = await OgUow.ReadAllAsync<HSE_INCIDENT_RESPONSE>(source).ConfigureAwait(false);

            var responsePresent = responseRows.Any(row =>
                string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase) &&
                row.RESPONSE_OBS_NO == 1m);

            if (!responsePresent)
            {
                var response = OgUow.For<HSE_INCIDENT_RESPONSE>(source);
                response.PrimaryKey = "INCIDENT_ID";

                response.Add(new HSE_INCIDENT_RESPONSE
                {
                    INCIDENT_ID = incidentId,
                    RESPONSE_OBS_NO = 1m,
                    RESPONSE_TYPE = "STATUS",
                    RESPONSE_RESULT = IncidentState.Reported,
                    EFFECTIVE_DATE = occurred.UtcDateTime,
                    ACTIVE_IND = "Y",
                    SOURCE = "BeepOilGasDesktop",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var responseCommitted = await response.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(responseCommitted))
                {
                    return OgResult<OgIncidentWorkspace>.Failed(
                        OgUow.Message(responseCommitted, "The incident's first status was not saved."),
                        IncidentRules.CodeWriteFailed);
                }
            }

            // A re-read rather than a hand-built workspace, so the page shows exactly what was stored.
            var after = await LoadAsync(
                BeginLoad(), scope, () => scope, OgIncidentFilter.None, timeZoneId, cancellationToken)
                .ConfigureAwait(false);

            // The write succeeded; if the re-read did not, say so rather than showing an empty register as if the report
            // had vanished. The header is idempotent, so a retry re-reads it without writing a second incident.
            return after.Success
                ? after
                : OgResult<OgIncidentWorkspace>.Failed(
                    "The incident was reported, but the register could not be re-read. Load again to see it.",
                    IncidentRules.CodeReadFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"reporting an incident for field {report.FieldId}", "the incident was not reported",
                FailureSeverity.Error);

            return OgResult<OgIncidentWorkspace>.Failed(
                "The incident could not be reported" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? IncidentRules.CodeWriteFailed);
        }
    }

    public Task<OgResult<OgIncidentWorkspace>> TransitionAsync(
        string incidentId,
        string trigger,
        string? reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        // AC-5: a state change is a workflow event. Without WFL-01/WFL-02 there is no event to write, so the command is
        // refused with a stated reason instead of performing a direct update behind the workflow's back.
        _ = incidentId;
        _ = trigger;
        _ = reason;
        _ = actor;
        _ = cancellationToken;

        return Task.FromResult(OgResult<OgIncidentWorkspace>.Failed(
            "A state change goes through the workflow engine, which this build does not have (WFL-01/WFL-02). " +
            "The incident keeps its recorded state.",
            IncidentRules.CodeWorkflowNotWired));
    }

    // ------------------------------------------------------------------ the read model

    private static IReadOnlyList<OgIncident> Build(
        IReadOnlyList<HSE_INCIDENT> incidents,
        IReadOnlyList<HSE_INCIDENT_DETAIL> details,
        IReadOnlyList<HSE_INCIDENT_COMPONENT> components,
        IReadOnlyList<HSE_INCIDENT_RESPONSE> responses,
        IReadOnlyList<PROJECT> projects,
        IReadOnlyList<PROJECT_STEP> steps,
        IReadOnlyList<PROJECT_STATUS> statuses,
        OgIncidentFilter filter,
        string? timeZoneId,
        CancellationToken cancellationToken)
    {
        var latestState = responses
            .Where(response => !string.Equals(response.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(response => string.Equals(response.RESPONSE_TYPE?.Trim(), "STATUS", StringComparison.OrdinalIgnoreCase))
            .GroupBy(response => response.INCIDENT_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(response => response.RESPONSE_OBS_NO)
                    .ThenByDescending(response => response.EFFECTIVE_DATE)
                    .First().RESPONSE_RESULT,
                StringComparer.OrdinalIgnoreCase);

        // The type and severity live on the report detail row, not on every detail row - so the latest row that actually
        // carries a type wins, and a later note (an action, a person) can never blank the incident's type.
        var latestDetail = details
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.INCIDENT_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Where(row => !string.IsNullOrWhiteSpace(row.INCIDENT_TYPE_ID))
                             .OrderByDescending(row => row.DETAIL_OBS_NO)
                             .FirstOrDefault()
                         ?? group.OrderByDescending(row => row.DETAIL_OBS_NO).First(),
                StringComparer.OrdinalIgnoreCase);

        var componentsByIncident = components
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.INCIDENT_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var correctiveProjects = projects
            .Where(project => string.Equals(project.PROJECT_TYPE?.Trim(),
                HseReferenceCodes.CorrectiveAction.ProjectType, StringComparison.OrdinalIgnoreCase))
            .Select(project => project.PROJECT_ID)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var latestStepStatus = statuses
            .Where(status => !string.Equals(status.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(status => string.Equals(status.STATUS_TYPE?.Trim(),
                HseReferenceCodes.CorrectiveAction.StatusType, StringComparison.OrdinalIgnoreCase))
            .GroupBy(status => Key(status.PROJECT_ID, status.STEP_ID), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(status => status.EFFECTIVE_DATE)
                    .ThenByDescending(status => status.ROW_CREATED_DATE)
                    .First().STATUS,
                StringComparer.OrdinalIgnoreCase);

        var result = new List<OgIncident>();

        foreach (var incident in incidents
                     .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                     .Where(row => !string.IsNullOrWhiteSpace(row.INCIDENT_ID)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            latestState.TryGetValue(incident.INCIDENT_ID, out var state);
            latestDetail.TryGetValue(incident.INCIDENT_ID, out var detail);

            var mine = componentsByIncident.TryGetValue(incident.INCIDENT_ID, out var rows) ? rows : [];

            var primary = mine.FirstOrDefault(row => string.Equals(row.COMPONENT_ROLE?.Trim(),
                HseReferenceCodes.FieldContext.PrimaryRole, StringComparison.OrdinalIgnoreCase));

            var investigator = mine.FirstOrDefault(row => string.Equals(row.COMPONENT_ROLE?.Trim(),
                HseReferenceCodes.Investigator.Role, StringComparison.OrdinalIgnoreCase));

            var actionProjects = mine
                .Select(row => row.PROJECT_ID)
                .Where(id => !string.IsNullOrWhiteSpace(id) && correctiveProjects.Contains(id!))
                .Select(id => id!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var openActions = steps
                .Where(step => actionProjects.Contains(step.PROJECT_ID))
                .Where(step => !string.Equals(step.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Count(step => !string.Equals(
                    latestStepStatus.TryGetValue(Key(step.PROJECT_ID, step.STEP_ID), out var status) ? status : null,
                    HseReferenceCodes.CorrectiveAction.Completed,
                    StringComparison.OrdinalIgnoreCase));

            var occurred = incident.INCIDENT_DATE ?? DateTime.MinValue;

            var mapped = new OgIncident(
                incident.INCIDENT_ID,
                incident.INCIDENT_ID,
                occurred,
                DescribeOccurred(occurred, timeZoneId),
                detail?.INCIDENT_TYPE_ID ?? string.Empty,
                detail?.INCIDENT_SEVERITY_ID ?? string.Empty,
                IncidentRules.DescribeSeverity(detail?.INCIDENT_SEVERITY_ID),
                primary?.FIELD_ID,
                primary?.FACILITY_ID,
                primary?.JURISDICTION ?? string.Empty,
                state ?? string.Empty,
                string.IsNullOrWhiteSpace(investigator?.BUSINESS_ASSOCIATE_ID)
                    ? investigator?.REMARK
                    : investigator.BUSINESS_ASSOCIATE_ID,
                openActions,
                incident.REMARK,
                primary?.REMARK);

            if (!Matches(mapped, filter))
            {
                continue;
            }

            result.Add(mapped);
        }

        return result
            .OrderByDescending(incident => incident.OccurredUtc)
            .ThenBy(incident => incident.Number, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// The incident's instant in UTC <b>and</b> in the company's zone, which is what the register shows.
    /// </summary>
    /// <remarks>
    /// The stored value is UTC. The company zone comes from the session; when it is absent or unknown the text says so
    /// rather than silently showing a local time as if it were the company's.
    /// </remarks>
    public static string DescribeOccurred(DateTime occurredUtc, string? timeZoneId)
    {
        var utc = occurredUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " UTC";

        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return utc + " · no company zone selected";
        }

        // Ask the machine for the zone it knows rather than throwing and catching: an unknown zone is an ordinary answer
        // for a configured value, not a failure.
        var zone = TimeZoneInfo.GetSystemTimeZones()
            .FirstOrDefault(candidate => string.Equals(candidate.Id, timeZoneId, StringComparison.OrdinalIgnoreCase));

        if (zone is null)
        {
            return utc + " · company zone '" + timeZoneId + "' is not known on this machine";
        }

        var local = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(occurredUtc, DateTimeKind.Utc), zone);

        return utc + " · " + local.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " " + zone.Id;
    }

    private static bool Matches(OgIncident incident, OgIncidentFilter filter)
    {
        if (filter.State is { Length: > 0 } state &&
            !string.Equals(incident.State, state.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (filter.Tier is { Length: > 0 } tier &&
            !string.Equals(incident.Tier, tier.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (filter.Type is { Length: > 0 } type &&
            !string.Equals(incident.Type, type.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (filter.FieldId is { Length: > 0 } field &&
            !string.Equals(incident.FieldId, field.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (filter.From is { } from && incident.OccurredUtc.Date < from.Date)
        {
            return false;
        }

        if (filter.To is { } to && incident.OccurredUtc.Date > to.Date)
        {
            return false;
        }

        return true;
    }

    private static OgResult<OgIncidentWorkspace> ScopeChanged(OgHseScope scope, OgHseScope current) =>
        OgResult<OgIncidentWorkspace>.Failed(
            "The company or field changed while the register was loading, so the rows were discarded. " +
            $"The load began for company {scope.CompanyId}, field {scope.FieldId ?? "(none)"} and the session is now " +
            $"company {current.CompanyId}, field {current.FieldId ?? "(none)"}. Load again for the current field.",
            IncidentRules.CodeScopeChanged);

    private static string Key(string? projectId, string? stepId) =>
        (projectId ?? string.Empty) + "\u001f" + (stepId ?? string.Empty);
}
