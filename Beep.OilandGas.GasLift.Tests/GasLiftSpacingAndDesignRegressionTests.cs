using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.GasLift.Constants;
using Beep.OilandGas.Models.Data.GasLift;
using Xunit;

namespace Beep.OilandGas.GasLift.Tests;

/// <summary>
/// Regression locks for spacing geometry and valve design invariants (golden-style for equal-depth spacing; design path vs Brill–Beggs Z).
/// </summary>
public class GasLiftSpacingAndDesignRegressionTests
{
    private static GAS_LIFT_WELL_PROPERTIES SampleWell(
        decimal wellDepth = 10_000m,
        decimal wellheadPressure = 200m,
        decimal gasInjectionPressure = 1500m)
    {
        return new GAS_LIFT_WELL_PROPERTIES
        {
            WELL_DEPTH = wellDepth,
            WELLHEAD_PRESSURE = wellheadPressure,
            BOTTOM_HOLE_PRESSURE = 2800m,
            WELLHEAD_TEMPERATURE = 560m,
            BOTTOM_HOLE_TEMPERATURE = 660m,
            OIL_GRAVITY = 35m,
            WATER_CUT = 0.1m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            GAS_OIL_RATIO = 800m,
            DESIRED_PRODUCTION_RATE = 400m
        };
    }

    [Fact]
    public void CalculateEqualDepthSpacing_ValveDepths_AreUniformGolden()
    {
        const int n = 5;
        const decimal depth = 10_000m;
        var well = SampleWell(depth);
        var spacing = GasLiftValveSpacingCalculator.CalculateEqualDepthSpacing(well, 1500m, n);

        Assert.Equal(n, spacing.ValveDepths.Count);
        var step = depth / n;
        for (var i = 0; i < n; i++)
            Assert.Equal((i + 1) * step, spacing.ValveDepths[i]);

        Assert.Equal(spacing.ValveDepths.Last() - spacing.ValveDepths.First(), spacing.TotalDepthCoverage);
    }

    [Fact]
    public void CalculateValveSpacing_IsDeterministic_ForFixedInputs()
    {
        var well = SampleWell();
        const decimal inj = 1500m;
        const int n = 5;

        var a = GasLiftValveSpacingCalculator.CalculateValveSpacing(well, inj, n);
        var b = GasLiftValveSpacingCalculator.CalculateValveSpacing(well, inj, n);

        Assert.Equal(a.ValveDepths, b.ValveDepths);
        Assert.Equal(a.OpeningPressures, b.OpeningPressures);
        Assert.Equal(a.NumberOfValves, b.NumberOfValves);
    }

    [Fact]
    public void DesignValvesUS_ProducesValvesWithClosingPressureRatio_AndOrderedDepths()
    {
        var well = SampleWell();
        var design = GasLiftValveDesignCalculator.DesignValvesUS(well, 1500m, 5);

        Assert.Equal(5, design.Valves.Count);
        decimal prev = 0m;
        foreach (var v in design.Valves)
        {
            Assert.True(v.DEPTH > prev, "Valve depths should increase.");
            prev = v.DEPTH;
            Assert.True(v.PORT_SIZE > 0m);
            Assert.Equal(GasLiftConstants.StandardClosingPressureRatio * v.OPENING_PRESSURE, v.CLOSING_PRESSURE, 4);
            Assert.True(v.GAS_INJECTION_RATE >= 0m);
        }

        Assert.True(design.TOTAL_GAS_INJECTION_RATE >= 0m);
    }

    [Fact]
    public void CalculateEqualPressureDropSpacing_DepthsDoNotExceedWellDepth()
    {
        var well = SampleWell(8000m);
        var r = GasLiftValveSpacingCalculator.CalculateEqualPressureDropSpacing(well, 1500m, 8);

        Assert.NotEmpty(r.ValveDepths);
        Assert.All(r.ValveDepths, d => Assert.True(d <= well.WELL_DEPTH + 0.0001m));
    }
}
