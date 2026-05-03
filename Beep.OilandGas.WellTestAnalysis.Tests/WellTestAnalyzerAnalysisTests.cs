using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Constants;
using Xunit;

namespace Beep.OilandGas.WellTestAnalysis.Tests;

/// <summary>
/// Smoke and synthetic-vector tests for <see cref="WellTestAnalyzer"/> analysis paths.
/// </summary>
public class WellTestAnalyzerAnalysisTests
{
    private static WELL_TEST_DATA BaseOilTestData(string testType, decimal? productionTimeHours)
    {
        var data = new WELL_TEST_DATA
        {
            FLOW_RATE = 1000m,
            WELLBORE_RADIUS = 0.25m,
            FORMATION_THICKNESS = 50m,
            POROSITY = 0.20m,
            TOTAL_COMPRESSIBILITY = 1e-5m,
            OIL_VISCOSITY = 1.5m,
            OIL_FORMATION_VOLUME_FACTOR = 1.2m,
            RESERVOIR_TEMPERATURE = 200m,
            TEST_TYPE = testType,
            PRODUCTION_TIME = productionTimeHours
        };
        return data;
    }

    [Fact]
    public void AnalyzeHorner_perfect_semilog_Horner_line_has_high_r_squared_and_finite_permeability()
    {
        const double tp = 120.0;
        const double slopePsiPerLogCycle = 55.0;
        const double interceptPsi = 2850.0;

        var times = new List<double> { 0.1, 0.15, 0.25, 0.4, 0.6, 1, 1.5, 2.5, 4, 6, 10, 15, 25, 40, 70, 120 };
        var pressures = times.Select(t =>
        {
            var hornerRatio = (tp + t) / t;
            return interceptPsi + slopePsiPerLogCycle * Math.Log10(hornerRatio);
        }).ToList();

        var data = BaseOilTestData(WellTestType.BuildUp.ToString(), (decimal)tp);
        data.Time = times;
        data.Pressure = pressures;

        var result = WellTestAnalyzer.AnalyzeBuildUp(data);

        var expectedK = (decimal)(WellTestConstants.HornerOilPermeabilityFactor * 1000.0 * 1.2 * 1.5 / (slopePsiPerLogCycle * 50.0));
        var tol = expectedK * 0.14m;

        Assert.True(result.R_SQUARED >= 0.92m, $"Expected high R², got {result.R_SQUARED}");
        Assert.InRange(result.PERMEABILITY, expectedK - tol, expectedK + tol);
    }

    [Fact]
    public void AnalyzeDrawdown_perfect_semilog_line_completes_with_finite_permeability()
    {
        const double mDrawdown = 38.0;
        const double pi = 3050.0;

        var times = new List<double> { 1, 1.5, 2, 3, 4, 6, 8, 11, 15, 22, 35, 55, 90, 150, 280, 500 };
        var pressures = times.Select(t => pi - mDrawdown * Math.Log10(t)).ToList();

        var data = BaseOilTestData(WellTestType.Drawdown.ToString(), productionTimeHours: null);
        data.INITIAL_RESERVOIR_PRESSURE = (decimal)pi;
        data.Time = times;
        data.Pressure = pressures;

        var result = WellTestAnalyzer.AnalyzeDrawdown(data);

        var expectedK = (decimal)(WellTestConstants.HornerOilPermeabilityFactor * 1000.0 * 1.2 * 1.5 / (mDrawdown * 50.0));
        var tol = expectedK * 0.14m;

        Assert.True(result.R_SQUARED >= 0.90m, $"Expected high R², got {result.R_SQUARED}");
        Assert.InRange(result.PERMEABILITY, expectedK - tol, expectedK + tol);
    }

    [Fact]
    public void AnalyzeGasBuildUp_distinct_pressures_completes_without_throw()
    {
        const double tp = 240.0;
        var times = new List<double> { 0.2, 0.4, 0.7, 1, 1.5, 2.5, 4, 7, 12, 20, 35, 60, 100, 160 };
        var pressures = new List<double> { 4200, 4150, 4100, 4050, 3980, 3900, 3820, 3720, 3600, 3450, 3280, 3100, 2920, 2750 };

        var data = BaseOilTestData(WellTestAnalysisWellKnown.AnalysisClassification.BuildUp, (decimal)tp);
        data.Time = times;
        data.Pressure = pressures;
        data.FLOW_RATE = 2500m;

        const double gasGravity = 0.65;
        const double reservoirTempRankine = 200.0 + 459.67;

        var result = WellTestAnalyzer.AnalyzeGasBuildUp(data, gasGravity, reservoirTempRankine);

        Assert.True(result.PERMEABILITY > 0.01m);
        Assert.Equal(WellTestAnalysisWellKnown.ResultAnalysisMethodLabel.GasPseudoPressureHorner, result.ANALYSIS_METHOD);
    }
}
