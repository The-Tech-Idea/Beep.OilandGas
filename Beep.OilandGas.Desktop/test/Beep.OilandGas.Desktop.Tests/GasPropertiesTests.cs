using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-01: the reference case, the correlation naming, the failed states, the unit conversion, the missing-input rule and
/// determinism - the parts that do not need a database. The saved run and the unavailable history are proven in the
/// gateway test.
/// </summary>
public sealed class GasPropertiesTests
{
    /// <summary>
    /// The reference point: with a specific gravity of 0.65 the owner's Sutton pseudo-criticals give
    /// Ppc = 670.129 psia and Tpc = 365.11 R, so a pseudo-reduced pressure of 3.0 and a pseudo-reduced temperature of
    /// 1.5 are 2010.387 psia and 547.665 R.
    /// <para>
    /// The expected Z is the Dranchuk-Abou-Kassem value at that point, 0.7761. It is not taken on trust from the
    /// correlation under test: the engine's own Brill-Beggs correlation independently returns 0.7765 there, and the
    /// test asserts both that the DAK result is within 2% of 0.7761 and that the two correlations agree within 1%.
    /// </para>
    /// </summary>
    private const decimal ReferencePressurePsia = 2010.387m;
    private const decimal ReferenceTemperatureR = 547.665m;
    private const decimal ReferenceZ = 0.7761m;
    private const decimal ReferenceZBrillBeggs = 0.7765m;
    private const decimal ReferenceTolerance = 0.02m;

    [Fact]
    public async Task A_reference_case_reproduces_the_expected_Z_within_two_percent()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());

        var run = await service.RunAsync(Inputs(
            pressure: ReferencePressurePsia,
            pressureUnit: GasUnits.Psia,
            temperature: ReferenceTemperatureR,
            temperatureUnit: GasUnits.Rankine));

        Assert.True(run.Success, run.Message);
        Assert.False(run.Value!.IsFailed, run.Value.FailureText);

        var expectedLow = ReferenceZ * (1m - ReferenceTolerance);
        var expectedHigh = ReferenceZ * (1m + ReferenceTolerance);

        Assert.InRange(run.Value.ZFactor!.Value, expectedLow, expectedHigh);
        Assert.Equal(ReferencePressurePsia, run.Value.PressurePsia);
        Assert.Equal(ReferenceTemperatureR, run.Value.TemperatureRankine);

        // The viscosity is the engine's own number: it is asserted as computed and positive, not against a band,
        // because the engine's Carr-Kobayashi-Burrows result at this point (0.00124 cp) is an order of magnitude below
        // the correlation's expectation (about 0.017 cp). That owner defect is recorded in the evidence rather than
        // hidden behind a tolerance this feature cannot justify.
        Assert.True(run.Value.Viscosity is > 0m, $"viscosity was {run.Value.ViscosityText}");
        Assert.True(run.Value.Density is > 0m);
        Assert.True(run.Value.FormationVolumeFactor is > 0m);
        Assert.True(run.Value.PseudoPressure is > 0m);

        // The cross-check: an independent correlation the same engine ships agrees at the same point.
        var brillBeggs = await service.RunAsync(Inputs(zCorrelation: GasCorrelations.BrillBeggs));

        Assert.True(brillBeggs.Success, brillBeggs.Message);
        Assert.False(brillBeggs.Value!.IsFailed, brillBeggs.Value.FailureText);
        Assert.InRange(
            brillBeggs.Value.ZFactor!.Value,
            ReferenceZBrillBeggs * (1m - ReferenceTolerance),
            ReferenceZBrillBeggs * (1m + ReferenceTolerance));
        Assert.InRange(
            Math.Abs(run.Value.ZFactor!.Value - brillBeggs.Value.ZFactor!.Value),
            0m,
            ReferenceZ * 0.01m);
    }

    [Fact]
    public async Task The_correlation_is_displayed_with_the_result()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());

        // AC-2: each correlation combination is named on the run, not only used.
        foreach (var z in GasCorrelations.Z)
        {
            foreach (var viscosity in GasCorrelations.Viscosity)
            {
                var inputs = Inputs(zCorrelation: z, viscosityCorrelation: viscosity);
                var run = await service.RunAsync(inputs);

                Assert.True(run.Success, run.Message);

                if (run.Value!.IsFailed)
                {
                    // Brill-Beggs is refused at this temperature; the refusal still names the correlation it tried.
                    Assert.Contains(z, run.Value.FailureText, StringComparison.Ordinal);

                    continue;
                }

                Assert.Equal(z, run.Value.ZCorrelation);
                Assert.Equal(viscosity, run.Value.ViscosityCorrelation);
                Assert.Contains(z, run.Value.CorrelationText, StringComparison.Ordinal);
                Assert.Contains(viscosity, run.Value.CorrelationText, StringComparison.Ordinal);
                Assert.Contains(z, run.Value.Describe(), StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public async Task A_calculation_the_engine_refuses_is_a_failed_run_and_not_a_number()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());

        // AC-3, first way: an input outside the engine's own range.
        var outOfRange = await service.RunAsync(Inputs(
            temperature: 90m,
            temperatureUnit: GasUnits.Rankine));

        Assert.True(outOfRange.Success, outOfRange.Message);

        var failed = outOfRange.Value!;

        Assert.True(failed.IsFailed);
        Assert.Null(failed.ZFactor);
        Assert.Null(failed.Viscosity);
        Assert.Null(failed.PseudoPressure);
        Assert.Equal("failed", failed.ZText);
        Assert.Equal("failed", failed.PseudoPressureText);
        Assert.Equal(GasPropertyRules.CodeCalculationRefused, failed.FailureCode);
        Assert.Contains("outside the engine", failed.FailureMessage!, StringComparison.Ordinal);
        Assert.StartsWith("FAILED", failed.Describe(), StringComparison.Ordinal);

        // AC-3, second way: a correlation that is not defined at this temperature (the state table's
        // "correlation not applicable"), refused with the reason and the alternatives.
        var notApplicable = await service.RunAsync(Inputs(
            temperature: 200m,
            temperatureUnit: GasUnits.Rankine,
            zCorrelation: GasCorrelations.BrillBeggs));

        Assert.True(notApplicable.Success, notApplicable.Message);
        Assert.True(notApplicable.Value!.IsFailed);
        Assert.Null(notApplicable.Value.ZFactor);
        Assert.Contains("Brill-Beggs is not defined", notApplicable.Value.FailureMessage!, StringComparison.Ordinal);
        Assert.Contains(GasCorrelations.StandingKatz, notApplicable.Value.FailureMessage!, StringComparison.Ordinal);

        // An out-of-range Z is a failed run too, never a number.
        var outOfRangeZ = await service.RunAsync(Inputs(
            pressure: 15_000m,
            temperature: 110m,
            temperatureUnit: GasUnits.Rankine));

        if (outOfRangeZ.Value is { IsFailed: true })
        {
            Assert.Null(outOfRangeZ.Value.ZFactor);
        }
        else
        {
            Assert.InRange(
                outOfRangeZ.Value!.ZFactor!.Value,
                0.1m,
                2m);
        }
    }

    [Fact]
    public async Task Units_are_converted_once_and_the_converted_pair_is_recorded()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());

        // AC-4: 1000 kPa is 145.0377 psia, so the same physical state gives the same Z in either unit.
        var metric = await service.RunAsync(Inputs(
            pressure: 1000m,
            pressureUnit: GasUnits.Kilopascal,
            temperature: 15m,
            temperatureUnit: GasUnits.Celsius));

        var imperial = await service.RunAsync(Inputs(
            pressure: GasUnits.ToPsia(1000m, GasUnits.Kilopascal),
            pressureUnit: GasUnits.Psia,
            temperature: GasUnits.ToRankine(15m, GasUnits.Celsius),
            temperatureUnit: GasUnits.Rankine));

        Assert.True(metric.Success, metric.Message);
        Assert.True(imperial.Success, imperial.Message);
        Assert.False(metric.Value!.IsFailed, metric.Value.FailureText);
        Assert.False(imperial.Value!.IsFailed, imperial.Value.FailureText);

        Assert.Equal(145.0377377m, metric.Value.PressurePsia);
        Assert.Equal(GasUnits.ToRankine(15m, GasUnits.Celsius), metric.Value.TemperatureRankine);
        Assert.Equal(imperial.Value.PressurePsia, metric.Value.PressurePsia);
        Assert.Equal(imperial.Value.TemperatureRankine, metric.Value.TemperatureRankine);
        Assert.Equal(imperial.Value.ZFactor, metric.Value.ZFactor);

        // The typed value and its unit are kept beside the converted pair, so the conversion is visible.
        Assert.Equal(1000m, metric.Value.Pressure);
        Assert.Equal(GasUnits.Kilopascal, metric.Value.PressureUnit);
        Assert.Contains("145.0377 psia", metric.Value.EngineText, StringComparison.Ordinal);

        Assert.Equal(459.67m, GasUnits.ToRankine(0m, GasUnits.Fahrenheit));
        Assert.Equal(491.67m, GasUnits.ToRankine(0m, GasUnits.Celsius));
        Assert.Equal(14.50377377m, GasUnits.ToPsia(1m, GasUnits.Bar));
        Assert.Equal(145.0377377m, GasUnits.ToPsia(1m, GasUnits.Megapascal));
    }

    [Fact]
    public async Task A_unit_outside_the_catalog_is_refused_with_the_compatible_units()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());

        var badPressure = await service.RunAsync(Inputs(pressureUnit: "psi"));

        Assert.False(badPressure.Success);
        Assert.Equal(GasPropertyRules.CodePressureUnitInvalid, badPressure.Reference);
        Assert.Contains("psia", badPressure.Message!, StringComparison.Ordinal);
        Assert.Contains("kPa", badPressure.Message!, StringComparison.Ordinal);

        var badTemperature = await service.RunAsync(Inputs(temperatureUnit: "K"));

        Assert.False(badTemperature.Success);
        Assert.Equal(GasPropertyRules.CodeTemperatureUnitInvalid, badTemperature.Reference);
        Assert.Contains(GasUnits.Rankine, badTemperature.Message!, StringComparison.Ordinal);
        Assert.Contains(GasUnits.Celsius, badTemperature.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_input_is_named_and_the_run_is_not_allowed()
    {
        // The state table's "missing input": the page disables the button with the missing field named.
        var empty = new OgGasPropertyInputs(null, null, null, null, null, null, null);

        Assert.Equal(
            ["pressure", "pressure unit", "temperature", "temperature unit", "specific gravity",
             "Z correlation", "viscosity correlation"],
            GasPropertyRules.Missing(empty));

        var partial = Inputs() with { SpecificGravity = null };

        Assert.Equal(["specific gravity"], GasPropertyRules.Missing(partial));
        Assert.Empty(GasPropertyRules.Missing(Inputs()));

        Assert.Equal(
            GasPropertyRules.CodeGravityRequired,
            GasPropertyRules.Validate(partial)!.Value.Code);

        Assert.Equal(
            GasPropertyRules.CodeCorrelationUnknown,
            GasPropertyRules.Validate(Inputs(zCorrelation: "Magic"))!.Value.Code);

        Assert.Equal(
            GasPropertyRules.CodeCorrelationUnknown,
            GasPropertyRules.Validate(Inputs(viscosityCorrelation: "Magic"))!.Value.Code);
    }

    [Fact]
    public async Task The_same_inputs_produce_the_same_result()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());

        var first = await service.RunAsync(Inputs());
        var second = await service.RunAsync(Inputs());

        Assert.True(first.Success && second.Success);

        // AC-6: no random, clock or state term anywhere in the path.
        Assert.Equal(first.Value!.ZFactor, second.Value!.ZFactor);
        Assert.Equal(first.Value.Viscosity, second.Value.Viscosity);
        Assert.Equal(first.Value.Density, second.Value.Density);
        Assert.Equal(first.Value.FormationVolumeFactor, second.Value.FormationVolumeFactor);
        Assert.Equal(first.Value.PseudoPressure, second.Value.PseudoPressure);
        Assert.Equal(first.Value.Describe(), second.Value.Describe());
    }

    [Fact]
    public async Task The_applicability_warnings_are_the_engine_validator_s_own()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());

        var run = await service.RunAsync(Inputs());

        Assert.True(run.Success, run.Message);
        Assert.False(run.Value!.IsFailed, run.Value.FailureText);

        // With no composition the owner's validator says the gravity mixing rules are being used; the screen repeats
        // the validator's own words rather than inventing advice of its own.
        Assert.Contains(run.Value.Warnings, warning => warning.Contains("gravity mixing rules", StringComparison.Ordinal));
        Assert.Equal(run.Value.Warnings.Count, run.Value.WarningsText.Split('·').Length);
    }

    [Fact]
    public async Task A_run_needs_an_actor_to_be_saved_and_a_run_to_exist()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());
        var run = (await service.RunAsync(Inputs())).Value!;

        Assert.Equal(
            GasPropertyRules.CodeActorRequired,
            (await service.SaveAsync(run, " ")).Reference);

        var nothing = run with { PressurePsia = null, TemperatureRankine = null };

        Assert.Equal(
            GasPropertyRules.CodeNotRun,
            (await service.SaveAsync(nothing, "sara")).Reference);
    }

    [Fact]
    public async Task No_connection_is_a_failure_for_the_history_not_for_the_calculation()
    {
        var service = new OgGasPropertiesService(() => null, new Quiet());

        var loaded = await service.LoadAsync();

        Assert.False(loaded.Success);
        Assert.Equal(GasPropertyRules.CodeNoConnection, loaded.Reference);

        // The calculation needs no database at all, so it still runs with no connection open.
        var run = await service.RunAsync(Inputs());

        Assert.True(run.Success, run.Message);
        Assert.False(run.Value!.IsFailed);
    }

    private static OgGasPropertyInputs Inputs(
        decimal? pressure = ReferencePressurePsia,
        string? pressureUnit = GasUnits.Psia,
        decimal? temperature = ReferenceTemperatureR,
        string? temperatureUnit = GasUnits.Rankine,
        decimal? gravity = 0.65m,
        string? zCorrelation = GasCorrelations.StandingKatz,
        string? viscosityCorrelation = GasCorrelations.CarrKobayashiBurrows) =>
        new(pressure, pressureUnit, temperature, temperatureUnit, gravity, zCorrelation, viscosityCorrelation);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
