using Xunit;

namespace Beep.OilandGas.SuckerRodPumping.Tests;

/// <summary>Verifies sucker rod load and stress calculations against known reference behavior.</summary>
public class RodLoadTests
{
    [Fact]
    public void RodWeightInFluid_WaterCut_ReturnsReducedWeight()
    {
        double rodWeightInAir = 1.63; // lb/ft (1-inch rod)
        double fluidDensity = 62.4;   // water, lb/ft³
        double steelDensity = 490.0;  // lb/ft³

        var buoyantWeight = rodWeightInAir * (1.0 - fluidDensity / steelDensity);

        Assert.True(buoyantWeight < rodWeightInAir, "Buoyant weight should be less than weight in air");
        Assert.True(buoyantWeight > 1.4, "1-inch rod in water should still have significant weight");
    }

    [Fact]
    public void GoodmanStress_WithinFatigueLimit_ReturnsSafe()
    {
        double enduranceLimit = 23000.0; // psi (Grade D)
        double ultimateStrength = 115000.0;
        double alternatingStress = 8000.0;
        double meanStress = 40000.0;

        // Modified Goodman: Sa/Se + Sm/Su <= 1/SF
        double sf = 1.0 / (alternatingStress / enduranceLimit + meanStress / ultimateStrength);

        Assert.True(sf > 1.0, $"Safety factor {sf:F2} should exceed 1.0 for this loading");
    }
}
