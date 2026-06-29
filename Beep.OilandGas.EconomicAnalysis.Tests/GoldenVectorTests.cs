using Xunit;

namespace Beep.OilandGas.EconomicAnalysis.Tests;

/// <summary>
/// Golden-vector tests with known reference values from industry-standard examples.
/// These guard against regressions in core financial calculations.
/// </summary>
public class GoldenVectorTests
{
    // Standard example: $1000 investment, $300/year for 5 years at 10%
    // NPV = -1000 + 300/1.1 + 300/1.1² + 300/1.1³ + 300/1.1⁴ + 300/1.1⁵
    //    = -1000 + 272.73 + 247.93 + 225.39 + 204.90 + 186.28 = 137.24
    [Fact]
    public void NPV_StandardExample_MatchesKnownValue()
    {
        double[] cashFlows = { -1000, 300, 300, 300, 300, 300 };
        double rate = 0.10;

        var npv = EconomicAnalysis.Calculations.EconomicCalculator.CalculateNPV(cashFlows, rate);

        Assert.Equal(137.24, npv, 1); // Within $1 of known value
    }

    // Payback: $1000 investment, $300/year → payback in year 4
    [Fact]
    public void Payback_StandardExample_MatchesKnownValue()
    {
        double[] cashFlows = { -1000, 300, 300, 300, 300, 300 };

        var payback = EconomicAnalysis.Calculations.EconomicCalculator.CalculatePaybackPeriod(cashFlows);

        Assert.Equal(3.33, payback, 1); // ~3.33 years (1000/300)
    }

    // PI = (NPV + Investment) / Investment = (137.24 + 1000) / 1000 = 1.137
    [Fact]
    public void ProfitabilityIndex_StandardExample_GreaterThanOne()
    {
        double[] cashFlows = { -1000, 300, 300, 300, 300, 300 };
        double rate = 0.10;

        var pi = EconomicAnalysis.Calculations.EconomicCalculator.CalculateProfitabilityIndex(cashFlows, rate);

        Assert.True(pi > 1.0, "PI should exceed 1.0 for profitable project");
        Assert.True(pi < 1.5, "PI should be reasonable for this example");
    }
}
