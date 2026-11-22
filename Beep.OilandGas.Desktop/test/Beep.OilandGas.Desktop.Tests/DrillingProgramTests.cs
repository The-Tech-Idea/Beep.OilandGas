using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-05: the section completeness rule, the unit rules, the total-cost arithmetic, the submission gaps and the
/// "not run" check status - the parts that do not need a database. The versioning, the author/reviewer separation and
/// the DEV-03 gate are proven in the gateway test.
/// </summary>
public sealed class DrillingProgramTests
{
    [Fact]
    public void An_incomplete_section_names_what_it_still_needs()
    {
        var partial = Section(hole: 12.25m, holeUnit: "in", top: null, bottom: null, depthUnit: null,
            days: null, cost: null);

        var missing = OgDrillProgramService.MissingItems(partial);

        Assert.Contains("top depth", missing);
        Assert.Contains("bottom depth", missing);
        Assert.Contains("depth unit", missing);
        Assert.Contains("duration", missing);
        Assert.Contains("cost", missing);
        Assert.False(partial.IsComplete);

        var complete = Section(hole: 12.25m, holeUnit: "in", top: 0m, bottom: 1500m, depthUnit: "m",
            days: 12m, cost: 1_200_000m);

        Assert.Empty(complete.MissingItems);
        Assert.True(complete.IsComplete);
    }

    [Fact]
    public void A_present_diameter_or_mud_weight_must_carry_its_unit()
    {
        var noHoleUnit = Section(hole: 12.25m, holeUnit: null, top: 0m, bottom: 1500m, depthUnit: "m", days: 5m, cost: 1m);
        var noCasingUnit = Section(hole: 12.25m, holeUnit: "in", top: 0m, bottom: 1500m, depthUnit: "m", days: 5m, cost: 1m)
            with { CasingOd = 9.625m, CasingOdUnit = null };
        var noMudUnit = Section(hole: 12.25m, holeUnit: "in", top: 0m, bottom: 1500m, depthUnit: "m", days: 5m, cost: 1m)
            with { MudWeight = 10.5m, MudWeightUnit = null };

        Assert.Contains("hole size unit", OgDrillProgramService.MissingItems(noHoleUnit));
        Assert.Contains("casing diameter unit", OgDrillProgramService.MissingItems(noCasingUnit));
        Assert.Contains("mud weight unit", OgDrillProgramService.MissingItems(noMudUnit));
    }

    [Fact]
    public void A_section_that_cannot_be_represented_is_refused()
    {
        Assert.Equal(
            OgDrillProgramService.CodeSectionRequired,
            OgDrillProgramService.ValidateSection(Values(sequence: 0), "USD")!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeUnitRequired,
            OgDrillProgramService.ValidateSection(Values(holeUnit: null), "USD")!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeIntervalInvalid,
            OgDrillProgramService.ValidateSection(Values(top: 1500m, bottom: 1500m), "USD")!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeDepthUnitRequired,
            OgDrillProgramService.ValidateSection(Values(depthUnit: null), "USD")!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeUnitRequired,
            OgDrillProgramService.ValidateSection(Values(casingOd: 9.625m, casingOdUnit: null), "USD")!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeUnitRequired,
            OgDrillProgramService.ValidateSection(Values(mudWeight: 10m, mudUnit: null), "USD")!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeCostInvalid,
            OgDrillProgramService.ValidateSection(Values(cost: -1m), "USD")!.Value.Code);

        // AC-6: a cost is in the program's currency, so a program without one cannot take a cost.
        Assert.Equal(
            OgDrillProgramService.CodeCurrencyRequired,
            OgDrillProgramService.ValidateSection(Values(cost: 100m), null)!.Value.Code);

        Assert.Null(OgDrillProgramService.ValidateSection(Values(), "USD"));
    }

    [Fact]
    public void A_program_header_needs_a_name_and_a_depth_unit()
    {
        Assert.Equal(
            OgDrillProgramService.CodeNameRequired,
            OgDrillProgramService.ValidateProgram(new OgDrillProgramValues(" ", null, null, null, null, null, null))!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeDepthUnitRequired,
            OgDrillProgramService.ValidateProgram(new OgDrillProgramValues("Alpha", null, 2500m, 2400m, null, null, "USD"))!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeDepthInvalid,
            OgDrillProgramService.ValidateProgram(new OgDrillProgramValues("Alpha", null, 2000m, 2400m, "m", null, "USD"))!.Value.Code);

        Assert.Equal(
            OgDrillProgramService.CodeDurationInvalid,
            OgDrillProgramService.ValidateProgram(new OgDrillProgramValues("Alpha", null, null, null, null, -1m, "USD"))!.Value.Code);

        Assert.Null(OgDrillProgramService.ValidateProgram(
            new OgDrillProgramValues("Alpha", "Drain the sand", 2500m, 2400m, "m", 30m, "USD")));
    }

    [Fact]
    public void The_total_is_the_sum_of_the_section_costs()
    {
        var program = Program(
            sections:
            [
                Section(1, cost: 1_200_000m),
                Section(2, cost: 800_000m),
            ]);

        Assert.Equal(2_000_000m, program.TotalCost);
        Assert.True(program.TotalIsComplete);
        Assert.Equal("2,000,000 USD", program.TotalCostText);

        var partial = Program(sections: [Section(1, cost: 100m), Section(2, cost: null)]);

        Assert.Equal(100m, partial.TotalCost);
        Assert.False(partial.TotalIsComplete);
        Assert.Contains("partial", partial.TotalCostText, StringComparison.Ordinal);
        Assert.Contains("no cost recorded", partial.TotalCostText, StringComparison.Ordinal);
    }

    [Fact]
    public void Submission_gaps_name_the_incomplete_section_and_the_missing_cost()
    {
        var noSections = Program(sections: []);

        Assert.Contains("no hole section is recorded", Assert.Single(noSections.SubmissionGaps));
        Assert.False(noSections.CanSubmit);

        var incomplete = Program(
            sections:
            [
                Section(1, cost: 100m),
                Section(2, top: null, bottom: null, depthUnit: null, cost: null),
            ]);

        var gaps = incomplete.SubmissionGaps;

        Assert.Contains(gaps, gap => gap.StartsWith("section 2:", StringComparison.Ordinal));
        Assert.Contains(gaps, gap => gap.StartsWith("section 2:", StringComparison.Ordinal) && gap.Contains("top depth", StringComparison.Ordinal));

        var complete = Program(sections: [Section(1, cost: 100m)]);

        Assert.Empty(complete.SubmissionGaps);
        Assert.True(complete.CanSubmit);
    }

    [Fact]
    public void A_check_that_was_not_run_reads_as_not_run()
    {
        var notRun = new OgDrillCheck("HYDRAULICS", OgDrillCheck.NotRun, null, null, null, null);

        Assert.False(notRun.IsRun);
        Assert.False(notRun.IsPassed);
        Assert.Equal("not run", notRun.StatusText);
        Assert.Contains("not run", notRun.Text, StringComparison.Ordinal);

        var passed = new OgDrillCheck("HYDRAULICS", OgDrillCheck.Passed, "ECD 1.42", "ok", "sara", DateTime.Today);

        Assert.True(passed.IsRun);
        Assert.True(passed.IsPassed);
        Assert.Equal("pass", passed.StatusText);
        Assert.Contains("ECD 1.42", passed.Text, StringComparison.Ordinal);

        Assert.True(new OgDrillCheck("CASING_DESIGN", OgDrillCheck.Failed, null, null, null, null).IsFailed);
        Assert.True(Program(sections: [], checks: [new OgDrillCheck("CASING_DESIGN", OgDrillCheck.Failed, null, null, null, null)]).AnyCheckFailed);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_program_list()
    {
        var service = new OgDrillProgramService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(OgDrillProgramService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task A_program_and_its_checks_need_an_actor_and_a_known_check()
    {
        var service = new OgDrillProgramService(() => null, new Quiet());

        Assert.Equal(
            OgDrillProgramService.CodeActorRequired,
            (await service.CreateAsync("C-1", "D-1", new OgDrillProgramValues("Alpha", null, null, null, null, null, null), " ")).Reference);

        Assert.Equal(
            OgDrillProgramService.CodeActorRequired,
            (await service.SubmitAsync("P-1", " ")).Reference);

        Assert.Equal(
            OgDrillProgramService.CodeChecksRequired,
            (await service.RecordCheckAsync("P-1", "SOMETHING", OgDrillCheck.NotRun, null, null, "sara")).Reference);

        Assert.Equal(
            OgDrillProgramService.CodeCheckStatusInvalid,
            (await service.RecordCheckAsync("P-1", "HYDRAULICS", "MAYBE", null, null, "sara")).Reference);
    }

    [Fact]
    public async Task A_review_needs_a_reviewer_a_known_decision_and_comments_for_a_return()
    {
        var service = new OgDrillProgramService(() => null, new Quiet());

        Assert.Equal(
            OgDrillProgramService.CodeActorRequired,
            (await service.ReviewAsync("P-1", new OgDrillDecision(OgDrillDecision.Approve, null, " "))).Reference);

        Assert.Equal(
            OgDrillProgramService.CodeDecisionRequired,
            (await service.ReviewAsync("P-1", new OgDrillDecision("BOGUS", null, "sara"))).Reference);

        Assert.Equal(
            OgDrillProgramService.CodeCommentsRequired,
            (await service.ReviewAsync("P-1", new OgDrillDecision(OgDrillDecision.Return, " ", "sara"))).Reference);

        Assert.Equal(
            OgDrillProgramService.CodeCommentsRequired,
            (await service.ReviewAsync("P-1", new OgDrillDecision(OgDrillDecision.ApproveWithComments, "  ", "sara"))).Reference);

        Assert.Equal(
            OgDrillProgramService.CodeNothingPending,
            (await service.RetrySubmitAsync()).Reference);
    }

    private static OgDrillSection Section(
        int sequence = 1,
        decimal? hole = 12.25m,
        string? holeUnit = "in",
        decimal? top = 0m,
        decimal? bottom = 1500m,
        string? depthUnit = "m",
        decimal? days = 10m,
        decimal? cost = 100m) =>
        new("S-" + sequence, sequence, "Section " + sequence, hole, holeUnit, top, bottom, depthUnit,
            null, null, null, null, days, cost);

    private static OgDrillSectionValues Values(
        int sequence = 1,
        decimal? hole = 12.25m,
        string? holeUnit = "in",
        decimal? top = 0m,
        decimal? bottom = 1500m,
        string? depthUnit = "m",
        decimal? casingOd = null,
        string? casingOdUnit = null,
        decimal? mudWeight = null,
        string? mudUnit = null,
        decimal? cost = 100m) =>
        new(sequence, "Section", hole, holeUnit, top, bottom, depthUnit, casingOd, casingOdUnit,
            mudWeight, mudUnit, 10m, cost);

    private static OgDrillProgram Program(
        IReadOnlyList<OgDrillSection> sections,
        IReadOnlyList<OgDrillCheck>? checks = null,
        IReadOnlyList<string>? designMissing = null) =>
        new(
            "P-1",
            "P-1",
            "WELL-1",
            "F-1",
            "B-1",
            1,
            OgDrillProgram.Draft,
            "Alpha program",
            "Drain the sand",
            2500m,
            2400m,
            "m",
            30m,
            "USD",
            "sara",
            DateTime.Today,
            sections,
            checks ?? [],
            [],
            designMissing ?? [],
            true);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
