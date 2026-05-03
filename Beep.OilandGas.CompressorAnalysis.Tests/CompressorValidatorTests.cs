using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Exceptions;
using Beep.OilandGas.CompressorAnalysis.Validation;
using Beep.OilandGas.CompressorAnalysis.Data;
using Xunit;

namespace Beep.OilandGas.CompressorAnalysis.Tests;

public class CompressorValidatorTests
{
    private static COMPRESSOR_OPERATING_CONDITIONS ValidOperatingConditions() =>
        new()
        {
            SUCTION_PRESSURE = 100m,
            DISCHARGE_PRESSURE = 500m,
            SUCTION_TEMPERATURE = 520m,
            DISCHARGE_TEMPERATURE = 520m,
            GAS_FLOW_RATE = 5000m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            COMPRESSOR_EFFICIENCY = 0.75m,
            MECHANICAL_EFFICIENCY = CompressorConstants.StandardMECHANICAL_EFFICIENCY
        };

    [Fact]
    public void ValidateOperatingConditions_NonPositiveSuction_Throws()
    {
        var oc = ValidOperatingConditions();
        oc.SUCTION_PRESSURE = 0m;
        var ex = Assert.Throws<InvalidOperatingConditionsException>(() => CompressorValidator.ValidateOperatingConditions(oc));
        Assert.Contains("Suction pressure", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateOperatingConditions_DischargeNotAboveSuction_Throws()
    {
        var oc = ValidOperatingConditions();
        oc.DISCHARGE_PRESSURE = oc.SUCTION_PRESSURE;
        Assert.Throws<InvalidOperatingConditionsException>(() => CompressorValidator.ValidateOperatingConditions(oc));
    }

    [Fact]
    public void ValidateOperatingConditions_CompressorEfficiencyAboveOne_Throws()
    {
        var oc = ValidOperatingConditions();
        oc.COMPRESSOR_EFFICIENCY = 1.1m;
        Assert.Throws<InvalidOperatingConditionsException>(() => CompressorValidator.ValidateOperatingConditions(oc));
    }

    [Fact]
    public void ValidateOperatingConditions_CompressionRatioExceedsMaximum_ThrowsOutOfRange()
    {
        var oc = ValidOperatingConditions();
        oc.SUCTION_PRESSURE = 10m;
        oc.DISCHARGE_PRESSURE = 10m * (CompressorConstants.MaximumCompressionRatio + 0.5m);
        var ex = Assert.Throws<CompressorParameterOutOfRangeException>(() => CompressorValidator.ValidateOperatingConditions(oc));
        Assert.Equal("compressionRatio", ex.ParameterName, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateCentrifugalCompressorProperties_PolytropicAboveOne_Throws()
    {
        var oc = ValidOperatingConditions();
        var props = new CENTRIFUGAL_COMPRESSOR_PROPERTIES
        {
            OPERATING_CONDITIONS = oc,
            POLYTROPIC_EFFICIENCY = 1.01m,
            SPECIFIC_HEAT_RATIO = CompressorConstants.StandardSpecificHeatRatio,
            NUMBER_OF_STAGES = 1,
            SPEED = 10000m
        };
        Assert.Throws<InvalidCompressorPropertiesException>(() => CompressorValidator.ValidateCentrifugalCompressorProperties(props));
    }

    [Fact]
    public void ValidateCentrifugalCompressorProperties_NonPositiveSpeed_Throws()
    {
        var oc = ValidOperatingConditions();
        var props = new CENTRIFUGAL_COMPRESSOR_PROPERTIES
        {
            OPERATING_CONDITIONS = oc,
            POLYTROPIC_EFFICIENCY = 0.75m,
            SPECIFIC_HEAT_RATIO = CompressorConstants.StandardSpecificHeatRatio,
            NUMBER_OF_STAGES = 1,
            SPEED = 0m
        };
        Assert.Throws<InvalidCompressorPropertiesException>(() => CompressorValidator.ValidateCentrifugalCompressorProperties(props));
    }

    [Fact]
    public void ValidateReciprocatingCompressorProperties_NonPositiveCylinder_Throws()
    {
        var oc = ValidOperatingConditions();
        var props = new RECIPROCATING_COMPRESSOR_PROPERTIES
        {
            OPERATING_CONDITIONS = oc,
            CYLINDER_DIAMETER = 0m,
            STROKE_LENGTH = 12m,
            ROTATIONAL_SPEED = 300m,
            NUMBER_OF_CYLINDERS = 2,
            VOLUMETRIC_EFFICIENCY = CompressorConstants.StandardVolumetricEfficiency,
            CLEARANCE_FACTOR = CompressorConstants.StandardClearanceFactor
        };
        Assert.Throws<InvalidCompressorPropertiesException>(() => CompressorValidator.ValidateReciprocatingCompressorProperties(props));
    }
}
