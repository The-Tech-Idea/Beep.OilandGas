using System;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Exceptions;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Xunit;

namespace Beep.OilandGas.ChokeAnalysis.Tests;

/// <summary>
/// Golden-vector style regression tests for <see cref="GasChokeCalculator"/>.
/// Inputs use absolute pressure (psia), temperature (°R), and choke area (in²).
/// </summary>
public class GasChokeCalculatorRegressionTests
{
    private static CHOKE_PROPERTIES MakeChoke(decimal diameterInches, decimal dischargeCoefficient = 0.85m)
    {
        decimal areaSqIn = (decimal)Math.PI * diameterInches * diameterInches / 4m;
        return new CHOKE_PROPERTIES
        {
            CHOKE_DIAMETER = diameterInches,
            DISCHARGE_COEFFICIENT = dischargeCoefficient,
            CHOKE_TYPE = "BEAN",
            CHOKE_AREA = areaSqIn
        };
    }

    private static GAS_CHOKE_PROPERTIES MakeGas(
        decimal upstreamPsia,
        decimal downstreamPsia,
        decimal temperatureRankine = 520m,
        decimal gasSpecificGravity = 0.65m,
        decimal zFactor = 0.92m)
    {
        return new GAS_CHOKE_PROPERTIES
        {
            UPSTREAM_PRESSURE = upstreamPsia,
            DOWNSTREAM_PRESSURE = downstreamPsia,
            TEMPERATURE = temperatureRankine,
            GAS_SPECIFIC_GRAVITY = gasSpecificGravity,
            Z_FACTOR = zFactor
        };
    }

    [Fact]
    public void CalculateDownholeChokeFlow_DerivesThroatAreaFromDiameter_WhenAreaUnset()
    {
        var choke = new CHOKE_PROPERTIES
        {
            CHOKE_DIAMETER = 0.5m,
            DISCHARGE_COEFFICIENT = 0.85m,
            CHOKE_TYPE = "BEAN",
            CHOKE_AREA = 0m
        };
        var gas = MakeGas(upstreamPsia: 1000m, downstreamPsia: 150m);

        var result = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gas);

        Assert.True(choke.CHOKE_AREA > 0m);
        Assert.Equal(GasChokeCalculator.ChokeAreaSquareInchesFromDiameter(0.5m), choke.CHOKE_AREA, precision: 5);
        Assert.True(result.FLOW_RATE > 0m);
    }

    [Fact]
    public void CalculateDownholeChokeFlow_SelectsSonic_WhenPressureRatioWellBelowCritical()
    {
        var choke = MakeChoke(0.5m);
        var gas = MakeGas(upstreamPsia: 1000m, downstreamPsia: 150m);

        var result = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gas);

        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSonic, result.FLOW_REGIME);
        Assert.True(result.FLOW_RATE > 0m);
        Assert.Equal(0.15m, result.PRESSURE_RATIO, precision: 6);
        Assert.True(result.CRITICAL_PRESSURE_RATIO > 0m);
        Assert.True(result.PRESSURE_RATIO < result.CRITICAL_PRESSURE_RATIO);
    }

    [Fact]
    public void CalculateDownholeChokeFlow_SelectsSubsonic_WhenPressureRatioAboveCritical()
    {
        var choke = MakeChoke(0.5m);
        var gas = MakeGas(upstreamPsia: 1000m, downstreamPsia: 700m);

        var result = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gas);

        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSubsonic, result.FLOW_REGIME);
        Assert.True(result.FLOW_RATE > 0m);
        Assert.Equal(0.7m, result.PRESSURE_RATIO, precision: 6);
        Assert.True(result.PRESSURE_RATIO > result.CRITICAL_PRESSURE_RATIO);
    }

    [Fact]
    public void CalculateDownholeChokeFlow_SubsonicRateDiffersFromSonicAtSameConditionsExceptPressureRatio()
    {
        var choke = MakeChoke(0.375m);
        var sonicGas = MakeGas(800m, 200m);
        var subGas = MakeGas(800m, 580m);

        var sonic = GasChokeCalculator.CalculateDownholeChokeFlow(choke, sonicGas);
        var sub = GasChokeCalculator.CalculateDownholeChokeFlow(choke, subGas);

        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSonic, sonic.FLOW_REGIME);
        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSubsonic, sub.FLOW_REGIME);
        Assert.NotEqual(sonic.FLOW_RATE, sub.FLOW_RATE);
    }

    [Fact]
    public void CalculateDownholeChokeFlow_Throws_WhenDownstreamNotLessThanUpstream()
    {
        var choke = MakeChoke(0.25m);
        var gas = MakeGas(500m, 500m);

        Assert.Throws<InvalidChokePropertiesException>(() =>
            GasChokeCalculator.CalculateDownholeChokeFlow(choke, gas));
    }

    [Fact]
    public void CalculateDownholeChokeFlow_Throws_WhenChokeNull()
    {
        var gas = MakeGas(1000m, 400m);

        Assert.Throws<ArgumentNullException>(() =>
            GasChokeCalculator.CalculateDownholeChokeFlow(null!, gas));
    }

    [Fact]
    public void CalculateRequiredChokeSize_ReturnsPositiveDiameter_ForModerateRate()
    {
        var gas = MakeGas(800m, 320m, temperatureRankine: 540m);
        decimal d = GasChokeCalculator.CalculateRequiredChokeSize(gas, flowRate: 2500m);

        Assert.True(d >= 0.01m && d <= 2.0m);
    }
}
