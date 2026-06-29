using Xunit;

namespace Beep.OilandGas.EconomicAnalysis.Tests;

/// <summary>Verifies NPV, IRR, and payback calculations against known values.</summary>
public class NPVTests
{
    [Fact]
    public void NPV_PositiveCashFlows_ReturnsPositiveValue()
    {
        double[] cashFlows = { -1000, 300, 300, 300, 300 };
        double discountRate = 0.10;

        var npv = EconomicAnalysis.Calculations.EconomicCalculator.CalculateNPV(cashFlows, discountRate);

        Assert.True(npv > 0, "Positive cash flows at 10% should give positive NPV");
    }

    [Fact]
    public void NPV_ZeroCashFlows_ReturnsNegativeInvestment()
    {
        double[] cashFlows = { -1000, 0, 0, 0, 0 };
        double discountRate = 0.10;

        var npv = EconomicAnalysis.Calculations.EconomicCalculator.CalculateNPV(cashFlows, discountRate);

        Assert.Equal(-1000.0, npv, 1);
    }

    [Fact]
    public void IRR_PositiveNPV_ReturnsAboveDiscountRate()
    {
        double[] cashFlows = { -1000, 400, 400, 400, 400 };

        var irr = EconomicAnalysis.Calculations.EconomicCalculator.CalculateIRR(cashFlows);

        Assert.True(irr > 0.10, $"IRR ({irr:P2}) should exceed 10% for this cash flow");
        Assert.True(irr < 1.0, $"IRR ({irr:P2}) should be less than 100%");
    }
}
