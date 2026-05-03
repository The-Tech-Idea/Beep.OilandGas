using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Performance;
using Beep.OilandGas.DCA.Results;
using Beep.OilandGas.DCA.Statistics;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA.MultiWell
{
    /// <summary>
    /// Represents a single well with its production data and analysis results.
    /// </summary>
    public class WellAnalysis
    {
        /// <summary>
        /// Gets or sets the well identifier.
        /// </summary>
        public string WellId { get; set; }

        /// <summary>
        /// Gets or sets the production data.
        /// </summary>
        public List<double> ProductionData { get; set; }

        /// <summary>
        /// Gets or sets the time data.
        /// </summary>
        public List<DateTime> TimeData { get; set; }

        /// <summary>
        /// Gets or sets the fit result.
        /// </summary>
        public DCAFitResult FitResult { get; set; }
    }

    /// <summary>
    /// Provides methods for analyzing multiple wells and generating type curves.
    /// </summary>
    public static class MultiWellAnalyzer
    {
        /// <summary>
        /// Analyzes multiple wells and returns their individual fit results.
        /// </summary>
        /// <param name="wells">Dictionary mapping well IDs to (productionData, timeData) tuples.</param>
        /// <param name="qi">Initial guess for initial production rate.</param>
        /// <param name="di">Initial guess for initial decline rate.</param>
        /// <param name="useParallelProcessing">Whether to process wells in parallel.</param>
        /// <returns>List of well analysis results.</returns>
        public static async Task<List<WellAnalysis>> AnalyzeWellsAsync(
            Dictionary<string, (List<double> productionData, List<DateTime> timeData)> wells,
            double qi = DefaultInitialProductionRate,
            double di = DefaultInitialDeclineRate,
            bool useParallelProcessing = true)
        {
            if (wells == null)
                throw new ArgumentNullException(nameof(wells));

            if (wells.Count == 0)
            {
                throw new Exceptions.InvalidDataException("At least one well is required.");
            }

            var results = new List<WellAnalysis>();

            if (useParallelProcessing)
            {
                var fitResults = await AsyncDCACalculator.ProcessMultipleWellsAsync(wells, qi, di);
                
                foreach (var kvp in wells)
                {
                    results.Add(new WellAnalysis
                    {
                        WellId = kvp.Key,
                        ProductionData = kvp.Value.productionData,
                        TimeData = kvp.Value.timeData,
                        FitResult = fitResults[kvp.Key]
                    });
                }
            }
            else
            {
                foreach (var kvp in wells)
                {
                    var fitResult = await AsyncDCACalculator.FitCurveAsync(
                        kvp.Value.productionData,
                        kvp.Value.timeData,
                        qi,
                        di);

                    results.Add(new WellAnalysis
                    {
                        WellId = kvp.Key,
                        ProductionData = kvp.Value.productionData,
                        TimeData = kvp.Value.timeData,
                        FitResult = fitResult
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// Generates a type curve from multiple wells by averaging their decline parameters.
        /// </summary>
        /// <param name="wellAnalyses">List of well analysis results.</param>
        /// <returns>Type curve parameters [averageQi, averageB].</returns>
        public static double[] GenerateTypeCurve(List<WellAnalysis> wellAnalyses)
        {
            if (wellAnalyses == null || wellAnalyses.Count == 0)
            {
                throw new Exceptions.InvalidDataException("At least one well analysis is required.");
            }

            var validAnalyses = wellAnalyses.Where(w => w.FitResult != null && w.FitResult.Converged).ToList();

            if (validAnalyses.Count == 0)
            {
                throw new Exceptions.InvalidDataException("No valid converged fits found.");
            }

            // Average the parameters
            double avgQi = validAnalyses.Average(w => w.FitResult.Parameters[0]);
            double avgB = validAnalyses.Count(w => w.FitResult.Parameters.Length > 1) > 0
                ? validAnalyses.Where(w => w.FitResult.Parameters.Length > 1)
                    .Average(w => w.FitResult.Parameters[1])
                : DefaultDeclineExponent;

            return new double[] { avgQi, avgB };
        }

        /// <summary>
        /// Calculates statistical summary across multiple wells.
        /// </summary>
        /// <param name="wellAnalyses">List of well analysis results.</param>
        /// <returns>Dictionary containing statistical summaries.</returns>
        public static Dictionary<string, object> CalculateStatisticalSummary(List<WellAnalysis> wellAnalyses)
        {
            if (wellAnalyses == null || wellAnalyses.Count == 0)
            {
                throw new Exceptions.InvalidDataException("At least one well analysis is required.");
            }

            var validAnalyses = wellAnalyses.Where(w => w.FitResult != null && w.FitResult.Converged).ToList();

            if (validAnalyses.Count == 0)
            {
                throw new Exceptions.InvalidDataException("No valid converged fits found.");
            }

            var summary = new Dictionary<string, object>
            {
                ["TotalWells"] = wellAnalyses.Count,
                ["ConvergedWells"] = validAnalyses.Count,
                ["AverageRSquared"] = validAnalyses.Average(w => w.FitResult.RSquared),
                ["AverageRMSE"] = validAnalyses.Average(w => w.FitResult.RMSE),
                ["AverageMAE"] = validAnalyses.Average(w => w.FitResult.MAE),
                ["AverageInitialProductionRate"] = validAnalyses.Average(w => w.FitResult.Parameters[0]),
                ["MinRSquared"] = validAnalyses.Min(w => w.FitResult.RSquared),
                ["MaxRSquared"] = validAnalyses.Max(w => w.FitResult.RSquared)
            };

            return summary;
        }
    }
}

