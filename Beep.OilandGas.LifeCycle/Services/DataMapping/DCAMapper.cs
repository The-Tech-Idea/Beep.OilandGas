using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 PRODUCTION entities to DCA production data.
    /// </summary>
    public class DCAMapper
    {
        /// <summary>
        /// Represents production data point for DCA.
        /// </summary>
        public class ProductionDataPoint
        {
            public DateTime Date { get; set; }
            public double ProductionRate { get; set; }
            public double CumulativeProduction { get; set; }
        }

        /// <summary>
        /// Maps PPDM39 PDEN_VOL_SUMMARY records to DCA production data points.
        /// Uses OIL_VOLUME as the primary production indicator, with BOE_VOLUME as fallback.
        /// Records are sorted by EFFECTIVE_DATE and cumulative production is calculated
        /// incrementally from the series start.
        /// </summary>
        /// <param name="wellUWI">The well UWI to get production data for.</param>
        /// <param name="productionEntities">Collection of PDEN_VOL_SUMMARY or generic production entities.</param>
        /// <returns>List of production data points sorted by date for DCA input.</returns>
        public static List<ProductionDataPoint> MapProductionDataToDCA(
            string wellUWI,
            IEnumerable<object> productionEntities)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty.", nameof(wellUWI));

            if (productionEntities == null)
                return new List<ProductionDataPoint>();

            var rawPoints = new List<(DateTime date, double rate)>();

            foreach (var entity in productionEntities)
            {
                DateTime? date = null;
                double rate = 0;

                if (entity is PDEN_VOL_SUMMARY pden)
                {
                    date = pden.EFFECTIVE_DATE ?? pden.VOLUME_DATE;
                    // Prefer oil volume; fall back to BOE
                    rate = pden.OIL_VOLUME > 0
                        ? (double)pden.OIL_VOLUME
                        : (double)pden.BOE_VOLUME;
                }
                else
                {
                    // Generic fallback: try common column names via reflection
                    var dateVal = TryGetValue(entity, "EFFECTIVE_DATE", "VOLUME_DATE", "PERIOD_DATE");
                    if (dateVal is DateTime dt) date = dt;
                    else if (dateVal is string s && DateTime.TryParse(s, out var pd)) date = pd;

                    var rateVal = TryGetValue(entity, "OIL_VOLUME", "BOE_VOLUME", "PRODUCTION_RATE");
                    rate = rateVal is decimal d ? (double)d : rateVal is double dbl ? dbl : 0;
                }

                if (date.HasValue && rate > 0)
                    rawPoints.Add((date.Value.Date, rate));
            }

            // Sort by date, then build cumulative
            var sorted = rawPoints.OrderBy(p => p.date).ToList();
            var dataPoints = new List<ProductionDataPoint>(sorted.Count);
            double cumulative = 0;
            foreach (var (date, rate) in sorted)
            {
                cumulative += rate;
                dataPoints.Add(new ProductionDataPoint
                {
                    Date = date,
                    ProductionRate = rate,
                    CumulativeProduction = cumulative
                });
            }

            return dataPoints;
        }

        private static object? TryGetValue(object entity, params string[] propertyNames)
        {
            var type = entity.GetType();
            foreach (var name in propertyNames)
            {
                var prop = type.GetProperty(name);
                if (prop != null)
                {
                    var val = prop.GetValue(entity);
                    if (val != null) return val;
                }
            }
            return null;
        }

        /// <summary>
        /// Converts production data points to arrays for DCA calculations.
        /// Time is expressed in days from the first production date.
        /// </summary>
        public static (double[] time, double[] productionRate) ConvertToDCAArrays(
            List<ProductionDataPoint> dataPoints)
        {
            if (dataPoints == null || !dataPoints.Any())
                throw new ArgumentException("Production data points cannot be null or empty.", nameof(dataPoints));

            var sortedData = dataPoints.OrderBy(d => d.Date).ToList();
            var startDate = sortedData.First().Date;

            var time = sortedData.Select(d => (d.Date - startDate).TotalDays).ToArray();
            var productionRate = sortedData.Select(d => d.ProductionRate).ToArray();

            return (time, productionRate);
        }
    }
}

