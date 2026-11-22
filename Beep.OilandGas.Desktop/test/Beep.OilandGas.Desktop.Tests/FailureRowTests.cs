using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-06: the failures page reads the local rows and says, for each one, whether the API has it. These are
/// the rules, tested without a window.
/// </summary>
public sealed class FailureRowTests
{
    [Fact]
    public void A_forwarded_record_says_Forwarded()
    {
        var row = OgFailureRow.From(Record(forwarded: true), forwardingEnabled: true);

        Assert.Equal(OgForwardingState.Forwarded, row.Forwarding);
        Assert.Equal("Forwarded", row.ForwardingLabel);
    }

    [Fact]
    public void A_record_not_yet_accepted_by_the_api_says_pending_delivery()
    {
        var row = OgFailureRow.From(Record(forwarded: false), forwardingEnabled: true);

        Assert.Equal(OgForwardingState.Pending, row.Forwarding);
        Assert.Equal("Pending delivery", row.ForwardingLabel);
    }

    [Fact]
    public void With_forwarding_off_a_record_says_local_only_rather_than_pretending_to_queue()
    {
        var row = OgFailureRow.From(Record(forwarded: false), forwardingEnabled: false);

        Assert.Equal(OgForwardingState.LocalOnly, row.Forwarding);
        Assert.Equal("Local only", row.ForwardingLabel);
    }

    [Fact]
    public void The_rows_come_from_the_local_store_in_its_order()
    {
        var store = new SqliteFailureStore(PrivateStorePath());
        var older = Stored(store, DateTimeOffset.UtcNow.AddMinutes(-2), "older");
        var newer = Stored(store, DateTimeOffset.UtcNow, "newer");

        var rows = OgFailureRow.From(store.Recent(10), forwardingEnabled: true);

        Assert.Equal([newer, older], rows.Select(row => row.Reference).ToArray());
    }

    [Fact]
    public void The_summary_counts_forwarded_and_pending_and_says_when_forwarding_is_off()
    {
        var rows = new[]
        {
            OgFailureRow.From(Record(forwarded: true), true),
            OgFailureRow.From(Record(forwarded: false), true),
            OgFailureRow.From(Record(forwarded: false), true),
        };

        var on = OgFailureSummary.From(rows, forwardingEnabled: true);
        Assert.Equal(3, on.Total);
        Assert.Equal(1, on.Forwarded);
        Assert.Equal(2, on.Pending);
        Assert.Contains("2 pending delivery", on.Describe(), StringComparison.Ordinal);

        var off = OgFailureSummary.From(rows, forwardingEnabled: false);
        Assert.Contains("forwarding is off", off.Describe(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Marking_delivered_records_is_what_turns_a_row_into_Forwarded()
    {
        var store = new SqliteFailureStore(PrivateStorePath());
        var reference = Stored(store, DateTimeOffset.UtcNow, "probe");

        Assert.False(store.Find(reference)!.Forwarded);
        Assert.Equal(OgForwardingState.Pending, OgFailureRow.From(store.Find(reference)!, true).Forwarding);

        Assert.Equal(1, store.MarkForwarded([reference]));

        Assert.True(store.Find(reference)!.Forwarded);
        Assert.Equal(OgForwardingState.Forwarded, OgFailureRow.From(store.Find(reference)!, true).Forwarding);
    }

    [Fact]
    public void An_audit_row_carries_the_actor_the_subject_and_the_outcome()
    {
        var row = OgAuditRow.From(new OgAuditRecord(
            7,
            DateTimeOffset.UtcNow,
            "well.status.change",
            "well",
            "W-1",
            "refused",
            "period is closed",
            "auditor",
            "c1",
            "F1"));

        Assert.Equal("well.status.change", row.Verb);
        Assert.Equal("W-1", row.SubjectId);
        Assert.Equal("refused", row.Outcome);
        Assert.Equal("auditor", row.UserId);
        Assert.False(string.IsNullOrWhiteSpace(row.AtText));
    }

    private static OgFailureRecord Record(bool forwarded) =>
        new(
            FailureReferenceFormat.New(),
            DateTimeOffset.UtcNow,
            FailureSeverity.Error,
            "probe",
            "nothing shown",
            "System.InvalidOperationException",
            "probe",
            string.Empty,
            "u1",
            "c1",
            forwarded);

    private static string Stored(SqliteFailureStore store, DateTimeOffset at, string operation)
    {
        var record = new OgFailureRecord(
            FailureReferenceFormat.New(),
            at,
            FailureSeverity.Error,
            operation,
            "nothing shown",
            "System.InvalidOperationException",
            "probe",
            string.Empty,
            "u1",
            "c1",
            Forwarded: false);

        store.Write(record);
        return record.Reference;
    }

    private static string PrivateStorePath() =>
        Path.Combine(OgPaths.DataRoot, "rows-" + Guid.NewGuid().ToString("n")[..8], "failures.db");
}
