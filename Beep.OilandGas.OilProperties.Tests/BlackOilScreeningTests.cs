using Beep.OilandGas.OilProperties.Calculations;
using Xunit;

namespace Beep.OilandGas.OilProperties.Tests;

public class BlackOilScreeningTests
{
    [Fact]
    public void GetPbAndRsAtPressure_with_lab_bubble_uses_Rs_at_P_above_bubble()
    {
        decimal tempF = 160m;
        var (pb, rsAtP) = BlackOilScreening.GetPbAndRsAtPressure(
            pressurePsia: 3000m,
            tempFahrenheit: tempF,
            apiGravity: 40m,
            gasSpecificGravity: 0.8m,
            bubblePointPressure: 2000m,
            solutionGasOilRatio: 600m);

        Assert.Equal(2000m, pb);
        decimal rsAtBubble = OilPropertyCalculator.CalculateSolutionGOR_Standing(2000m, 0.8m, 40m, tempF);
        Assert.Equal(rsAtBubble, rsAtP);
    }

    [Fact]
    public void GetPbAndRsAtPressure_without_bubble_uses_saturated_at_current_P()
    {
        decimal tempF = 150m;
        var (pb, rsAtP) = BlackOilScreening.GetPbAndRsAtPressure(
            pressurePsia: 2500m,
            tempFahrenheit: tempF,
            apiGravity: 38m,
            gasSpecificGravity: 0.75m,
            bubblePointPressure: null,
            solutionGasOilRatio: null);

        Assert.Equal(2500m, pb);
        decimal rsExpected = OilPropertyCalculator.CalculateSolutionGOR_Standing(2500m, 0.75m, 38m, tempF);
        Assert.Equal(rsExpected, rsAtP);
    }
}
