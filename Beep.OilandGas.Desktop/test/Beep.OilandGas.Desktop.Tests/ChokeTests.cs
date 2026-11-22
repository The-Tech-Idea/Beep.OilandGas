using System.Globalization;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-07: the reference cases, the regime and correlation on every result, the erosion threshold's source, the enums,
/// determinism and the validation - the parts that do not need a database. Saving and reloading are proven in the gateway
/// test.
/// </summary>
public sealed class ChokeTests
{
    private const string PageFileName = "ChokePageView.cs";

    [Fact]
    public void The_gas_rate_scales_with_the_throat_area()
    {
        // AC-1 for the single-phase gas path. Rather than re-implement the module's sonic equation, this asserts the
        // invariant the published orifice relation must satisfy: the rate is proportional to the throat area, so doubling
        // the diameter quadruples it. A wrong area or a wrong unit breaks this.
        var half = OgChokeService.Build(Gas() with { ChokeDiameterIn = 0.5m });
        var one = OgChokeService.Build(Gas() with { ChokeDiameterIn = 1.0m });

        Assert.NotNull(half);
        Assert.NotNull(one);

        var ratio = one!.Rate / half!.Rate;

        Assert.InRange(ratio, 3.99m, 4.01m);
        Assert.Equal(half.Regime, one.Regime);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSonic, half.Regime);

        // A plausible magnitude, and a positive rate through a real throat.
        Assert.InRange(half.Rate, 0.01m, 1000m);
        Assert.Contains("MMscf/d", half.RateText, StringComparison.Ordinal);

        // The pressure ratio and the module's critical ratio are both reported, and the regime follows from them.
        Assert.NotNull(half.PressureRatio);
        Assert.NotNull(half.CriticalPressureRatio);
        Assert.Equal(1000m / 3000m, half.PressureRatio!.Value, 4);
        Assert.True(half.PressureRatio < half.CriticalPressureRatio);

        // A downstream pressure above the critical ratio is the subsonic branch, with less gas through the same choke.
        var subsonic = OgChokeService.Build(Gas() with { DownstreamPressure = 2500m });

        Assert.NotNull(subsonic);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSubsonic, subsonic!.Regime);
        Assert.True(subsonic.Rate > 0m);
        Assert.True(subsonic.PressureRatio > subsonic.CriticalPressureRatio);
    }

    [Fact]
    public void The_multiphase_pressures_match_the_published_correlations()
    {
        // AC-1 for the empirical path: each correlation is P = C * Q * R^a / S^b with the published coefficients, so the
        // expected pressure can be computed here from the literature's constants and compared with the module's answer.
        var liquid = 1000m;
        var glr = 600m;
        var diameter = 0.5m;
        var size64 = diameter * 64m;

        var cases = new (string Correlation, double C, double A, double B, string Cite)[]
        {
            (ChokeAnalysisReferenceCodes.CorrelationGilbert, 435.0, 0.546, 1.89, "Gilbert (1954)"),
            (ChokeAnalysisReferenceCodes.CorrelationRos, 17.4, 0.5, 2.0, "Ros (1961)"),
            (ChokeAnalysisReferenceCodes.CorrelationAchong, 3.82, 0.65, 1.88, "Achong (1961)"),
            (ChokeAnalysisReferenceCodes.CorrelationBaxendell, 9.56, 0.546, 1.93, "Baxendell"),
        };

        foreach (var (correlation, c, a, b, cite) in cases)
        {
            var result = OgChokeService.Build(Empirical() with { Correlation = correlation });

            Assert.NotNull(result);

            var expected = (decimal)(c * (double)liquid * Math.Pow((double)glr, a) / Math.Pow((double)size64, b));

            Assert.Equal(expected, result!.Rate, 2);
            Assert.Equal(OgChokeResult.PressureUnit, result.RateUnit);
            Assert.Equal(correlation, result.CorrelationUsed);
            Assert.False(result.CorrelationFellBack, cite);
        }
    }

    [Fact]
    public void The_regime_and_the_correlation_are_on_every_result()
    {
        // AC-2.
        foreach (var correlation in AllCorrelationCodes())
        {
            var inputs = PathOf(correlation) == ChokeCorrelationPath.MultiphaseEmpirical
                ? Empirical() with { Correlation = correlation }
                : Gas() with { Correlation = correlation };

            var result = OgChokeService.Build(inputs);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result!.Regime));
            Assert.False(string.IsNullOrWhiteSpace(result.CorrelationChosen));
            Assert.False(string.IsNullOrWhiteSpace(result.CorrelationUsed));
            Assert.Contains(result.Regime is "SONIC" or "SUBSONIC" ? "sonic" : "subsonic",
                result.RatiosText, StringComparison.Ordinal);
            Assert.Contains("critical", result.RatiosText, StringComparison.Ordinal);
        }

        // The module has no Pilehvari or Sachdeva model: the result says which correlation it actually ran.
        foreach (var missing in new[]
                 {
                     ChokeAnalysisReferenceCodes.CorrelationPilehvari,
                     ChokeAnalysisReferenceCodes.CorrelationSachdeva,
                 })
        {
            var result = OgChokeService.Build(Empirical() with { Correlation = missing });

            Assert.NotNull(result);
            Assert.True(result!.CorrelationFellBack);
            Assert.Equal(missing, result.CorrelationChosen);
            Assert.Equal(ChokeAnalysisReferenceCodes.CorrelationGilbert, result.CorrelationUsed);
            Assert.Contains("no model", result.CorrelationText, StringComparison.Ordinal);
            Assert.Contains(
                result.Notes,
                note => note.Code == "CORRELATION_FALLBACK" && note.Warning);
        }
    }

    [Fact]
    public void The_erosion_threshold_always_names_its_source()
    {
        // AC-3: a limit without a source is not a threshold.
        var erosive = OgChokeService.Build(Gas() with { ChokeDiameterIn = 0.25m, LiquidRate = 4000m, Glr = 900m });

        Assert.NotNull(erosive);
        Assert.NotNull(erosive!.Erosion);

        var erosion = erosive.Erosion!;

        Assert.Equal(OgChokeErosion.Source, erosion.ThresholdSource);
        Assert.Contains("API RP 14E", erosion.ThresholdSource, StringComparison.Ordinal);
        Assert.Contains("API RP 14E", erosion.Describe(), StringComparison.Ordinal);
        Assert.True(erosion.Limit > 0m);
        Assert.True(erosion.Velocity > 0m);
        // The ratio is the engine's own (double) figure; velocity and limit are that ratio's inputs rounded to decimal,
        // so they agree to a tolerance rather than exactly.
        Assert.InRange(Math.Abs(erosion.Velocity / erosion.Limit - erosion.Ratio), 0m, 0.05m);
        Assert.True(erosion.AboveThreshold);
        Assert.Equal("SEVERE", erosion.Risk);
        Assert.Contains(erosive.Notes, note => note.Code == "EROSION_ABOVE_THRESHOLD" && note.Warning);

        // A wide choke at a low rate is under the limit and still names the source.
        var gentle = OgChokeService.Build(Gas() with { ChokeDiameterIn = 1.0m, LiquidRate = 150m, Glr = 300m });

        Assert.NotNull(gentle);
        Assert.NotNull(gentle!.Erosion);
        Assert.False(gentle.Erosion!.AboveThreshold);
        Assert.True(gentle.Erosion.Ratio < 1m);
        Assert.Equal(OgChokeErosion.Source, gentle.Erosion.ThresholdSource);
    }

    [Fact]
    public void The_choke_types_and_regimes_come_from_the_owning_enums()
    {
        // AC-4.
        Assert.Equal(Enum.GetNames<ChokeType>(), Enum.GetNames<ChokeType>().ToList());
        Assert.Contains(ChokeType.Bean.ToString(), Enum.GetNames<ChokeType>());
        Assert.Contains(ChokeType.Adjustable.ToString(), Enum.GetNames<ChokeType>());
        Assert.Contains(nameof(ChokeFlowRegime.Sonic), Enum.GetNames<ChokeFlowRegime>());
        Assert.Contains(nameof(ChokeFlowRegime.Subsonic), Enum.GetNames<ChokeFlowRegime>());

        // Every choke type the enum names is accepted and echoed in the module's own code.
        foreach (var type in Enum.GetNames<ChokeType>())
        {
            var result = OgChokeService.Build(Gas() with { ChokeType = type });

            Assert.NotNull(result);
            Assert.Equal(type.ToUpperInvariant(), result!.ChokeType);
        }

        // An unknown type is refused, naming the enum's names rather than a list of strings.
        var refused = ChokeRules.Validate(Gas() with { ChokeType = "BUTTERFLY" });

        Assert.Equal(ChokeRules.CodeTypeUnknown, refused!.Value.Code);
        Assert.Contains("Bean", refused.Value.Message, StringComparison.Ordinal);

        // The regime on a result is one of the enum's own names, never a free string.
        var gas = OgChokeService.Build(Gas());

        Assert.NotNull(gas);
        // The stored regime is the enum's own name in the module's stored-code casing (SONIC/SUBSONIC), not a free string.
        Assert.Contains(gas!.Regime, Enum.GetNames<ChokeFlowRegime>(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains(gas.RegimeText, new[] { "sonic (critical)", "subsonic" });

        // The view binds the workspace's type list and names no type or regime literal of its own.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        Assert.Contains("workspace.ChokeTypes", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"BEAN\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"SONIC\"", page, StringComparison.Ordinal);
    }

    [Fact]
    public void The_same_inputs_give_the_same_result()
    {
        // AC-5.
        var first = OgChokeService.Build(Empirical());
        var second = OgChokeService.Build(Empirical());

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first!.Describe(), second!.Describe());
        Assert.Equal(first.Rate, second.Rate);
        Assert.Equal(first.Regime, second.Regime);
        Assert.Equal(first.Erosion!.Ratio, second.Erosion!.Ratio);
        Assert.Equal(first.NotesText, second.NotesText);
    }

    [Fact]
    public void A_required_pressure_below_the_downstream_is_reported_not_refused()
    {
        // The correlation can answer with less pressure than the caller's downstream: that says the choke is not the
        // limit at this rate, which is a finding about the design rather than a broken calculation.
        var result = OgChokeService.Build(Empirical() with { Correlation = ChokeAnalysisReferenceCodes.CorrelationRos });

        Assert.NotNull(result);
        Assert.True(result!.Rate < result.DownstreamPressure);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSubsonic, result.Regime);
        Assert.Contains(result.Notes, note => note.Code == "REQUIRED_PRESSURE_BELOW_DOWNSTREAM" && note.Warning);

        // Gilbert needs more than the downstream at the same rate, so it carries no such note.
        var gilbert = OgChokeService.Build(Empirical());

        Assert.NotNull(gilbert);
        Assert.True(gilbert!.Rate > gilbert.DownstreamPressure);
        Assert.DoesNotContain(gilbert.Notes, note => note.Code == "REQUIRED_PRESSURE_BELOW_DOWNSTREAM");
    }

    [Fact]
    public void The_inputs_are_validated_and_a_missing_ratio_is_named()
    {
        Assert.Equal(ChokeRules.CodeWellRequired, ChokeRules.Validate(Gas() with { WellId = " " })!.Value.Code);
        Assert.Equal(
            ChokeRules.CodeCorrelationUnknown,
            ChokeRules.Validate(Gas() with { Correlation = "MAGIC" })!.Value.Code);
        Assert.Equal(ChokeRules.CodeInputRequired, ChokeRules.Validate(Gas() with { ChokeDiameterIn = null })!.Value.Code);

        // The state table's "missing GOR - refused".
        var noRatio = ChokeRules.Validate(Empirical() with { Glr = null, GasRateMscfPerDay = null });

        Assert.Equal(ChokeRules.CodeInputRequired, noRatio!.Value.Code);
        Assert.Contains("gas-liquid ratio", noRatio.Value.Message, StringComparison.Ordinal);
        Assert.Contains("what the correlation is a function of", noRatio.Value.Message, StringComparison.Ordinal);

        // The diameter range is the module's own bounds.
        Assert.Equal(
            ChokeRules.CodeInputInvalid,
            ChokeRules.Validate(Gas() with { ChokeDiameterIn = 5m })!.Value.Code);

        // Downstream at or above upstream leaves nothing to analyse.
        Assert.Equal(
            ChokeRules.CodeInputInvalid,
            ChokeRules.Validate(Gas() with { DownstreamPressure = 3000m })!.Value.Code);

        // The temperature is Fahrenheit on the way in and Rankine inside the gas calculator: its own guard accepts only
        // 450-800 degR, which is about -10 to 340 degF, and that is refused with the range named rather than met as an
        // engine exception.
        Assert.Equal(459.67m, ChokeRules.ToRankine(0m), 2);
        Assert.Equal(659.67m, ChokeRules.ToRankine(200m), 2);

        var cold = ChokeRules.Validate(Gas() with { TemperatureF = -50m });

        Assert.Equal(ChokeRules.CodeTemperatureUnaffordable, cold!.Value.Code);
        Assert.Contains("450", cold.Value.Message, StringComparison.Ordinal);
        Assert.Contains("800", cold.Value.Message, StringComparison.Ordinal);

        var hot = ChokeRules.Validate(Gas() with { TemperatureF = 400m });

        Assert.Equal(ChokeRules.CodeTemperatureUnaffordable, hot!.Value.Code);
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor_and_a_regime_is_required()
    {
        var service = new OgChokeService(() => null, new Quiet());
        var result = OgChokeService.Build(Gas())!;

        Assert.Equal(ChokeRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(ChokeRules.CodeActorRequired, (await service.SaveAsync(result, " ")).Reference);
        Assert.Equal(ChokeRules.CodeNoConnection, (await service.SaveAsync(result, "sara")).Reference);

        // A result with no regime is not a result.
        Assert.Equal(
            ChokeRules.CodeNothingToSave,
            (await service.SaveAsync(result with { Regime = string.Empty }, "sara")).Reference);

        Assert.Equal(ChokeRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);
    }

    // ------------------------------------------------------------------ helpers

    private static IReadOnlyList<string> AllCorrelationCodes() =>
        ChokeCorrelationRegistry.All.Select(info => info.Code).ToList();

    private static ChokeCorrelationPath PathOf(string code) =>
        ChokeCorrelationRegistry.Resolve(code)!.Path;

    /// <summary>A 0.5 in bean with 3,000 psia upstream, 1,000 psia downstream and a gas-specific gravity.</summary>
    private static OgChokeInputs Gas() =>
        new(
            "WELL-1",
            ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase,
            ChokeType.Bean.ToString(),
            ChokeDiameterIn: 0.5m,
            DischargeCoefficient: null,
            UpstreamPressure: 3000m,
            DownstreamPressure: 1000m,
            TemperatureF: 200m,
            GasSpecificGravity: 0.65m,
            ZFactor: 0.9m,
            LiquidRate: 1000m,
            GasRateMscfPerDay: 600m,
            Glr: 600m,
            SandRate: null);

    private static OgChokeInputs Empirical() =>
        Gas() with { Correlation = ChokeAnalysisReferenceCodes.CorrelationGilbert };

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
