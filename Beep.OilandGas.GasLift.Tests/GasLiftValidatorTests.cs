using Beep.OilandGas.GasLift.Constants;
using Beep.OilandGas.GasLift.Exceptions;
using Beep.OilandGas.GasLift.Validation;
using Beep.OilandGas.Models.Data.GasLift;
using Xunit;

namespace Beep.OilandGas.GasLift.Tests;

public class GasLiftValidatorTests
{
    private static GAS_LIFT_WELL_PROPERTIES MinimalWell() =>
        new()
        {
            WELL_DEPTH = 10_000m,
            WELLHEAD_PRESSURE = 200m,
            BOTTOM_HOLE_PRESSURE = 2800m,
            WELLHEAD_TEMPERATURE = 560m,
            BOTTOM_HOLE_TEMPERATURE = 660m,
            OIL_GRAVITY = 35m,
            WATER_CUT = 0.1m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            DESIRED_PRODUCTION_RATE = 400m
        };

    [Fact]
    public void ValidateNumberOfValves_ExceedsMaximum_Throws()
    {
        var ex = Assert.Throws<GasLiftParameterOutOfRangeException>(() =>
            GasLiftValidator.ValidateNumberOfValves(GasLiftConstants.MaximumNumberOfValves + 1));

        Assert.Contains("exceeds maximum", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateCalculationParameters_ValidInputs_DoesNotThrow()
    {
        var w = MinimalWell();
        GasLiftValidator.ValidateCalculationParameters(w, 1500m, 5);
    }
}
