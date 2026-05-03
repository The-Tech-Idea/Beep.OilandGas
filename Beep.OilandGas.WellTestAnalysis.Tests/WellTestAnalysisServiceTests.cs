using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Constants;
using Beep.OilandGas.WellTestAnalysis.Services;
using Xunit;

namespace Beep.OilandGas.WellTestAnalysis.Tests;

public class WellTestAnalysisServiceTests
{
    [Fact]
    public async Task AnalyzeDrawdownAsync_delegates_to_analyzer_and_sets_audit_fields()
    {
        const double mDrawdown = 38.0;
        const double pi = 3050.0;
        var times = new List<double> { 1, 1.5, 2, 3, 4, 6, 8, 11, 15, 22, 35, 55, 90, 150, 280, 500 };
        var pressures = times.Select(t => pi - mDrawdown * Math.Log10(t)).ToList();

        var testData = new WELL_TEST_DATA
        {
            FLOW_RATE = 1000m,
            WELLBORE_RADIUS = 0.25m,
            FORMATION_THICKNESS = 50m,
            POROSITY = 0.20m,
            TOTAL_COMPRESSIBILITY = 1e-5m,
            OIL_VISCOSITY = 1.5m,
            OIL_FORMATION_VOLUME_FACTOR = 1.2m,
            RESERVOIR_TEMPERATURE = 200m,
            TEST_TYPE = WellTestType.Drawdown.ToString(),
            INITIAL_RESERVOIR_PRESSURE = (decimal)pi,
            Time = times,
            Pressure = pressures
        };

        var svc = new WellTestAnalysisService(logger: null);
        var result = await svc.AnalyzeDrawdownAsync("01-001-00001", testData, "test-user");

        Assert.False(string.IsNullOrEmpty(result.ANALYSIS_ID));
        Assert.Equal("01-001-00001", result.WELL_UWI);
        Assert.Equal("test-user", result.ANALYSIS_BY_USER);

        var expectedK = (decimal)(WellTestConstants.HornerOilPermeabilityFactor * 1000.0 * 1.2 * 1.5 / (mDrawdown * 50.0));
        var tol = expectedK * 0.14m;
        Assert.InRange(result.PERMEABILITY, expectedK - tol, expectedK + tol);
    }

    [Fact]
    public async Task ValidateTestDataAsync_returns_passed_when_data_valid()
    {
        var testData = new WELL_TEST_DATA
        {
            FLOW_RATE = 1000m,
            WELLBORE_RADIUS = 0.25m,
            FORMATION_THICKNESS = 50m,
            POROSITY = 0.20m,
            TOTAL_COMPRESSIBILITY = 1e-5m,
            OIL_VISCOSITY = 1.5m,
            OIL_FORMATION_VOLUME_FACTOR = 1.2m,
            RESERVOIR_TEMPERATURE = 200m,
            TEST_TYPE = WellTestType.BuildUp.ToString(),
            PRODUCTION_TIME = 100m,
            Time = [0.1, 0.2, 0.5, 1, 2, 5],
            Pressure = [3000, 2995, 2990, 2985, 2980, 2975]
        };

        var svc = new WellTestAnalysisService(logger: null);
        var vr = await svc.ValidateTestDataAsync("01-001-00001", testData);

        Assert.True(vr.IsValid);
        Assert.Equal(1.0, vr.DATA_QUALITY_SCORE);
        Assert.Equal("Passed", vr.DataQualityRating);
    }

    [Fact]
    public async Task ValidateTestDataAsync_returns_failed_without_throwing_when_data_invalid()
    {
        var testData = new WELL_TEST_DATA
        {
            FLOW_RATE = 1000m,
            WELLBORE_RADIUS = 0.25m,
            FORMATION_THICKNESS = 50m,
            POROSITY = 0.20m,
            TOTAL_COMPRESSIBILITY = 1e-5m,
            OIL_VISCOSITY = 1.5m,
            OIL_FORMATION_VOLUME_FACTOR = 1.2m,
            RESERVOIR_TEMPERATURE = 200m,
            TEST_TYPE = WellTestType.BuildUp.ToString(),
            PRODUCTION_TIME = 100m,
            Time = [1, 2, 2, 4],
            Pressure = [3000, 2990, 2980, 2970]
        };

        var svc = new WellTestAnalysisService(logger: null);
        var vr = await svc.ValidateTestDataAsync("01-001-00001", testData);

        Assert.False(vr.IsValid);
        Assert.NotEmpty(vr.Errors);
    }

    [Fact]
    public async Task SaveAnalysisResultAsync_throws_NotImplementedException()
    {
        var svc = new WellTestAnalysisService(logger: null);
        var result = new WELL_TEST_ANALYSIS_RESULT { ANALYSIS_ID = "a1" };

        await Assert.ThrowsAsync<NotImplementedException>(() =>
            svc.SaveAnalysisResultAsync("01-001-00001", result, "u1"));
    }

    [Fact]
    public async Task PerformTypeCurveMatchingAsync_throws_NotImplementedException()
    {
        var testData = new WELL_TEST_DATA { TEST_TYPE = WellTestType.BuildUp.ToString(), Time = [1, 2, 3], Pressure = [1, 2, 3] };
        var svc = new WellTestAnalysisService(logger: null);

        await Assert.ThrowsAsync<NotImplementedException>(() =>
            svc.PerformTypeCurveMatchingAsync("01-001-00001", testData, "u1"));
    }
}
