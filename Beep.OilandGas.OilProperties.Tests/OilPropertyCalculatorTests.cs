using Beep.OilandGas.OilProperties.Calculations;
using Xunit;

namespace Beep.OilandGas.OilProperties.Tests;

public class OilPropertyCalculatorTests
{
    [Fact]
    public void OilSpecificGravityFromApi_35API_matches_formula()
    {
        decimal go = OilPropertyCalculator.OilSpecificGravityFromApi(35m);
        // 141.5 / (131.5 + 35)
        Assert.Equal(0.8498m, decimal.Round(go, 4));
    }

    [Fact]
    public void CalculateOilFVF_Standing_regression_vector()
    {
        // Standing Bo: Rs=500 scf/stb, Gg=0.8, API=40, T=150 °F — locks current implementation.
        decimal bo = OilPropertyCalculator.CalculateOilFVF_Standing(500m, 0.8m, 40m, 150m);
        Assert.Equal(1.2766m, decimal.Round(bo, 4));
    }

    [Fact]
    public void CalculateDeadOilViscosity_BeggsRobinson_regression_vector()
    {
        decimal mu = OilPropertyCalculator.CalculateDeadOilViscosity_BeggsRobinson(40m, 150m);
        Assert.Equal(2.1081m, decimal.Round(mu, 4));
    }

    [Fact]
    public void CalculateBubblePointPressure_Standing_positive()
    {
        decimal pb = OilPropertyCalculator.CalculateBubblePointPressure_Standing(800m, 0.75m, 38m, 200m);
        Assert.True(pb > 0m);
        Assert.True(pb < 5000m);
    }

    [Fact]
    public void CalculateSaturatedViscosity_BeggsRobinson_with_zero_Rs_returns_dead_oil()
    {
        decimal dead = 2.5m;
        decimal mu = OilPropertyCalculator.CalculateSaturatedViscosity_BeggsRobinson(dead, 0m);
        Assert.Equal(dead, mu);
    }
}
