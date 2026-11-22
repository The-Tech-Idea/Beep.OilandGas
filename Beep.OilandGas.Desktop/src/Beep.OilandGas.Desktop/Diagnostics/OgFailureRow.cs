using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>Where a stored failure is in its journey to the API.</summary>
public enum OgForwardingState
{
    /// <summary>This installation does not forward; the record exists only on this machine.</summary>
    LocalOnly,

    /// <summary>Forwarding is on and the API has not confirmed the report yet.</summary>
    Pending,

    /// <summary>The API accepted the report; the same reference is in its store.</summary>
    Forwarded,
}

/// <summary>
/// One row of the failures page: the stored record projected for display, with its forwarding state.
/// </summary>
/// <remarks>
/// A separate type so the page's rules — which label, what "forwarded" means, which fields are shown — can
/// be tested without a window. The page reads local SQLite first; forwarding never decides what is shown,
/// only how the state reads.
/// </remarks>
public sealed record OgFailureRow(
    string Reference,
    DateTimeOffset OccurredAt,
    string Severity,
    string Operation,
    string Consequence,
    OgForwardingState Forwarding,
    string? UserId)
{
    /// <summary>The state in words, as the page shows it.</summary>
    public string ForwardingLabel => Forwarding switch
    {
        OgForwardingState.Forwarded => "Forwarded",
        OgForwardingState.Pending => "Pending delivery",
        OgForwardingState.LocalOnly => "Local only",
    };

    /// <summary>Local time, because the person reads it on this machine.</summary>
    public string OccurredAtText => OccurredAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

    public static OgFailureRow From(OgFailureRecord record, bool forwardingEnabled)
    {
        ArgumentNullException.ThrowIfNull(record);

        var state = record.Forwarded
            ? OgForwardingState.Forwarded
            : forwardingEnabled
                ? OgForwardingState.Pending
                : OgForwardingState.LocalOnly;

        return new OgFailureRow(
            record.Reference,
            record.OccurredAtUtc,
            record.Severity.ToString(),
            record.Operation,
            record.Consequence,
            state,
            record.UserId);
    }

    /// <summary>Projects a list, keeping the store's order (newest first).</summary>
    public static IReadOnlyList<OgFailureRow> From(
        IEnumerable<OgFailureRecord> records,
        bool forwardingEnabled) =>
        records.Select(record => From(record, forwardingEnabled)).ToList();
}

/// <summary>One row of the audit page.</summary>
public sealed record OgAuditRow(
    long Id,
    string AtText,
    string Verb,
    string SubjectKind,
    string SubjectId,
    string Outcome,
    string? Detail,
    string? UserId)
{
    public static OgAuditRow From(OgAuditRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new OgAuditRow(
            record.Id,
            record.AtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
            record.Verb,
            record.SubjectKind,
            record.SubjectId,
            record.Outcome,
            record.Detail,
            record.UserId);
    }

    public static IReadOnlyList<OgAuditRow> From(IEnumerable<OgAuditRecord> records) =>
        records.Select(From).ToList();
}

/// <summary>The failure count summary the page shows above the grid.</summary>
public sealed record OgFailureSummary(int Total, int Forwarded, int Pending, bool ForwardingEnabled)
{
    public static OgFailureSummary From(IReadOnlyList<OgFailureRow> rows, bool forwardingEnabled) =>
        new(
            rows.Count,
            rows.Count(row => row.Forwarding == OgForwardingState.Forwarded),
            rows.Count(row => row.Forwarding == OgForwardingState.Pending),
            forwardingEnabled);

    public string Describe() => ForwardingEnabled
        ? $"{Total} record(s) · {Forwarded} forwarded · {Pending} pending delivery"
        : $"{Total} record(s) · forwarding is off for this installation";
}
