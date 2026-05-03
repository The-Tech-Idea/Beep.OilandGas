using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class NodalAnalysisNumericalEdgeCaseTests
{
    [Fact]
    public void FindOperatingPoint_DeterministicIntersectionVector_MatchesExpectedWindow()
    {
        var ipr = new List<IPRPoint>
        {
            new(0, 3200),
            new(500, 2800),
            new(1000, 2400),
            new(1500, 2000),
            new(2000, 1600)
        };
        var vlp = new List<VLPPoint>
        {
            new(0, 1200),
            new(500, 1500),
            new(1000, 1800),
            new(1500, 2100),
            new(2000, 2400)
        };

        var op = NodalAnalyzer.FindOperatingPoint(ipr, vlp);

        Assert.InRange(op.FlowRate, 1420, 1435);
        Assert.InRange(op.BottomholePressure, 2050, 2065);
    }

    [Fact]
    public void FindOperatingPoint_NoFlowOverlap_ThrowsArgumentException()
    {
        var ipr = new List<IPRPoint> { new(0, 3200), new(100, 3000) };
        var vlp = new List<VLPPoint> { new(500, 1500), new(700, 1700) };

        var ex = Assert.Throws<ArgumentException>(() => NodalAnalyzer.FindOperatingPoint(ipr, vlp));
        Assert.Contains("do not overlap", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindOperatingPoint_LowPiHighWhpProxyCurve_ReturnsLowRateOperatingPoint()
    {
        var ipr = new List<IPRPoint>
        {
            new(0, 2200),
            new(200, 2100),
            new(400, 2000),
            new(600, 1900),
            new(800, 1800)
        };
        var vlp = new List<VLPPoint>
        {
            new(0, 2050),
            new(200, 2125),
            new(400, 2200),
            new(600, 2275),
            new(800, 2350)
        };

        var op = NodalAnalyzer.FindOperatingPoint(ipr, vlp);

        Assert.InRange(op.FlowRate, 150, 250);
        Assert.InRange(op.BottomholePressure, 2090, 2130);
    }

    [Fact]
    public void FindOperatingPoint_SparseCurves_StillFindsStableIntersection()
    {
        var ipr = new List<IPRPoint> { new(0, 3000), new(2000, 1000) };
        var vlp = new List<VLPPoint> { new(0, 1100), new(2000, 2600) };

        var op = NodalAnalyzer.FindOperatingPoint(ipr, vlp);

        Assert.InRange(op.FlowRate, 1000, 1150);
        Assert.InRange(op.BottomholePressure, 1850, 1950);
    }

    [Fact]
    public void FindOperatingPoint_FlatNoisyCurves_UsesBestMatchWithoutInstability()
    {
        var ipr = new List<IPRPoint>
        {
            new(0, 2000), new(500, 1995), new(1000, 2002), new(1500, 1998), new(2000, 1992)
        };
        var vlp = new List<VLPPoint>
        {
            new(0, 1988), new(500, 1991), new(1000, 1997), new(1500, 2001), new(2000, 2005)
        };

        var op = NodalAnalyzer.FindOperatingPoint(ipr, vlp);

        Assert.InRange(op.FlowRate, 900, 1600);
        Assert.InRange(op.BottomholePressure, 1990, 2002);
    }

    [Fact]
    public void FindOperatingPoint_ExtremePressureBoundaries_ReturnsFinitePoint()
    {
        var ipr = new List<IPRPoint>
        {
            new(0, 15000), new(1000, 11000), new(2000, 7500), new(3000, 5000)
        };
        var vlp = new List<VLPPoint>
        {
            new(0, 3000), new(1000, 5200), new(2000, 7600), new(3000, 10300)
        };

        var op = NodalAnalyzer.FindOperatingPoint(ipr, vlp);

        Assert.True(double.IsFinite(op.FlowRate));
        Assert.True(double.IsFinite(op.BottomholePressure));
        Assert.InRange(op.FlowRate, 1900, 2100);
        Assert.InRange(op.BottomholePressure, 7400, 7800);
    }
}
