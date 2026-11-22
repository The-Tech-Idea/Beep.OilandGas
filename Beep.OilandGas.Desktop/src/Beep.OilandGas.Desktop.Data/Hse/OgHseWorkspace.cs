using System.Globalization;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;

/// <summary>
/// The workspace's headline counts (HSE-01). Each is counted from records; a read that fails produces no snapshot at all
/// rather than a row of zeros (AC-3).
/// </summary>
public sealed record OgHseCounts(
    int OpenIncidents,
    int OverdueActions,
    int ObligationsDue,
    int OpenHazopActions,
    int PermitsPending)
{
    public string OpenIncidentsText => Count(OpenIncidents);

    public string OverdueActionsText => Count(OverdueActions);

    public string ObligationsDueText => Count(ObligationsDue);

    public string OpenHazopActionsText => Count(OpenHazopActions);

    public string PermitsPendingText => Count(PermitsPending);

    /// <summary>One line for the cards, so the page does not compose the sentence itself.</summary>
    public string Summary =>
        $"open incidents {OpenIncidentsText} · overdue actions {OverdueActionsText} · " +
        $"obligations due {ObligationsDueText} · open HAZOP actions {OpenHazopActionsText} · " +
        $"permits pending {PermitsPendingText}";

    private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>One row of the incident state strip (HSE-01, AC-1, AC-2).</summary>
/// <param name="State">The recorded state, or <see cref="HseRules.OtherState"/> for an unrecorded or unrecognised one.</param>
/// <param name="Label">The state's name for a person.</param>
/// <param name="Count">How many incidents are in that state.</param>
/// <param name="Active">True for the five states that count as open.</param>
public sealed record OgIncidentStateCount(string State, string Label, int Count, bool Active)
{
    public string Describe() => Label + " " + Count.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// One source link. <see cref="Available"/> is the shell's own page-permission answer, so a link offered is one that will
/// open (AC-5); a route this build does not carry says which feature adds it.
/// </summary>
public sealed record OgHseLink(string Route, string Label, bool Available, string? WhyNot);

/// <summary>The assigned work, composed from WFL-05's inbox rather than re-read here (HSE-01).</summary>
public sealed record OgHseInbox(bool Available, int? Overdue, int? Today, int? All, string Text)
{
    public static OgHseInbox Unavailable(string reason) => new(false, null, null, null, reason);
}

/// <summary>Everything the HSE workspace shows for one field (HSE-01).</summary>
public sealed record OgHseSnapshot(
    string? FieldId,
    string FieldName,
    string Jurisdiction,
    DateTime AsOf,
    OgHseCounts Counts,
    IReadOnlyList<OgIncidentStateCount> IncidentStates,
    OgHseInbox Inbox,
    IReadOnlyList<OgHseLink> Links,
    DateTime ReadAt)
{
    public IReadOnlyList<OgHseLink> AvailableLinks => Links.Where(link => link.Available).ToList();

    /// <summary>AC-2: the states that are not open are reported as their own rows, never folded into open.</summary>
    public IReadOnlyList<OgIncidentStateCount> NotOpen =>
        IncidentStates.Where(row => !row.Active).ToList();

    public string IncidentStrip => IncidentStates.Count == 0
        ? "no incident state is recorded"
        : string.Join(" · ", IncidentStates.Select(row => row.Describe()));

    /// <summary>AC-1: the open figure is the sum of the five active states and nothing else.</summary>
    public int OpenFromStrip => HseRules.OpenOf(IncidentStates);

    public string LinksText => Links.Count == 0
        ? "no source is configured"
        : string.Join(Environment.NewLine, Links.Select(link =>
            link.Available
                ? link.Label + " · available"
                : link.Label + " · not available - " + link.WhyNot));

    public string JurisdictionText => string.IsNullOrWhiteSpace(Jurisdiction)
        ? "jurisdiction not specified"
        : "jurisdiction " + Jurisdiction;

    /// <summary>One sentence for the page, so the view composes none of its own.</summary>
    public string NoticeText =>
        AvailableLinks.Count.ToString(CultureInfo.InvariantCulture) + " source(s) available · " +
        NotOpen.Sum(row => row.Count).ToString(CultureInfo.InvariantCulture) +
        " incident(s) closed, cancelled or unrecorded";

    /// <summary>The refresh time, always printable.</summary>
    public string RefreshedText =>
        "last refreshed " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>
/// The rules HSE-01 states once, so the service, the page and the tests cannot disagree.
/// </summary>
/// <remarks>
/// The five active states are the owner's own <see cref="IncidentState"/> values; the state names are not repeated here.
/// The corrective-action and HAZOP vocabularies are the owner's <c>HseReferenceCodes</c>; the permit states are the owner's
/// <see cref="PermitApplicationStatusCodes"/> storage keys.
/// </remarks>
public static class HseRules
{
    /// <summary>The state row for an incident whose latest state is missing or not one this build knows.</summary>
    public const string OtherState = "OTHER";

    /// <summary>The five states that count as open (AC-1).</summary>
    public static IReadOnlyList<string> ActiveStates { get; } =
    [
        IncidentState.Reported,
        IncidentState.UnderInvestigation,
        IncidentState.PendingRCA,
        IncidentState.PendingCorrectiveAction,
        IncidentState.PendingCloseOut,
    ];

    /// <summary>The states reported separately from open (AC-2).</summary>
    public static IReadOnlyList<string> ClosedStates { get; } = [IncidentState.Closed, IncidentState.Cancelled];

    /// <summary>The permit states that mean an application is still waiting on someone.</summary>
    public static IReadOnlyList<string> PendingPermitStates { get; } =
    [
        PermitApplicationStatusCodes.Submitted,
        PermitApplicationStatusCodes.UnderReview,
        PermitApplicationStatusCodes.AdditionalInformationRequired,
    ];

    /// <summary>The deviation states that mean a HAZOP action is still open.</summary>
    public static IReadOnlyList<string> OpenHazopStates { get; } =
        [HseReferenceCodes.Hazop.Open, HseReferenceCodes.Hazop.ActionRequired];

    /// <summary>Whether a state is one of the five that count as open.</summary>
    public static bool IsActive(string? state) =>
        ActiveStates.Contains(state?.Trim().ToUpperInvariant() ?? string.Empty, StringComparer.Ordinal);

    /// <summary>Classifies a set of recorded states into the strip's rows, in a stable order.</summary>
    public static IReadOnlyList<OgIncidentStateCount> Classify(IEnumerable<string?> states)
    {
        ArgumentNullException.ThrowIfNull(states);

        var recorded = states
            .Select(state => state?.Trim().ToUpperInvariant() ?? string.Empty)
            .ToList();

        var rows = new List<OgIncidentStateCount>();

        foreach (var state in ActiveStates)
        {
            rows.Add(new OgIncidentStateCount(state, Label(state), recorded.Count(row => row == state), true));
        }

        rows.Add(new OgIncidentStateCount(
            IncidentState.Closed, Label(IncidentState.Closed),
            recorded.Count(row => row == IncidentState.Closed), false));

        rows.Add(new OgIncidentStateCount(
            IncidentState.Cancelled, Label(IncidentState.Cancelled),
            recorded.Count(row => row == IncidentState.Cancelled), false));

        // Anything else - an unrecorded state, or one this build does not know - is its own row rather than being
        // quietly counted as open.
        var known = ActiveStates.Concat(ClosedStates).ToHashSet(StringComparer.Ordinal);
        var other = recorded.Count(row => !known.Contains(row));

        rows.Add(new OgIncidentStateCount(OtherState, "Other or unrecorded", other, false));

        return rows;
    }

    /// <summary>The sum of the five active rows.</summary>
    public static int OpenOf(IEnumerable<OgIncidentStateCount> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return rows.Where(row => row.Active).Sum(row => row.Count);
    }

    /// <summary>An incident state's name for a person.</summary>
    public static string Label(string? state) => state?.Trim().ToUpperInvariant() switch
    {
        IncidentState.Reported => "reported",
        IncidentState.UnderInvestigation => "under investigation",
        IncidentState.PendingRCA => "pending RCA",
        IncidentState.PendingCorrectiveAction => "pending corrective action",
        IncidentState.PendingCloseOut => "pending close-out",
        IncidentState.Closed => "closed",
        IncidentState.Cancelled => "cancelled",
        OtherState => "other or unrecorded",
        _ => state ?? "other or unrecorded",
    };

    /// <summary>
    /// Whether an obligation is due: it is not fulfilled and its recorded critical date - or its expiry date when no
    /// critical date is recorded - has arrived at the as-of date.
    /// </summary>
    /// <remarks>
    /// Only recorded dates are used; no lead time is invented, and an obligation with no date at all is not counted as due
    /// because nothing says when it is. HSE-05's calendar is where a fuller reading belongs.
    /// </remarks>
    public static bool IsDue(bool fulfilled, DateTime? criticalDate, DateTime? expiryDate, DateTime asOf)
    {
        if (fulfilled)
        {
            return false;
        }

        var due = criticalDate ?? expiryDate;

        return due is { } date && date.Date <= asOf.Date;
    }

    /// <summary>
    /// Whether a corrective-action step is overdue: its recorded due date has passed and its latest recorded step status is
    /// not completed. A step with no status row is open, which is the owner's default.
    /// </summary>
    public static bool IsOverdueAction(DateTime? dueDate, string? latestStatus, DateTime asOf) =>
        dueDate is { } due && due.Date < asOf.Date &&
        !string.Equals(latestStatus?.Trim(), HseReferenceCodes.CorrectiveAction.Completed,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether a HAZOP deviation's latest status still needs action.</summary>
    public static bool IsOpenHazopAction(string? latestStatus) =>
        OpenHazopStates.Contains(latestStatus?.Trim().ToUpperInvariant() ?? string.Empty, StringComparer.Ordinal);

    /// <summary>Whether a permit's latest recorded status means it is still pending.</summary>
    public static bool IsPendingPermit(string? latestStatus) =>
        PendingPermitStates.Contains(latestStatus?.Trim().ToUpperInvariant() ?? string.Empty, StringComparer.Ordinal);
}

/// <summary>HSE-01: the safety officer's landing - counts from recorded facts, the state strip, the inbox and the links.</summary>
public interface IOgHseWorkspace
{
    /// <param name="canOpen">
    /// The shell's page-permission answer for a route. Every source link is decided by it (AC-5), so a link that is offered
    /// is one the person can reach.
    /// </param>
    Task<OgResult<OgHseSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? jurisdiction,
        string personaCode,
        IReadOnlyCollection<string> roles,
        DateTime asOf,
        string timeZoneId,
        Func<string, bool> canOpen,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HSE-01 over the owner's <c>HSE_INCIDENT</c>, <c>HSE_INCIDENT_RESPONSE</c>, <c>OBLIGATION</c>, <c>PROJECT</c>,
/// <c>PROJECT_STEP</c>, <c>PROJECT_STATUS</c> and <c>PERMIT_STATUS_HISTORY</c>, composed with WFL-05's task inbox.
/// </summary>
/// <remarks>
/// <para>
/// Every figure is a count over rows: an incident is in the state of its <b>latest</b> STATUS response, an action is overdue
/// when its recorded due date has passed and its latest step status is not completed, and a permit is pending when its latest
/// recorded history status is one of the pending keys. Nothing is defaulted and no jurisdiction is assumed - an absent one is
/// shown as "not specified" (the Web's default-to-USA bug is not repeated).
/// </para>
/// <para>
/// A read that fails returns a failed result with a reference and <b>no counts</b>, so a zero on the page always means the
/// records say zero (AC-3). The inbox is composed rather than re-read: WFL-05 owns it and already proves it.
/// </para>
/// </remarks>
public sealed class OgHseWorkspace : IOgHseWorkspace
{
    public const string CodeNoConnection = "hse-workspace-no-connection";
    public const string CodeReadFailed = "hse-workspace-read-failed";

    /// <summary>
    /// The workspace's sources and the feature that adds any page this build does not carry. A route that is not in the
    /// catalog, or that this account may not open, is not offered and its reason is shown (AC-5).
    /// </summary>
    public static IReadOnlyList<(string Route, string Label, string? ArrivesWith)> Sources { get; } =
    [
        ("wells", "Well index", null),
        ("tasks", "Task inbox", null),
        ("incidents", "Incidents", null),
        ("hazop", "HAZOP studies", null),
        ("compliance", "Compliance calendar", null),
        ("ghg", "GHG reporting", null),
        ("permits", "Permit applications", null),
    ];

    private readonly Func<IDataSource?> _source;
    private readonly IOgTaskInbox _inbox;
    private readonly IFailureReporter _failures;

    public OgHseWorkspace(Func<IDataSource?> source, IOgTaskInbox inbox, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _inbox = inbox ?? throw new ArgumentNullException(nameof(inbox));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgHseSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? jurisdiction,
        string personaCode,
        IReadOnlyCollection<string> roles,
        DateTime asOf,
        string timeZoneId,
        Func<string, bool> canOpen,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentNullException.ThrowIfNull(canOpen);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgHseSnapshot>.Failed(
                "No connection is open, so the HSE workspace cannot be read.", CodeNoConnection);
        }

        try
        {
            var readAt = DateTime.UtcNow;

            var incidents = await OgUow.ReadAllAsync<HSE_INCIDENT>(source).ConfigureAwait(false);
            var responses = await OgUow.ReadAllAsync<HSE_INCIDENT_RESPONSE>(source).ConfigureAwait(false);
            var obligations = await OgUow.ReadAllAsync<OBLIGATION>(source).ConfigureAwait(false);
            var projects = await OgUow.ReadAllAsync<PROJECT>(source).ConfigureAwait(false);
            var steps = await OgUow.ReadAllAsync<PROJECT_STEP>(source).ConfigureAwait(false);
            var statuses = await OgUow.ReadAllAsync<PROJECT_STATUS>(source).ConfigureAwait(false);
            var permitHistory = await OgUow.ReadAllAsync<PERMIT_STATUS_HISTORY>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            // ---- incidents: the state is the latest STATUS response ----
            var latestState = responses
                .Where(response => !string.Equals(response.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(response => string.Equals(response.RESPONSE_TYPE?.Trim(), "STATUS",
                    StringComparison.OrdinalIgnoreCase))
                .GroupBy(response => response.INCIDENT_ID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(response => response.RESPONSE_OBS_NO)
                        .ThenByDescending(response => response.EFFECTIVE_DATE)
                        .First().RESPONSE_RESULT,
                    StringComparer.OrdinalIgnoreCase);

            var activeIncidents = incidents
                .Where(incident => !string.Equals(incident.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(incident => !string.IsNullOrWhiteSpace(incident.INCIDENT_ID))
                .ToList();

            var incidentStates = HseRules.Classify(activeIncidents
                .Select(incident => latestState.TryGetValue(incident.INCIDENT_ID, out var state) ? state : null)
                .ToList());

            var openIncidents = HseRules.OpenOf(incidentStates);

            // ---- obligations ----
            var obligationsDue = obligations
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Count(row => HseRules.IsDue(
                    string.Equals(row.FULFILLED_IND?.Trim(), "Y", StringComparison.OrdinalIgnoreCase),
                    row.CRITICAL_DATE,
                    row.EXPIRY_DATE,
                    asOf));

            // ---- corrective actions: overdue steps of a CORRECTIVE_ACTION project ----
            var correctiveProjects = projects
                .Where(project => string.Equals(project.PROJECT_TYPE?.Trim(),
                    HseReferenceCodes.CorrectiveAction.ProjectType, StringComparison.OrdinalIgnoreCase))
                .Where(project => !string.Equals(project.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Select(project => project.PROJECT_ID)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var latestStepStatus = statuses
                .Where(status => !string.Equals(status.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(status => string.Equals(status.STATUS_TYPE?.Trim(),
                    HseReferenceCodes.CorrectiveAction.StatusType, StringComparison.OrdinalIgnoreCase))
                .GroupBy(status => Key(status.PROJECT_ID, status.STEP_ID), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(status => status.EFFECTIVE_DATE)
                        .ThenByDescending(status => status.ROW_CREATED_DATE)
                        .First().STATUS,
                    StringComparer.OrdinalIgnoreCase);

            var overdueActions = steps
                .Where(step => correctiveProjects.Contains(step.PROJECT_ID))
                .Where(step => !string.Equals(step.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Count(step => HseRules.IsOverdueAction(
                    step.DUE_DATE,
                    latestStepStatus.TryGetValue(Key(step.PROJECT_ID, step.STEP_ID), out var status) ? status : null,
                    asOf));

            // ---- HAZOP actions: the latest deviation status of a HAZOP project ----
            var hazopProjects = projects
                .Where(project => string.Equals(project.PROJECT_TYPE?.Trim(),
                    HseReferenceCodes.Hazop.ProjectType, StringComparison.OrdinalIgnoreCase))
                .Where(project => !string.Equals(project.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Select(project => project.PROJECT_ID)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var latestDeviation = statuses
                .Where(status => !string.Equals(status.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(status => hazopProjects.Contains(status.PROJECT_ID))
                .Where(status => string.Equals(status.STATUS_TYPE?.Trim(),
                    HseReferenceCodes.Hazop.DeviationStatusType, StringComparison.OrdinalIgnoreCase))
                .GroupBy(status => Key(status.PROJECT_ID, status.STATUS_ID), StringComparer.OrdinalIgnoreCase)
                .Select(group => group
                    .OrderByDescending(status => status.EFFECTIVE_DATE)
                    .ThenByDescending(status => status.ROW_CREATED_DATE)
                    .First().STATUS)
                .ToList();

            var openHazopActions = latestDeviation.Count(HseRules.IsOpenHazopAction);

            // ---- permits: the latest recorded history status of each application ----
            var latestPermit = permitHistory
                .Where(row => !string.IsNullOrWhiteSpace(row.PERMIT_APPLICATION_ID))
                .GroupBy(row => row.PERMIT_APPLICATION_ID, StringComparer.OrdinalIgnoreCase)
                .Select(group => group
                    .OrderByDescending(row => row.STATUS_DATE)
                    .ThenByDescending(row => row.ROW_CREATED_DATE)
                    .First().STATUS)
                .ToList();

            var permitsPending = latestPermit.Count(HseRules.IsPendingPermit);

            // ---- the assigned work, from WFL-05's inbox ----
            var inbox = await ReadInboxAsync(personaCode, roles, fieldId, fieldName, asOf, timeZoneId,
                cancellationToken).ConfigureAwait(false);

            var links = LinksFor(canOpen);

            return OgResult<OgHseSnapshot>.Ok(new OgHseSnapshot(
                fieldId,
                fieldName,
                jurisdiction ?? string.Empty,
                asOf,
                new OgHseCounts(openIncidents, overdueActions, obligationsDue, openHazopActions, permitsPending),
                incidentStates,
                inbox,
                links,
                readAt));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // No counts are returned: a zero on the page must always mean the records say zero (AC-3).
            var reference = _failures.ReportHandled(
                ex, "reading the HSE workspace", "the HSE workspace is not shown; no count is inferred",
                FailureSeverity.Error);

            return OgResult<OgHseSnapshot>.Failed(
                "The HSE workspace could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + "). "),
                reference ?? CodeReadFailed);
        }
    }

    private async Task<OgHseInbox> ReadInboxAsync(
        string personaCode,
        IReadOnlyCollection<string> roles,
        string? fieldId,
        string fieldName,
        DateTime asOf,
        string timeZoneId,
        CancellationToken cancellationToken)
    {
        var inbox = await _inbox.LoadAsync(
            personaCode,
            roles,
            fieldId,
            fieldName,
            OgTaskTab.Overdue,
            asOf,
            string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId,
            cancellationToken).ConfigureAwait(false);

        if (!inbox.Success || inbox.Value is null)
        {
            // The inbox is a composed source, so its failure is stated on its own line rather than failing the page.
            return OgHseInbox.Unavailable(inbox.Message ?? "the task inbox could not be read");
        }

        var counts = inbox.Value.Counts;

        return new OgHseInbox(
            true,
            counts.Overdue,
            counts.Today,
            counts.All,
            $"overdue {counts.Overdue.ToString(CultureInfo.InvariantCulture)} · " +
            $"today {counts.Today.ToString(CultureInfo.InvariantCulture)} · " +
            $"all {counts.All.ToString(CultureInfo.InvariantCulture)}");
    }

    /// <summary>
    /// The source links for a page-permission answer (AC-5). A route this build does not carry says which feature adds it;
    /// a route that exists is offered only when the shell would open it.
    /// </summary>
    public static IReadOnlyList<OgHseLink> LinksFor(Func<string, bool> canOpen)
    {
        var routes = OgModuleCatalog.Pages.Select(page => page.Route).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return Sources.Select(source =>
        {
            if (!routes.Contains(source.Route))
            {
                return new OgHseLink(
                    source.Route,
                    source.Label,
                    false,
                    source.ArrivesWith is { Length: > 0 }
                        ? "arrives with " + source.ArrivesWith
                        : "not in this build");
            }

            return canOpen(source.Route)
                ? new OgHseLink(source.Route, source.Label, true, null)
                : new OgHseLink(source.Route, source.Label, false,
                    "your account may not open " + source.Route);
        }).ToList();
    }

    private static string Key(string? projectId, string? stepId) =>
        (projectId ?? string.Empty) + "\u001f" + (stepId ?? string.Empty);
}
