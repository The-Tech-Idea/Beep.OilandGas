using System.Globalization;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WTA-01: the reference build-up, the tokens, the radial-flow window, the Horner cross-check, the derivative verdict and the
/// validation - the parts that do not need a database. Saving and reloading are proven in the gateway test.
/// </summary>
public sealed class WellTestTests
{
    private const string PageFileName = "WellTestPageView.cs";

    /// <summary>k = 50 md, q = 500 STB/d, B = 1.2, mu = 1.5 cP, h = 40 ft -> m = 73.17 psi/cycle.</summary>
    private const decimal ReferencePermeability = 50m;

    [Fact]
    public void A_reference_build_up_yields_the_permeability_it_was_built_from()
    {
        // AC-1 and SG-08-1: the readings are generated from p = p* - m log10((tp + dt)/dt) with m from the published
        // relation, so the analysis has to give back 50 md.
        var result = Analyse(Horner());

        Assert.True(result.Converged);
        Assert.Equal(WellTestAnalyses.BuildUp, result.AnalysisType);
        Assert.Equal(WellTestAnalyses.Horner, result.Method);

        Assert.InRange(
            Math.Abs(result.Permeability - ReferencePermeability) / ReferencePermeability,
            0m,
            0.01m);

        // The slope it reports is the published relation applied to the permeability it found.
        var expectedSlope = 162.6m * 500m * 1.2m * 1.5m / (ReferencePermeability * 40m);

        Assert.NotNull(result.AnalysisSlope);
        Assert.InRange(Math.Abs(result.AnalysisSlope!.Value - expectedSlope) / expectedSlope, 0m, 0.01m);

        // The extrapolated pressure is the one the synthetic line was built to.
        Assert.NotNull(result.ReservoirPressure);
        Assert.InRange(result.ReservoirPressure!.Value, 2999m, 3001m);

        // The window is the one asked for, and the record says how many readings it held.
        Assert.Equal(1m, result.WindowStartHours);
        Assert.Equal(60m, result.WindowEndHours);
        Assert.Equal(9, result.PointsInWindow);
        Assert.Contains("9 reading(s)", result.WindowText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_analysis_type_and_method_are_recorded_tokens()
    {
        // AC-2.
        Assert.Equal(WellTestAnalysisWellKnown.AnalysisClassification.BuildUp, WellTestAnalyses.BuildUp);
        Assert.Equal(WellTestAnalysisWellKnown.AnalysisClassification.DrawDown, WellTestAnalyses.DrawDown);
        Assert.Equal(WellTestAnalysisWellKnown.AnalysisMethod.Horner, WellTestAnalyses.Horner);
        Assert.Equal(WellTestAnalysisWellKnown.AnalysisMethod.Mdh, WellTestAnalyses.Mdh);

        Assert.Equal([WellTestAnalyses.BuildUp, WellTestAnalyses.DrawDown], WellTestAnalyses.Types);
        Assert.Equal([WellTestAnalyses.Horner, WellTestAnalyses.Mdh], WellTestAnalyses.Methods);

        var result = Analyse(Horner());

        Assert.Contains(result.AnalysisType, WellTestAnalyses.Types);
        Assert.Contains(result.Method, WellTestAnalyses.Methods);

        // An unknown type is refused naming the tokens.
        var unknown = WellTestRules.Validate(Inputs(HornerReadings()) with { AnalysisType = "INTERFERENCE" });

        Assert.Equal(WellTestRules.CodeTypeUnknown, unknown!.Value.Code);
        Assert.Contains("BUILDUP", unknown.Value.Message, StringComparison.Ordinal);

        // A drawdown has no Horner straight line, and that is said rather than quietly substituted.
        var wrongMethod = WellTestRules.Validate(Inputs(HornerReadings()) with
        {
            AnalysisType = WellTestAnalyses.DrawDown, Method = WellTestAnalyses.Horner,
        });

        Assert.Equal(WellTestRules.CodeMethodUnsupported, wrongMethod!.Value.Code);
        Assert.Contains("DRAWDOWN supports MDH", wrongMethod.Value.Message, StringComparison.Ordinal);

        Assert.Equal([WellTestAnalyses.Mdh], WellTestAnalyses.MethodsFor(WellTestAnalyses.DrawDown));
    }

    [Fact]
    public void A_missing_radial_flow_window_is_refused_not_guessed()
    {
        // AC-3, and the state table's "no radial-flow window - the result is refused".
        var noWindow = WellTestRules.Validate(Inputs(HornerReadings()) with
        {
            WindowStartHours = null, WindowEndHours = null,
        });

        Assert.Equal(WellTestRules.CodeInputRequired, noWindow!.Value.Code);
        Assert.Contains("radial-flow window start", noWindow.Value.Message, StringComparison.Ordinal);

        var backwards = WellTestRules.Validate(Inputs(HornerReadings()) with
        {
            WindowStartHours = 60m, WindowEndHours = 10m,
        });

        Assert.Equal(WellTestRules.CodeWindowOutsideData, backwards!.Value.Code);

        // A window with no reading in it names the window and asks for a selection.
        var empty = WellTestRules.Validate(Inputs(HornerReadings()) with
        {
            WindowStartHours = 500m, WindowEndHours = 600m,
        });

        Assert.Equal(WellTestRules.CodeNoWindow, empty!.Value.Code);
        Assert.Contains("select the radial-flow window", empty.Value.Message, StringComparison.Ordinal);

        // Two readings cannot make a straight line.
        var tooSmall = WellTestRules.Validate(Inputs(HornerReadings()) with
        {
            WindowStartHours = 0.5m, WindowEndHours = 1m,
        });

        Assert.Equal(WellTestRules.CodeWindowTooSmall, tooSmall!.Value.Code);
        Assert.Equal(3, WellTestRules.MinimumPointsInWindow);
    }

    [Fact]
    public void The_independent_horner_agrees_or_the_difference_is_shown()
    {
        // AC-4 and SG-08-2.
        var result = Analyse(Horner());

        Assert.True(result.CrossCheck.Available, result.CrossCheck.Text);
        Assert.True(result.CrossCheck.Agrees, result.CrossCheck.Text);
        Assert.Equal("agrees", result.CrossCheck.AgreeText);
        Assert.Contains("HornerAnalysis", result.CrossCheck.Source, StringComparison.Ordinal);

        // Both numbers are stored, and the flag is exactly the stated tolerance between them.
        Assert.NotNull(result.CrossCheck.Slope);
        Assert.NotNull(result.CrossCheck.Permeability);
        Assert.NotNull(result.AnalysisSlope);
        Assert.InRange(result.CrossCheck.Permeability!.Value, 49.9m, 50.1m);
        Assert.Equal(
            Math.Abs(result.AnalysisSlope!.Value - result.CrossCheck.Slope!.Value) / result.AnalysisSlope.Value
                <= WellTestRules.CrossCheckTolerance,
            result.CrossCheck.Agrees);
        Assert.Equal(0.02m, WellTestRules.CrossCheckTolerance);

        // The sentence carries both values, so a disagreement would be readable rather than hidden.
        Assert.Contains("psi/cycle", result.CrossCheck.Text, StringComparison.Ordinal);
        Assert.Contains(" md", result.CrossCheck.Text, StringComparison.Ordinal);

        // The cross-check is a Horner fit, so it is not applied to another method and says why.
        var mdh = Analyse(MdhModel());

        Assert.False(mdh.CrossCheck.Available);
        Assert.Contains("not applied", mdh.CrossCheck.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void An_unusable_derivative_is_reported_as_unusable_and_identifies_no_model()
    {
        // SG-08-3, first half: a flat pressure record gives the engine nothing to fit, so the run is refused with a
        // sentence and a reference rather than a result with a meaningless straight line.
        var flat = HornerReadings()
            .Select(reading => reading with { Pressure = 3000m })
            .ToList();

        var refused = new OgWellTestService(() => null, new Quiet())
            .AnalyseAsync(Inputs(flat))
            .GetAwaiter()
            .GetResult();

        Assert.False(refused.Success);
        Assert.Equal(WellTestRules.CodeAnalysisRefused, refused.Reference);
        Assert.Contains("did not converge on this window", refused.Message!, StringComparison.Ordinal);
        Assert.Contains("reference", refused.Message, StringComparison.Ordinal);

        // SG-08-3, second half: a real build-up's derivative is usable and does identify a model.
        var usable = Analyse(Horner());

        Assert.True(usable.DerivativeUsable);
        Assert.NotNull(usable.IdentifiedModel);
        Assert.Contains("derivative usable", usable.DerivativeText, StringComparison.Ordinal);

        // SG-08-3, the display rule: an unusable derivative is printed as unusable and never as a series, and no model is
        // claimed for it. The gateway test stores the same flag, so a saved row cannot read as a diagnostic either.
        var unusable = usable with
        {
            DerivativeUsable = false,
            DerivativeReason = "the pressure derivative is not positive anywhere in the window",
            DerivativePoints = 0,
            IdentifiedModel = null,
        };

        Assert.Contains("UNUSABLE", unusable.DerivativeText, StringComparison.Ordinal);
        Assert.Contains("not positive", unusable.DerivativeText, StringComparison.Ordinal);
        Assert.Null(unusable.IdentifiedModel);
        Assert.Contains("no model was identified", unusable.ModelText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_calculators_stay_in_the_data_layer()
    {
        // AC-5 with a stated substitution: WellTestRenderer needs the chart host these screens do not have, so the
        // straight line and the derivative are printed rather than drawn. The arithmetic is still the owner's.
        var sources = SourceRoot.AllSources();
        var page = File.ReadAllText(sources.Single(file => Path.GetFileName(file) == PageFileName));

        foreach (var calculator in new[] { "BuildUpAnalysis", "DrawdownAnalysis", "DerivativeAnalysis", "WellTestAnalyzer" })
        {
            Assert.DoesNotContain(calculator, page, StringComparison.Ordinal);
        }

        var service = File.ReadAllText(sources.Single(file => Path.GetFileName(file) == "OgWellTest.cs"));

        Assert.Contains("BuildUpAnalysis.", service, StringComparison.Ordinal);
        Assert.Contains("DrawdownAnalysis.", service, StringComparison.Ordinal);
        Assert.Contains("DerivativeAnalysis.", service, StringComparison.Ordinal);
        Assert.Contains("HornerAnalysis.", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Mixed_units_are_refused_with_the_offending_column()
    {
        // The state table's "mixed units - refused with the offending column".
        var time = WellTestRules.Validate(Inputs(HornerReadings()) with { TimeUnit = "min" });

        Assert.Equal(WellTestRules.CodeMixedUnits, time!.Value.Code);
        Assert.Contains("time column", time.Value.Message, StringComparison.Ordinal);
        Assert.Contains("min", time.Value.Message, StringComparison.Ordinal);

        var pressure = WellTestRules.Validate(Inputs(HornerReadings()) with { PressureUnit = "kPa" });

        Assert.Equal(WellTestRules.CodeMixedUnits, pressure!.Value.Code);
        Assert.Contains("pressure column", pressure.Value.Message, StringComparison.Ordinal);

        var rate = WellTestRules.Validate(Inputs(HornerReadings()) with { RateUnit = "bbl/min" });

        Assert.Equal(WellTestRules.CodeMixedUnits, rate!.Value.Code);
        Assert.Contains("rate column", rate.Value.Message, StringComparison.Ordinal);

        Assert.Null(WellTestRules.Validate(Inputs(HornerReadings())));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor_and_a_refusal_is_not_saved()
    {
        var service = new OgWellTestService(() => null, new Quiet());
        var result = Analyse(Horner());

        Assert.Equal(WellTestRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(WellTestRules.CodeActorRequired, (await service.SaveAsync(result, " ")).Reference);
        Assert.Equal(WellTestRules.CodeNoConnection, (await service.SaveAsync(result, "sara")).Reference);
        Assert.Equal(WellTestRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);

        // A result that did not converge is not a result to keep.
        Assert.Equal(
            WellTestRules.CodeNothingToSave,
            (await service.SaveAsync(result with { Converged = false }, "sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    private static OgWellTestResult Analyse(OgWellTestInputs inputs)
    {
        var run = new OgWellTestService(() => null, new Quiet()).AnalyseAsync(inputs).GetAwaiter().GetResult();

        Assert.True(run.Success, run.Message);
        Assert.NotNull(run.Value!.Selected);

        return run.Value.Selected!;
    }

    private static OgWellTestInputs Horner() => Inputs(HornerReadings());

    private static OgWellTestInputs MdhModel() =>
        Inputs(HornerReadings()) with { Method = WellTestAnalyses.Mdh };

    /// <summary>Nine build-up readings on the published Horner line for 50 md.</summary>
    private static IReadOnlyList<OgWellTestReading> HornerReadings()
    {
        const decimal slope = 162.6m * 500m * 1.2m * 1.5m / (ReferencePermeability * 40m);
        const decimal producingTime = 200m;

        return new[] { 1m, 2m, 4m, 8m, 12m, 20m, 30m, 45m, 60m }
            .Select(dt =>
            {
                var horner = (producingTime + dt) / dt;
                var pressure = 3000m - slope * (decimal)Math.Log10((double)horner);

                return new OgWellTestReading(dt, pressure);
            })
            .ToList();
    }

    private static OgWellTestInputs Inputs(IReadOnlyList<OgWellTestReading> readings) =>
        new(
            "WELL-1",
            WellTestAnalyses.BuildUp,
            WellTestAnalyses.Horner,
            TimeUnit: "h",
            PressureUnit: "psia",
            RateUnit: "STB/d",
            Readings: readings,
            WindowStartHours: 1m,
            WindowEndHours: 60m,
            FlowRate: 500m,
            WellboreRadius: 0.35m,
            Thickness: 40m,
            Porosity: 0.2m,
            TotalCompressibility: 0.000012m,
            Viscosity: 1.5m,
            FormationVolumeFactor: 1.2m,
            ProductionTimeHours: 200m,
            InitialReservoirPressure: 3000m);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
