using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.FieldManagement.DataMapping
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
        /// Maps PPDM39 production data to DCA production data points.
        /// Note: PPDM39 uses various production-related entities. This mapper
        /// would need to aggregate data from multiple sources.
        /// </summary>
        /// <param name="wellUWI">The well UWI to get production data for.</param>
        /// <param name="productionEntities">Collection of production-related entities.</param>
        /// <returns>List of production data points for DCA.</returns>
        public static List<ProductionDataPoint> MapProductionDataToDCA(
            string wellUWI,
            IEnumerable<object> productionEntities)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty.", nameof(wellUWI));

            var dataPoints = new List<ProductionDataPoint>();

            // TODO: Map actual PPDM39 production entities to production data points
            // This would involve:
            // 1. Querying PRODUCTION or related entities for the well
            // 2. Aggregating daily/monthly production rates
            // 3. Calculating cumulative production
            // 4. Sorting by date

            // Placeholder implementation
            // In production, this would query the actual PPDM39 database

            return dataPoints;
        }

        /// <summary>
        /// Converts production data points to arrays for DCA calculations.
        /// </summary>
        /// <param name="dataPoints">Production data points.</param>
        /// <returns>Tuple containing time array and production rate array.</returns>
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

