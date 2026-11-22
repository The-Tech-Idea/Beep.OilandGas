using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PRD-05: the waterfall arithmetic, the materiality rule, the unit rule, the closed-month rule, the monthly-total rule
/// and the export gate - the parts that do not need a database. The record reads, the reason assignment, the close and
/// the report are proven in the gateway test.
/// </summary>
public sealed class ReconciliationTests
{
    private static readonly DateTime Month = new(2026, 10, 1);

    [Fact]
    public void The_waterfall_is_arithmetically_consistent_step_by_step()
    {
        var totals = new OgReconciliationTotals(1000m, 980m, 960m, 950m, 940m, "bbl");
        var steps = ReconciliationRules.Steps(totals);

        Assert.Equal(5, steps.Count);
        Assert.All(steps, step => Assert.True(step.Available));
        Assert.Equal(["PRODUCED", "MEASURED", "ALLOCATED", "SOLD", "BOOKED"], steps.Select(step => step.Stage));

        // AC-1: each step is the previous one plus its own difference, and the first has none.
        Assert.Null(steps[0].Difference);
        Assert.Equal(-20m, steps[1].Difference);
        Assert.Equal(-20m, steps[2].Difference);
        Assert.Equal(-10m, steps[3].Difference);
        Assert.Equal(-10m, steps[4].Difference);

        Assert.True(ReconciliationRules.IsConsistent(totals));

        for (var index = 1; index < steps.Count; index++)
        {
            Assert.Equal(steps[index - 1].Value + steps[index].Difference, steps[index].Value);
        }

        Assert.Equal("-2%", steps[1].PercentText);
        Assert.Contains("measured 980 bbl (-20 bbl)", steps[1].Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_stage_stops_the_waterfall_with_an_em_dash()
    {
        // No allocation run: the measured step exists, the allocated one does not, and the rest stop.
        var totals = new OgReconciliationTotals(1000m, 980m, null, 950m, null, "bbl");
        var steps = ReconciliationRules.Steps(totals);

        Assert.True(steps[0].Available);
        Assert.True(steps[1].Available);
        Assert.Equal("-20 bbl", steps[1].DifferenceText);

        Assert.False(steps[2].Available);
        Assert.Equal("—", steps[2].ValueText);
        Assert.Equal("—", steps[2].DifferenceText);

        // Sold is recorded but is not compared, because the waterfall stopped at the missing stage.
        Assert.False(steps[3].Available);
        Assert.Null(steps[3].Difference);
        Assert.Contains("waterfall stopped", steps[3].Note!, StringComparison.Ordinal);

        // A waterfall with one available step has nothing to compare, and is not "inconsistent".
        Assert.True(ReconciliationRules.IsConsistent(new OgReconciliationTotals(1000m, null, null, null, null, "bbl")));
    }

    [Fact]
    public void A_material_variance_is_measured_against_the_stage_before_it()
    {
        Assert.True(ReconciliationRules.IsMaterial(15m, 1000m, 1m));      // 1.5% of 1000
        Assert.False(ReconciliationRules.IsMaterial(5m, 1000m, 1m));      // 0.5%
        Assert.False(ReconciliationRules.IsMaterial(-5m, 1000m, 1m));     // the size matters, not the sign
        Assert.True(ReconciliationRules.IsMaterial(-15m, 1000m, 1m));

        Assert.False(ReconciliationRules.IsMaterial(null, 1000m, 1m));
        Assert.False(ReconciliationRules.IsMaterial(10m, null, 1m));
        Assert.False(ReconciliationRules.IsMaterial(10m, 0m, 1m));

        // A tighter materiality catches a smaller variance, and a wider one does not.
        Assert.True(ReconciliationRules.IsMaterial(5m, 1000m, 0.25m));
        Assert.False(ReconciliationRules.IsMaterial(5m, 1000m, 5m));
    }

    [Fact]
    public void A_line_without_a_reason_blocks_the_export_and_a_reasoned_one_does_not()
    {
        var unreasoned = Line(isMaterial: true, reason: null);
        var reasoned = Line(isMaterial: true, reason: "MEASUREMENT");

        Assert.True(unreasoned.NeedsReason);
        Assert.Equal("needs a reason", unreasoned.ReasonText);
        Assert.False(reasoned.NeedsReason);
        Assert.Contains("MEASUREMENT", reasoned.ReasonText, StringComparison.Ordinal);

        // A variance within tolerance needs no reason.
        Assert.False(Line(isMaterial: false, reason: null).NeedsReason);

        var blocked = Workspace([unreasoned]);

        Assert.False(blocked.ExportReady);
        Assert.Contains("no reason", blocked.ExportBlockedReason, StringComparison.Ordinal);
        Assert.Equal(
            ReconciliationRules.CodeMaterialVarianceWithoutReason,
            ReconciliationRules.ValidateExport(blocked)!.Value.Code);

        Assert.True(Workspace([reasoned]).ExportReady);
        Assert.Null(ReconciliationRules.ValidateExport(Workspace([reasoned])));
    }

    [Fact]
    public void A_comparison_across_units_is_refused()
    {
        var mixed = new OgReconciliationTotals(1000m, 980m, null, null, null, "bbl") with { };

        var steps = new[]
        {
            new OgReconciliationStep(ReconciliationRules.Produced, 1000m, "bbl", null, null, true, null),
            new OgReconciliationStep(ReconciliationRules.Measured, 980m, "m3", -20m, -0.02m, true, null),
        };

        var mismatch = ReconciliationRules.UnitMismatch(steps);

        Assert.Contains("more than one unit", mismatch, StringComparison.Ordinal);
        Assert.Contains("bbl", mismatch, StringComparison.Ordinal);
        Assert.Contains("m3", mismatch, StringComparison.Ordinal);

        Assert.Empty(ReconciliationRules.UnitMismatch(
        [
            new OgReconciliationStep(ReconciliationRules.Produced, 1m, "bbl", null, null, true, null),
            new OgReconciliationStep(ReconciliationRules.Measured, 1m, "BBL", 0m, 0m, true, null),
        ]));

        var workspace = Workspace([Line(isMaterial: false, reason: null)]) with { UnitMismatch = mismatch };

        Assert.False(workspace.ExportReady);
        Assert.Equal(mismatch, workspace.ExportBlockedReason);
        Assert.Equal(ReconciliationRules.CodeUnitMismatch, ReconciliationRules.ValidateExport(workspace)!.Value.Code);

        // The totals parameter is only there to keep the constructor shape; the steps carry the units.
        Assert.Equal("bbl", mixed.Unit);
    }

    [Fact]
    public void An_empty_month_has_nothing_to_export()
    {
        var empty = Workspace([]);

        Assert.True(empty.IsEmpty);
        Assert.Equal(ReconciliationRules.CodeNothingToExport, ReconciliationRules.ValidateExport(empty)!.Value.Code);
    }

    [Fact]
    public void A_reason_is_validated_against_the_reference_set()
    {
        var reasons = new[] { "MEASUREMENT", "SALES" };

        Assert.Equal(
            ReconciliationRules.CodePeriodRequired,
            ReconciliationRules.Validate(Values() with { Period = null }, reasons)!.Value.Code);

        Assert.Equal(
            ReconciliationRules.CodeDimensionRequired,
            ReconciliationRules.Validate(Values(dimension: " "), reasons)!.Value.Code);

        Assert.Equal(
            ReconciliationRules.CodeStageInvalid,
            ReconciliationRules.Validate(Values(stage: "GUESS"), reasons)!.Value.Code);

        Assert.Equal(
            ReconciliationRules.CodeReasonRequired,
            ReconciliationRules.Validate(Values(reason: null), reasons)!.Value.Code);

        Assert.Equal(
            ReconciliationRules.CodeReasonReferenceMissing,
            ReconciliationRules.Validate(Values(reason: "MEASUREMENT"), [])!.Value.Code);

        Assert.Equal(
            ReconciliationRules.CodeReasonUnknown,
            ReconciliationRules.Validate(Values(reason: "MADE-UP"), reasons)!.Value.Code);

        Assert.Null(ReconciliationRules.Validate(Values(reason: "measurement"), reasons));
    }

    [Fact]
    public void The_month_bounds_are_the_calendar_month()
    {
        Assert.Equal(new DateTime(2026, 10, 1), ReconciliationRules.MonthOf(new DateTime(2026, 10, 17)));
        Assert.Equal(new DateTime(2026, 10, 31), ReconciliationRules.MonthEnd(Month));
        Assert.Equal(new DateTime(2026, 2, 28), ReconciliationRules.MonthEnd(new DateTime(2026, 2, 14)));
        Assert.Equal(new DateTime(2024, 2, 29), ReconciliationRules.MonthEnd(new DateTime(2024, 2, 1)));
    }

    [Fact]
    public void The_workspace_counts_its_material_and_unreasoned_lines()
    {
        var workspace = Workspace(
        [
            Line("WELL-1", isMaterial: true, reason: "MEASUREMENT"),
            Line("WELL-2", isMaterial: true, reason: null),
            Line("WELL-3", isMaterial: false, reason: null),
        ]);

        Assert.Equal(2, workspace.MaterialCount);
        Assert.Equal(1, workspace.NeedsReasonCount);
        Assert.Contains("3 line(s)", workspace.Summary, StringComparison.Ordinal);
        Assert.Contains("2 material", workspace.Summary, StringComparison.Ordinal);
        Assert.Contains("from 2 day(s) with data", workspace.MonthlyText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_closed_state_is_its_own_value()
    {
        var closed = Workspace([Line(isMaterial: false, reason: null)]) with
        {
            State = OgReconciliationWorkspace.ClosedState,
            IsClosed = true,
            ClosedBy = "sara",
            ClosedAt = new DateTime(2026, 11, 2, 9, 30, 0),
        };

        Assert.True(closed.IsClosed);
        Assert.Contains("closed", closed.Summary, StringComparison.Ordinal);
        Assert.Equal("sara", closed.ClosedBy);
        Assert.NotEqual(OgReconciliationWorkspace.ClosedState, Workspace([]).State);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_reconciliation()
    {
        var service = new OgReconciliationService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(ReconciliationRules.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task A_reason_and_a_close_both_need_an_actor()
    {
        var service = new OgReconciliationService(() => null, new Quiet());

        Assert.Equal(
            ReconciliationRules.CodeActorRequired,
            (await service.AssignReasonAsync(Values(), " ")).Reference);

        Assert.Equal(
            ReconciliationRules.CodeActorRequired,
            (await service.ClosePeriodAsync(Month, "F-1", " ")).Reference);
    }

    private static OgReconciliationReasonValues Values(
        DateTime? period = null,
        string? dimension = "WELL-1",
        string? stage = ReconciliationRules.Measured,
        string? reason = "MEASUREMENT") =>
        new("F-1", period ?? Month, dimension, stage, reason, "note");

    private static OgReconciliationLine Line(
        string dimension = "WELL-1",
        bool isMaterial = false,
        string? reason = null) =>
        new(
            dimension,
            dimension,
            false,
            1000m,
            980m,
            960m,
            950m,
            940m,
            "bbl",
            ReconciliationRules.Steps(new OgReconciliationTotals(1000m, 980m, 960m, 950m, 940m, "bbl")),
            reason,
            reason is null ? null : "the meter was re-calibrated",
            isMaterial,
            null);

    private static OgReconciliationWorkspace Workspace(
        IReadOnlyList<OgReconciliationLine> lines) =>
        new(
            "Alpha field",
            "F-1",
            Month,
            ReconciliationRules.MonthEnd(Month),
            "2026-10",
            OgReconciliationWorkspace.OpenState,
            false,
            null,
            null,
            ReconciliationRules.DefaultMaterialityPercent,
            [
                new OgReconciliationDay(Month, 500m, 490m, 480m, 470m, 460m, "bbl"),
                new OgReconciliationDay(Month.AddDays(1), 500m, 490m, 480m, 470m, 460m, "bbl"),
            ],
            lines,
            lines.FirstOrDefault(),
            new OgReconciliationTotals(1000m, 980m, 960m, 950m, 940m, "bbl"),
            ["MEASUREMENT", "SALES"],
            false,
            string.Empty,
            "joined on field + well + date");

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
