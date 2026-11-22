using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WRK-02: the transitions as the owner's graph reports them, the cost as the sum of the lines, the permit rule, the completion
/// evidence rule, the submitter/approver split and the available-stock arithmetic - the parts that need no database. The writes
/// and the read over real rows are proven in the gateway test.
/// </summary>
public sealed class WorkOrderDetailTests
{
    private const string PageFileName = "WorkOrderDetailPageView.cs";

    [Fact]
    public void Only_the_owners_graph_offers_a_transition()
    {
        // AC-1: the list is the owner's own WorkOrderWorkflow, not a state machine written here.
        Assert.Contains(WorkOrderState.Planned, WorkOrderDetailRules.Transitions(WorkOrderState.Draft, WorkOrderSubType.Corrective));
        Assert.Contains(WorkOrderState.Planned, WorkOrderDetailRules.Transitions(WorkOrderState.Scoped, WorkOrderSubType.Corrective));
        Assert.Contains(WorkOrderState.InProgress, WorkOrderDetailRules.Transitions(WorkOrderState.Planned, WorkOrderSubType.Corrective));
        Assert.Contains(WorkOrderState.UnderReview, WorkOrderDetailRules.Transitions(WorkOrderState.InProgress, WorkOrderSubType.Safety));

        // A skipped transition, a terminal state and an unknown subtype offer nothing.
        Assert.DoesNotContain(WorkOrderState.Completed, WorkOrderDetailRules.Transitions(WorkOrderState.Draft, WorkOrderSubType.Corrective));
        Assert.Empty(WorkOrderDetailRules.Transitions(WorkOrderState.Completed, WorkOrderSubType.Corrective));
        Assert.Empty(WorkOrderDetailRules.Transitions(WorkOrderState.Draft, "UNKNOWN"));
    }

    [Fact]
    public void The_actual_cost_is_the_sum_of_the_labour_and_material_lines()
    {
        // AC-2: cost is quantity x the recorded rate, and an uncosted line is counted rather than costed at zero.
        var labour = new List<OgLabourLine>
        {
            Line("omar", 2m, 50m, "USD"),
            Line("nadia", 1.5m, 40m, "USD"),
            Line("sara", 1m, null, "USD"),
        };

        var materials = new List<OgMaterialLine>
        {
            Material(3m, 12.50m, "USD"),
            Material(1m, null, "USD"),
        };

        var usd = Assert.Single(WorkOrderDetailRules.CostsOf(labour, materials));

        Assert.Equal(160m, usd.Labour);
        Assert.Equal(37.50m, usd.Materials);
        Assert.Equal(197.50m, usd.Total);
        Assert.False(usd.Complete);
        Assert.Equal(2, usd.UncostedLines);
        Assert.Contains("197.50 USD", usd.Describe(), StringComparison.Ordinal);
        Assert.Contains("partial", usd.Describe(), StringComparison.Ordinal);

        // Two currencies are never added together.
        var mixed = WorkOrderDetailRules.CostsOf(
            [Line("omar", 1m, 10m, "USD"), Line("omar", 1m, 10m, "NOK")],
            []);

        Assert.Equal(2, mixed.Count);
        Assert.Equal(10m, mixed.Single(cost => cost.Currency == "NOK").Total);
    }

    [Fact]
    public void Execution_needs_a_permit_that_covers_now()
    {
        // AC-3.
        var now = new DateTime(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);

        Assert.False(WorkOrderDetailRules.PermitOf(null, null, null, now).IsValid);

        var pending = WorkOrderDetailRules.PermitOf("PENDING", now.AddHours(-1), now.AddHours(1), now);

        Assert.False(pending.IsValid);
        Assert.Contains("pending", pending.Sentence, StringComparison.Ordinal);

        var expiredWindow = WorkOrderDetailRules.PermitOf("ISSUED", now.AddDays(-2), now.AddDays(-1), now);

        Assert.False(expiredWindow.IsValid);
        Assert.Contains("does not cover now", expiredWindow.Sentence, StringComparison.Ordinal);

        var noWindow = WorkOrderDetailRules.PermitOf("ISSUED", null, null, now);

        Assert.False(noWindow.IsValid);

        var valid = WorkOrderDetailRules.PermitOf("ISSUED", now.AddHours(-1), now.AddHours(1), now);

        Assert.True(valid.IsValid);
        Assert.Contains("valid until", valid.Sentence, StringComparison.Ordinal);
    }

    [Fact]
    public void Completion_needs_recorded_evidence()
    {
        // AC-4: the owner's condition table has no status column, so a recorded close-out is what counts.
        Assert.False(WorkOrderDetailRules.EvidenceOf([]).IsRecorded);

        var closeOut = new WORK_ORDER_CONDITION
        {
            WORK_ORDER_ID = "WO-1",
            CONDITION_OBS_NO = 1m,
            CONDITION_TYPE = ConditionType.CloseOut,
            BUSINESS_ASSOCIATE_ID = "nadia",
            REMARK = "Completion inspection passed.",
            EFFECTIVE_DATE = new DateTime(2026, 11, 7),
            ACTIVE_IND = "Y",
        };

        Assert.True(WorkOrderDetailRules.EvidenceOf([closeOut]).IsRecorded);

        // A close-out without its inspector, its date or its remark is not evidence.
        Assert.False(WorkOrderDetailRules.EvidenceOf([Without(closeOut, remark: " ")]).IsRecorded);
        Assert.False(WorkOrderDetailRules.EvidenceOf([Without(closeOut, inspector: null)]).IsRecorded);
        Assert.False(WorkOrderDetailRules.EvidenceOf([Without(closeOut, dated: false)]).IsRecorded);
        Assert.False(WorkOrderDetailRules.EvidenceOf([Without(closeOut, type: ConditionType.PreStart)]).IsRecorded);
    }

    [Fact]
    public void The_submitter_cannot_approve_or_complete_their_own_order()
    {
        // AC-5.
        var refused = WorkOrderDetailRules.CheckApprover("sara", "sara", WorkOrderState.Completed);

        Assert.Equal(WorkOrderDetailRules.CodeSelfApproval, refused!.Value.Code);
        Assert.Contains("cannot approve or complete", refused.Value.Message, StringComparison.Ordinal);

        Assert.Equal(WorkOrderDetailRules.CodeSelfApproval,
            WorkOrderDetailRules.CheckApprover("sara", "sara", WorkOrderState.UnderReview)!.Value.Code);

        Assert.Null(WorkOrderDetailRules.CheckApprover("sara", "nadia", WorkOrderState.Completed));

        // Starting or planning your own order is not an approval, so it is not blocked.
        Assert.Null(WorkOrderDetailRules.CheckApprover("sara", "sara", WorkOrderState.InProgress));
    }

    [Fact]
    public void A_reservation_cannot_exceed_the_available_stock()
    {
        // SG-13-3: availability is what is on hand less what is already allocated.
        Assert.Equal(6m, WorkOrderDetailRules.Available(10m, [2m, 2m]));
        Assert.Equal(10m, WorkOrderDetailRules.Available(10m, []));

        // More allocated than on hand is a negative availability, not a fresh ten.
        Assert.Equal(-1m, WorkOrderDetailRules.Available(9m, [10m]));
    }

    [Fact]
    public void The_page_shows_the_gates_the_cost_and_the_lines()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("detail.Permit.Sentence", page, StringComparison.Ordinal);
        Assert.Contains("detail.Evidence.Sentence", page, StringComparison.Ordinal);
        Assert.Contains("detail.CostText", page, StringComparison.Ordinal);
        Assert.Contains("detail.LabourText", page, StringComparison.Ordinal);
        Assert.Contains("detail.MaterialText", page, StringComparison.Ordinal);
        Assert.Contains("detail.AvailableTransitions", page, StringComparison.Ordinal);

        // A failed read clears every figure rather than showing an empty order.
        Assert.Contains("_cost.Text = string.Empty", page, StringComparison.Ordinal);
    }

    private static WORK_ORDER_CONDITION Without(
        WORK_ORDER_CONDITION condition,
        string? remark = null,
        string? inspector = "keep",
        bool dated = true,
        string? type = null) =>
        new()
        {
            WORK_ORDER_ID = condition.WORK_ORDER_ID,
            CONDITION_OBS_NO = condition.CONDITION_OBS_NO,
            CONDITION_TYPE = type ?? condition.CONDITION_TYPE,
            BUSINESS_ASSOCIATE_ID = inspector == "keep" ? condition.BUSINESS_ASSOCIATE_ID : inspector,
            REMARK = remark ?? condition.REMARK,
            EFFECTIVE_DATE = dated ? condition.EFFECTIVE_DATE : null,
            ACTIVE_IND = "Y",
        };

    private static OgLabourLine Line(string person, decimal quantity, decimal? rate, string currency) =>
        new(
            "L-" + person + quantity,
            person,
            "GENERAL",
            new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 11, 1, 16, 0, 0, DateTimeKind.Utc),
            quantity,
            "HR",
            "sara",
            rate,
            currency);

    private static OgMaterialLine Material(decimal quantity, decimal? unitCost, string? currency) =>
        new(
            "M-1",
            "ITEM-1",
            "Seal kit",
            quantity,
            "EA",
            currency ?? string.Empty,
            unitCost,
            new DateTime(2026, 11, 2, 9, 0, 0, DateTimeKind.Utc),
            null);
}
