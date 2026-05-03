using System;
using System.Linq;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionForecastingNumericalEdgeCaseRegressionTests
{
    [Fact]
    public void Exponential_VeryShortPeriod_DeterministicVectorMatches()
    {
        var forecast = DeclineForecast.GenerateExponentialDeclineForecast(
            qi: 1000m,
            di: 0.05m,
            forecastDuration: 1m,
            timeSteps: 2);

        var points = forecast.FORECAST_POINTS!;
        Assert.Equal(3, points.Count);

        // Deterministic reference vector for t=[0,0.5,1.0], qi=1000, Di=0.05
        AssertClose(points[0].PRODUCTION_RATE, 1000m, 0.001m);
        AssertClose(points[1].PRODUCTION_RATE, 975.3099m, 0.01m);
        AssertClose(points[2].PRODUCTION_RATE, 951.2294m, 0.01m);

        AssertClose(points[0].CUMULATIVE_PRODUCTION, 0m, 0.001m);
        AssertClose(points[1].CUMULATIVE_PRODUCTION, 493.8018m, 0.02m);
        AssertClose(points[2].CUMULATIVE_PRODUCTION, 975.4115m, 0.02m);
    }

    [Fact]
    public void Hyperbolic_UltraLowDeclineRate_RemainsStableAndMonotonic()
    {
        var forecast = DeclineForecast.GenerateHyperbolicDeclineForecast(
            qi: 1000m,
            di: 0.00000001m,
            b: 0.5m,
            forecastDuration: 3650m,
            economicLimit: null,
            timeSteps: 20);

        var points = forecast.FORECAST_POINTS!;
        Assert.NotEmpty(points);
        Assert.All(points, p => Assert.True(p.PRODUCTION_RATE > 0m));
        Assert.All(points, p => Assert.True(p.CUMULATIVE_PRODUCTION >= 0m));

        for (int i = 1; i < points.Count; i++)
        {
            Assert.True(points[i].PRODUCTION_RATE <= points[i - 1].PRODUCTION_RATE + 0.0001m);
            Assert.True(points[i].CUMULATIVE_PRODUCTION >= points[i - 1].CUMULATIVE_PRODUCTION);
        }

        // For ultra-low decline, end rate should remain very close to qi.
        Assert.True(Math.Abs(points.Last().PRODUCTION_RATE - 1000m) < 0.1m);
    }

    [Fact]
    public void Hyperbolic_BEqualsZero_MatchesExponentialDeterministically()
    {
        const decimal qi = 1200m;
        const decimal di = 0.015m;
        const decimal duration = 365m;
        const int steps = 24;

        var hyperbolic = DeclineForecast.GenerateHyperbolicDeclineForecast(qi, di, 0m, duration, null, steps);
        var exponential = DeclineForecast.GenerateExponentialDeclineForecast(qi, di, duration, steps);

        var hPts = hyperbolic.FORECAST_POINTS!;
        var ePts = exponential.FORECAST_POINTS!;
        Assert.Equal(ePts.Count, hPts.Count);

        for (int i = 0; i < hPts.Count; i++)
        {
            AssertClose(hPts[i].PRODUCTION_RATE, ePts[i].PRODUCTION_RATE, 0.0001m);
            AssertClose(hPts[i].CUMULATIVE_PRODUCTION, ePts[i].CUMULATIVE_PRODUCTION, 0.001m);
        }
    }

    [Fact]
    public void Hyperbolic_BNearOne_ApproximatesHarmonic()
    {
        const decimal qi = 1200m;
        const decimal di = 0.015m;
        const decimal duration = 365m;
        const int steps = 24;

        var hyperbolic = DeclineForecast.GenerateHyperbolicDeclineForecast(qi, di, 0.999999m, duration, null, steps);
        var harmonic = DeclineForecast.GenerateHarmonicDeclineForecast(qi, di, duration, null, steps);

        var hPts = hyperbolic.FORECAST_POINTS!;
        var haPts = harmonic.FORECAST_POINTS!;
        Assert.Equal(haPts.Count, hPts.Count);

        for (int i = 0; i < hPts.Count; i++)
        {
            var rateTol = Math.Max(0.5m, Math.Abs(haPts[i].PRODUCTION_RATE) * 0.005m); // 0.5%
            var cumTol = Math.Max(5m, Math.Abs(haPts[i].CUMULATIVE_PRODUCTION) * 0.02m); // 2%
            Assert.True(Math.Abs(hPts[i].PRODUCTION_RATE - haPts[i].PRODUCTION_RATE) <= rateTol);
            Assert.True(Math.Abs(hPts[i].CUMULATIVE_PRODUCTION - haPts[i].CUMULATIVE_PRODUCTION) <= cumTol);
        }
    }

    [Fact]
    public void ModifiedHyperbolic_TransitionVector_LocksDownTSwitchAndQSwitch()
    {
        // Choose values that make t_switch land exactly on a generated time step.
        // t_switch = (Di / Dlim - 1) / (b * Di)
        const decimal qi = 1000m;
        const decimal di = 0.01m;
        const decimal b = 0.5m;
        const decimal dlim = 0.005m;
        const decimal duration = 400m;
        const int steps = 4; // t = 0,100,200,300,400

        var forecast = DeclineForecast.GenerateModifiedHyperbolicDeclineForecast(
            qi, di, b, duration, dlim, null, steps);
        var points = forecast.FORECAST_POINTS!;

        Assert.Equal(5, points.Count);
        AssertClose(points[2].TIME, 200m, 0.0001m); // t_switch

        // q_switch from hyperbolic branch:
        // q = qi / (1 + b*Di*t)^(1/b) = 1000 / (1 + 0.5*0.01*200)^2 = 250
        AssertClose(points[2].PRODUCTION_RATE, 250m, 0.0001m);

        // After switch, exponential tail at Dlim with continuity at t_switch:
        // q(300) = q_switch * exp(-Dlim*(300-200)) = 250*exp(-0.5) = 151.63266...
        AssertClose(points[3].PRODUCTION_RATE, 151.6327m, 0.001m);
        Assert.True(points[3].PRODUCTION_RATE < points[2].PRODUCTION_RATE);
    }

    private static void AssertClose(decimal actual, decimal expected, decimal tolerance)
    {
        Assert.True(Math.Abs(actual - expected) <= tolerance, $"Expected {expected} +/- {tolerance}, got {actual}");
    }
}
