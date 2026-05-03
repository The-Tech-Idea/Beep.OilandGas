using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Xunit;

namespace Beep.OilandGas.ChokeAnalysis.Tests;

public class ChokePerformanceCurveCalculatorTests
{
    [Fact]
    public void GenerateGasCurve_ProducesPositiveMaxFlow_WithRankineTemperature()
    {
        var choke = new CHOKE_PROPERTIES
        {
            CHOKE_DIAMETER = 32m / 64m,
            DISCHARGE_COEFFICIENT = 0.85m,
            CHOKE_TYPE = "BEAN"
        };
        GasChokeCalculator.EnsureChokeThroatArea(choke);

        var gas = new GAS_CHOKE_PROPERTIES
        {
            UPSTREAM_PRESSURE = 800m,
            DOWNSTREAM_PRESSURE = 400m,
            TEMPERATURE = 520m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            Z_FACTOR = 0.9m
        };

        var curve = ChokePerformanceCurveCalculator.GenerateGasCurve(choke, gas, steps: 10, k: 1.28, zFactor: 0.9);

        Assert.True(curve.MaxFlowRate > 0);
        Assert.True(curve.Points.Count > 0);
        Assert.InRange(curve.CriticalPressureRatio, 0.4, 0.65);
        Assert.Contains(curve.Points, p => p.Regime == ChokeAnalysisReferenceCodes.RegimeSonic);
        Assert.Contains(curve.Points, p => p.Regime == ChokeAnalysisReferenceCodes.RegimeSubsonic);
    }
}
