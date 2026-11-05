using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Xunit;

namespace Beep.OilandGas.NodalAnalysis.Tests;

public class FetkovichIPRTests
{
    [Fact]
    public void Fetkovich_AtZeroFlow_ReturnsReservoirPressure()
    {
        // Recorded test points follow q=0.001*(3000^2-Pwf^2).
        var curve=IPRCalculator.GenerateFetkovichIPR(new ReservoirProperties{ReservoirPressure=3000},[(3240,2400),(7560,1200)],9000,2);
        Assert.Equal(0,curve[0].FlowRate);
        Assert.Equal(3000,curve[0].FlowingBottomholePressure,6);
    }

    [Fact]
    public void Fetkovich_AtAof_ReturnsZeroFlowingPressure()
    {
        // qMax=0.001*2500^2=6250; exact positive-pressure observations determine C and n.
        var curve=IPRCalculator.GenerateFetkovichIPR(new ReservoirProperties{ReservoirPressure=2500},[(2250,2000),(5250,1000)],6250,2);
        Assert.Equal(6250,curve[^1].FlowRate,6);
        Assert.InRange(curve[^1].FlowingBottomholePressure,0,0.001);
    }

    [Fact]
    public void Fetkovich_FractionalExponent_RecoversRecordedReferencePressure()
    {
        // C=0.01,n=0.5: q(1600)=12 and q(1000)=sqrt(300), at Pr=2000.
        const double referenceFlow=17.32050807568877;
        var curve=IPRCalculator.GenerateFetkovichIPR(new ReservoirProperties{ReservoirPressure=2000},[(12,1600),(referenceFlow,1000)],referenceFlow,2);
        Assert.Equal(referenceFlow,curve[^1].FlowRate,8);
        Assert.Equal(1000,curve[^1].FlowingBottomholePressure,6);
        Assert.True(curve[1].FlowingBottomholePressure>curve[^1].FlowingBottomholePressure);
    }
}
