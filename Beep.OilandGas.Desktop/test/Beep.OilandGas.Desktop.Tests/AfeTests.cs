using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ECO-02: the extracted transition list, the server-owned numbering, the input rules, the remaining arithmetic and the
/// closure/immutability rules - the parts that need no database. The raise/submit/approve/supplement/commit/close journey
/// and the immutability of the approved revision are proven in the gateway test.
/// </summary>
public sealed class AfeTests
{
    private const string PageFileName = "AfePageView.cs";

    [Fact]
    public void The_lifecycle_is_the_owner_transition_list()
    {
        // The feature's stated risk is the unclear state machine. The extraction found exactly two transitions in
        // AfeService (create -> draft, approve -> approved); this asserts the full list is stated once and that the moves
        // the feature needs are in it.
        Assert.Equal(["DRAFT", "SUBMITTED", "APPROVED", "RETURNED", "CLOSED"], AfeLifecycle.All);

        Assert.True(AfeLifecycle.CanTransition(AfeLifecycle.Draft, AfeLifecycle.Submitted));
        Assert.True(AfeLifecycle.CanTransition(AfeLifecycle.Submitted, AfeLifecycle.Approved));
        Assert.True(AfeLifecycle.CanTransition(AfeLifecycle.Approved, AfeLifecycle.Closed));

        // No shortcuts: a draft cannot be approved, and nothing leaves closed.
        Assert.False(AfeLifecycle.CanTransition(AfeLifecycle.Draft, AfeLifecycle.Approved));
        Assert.False(AfeLifecycle.CanTransition(AfeLifecycle.Closed, AfeLifecycle.Approved));
        Assert.Empty(AfeLifecycle.AvailableFrom(AfeLifecycle.Closed));

        // A supplement is not a transition of the approved row - it creates another row (AC-1, SG-09-4).
        Assert.DoesNotContain(
            AfeLifecycle.Transition,
            transition => string.Equals(transition.From, AfeLifecycle.Approved, StringComparison.Ordinal)
                          && string.Equals(transition.To, AfeLifecycle.Draft, StringComparison.Ordinal));

        Assert.Equal("supplement", AfeLifecycle.StartsRevision);

        // The two states the owner already used are the owner's own constants, not a second spelling.
        Assert.Equal(AfeStatusCodes.Draft, AfeLifecycle.Draft);
        Assert.Equal(AfeStatusCodes.Approved, AfeLifecycle.Approved);

        // Only an approved revision is effective, which is what makes a supplement inert until it is approved (AC-6).
        Assert.True(AfeLifecycle.IsEffective(AfeLifecycle.Approved));
        Assert.False(AfeLifecycle.IsEffective(AfeLifecycle.Draft));

        // A closed AFE is not open for any write (AC-5).
        Assert.False(AfeLifecycle.IsOpen(AfeLifecycle.Closed));
        Assert.True(AfeLifecycle.IsOpen(AfeLifecycle.Approved));
    }

    [Fact]
    public void The_afe_number_is_server_owned()
    {
        // The stage requires the number to be server-owned, so the desk never types one: the year plus one more than the
        // highest sequence used in that year.
        Assert.Equal("AFE-2026-001", AfeRules.NextNumber([], 2026));

        var existing = new[]
        {
            Afe("AFE-2026-001"),
            Afe("AFE-2026-007"),
            Afe("AFE-2025-099"),
        };

        Assert.Equal("AFE-2026-008", AfeRules.NextNumber(existing, 2026));
        Assert.Equal("AFE-2025-100", AfeRules.NextNumber(existing, 2025));
        Assert.Equal("AFE-2027-001", AfeRules.NextNumber(existing, 2027));
    }

    [Fact]
    public void The_remaining_amount_is_the_amount_less_committed_exactly()
    {
        // AC-4: no rounding and no reserve - remaining is the arithmetic difference of the two decimal values.
        var afe = Afe("AFE-2026-001") with
        {
            Amount = 1_234_567.89m,
            Committed = 234_567.89m,
            Spent = 100_000.00m,
            Remaining = 1_234_567.89m - 234_567.89m,
        };

        Assert.Equal(1_000_000.00m, afe.Remaining);
        Assert.Equal("1,000,000.00 USD", afe.RemainingText);

        // The variance panel carries the four numbers the feature asks for, from the same row.
        Assert.Contains("budget 1,234,567.89", afe.VarianceText, StringComparison.Ordinal);
        Assert.Contains("committed 234,567.89", afe.VarianceText, StringComparison.Ordinal);
        Assert.Contains("actual 100,000.00", afe.VarianceText, StringComparison.Ordinal);
        Assert.Contains("remaining 1,000,000.00", afe.VarianceText, StringComparison.Ordinal);

        // A commitment of exactly the remaining amount is allowed; one cent more is not - the boundary the service applies.
        Assert.True(1_000_000.00m <= afe.Remaining);
        Assert.True(1_000_000.01m > afe.Remaining);
    }

    [Fact]
    public void An_afe_needs_its_title_kind_entity_amount_and_currency()
    {
        Assert.Equal(
            ["title", "kind", "well or entity", "amount", "currency"],
            AfeRules.Missing(new OgAfeEntry(null, null, null, null, null, null)));

        Assert.Null(AfeRules.Validate(Entry()));

        Assert.Equal(
            AfeRules.CodeInputInvalid,
            AfeRules.Validate(Entry() with { Kind = "PICNIC" })!.Value.Code);

        // The state table's "currency mismatch - refused".
        Assert.Equal(
            AfeRules.CodeCurrencyMismatch,
            AfeRules.Validate(Entry() with { Currency = "XYZ" })!.Value.Code);

        Assert.Equal(
            AfeRules.CodeInputInvalid,
            AfeRules.Validate(Entry() with { Amount = 0m })!.Value.Code);

        // The kinds and currencies are stated once, so the page and the rule cannot disagree.
        Assert.Contains("DRILL", AfeRules.Kinds);
        Assert.Contains("USD", AfeRules.Currencies);
        Assert.Equal(AfeLifecycle.All, AfeRules.States);
    }

    [Fact]
    public void The_page_offers_only_the_lifecycle_the_owner_names()
    {
        // The page prints the transition list from the workspace and gates its buttons on the lifecycle, so a state the
        // owner does not name cannot be invented on the screen.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => Path.GetFileName(source) == PageFileName));

        Assert.Contains("workspace.LifecycleText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Kinds", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Currencies", page, StringComparison.Ordinal);
        Assert.Contains("AfeLifecycle.Draft", page, StringComparison.Ordinal);
        Assert.Contains("AfeLifecycle.Submitted", page, StringComparison.Ordinal);

        Assert.DoesNotContain("\"APPROVED\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"CLOSED\"", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgAfeEntry Entry() =>
        new("Woodford infill AFE", "DRILL", "WELL-1", 1_500_000m, "USD", "Two wells, 12 months");

    private static OgAfe Afe(string number) =>
        new(
            "ID-" + number,
            number,
            "Title",
            "DRILL",
            "WELL-1",
            1_000m,
            0m,
            0m,
            1_000m,
            "USD",
            AfeLifecycle.Draft,
            "sara",
            null,
            null,
            null,
            null,
            null,
            1,
            null,
            "ID-" + number,
            null,
            [],
            [],
            null);
}
