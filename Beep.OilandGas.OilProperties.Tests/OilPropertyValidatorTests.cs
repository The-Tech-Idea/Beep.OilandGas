using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.OilProperties;
using Beep.OilandGas.OilProperties.Exceptions;
using Beep.OilandGas.OilProperties.Validation;
using Xunit;

namespace Beep.OilandGas.OilProperties.Tests;

public class OilPropertyValidatorTests
{
    [Fact]
    public void ValidateOilPropertyConditions_throws_when_temperature_below_rankine_floor()
    {
        var c = new OIL_PROPERTY_CONDITIONS
        {
            PRESSURE = 2000m,
            TEMPERATURE = 400m,
            API_GRAVITY = 35m,
            GAS_SPECIFIC_GRAVITY = 0.65m
        };

        Assert.Throws<InvalidOilPropertyConditionsException>(() =>
            OilPropertyValidator.ValidateOilPropertyConditions(c));
    }

    [Fact]
    public void ValidateOilPropertyConditions_passes_typical_reservoir_rankine()
    {
        var c = new OIL_PROPERTY_CONDITIONS
        {
            PRESSURE = 2000m,
            TEMPERATURE = 620m,
            API_GRAVITY = 35m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            SOLUTION_GAS_OIL_RATIO = 500m
        };

        OilPropertyValidator.ValidateOilPropertyConditions(c);
    }

    [Fact]
    public void ValidateOilPropertyConditions_allows_null_optional_gor_and_bubble()
    {
        var c = new OIL_PROPERTY_CONDITIONS
        {
            PRESSURE = 2500m,
            TEMPERATURE = 620m,
            API_GRAVITY = 30m,
            GAS_SPECIFIC_GRAVITY = 0.72m,
            SOLUTION_GAS_OIL_RATIO = null,
            BUBBLE_POINT_PRESSURE = null
        };

        OilPropertyValidator.ValidateOilPropertyConditions(c);
    }

    [Fact]
    public void ValidateSimpleScreeningInputs_throws_when_temperature_too_cold()
    {
        Assert.Throws<InvalidOilPropertyConditionsException>(() =>
            OilPropertyValidator.ValidateSimpleScreeningInputs(2000m, 400m, 35m));
    }

    [Fact]
    public void ValidateCompositionForPvtSweeps_throws_when_gor_negative()
    {
        var c = new OilComposition { OilGravity = 35m, GasOilRatio = -1m };
        Assert.Throws<OilPropertyParameterOutOfRangeException>(() =>
            OilPropertyValidator.ValidateCompositionForPvtSweeps(c, 500m, 3000m, 620m, 680m));
    }

    [Fact]
    public void ValidateOilPropertyConditions_throws_when_bubble_point_out_of_range()
    {
        var c = new OIL_PROPERTY_CONDITIONS
        {
            PRESSURE = 2000m,
            TEMPERATURE = 620m,
            API_GRAVITY = 35m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            BUBBLE_POINT_PRESSURE = 20_000m
        };

        Assert.Throws<OilPropertyParameterOutOfRangeException>(() =>
            OilPropertyValidator.ValidateOilPropertyConditions(c));
    }
}
