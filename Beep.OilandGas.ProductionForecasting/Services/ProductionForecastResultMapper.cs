using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionForecasting.Constants;

namespace Beep.OilandGas.ProductionForecasting.Services
{
    /// <summary>
    /// Maps decline-engine <see cref="PRODUCTION_FORECAST"/> / <see cref="FORECAST_POINT"/> to API <see cref="ProductionForecastResult"/>.
    /// </summary>
    internal static class ProductionForecastResultMapper
    {
        public static ProductionForecastResult FromDeclineForecast(
            IPPDM39DefaultsRepository defaults,
            PRODUCTION_FORECAST pf,
            string? wellUWI,
            string? fieldId,
            ForecastType forecastMethod,
            DateTime anchorDate,
            string status)
        {
            var forecastId = defaults.FormatIdForTable("PRODUCTION_FORECAST", Guid.NewGuid().ToString());
            var result = new ProductionForecastResult
            {
                ForecastId = forecastId,
                WellUWI = wellUWI,
                FieldId = fieldId,
                ForecastDate = DateTime.UtcNow,
                ForecastMethod = forecastMethod,
                EstimatedReserves = pf.TOTAL_CUMULATIVE_PRODUCTION,
                Status = status,
                ForecastPoints = new List<ProductionForecastPoint>()
            };

            if (pf.FORECAST_POINTS == null)
                return result;

            foreach (var p in pf.FORECAST_POINTS)
            {
                var date = anchorDate.AddDays((double)p.TIME);
                result.ForecastPoints.Add(new ProductionForecastPoint
                {
                    Date = date,
                    OilRate = p.PRODUCTION_RATE,
                    CumulativeOil = p.CUMULATIVE_PRODUCTION,
                    GasRate = 0,
                    WaterRate = 0
                });
            }

            return result;
        }

        /// <summary>Clamp b into documented Arps BDF bounds.</summary>
        public static decimal ClampDeclineExponentB(decimal b) =>
            Math.Clamp(b, ForecastAlgorithmConstants.ArpsBdfMinB, ForecastAlgorithmConstants.ArpsBdfMaxB);
    }
}
