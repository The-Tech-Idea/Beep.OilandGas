using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Data;
using Xunit;

namespace Beep.OilandGas.CompressorAnalysis.Tests;

/// <summary>
/// Locked numerical vectors for regression — same inputs must yield stable compression ratio and positive brake HP.
/// </summary>
public class CompressorCalculatorGoldenVectorTests
{
    [Fact]
    public void CentrifugalCalculatePower_VectorA_CompressionRatioAndPowerStable()
    {
        var oc = new COMPRESSOR_OPERATING_CONDITIONS
        {
            COMPRESSOR_OPERATING_CONDITIONS_ID = "gv-oc",
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

        var props = new CENTRIFUGAL_COMPRESSOR_PROPERTIES
        {
            OPERATING_CONDITIONS = oc,
            POLYTROPIC_EFFICIENCY = 0.75m,
            SPECIFIC_HEAT_RATIO = CompressorConstants.StandardSpecificHeatRatio,
            NUMBER_OF_STAGES = 1,
            SPEED = 10000m
        };

        var r = CentrifugalCompressorCalculator.CalculatePower(props);

        Assert.Equal(5m, r.COMPRESSION_RATIO);
        Assert.True(r.BRAKE_HORSEPOWER > 100m, $"Expected meaningful brake HP, got {r.BRAKE_HORSEPOWER}");
        Assert.True(r.DISCHARGE_TEMPERATURE >= oc.SUCTION_TEMPERATURE);
    }

    [Fact]
    public void ReciprocatingCalculatePower_VectorA_PositiveBrakeHp()
    {
        var oc = new COMPRESSOR_OPERATING_CONDITIONS
        {
            COMPRESSOR_OPERATING_CONDITIONS_ID = "gv-oc-r",
            SUCTION_PRESSURE = 100m,
            DISCHARGE_PRESSURE = 400m,
            SUCTION_TEMPERATURE = 520m,
            DISCHARGE_TEMPERATURE = 520m,
            GAS_FLOW_RATE = 3000m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            GAS_MOLECULAR_WEIGHT = 0.65m * CompressorConstants.AirMolecularWeight,
            COMPRESSOR_EFFICIENCY = 0.85m,
            MECHANICAL_EFFICIENCY = CompressorConstants.StandardMECHANICAL_EFFICIENCY
        };

        var props = new RECIPROCATING_COMPRESSOR_PROPERTIES
        {
            OPERATING_CONDITIONS = oc,
            CYLINDER_DIAMETER = 10m,
            STROKE_LENGTH = 12m,
            ROTATIONAL_SPEED = 300m,
            NUMBER_OF_CYLINDERS = 2,
            VOLUMETRIC_EFFICIENCY = CompressorConstants.StandardVolumetricEfficiency,
            CLEARANCE_FACTOR = CompressorConstants.StandardClearanceFactor
        };

        var r = ReciprocatingCompressorCalculator.CalculatePower(props);

        Assert.Equal(4m, r.COMPRESSION_RATIO);
        Assert.True(r.BRAKE_HORSEPOWER > 0m);
    }

    [Fact]
    public void CalculateRequiredPressure_VectorA_RespectsMaxPowerAndRaisesDischarge()
    {
        var oc = new COMPRESSOR_OPERATING_CONDITIONS
        {
            COMPRESSOR_OPERATING_CONDITIONS_ID = "gv-p",
            SUCTION_PRESSURE = 100m,
            DISCHARGE_PRESSURE = 500m,
            SUCTION_TEMPERATURE = 520m,
            DISCHARGE_TEMPERATURE = 520m,
            GAS_FLOW_RATE = 2000m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            GAS_MOLECULAR_WEIGHT = 0.65m * CompressorConstants.AirMolecularWeight,
            COMPRESSOR_EFFICIENCY = 0.75m,
            MECHANICAL_EFFICIENCY = CompressorConstants.StandardMECHANICAL_EFFICIENCY
        };

        const decimal maxHp = 500m;
        var r = CompressorPressureCalculator.CalculateRequiredPressure(
            oc,
            requiredFlowRate: oc.GAS_FLOW_RATE,
            maxPower: maxHp,
            COMPRESSOR_EFFICIENCY: 0.75m);

        Assert.NotNull(r.REQUIRED_DISCHARGE_PRESSURE);
        Assert.True(r.REQUIRED_DISCHARGE_PRESSURE > oc.SUCTION_PRESSURE);
        Assert.NotNull(r.REQUIRED_POWER);
        Assert.True(r.REQUIRED_POWER <= maxHp);
        Assert.NotNull(r.COMPRESSION_RATIO);
        Assert.True(r.COMPRESSION_RATIO >= 1m);
    }
}
