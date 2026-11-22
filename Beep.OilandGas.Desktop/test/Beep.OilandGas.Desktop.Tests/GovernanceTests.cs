using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-08: a restore appends a version instead of rewriting history, the access audit records reader/table/filter/
/// row count and exports exactly what the screen shows, never-run and last-failed are distinct, a sync conflict
/// needs an explicit attributed choice, and a broken audit chain blocks governed writes until acknowledged.
/// </summary>
public sealed class GovernanceTests
{
    private static Dictionary<string, object?> Values(string name) =>
        new(StringComparer.OrdinalIgnoreCase) { ["WELL_ID"] = "W-1", ["WELL_NAME"] = name };

    [Fact]
    public void A_restore_appends_a_version_and_leaves_the_history_untouched()
    {
        var store = new OgVersionStore();
        var first = store.Record("T_WELL", "W-1", Values("Alpha"), "fahad");
        store.Record("T_WELL", "W-1", Values("Bravo"), "fahad");

        var restored = store.Restore("T_WELL", "W-1", first.Id, "reviewer");

        Assert.True(restored.Success);
        var version = restored.Value!;

        // AC-1: a new version, not a rewrite.
        Assert.Equal(3, version.Version);
        Assert.Equal(first.Id, version.RestoredFrom);
        Assert.Equal("Alpha", version.Values["WELL_NAME"]);

        var history = store.History("T_WELL", "W-1");

        Assert.Equal(3, history.Count);
        Assert.Equal("Alpha", history[0].Values["WELL_NAME"]);
        Assert.Equal("Bravo", history[1].Values["WELL_NAME"]);
        Assert.Equal([1, 2, 3], history.Select(entry => entry.Version));
    }

    [Fact]
    public void A_restore_of_an_unknown_version_or_without_an_actor_is_refused()
    {
        var store = new OgVersionStore();
        store.Record("T_WELL", "W-1", Values("Alpha"), "fahad");

        var unknown = store.Restore("T_WELL", "W-1", "not-a-version", "reviewer");
        Assert.False(unknown.Success);
        Assert.Equal("version-unknown", unknown.Reference);

        var anonymous = store.Restore("T_WELL", "W-1", store.History("T_WELL", "W-1")[0].Id, "  ");
        Assert.False(anonymous.Success);
        Assert.Equal("version-actor-required", anonymous.Reference);
    }

    [Fact]
    public void The_access_audit_records_reader_table_filter_and_row_count()
    {
        var audit = new OgAccessAudit();

        audit.Record("fahad", "T_WELL", "WELL_NAME~Alpha", 12);

        var entry = Assert.Single(audit.Query());

        // AC-2
        Assert.Equal("fahad", entry.Reader);
        Assert.Equal("T_WELL", entry.Table);
        Assert.Equal("WELL_NAME~Alpha", entry.Filter);
        Assert.Equal(12, entry.RowCount);
    }

    [Fact]
    public void The_export_is_exactly_the_rows_the_filter_shows()
    {
        var audit = new OgAccessAudit();
        var at = new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

        audit.Record("fahad", "T_WELL", "none", 5, at);
        audit.Record("sara", "T_WELL", "DEPTH>10", 2, at.AddMinutes(1));
        audit.Record("sara", "T_LOG", "none", 900, at.AddMinutes(2));

        var query = new OgAccessAuditQuery(Table: "T_WELL");
        var shown = audit.Query(query);
        var csv = audit.ToCsv(query);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // AC-5: one header plus exactly the shown rows - the T_LOG read is not in the file.
        Assert.Equal(2, shown.Count);
        Assert.Equal(3, lines.Length);
        Assert.DoesNotContain("T_LOG", csv, StringComparison.Ordinal);
        Assert.Contains("DEPTH>10", csv, StringComparison.Ordinal);
    }

    [Fact]
    public void Never_run_and_last_failed_are_distinct_in_word_and_badge()
    {
        var never = new OgSyncStatus("company-api", OgSyncState.NeverRun);
        var failed = new OgSyncStatus(
            "company-api",
            OgSyncState.Failed,
            LastAttemptUtc: new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero),
            LastSuccessUtc: new DateTimeOffset(2026, 11, 6, 10, 0, 0, TimeSpan.Zero),
            Error: "connection refused");
        var ok = new OgSyncStatus("company-api", OgSyncState.Succeeded, LastSuccessUtc: DateTimeOffset.UtcNow);

        // AC-3: different sentences, different badges, so colour is not the only signal.
        Assert.Contains("never", never.Describe(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("failed", failed.Describe(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("connection refused", failed.Describe(), StringComparison.Ordinal);
        Assert.NotEqual(never.Badge, failed.Badge);
        Assert.NotEqual(failed.Badge, ok.Badge);
        Assert.NotEqual(never.Describe(), failed.Describe());
    }

    [Fact]
    public void A_sync_conflict_stays_unresolved_until_a_person_chooses()
    {
        var conflict = new OgSyncConflict(
            "c1", "T_WELL", "W-1",
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["WELL_NAME"] = "Local" },
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["WELL_NAME"] = "Remote" },
            DateTimeOffset.UtcNow);

        Assert.False(conflict.IsResolved);
        Assert.Equal(1, OgSyncConflicts.Unresolved([conflict]));

        var noChoice = OgSyncConflicts.Resolve(conflict, null, "fahad");
        Assert.False(noChoice.Success);
        Assert.Equal("conflict-choice-required", noChoice.Reference);

        var noActor = OgSyncConflicts.Resolve(conflict, OgConflictChoice.KeepLocal, "  ");
        Assert.False(noActor.Success);
        Assert.Equal("conflict-actor-required", noActor.Reference);

        var resolved = OgSyncConflicts.Resolve(conflict, OgConflictChoice.KeepRemote, "fahad");

        // AC-4: explicit choice, recorded actor.
        Assert.True(resolved.Success);
        Assert.Equal(OgConflictChoice.KeepRemote, resolved.Value!.Resolution);
        Assert.Equal("fahad", resolved.Value.ResolvedBy);
        Assert.Equal(0, OgSyncConflicts.Unresolved([resolved.Value]));
    }

    [Fact]
    public void A_broken_chain_is_reported_and_blocks_writes_until_acknowledged()
    {
        var chain = new OgAuditChain();
        chain.Append("row.update", "T_WELL/W-1 WELL_NAME Alpha->Bravo", "fahad");
        chain.Append("row.delete", "T_WELL/W-2", "fahad");

        Assert.True(chain.Verify().IsIntact);
        Assert.True(chain.WritesAllowed(chain.Verify(), null));

        chain.Tamper(1, "T_WELL/W-1 WELL_NAME Alpha->Charlie");
        var check = chain.Verify();

        // AC-6: reported with the entry, and writes blocked.
        Assert.False(check.IsIntact);
        Assert.Equal(1, check.BrokenAtSequence);
        Assert.Contains("broken", check.Describe(), StringComparison.OrdinalIgnoreCase);
        Assert.False(chain.WritesAllowed(check, null));

        // An acknowledgement for a different break does not open writes.
        Assert.False(chain.WritesAllowed(check, new OgChainAcknowledgement(2, "fahad", DateTimeOffset.UtcNow)));

        // A named acknowledgement of this break does.
        Assert.True(chain.WritesAllowed(check, new OgChainAcknowledgement(1, "fahad", DateTimeOffset.UtcNow)));
    }
}
