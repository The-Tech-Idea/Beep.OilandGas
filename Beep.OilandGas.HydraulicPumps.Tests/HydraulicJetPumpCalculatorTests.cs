using Beep.OilandGas.HydraulicPumps.Calculations;
using Beep.OilandGas.HydraulicPumps.Exceptions;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Xunit;

namespace Beep.OilandGas.HydraulicPumps.Tests;

public class HydraulicJetPumpCalculatorTests
{
    internal static HYDRAULIC_PUMP_WELL_PROPERTIES SampleWell() => new()
    {
        WELL_DEPTH = 8000m,
        TUBING_DIAMETER = 2.875m,
        CASING_DIAMETER = 7.0m,
        WELLHEAD_PRESSURE = 50m,
        BOTTOM_HOLE_PRESSURE = 500m,
        WELLHEAD_TEMPERATURE = 520m,
        BOTTOM_HOLE_TEMPERATURE = 580m,
        OIL_GRAVITY = 35m,
        WATER_CUT = 0.2m,
        GAS_OIL_RATIO = 200m,
        GAS_SPECIFIC_GRAVITY = 0.65m,
        DESIRED_PRODUCTION_RATE = 500m,
        PUMP_DEPTH = 7500m
    };

    [Fact]
    public void CalculatePerformance_screening_case_matches_regression()
    {
        var jet = new HYDRAULIC_JET_PUMP_PROPERTIES
        {
            NOZZLE_DIAMETER = 0.5m,
            THROAT_DIAMETER = 1.0m,
            DIFFUSER_DIAMETER = 1.5m,
            POWER_FLUID_PRESSURE = 2000m,
            POWER_FLUID_RATE = 300m,
            POWER_FLUID_SPECIFIC_GRAVITY = 1.0m
        };

        var r = HydraulicJetPumpCalculator.CalculatePerformance(SampleWell(), jet);

        Assert.Equal(593.9189m, decimal.Round(r.PRODUCTION_RATE, 4));
        Assert.Equal(893.9189m, decimal.Round(r.TOTAL_FLOW_RATE, 4));
        Assert.Equal(1.9797m, decimal.Round(r.PRODUCTION_RATIO, 4));
        Assert.Equal(0.487988m, decimal.Round(r.PUMP_EFFICIENCY, 6));
        Assert.Equal(500.0000m, decimal.Round(r.PUMP_INTAKE_PRESSURE, 4));
        Assert.Equal(3105.1534m, decimal.Round(r.PUMP_DISCHARGE_PRESSURE, 4));
        Assert.Equal(10.2100m, decimal.Round(r.POWER_FLUID_HORSEPOWER, 4));
        Assert.Equal(26.3291m, decimal.Round(r.HYDRAULIC_HORSEPOWER, 4));
        Assert.Equal(1.0m, decimal.Round(r.SYSTEM_EFFICIENCY, 6));
    }

    [Fact]
    public void CalculatePerformance_throws_on_null_well()
    {
        var jet = new HYDRAULIC_JET_PUMP_PROPERTIES { POWER_FLUID_RATE = 1m };
        Assert.Throws<ArgumentNullException>(() =>
            HydraulicJetPumpCalculator.CalculatePerformance(null!, jet));
    }

    [Fact]
    public void CalculatePerformance_with_validateInputs_throws_when_oil_gravity_out_of_range()
    {
        var well = SampleWell();
        well.OIL_GRAVITY = 2m;
        var jet = new HYDRAULIC_JET_PUMP_PROPERTIES
        {
            NOZZLE_DIAMETER = 0.5m,
            THROAT_DIAMETER = 1.0m,
            DIFFUSER_DIAMETER = 1.5m,
            POWER_FLUID_PRESSURE = 2000m,
            POWER_FLUID_RATE = 300m,
            POWER_FLUID_SPECIFIC_GRAVITY = 1.0m
        };

        Assert.Throws<InvalidWellPropertiesException>(() =>
            HydraulicJetPumpCalculator.CalculatePerformance(well, jet, validateInputs: true));
    }
}
