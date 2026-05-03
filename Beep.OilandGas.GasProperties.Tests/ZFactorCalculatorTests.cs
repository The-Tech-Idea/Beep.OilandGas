using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data.GasProperties;
using Xunit;

namespace Beep.OilandGas.GasProperties.Tests;

public class ZFactorCalculatorTests
{
    [Fact]
    [Trait("Scenario", "Well")]
    public void CalculateBrillBeggs_dry_gas_well_screening_point_matches_regression()
    {
        decimal z = ZFactorCalculator.CalculateBrillBeggs(800m, 580m, 0.65m);
        Assert.Equal(0.910862m, decimal.Round(z, 6));
    }

    [Fact]
    [Trait("Scenario", "Well")]
    public void CalculateHallYarborough_dry_gas_well_screening_point_matches_regression()
    {
        decimal z = ZFactorCalculator.CalculateHallYarborough(800m, 580m, 0.65m);
        Assert.Equal(0.909670m, decimal.Round(z, 6));
    }

    [Fact]
    [Trait("Scenario", "Well")]
    public void CalculateStandingKatz_dry_gas_well_screening_point_matches_regression()
    {
        decimal z = ZFactorCalculator.CalculateStandingKatz(800m, 580m, 0.65m);
        Assert.Equal(0.734231m, decimal.Round(z, 6));
    }

    [Fact]
    public void CalculateStandingKatz_high_pr_clamped_to_library_maximum()
    {
        // Documents current clamp behavior for aggressive Pr/Tr (see README applicability).
        decimal z = ZFactorCalculator.CalculateStandingKatz(2000m, 580m, 0.65m);
        Assert.Equal(2.0m, z);
    }

    [Fact]
    public void CalculateBrillBeggs_throws_when_pressure_non_positive()
    {
        Assert.Throws<ArgumentException>(() =>
            ZFactorCalculator.CalculateBrillBeggs(0m, 580m, 0.65m));
    }

    [Fact]
    public void CalculatePseudoCriticalProperties_pure_methane_returns_methane_criticals()
    {
        var c = new GasComposition { MethaneFraction = 1.0m };
        var (pc, tc) = ZFactorCalculator.CalculatePseudoCriticalProperties(c);
        Assert.Equal(667.8m, pc);
        Assert.Equal(343.0m, tc);
    }
}
