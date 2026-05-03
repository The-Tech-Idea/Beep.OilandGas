using Beep.OilandGas.GasProperties.Calculations;
using Xunit;

namespace Beep.OilandGas.GasProperties.Tests;

public class GasViscosityCalculatorTests
{
    [Fact]
    [Trait("Scenario", "Well")]
    public void CalculateCarrKobayashiBurrows_dry_gas_well_screening_point_matches_regression()
    {
        decimal z = ZFactorCalculator.CalculateBrillBeggs(800m, 580m, 0.65m);
        decimal mu = GasViscosityCalculator.CalculateCarrKobayashiBurrows(800m, 580m, 0.65m, z);
        Assert.Equal(0.001882m, decimal.Round(mu, 6));
    }

    [Fact]
    [Trait("Scenario", "Well")]
    public void CalculateLeeGonzalezEakin_dry_gas_well_screening_point_matches_regression()
    {
        decimal z = ZFactorCalculator.CalculateBrillBeggs(800m, 580m, 0.65m);
        decimal mu = GasViscosityCalculator.CalculateLeeGonzalezEakin(800m, 580m, 0.65m, z);
        Assert.Equal(0.012939m, decimal.Round(mu, 6));
    }
}
