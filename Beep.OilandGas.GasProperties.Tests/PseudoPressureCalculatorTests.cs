using Beep.OilandGas.GasProperties.Calculations;
using Xunit;

namespace Beep.OilandGas.GasProperties.Tests;

public class PseudoPressureCalculatorTests
{
    [Fact]
    [Trait("Scenario", "Facility")]
    public void CalculatePseudoPressure_Simpson_pipeline_facility_like_pressure_matches_regression()
    {
        decimal pp = PseudoPressureCalculator.CalculatePseudoPressure(
            500m,
            580m,
            0.65m,
            ZFactorCalculator.CalculateStandingKatz,
            GasViscosityCalculator.CalculateCarrKobayashiBurrows);

        Assert.Equal(144003932.50m, decimal.Round(pp, 2));
    }
}
