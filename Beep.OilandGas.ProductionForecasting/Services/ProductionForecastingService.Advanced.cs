using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ProductionForecasting.Services
{
    public partial class ProductionForecastingService
    {
        public async Task<PRODUCTION_FORECAST> GenerateProductionForecastAsync(
            string wellId,
            decimal qi,
            decimal di_nominal_daily,
            decimal b_factor,
            int durationDays,
            decimal economicLimitRate = 0)
        {
            _logger?.LogInformation("Generating Production Forecast for Well {WellId}", wellId);

            var forecast = new PRODUCTION_FORECAST
            {
                 FORECAST_ID = Guid.NewGuid().ToString(),
                 WELL_UWI = wellId,
                 INITIAL_PRODUCTION_RATE = qi,
                 FORECAST_START_DATE = DateTime.UtcNow,
                 FORECAST_DURATION = durationDays,
                 FORECAST_TYPE = GetForecastType(b_factor),
                 FORECAST_POINTS = new List<FORECAST_POINT>()
            };

            decimal cumProd = 0;
            decimal qt = qi;

            // Generate Monthly points (every 30 days) to avoid huge lists, or daily?
            // Let's do monthly points for summary, but calc daily logic if needed.
            // For this impl, assume stepping every 'step' days.
            int step = 30; 
            
            for (int t = 0; t <= durationDays; t += step)
            {
                qt = ForecastCalculator.CalculateFlowRate_Arps(qi, di_nominal_daily, b_factor, t);
                
                if (t > 0)
                {
                   // Cum prod calc rigorously from Arps, instead of summing rates
                   cumProd = ForecastCalculator.CalculateCumulative_Arps(qi, di_nominal_daily, b_factor, qt);
                }

                if (qt < economicLimitRate)
                {
                    _logger?.LogInformation("Economic limit reached at day {Day}", t);
                    break;
                }

                forecast.FORECAST_POINTS.Add(new FORECAST_POINT
                {
                     FORECAST_POINT_ID = Guid.NewGuid().ToString(),
                     TIME = t,
                     PRODUCTION_RATE = qt,
                     CUMULATIVE_PRODUCTION = cumProd,
                     FORECAST_DATE = DateTime.UtcNow.AddDays(t)
                });
            }

            forecast.FINAL_PRODUCTION_RATE = qt;
            forecast.TOTAL_CUMULATIVE_PRODUCTION = cumProd;
            
            return await Task.FromResult(forecast);
        }

        private ForecastType GetForecastType(decimal b)
        {
            if (Math.Abs(b) < 0.001m) return ForecastType.Exponential; // Assuming enum exists or map to string?
            if (Math.Abs(b - 1.0m) < 0.001m) return ForecastType.Harmonic;
            return ForecastType.Hyperbolic;
        }
    }
}
