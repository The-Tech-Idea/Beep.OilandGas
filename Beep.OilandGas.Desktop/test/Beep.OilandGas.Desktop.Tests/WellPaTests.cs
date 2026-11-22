using TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEC-02: the required barrier set read from the owner's template, the completion refusals, the leak-path rule, the depth
/// unit rule and the segregation of duties - the parts that need no database. The programme's rows and its closure are proven
/// in the gateway test.
/// </summary>
public sealed class WellPaTests
{
    private const string PageFileName = "WellPaPageView.cs";

    [Fact]
    public void The_required_barriers_come_from_the_owners_template()
    {
        // AC-2: the set is read from the owner's permanent-abandonment template, not written in this view.
        var required = WellPaRules.RequiredBarriers();

        Assert.NotEmpty(required);

        // The template's deep envelopes are Primary and Secondary and its surface envelope is Environmental.
        Assert.Equal(
            ["Environmental", "Primary", "Secondary"],
            required.Select(row => row.Envelope).Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.Ordinal));

        // A cement plug is required outright in every envelope; the elements the template guards with a condition are
        // marked conditional rather than required outright.
        Assert.All(
            required.Where(row => row.BarrierType == "CEMENT_PLUG"),
            row => Assert.False(row.Conditional));

        Assert.Contains(required, row => row.Conditional);

        // The page reads the same set, so the view cannot disagree with the rule.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("programme.Required", page, StringComparison.Ordinal);
        Assert.DoesNotContain("CEMENT_PLUG\", \"CASING_CEMENT", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Completion_is_refused_without_evidence_for_every_required_barrier()
    {
        // AC-1: an envelope with no verified barrier blocks, and the refusal names what the template requires in it.
        var none = Programme([]);
        var blocks = WellPaRules.CompletionBlocks(none);

        Assert.Contains(blocks, block => block.Contains("Primary envelope has no verified barrier", StringComparison.Ordinal));
        Assert.Contains(blocks, block => block.Contains("CEMENT_PLUG", StringComparison.Ordinal));
        Assert.Contains(blocks, block => block.Contains("Secondary envelope", StringComparison.Ordinal));
        Assert.Contains(blocks, block => block.Contains("Environmental envelope", StringComparison.Ordinal));
        Assert.Contains(blocks, block => block.Contains("No barrier is recorded", StringComparison.Ordinal));

        // A barrier marked executed without an evidence reference does not count, and is named as such.
        var noEvidence = Programme([Barrier("Primary", executed: true, evidence: null)]);
        var withUnevidenced = WellPaRules.CompletionBlocks(noEvidence);

        Assert.Contains(withUnevidenced,
            block => block.Contains("marked executed but records no evidence reference", StringComparison.Ordinal));
        Assert.Contains(withUnevidenced, block => block.Contains("Primary envelope", StringComparison.Ordinal));

        // Evidence for every required envelope is what clears the barrier refusals.
        var ready = Programme(
        [
            Barrier("Primary", executed: true, evidence: "CBL-2026-11"),
            Barrier("Secondary", executed: true, evidence: "PRESSURE-TEST-7"),
            Barrier("Environmental", executed: true, evidence: "PLUG-TAG-3"),
        ]);

        Assert.Empty(WellPaRules.CompletionBlocks(ready));
        Assert.True(ready.Barriers.All(barrier => barrier.IsVerifiedWithEvidence));
    }

    [Fact]
    public void An_open_leak_path_blocks_completion()
    {
        // AC-3.
        var verified = new List<OgAbandonmentBarrier>
        {
            Barrier("Primary", executed: true, evidence: "CBL-1"),
            Barrier("Secondary", executed: true, evidence: "PT-1"),
            Barrier("Environmental", executed: true, evidence: "TAG-1"),
        };

        var open = Programme(verified, [LeakPath("ADDRESSED")], openPath: "Annulus pressure at the casing shoe");

        var blocks = WellPaRules.CompletionBlocks(open);

        var block = Assert.Single(blocks);
        Assert.Contains("Annulus pressure at the casing shoe", block, StringComparison.Ordinal);
        Assert.Contains("still open", block, StringComparison.Ordinal);

        // Once it is addressed the block clears.
        var addressed = Programme(verified, [LeakPath("ADDRESSED")]);

        Assert.Empty(WellPaRules.CompletionBlocks(addressed));
    }

    [Fact]
    public void A_depth_carries_its_unit()
    {
        // AC-4: a depth without its unit is refused, and both ends are needed.
        Assert.Null(WellPaRules.ValidateBarrier("CEMENT_PLUG", 1500m, 1560m, "m"));
        Assert.Null(WellPaRules.ValidateBarrier("CEMENT_PLUG", null, null, null));

        Assert.Equal(WellPaRules.CodeDepthUnitRequired,
            WellPaRules.ValidateBarrier("CEMENT_PLUG", 1500m, 1560m, null)!.Value.Code);
        Assert.Equal(WellPaRules.CodeBarrierInvalid,
            WellPaRules.ValidateBarrier("CEMENT_PLUG", 1500m, null, "m")!.Value.Code);
        Assert.Equal(WellPaRules.CodeBarrierInvalid,
            WellPaRules.ValidateBarrier("CEMENT_PLUG", 1560m, 1500m, "m")!.Value.Code);
        Assert.Equal(WellPaRules.CodeBarrierInvalid,
            WellPaRules.ValidateBarrier(null, null, null, null)!.Value.Code);

        // The text shows the unit, because a bare interval says nothing.
        var barrier = Barrier("Primary", executed: false, evidence: null, top: 1500m, bottom: 1560m, unit: "m");

        Assert.Equal("1500 - 1560 m", barrier.DepthText);
        Assert.Equal("no recorded depth", (barrier with { TopDepth = null, BaseDepth = null }).DepthText);
        Assert.Contains("(no unit)", (barrier with { DepthUom = string.Empty }).DepthText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_planner_and_the_evidence_recorder_cannot_close()
    {
        // AC-6's segregation of duties.
        var planned = WellPaRules.CheckSeparateDuties("sara", ["omar"], "sara");

        Assert.Equal(WellPaRules.CodeSelfClosure, planned!.Value.Code);
        Assert.Contains("planned this abandonment", planned.Value.Message, StringComparison.Ordinal);

        var executed = WellPaRules.CheckSeparateDuties("sara", ["omar"], "omar");

        Assert.Equal(WellPaRules.CodeSelfClosure, executed!.Value.Code);
        Assert.Contains("recorded this abandonment's barrier evidence", executed.Value.Message, StringComparison.Ordinal);

        Assert.Null(WellPaRules.CheckSeparateDuties("sara", ["omar"], "nadia"));
    }

    [Fact]
    public void A_closed_programme_cannot_be_closed_again()
    {
        var complete = Programme([], closed: true);
        var blocks = WellPaRules.CompletionBlocks(complete);

        Assert.Contains(blocks, block => block.Contains("already completed", StringComparison.Ordinal));
        Assert.True(complete.IsComplete);
        Assert.Equal("completed", complete.StatusLabel);
    }

    [Fact]
    public void The_page_says_the_schematic_is_not_the_evidence()
    {
        // AC-5: the visual aid is declared to be a visual aid, and the evidence is the barrier's own reference.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("OgWellPa.SchematicNotice", page, StringComparison.Ordinal);
        Assert.Contains("is not the barrier evidence", OgWellPa.SchematicNotice, StringComparison.Ordinal);

        Assert.Contains("programme.ChecklistText", page, StringComparison.Ordinal);
        Assert.Contains("EvidenceRef", page, StringComparison.Ordinal);
    }

    private static OgAbandonmentBarrier Barrier(
        string envelope,
        bool executed,
        string? evidence,
        decimal? top = 1500m,
        decimal? bottom = 1560m,
        string? unit = "m") =>
        new(
            "AB-" + envelope,
            envelope,
            "CEMENT_PLUG",
            null,
            top,
            bottom,
            unit ?? string.Empty,
            "Pressure test",
            evidence,
            executed ? "omar" : null,
            executed ? new DateTime(2026, 11, 2) : null,
            executed);

    private static OgLeakPath LeakPath(string status) =>
        new("LP-1", "Annulus pressure at the casing shoe", status, "Cement squeeze", "omar",
            new DateTime(2026, 11, 3));

    private static OgWellPa Programme(
        IReadOnlyList<OgAbandonmentBarrier> barriers,
        IReadOnlyList<OgLeakPath>? leakPaths = null,
        string? openPath = null,
        bool closed = false)
    {
        var paths = leakPaths ?? [];
        var status = closed ? WellPaRules.Completed : WellPaRules.Planned;

        var programme = new OgWellPa(
            new OgHseScope("SMOKE", "F-1"),
            "WELL-1",
            status,
            WellPaRules.Label(status),
            new DateTime(2026, 12, 1),
            "sara",
            barriers,
            WellPaRules.RequiredBarriers(),
            paths,
            null,
            [],
            DateTime.UtcNow);

        // The open path is added to the programme's own list rather than assumed, so the count and the rule agree.
        if (openPath is not null)
        {
            programme = programme with
            {
                LeakPaths = [.. paths, new OgLeakPath("LP-2", openPath, WellPaRules.Open, null, null, null)],
            };
        }

        return programme with { Blocks = WellPaRules.CompletionBlocks(programme) };
    }
}
