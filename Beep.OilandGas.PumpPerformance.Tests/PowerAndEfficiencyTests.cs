using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Constants;
using Xunit;

namespace Beep.OilandGas.PumpPerformance.Tests;

public class PowerAndEfficiencyTests
{
    [Fact]
    public void CalculateBrakeHorsepower_MatchesHydraulicOverEfficiency()
    {
        double bhp = PowerCalculations.CalculateBrakeHorsepower(300, 75, 1.0, 0.75);
        double whp = (300 * 75 * 1.0) / PumpConstants.HorsepowerConversionFactor;
        Assert.Equal(whp / 0.75, bhp, 9);
    }

    [Fact]
    public void CalculateOverallEfficiency_RoundTripsWithBhpFormula()
    {
        const double q = 400;
        const double h = 80;
        const double sg = 0.85;
        const double eta = 0.72;
        double bhp = PowerCalculations.CalculateBrakeHorsepower(q, h, sg, eta);
        double etaOut = EfficiencyCalculations.CalculateOverallEfficiency(q, h, bhp, sg);
        Assert.Equal(eta, etaOut, 9);
    }
}
