using Beep.OilandGas.HydraulicPumps.Calculations;
using Beep.OilandGas.HydraulicPumps.Exceptions;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Xunit;

namespace Beep.OilandGas.HydraulicPumps.Tests;

public class HydraulicPistonPumpCalculatorTests
{
    [Fact]
    public void CalculatePerformance_screening_case_matches_regression()
    {
        var well = HydraulicJetPumpCalculatorTests.SampleWell();
        var piston = new HYDRAULIC_PISTON_PUMP_PROPERTIES
        {
            PISTON_DIAMETER = 2.0m,
            ROD_DIAMETER = 0.875m,
            STROKE_LENGTH = 48m,
            STROKES_PER_MINUTE = 12m,
            POWER_FLUID_PRESSURE = 2000m,
            POWER_FLUID_RATE = 400m,
            POWER_FLUID_SPECIFIC_GRAVITY = 1.0m
        };

        var r = HydraulicPistonPumpCalculator.CalculatePerformance(well, piston);

        Assert.Equal(268.5600m, decimal.Round(r.PUMP_DISPLACEMENT, 4));
        Assert.Equal(208.2528m, decimal.Round(r.PRODUCTION_RATE, 4));
        Assert.Equal(0.775442m, decimal.Round(r.VOLUMETRIC_EFFICIENCY, 6));
        Assert.Equal(245.0033m, decimal.Round(r.POWER_FLUID_CONSUMPTION, 4));
        Assert.Equal(500.0000m, decimal.Round(r.PUMP_INTAKE_PRESSURE, 4));
        Assert.Equal(3101.0337m, decimal.Round(r.PUMP_DISCHARGE_PRESSURE, 4));
        Assert.Equal(8.3383m, decimal.Round(r.POWER_FLUID_HORSEPOWER, 4));
        Assert.Equal(9.2175m, decimal.Round(r.HYDRAULIC_HORSEPOWER, 4));
        Assert.Equal(1.0m, decimal.Round(r.SYSTEM_EFFICIENCY, 6));
    }

    [Fact]
    public void CalculatePerformance_with_validateInputs_throws_when_casing_diameter_invalid()
    {
        var well = HydraulicJetPumpCalculatorTests.SampleWell();
        well.CASING_DIAMETER = 0m;
        var piston = new HYDRAULIC_PISTON_PUMP_PROPERTIES
        {
            PISTON_DIAMETER = 2.0m,
            ROD_DIAMETER = 0.875m,
            STROKE_LENGTH = 48m,
            STROKES_PER_MINUTE = 12m,
            POWER_FLUID_PRESSURE = 2000m,
            POWER_FLUID_RATE = 400m,
            POWER_FLUID_SPECIFIC_GRAVITY = 1.0m
        };

        Assert.Throws<InvalidWellPropertiesException>(() =>
            HydraulicPistonPumpCalculator.CalculatePerformance(well, piston, validateInputs: true));
    }
}
