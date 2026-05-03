using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Xunit;

namespace Beep.OilandGas.ChokeAnalysis.Tests;

public class MultiphaseChokeCalculatorCorrelationTests
{
    [Fact]
    public void SelectCorrelationUpstreamPressure_MatchesCorrelationBranch()
    {
        var p = new MultiphaseChokeCalculator.MultiphaseFlowResult
        {
            GilbertPressure = 100m,
            RosPressure = 200m,
            AchongPressure = 300m,
            BaxendellPressure = 400m
        };

        Assert.Equal(100m, MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(p, ChokeAnalysisReferenceCodes.CorrelationGilbert));
        Assert.Equal(100m, MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(p, ChokeAnalysisReferenceCodes.CorrelationMultiphase));
        Assert.Equal(200m, MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(p, ChokeAnalysisReferenceCodes.CorrelationRos));
        Assert.Equal(300m, MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(p, ChokeAnalysisReferenceCodes.CorrelationAchong));
        Assert.Equal(400m, MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(p, ChokeAnalysisReferenceCodes.CorrelationBaxendell));
        Assert.Equal(100m, MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(p, ChokeAnalysisReferenceCodes.CorrelationPilehvari));
        Assert.Equal(100m, MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(p, null));
    }

    [Fact]
    public void SelectCorrelationUpstreamPressure_CalculatePressures_RosDiffersFromGilbert_ForTypicalInputs()
    {
        var liquidRate = 500m;
        var glr = 500m;
        var diameterInches = 0.5m;
        var pressures = MultiphaseChokeCalculator.CalculatePressures(liquidRate, glr, diameterInches);

        var ros = MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(pressures, ChokeAnalysisReferenceCodes.CorrelationRos);
        var gilbert = MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(pressures, ChokeAnalysisReferenceCodes.CorrelationGilbert);

        Assert.True(ros > 0m && gilbert > 0m);
        Assert.NotEqual(ros, gilbert);
    }
}
