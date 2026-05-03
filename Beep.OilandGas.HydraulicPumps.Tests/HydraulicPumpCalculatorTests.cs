using Beep.OilandGas.HydraulicPumps.Calculations;
using Xunit;

namespace Beep.OilandGas.HydraulicPumps.Tests;

public class HydraulicPumpCalculatorTests
{
    [Fact]
    public void CalculateJetPumpPerformance_clamps_efficiency_to_unit_interval()
    {
        var (efficiency, _) = HydraulicPumpCalculator.CalculateJetPumpPerformance(
            q_produced: 500m,
            q_power: 100m,
            p_suction: 100m,
            p_discharge: 800m,
            p_power_fluid_surface: 2000m);

        Assert.InRange(efficiency, 0m, 1m);
    }

    [Fact]
    public void CalculateHydraulicHorsepower_returns_zero_for_non_positive_inputs()
    {
        Assert.Equal(0m, HydraulicPumpCalculator.CalculateHydraulicHorsepower(0m, 100m));
        Assert.Equal(0m, HydraulicPumpCalculator.CalculateHydraulicHorsepower(100m, 0m));
    }
}
