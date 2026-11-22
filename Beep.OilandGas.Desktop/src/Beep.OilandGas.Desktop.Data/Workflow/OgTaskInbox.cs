using System.Globalization;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data.Lease;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;

/// <summary>One queue of the task inbox (WFL-05).</summary>
public enum OgTaskTab
{
    Today,
    Overdue,
    Unscheduled,
    All,
    CompletedRecently,
}

/// <summary>
/// One task in the inbox. The deadline is shown in UTC and the company zone, and a missing or unconfirmed deadline
/// is never treated as overdue (AC-1, AC-2, AC-6).
/// </summary>
public sealed record OgTask(
    string Id,
    string Kind,
    string Module,
    string SubjectKind,
    string SubjectId,
    DateTime? DueAtUtc,
    bool DueConfirmed,
    string State,
    string SourceRoute,
    string? Title,
    string? Description,
    bool SourceExists,
    bool Overdue,
    bool DueToday,
    bool Unscheduled,
    bool Unconfirmed,
    string DueText,
    DateTime? CompletedAt)
{
    public bool Completed =>
        string.Equals(State, "COMPLETED", StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, "DISMISSED", StringComparison.OrdinalIgnoreCase);

    public string StateText => Completed
        ? State.ToLowerInvariant()
        : string.Equals(State, "IN_PROGRESS", StringComparison.OrdinalIgnoreCase)
            ? "in progress"
            : "pending";

    /// <summary>AC-4: a source record that cannot be found is visibly broken, not silently absent.</summary>
    public string BrokenText => SourceExists ? string.Empty : "source record not found";

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Title ?? Id,
                Module,
                DueText,
                StateText,
                SourceExists ? null : "broken link",
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>The counts per tab; each count uses the same predicate as its tab's grid (AC-3).</summary>
public sealed record OgTaskCounts(
    int Today,
    int Overdue,
    int Unscheduled,
    int All,
    int CompletedRecently,
    int Broken)
{
    public string Summary =>
        $"today {Today.ToString(CultureInfo.InvariantCulture)} · " +
        $"overdue {Overdue.ToString(CultureInfo.InvariantCulture)} · " +
        $"unscheduled {Unscheduled.ToString(CultureInfo.InvariantCulture)} · " +
        $"all {All.ToString(CultureInfo.InvariantCulture)} · " +
        $"completed {CompletedRecently.ToString(CultureInfo.InvariantCulture)}" +
        (Broken == 0 ? string.Empty : $" · broken {Broken.ToString(CultureInfo.InvariantCulture)}");
}

/// <summary>The inbox at one persona, tab and field scope.</summary>
public sealed record OgTaskInboxSnapshot(
    string PersonaCode,
    OgTaskTab Tab,
    IReadOnlyList<OgTask> Tasks,
    OgTaskCounts Counts,
    DateTime AsOf)
{
    public int Shown => Tasks.Count;

    public bool IsEmpty => Tasks.Count == 0;

    public string TabLabel => Tab switch
    {
        OgTaskTab.Today => "Today",
        OgTaskTab.Overdue => "Overdue",
        OgTaskTab.Unscheduled => "Unscheduled",
        OgTaskTab.CompletedRecently => "Completed recently",
        OgTaskTab.All => "All",
    };
}

/// <summary>WFL-05: the signed-in person's tasks across modules.</summary>
public interface IOgTaskInbox
{
    Task<OgResult<OgTaskInboxSnapshot>> LoadAsync(
        string personaCode,
        IReadOnlyCollection<string> roles,
        string? fieldId,
        string fieldName,
        OgTaskTab tab,
        DateTime asOf,
        string timeZoneId,
        CancellationToken cancellationToken = default);

    /// <summary>Starts a load; a later load invalidates this one (AC-5).</summary>
    long BeginLoad();

    /// <summary>False when a newer load has started, so a late response must be discarded (AC-5).</summary>
    bool IsCurrent(long version);
}

/// <summary>
/// The inbox over the LifeCycle-owned <c>CROSS_PERSONA_TASK</c> table and the lease-expiry projection of LSE-03,
/// read through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// The two sources are merged into one list: process tasks carry their own route and role, while the expiring
/// leases are projected with the lease route so LSE-03 AC-3's items are consumed here. A process task whose
/// <c>ENTITY_TYPE</c> is <c>LEASE_ACQUISITION</c> is checked against the acquisition table; a missing row is a
/// broken link. A type this inbox cannot check is not marked broken.
/// </remarks>
public sealed class OgTaskInboxService : IOgTaskInbox
{
    public const string CodeNoConnection = "task-inbox-no-connection";
    public const string CodeReadFailed = "task-inbox-read-failed";
    public const string CodeLeaseReadFailed = "task-inbox-lease-read-failed";

    private const string LeaseEntityType = "LEASE_ACQUISITION";

    private readonly Func<IDataSource?> _source;
    private readonly IOgLeaseFinancials _leases;
    private readonly IFailureReporter _failures;
    private long _version;

    public OgTaskInboxService(
        Func<IDataSource?> source,
        IOgLeaseFinancials leases,
        IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _leases = leases ?? throw new ArgumentNullException(nameof(leases));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public long BeginLoad() => Interlocked.Increment(ref _version);

    public bool IsCurrent(long version) => version == Interlocked.Read(ref _version);

    public async Task<OgResult<OgTaskInboxSnapshot>> LoadAsync(
        string personaCode,
        IReadOnlyCollection<string> roles,
        string? fieldId,
        string fieldName,
        OgTaskTab tab,
        DateTime asOf,
        string timeZoneId,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgTaskInboxSnapshot>.Failed(
                "No connection is open, so the task inbox cannot be read.", CodeNoConnection);
        }

        try
        {
            var processTasks = await OgUow.ReadAllAsync<CROSS_PERSONA_TASK>(source).ConfigureAwait(false);
            var acquisitions = await OgUow.ReadAllAsync<LEASE_ACQUISITION>(source).ConfigureAwait(false);
            var leases = await _leases.LoadAsync(fieldId, fieldName, asOf, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (!leases.Success || leases.Value is null)
            {
                return OgResult<OgTaskInboxSnapshot>.Failed(
                    leases.Message ?? "The lease expiry tasks could not be read.",
                    leases.Reference ?? CodeLeaseReadFailed);
            }

            var leaseIds = acquisitions
                .Where(acquisition => !string.IsNullOrWhiteSpace(acquisition.LEASE_ACQUISITION_ID))
                .Select(acquisition => acquisition.LEASE_ACQUISITION_ID!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var zone = ResolveZone(timeZoneId);
            var items = new List<OgTask>();

            foreach (var task in processTasks)
            {
                if (!PersonaMatches(task, personaCode) || !RoleMatches(task, roles))
                {
                    continue;
                }

                if (string.Equals(task.TASK_STATUS, "DISMISSED", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                items.Add(MapTask(task, asOf.Date, zone, leaseIds));
            }

            foreach (var lease in leases.Value.Leases)
            {
                if (lease.Expiry is not { } expiry
                    || expiry.Date < asOf.Date
                    || (expiry.Date - asOf.Date).TotalDays > 90)
                {
                    continue;
                }

                items.Add(LeaseTask(lease, asOf.Date, zone));
            }

            var counts = Count(items);
            var shown = Filter(items, tab);

            return OgResult<OgTaskInboxSnapshot>.Ok(new OgTaskInboxSnapshot(
                string.IsNullOrWhiteSpace(personaCode) ? "(no persona)" : personaCode.Trim(),
                tab,
                shown,
                counts,
                asOf.Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the task inbox",
                "the inbox is not shown",
                FailureSeverity.Error);

            return OgResult<OgTaskInboxSnapshot>.Failed(
                "The task inbox could not be read.", reference ?? CodeReadFailed);
        }
    }

    public static OgTask MapTask(
        CROSS_PERSONA_TASK task,
        DateTime asOf,
        TimeZoneInfo zone,
        IReadOnlySet<string> leaseIds)
    {
        var due = task.DUE_DATE;
        var confirmed = string.Equals(task.DUE_CONFIRMED_IND, "Y", StringComparison.OrdinalIgnoreCase);
        var state = string.IsNullOrWhiteSpace(task.TASK_STATUS) ? "PENDING" : task.TASK_STATUS.Trim().ToUpperInvariant();
        var completed = string.Equals(state, "COMPLETED", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(state, "DISMISSED", StringComparison.OrdinalIgnoreCase);

        var sourceExists = !string.Equals(task.ENTITY_TYPE, LeaseEntityType, StringComparison.OrdinalIgnoreCase)
                           || (!string.IsNullOrWhiteSpace(task.ENTITY_ID) && leaseIds.Contains(task.ENTITY_ID));

        var overdue = !completed && confirmed && due is { } deadline && deadline.Date < asOf.Date;
        var dueToday = !completed && due is { } today && today.Date == asOf.Date;
        var unscheduled = !completed && due is null;
        var unconfirmed = !completed && due is not null && !confirmed;

        return new OgTask(
            task.CROSS_TASK_ID ?? string.Empty,
            string.IsNullOrWhiteSpace(task.TASK_TYPE) ? "TASK" : task.TASK_TYPE.Trim().ToUpperInvariant(),
            "workflow",
            task.ENTITY_TYPE ?? string.Empty,
            task.ENTITY_ID ?? string.Empty,
            due,
            confirmed,
            state,
            task.ROUTE ?? string.Empty,
            string.IsNullOrWhiteSpace(task.ENTITY_DESCRIPTION) ? task.TASK_TYPE : task.ENTITY_DESCRIPTION,
            task.TASK_CONTEXT_JSON,
            sourceExists,
            overdue,
            dueToday,
            unscheduled,
            unconfirmed,
            DueText(due, confirmed, zone),
            completed ? task.ROW_CHANGED_DATE : null);
    }

    private static OgTask LeaseTask(OgLeaseFinancialSummary lease, DateTime asOf, TimeZoneInfo zone)
    {
        var expiry = lease.Expiry!.Value;

        return new OgTask(
            $"lease-expiry:{lease.Id}:{expiry:yyyyMMdd}",
            "LEASE_EXPIRY",
            "lease",
            LeaseEntityType,
            lease.Id,
            expiry,
            true,
            "PENDING",
            "leasefinancials",
            $"{lease.Name} expires {expiry.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
            $"Expiry state {lease.ExpiryState}",
            true,
            Overdue: false,
            DueToday: expiry.Date == asOf.Date,
            Unscheduled: false,
            Unconfirmed: false,
            DueText: DueText(expiry, true, zone),
            CompletedAt: null);
    }

    public static OgTaskCounts Count(IReadOnlyList<OgTask> tasks) =>
        new(
            tasks.Count(task => task.DueToday && !task.Completed),
            tasks.Count(task => task.Overdue),
            tasks.Count(task => task.Unscheduled),
            tasks.Count,
            tasks.Count(task => task.Completed),
            tasks.Count(task => !task.SourceExists));

    public static IReadOnlyList<OgTask> Filter(IReadOnlyList<OgTask> tasks, OgTaskTab tab) =>
        tab switch
        {
            OgTaskTab.Today => tasks
                .Where(task => task.DueToday && !task.Completed)
                .OrderBy(task => task.DueAtUtc)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            OgTaskTab.Overdue => tasks
                .Where(task => task.Overdue)
                .OrderBy(task => task.DueAtUtc)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            OgTaskTab.Unscheduled => tasks
                .Where(task => task.Unscheduled)
                .OrderBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            OgTaskTab.CompletedRecently => tasks
                .Where(task => task.Completed)
                .OrderByDescending(task => task.CompletedAt ?? DateTime.MinValue)
                .ToList(),
            OgTaskTab.All => tasks
                .OrderBy(task => task.Completed)
                .ThenBy(task => task.DueAtUtc ?? DateTime.MaxValue)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .ToList(),
        };

    private static bool PersonaMatches(CROSS_PERSONA_TASK task, string personaCode) =>
        !string.IsNullOrWhiteSpace(personaCode)
        && string.Equals(
            task.TARGET_PERSONA_CODE?.Trim() ?? string.Empty,
            personaCode.Trim(),
            StringComparison.OrdinalIgnoreCase);

    private static bool RoleMatches(CROSS_PERSONA_TASK task, IReadOnlyCollection<string> roles) =>
        roles is null
        || roles.Count == 0
        || string.IsNullOrWhiteSpace(task.ASSIGNED_ROLE)
        || roles.Contains(task.ASSIGNED_ROLE.Trim(), StringComparer.OrdinalIgnoreCase);

    public static string DueText(DateTime? due, bool confirmed, TimeZoneInfo zone)
    {
        if (due is null)
        {
            return "unscheduled";
        }

        if (!confirmed)
        {
            return $"{due.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} " +
                   "(time zone unconfirmed)";
        }

        var utc = DateTime.SpecifyKind(due.Value, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, zone);

        return $"{utc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} UTC " +
               $"({local.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} {zone.Id})";
    }

    public static TimeZoneInfo ResolveZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId)
            && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId.Trim(), out var zone))
        {
            return zone;
        }

        return TimeZoneInfo.Utc;
    }
}
