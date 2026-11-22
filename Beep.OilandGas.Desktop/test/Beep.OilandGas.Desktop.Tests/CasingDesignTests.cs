using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-02: the API 5C3 ratings against published values (AC-1), the utilisation arithmetic (AC-2), the governing case (AC-3) and
/// the refusal of a grade the configuration does not carry (AC-5) - the parts that need no database. The loads over real rows
/// are proven in the gateway test.
/// </summary>
public sealed class CasingDesignTests
{
    private const string PageFileName = "CasingDesignPageView.cs";

    [Theory]
    // OD, wall, yield (psi), published collapse pressure, regime - the values the tools' own conformance test uses.
    [InlineData(9.625, 0.472, 80000, 4760, "Plastic")]
    [InlineData(9.625, 0.435, 80000, 3810, "Plastic")]
    [InlineData(9.625, 0.435, 55000, 3250, "Plastic")]
    [InlineData(9.625, 0.352, 80000, 2370, "Transition")]
    [InlineData(9.625, 0.352, 110000, 2470, "Elastic")]
    [InlineData(20.0, 0.438, 55000, 520, "Elastic")]
    public void Collapse_reproduces_the_published_ratings(double od, double wall, double yield, double published, string regime)
    {
        // AC-1: the desktop does not implement its own collapse curve - these are API 5C3, agreed by two published sources.
        var (pressure, actualRegime) = CasingDesign.Collapse(od, wall, yield);

        Assert.Equal(published, pressure);
        Assert.Equal(regime, actualRegime);
    }

    [Fact]
    public void Burst_and_tension_reproduce_the_published_ratings()
    {
        Assert.Equal(6870, CasingDesign.Burst(9.625, 0.472, 80000));      // 9 5/8 47 lb/ft L80
        Assert.Equal(9440, CasingDesign.Burst(9.625, 0.472, 110000));     // 9 5/8 47 lb/ft P110
        Assert.Equal(14520, CasingDesign.Burst(5.5, 0.415, 110000));      // the EPA worked example
        Assert.Equal(1086000, CasingDesign.Tension(9.625, 0.472, 80000));
        Assert.Equal(1480000, CasingDesign.Tension(20.0, 0.438, 55000));
    }

    [Fact]
    public void The_regimes_change_at_the_api_boundaries()
    {
        string Regime(double dt) => CasingDesign.Collapse(10, 10 / dt, 80000).regime;

        Assert.Equal(
            ["Yield", "Plastic", "Plastic", "Transition", "Transition", "Elastic"],
            new[] { 13.3, 13.5, 22.4, 22.6, 30.9, 31.1 }.Select(Regime));
    }

    [Fact]
    public void Utilisation_is_the_load_times_the_factor_over_the_resistance()
    {
        // AC-2: above 1 does not pass, and the utilisation times the achieved factor is the required factor.
        Assert.Equal(1m, CasingDesignRules.Utilisation(4000m, 5000m, 1.25m));
        Assert.Equal(1.25m,
            CasingDesignRules.Utilisation(4000m, 5000m, 1.25m) * (5000m / 4000m));

        Assert.True(CasingDesignRules.Passes(1m));
        Assert.True(CasingDesignRules.Passes(0.999m));
        Assert.False(CasingDesignRules.Passes(1.001m));

        // A pipe with no resistance cannot pass, whatever the load.
        Assert.Equal(decimal.MaxValue, CasingDesignRules.Utilisation(1m, 0m, 1m));

        var verdict = new OgCasingVerdict(
            "CS-1", "PRODUCTION", CasingDesignRules.Burst, 5000m, 6250m, 1m, 1.25m, Passes: true);

        Assert.Equal(1.25m, verdict.Factor);
        Assert.Equal("psi", verdict.Unit);
        Assert.Contains("passes", verdict.Describe(), StringComparison.Ordinal);
        Assert.Equal("lbf", new OgCasingVerdict(
            "CS-1", "PRODUCTION", CasingDesignRules.Tension, 1m, 1m, 1m, 1m, true).Unit);
    }

    [Fact]
    public void The_governing_case_is_the_highest_utilisation_and_it_is_named()
    {
        // AC-3: the section and the case, not a bare number.
        var verdicts = new List<OgCasingVerdict>
        {
            new("CS-1", "SURFACE", CasingDesignRules.Burst, 5000m, 6870m, 0.91m, 1.25m, true),
            new("CS-2", "PRODUCTION", CasingDesignRules.Collapse, 5194.8m, 4760m, 1.201m, 1.1m, false),
            new("CS-2", "PRODUCTION", CasingDesignRules.Tension, 398278m, 1086000m, 0.513m, 1.4m, true),
        };

        var (governing, utilisation) = CasingDesignRules.Governing(verdicts);

        Assert.Equal("PRODUCTION · COLLAPSE", governing);
        Assert.Equal(1.201m, utilisation);

        // Ties are broken by section then case, so the answer is stable rather than whichever row came first.
        var tied = new List<OgCasingVerdict>
        {
            new("CS-2", "A", CasingDesignRules.Tension, 1m, 1m, 2m, 1m, false),
            new("CS-1", "Z", CasingDesignRules.Burst, 1m, 1m, 2m, 1m, false),
        };

        Assert.Equal("A · TENSION", CasingDesignRules.Governing(tied).Case);

        Assert.Null(CasingDesignRules.Governing([]).Case);
        Assert.Null(CasingDesignRules.Governing([]).Utilisation);
    }

    [Fact]
    public void The_factors_come_from_the_configuration_object()
    {
        // AC-4: the only source is the owner's settings, and a changed settings object changes the answer - the view cannot
        // supply a factor because it has no field for one.
        var defaults = CasingDesignRules.FactorsOf(new CasingDesignSettings());

        Assert.Equal(1.25m, defaults.Burst);
        Assert.Equal(1.1m, defaults.Collapse);
        Assert.Equal(1.4m, defaults.Tension);
        Assert.Contains("from configuration", defaults.Describe(), StringComparison.Ordinal);

        var changed = CasingDesignRules.FactorsOf(new CasingDesignSettings
        {
            DesignFactors = new CasingDesignFactors { Burst = 1.5, Collapse = 1.125, Tension = 1.8 },
        });

        Assert.Equal(1.5m, changed.Of(CasingDesignRules.Burst));
        Assert.Equal(1.125m, changed.Of(CasingDesignRules.Collapse));
        Assert.Equal(1.8m, changed.Of(CasingDesignRules.Tension));

        // The running environment is the configuration the service reads.
        Assert.Equal(
            CasingDesignRules.FactorsOf(WellboreEnvironment.Current.CasingDesign),
            CasingDesignRules.FactorsOf(WellboreEnvironment.Current.CasingDesign));
    }

    [Fact]
    public async Task A_grade_the_configuration_does_not_carry_is_refused_with_the_grade()
    {
        // AC-5: refused before anything is written, naming the grade the record holds.
        var offline = new OgCasingDesignService(() => null, new Quiet());

        var unknown = await offline.AddSectionAsync(
            "PD-1", "PRODUCTION", 9.625m, 0.472m, 47m, "Д", 0m, 10000m, "ft", "sara");

        Assert.False(unknown.Success);
        Assert.Equal(CasingDesignRules.CodeUnknownGrade, unknown.Reference);
        Assert.Contains("Д", unknown.Message!, StringComparison.Ordinal);

        // A base grade is accepted through its family, exactly as the owner's own YieldOf does.
        Assert.Equal(80000, CasingDesignRules.YieldOf("L80-13Cr", new CasingDesignSettings()));
        Assert.Equal(80000, CasingDesignRules.YieldOf("N80Q", new CasingDesignSettings()));
        Assert.Null(CasingDesignRules.YieldOf("Д", new CasingDesignSettings()));

        var noGrade = await offline.AddSectionAsync(
            "PD-1", "PRODUCTION", 9.625m, 0.472m, 47m, " ", 0m, 10000m, "ft", "sara");

        Assert.False(noGrade.Success);
        Assert.Equal(CasingDesignRules.CodeUnknownGrade, noGrade.Reference);
    }

    [Fact]
    public void A_section_with_no_length_or_a_missing_actor_is_refused()
    {
        var offline = new OgCasingDesignService(() => null, new Quiet());

        var actor = offline.AddSectionAsync("PD-1", "P", 9.625m, 0.472m, 47m, "L80", 0m, 100m, "ft", " ")
            .GetAwaiter().GetResult();

        Assert.False(actor.Success);
        Assert.Equal(CasingDesignRules.CodeActorRequired, actor.Reference);

        var inverted = offline.AddSectionAsync("PD-1", "P", 9.625m, 0.472m, 47m, "L80", 500m, 100m, "ft", "sara")
            .GetAwaiter().GetResult();

        Assert.False(inverted.Success);
        Assert.Equal(CasingDesignRules.CodeSectionInvalid, inverted.Reference);

        var noDesign = offline.AddSectionAsync(" ", "P", 9.625m, 0.472m, 47m, "L80", 0m, 100m, "ft", "sara")
            .GetAwaiter().GetResult();

        Assert.False(noDesign.Success);
        Assert.Equal(CasingDesignRules.CodeDesignRequired, noDesign.Reference);
    }

    [Fact]
    public void Depth_conversion_hands_the_api_arithmetic_feet()
    {
        Assert.True(CasingDesignRules.TryFeet(10000m, "ft", out var feet));
        Assert.Equal(10000d, feet);

        Assert.True(CasingDesignRules.TryFeet(3048m, "m", out var fromMetres));
        Assert.Equal(10000d, fromMetres, 3);

        // Nothing is assumed when the unit is not in the catalogue.
        Assert.False(CasingDesignRules.TryFeet(10m, "furlong", out _));

        // The plain-end weight is the API 5CT relation the owner's own Check falls back to.
        Assert.Equal(10.69 * (9.625 - 0.472) * 0.472, CasingDesignRules.PlainEndWeight(9.625, 0.472), 6);
    }

    [Fact]
    public void The_page_prints_the_factors_and_offers_no_way_to_edit_them()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.Factors.Describe()", page, StringComparison.Ordinal);
        Assert.Contains("workspace.GoverningText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Verdicts", page, StringComparison.Ordinal);

        // AC-4: the load-case bar carries the mud, the test pressure and the overpull - and no design-factor field.
        Assert.DoesNotContain("Design factor\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("_burstFactor", page, StringComparison.Ordinal);
        Assert.DoesNotContain("_collapseFactor", page, StringComparison.Ordinal);
        Assert.DoesNotContain("_tensionFactor", page, StringComparison.Ordinal);
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
