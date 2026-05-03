using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Calculations;
using Beep.OilandGas.WellTestAnalysis.Validation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.WellTestAnalysis.Services
{
    /// <summary>
    /// Service for well test analysis operations.
    /// Provides methods for pressure transient analysis, reservoir characterization, and well performance evaluation.
    /// Wraps the <see cref="WellTestAnalyzer"/> static methods with logging and audit fields on results.
    /// Persistence, type-curve matching, multi-rate/deconvolution, exports, and plots throw
    /// <see cref="NotImplementedException"/> on the default implementation; supply a host-specific
    /// <see cref="Beep.OilandGas.Models.Core.Interfaces.IWellTestAnalysisService"/> implementation when those capabilities are required.
    /// </summary>
    public partial class WellTestAnalysisService : IWellTestAnalysisService
    {
        private readonly ILogger<WellTestAnalysisService>? _logger;

        public WellTestAnalysisService(ILogger<WellTestAnalysisService>? logger = null)
        {
            _logger = logger;
        }

        #region Build-up Analysis Methods

        public async Task<WELL_TEST_ANALYSIS_RESULT> AnalyzeBuildUpHornerAsync(string wellUWI, WELL_TEST_DATA testData, string userId)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentNullException(nameof(wellUWI));
            if (testData == null) throw new ArgumentNullException(nameof(testData));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            try
            {
                EnsureCalculationWellUwi(testData, wellUWI);
                _logger?.LogInformation("Starting Horner build-up analysis for well {WellUWI}", wellUWI);
                
                var result = WellTestAnalyzer.AnalyzeBuildUp(testData);
                result.ANALYSIS_ID = Guid.NewGuid().ToString();
                result.WELL_UWI = wellUWI;
                result.ANALYSIS_DATE = DateTime.UtcNow;
                result.ANALYSIS_BY_USER = userId;
                
                _logger?.LogInformation("Horner analysis completed for well {WellUWI}: K={Permeability}md, S={SkinFactor}", 
                    wellUWI, result.PERMEABILITY, result.SKIN_FACTOR);
                
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in Horner build-up analysis for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<WELL_TEST_ANALYSIS_RESULT> AnalyzeBuildUpMDHAsync(string wellUWI, WELL_TEST_DATA testData, string userId)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentNullException(nameof(wellUWI));
            if (testData == null) throw new ArgumentNullException(nameof(testData));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            try
            {
                EnsureCalculationWellUwi(testData, wellUWI);
                _logger?.LogInformation("Starting MDH build-up analysis for well {WellUWI}", wellUWI);
                
                var result = WellTestAnalyzer.AnalyzeBuildUpMDH(testData);
                result.ANALYSIS_ID = Guid.NewGuid().ToString();
                result.WELL_UWI = wellUWI;
                result.ANALYSIS_DATE = DateTime.UtcNow;
                result.ANALYSIS_BY_USER = userId;
                
                _logger?.LogInformation("MDH analysis completed for well {WellUWI}: K={Permeability}md, S={SkinFactor}", 
                    wellUWI, result.PERMEABILITY, result.SKIN_FACTOR);
                
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in MDH build-up analysis for well {WellUWI}", wellUWI);
                throw;
            }
        }

        #endregion

        #region Drawdown Analysis Methods

        public async Task<WELL_TEST_ANALYSIS_RESULT> AnalyzeDrawdownAsync(string wellUWI, WELL_TEST_DATA testData, string userId)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentNullException(nameof(wellUWI));
            if (testData == null) throw new ArgumentNullException(nameof(testData));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            try
            {
                EnsureCalculationWellUwi(testData, wellUWI);
                _logger?.LogInformation("Starting drawdown analysis for well {WellUWI}", wellUWI);

                var result = WellTestAnalyzer.AnalyzeDrawdown(testData);
                result.ANALYSIS_ID = Guid.NewGuid().ToString();
                result.WELL_UWI = wellUWI;
                result.ANALYSIS_DATE = DateTime.UtcNow;
                result.ANALYSIS_BY_USER = userId;

                _logger?.LogInformation("Drawdown analysis completed for well {WellUWI}: K={Permeability}md, S={SkinFactor}",
                    wellUWI, result.PERMEABILITY, result.SKIN_FACTOR);
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in drawdown analysis for well {WellUWI}", wellUWI);
                throw;
            }
        }

        #endregion

        #region Derivative Analysis Methods

        public async Task<List<PRESSURE_TIME_POINT>> CalculateDerivativeAsync(string wellUWI, List<PRESSURE_TIME_POINT> pressureData, double smoothingFactor = Constants.WellTestConstants.DefaultDerivativeSmoothing)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentNullException(nameof(wellUWI));
            if (pressureData == null || pressureData.Count == 0) throw new ArgumentNullException(nameof(pressureData));

            try
            {
                _logger?.LogInformation("Calculating pressure derivative for well {WellUWI}", wellUWI);
                
                var result = WellTestAnalyzer.CalculateDerivative(pressureData, smoothingFactor);
                
                _logger?.LogInformation("Derivative calculation completed for well {WellUWI} with {PointCount} points", 
                    wellUWI, result.Count);
                
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating derivative for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<ReservoirModel> IdentifyReservoirModelAsync(string wellUWI, List<PRESSURE_TIME_POINT> derivativeData)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentNullException(nameof(wellUWI));
            if (derivativeData == null || derivativeData.Count == 0) throw new ArgumentNullException(nameof(derivativeData));

            try
            {
                _logger?.LogInformation("Identifying reservoir model for well {WellUWI}", wellUWI);
                
                var result = WellTestAnalyzer.IdentifyReservoirModel(derivativeData);
                
                _logger?.LogInformation("Reservoir model identified for well {WellUWI}: {ModelType}", 
                    wellUWI, result.ToString());
                
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error identifying reservoir model for well {WellUWI}", wellUWI);
                throw;
            }
        }

        #endregion

        /// <summary>
        /// When <paramref name="testData"/> has no <see cref="WELL_TEST_DATA.CalculationWellUwi"/>, copies the explicit <paramref name="wellUWI"/> argument
        /// so in-memory rows stay aligned with PPDM (do not use <see cref="WELL_TEST_DATA.AREA_ID"/> for UWI).
        /// </summary>
        private static void EnsureCalculationWellUwi(WELL_TEST_DATA testData, string wellUWI)
        {
            if (string.IsNullOrWhiteSpace(testData.CalculationWellUwi))
                testData.CalculationWellUwi = wellUWI.Trim();
        }
    }
}
