using System.Linq;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Beep.OilandGas.ProductionForecasting.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionForecastingAlgorithmGuardrailTests
{
    [Fact]
    public void ModifiedHyperbolic_FinalRate_IsNotHigherThanPureHyperbolic()
    {
        var qi = 1200m;
        var di = 0.015m;
        var b = 0.7m;
        var durationDays = 360m * ForecastAlgorithmConstants.DaysPerMonth;
        var timeSteps = 360;
        var terminalDi = 0.0002m;

        var pure = DeclineForecast.GenerateHyperbolicDeclineForecast(qi, di, b, durationDays, null, timeSteps);
        var modified = DeclineForecast.GenerateModifiedHyperbolicDeclineForecast(
            qi, di, b, durationDays, terminalDi, null, timeSteps);

        var pureFinal = pure.FORECAST_POINTS!.Last().PRODUCTION_RATE;
        var modifiedFinal = modified.FORECAST_POINTS!.Last().PRODUCTION_RATE;

        Assert.True(modifiedFinal <= pureFinal);
    }

    [Fact]
    public void Harmonic_WithEconomicLimit_TruncatesTailToZero()
    {
        var forecast = DeclineForecast.GenerateHarmonicDeclineForecast(
            qi: 1000m,
            di: 0.03m,
            forecastDuration: 120m * ForecastAlgorithmConstants.DaysPerMonth,
            economicLimit: 20m,
            timeSteps: 240);

        Assert.NotEmpty(forecast.FORECAST_POINTS!);
        Assert.Equal(0m, forecast.FORECAST_POINTS!.Last().PRODUCTION_RATE);
    }
}
