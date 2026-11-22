using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.OilGas.Drilling;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-06: the build-and-hold design closing on its target (AC-1), the dogleg check (AC-2), the unit rules (AC-4), the method
/// refusals and the tortuosity definition - the parts that need no database. Saving and the handoff are proven in the gateway
/// test.
/// </summary>
public sealed class WellPlanTests
{
    private const string PageFileName = "WellPlanPageView.cs";

    private static OgPlanRequest BuildAndHold(string unit = "m", double targetNorth = 800, double targetEast = 0,
        double targetTvd = 2000, double dogleg = 3, double tolerance = 30) =>
        new("W-1", "North", "Plan 1", WellPlanRules.BuildAndHold, unit,
            0, 0, 0, 0, 0, 0, targetNorth, targetEast, targetTvd, tolerance, dogleg, []);

    [Fact]
    public async Task A_build_and_hold_plan_closes_on_its_target()
    {
        // AC-1: the owner's curve-and-hold solver lands exactly, so the miss is numerical noise.
        var plan = new OgWellPlanService(() => null, new Quiet());
        var designed = await plan.DesignAsync(BuildAndHold());

        Assert.True(designed.Success, designed.Message);

        var workspace = designed.Value!;

        Assert.Equal(WellPlanRules.BuildAndHold, workspace.Method);
        Assert.True(workspace.Stations.Count > 2);
        Assert.True(workspace.Miss < 1e-6, workspace.Miss?.ToString("G17"));
        Assert.True(workspace.TargetHit);
        Assert.Contains("On target", workspace.TargetText, StringComparison.Ordinal);

        // The stations are the owner's: the last one is the target itself.
        var last = workspace.Stations[^1];

        Assert.Equal(800d, last.North, 4);
        Assert.Equal(0d, last.East, 4);
        Assert.Equal(2000d, last.Tvd, 4);
        Assert.True(last.Inclination > 0 && last.Inclination < 90, last.Inclination.ToString("0.##"));

        // The trajectory the handoff would pass is built from the same stations.
        Assert.NotNull(workspace.Trajectory);
        Assert.Equal(workspace.Stations.Count, workspace.Trajectory!.Survey.Count);
    }

    [Fact]
    public async Task The_dogleg_each_station_shows_is_checked_against_the_configured_limit()
    {
        // AC-2: the figure is the minimum-curvature walk's, not the requested number repeated.
        var service = new OgWellPlanService(() => null, new Quiet());
        var designed = await service.DesignAsync(BuildAndHold(dogleg: 3));

        Assert.True(designed.Success, designed.Message);

        var plan = designed.Value!;

        Assert.NotNull(plan.MaxStationDogleg);
        Assert.True(plan.MaxStationDogleg!.Value > 0, plan.DoglegText);

        // Every station reports a dogleg no larger than the design dogleg plus rounding.
        Assert.All(plan.Stations.Where(station => station.Index > 1),
            station => Assert.True(station.Dogleg <= 3 + 0.5, station.Describe(plan.CourseLength)));

        // A configured limit below the design value is reported, not hidden.
        var settings = DrillingToolbox.Settings;
        var restore = settings.MaxDogleg;

        try
        {
            settings.MaxDogleg = 1.0;

            var strict = (await service.DesignAsync(BuildAndHold(dogleg: 3))).Value!;

            Assert.Equal(1.0, strict.DoglegLimit);
            Assert.False(strict.DoglegWithinLimit);
            Assert.Contains("above", strict.DoglegText, StringComparison.Ordinal);
            Assert.Contains(strict.Warnings, warning => warning.Contains("drilling.maxDogleg", StringComparison.Ordinal));
        }
        finally
        {
            settings.MaxDogleg = restore;
        }

        // With no limit configured the plan says so rather than reporting a pass.
        var unlimited = (await service.DesignAsync(BuildAndHold())).Value!;

        Assert.Null(unlimited.DoglegLimit);
        Assert.True(unlimited.DoglegWithinLimit);
        Assert.Contains("No dogleg limit is configured", unlimited.DoglegText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_tortuosity_figure_states_how_it_was_formed()
    {
        var straight = (await new OgWellPlanService(() => null, new Quiet()).DesignAsync(
            new OgPlanRequest("W-1", null, null, WellPlanRules.Explicit, "m",
                0, 0, 0, 0, 0, 0, null, null, null, 0, 0,
                [new OgPlanSection(1000, 0, 0)]))).Value!;

        // A straight hold is a straight hole: tortuosity zero.
        Assert.Equal(0d, straight.Tortuosity!.Value, 6);
        Assert.Contains("measured length over the straight-line distance", straight.TortuosityBasis, StringComparison.Ordinal);
        Assert.Contains("zero is a straight hole", straight.TortuosityBasis, StringComparison.Ordinal);

        var built = (await new OgWellPlanService(() => null, new Quiet()).DesignAsync(BuildAndHold())).Value!;

        Assert.True(built.Tortuosity > 0, built.TortuosityText);
        Assert.Contains("·", built.TortuosityText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_section_is_parsed_with_its_unit_and_a_mixed_one_is_refused()
    {
        // AC-4: the unit is explicit, and a section written in another unit is refused rather than converted.
        var (hold, holdProblem) = WellPlanRules.ParseSection("hold 500", "m", 30);

        Assert.Null(holdProblem);
        Assert.True(hold!.IsHold);
        Assert.Equal(500d, hold.Length);
        Assert.Contains("hold 500", hold.Describe(30), StringComparison.Ordinal);

        var (curve, curveProblem) = WellPlanRules.ParseSection("curve 3, 90, 300", "m", 30);

        Assert.Null(curveProblem);
        Assert.False(curve!.IsHold);
        Assert.Equal(3d, curve.Dogleg);
        Assert.Equal(90d, curve.Toolface);
        Assert.Equal(300d, curve.Length);
        Assert.Contains("curve 3 deg/30", curve.Describe(30), StringComparison.Ordinal);

        var (mixed, mixedProblem) = WellPlanRules.ParseSection("hold 500 ft", "m", 30);

        Assert.Null(mixed);
        Assert.Contains("units are mixed", mixedProblem!, StringComparison.Ordinal);

        var (bad, badProblem) = WellPlanRules.ParseSection("sideways 10", "m", 30);

        Assert.Null(bad);
        Assert.Contains("is not a section", badProblem!, StringComparison.Ordinal);

        var (shortCurve, shortProblem) = WellPlanRules.ParseSection("curve 3, 90", "m", 30);

        Assert.Null(shortCurve);
        Assert.Contains("a curve is 'curve <dogleg>, <toolface>, <length>'", shortProblem!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_methods_this_build_offers_are_named_and_the_others_are_refused_with_the_reason()
    {
        var service = new OgWellPlanService(() => null, new Quiet());

        Assert.Equal(["BUILD_AND_HOLD", "EXPLICIT"], WellPlanRules.Methods);

        var sShape = await service.DesignAsync(BuildAndHold() with { Method = "S_SHAPE" });

        Assert.False(sShape.Success);
        Assert.Equal(WellPlanRules.CodeMethodUnknown, sShape.Reference);
        Assert.Contains("an S-shape needs a drop-back solve", sShape.Message!, StringComparison.Ordinal);

        var horizontal = await service.DesignAsync(BuildAndHold() with { Method = "HORIZONTAL" });

        Assert.False(horizontal.Success);
        Assert.Equal(WellPlanRules.CodeMethodUnknown, horizontal.Reference);
    }

    [Fact]
    public async Task A_missing_datum_target_or_well_is_refused()
    {
        var service = new OgWellPlanService(() => null, new Quiet());

        var noWell = await service.DesignAsync(BuildAndHold() with { WellId = " " });

        Assert.False(noWell.Success);
        Assert.Equal(WellPlanRules.CodeWellRequired, noWell.Reference);

        var noUnit = await service.DesignAsync(BuildAndHold() with { Unit = " " });

        Assert.False(noUnit.Success);
        Assert.Equal(WellPlanRules.CodeUnitRequired, noUnit.Reference);
        Assert.Contains("no start datum", noUnit.Message!, StringComparison.Ordinal);

        var noTarget = await service.DesignAsync(BuildAndHold() with { TargetNorth = null });

        Assert.False(noTarget.Success);
        Assert.Equal(WellPlanRules.CodeTargetMissing, noTarget.Reference);

        var noSections = await service.DesignAsync(BuildAndHold() with { Method = WellPlanRules.Explicit, Sections = [] });

        Assert.False(noSections.Success);
        Assert.Equal(WellPlanRules.CodeNoDesign, noSections.Reference);

        var noDogleg = await service.DesignAsync(BuildAndHold() with { Dogleg = 0 });

        Assert.False(noDogleg.Success);
        Assert.Equal(WellPlanRules.CodeNoDesign, noDogleg.Reference);
    }

    [Fact]
    public async Task A_target_that_cannot_be_reached_is_refused_with_a_sentence_not_an_exception()
    {
        // Behind the bit: a negative TVD target from a vertical tie-on at the surface.
        var service = new OgWellPlanService(() => null, new Quiet());

        var behind = await service.DesignAsync(BuildAndHold(targetTvd: -500, targetNorth: 0));

        Assert.False(behind.Success);
        Assert.Equal(WellPlanRules.CodeUnreachable, behind.Reference);
        Assert.Contains("cannot be reached", behind.Message!, StringComparison.Ordinal);
        Assert.Contains("turning circle", behind.Message!, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", behind.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Saving_needs_an_actor_and_handing_off_needs_a_saved_plan()
    {
        var offline = new OgWellPlanService(() => null, new Quiet());

        var noActor = await offline.SaveAsync(BuildAndHold(), " ");

        Assert.False(noActor.Success);
        Assert.Equal(WellPlanRules.CodeActorRequired, noActor.Reference);

        var noPlan = await offline.HandoffAsync(" ", "sara");

        Assert.False(noPlan.Success);
        Assert.Equal(WellPlanRules.CodeHandoffBeforeSave, noPlan.Reference);
    }

    [Fact]
    public void The_page_puts_the_unit_on_every_geometric_column_and_builds_the_views_from_the_table()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-4: the headers are rewritten from the plan's own unit on load.
        Assert.Contains("Headers((\"MD (\" + plan.Unit + \")\"", page, StringComparison.Ordinal);
        Assert.Contains("DoglegCourseLabel(plan.Unit)", page, StringComparison.Ordinal);

        // AC-3: both views come from the station list the grid binds.
        Assert.Contains("OgWellPath.PlanPath(stations)", page, StringComparison.Ordinal);
        Assert.Contains("OgWellPath.SectionPath(stations, sectionAzimuth)", page, StringComparison.Ordinal);
        Assert.Contains("_stationGrid.ItemsSource = plan.Stations.ToList()", page, StringComparison.Ordinal);

        // AC-5: the handoff is only offered for a saved plan.
        Assert.Contains("Save the plan before handing its trajectory on", page, StringComparison.Ordinal);
        Assert.Contains("plan.SavedText", page, StringComparison.Ordinal);
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
