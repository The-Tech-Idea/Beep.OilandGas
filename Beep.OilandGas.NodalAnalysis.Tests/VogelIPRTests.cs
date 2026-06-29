using Xunit;

namespace Beep.OilandGas.NodalAnalysis.Tests;

/// <summary>Verifies the Vogel IPR correlation against known reference behavior.</summary>
public class VogelIPRTests
{
    [Fact]
    public void Vogel_AtReservoirPressure_ReturnsMaxFlowRate()
    {
        double pr = 3000.0; // reservoir pressure, psia
        double pwf = 3000.0; // flowing bottomhole pressure = Pr → zero drawdown
        double qMax = 1000.0; // AOF, bbl/d

        var ipr = NodalAnalysis.Calculations.IPRCalculator.CalculateVogelIPR(pr, pwf, qMax);

        Assert.Equal(0.0, ipr, 3); // No flow at zero drawdown
    }

    [Fact]
    public void Vogel_AtZeroFlowingPressure_ReturnsAOF()
    {
        double pr = 2500.0;
        double pwf = 0.0; // maximum drawdown
        double qMax = 800.0;

        var ipr = NodalAnalysis.Calculations.IPRCalculator.CalculateVogelIPR(pr, pwf, qMax);

        Assert.Equal(qMax, ipr, 3); // At Pwf=0, flow = AOF
    }

    [Fact]
    public void Vogel_MidDrawdown_ReturnsReasonableValue()
    {
        double pr = 2000.0;
        double pwf = 1000.0; // 50% drawdown
        double qMax = 500.0;

        var ipr = NodalAnalysis.Calculations.IPRCalculator.CalculateVogelIPR(pr, pwf, qMax);

        Assert.True(ipr > 0, "Flow should be positive");
        Assert.True(ipr < qMax, "Flow should be less than AOF");
        Assert.True(ipr > qMax * 0.5, "At 50% drawdown, flow should exceed 50% of AOF (Vogel curvature)");
    }
}
