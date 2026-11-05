using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Xunit;

namespace Beep.OilandGas.NodalAnalysis.Tests;

public class VogelIPRTests
{
    [Fact]
    public void Vogel_AtZeroFlow_ReturnsReservoirPressure()
    {
        var curve=IPRCalculator.GenerateVogelIPR(new ReservoirProperties{ReservoirPressure=3000,ProductivityIndex=0.6},1000,10);
        Assert.Equal(0,curve[0].FlowRate);
        Assert.Equal(3000,curve[0].FlowingBottomholePressure,8);
    }

    [Fact]
    public void Vogel_AtAof_ReturnsZeroFlowingPressure()
    {
        var curve=IPRCalculator.GenerateVogelIPR(new ReservoirProperties{ReservoirPressure=2500,ProductivityIndex=1.8*800/2500},800,10);
        Assert.Equal(800,curve[^1].FlowRate,8);
        Assert.Equal(0,curve[^1].FlowingBottomholePressure,8);
    }

    [Fact]
    public void Vogel_HalfReservoirPressure_CorrespondsToSeventyPercentOfAof()
    {
        // Vogel: q/qMax = 1 - 0.2*(Pwf/Pr) - 0.8*(Pwf/Pr)^2; at Pwf/Pr=0.5, q/qMax=0.7.
        var curve=IPRCalculator.GenerateVogelIPR(new ReservoirProperties{ReservoirPressure=2000,ProductivityIndex=1.8*500/2000},500,10);
        Assert.Equal(350,curve[7].FlowRate,8);
        Assert.Equal(1000,curve[7].FlowingBottomholePressure,8);
    }
    [Fact]
    public void Vogel_GeneratedPressuresSatisfyTheForwardCorrelation()
    {
        var curve=IPRCalculator.GenerateVogelIPR(new ReservoirProperties{ReservoirPressure=2000,ProductivityIndex=0.45},500,20);
        foreach(var point in curve)
        {
            var pressureRatio=point.FlowingBottomholePressure/2000;
            Assert.Equal(point.FlowRate/500,1-0.2*pressureRatio-0.8*pressureRatio*pressureRatio,10);
        }
    }

    [Theory][InlineData(0,1,500,10)][InlineData(2000,0,500,10)]
    [InlineData(2000,1,0,10)][InlineData(2000,1,500,0)]
    [InlineData(double.NaN,1,500,10)][InlineData(2000,double.PositiveInfinity,500,10)]
    public void Vogel_RefusesUnavailableCurveInputs(double pressure,double index,double limit,int points)
    {
        Assert.Throws<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>IPRCalculator.GenerateVogelIPR(
            new ReservoirProperties{ReservoirPressure=pressure,ProductivityIndex=index},limit,points));
    }
}
