using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdea.OilGas.Drilling;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-08: the textbook reference arithmetic (AC-1), the per-section velocity and loss relation (AC-2), the fracture comparison
/// (AC-3), the 32nds nozzle rule (AC-4), the monotone ECD relation the optimiser rests on (AC-5) and the refusals - the parts that
/// need no database. The circulating run along a plan is proven in the gateway test.
/// </summary>
public sealed class HydraulicsTests
{
    private const string PageFileName = "HydraulicsPageView.cs";

    /// <summary>A Bingham mud: 10 ppg, and 30/20 on the viscometer, which is PV 10 cP and YP 10 lbf/100 ft2.</summary>
    private static MudRheology Mud() => new() { DensityPpg = 10, Theta600 = 30, Theta300 = 20 };

    /// <summary>The plastic viscosity the readings give: theta600 - theta300.</summary>
    private const double Pv = 30 - 20;

    /// <summary>The yield point the readings give: theta300 - PV.</summary>
    private const double Yp = 20 - Pv;

    [Fact]
    public void An_annulus_gradient_matches_the_textbook_laminar_formula()
    {
        // AC-1: the reference is the textbook Bingham laminar relation written out here, not the owner's code read back.
        //   v = q / (2.448 (d2^2 - d1^2));  mu_a = PV + 5 YP (d2-d1)/v
        //   Re = 757 rho v (d2-d1) / mu_a;  dp/dL = PV v / (1000 (d2-d1)^2) + YP / (200 (d2-d1))
        const double q = 200, outer = 8.5, inner = 5;
        var gap = outer - inner;
        var velocity = q / (2.448 * (outer * outer - inner * inner));
        var apparent = Pv + 5 * Yp * gap / velocity;
        var reynolds = 757 * 10 * velocity * gap / apparent;
        var gradient = Pv * velocity / (1000 * gap * gap) + Yp / (200 * gap);

        var (actual, actualReynolds, turbulent) = Hydraulics.Gradient(Mud(), q, outer, inner, annulus: true);

        Assert.False(turbulent);
        Assert.Equal(reynolds, actualReynolds, 1);
        Assert.Equal(gradient, actual!.Value, 9);

        // 1 000 ft of that annulus at a 1 000 ft true vertical depth is the reference ECD.
        var loss = gradient * 1000;
        var ecd = HydraulicsRules.EcdOf(10, loss, 1000);

        Assert.Equal(0.0156971, gradient, 6);
        Assert.Equal(15.6971, loss, 3);
        Assert.Equal(10.3019, ecd, 3);
    }

    [Fact]
    public void A_pipe_gradient_matches_the_textbook_laminar_formula()
    {
        // AC-1 for the inside of the string: dp/dL = PV v / (1500 d^2) + YP / (225 d), Re = 928 rho v d / mu_a.
        const double q = 150, diameter = 4.276;
        var velocity = q / (2.448 * diameter * diameter);
        var apparent = Pv + 6.66 * Yp * diameter / velocity;
        var reynolds = 928 * 10 * velocity * diameter / apparent;
        var gradient = Pv * velocity / (1500 * diameter * diameter) + Yp / (225 * diameter);

        var (actual, actualReynolds, turbulent) = Hydraulics.Gradient(Mud(), q, diameter, 0, annulus: false);

        Assert.False(turbulent);
        Assert.Equal(reynolds, actualReynolds, 1);
        Assert.Equal(gradient, actual!.Value, 9);
        Assert.Equal(0.011616, gradient, 6);
    }

    [Fact]
    public void The_velocity_is_the_rate_over_the_flow_area()
    {
        // AC-2: one velocity per flow path, from the area the path actually has.
        Assert.Equal(600 / (2.448 * 4.276 * 4.276), Hydraulics.Velocity(600, 4.276), 9);
        Assert.Equal(600 / (2.448 * (8.681 * 8.681 - 5 * 5)), Hydraulics.Velocity(600, 8.681, 5), 9);

        // A wider annulus at the same rate flows slower, which is what makes the per-section column meaningful.
        Assert.True(Hydraulics.Velocity(600, 12.25, 5) < Hydraulics.Velocity(600, 8.5, 5));
    }

    [Fact]
    public void The_bit_loss_matches_the_nozzle_area_formula_and_is_in_32nds()
    {
        // AC-1 and AC-4: dp = 8.311e-5 rho q^2 / (Cd^2 A^2), A the total area of the nozzles sized in 32nds.
        double[] nozzles = [12, 12, 12];
        var area = nozzles.Sum(size => Math.PI / 4 * Math.Pow(size / 32, 2));
        var expected = 8.311e-5 * 10 * 600 * 600 / (0.95 * 0.95 * area * area);

        Assert.Equal(expected, Hydraulics.BitLoss(Mud(), 600, nozzles), 6);

        // The size is shown as a fraction over 32, so it cannot be read as inches.
        Assert.Equal("12/32 in", HydraulicsRules.NozzleAsFraction(12));
        Assert.Equal("8/32 in", HydraulicsRules.NozzleAsFraction(8));
    }

    [Fact]
    public void The_ecd_is_the_mud_weight_plus_the_annular_loss_over_the_depth()
    {
        // AC-1's relation, stated once: an ECD can never be below the mud weight, and it rises with the loss and falls with depth.
        Assert.Equal(10, HydraulicsRules.EcdOf(10, 0, 1000));
        Assert.Equal(10.5, HydraulicsRules.EcdOf(10, 26, 1000), 6);
        Assert.True(HydraulicsRules.EcdOf(10, 26, 500) > HydraulicsRules.EcdOf(10, 26, 1000));

        // A depth of zero has no ECD to quote, so the mud weight stands rather than an infinite number.
        Assert.Equal(10, HydraulicsRules.EcdOf(10, 26, 0));

        // The relation is monotone in the loss, which is what the optimiser's search relies on.
        var previous = double.MinValue;

        for (var loss = 0d; loss <= 100d; loss += 10d)
        {
            var ecd = HydraulicsRules.EcdOf(10, loss, 2000);

            Assert.True(ecd > previous, loss.ToString("0"));
            previous = ecd;
        }
    }

    [Fact]
    public void A_fracture_gradient_is_read_in_either_unit_and_the_absent_one_is_not_invented()
    {
        // AC-3: a gradient recorded as a pressure gradient and one recorded as a mud weight both arrive in ppg.
        Assert.Equal(1.0 / 0.052, HydraulicsRules.FractureGradientPpg(1.0, "psi/ft")!.Value, 6);
        Assert.Equal(11.5, HydraulicsRules.FractureGradientPpg(11.5, "ppg")!.Value, 6);
        // No unit is not a convention: it could be either reading and the two differ by a factor of nineteen.
        Assert.Null(HydraulicsRules.FractureGradientPpg(11.5, null));

        Assert.Null(HydraulicsRules.FractureGradientPpg(0, "ppg"));
        Assert.Null(HydraulicsRules.FractureGradientPpg(-1, "psi/ft"));
    }

    [Fact]
    public void A_profile_above_the_fracture_gradient_is_critical_and_names_the_depth()
    {
        // AC-3: the worst point decides, and the sentence carries the depth and the limit.
        var profile = new List<OgEcdPoint>
        {
            new(500, 10.2),
            new(1200, 11.9),
            new(2000, 11.1),
        };

        var (exceeded, worst) = HydraulicsRules.AboveFracture(profile, 11.5);

        Assert.True(exceeded);
        Assert.Equal(1200, worst!.Depth);
        Assert.Equal(11.9, worst.Value);

        var critical = HydraulicsRules.CriticalOf(exceeded, worst, 11.5, "m");

        Assert.StartsWith("CRITICAL", critical, StringComparison.Ordinal);
        Assert.Contains("11.9 ppg", critical, StringComparison.Ordinal);
        Assert.Contains("1200 m", critical, StringComparison.Ordinal);
        Assert.Contains("Reduce the rate or the mud weight", critical, StringComparison.Ordinal);

        // Below the gradient, and with no gradient recorded, the sentence is a statement rather than a warning.
        Assert.False(HydraulicsRules.AboveFracture(profile, 12.5).Exceeded);
        Assert.False(HydraulicsRules.AboveFracture(profile, null).Exceeded);
        Assert.Contains("within the recorded fracture gradient",
            HydraulicsRules.CriticalOf(false, null, 12.5, "m"), StringComparison.Ordinal);
    }

    [Fact]
    public void Nozzles_are_parsed_in_32nds_and_a_size_outside_the_range_is_refused()
    {
        // AC-4: '12', '12/32' and a list all mean 32nds; a size that ends in 2 or 3 is not eaten by a suffix trim.
        var three = HydraulicsPageView.Nozzles("12, 12, 12", out var none);

        Assert.Null(none);
        Assert.Equal([12d, 12d, 12d], three);

        Assert.Equal([12d], HydraulicsPageView.Nozzles("12/32", out _));
        Assert.Equal([22d, 23d], HydraulicsPageView.Nozzles("22 23", out _));
        Assert.Equal([8d, 10d], HydraulicsPageView.Nozzles("8 + 10", out _));

        // An empty box is the feature's "no nozzle" state, which the run accepts and flags.
        Assert.Empty(HydraulicsPageView.Nozzles("  ", out var emptyProblem));
        Assert.Null(emptyProblem);

        Assert.Empty(HydraulicsPageView.Nozzles("0", out var zeroProblem));
        Assert.Contains("outside the range", zeroProblem!, StringComparison.Ordinal);

        Assert.Empty(HydraulicsPageView.Nozzles("40", out var bigProblem));
        Assert.Contains("1 to 32", bigProblem!, StringComparison.Ordinal);

        Assert.Empty(HydraulicsPageView.Nozzles("big", out var textProblem));
        Assert.Contains("not a nozzle size", textProblem!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_flow_sections_are_parsed_from_a_pipe_and_an_annulus()
    {
        var sections = HydraulicsPageView.Sections(
            "drill pipe, 0, 2000, 4.276\nannulus, 0, 1000, 8.681, 5\n", out var problem);

        Assert.Null(problem);
        Assert.Equal(2, sections.Count);

        Assert.Equal("drill pipe", sections[0].Name);
        Assert.Equal("inside pipe", sections[0].Interval);
        Assert.False(sections[0].IsAnnulus);
        Assert.Equal(2000, sections[0].Length);
        Assert.Equal(4.276, sections[0].OuterDiameter);
        Assert.Null(sections[0].InnerDiameter);

        Assert.Equal("annulus", sections[1].Interval);
        Assert.True(sections[1].IsAnnulus);
        Assert.Equal(8.681, sections[1].OuterDiameter);
        Assert.Equal(5, sections[1].InnerDiameter!.Value);

        Assert.Empty(HydraulicsPageView.Sections("pipe, 0, 100", out var shortProblem));
        Assert.Contains("at least a name, a top, a bottom and a diameter", shortProblem!, StringComparison.Ordinal);

        Assert.Empty(HydraulicsPageView.Sections("pipe, 0, deep, 4.276", out var textProblem));
        Assert.Contains("not a number", textProblem!, StringComparison.Ordinal);

        Assert.Empty(HydraulicsPageView.Sections(" ", out var emptyProblem));
        Assert.Contains("needs the flow sections", emptyProblem!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_run_is_refused_without_a_well_mud_a_rate_a_section_or_a_plan()
    {
        var plans = new OgWellPlanService(() => null, new Quiet());
        var service = new OgHydraulicsService(() => null, new Quiet(), plans);

        var noWell = await service.RunAsync(Request() with { WellId = " " });

        Assert.False(noWell.Success);
        Assert.Equal(HydraulicsRules.CodeWellRequired, noWell.Reference);

        // Each missing rheology input is named, because the owner derives everything from the two readings.
        var noDensity = await service.RunAsync(Request() with { MudDensityPpg = 0 });

        Assert.Equal(HydraulicsRules.CodeRheologyMissing, noDensity.Reference);
        Assert.Contains("mud density", noDensity.Message!, StringComparison.Ordinal);

        var no600 = await service.RunAsync(Request() with { Theta600 = 0 });

        Assert.Equal(HydraulicsRules.CodeRheologyMissing, no600.Reference);
        Assert.Contains("600 rpm", no600.Message!, StringComparison.Ordinal);

        var no300 = await service.RunAsync(Request() with { Theta300 = 0 });

        Assert.Equal(HydraulicsRules.CodeRheologyMissing, no300.Reference);
        Assert.Contains("300 rpm", no300.Message!, StringComparison.Ordinal);

        var inverted = await service.RunAsync(Request() with { Theta600 = 20, Theta300 = 30 });

        Assert.Equal(HydraulicsRules.CodeRheologyMissing, inverted.Reference);
        Assert.Contains("not a physical Bingham fit", inverted.Message!, StringComparison.Ordinal);

        // 40/15 gives PV 25 and a negative yield point, which is also refused rather than modelled.
        var negativeYp = await service.RunAsync(Request() with { Theta600 = 40, Theta300 = 15 });

        Assert.Equal(HydraulicsRules.CodeRheologyMissing, negativeYp.Reference);
        Assert.Contains("yield point", negativeYp.Message!, StringComparison.Ordinal);

        var noRate = await service.RunAsync(Request() with { FlowRate = 0 });

        Assert.Equal(HydraulicsRules.CodeNoFlowRate, noRate.Reference);

        var noSections = await service.RunAsync(Request() with { Sections = [] });

        Assert.Equal(HydraulicsRules.CodeNoSections, noSections.Reference);

        var badNozzle = await service.RunAsync(Request() with { Nozzles32nds = [40] });

        Assert.Equal(HydraulicsRules.CodeNozzleInvalid, badNozzle.Reference);

        // With no plan saved there is no trajectory, and the ECD has no depth to be quoted at.
        var noPlan = await service.RunAsync(Request());

        Assert.False(noPlan.Success);
        Assert.Equal(HydraulicsRules.CodeNoTrajectory, noPlan.Reference);
        Assert.Contains("No trajectory is available for W-1", noPlan.Message!, StringComparison.Ordinal);

        var noActor = await service.SaveAsync(Request(), " ");

        Assert.Equal(HydraulicsRules.CodeActorRequired, noActor.Reference);
    }

    [Fact]
    public async Task The_optimiser_refuses_a_target_below_the_mud_weight_with_the_reason()
    {
        // A target under the static mud weight cannot be reached by any rate: the annulus only adds to the weight. The refusal comes
        // before any trajectory is needed, because it is a statement about the physics rather than about the well.
        var service = new OgHydraulicsService(() => null, new Quiet(), new OgWellPlanService(() => null, new Quiet()));

        var below = await service.OptimiseAsync(Request(), 9.5);

        Assert.True(below.Success);
        Assert.False(below.Value!.Feasible);
        Assert.Contains("can never fall to the 9.5 ppg", below.Value.Reason, StringComparison.Ordinal);
        Assert.Contains("Reduce the mud weight", below.Value.Reason, StringComparison.Ordinal);
        Assert.Null(below.Value.FlowRateGpm);
        Assert.StartsWith("Infeasible", below.Value.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_charts_the_profile_and_shows_the_threshold_only_when_one_is_recorded()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("OgEcdProfileChart", page, StringComparison.Ordinal);
        Assert.Contains("_chart.Profile.AddRange", page, StringComparison.Ordinal);
        Assert.Contains("_chart.Fracture.AddRange(workspace.FracturePoints", page, StringComparison.Ordinal);
        Assert.Contains("_host.Redraw()", page, StringComparison.Ordinal);

        Assert.Contains("workspace.CriticalText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.FractureText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.NozzlesText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.MudText", page, StringComparison.Ordinal);

        // A failed run clears the chart rather than leaving the last profile on screen.
        Assert.Contains("_chart.Profile.Clear()", page, StringComparison.Ordinal);
    }

    private static OgHydraulicsRequest Request() =>
        new("W-1", 10, 30, 20, 300, "gpm", 100, [12, 12, 12],
        [
            new OgFlowSection("drill pipe", 0, 2000, 4.276, null),
            new OgFlowSection("annulus", 0, 1000, 8.681, 5),
            new OgFlowSection("annulus", 1000, 2000, 8.5, 5),
        ]);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
