using Beep.OilandGas.HydraulicPumps.Exceptions;
using Beep.OilandGas.HydraulicPumps.Validation;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Xunit;

namespace Beep.OilandGas.HydraulicPumps.Tests;

public class HydraulicPumpValidatorTests
{
    [Fact]
    public void ValidateWellProperties_throws_when_BHP_not_greater_than_WHP()
    {
        var bad = new HYDRAULIC_PUMP_WELL_PROPERTIES
        {
            WELL_DEPTH = 5000m,
            TUBING_DIAMETER = 2.375m,
            CASING_DIAMETER = 7m,
            WELLHEAD_PRESSURE = 200m,
            BOTTOM_HOLE_PRESSURE = 100m,
            WELLHEAD_TEMPERATURE = 520m,
            BOTTOM_HOLE_TEMPERATURE = 580m,
            OIL_GRAVITY = 35m,
            WATER_CUT = 0.1m,
            GAS_OIL_RATIO = 200m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            DESIRED_PRODUCTION_RATE = 100m
        };

        Assert.Throws<InvalidWellPropertiesException>(() =>
            HydraulicPumpValidator.ValidateWellProperties(bad));
    }
}
