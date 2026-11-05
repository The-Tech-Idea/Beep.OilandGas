using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Xunit;

namespace Beep.OilandGas.EconomicAnalysis.Tests;

/// <summary>Verifies NPV, IRR, and payback calculations against known values.</summary>
public class NPVTests
{
    [Fact]
    public void NPV_FourAnnualReceiptsAtTenPercent_RemainsBelowInvestment()
    {
        CashFlow[] cashFlows = [new(0, -1000), new(1, 300), new(2, 300), new(3, 300), new(4, 300)];
        double discountRate = 0.10;

        var npv = EconomicAnalysis.Calculations.EconomicCalculator.CalculateNPV(cashFlows, discountRate);

        Assert.Equal(-49.0404, npv, 4); // Four discounted receipts do not recover the initial investment.
    }

    [Fact]
    public void NPV_ZeroCashFlows_ReturnsNegativeInvestment()
    {
        CashFlow[] cashFlows = [new(0, -1000), new(1, 0), new(2, 0), new(3, 0), new(4, 0)];
        double discountRate = 0.10;

        var npv = EconomicAnalysis.Calculations.EconomicCalculator.CalculateNPV(cashFlows, discountRate);

        Assert.Equal(-1000.0, npv, 1);
    }

    [Fact]
    public void IRR_PositiveNPV_ReturnsAboveDiscountRate()
    {
        CashFlow[] cashFlows = [new(0, -1000), new(1, 400), new(2, 400), new(3, 400), new(4, 400)];

        var irr = EconomicAnalysis.Calculations.EconomicCalculator.CalculateIRR(cashFlows);

        Assert.True(irr > 0.10, $"IRR ({irr:P2}) should exceed 10% for this cash flow");
        Assert.True(irr < 1.0, $"IRR ({irr:P2}) should be less than 100%");
    }
}
