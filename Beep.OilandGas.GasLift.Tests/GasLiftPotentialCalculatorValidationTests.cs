using System;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.Models.Data.GasLift;
using Xunit;

namespace Beep.OilandGas.GasLift.Tests;

public class GasLiftPotentialCalculatorValidationTests
{
    private static GAS_LIFT_WELL_PROPERTIES MinimalWell() => new()
    {
        WELL_DEPTH = 10_000m,
        WELLHEAD_PRESSURE = 200m,
        BOTTOM_HOLE_PRESSURE = 2500m,
        WELLHEAD_TEMPERATURE = 560m,
        BOTTOM_HOLE_TEMPERATURE = 660m,
        OIL_GRAVITY = 35m,
        WATER_CUT = 0.1m,
        GAS_SPECIFIC_GRAVITY = 0.65m,
        GAS_OIL_RATIO = 500m,
        DESIRED_PRODUCTION_RATE = 400m
    };

    [Fact]
    public void AnalyzeGasLiftPotential_ThrowsWhenNumberOfPointsBelowTwo()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            GasLiftPotentialCalculator.AnalyzeGasLiftPotential(MinimalWell(), 100m, 500m, 1, default));
        Assert.Equal("numberOfPoints", ex.ParamName);
    }

    [Fact]
    public void AnalyzeGasLiftPotential_ThrowsWhenMinExceedsMax()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            GasLiftPotentialCalculator.AnalyzeGasLiftPotential(MinimalWell(), 600m, 500m, 10, default));
        Assert.Equal("minGasInjectionRate", ex.ParamName);
    }
}
