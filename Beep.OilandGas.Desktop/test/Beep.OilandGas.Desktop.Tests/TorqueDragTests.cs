using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdea.OilGas.Drilling;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-07: the scenario mapping, the per-interval friction, the component limit, the limiting-factor rule, the unit refusal in the
/// parser and the refusals before a run - the parts that need no database. The reference hook load is proven in the gateway test.
/// </summary>
public sealed class TorqueDragTests
{
    private const string PageFileName = "TorqueDragPageView.cs";

    [Fact]
    public void The_four_scenarios_map_to_the_owners_operations()
    {
        Assert.Equal(StringOperation.TripOut, TorqueDragRules.OperationOf("PICK_UP"));
        Assert.Equal(StringOperation.TripIn, TorqueDragRules.OperationOf(" slack_off "));
        Assert.Equal(StringOperation.RotatingOffBottom, TorqueDragRules.OperationOf("ROTATE_OFF"));
        Assert.Equal(StringOperation.Drilling, TorqueDragRules.OperationOf("drilling"));

        Assert.Null(TorqueDragRules.OperationOf("REAMING"));
        Assert.Null(TorqueDragRules.OperationOf(null));

        Assert.Equal(["PICK_UP", "SLACK_OFF", "ROTATE_OFF", "DRILLING"], TorqueDragRules.Scenarios);
    }

    [Fact]
    public void The_friction_factor_follows_the_interval_and_the_shoe()
    {
        // AC-3: cased above the shoe, open hole below it, and no shoe means all cased.
        Assert.Equal(("cased", 0.2), TorqueDragRules.IntervalOf(500, 0.2, 0.3, 1000));
        Assert.Equal(("cased", 0.2), TorqueDragRules.IntervalOf(1000, 0.2, 0.3, 1000));
        Assert.Equal(("open hole", 0.3), TorqueDragRules.IntervalOf(1000.1, 0.2, 0.3, 1000));
        Assert.Equal(("cased", 0.2), TorqueDragRules.IntervalOf(2500, 0.2, 0.3, null));
    }

    [Fact]
    public void A_components_limit_is_the_owners_pipe_body_yield()
    {
        // The limit is the API 5C3 pipe-body yield at the component's own wall and grade, in newtons.
        // The shipped grade table is API 5CT casing grades, so the limit maths is shown with a grade it carries; a drill-pipe
        // grade such as S135 is refused until the operator adds it, which is the next assertion.
        var pipe = new OgDrillStringComponent("drill pipe", 5, 4.276, 19.5, 2000, "P110");

        Assert.Equal((5 - 4.276) / 2, pipe.Wall, 6);

        var limit = TorqueDragRules.LimitOf(pipe, new CasingDesignSettings());

        Assert.NotNull(limit);
        Assert.Equal(CasingDesign.Tension(5, 0.362, 110000) * 4.4482216152605, limit!.Value, 3);
        Assert.True(limit > 2_000_000d, limit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        // A grade the configuration does not carry has no limit, so the run would have nothing to compare with. S135 is not in
        // the shipped table: the operator adds their drill-pipe grades to casingDesign.gradeYield.
        Assert.Null(TorqueDragRules.LimitOf(pipe with { Grade = "S135" }, new CasingDesignSettings()));
        Assert.Null(TorqueDragRules.LimitOf(pipe with { Grade = "X999" }, new CasingDesignSettings()));
        Assert.Null(TorqueDragRules.LimitOf(pipe with { Grade = null }, new CasingDesignSettings()));
        Assert.Null(TorqueDragRules.LimitOf(pipe with { Id = 5 }, new CasingDesignSettings()));
    }

    [Fact]
    public void The_limiting_factor_is_named_with_its_component()
    {
        // AC-2: the highest load over its limit, and the sentence names the component and the depth.
        var rows = new List<OgTorqueDragRow>
        {
            Row(1, 0, load: 400, limit: 3000, component: "drill pipe"),
            Row(2, 500, load: 2500, limit: 2600, component: "heavy weight"),
            Row(3, 900, load: 900, limit: 2600, component: "heavy weight"),
        };

        var (text, component, utilisation) = TorqueDragRules.Limiting(rows);

        Assert.Equal("heavy weight", component);
        Assert.Equal(2500d / 2600d, utilisation!.Value, 6);
        Assert.Contains("Limiting factor: heavy weight at 500", text, StringComparison.Ordinal);
        Assert.Contains("pipe-body yield", text, StringComparison.Ordinal);
        Assert.Contains("96.2 %", text, StringComparison.Ordinal);

        // No rated station means no limiting factor, said rather than guessed.
        var none = TorqueDragRules.Limiting([Row(1, 0, 100, 0, "(above the string)")]);

        Assert.Null(none.Component);
        Assert.Contains("no string limit is available", none.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_length_in_another_unit_is_refused_at_the_entry_point()
    {
        // AC-5: the parser is where a mixed unit enters, so the refusal lives there.
        var (suffix, problem) = TorqueDragRules.CheckUnit("ft", "m");

        Assert.Equal("ft", suffix);
        Assert.Contains("while the trajectory is in 'm'", problem!, StringComparison.Ordinal);
        Assert.Contains("units are mixed", problem!, StringComparison.Ordinal);

        Assert.Null(TorqueDragRules.CheckUnit("m", "m").Problem);
        Assert.Null(TorqueDragRules.CheckUnit("M", "m").Problem);

        var components = TorqueDragPageView.Components("drill pipe, 5, 4.276, 19.5, 2000, P110", "m", out var none);

        Assert.Null(none);
        var component = Assert.Single(components);

        Assert.Equal("drill pipe", component.Name);
        Assert.Equal(5d, component.Od);
        Assert.Equal(4.276d, component.Id);
        Assert.Equal(19.5d, component.WeightPerLength);
        Assert.Equal(2000d, component.Length);
        Assert.Equal("P110", component.Grade);

        var mixed = TorqueDragPageView.Components("drill pipe, 5, 4.276, 19.5, 6500 ft, S135", "m", out var mixedProblem);

        Assert.Empty(mixed);
        Assert.Contains("units are mixed", mixedProblem!, StringComparison.Ordinal);

        // A length with no suffix is taken in the trajectory's unit.
        Assert.Equal(2000d,
            TorqueDragPageView.Components("pipe, 5, 4.276, 19.5, 2000", "m", out _)[0].Length);

        Assert.Empty(TorqueDragPageView.Components(" ", "m", out var emptyProblem));
        Assert.Contains("needs a drill string", emptyProblem!, StringComparison.Ordinal);

        var incomplete = TorqueDragPageView.Components("pipe, 5, 4.276, 19.5", "m", out var incompleteProblem);

        Assert.Empty(incomplete);
        Assert.Contains("needs an outside diameter, an inside diameter and a length", incompleteProblem!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_run_is_refused_without_a_well_a_scenario_or_a_trajectory()
    {
        var plans = new OgWellPlanService(() => null, new Quiet());
        var service = new OgTorqueDragService(() => null, new Quiet(), plans);
        var request = Request();

        var noWell = await service.RunAsync(request with { WellId = " " });

        Assert.False(noWell.Success);
        Assert.Equal(TorqueDragRules.CodeWellRequired, noWell.Reference);

        var noScenario = await service.RunAsync(request with { Scenario = "REAMING" });

        Assert.False(noScenario.Success);
        Assert.Equal(TorqueDragRules.CodeScenarioUnknown, noScenario.Reference);
        Assert.Contains("PICK_UP", noScenario.Message!, StringComparison.Ordinal);

        // With no plan saved there is no trajectory, and the refusal says so rather than running on nothing.
        var noTrajectory = await service.RunAsync(request);

        Assert.False(noTrajectory.Success);
        Assert.Equal(TorqueDragRules.CodeNoTrajectory, noTrajectory.Reference);
        Assert.Contains("No trajectory is available for W-1", noTrajectory.Message!, StringComparison.Ordinal);

        var noActor = await service.SaveAsync(request, " ");

        Assert.False(noActor.Success);
        Assert.Equal(TorqueDragRules.CodeActorRequired, noActor.Reference);

        var noRun = await service.LoadLastAsync(" ");

        Assert.False(noRun.Success);
        Assert.Equal(TorqueDragRules.CodeWellRequired, noRun.Reference);
    }

    [Fact]
    public void The_page_charts_the_load_against_the_limit_and_shows_the_friction_it_used()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // The chart is the desktop's own surface, hosted the same way as the board.
        Assert.Contains("OgTorqueDragChart", page, StringComparison.Ordinal);
        Assert.Contains("_chart.Points.AddRange", page, StringComparison.Ordinal);
        Assert.Contains("_host.Redraw()", page, StringComparison.Ordinal);

        Assert.Contains("workspace.LimitText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.FrictionText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.SavedText", page, StringComparison.Ordinal);
        Assert.Contains("Components(_string.Text, unit, out problem)", page, StringComparison.Ordinal);

        // A failed run clears the chart rather than leaving the last one on screen.
        Assert.Contains("_chart.Points.Clear()", page, StringComparison.Ordinal);
    }

    private static OgTorqueDragRow Row(int index, double md, double load, double limit, string component) =>
        new(index, md, 0, 0, load, 0, 0, 0, limit, "cased", component, load > limit);

    private static OgTorqueDragRequest Request() =>
        new("W-1", TorqueDragRules.PickUp, 0.2, 0.3, 1000, 10, 10, null, 0, 0,
            [new OgDrillStringComponent("drill pipe", 5, 4.276, 19.5, 2000, "P110")]);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
