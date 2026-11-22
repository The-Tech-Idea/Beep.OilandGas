using System.Globalization;
using Beep.OilandGas.Models.Data.Decommissioning;
using LifeCycleRestoration = Beep.OilandGas.Models.Data.LifeCycle.ENVIRONMENTAL_RESTORATION;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;

/// <summary>One asset in the decommissioning scope (DEC-01), matching the feature's <c>OgDecomScope</c>.</summary>
/// <param name="PlannedYear">
/// The year the work is planned for, when a record carries one. The owner's tables record no planned date, so this is null
/// for every row this build reads - and nothing is therefore reported as overdue.
/// </param>
public sealed record OgDecomScope(
    string AssetId,
    string Kind,
    string State,
    string StateLabel,
    int? PlannedYear,
    decimal? EstimatedCost,
    decimal? ActualCost,
    string Currency,
    string? FieldId)
{
    /// <summary>AC-2: an asset with no estimate is not estimated, and is excluded from the totals.</summary>
    public bool IsEstimated => EstimatedCost is not null;

    /// <summary>AC-5: overdue is computed from the planned year, never stored.</summary>
    public bool IsOverdue(DateTime asOf) => PlannedYear is { } year && year < asOf.Year;

    public string StateText => string.IsNullOrWhiteSpace(State) ? "unclassified" : StateLabel;

    public string EstimatedText => EstimatedCost is { } estimate
        ? estimate.ToString("0.00", CultureInfo.InvariantCulture) + " " + Currency
        : "not estimated";

    public string Describe() =>
        AssetId + " · " + Kind + " · " + StateText + " · " +
        (PlannedYear is { } year ? "planned " + year.ToString(CultureInfo.InvariantCulture) : "no planned year") +
        " · " + EstimatedText;
}

/// <summary>One recorded state and how many assets are in it (DEC-01, AC-1).</summary>
public sealed record OgDecomStateCount(string State, string Label, int Count)
{
    public string Describe() => Label + " " + Count.ToString(CultureInfo.InvariantCulture);
}

/// <summary>One environmental restoration record (DEC-01), kept apart from the asset states (SG-12-4).</summary>
public sealed record OgRestoration(
    string Id,
    string SiteId,
    string Type,
    string Status,
    string Certification,
    DateTime? Completed)
{
    public bool IsCertified => string.Equals(Certification, "CERTIFIED", StringComparison.OrdinalIgnoreCase);

    public string Describe() =>
        SiteId + " · " + Type + " · " + Status.ToLowerInvariant() +
        " · certification " + (string.IsNullOrWhiteSpace(Certification) ? "not recorded" : Certification.ToLowerInvariant()) +
        (Completed is { } done ? " · completed " + done.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty);
}

/// <summary>The estimated and actual cost for one currency (DEC-01, AC-2 and AC-4).</summary>
public sealed record OgDecomCost(string Currency, decimal Estimated, decimal Actual, int Unestimated)
{
    public string Describe() =>
        Currency + " · estimated " + Estimated.ToString("0.00", CultureInfo.InvariantCulture) +
        " · actual " + Actual.ToString("0.00", CultureInfo.InvariantCulture) +
        (Unestimated == 0
            ? string.Empty
            : " · " + Unestimated.ToString(CultureInfo.InvariantCulture) + " asset(s) not estimated");
}

/// <summary>The decommissioning workspace (DEC-01).</summary>
public sealed record OgDecomWorkspace(
    string? FieldId,
    IReadOnlyList<OgDecomScope> Scopes,
    IReadOnlyList<OgDecomStateCount> States,
    IReadOnlyList<OgRestoration> Restoration,
    IReadOnlyList<OgDecomCost> Costs,
    IReadOnlyList<OgHseLink> Links,
    OgHseInbox Inbox,
    DateTime AsOf,
    DateTime ReadAt)
{
    public int Wells => Scopes.Count(scope => string.Equals(scope.Kind, "WELL", StringComparison.OrdinalIgnoreCase));

    public int Facilities =>
        Scopes.Count(scope => string.Equals(scope.Kind, "FACILITY", StringComparison.OrdinalIgnoreCase));

    /// <summary>AC-2: counted, and excluded from the totals below.</summary>
    public int Unestimated => Scopes.Count(scope => !scope.IsEstimated);

    public int Unclassified =>
        Scopes.Count(scope => string.IsNullOrWhiteSpace(scope.State));

    /// <summary>AC-5: computed at read time from the planned year.</summary>
    public int Overdue => Scopes.Count(scope => scope.IsOverdue(AsOf));

    public int RestorationOpen => Restoration.Count(record => record.Completed is null);

    public int RestorationCertified => Restoration.Count(record => record.IsCertified);

    public string CountsText =>
        Wells.ToString(CultureInfo.InvariantCulture) + " well(s) · " +
        Facilities.ToString(CultureInfo.InvariantCulture) + " facilit(ies) · " +
        Unclassified.ToString(CultureInfo.InvariantCulture) + " unclassified · " +
        Unestimated.ToString(CultureInfo.InvariantCulture) + " not estimated · " +
        Overdue.ToString(CultureInfo.InvariantCulture) + " overdue · " +
        RestorationOpen.ToString(CultureInfo.InvariantCulture) + " restoration open · " +
        RestorationCertified.ToString(CultureInfo.InvariantCulture) + " certified";

    public string StateStrip => States.Count == 0
        ? "no decommissioning state is recorded"
        : string.Join(" · ", States.Select(row => row.Describe()));

    public string ScopeText => Scopes.Count == 0
        ? "No asset is in the decommissioning scope for this field."
        : string.Join(Environment.NewLine, Scopes.Select(scope => scope.Describe()));

    public string CostText => Costs.Count == 0
        ? "No cost is recorded; nothing is estimated."
        : string.Join(Environment.NewLine, Costs.Select(cost => cost.Describe()));

    public string RestorationText => Restoration.Count == 0
        ? "No environmental restoration is recorded."
        : string.Join(Environment.NewLine, Restoration.Select(record => record.Describe()));

    public string LinksText => Links.Count == 0
        ? "no source is configured"
        : string.Join(Environment.NewLine, Links.Select(link =>
            link.Available ? link.Label + " · available" : link.Label + " · not available - " + link.WhyNot));

    public string InboxText => Inbox.Available ? "Task inbox: " + Inbox.Text : "Task inbox unavailable: " + Inbox.Text;

    public string PlannedText => Scopes.Any(scope => scope.PlannedYear is not null)
        ? "planned years are recorded for " + Scopes.Count(scope => scope.PlannedYear is not null)
            .ToString(CultureInfo.InvariantCulture) + " of " + Scopes.Count.ToString(CultureInfo.InvariantCulture) +
          " asset(s)" +
          (Scopes.Count(scope => scope.PlannedYear is null) == 0
              ? string.Empty
              : "; the rest record none, so they are not called overdue")
        : "no planned year is recorded for any asset, so nothing is overdue";

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The decommissioning workspace rules (DEC-01).</summary>
public static class DecomRules
{
    public const string CodeNoConnection = "decommissioning-no-connection";
    public const string CodeReadFailed = "decommissioning-read-failed";
    public const string CodeFieldRequired = "decommissioning-field-required";

    /// <summary>
    /// The workspace's sources and the feature that adds any page this build does not carry. A route that this account may
    /// not open is not offered and its reason is shown (DEC-01 AC-3's link rule, as HSE-01's).
    /// </summary>
    public static IReadOnlyList<(string Route, string Label, string? ArrivesWith)> Sources { get; } =
    [
        ("wells", "Well index", null),
        ("tasks", "Task inbox", null),
        ("hse", "HSE workspace", null),
        ("wellpa", "Well plug and abandonment", null),
        ("facilitycost", "Facilities and cost", null),
    ];

    /// <summary>The label for a recorded state, from the owner's reference set when it names one.</summary>
    public static string Label(
        IReadOnlyDictionary<string, string> reference,
        string referenceSet,
        string? state)
    {
        ArgumentNullException.ThrowIfNull(reference);

        if (string.IsNullOrWhiteSpace(state))
        {
            return "unclassified";
        }

        var code = state.Trim();

        return reference.TryGetValue(referenceSet + "\u001f" + code.ToUpperInvariant(), out var label) &&
               !string.IsNullOrWhiteSpace(label)
            ? label
            : code;
    }

    /// <summary>AC-1: the state strip, one row per recorded state plus the unclassified one.</summary>
    public static IReadOnlyList<OgDecomStateCount> Classify(
        IEnumerable<OgDecomScope> scopes,
        IReadOnlyDictionary<string, string> reference,
        string referenceSet)
    {
        ArgumentNullException.ThrowIfNull(scopes);

        var rows = new List<OgDecomStateCount>();

        foreach (var group in scopes
                     .GroupBy(scope => string.IsNullOrWhiteSpace(scope.State) ? string.Empty : scope.State.Trim(),
                         StringComparer.OrdinalIgnoreCase)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            rows.Add(new OgDecomStateCount(
                group.Key,
                Label(reference, referenceSet, group.Key),
                group.Count()));
        }

        return rows;
    }

    /// <summary>AC-4: the cost totals per currency, excluding the assets that are not estimated (AC-2).</summary>
    public static IReadOnlyList<OgDecomCost> CostsOf(IEnumerable<OgDecomScope> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);

        return scopes
            .GroupBy(scope => string.IsNullOrWhiteSpace(scope.Currency) ? "(no currency)" : scope.Currency.Trim(),
                StringComparer.OrdinalIgnoreCase)
            // An asset with no cost record at all - every well, on the owner's tables - is not a currency with a zero in
            // it: it is counted by the workspace as not estimated and produces no cost line.
            .Where(group => group.Any(scope => scope.IsEstimated || scope.ActualCost is not null))
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new OgDecomCost(
                group.Key,
                group.Sum(scope => scope.EstimatedCost ?? 0m),
                group.Sum(scope => scope.ActualCost ?? 0m),
                group.Count(scope => !scope.IsEstimated)))
            .ToList();
    }

    /// <summary>DEC-01's source links, decided by the page's own permission answer.</summary>
    public static IReadOnlyList<OgHseLink> LinksFor(Func<string, bool> canOpen)
    {
        ArgumentNullException.ThrowIfNull(canOpen);

        var routes = OgModuleCatalog.Pages.Select(page => page.Route).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return Sources.Select(source =>
        {
            if (!routes.Contains(source.Route))
            {
                return new OgHseLink(
                    source.Route,
                    source.Label,
                    false,
                    source.ArrivesWith is { Length: > 0 } ? "arrives with " + source.ArrivesWith : "not in this build");
            }

            return canOpen(source.Route)
                ? new OgHseLink(source.Route, source.Label, true, null)
                : new OgHseLink(source.Route, source.Label, false, "your account may not open " + source.Route);
        }).ToList();
    }
}

/// <summary>DEC-01: the decommissioning coordinator's workspace.</summary>
public interface IOgDecommissioning
{
    /// <param name="canOpen">The shell's page-permission answer, so a source link offered is one that will open.</param>
    Task<OgResult<OgDecomWorkspace>> LoadAsync(
        string? fieldId,
        string personaCode,
        IReadOnlyCollection<string> roles,
        DateTime asOf,
        string timeZoneId,
        Func<string, bool> canOpen,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DEC-01 over the owner's <c>ABANDONMENT_STATUS</c>, <c>DECOMMISSIONING_STATUS</c>, <c>ENVIRONMENTAL_RESTORATION</c> and
/// <c>R_DECOMMISSIONING_REFERENCE_CODE</c>, composed with WFL-05's task inbox.
/// </summary>
/// <remarks>
/// <para>
/// Every figure is a count over the latest recorded status row of an asset. The owner's status tables record no planned date
/// and no terminal-state flag, so the workspace reports no planned year and no overdue rather than inventing a schedule or a
/// rule for what "abandoned" means; both are recorded as the feature's limits. A read that fails returns no workspace at all,
/// so a zero on the page always means the records say zero (AC-1).
/// </para>
/// <para>
/// The restoration records are counted and listed separately from the asset states, because a facility's decommissioning state
/// says nothing about whether its site has been restored (SG-12-4).
/// </para>
/// </remarks>
public sealed class OgDecommissioning : IOgDecommissioning
{
    private const string AbandonmentReferenceSet = "ABANDONMENT_STATUS";
    private const string DecommissioningReferenceSet = "DECOMMISSIONING_STATUS";

    private readonly Func<IDataSource?> _source;
    private readonly IOgTaskInbox _inbox;
    private readonly IFailureReporter _failures;

    public OgDecommissioning(Func<IDataSource?> source, IOgTaskInbox inbox, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _inbox = inbox ?? throw new ArgumentNullException(nameof(inbox));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgDecomWorkspace>> LoadAsync(
        string? fieldId,
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
            return OgResult<OgDecomWorkspace>.Failed(
                "No connection is open, so the decommissioning workspace cannot be read.",
                DecomRules.CodeNoConnection);
        }

        try
        {
            var wellStatuses = await OgUow.ReadAllAsync<ABANDONMENT_STATUS>(source).ConfigureAwait(false);
            var facilityStatuses = await OgUow.ReadAllAsync<DECOMMISSIONING_STATUS>(source).ConfigureAwait(false);
            var restoration = await OgUow.ReadAllAsync<LifeCycleRestoration>(source).ConfigureAwait(false);
            var references = await OgUow.ReadAllAsync<R_DECOMMISSIONING_REFERENCE_CODE>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var labels = references
                .Where(row => !string.IsNullOrWhiteSpace(row.REFERENCE_SET) && !string.IsNullOrWhiteSpace(row.REFERENCE_CODE))
                .GroupBy(row => row.REFERENCE_SET!.Trim() + "\u001f" + row.REFERENCE_CODE!.Trim().ToUpperInvariant(),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().LONG_NAME ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            var scopes = new List<OgDecomScope>();

            // A well: the latest status row of its abandonment record.
            foreach (var latest in wellStatuses
                         .Where(row => !string.IsNullOrWhiteSpace(row.WELL_ABANDONMENT_ID))
                         .GroupBy(row => row.WELL_ABANDONMENT_ID, StringComparer.OrdinalIgnoreCase))
            {
                var row = latest
                    .OrderByDescending(candidate => candidate.STATUS_DATE)
                    .ThenByDescending(candidate => candidate.ROW_CREATED_DATE)
                    .First();

                scopes.Add(new OgDecomScope(
                    row.UWI ?? latest.Key,
                    "WELL",
                    row.STATUS ?? string.Empty,
                    DecomRules.Label(labels, AbandonmentReferenceSet, row.STATUS),
                    // DEC-02 records the planned date on the abandonment record, so a planned year can be read here now.
                    row.PLANNED_DATE?.Year,
                    // The abandonment table records no cost; a well's cost belongs to DEC-02's programme.
                    null,
                    null,
                    string.Empty,
                    null));
            }

            // A facility: the latest status row of its decommissioning record.
            foreach (var latest in facilityStatuses
                         .Where(row => !string.IsNullOrWhiteSpace(row.FACILITY_DECOMMISSIONING_ID))
                         .GroupBy(row => row.FACILITY_DECOMMISSIONING_ID, StringComparer.OrdinalIgnoreCase))
            {
                var row = latest
                    .OrderByDescending(candidate => candidate.STATUS_DATE)
                    .ThenByDescending(candidate => candidate.ROW_CREATED_DATE)
                    .First();

                scopes.Add(new OgDecomScope(
                    row.FACILITY_ID ?? latest.Key,
                    "FACILITY",
                    row.STATUS ?? string.Empty,
                    DecomRules.Label(labels, DecommissioningReferenceSet, row.STATUS),
                    // DEC-03 records the facility's planned date on its status row, so its planned year is readable here.
                    row.PLANNED_DATE?.Year,
                    row.ESTIMATED_COST,
                    row.ACTUAL_COST,
                    row.COST_CURRENCY ?? string.Empty,
                    row.FIELD_ID));
            }

            var scoped = string.IsNullOrWhiteSpace(fieldId)
                ? scopes
                : scopes.Where(scope =>
                    string.IsNullOrWhiteSpace(scope.FieldId) ||
                    string.Equals(scope.FieldId.Trim(), fieldId.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            var restorationRows = restoration
                .Select(row => new OgRestoration(
                    row.RESTORATION_ID ?? string.Empty,
                    row.SITE_ID ?? string.Empty,
                    row.RESTORATION_TYPE ?? string.Empty,
                    row.STATUS ?? string.Empty,
                    row.CERTIFICATION_STATUS ?? string.Empty,
                    row.COMPLETION_DATE))
                .OrderBy(record => record.SiteId, StringComparer.Ordinal)
                .ToList();

            var inbox = await ReadInboxAsync(personaCode, roles, fieldId, asOf, timeZoneId, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgDecomWorkspace>.Ok(new OgDecomWorkspace(
                string.IsNullOrWhiteSpace(fieldId) ? null : fieldId.Trim(),
                scoped.OrderBy(scope => scope.AssetId, StringComparer.Ordinal).ToList(),
                DecomRules.Classify(scoped, labels, DecommissioningReferenceSet),
                restorationRows,
                DecomRules.CostsOf(scoped),
                DecomRules.LinksFor(canOpen),
                inbox,
                asOf,
                DateTime.UtcNow));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the decommissioning workspace",
                "the decommissioning workspace is not shown; no count is inferred", FailureSeverity.Error);

            return OgResult<OgDecomWorkspace>.Failed(
                "The decommissioning workspace could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? DecomRules.CodeReadFailed);
        }
    }

    private async Task<OgHseInbox> ReadInboxAsync(
        string personaCode,
        IReadOnlyCollection<string> roles,
        string? fieldId,
        DateTime asOf,
        string timeZoneId,
        CancellationToken cancellationToken)
    {
        var inbox = await _inbox.LoadAsync(
            personaCode,
            roles,
            fieldId,
            string.Empty,
            OgTaskTab.Overdue,
            asOf,
            string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId,
            cancellationToken).ConfigureAwait(false);

        if (!inbox.Success || inbox.Value is null)
        {
            return OgHseInbox.Unavailable(inbox.Message ?? "the task inbox could not be read");
        }

        var counts = inbox.Value.Counts;

        return new OgHseInbox(
            true,
            counts.Overdue,
            counts.Today,
            counts.All,
            "overdue " + counts.Overdue.ToString(CultureInfo.InvariantCulture) +
            " · today " + counts.Today.ToString(CultureInfo.InvariantCulture) +
            " · all " + counts.All.ToString(CultureInfo.InvariantCulture));
    }
}
