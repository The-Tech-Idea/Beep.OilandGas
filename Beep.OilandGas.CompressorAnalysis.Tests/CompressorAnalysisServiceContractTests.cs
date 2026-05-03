using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Services;
using Beep.OilandGas.CompressorAnalysis.Data;
using Xunit;

namespace Beep.OilandGas.CompressorAnalysis.Tests;

/// <summary>
/// Smoke tests for <see cref="CompressorAnalysisService"/> implementing <see cref="Beep.OilandGas.CompressorAnalysis.Core.Interfaces.ICompressorAnalysisService"/>.
/// </summary>
public class CompressorAnalysisServiceContractTests
{
    private static COMPRESSOR_OPERATING_CONDITIONS BaseConditions() =>
        new()
        {
            COMPRESSOR_OPERATING_CONDITIONS_ID = "OC1",
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

    [Fact]
    public async Task CalculateCentrifugalPowerAsync_ReturnsPositivePower()
    {
        var svc = new CompressorAnalysisService(logger: null);
        var conditions = BaseConditions();
        var props = new CENTRIFUGAL_COMPRESSOR_PROPERTIES
        {
            OPERATING_CONDITIONS = conditions,
            POLYTROPIC_EFFICIENCY = conditions.COMPRESSOR_EFFICIENCY,
            SPECIFIC_HEAT_RATIO = CompressorConstants.StandardSpecificHeatRatio,
            NUMBER_OF_STAGES = 1,
            SPEED = 10000m
        };

        var result = await svc.CalculateCentrifugalPowerAsync(props);

        Assert.True(result.BRAKE_HORSEPOWER > 0 || result.THEORETICAL_POWER > 0,
            "Expected positive power for lifting discharge above suction.");
    }

    [Fact]
    public async Task CalculateRequiredPressureAsync_ReturnsDischargeAboveSuction()
    {
        var svc = new CompressorAnalysisService(logger: null);
        var conditions = BaseConditions();

        var pressure = await svc.CalculateRequiredPressureAsync(conditions, conditions.GAS_FLOW_RATE);

        Assert.NotNull(pressure.REQUIRED_DISCHARGE_PRESSURE);
        Assert.True(pressure.REQUIRED_DISCHARGE_PRESSURE > conditions.SUCTION_PRESSURE);
    }
}
