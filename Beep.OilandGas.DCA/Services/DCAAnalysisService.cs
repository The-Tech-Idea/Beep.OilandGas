using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.DCA.Results;
using Beep.OilandGas.DCA.Performance;
using Beep.OilandGas.DCA.Statistics;
using Beep.OilandGas.DCA.Validation;
using Beep.OilandGas.DCA.Constants;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.DCA.Services
{
    /// <summary>
    /// Core DCA Analysis Service for decline curve analysis operations.
    /// Provides async methods for production decline forecasting, sensitivity analysis,
    /// and multi-well portfolio management.
    /// </summary>
    public partial class DCAAnalysisService
    {
        private readonly ILogger<DCAAnalysisService>? _logger;
        private DCAFitResult? _lastAnalysisResult;

        /// <summary>
        /// Initializes a new instance of the DCAAnalysisService.
        /// </summary>
        public DCAAnalysisService(ILogger<DCAAnalysisService>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Performs decline curve analysis asynchronously.
        /// </summary>
        public async Task<DCAFitResult> AnalyzeDeclineAsync(
            List<double> productionData,
            List<DateTime> timeData,
            double qi = 1000.0,
            double di = 0.1,
            double confidenceLevel = 0.95)
        {
            if (productionData == null || productionData.Count == 0)
                throw new ArgumentException("Production data cannot be null or empty", nameof(productionData));

            _logger?.LogInformation("Starting DCA analysis: {Count} data points, qi={qi}, di={di}",
                productionData.Count, qi, di);

            try
            {
                var result = await AsyncDCACalculator.FitCurveAsync(productionData, timeData, qi, di);
                _lastAnalysisResult = result;

                _logger?.LogInformation("DCA analysis complete: R²={R2:F4}, RMSE={RMSE:F4}",
                    result.RSquared, result.RMSE);

                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during DCA analysis");
                throw;
            }
        }

        /// <summary>
        /// Gets the last analysis result.
        /// </summary>
        public DCAFitResult? GetLastResult() => _lastAnalysisResult;
    }
}
