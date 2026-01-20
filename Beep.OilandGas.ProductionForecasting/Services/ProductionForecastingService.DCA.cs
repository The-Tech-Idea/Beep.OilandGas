using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.ProductionForecasting.Calculations;

namespace Beep.OilandGas.ProductionForecasting.Services
{
    public partial class ProductionForecastingService
    {
        public async Task<DeclineCurveAnalysis> PerformDeclineCurveAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            _logger?.LogInformation("Performing decline curve analysis for well {WellUWI} from {StartDate} to {EndDate}",
                wellUWI, startDate, endDate);

            decimal qi = 1000m; // default initial rate (STB/day)
            decimal di = 0.01m; // default decline rate (1/day)

            decimal analysisDuration = (decimal)(endDate - startDate).TotalDays;
            if (analysisDuration <= 0) analysisDuration = 365m; // fallback to 1 year (days)

            var pf = DeclineForecast.GenerateAutoSelectDeclineForecast(qi, di, "conventional", analysisDuration, null, Math.Max(12, (int)Math.Min(360, analysisDuration)));

            var analysis = new DeclineCurveAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("DECLINE_ANALYSIS", Guid.NewGuid().ToString()),
                WellUWI = wellUWI,
                AnalysisDate = DateTime.UtcNow,
                DeclineType = "AutoSelected",
                DeclineRate = di,
                InitialRate = qi,
                EstimatedReserves = pf.TotalCumulativeProduction,
                Status = "OK",
                HistoricalData = new List<ForecastProductionDataPoint>()
            };

            foreach (var p in pf.ForecastPoints.Take(10))
            {
                analysis.HistoricalData.Add(new ForecastProductionDataPoint
                {
                    Date = endDate.AddDays((double)p.Time),
                    Time = p.Time,
                    OilRate = p.ProductionRate,
                    Cumulative = p.CumulativeProduction
                });
            }

            await Task.CompletedTask;
            return analysis;
        }

        public async Task<ProductionForecastResult> GenerateDCAForecastAsync(string wellUWI, string declineType, DateTime startDate, DateTime endDate, int forecastPeriod)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentNullException(nameof(wellUWI));

            decimal qi = 1000m; // STB/day
            decimal di = 0.01m; // 1/day
            decimal forecastDurationDays = forecastPeriod * 30m; // approximate months->days

            Beep.OilandGas.Models.Data.ProductionForecasting.ProductionForecast pf;
            if (!string.IsNullOrWhiteSpace(declineType) && declineType.ToLowerInvariant().StartsWith("harmonic"))
            {
                pf = DeclineForecast.GenerateHarmonicDeclineForecast(qi, di, forecastDurationDays, null, forecastPeriod);
            }
            else if (!string.IsNullOrWhiteSpace(declineType) && declineType.ToLowerInvariant().StartsWith("hyperbolic"))
            {
                decimal b = 0.5m;
                var parts = declineType.Split(':');
                if (parts.Length > 1 && parts[1].StartsWith("b="))
                {
                    var bstr = parts[1].Substring(2);
                    if (decimal.TryParse(bstr, out var vb)) b = vb;
                }
                pf = DeclineForecast.GenerateHyperbolicDeclineForecast(qi, di, b, forecastDurationDays, null, forecastPeriod);
            }
            else
            {
                pf = DeclineForecast.GenerateExponentialDeclineForecast(qi, di, forecastDurationDays, forecastPeriod);
            }

            var dto = new ProductionForecastResult
            {
                ForecastId = _defaults.FormatIdForTable("PRODUCTION_FORECAST", Guid.NewGuid().ToString()),
                WellUWI = wellUWI,
                FieldId = null,
                ForecastDate = DateTime.UtcNow,
                ForecastMethod = "DCA",
                ForecastPoints = new List<ProductionForecastPoint>(),
                EstimatedReserves = pf.TotalCumulativeProduction,
                Status = "Generated"
            };

            var start = endDate;
            foreach (var p in pf.ForecastPoints)
            {
                var date = start.AddDays((double)p.Time);
                dto.ForecastPoints.Add(new ProductionForecastPoint
                {
                    Date = date,
                    OilRate = p.ProductionRate,
                    CumulativeOil = p.CumulativeProduction
                });
            }

            await Task.CompletedTask;
            return dto;
        }

        public async Task<ProbabilisticForecast> GenerateProbabilisticForecastAsync(string wellUWI, string declineType, int forecastPeriod, int iterations = 1000)
        {
            var baseForecast = await GenerateForecastAsync(wellUWI, null, declineType, forecastPeriod);
            var probabilistic = new ProbabilisticForecast
            {
                ForecastId = _defaults.FormatIdForTable("PROB_FORECAST", Guid.NewGuid().ToString()),
                WellUWI = wellUWI,
                P10Forecast = baseForecast,
                P50Forecast = baseForecast,
                P90Forecast = baseForecast,
                ExpectedForecast = baseForecast,
                RiskAnalysis = new ForecastRiskAnalysisResult()
            };
            await Task.CompletedTask;
            return probabilistic;
        }
    }
}
