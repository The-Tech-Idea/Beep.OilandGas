using Beep.OilandGas.Accounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-06: availability as an exact subtraction, the issue ceiling, the reservation holder rule, the restock decision rule,
/// the negative-stock refusals and the movement record's own state - the parts that need no database. The receipt, issue,
/// return, adjustment, revaluation and link journey is proven in the gateway test.
/// </summary>
public sealed class WarehouseTests
{
    private const string PageFileName = "WarehousePageView.cs";

    [Fact]
    public void Availability_is_on_hand_less_reserved_exactly()
    {
        // AC-1, at the record: no rounding and no allowance.
        var item = Item(onHand: 1_234.567m, reserved: 234.567m);

        Assert.Equal(1_000.000m, item.Available);
        Assert.Equal("1,000 ea", item.AvailabilityText);

        // A reservation that takes everything leaves nothing available - and does not go negative on its own.
        Assert.Equal(0m, (item with { Reserved = 1_234.567m }).Available);

        // The shape of the number does not change the subtraction.
        Assert.Equal(0.01m, (Item(onHand: 1.01m, reserved: 1.00m)).Available);
    }

    [Fact]
    public void An_issue_above_availability_names_the_shortfall()
    {
        // AC-2: the refusal is the short quantity, not "invalid".
        var item = Item(onHand: 100m, reserved: 40m);

        var refused = WarehouseRules.CheckIssue(item, 70m);

        Assert.Equal(WarehouseRules.CodeInsufficientAvailability, refused!.Value.Code);
        Assert.Contains("Cannot issue 70 ea of CAS-1", refused.Value.Message, StringComparison.Ordinal);
        Assert.Contains("60 ea is available", refused.Value.Message, StringComparison.Ordinal);
        Assert.Contains("a shortfall of 10", refused.Value.Message, StringComparison.Ordinal);

        // It also says what is on hand and what is held, so the issuer can see where the stock went.
        Assert.Contains("100 is on hand and 40 is reserved", refused.Value.Message, StringComparison.Ordinal);

        // Exactly the available quantity is allowed.
        Assert.Null(WarehouseRules.CheckIssue(item, 60m));

        // And a non-positive issue is a different refusal.
        Assert.Equal(WarehouseRules.CodeInputInvalid, WarehouseRules.CheckIssue(item, 0m)!.Value.Code);
        Assert.Equal(WarehouseRules.CodeInputInvalid, WarehouseRules.CheckIssue(item, -1m)!.Value.Code);
    }

    [Fact]
    public void A_return_must_state_its_restock_decision()
    {
        // AC-4: worse condition or not, the decision is what moves the quantity and the value.
        var damaged = WarehouseRules.CheckReturn(WarehouseCodes.ConditionDamaged, null);

        Assert.Equal(WarehouseRules.CodeRestockDecisionRequired, damaged!.Value.Code);
        Assert.Contains("DAMAGED", damaged.Value.Message, StringComparison.Ordinal);

        Assert.Equal(
            WarehouseRules.CodeRestockDecisionRequired,
            WarehouseRules.CheckReturn(WarehouseCodes.ConditionNew, null)!.Value.Code);

        // With a decision it passes, whatever the condition.
        Assert.Null(WarehouseRules.CheckReturn(WarehouseCodes.ConditionDamaged, WarehouseCodes.RestockScrap));
        Assert.Null(WarehouseRules.CheckReturn(WarehouseCodes.ConditionServiceable, WarehouseCodes.RestockRestock));

        // An unknown condition is not a condition.
        Assert.Equal(
            WarehouseRules.CodeInputRequired,
            WarehouseRules.CheckReturn("SOGGY", WarehouseCodes.RestockRestock)!.Value.Code);

        Assert.Contains(WarehouseCodes.ConditionScrap, WarehouseRules.Conditions);
        Assert.Contains(WarehouseCodes.RestockRepair, WarehouseRules.RestockDecisions);
    }

    [Fact]
    public void A_count_below_zero_is_refused()
    {
        // AC-6 at the rule.
        var item = Item(onHand: 10m, reserved: 0m);

        var negative = WarehouseRules.CheckCount(item, -1m);

        Assert.Equal(WarehouseRules.CodeNegativeStock, negative!.Value.Code);
        Assert.Contains("never goes below zero", negative.Value.Message, StringComparison.Ordinal);

        Assert.Null(WarehouseRules.CheckCount(item, 0m));
        Assert.Null(WarehouseRules.CheckCount(item, 10m));
    }

    [Fact]
    public void A_movement_reports_the_sign_and_the_journal_it_produced()
    {
        // AC-5 at the record: a value change is either linked to its entry or said to be unlinked.
        var issue = Movement(WarehouseCodes.KindIssue, quantity: -5m, valueChange: -50m, journalId: "J1");

        Assert.True(issue.MovedStock);
        Assert.True(issue.MovedValue);
        Assert.Contains("journal J1", issue.JournalText, StringComparison.Ordinal);
        Assert.Contains("ISSUE", issue.Describe(), StringComparison.Ordinal);

        // A movement that changed no value says so instead of leaving a zero to be read as a missing journal.
        var noValue = Movement(WarehouseCodes.KindReturn, quantity: 0m, valueChange: 0m, journalId: null);

        Assert.False(noValue.MovedStock);
        Assert.False(noValue.MovedValue);
        Assert.Contains("no value change", noValue.JournalText, StringComparison.Ordinal);

        // A value change with no journal is called out rather than passing as fine.
        var unlinked = Movement(WarehouseCodes.KindAdjustment, quantity: 2m, valueChange: 20m, journalId: null);

        Assert.Contains("UNLINKED", unlinked.JournalText, StringComparison.Ordinal);

        // The owner's own sign convention: a deduction carries a negative quantity.
        Assert.Equal(-5m, WarehouseRules.SignedQuantity(WarehouseCodes.KindIssue, 5m));
        Assert.Equal(5m, WarehouseRules.SignedQuantity(WarehouseCodes.KindReceipt, 5m));
        Assert.Equal(5m, WarehouseRules.SignedQuantity(WarehouseCodes.KindReturn, 5m));
    }

    [Fact]
    public void An_item_says_who_holds_its_stock_and_what_it_is_linked_to()
    {
        // AC-3 at the record: the grid prints the holding document, not a bare number.
        var held = Item(onHand: 100m, reserved: 40m) with
        {
            Holds =
            [
                new OgReservation("R1", "I1", "CAS-1", 40m, "WO-2026-014", "WORK_ORDER",
                    new DateTime(2026, 5, 1), "sara", WarehouseCodes.ReservationOpen, null, null, null, null),
            ],
            LinkedAssetId = "PUMP-7",
            LinkedAssetType = "EQUIPMENT",
        };

        Assert.Contains("held by WO-2026-014 (40 ea)", held.HeldText, StringComparison.Ordinal);
        Assert.Contains("linked to EQUIPMENT PUMP-7", held.LinkText, StringComparison.Ordinal);

        Assert.Equal("nothing reserved", (held with { Holds = [] }).HeldText);
        Assert.Equal("not linked", (held with { LinkedAssetId = null }).LinkText);

        // Only an OPEN reservation holds stock.
        var released = held.Holds[0] with { State = WarehouseCodes.ReservationReleased };

        Assert.False(released.HoldsStock);
        Assert.True(held.Holds[0].HoldsStock);
        Assert.Equal("released", released.State.ToLowerInvariant());
    }

    [Fact]
    public void The_workspace_lists_only_the_open_holds()
    {
        var open = new OgReservation("R1", "I1", "CAS-1", 40m, "WO-1", "WORK_ORDER", null, "sara",
            WarehouseCodes.ReservationOpen, null, null, null, null);
        var spent = new OgReservation("R2", "I1", "CAS-1", 10m, "WO-2", "WORK_ORDER", null, "sara",
            WarehouseCodes.ReservationIssued, "T1", null, null, null);

        var workspace = new OgWarehouseWorkspace(
            [], null, [], [open, spent],
            WarehouseRules.Conditions, WarehouseRules.RestockDecisions, WarehouseRules.Kinds,
            "owner", false, null);

        Assert.Contains("WO-1", workspace.ReservationsText, StringComparison.Ordinal);
        Assert.DoesNotContain("WO-2", workspace.ReservationsText, StringComparison.Ordinal);
        Assert.Contains("1 open reservation", workspace.HistoryStateText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_binds_the_workspace_and_names_no_condition_literal()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.OwnerText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Conditions", page, StringComparison.Ordinal);
        Assert.Contains("workspace.RestockDecisions", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Items", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Movements", page, StringComparison.Ordinal);
        Assert.Contains("item.AvailabilityText", page, StringComparison.Ordinal);
        Assert.Contains("item.HeldText", page, StringComparison.Ordinal);

        // The vocabulary lives in the owner's codes, not in the view.
        Assert.DoesNotContain("\"DAMAGED\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"RESTOCK\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"RECEIPT\"", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgStockItem Item(decimal onHand, decimal reserved) =>
        new(
            "I1",
            "CAS-1",
            "Casing 9-5/8",
            "ea",
            onHand,
            reserved,
            10m,
            onHand * 10m,
            "USD",
            "WH-1",
            "BIN-A",
            "FIFO",
            null,
            null,
            []);

    private static OgStockMovement Movement(string kind, decimal quantity, decimal valueChange, string? journalId) =>
        new(
            "T1",
            kind,
            "I1",
            "CAS-1",
            quantity,
            10m,
            valueChange,
            "WO-1",
            new DateTime(2026, 5, 2),
            "sara",
            null,
            null,
            null,
            journalId,
            "test");
}
