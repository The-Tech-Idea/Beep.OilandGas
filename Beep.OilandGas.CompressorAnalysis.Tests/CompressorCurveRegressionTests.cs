using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Data;
using Xunit;

namespace Beep.OilandGas.CompressorAnalysis.Tests;

/// <summary>
/// Regression for surge / performance-map helpers (no SkiaSharp — pure numerics).
/// </summary>
public class CompressorCurveRegressionTests
{
    private static CENTRIFUGAL_COMPRESSOR_PROPERTIES SampleCentrifugal()
    {
        var oc = new COMPRESSOR_OPERATING_CONDITIONS
        {
            COMPRESSOR_OPERATING_CONDITIONS_ID = "curve-oc",
            SUCTION_PRESSURE = 100m,
            DISCHARGE_PRESSURE = 500m,
            SUCTION_TEMPERATURE = 520m,
            DISCHARGE_TEMPERATURE = 520m,
            GAS_FLOW_RATE = 5000m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            GAS_MOLECULAR_WEIGHT = 0.65m * CompressorConstants.AirMolecularWeight,
            COMPRESSOR_EFFICIENCY = 0.75m,
            MECHANICAL_EFFICIENCY = CompressorConstants.StandardMECHANICAL_EFFICIENCY
        };

        return new CENTRIFUGAL_COMPRESSOR_PROPERTIES
        {
            OPERATING_CONDITIONS = oc,
            POLYTROPIC_EFFICIENCY = 0.75m,
            SPECIFIC_HEAT_RATIO = CompressorConstants.StandardSpecificHeatRatio,
            NUMBER_OF_STAGES = 1,
            SPEED = 10000m
        };
    }

    [Fact]
    public void CalculateSurgeMargin_TypicalTrain_MarginAtLeastUnity()
    {
        var props = SampleCentrifugal();
        var (margin, surgeFlow, status) = CentrifugalCompressorCalculator.CalculateSurgeMargin(props);

        Assert.True(margin >= 1m, $"margin={margin}");
        Assert.True(surgeFlow > 0m);
        Assert.False(string.IsNullOrWhiteSpace(status));
    }

    [Fact]
    public void GeneratePerformanceMap_DefaultPoints_ReturnsRequestedCount()
    {
        var props = SampleCentrifugal();
        const int n = 10;
        var map = CentrifugalCompressorCalculator.GeneratePerformanceMap(props, n);

        Assert.Equal(n, map.Count);
        Assert.All(map, pt => Assert.True(pt.FlowRate > 0m));
        Assert.All(map, pt => Assert.True(pt.Head >= 0m));
    }

    [Fact]
    public void CheckOperatingRegion_DesignPoint_TypicallyInAorOrConsistentNotes()
    {
        var props = SampleCentrifugal();
        var (inAor, distSurge, distChoke, notes) = CentrifugalCompressorCalculator.CheckOperatingRegion(props);

        Assert.False(string.IsNullOrWhiteSpace(notes));
        Assert.True(distSurge >= 0m || !inAor);
        Assert.True(distChoke >= 0m || !inAor);
    }
}
