using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-08's durable store: the same behaviour as the in-memory classes, but read back from SQLite by a second
/// instance - which is the whole point, since the trail has to outlive the process that wrote it.
/// </summary>
public sealed class SqliteGovernanceStoreTests
{
    private static string NewPath()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "govtest-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "governance.db");
    }

    private static Dictionary<string, object?> Values(string name) =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["WELL_ID"] = "W-1",
            ["WELL_NAME"] = name,
            ["DEPTH_MD"] = 1234.56m,
            ["SPUD_DATE"] = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc),
            ["IS_ACTIVE"] = true,
        };

    [Fact]
    public void Versions_round_trip_through_a_second_instance_with_their_types_intact()
    {
        var path = NewPath();

        var writer = new SqliteGovernanceStore(path);
        writer.Record("T_WELL", "W-1", Values("Alpha"), "fahad");

        var reader = new SqliteGovernanceStore(path);
        var history = reader.History("T_WELL", "W-1");

        var version = Assert.Single(history);
        Assert.Equal(1, version.Version);
        Assert.Equal("fahad", version.Actor);
        Assert.Equal("Alpha", version.Values["WELL_NAME"]);

        // JSON preserved the shapes: a decimal did not become a double and a date did not become a string.
        Assert.Equal(1234.56m, version.Values["DEPTH_MD"]);
        Assert.Equal(new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc), version.Values["SPUD_DATE"]);
        Assert.Equal(true, version.Values["IS_ACTIVE"]);
    }

    [Fact]
    public void History_is_ordered_and_a_restore_appends_a_new_version()
    {
        var path = NewPath();
        var writer = new SqliteGovernanceStore(path);

        var first = writer.Record("T_WELL", "W-1", Values("Alpha"), "fahad");
        writer.Record("T_WELL", "W-1", Values("Bravo"), "fahad");

        var restored = writer.Restore("T_WELL", "W-1", first.Id, "reviewer");

        Assert.True(restored.Success);

        var reader = new SqliteGovernanceStore(path);
        var history = reader.History("T_WELL", "W-1");

        Assert.Equal([1, 2, 3], history.Select(version => version.Version));
        Assert.Equal("Alpha", history[0].Values["WELL_NAME"]);   // untouched
        Assert.Equal("Bravo", history[1].Values["WELL_NAME"]);
        Assert.Equal("Alpha", history[2].Values["WELL_NAME"]);   // restored copy
        Assert.Equal(first.Id, history[2].RestoredFrom);
        Assert.Equal("reviewer", history[2].Actor);
    }

    [Fact]
    public void A_restore_of_an_unknown_version_or_without_an_actor_is_refused_and_writes_nothing()
    {
        var path = NewPath();
        var store = new SqliteGovernanceStore(path);
        var only = store.Record("T_WELL", "W-1", Values("Alpha"), "fahad");

        Assert.Equal("version-unknown", store.Restore("T_WELL", "W-1", "nope", "reviewer").Reference);
        Assert.Equal("version-actor-required", store.Restore("T_WELL", "W-1", only.Id, "  ").Reference);

        Assert.Single(new SqliteGovernanceStore(path).History("T_WELL", "W-1"));
    }

    [Fact]
    public void The_access_audit_filters_and_exports_from_the_database()
    {
        var path = NewPath();
        var writer = new SqliteGovernanceStore(path);
        var at = new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

        writer.Record("fahad", "T_WELL", "none", 5, at);
        writer.Record("sara", "T_WELL", "DEPTH>10", 2, at.AddMinutes(1));
        writer.Record("sara", "T_LOG", "none", 900, at.AddMinutes(2));

        var reader = new SqliteGovernanceStore(path);
        var shown = reader.Query(new OgAccessAuditQuery(Table: "T_WELL"));
        var csv = reader.ToCsv(new OgAccessAuditQuery(Table: "T_WELL"));

        Assert.Equal(2, shown.Count);
        Assert.Equal("sara", shown[0].Reader);                 // newest first
        Assert.Equal("DEPTH>10", shown[0].Filter);
        Assert.Equal(2, shown[0].RowCount);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);                          // header + the two shown rows
        Assert.DoesNotContain("T_LOG", csv, StringComparison.Ordinal);

        // A reader filter composes with the table filter, and a time window narrows further.
        Assert.Single(reader.Query(new OgAccessAuditQuery(Reader: "fahad", Table: "T_WELL")));
        Assert.Single(reader.Query(new OgAccessAuditQuery(Table: "T_WELL", From: at.AddMinutes(1), To: at.AddMinutes(2))));
        Assert.Empty(reader.Query(new OgAccessAuditQuery(Reader: "nobody")));
    }

    [Fact]
    public void The_chain_verifies_from_the_database_and_a_tampered_entry_is_named()
    {
        var path = NewPath();
        var writer = new SqliteGovernanceStore(path);

        writer.Append("row.update", "T_WELL/W-1 WELL_NAME Alpha->Bravo", "fahad");
        writer.Append("row.delete", "T_WELL/W-2", "fahad");

        var reader = new SqliteGovernanceStore(path);

        Assert.Equal(2, reader.Entries.Count);
        Assert.True(reader.Verify().IsIntact);

        reader.Tamper(1, "T_WELL/W-1 WELL_NAME Alpha->Charlie");

        var checker = new SqliteGovernanceStore(path);
        var check = checker.Verify();

        Assert.False(check.IsIntact);
        Assert.Equal(1, check.BrokenAtSequence);
        Assert.False(checker.WritesAllowed(check, null));
        Assert.False(checker.WritesAllowed(check, new OgChainAcknowledgement(2, "fahad", DateTimeOffset.UtcNow)));
        Assert.True(checker.WritesAllowed(check, new OgChainAcknowledgement(1, "fahad", DateTimeOffset.UtcNow)));
    }

    [Fact]
    public void A_chain_written_by_the_in_memory_algorithm_verifies_against_the_same_hash()
    {
        var at = new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

        var memory = new OgAuditChain();
        var entry = memory.Append("row.update", "detail", "fahad", at);

        // DAT-08 AC-6 depends on the payload format being one format, not two that nearly match.
        Assert.Equal(
            SqliteGovernanceStore.HashOf(1, "row.update", "detail", "fahad", at, "genesis"),
            entry.Hash);
    }

    [Fact]
    public void The_quality_journal_advances_only_on_success_and_survives_a_restart()
    {
        var path = NewPath();
        var store = new SqliteGovernanceStore(path);
        var good = new DateTimeOffset(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

        var run = new OgQualityRun(
            "T_WELL", good, good.AddSeconds(2), 10, 3, Succeeded: true, Findings: []);

        store.Record(run);

        var reader = new SqliteGovernanceStore(path);
        Assert.Equal(good.AddSeconds(2), reader.LastSuccess("T_WELL"));

        // A failed run must not move the timestamp - that is the whole point of the journal (AC-3).
        reader.Record(run with
        {
            Succeeded = false,
            CompletedAtUtc = good.AddDays(1),
            Error = "the connection dropped",
        });

        Assert.Equal(good.AddSeconds(2), new SqliteGovernanceStore(path).LastSuccess("T_WELL"));
        Assert.Null(reader.LastSuccess("T_NEVER_RUN"));
    }
    [Fact]
    public void The_chain_gate_reopens_writes_only_for_the_acknowledged_break()
    {        var path = NewPath();
        var store = new SqliteGovernanceStore(path);
        store.Append("row.update", "one", "fahad");
        store.Append("row.update", "two", "fahad");
        store.Tamper(1, "changed");

        var gate = new OgChainGate();
        Assert.False(gate.Allows(store));

        gate.Acknowledge(2, "fahad");
        Assert.False(gate.Allows(store));                       // the wrong break

        gate.Acknowledge(1, "fahad");
        Assert.True(gate.Allows(store));                        // this break, named
    }
}
