using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LSE-02: the stage rules, the diligence gate, the money text, the rights matrix and the board columns - the
/// parts that do not need a database. The real transitions, history and retry are proven in the gateway test.
/// </summary>
public sealed class LeasePipelineTests
{
    private static readonly DateTime AsOf = new(2026, 6, 1);

    [Fact]
    public void The_pipeline_moves_one_stage_at_a_time_and_declines_from_a_live_stage()
    {
        Assert.Equal(LeasePipelineStage.Screening, LeasePipelineStage.Next(LeasePipelineStage.Opportunity));
        Assert.Equal(LeasePipelineStage.DueDiligence, LeasePipelineStage.Next(LeasePipelineStage.Screening));
        Assert.Equal(LeasePipelineStage.Executed, LeasePipelineStage.Next(LeasePipelineStage.RightsReview));
        Assert.Null(LeasePipelineStage.Next(LeasePipelineStage.Executed));
        Assert.Null(LeasePipelineStage.Next(LeasePipelineStage.Declined));

        Assert.Contains(LeasePipelineStage.Negotiation, LeasePipelineStage.LegalFrom(LeasePipelineStage.DueDiligence));
        Assert.Contains(LeasePipelineStage.Declined, LeasePipelineStage.LegalFrom(LeasePipelineStage.DueDiligence));
        Assert.Empty(LeasePipelineStage.LegalFrom(LeasePipelineStage.Executed));

        // An unclassified opportunity can enter at the first stage, or be declined.
        Assert.Equal(
            new[] { LeasePipelineStage.Opportunity, LeasePipelineStage.Declined },
            LeasePipelineStage.LegalFrom(null));
    }

    [Fact]
    public void Diligence_gates_the_move_out_of_due_diligence_and_later()
    {
        Assert.False(LeasePipelineStage.DiligenceGates(LeasePipelineStage.Opportunity));
        Assert.False(LeasePipelineStage.DiligenceGates(LeasePipelineStage.Screening));
        Assert.True(LeasePipelineStage.DiligenceGates(LeasePipelineStage.DueDiligence));
        Assert.True(LeasePipelineStage.DiligenceGates(LeasePipelineStage.Negotiation));
        Assert.True(LeasePipelineStage.DiligenceGates(LeasePipelineStage.RightsReview));
        Assert.False(LeasePipelineStage.DiligenceGates(LeasePipelineStage.Executed));
    }

    [Fact]
    public void An_open_diligence_item_is_not_complete_and_a_closed_one_is()
    {
        Assert.True(Item(Outcome: null).IsOpen);
        Assert.True(Item(Outcome: "OPEN").IsOpen);
        Assert.False(Item(Outcome: "PASS").IsOpen);
        Assert.False(Item(Outcome: "WAIVED").IsOpen);
        Assert.Equal("PASS", Item(Outcome: "PASS").OutcomeText);
    }

    [Fact]
    public void Money_and_acreage_never_show_a_bare_number()
    {
        var opportunity = Opportunity(
            price: 1000m,
            currency: null,
            acreage: 100m,
            acreageUnit: null);

        Assert.Contains("(currency not recorded)", opportunity.AskingPriceText, StringComparison.Ordinal);
        Assert.Contains("(unit not recorded)", opportunity.AcreageText, StringComparison.Ordinal);

        var complete = Opportunity(price: 1000m, currency: "USD", acreage: 100m, acreageUnit: "acres");

        Assert.Equal("1,000 USD", complete.AskingPriceText);
        Assert.Equal("100 acres", complete.AcreageText);
    }

    [Fact]
    public void The_rights_matrix_keeps_mineral_surface_and_working_apart()
    {
        var detail = new OgPipelineDetail(
            Opportunity(),
            [],
            [
                new OgLeaseRight(OgLeaseRight.Mineral, "Landowner", 1.0m, null, null, true),
                new OgLeaseRight(OgLeaseRight.Surface, "Rancher", 0.5m, null, null, true),
                new OgLeaseRight(OgLeaseRight.Working, "Operator", 0.25m, null, null, true),
            ],
            [],
            []);

        Assert.Single(detail.MineralRights);
        Assert.Single(detail.SurfaceRights);
        Assert.Single(detail.WorkingInterest);
        Assert.Equal("mineral 1 · surface 1 · working 1", detail.RightsSummary);
        Assert.Equal("100%", detail.MineralRights[0].InterestText);
    }

    [Fact]
    public void A_declined_opportunity_keeps_every_history_entry()
    {
        var detail = new OgPipelineDetail(
            Opportunity(stage: LeasePipelineStage.Declined),
            [],
            [],
            [],
            [
                new OgPipelineHistoryEntry(3, LeasePipelineStage.Declined, "DECLINED", "no access", "sara", AsOf),
                new OgPipelineHistoryEntry(2, LeasePipelineStage.Screening, "ADVANCED", "fit", "sara", AsOf),
                new OgPipelineHistoryEntry(1, LeasePipelineStage.Opportunity, "ADVANCED", "lead", "sara", AsOf),
            ]);

        Assert.Equal(3, detail.History.Count);
        Assert.Equal(LeasePipelineStage.Declined, detail.History[0].Stage);
        Assert.Equal("no access", detail.History[0].Rationale);
    }

    [Fact]
    public void The_board_has_a_column_for_every_stage_and_an_unclassified_column()
    {
        var cards = new[]
        {
            Card("OP-1", LeasePipelineStage.Opportunity),
            Card("OP-2", LeasePipelineStage.DueDiligence, openDiligence: 2),
            Card("OP-3", null),
        };

        var columns = OgLeasePipelineService.BuildColumns(cards);

        Assert.Equal(LeasePipelineStage.Order.Count + 1, columns.Count);
        Assert.True(columns[^1].IsUnclassified);
        Assert.Equal(OgLeaseOpportunity.UnclassifiedStage, columns[^1].Stage);
        Assert.Equal("OP-2", Assert.Single(columns.Single(column => column.Stage == LeasePipelineStage.DueDiligence).Cards).Id);
        Assert.Equal("2 open diligence item(s)", cards[1].DiligenceText);
    }

    [Fact]
    public async Task A_stage_change_needs_an_actor_a_target_and_a_rationale()
    {
        var pipeline = new OgLeasePipelineService(() => null, new Quiet());

        Assert.Equal(
            OgLeasePipelineService.CodeActorRequired,
            (await pipeline.AdvanceAsync(new OgPipelineDecision("OP-1", "NEGOTIATION", "why", " "))).Reference);

        Assert.Equal(
            OgLeasePipelineService.CodeStageRequired,
            (await pipeline.AdvanceAsync(new OgPipelineDecision("OP-1", null, "why", "sara"))).Reference);

        Assert.Equal(
            OgLeasePipelineService.CodeRationaleRequired,
            (await pipeline.AdvanceAsync(new OgPipelineDecision("OP-1", "NEGOTIATION", " ", "sara"))).Reference);

        Assert.Equal(
            OgLeasePipelineService.CodeNothingPending,
            (await pipeline.RetryAsync(AsOf)).Reference);
    }

    [Fact]
    public async Task A_negotiation_round_needs_a_counterparty_and_a_currency()
    {
        var pipeline = new OgLeasePipelineService(() => null, new Quiet());

        Assert.Equal(
            OgLeasePipelineService.CodeCounterpartyRequired,
            (await pipeline.RecordNegotiationAsync("OP-1", " ", 1, 100m, "USD", null, "sara")).Reference);

        Assert.Equal(
            OgLeasePipelineService.CodeCurrencyRequired,
            (await pipeline.RecordNegotiationAsync("OP-1", "LandCo", 1, 100m, " ", null, "sara")).Reference);

        Assert.Equal(
            OgLeasePipelineService.CodeActorRequired,
            (await pipeline.RecordNegotiationAsync("OP-1", "LandCo", 1, 100m, "USD", null, " ")).Reference);
    }

    private static OgPipelineCard Card(string id, string? stage, int openDiligence = 0) =>
        new(Opportunity(id: id, stage: stage), openDiligence, 0, 0);

    private static OgDiligenceItem Item(string? Outcome) =>
        new("I-1", "Title", "Is the title clear?", Outcome, "DOC-1", "sara", AsOf);

    private static OgLeaseOpportunity Opportunity(
        string id = "OP-1",
        string? stage = LeasePipelineStage.Opportunity,
        decimal? price = 1000m,
        string? currency = "USD",
        decimal? acreage = 100m,
        string? acreageUnit = "acres") =>
        new(
            id,
            "Alpha " + id,
            "NORTH",
            acreage,
            acreageUnit,
            price,
            currency,
            stage,
            "OP-A",
            AsOf,
            10);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
