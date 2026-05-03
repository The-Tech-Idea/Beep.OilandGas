using Beep.OilandGas.PumpPerformance;
using Xunit;

namespace Beep.OilandGas.PumpPerformance.Tests;

public class PumpPerformanceCalcTests
{
    [Fact]
    public void CFactorCalc_FirstPointPumpPowerEqualsMotorInput()
    {
        const double motorHp = 200;
        double[] q = { 100, 200, 300 };
        double[] h = { 100, 95, 88 };

        var results = PumpPerformanceCalc.CFactorCalc(motorHp, q, h);

        Assert.Equal(3, results.Count);
        Assert.Equal(motorHp, results[0].PumpPower, 9);
    }

    [Fact]
    public void HQCalc_ReturnsSameLengthAsInput()
    {
        double[] q = { 100, 200, 300 };
        double[] h = { 100, 90, 75 };
        double[] p = { 80, 150, 230 };
        double[] eta = PumpPerformanceCalc.HQCalc(q, h, p, 1.0);
        Assert.Equal(3, eta.Length);
        foreach (double e in eta)
        {
            Assert.InRange(e, 0, 1.2);
        }
    }
}
