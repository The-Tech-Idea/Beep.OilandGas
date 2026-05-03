using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.PumpTypes;
using Xunit;

namespace Beep.OilandGas.PumpPerformance.Tests;

public class PumpConstantsAndPdPumpTests
{
    [Fact]
    public void HorsepowerConversionFactors_MatchUsFieldHandbookValues()
    {
        Assert.Equal(3960.0, PumpConstants.HorsepowerConversionFactor);
        Assert.Equal(1714.0, PumpConstants.HorsepowerFromGpmPsiFactor);
    }

    [Fact]
    public void PositiveDisplacementPump_CalculatePower_UsesGpmPsiFactor()
    {
        double p = PositiveDisplacementPump.CalculatePower(100, 50, 0.8);
        double expected = 100 * 50 / (PumpConstants.HorsepowerFromGpmPsiFactor * 0.8);
        Assert.Equal(expected, p, 9);
    }
}
