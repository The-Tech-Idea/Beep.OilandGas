using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Constants;
using Xunit;

namespace Beep.OilandGas.PumpPerformance.Tests;

public class NPSHTests
{
    [Fact]
    public void CalculateNPSHAvailable_WithDefaults_IsPositive()
    {
        double npsha = NPSHCalculations.CalculateNPSHAvailable(
            PumpConstants.StandardAtmosphericPressure,
            suctionPressure: 0,
            PumpConstants.WaterVaporPressureAt60F,
            suctionLift: 0,
            frictionLoss: 0,
            PumpConstants.WaterSpecificGravity);

        double expectedAtmHead = PumpConstants.StandardAtmosphericPressure * (2.31 / 1.0);
        double expectedVaporHead = PumpConstants.WaterVaporPressureAt60F * (2.31 / 1.0);
        Assert.Equal(expectedAtmHead - expectedVaporHead, npsha, 6);
    }

    [Fact]
    public void IsCavitationLikely_DefaultMargin_MatchesPumpConstant()
    {
        bool likely = NPSHCalculations.IsCavitationLikely(13, 12);
        bool likelyExplicit = NPSHCalculations.IsCavitationLikely(13, 12, PumpConstants.DefaultNpshSafetyMarginFeet);
        Assert.Equal(likelyExplicit, likely);
        Assert.True(likely);
    }

    [Fact]
    public void IsCavitationLikely_WhenMarginAdequate_ReturnsFalse()
    {
        Assert.False(NPSHCalculations.IsCavitationLikely(20, 12));
    }
}
