using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WRK-04: the readiness gate, the expiry rule, the requester/issuer split, the validity window, the evidence references and
/// the intent key that prevents a second permit - the parts that need no database. The register and the writes are proven in
/// the gateway test.
/// </summary>
public sealed class PermitToWorkTests
{
    private const string PageFileName = "PermitToWorkPageView.cs";

    private static readonly DateTime AsOf = new(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Execution_start_is_refused_until_every_required_item_is_satisfied()
    {
        // AC-1: the missing items are listed, so the refusal is actionable.
        var blocked = Permit(
            state: PermitToWorkRules.Issued,
            items: [Item("PERSONNEL_COMPETENCY", satisfied: false, why: "omar holds no licence"),
                    Item("GAS_TEST", satisfied: true)]);

        var blocks = PermitToWorkRules.StartBlocks(blocked);

        Assert.Contains(blocks, block => block.Contains("PERSONNEL_COMPETENCY", StringComparison.Ordinal));
        Assert.Contains(blocks, block => block.Contains("omar holds no licence", StringComparison.Ordinal));
        Assert.False(blocked.StartAllowed);
        Assert.Contains("may not start", blocked.StartText, StringComparison.Ordinal);
        Assert.Contains("✗ PERSONNEL_COMPETENCY", blocked.BlocksText, StringComparison.Ordinal);

        // With every required item satisfied and the window covering now, the start is allowed.
        var ready = Permit(
            state: PermitToWorkRules.Issued,
            items: [Item("PERSONNEL_COMPETENCY", satisfied: true), Item("ISOLATION", satisfied: true)]);

        Assert.Empty(ready.Blocks);
        Assert.True(ready.StartAllowed);
        Assert.Contains("may start", ready.StartText, StringComparison.Ordinal);
    }

    [Fact]
    public void An_expired_licence_or_a_closed_window_leaves_the_item_unsatisfied()
    {
        // AC-2: the expiry is named, and nothing is treated as current without one.
        var expired = PermitToWorkRules.LicenceState("Y", "GRANTED", new DateTime(2026, 10, 1), AsOf);

        Assert.False(expired.IsCurrent);
        Assert.Contains("expired on 2026-10-01", expired.Why, StringComparison.Ordinal);

        Assert.False(PermitToWorkRules.LicenceState("Y", "GRANTED", null, AsOf).IsCurrent);
        Assert.False(PermitToWorkRules.LicenceState("Y", "SUSPENDED", new DateTime(2027, 1, 1), AsOf).IsCurrent);
        Assert.False(PermitToWorkRules.LicenceState("N", "GRANTED", new DateTime(2027, 1, 1), AsOf).IsCurrent);
        Assert.False(PermitToWorkRules.LicenceState("Y", null, new DateTime(2027, 1, 1), AsOf).IsCurrent);

        Assert.True(PermitToWorkRules.LicenceState("Y", "GRANTED", new DateTime(2027, 1, 1), AsOf).IsCurrent);

        // A permit outside its window is not usable, whichever end has passed.
        var closed = Permit(state: PermitToWorkRules.Issued,
            from: AsOf.AddHours(-8), to: AsOf.AddHours(-1));

        Assert.False(closed.IsWithinWindow);
        Assert.False(closed.StartAllowed);
        Assert.Contains("outside its window", closed.StateText, StringComparison.Ordinal);
        Assert.Contains("does not cover this moment", closed.StartText, StringComparison.Ordinal);

        // A permit that was never issued is refused however wide its window.
        Assert.False(Permit(state: PermitToWorkRules.Requested).StartAllowed);
    }

    [Fact]
    public void Only_a_different_person_may_issue_a_permit()
    {
        // AC-3.
        var self = PermitToWorkRules.CheckIssuer("sara", "sara");

        Assert.Equal(PermitToWorkRules.CodeSelfIssue, self!.Value.Code);
        Assert.Contains("cannot issue it", self.Value.Message, StringComparison.Ordinal);

        Assert.Null(PermitToWorkRules.CheckIssuer("sara", "nadia"));
        Assert.Null(PermitToWorkRules.CheckIssuer(null, "nadia"));
    }

    [Fact]
    public void A_validity_window_needs_both_ends_and_an_end_after_its_start()
    {
        // AC-4: nothing is defaulted to a day or an open-ended window.
        Assert.Null(PermitToWorkRules.ValidateWindow(AsOf, AsOf.AddHours(8)));

        Assert.Equal(PermitToWorkRules.CodeWindowInvalid, PermitToWorkRules.ValidateWindow(null, AsOf)!.Value.Code);
        Assert.Equal(PermitToWorkRules.CodeWindowInvalid, PermitToWorkRules.ValidateWindow(AsOf, null)!.Value.Code);

        var reversed = PermitToWorkRules.ValidateWindow(AsOf.AddHours(8), AsOf);

        Assert.Equal(PermitToWorkRules.CodeWindowInvalid, reversed!.Value.Code);
        Assert.Contains("must be after its start", reversed.Value.Message, StringComparison.Ordinal);

        // The same window measured at two moments: inside one, outside the other.
        Assert.True(PermitToWorkRules.Covers(AsOf.AddHours(-1), AsOf.AddHours(1), AsOf));
        Assert.False(PermitToWorkRules.Covers(AsOf.AddHours(1), AsOf.AddHours(2), AsOf));
    }

    [Fact]
    public void Every_readiness_item_carries_its_evidence_reference()
    {
        // AC-5.
        var satisfied = Item("PERSONNEL_COMPETENCY", satisfied: true, evidence: "LIC-1");
        var unsatisfied = Item("ISOLATION", satisfied: false, why: "no result is recorded yet", evidence: null);

        Assert.Contains("evidence LIC-1", satisfied.Describe(), StringComparison.Ordinal);
        Assert.Contains("no evidence reference", unsatisfied.Describe(), StringComparison.Ordinal);
        Assert.Contains("not satisfied: no result is recorded yet", unsatisfied.Describe(), StringComparison.Ordinal);

        var permit = Permit(PermitToWorkRules.Requested, items: [satisfied, unsatisfied]);

        Assert.Contains("evidence LIC-1", permit.ReadinessText, StringComparison.Ordinal);
        Assert.Contains("ISOLATION · not satisfied", permit.ReadinessText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_intent_key_is_what_a_retried_request_lands_on()
    {
        // AC-6: the permit id is derived from the intent, so a retry cannot write a second permit for the same request.
        Assert.Equal("PTW-permit-abc", PermitToWorkRules.IdFor("permit-abc"));
        Assert.Equal("PTW-permit-abc", PermitToWorkRules.IdFor("  permit-abc  "));
        Assert.Equal(
            PermitToWorkRules.IdFor("permit-abc"),
            PermitToWorkRules.IdFor("permit-abc"));
    }

    [Fact]
    public void The_page_shows_the_readiness_the_blocks_and_the_start_answer()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("permit.ReadinessText", page, StringComparison.Ordinal);
        Assert.Contains("permit.BlocksText", page, StringComparison.Ordinal);
        Assert.Contains("permit.StartText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Contractors", page, StringComparison.Ordinal);

        // A failed read clears the readiness rather than showing an empty checklist.
        Assert.Contains("_readiness.Text = string.Empty", page, StringComparison.Ordinal);
    }

    private static OgReadinessItem Item(
        string kind,
        bool satisfied,
        string why = "",
        string? evidence = "EV-1") =>
        new(kind, Required: true, satisfied, evidence, satisfied ? string.Empty : why);

    private static OgWorkPermit Permit(
        string state,
        DateTime? from = null,
        DateTime? to = null,
        IReadOnlyList<OgReadinessItem>? items = null)
    {
        var permit = new OgWorkPermit(
            "PTW-1",
            "WO-1",
            state,
            "Replace the wellhead seal",
            "Pressure",
            "ISO-1",
            "PER-1",
            "MAT-1",
            "sara",
            null,
            from ?? AsOf.AddHours(-1),
            to ?? AsOf.AddHours(8),
            null,
            items ?? [],
            [],
            AsOf);

        return permit with { Blocks = PermitToWorkRules.StartBlocks(permit) };
    }
}
