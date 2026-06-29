using Xunit;

namespace Beep.OilandGas.NodalAnalysis.Tests;

/// <summary>
/// Golden-vector tests for the Fetkovich IPR correlation.
/// Fetkovich: q = C × (Pr² - Pwf²)^n
/// </summary>
public class FetkovichIPRTests
{
    [Fact]
    public void Fetkovich_AtReservoirPressure_ReturnsZero()
    {
        double pr = 3000.0;
        double pwf = 3000.0;
        double c = 0.001;
        double n = 1.0;

        var q = NodalAnalysis.Calculations.IPRCalculator.CalculateFetkovichIPR(pr, pwf, c, n);

        Assert.Equal(0.0, q, 3);
    }

    [Fact]
    public void Fetkovich_AtZeroFlowingPressure_ReturnsMaximum()
    {
        double pr = 2500.0;
        double pwf = 0.0;
        double c = 0.001;
        double n = 1.0;

        var q = NodalAnalysis.Calculations.IPRCalculator.CalculateFetkovichIPR(pr, pwf, c, n);

        // q = 0.001 × (2500² - 0²)^1 = 0.001 × 6,250,000 = 6,250
        Assert.True(q > 6000 && q < 6500, $"Expected ~6250, got {q:F0}");
    }

    [Fact]
    public void Fetkovich_ExponentLessThanOne_CurvesDownward()
    {
        double pr = 2000.0;
        double c = 0.01;
        double n = 0.5;

        var qLinear = NodalAnalysis.Calculations.IPRCalculator.CalculateFetkovichIPR(pr, 1000.0, c, 1.0);
        var qFrac = NodalAnalysis.Calculations.IPRCalculator.CalculateFetkovichIPR(pr, 1000.0, c, n);

        Assert.True(qFrac > qLinear, "n < 1 should give higher mid-curve flow than n = 1 (greater curvature)");
    }
}
