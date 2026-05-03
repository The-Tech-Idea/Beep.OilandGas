using Beep.OilandGas.PumpPerformance.Calculations;
using Xunit;

namespace Beep.OilandGas.PumpPerformance.Tests;

public class AffinityLawsTests
{
    [Fact]
    public void CalculateFlowRateAtNewSpeed_ScalesLinearlyWithRpm()
    {
        double q2 = AffinityLaws.CalculateFlowRateAtNewSpeed(300, 1750, 2000);
        Assert.Equal(300 * (2000.0 / 1750.0), q2, 9);
    }

    [Fact]
    public void CalculateHeadAtNewSpeed_ScalesWithSpeedSquared()
    {
        double h2 = AffinityLaws.CalculateHeadAtNewSpeed(75, 1750, 2000);
        double ratio = 2000.0 / 1750.0;
        Assert.Equal(75 * ratio * ratio, h2, 9);
    }

    [Fact]
    public void CalculatePowerAtNewSpeed_ScalesWithSpeedCubed()
    {
        double p2 = AffinityLaws.CalculatePowerAtNewSpeed(230, 1750, 2000);
        double ratio = 2000.0 / 1750.0;
        Assert.Equal(230 * ratio * ratio * ratio, p2, 6);
    }
}
