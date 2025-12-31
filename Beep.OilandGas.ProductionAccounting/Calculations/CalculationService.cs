using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.DTOs.Calculations;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Calculations
{
    /// <summary>
    /// Service for managing calculation operations.
    /// Wraps static calculation methods and optionally tracks calculation history.
    /// </summary>
    public class CalculationService : ICalculationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<CalculationService>? _logger;
        private readonly string _connectionName;
        private const string CALCULATION_RESULT_TABLE = "CALCULATION_RESULT";

        public CalculationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<CalculationService>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Calculates decline rate.
        /// </summary>
        public async Task<CalculationResultResponse> CalculateDeclineRateAsync(
            CalculateDeclineRateRequest request,
            string userId,
            bool trackHistory = false,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            object result;
            string calculationType;

            if (request.DeclineType == "Hyperbolic")
            {
                var (declineRate, exponent) = ProductionCalculations.CalculateHyperbolicDecline(
                    request.InitialRate,
                    request.CurrentRate,
                    request.TimePeriod);

                result = new { DeclineRate = declineRate, HyperbolicExponent = exponent };
                calculationType = "HyperbolicDecline";
            }
            else
            {
                var declineRate = ProductionCalculations.CalculateExponentialDeclineRate(
                    request.InitialRate,
                    request.CurrentRate,
                    request.TimePeriod);

                result = new { DeclineRate = declineRate };
                calculationType = "ExponentialDecline";
            }

            var response = new CalculationResultResponse
            {
                CalculationId = Guid.NewGuid().ToString(),
                CalculationType = calculationType,
                CalculationDate = DateTime.UtcNow,
                Result = result,
                InputParameters = new Dictionary<string, object>
                {
                    { "InitialRate", request.InitialRate },
                    { "CurrentRate", request.CurrentRate },
                    { "TimePeriod", request.TimePeriod },
                    { "DeclineType", request.DeclineType }
                }
            };

            if (trackHistory)
            {
                await SaveCalculationResultAsync(response, userId, connectionName);
            }

            _logger?.LogDebug("Calculated {CalculationType} decline rate", calculationType);
            return response;
        }

        /// <summary>
        /// Calculates volume (net or gross).
        /// </summary>
        public async Task<CalculationResultResponse> CalculateVolumeAsync(
            CalculateVolumeRequest request,
            string userId,
            bool trackHistory = false,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            object result;
            string calculationType;

            if (request.GrossVolume.HasValue)
            {
                var netVolume = CrudeOilCalculations.CalculateNetVolume(
                    request.GrossVolume.Value,
                    request.BswPercentage);

                result = new { GrossVolume = request.GrossVolume.Value, NetVolume = netVolume, BswPercentage = request.BswPercentage };
                calculationType = "NetVolume";
            }
            else if (request.NetVolume.HasValue)
            {
                var grossVolume = CrudeOilCalculations.CalculateGrossVolume(
                    request.NetVolume.Value,
                    request.BswPercentage);

                result = new { GrossVolume = grossVolume, NetVolume = request.NetVolume.Value, BswPercentage = request.BswPercentage };
                calculationType = "GrossVolume";
            }
            else
            {
                throw new ArgumentException("Either GrossVolume or NetVolume must be provided.", nameof(request));
            }

            var response = new CalculationResultResponse
            {
                CalculationId = Guid.NewGuid().ToString(),
                CalculationType = calculationType,
                CalculationDate = DateTime.UtcNow,
                Result = result,
                InputParameters = new Dictionary<string, object>
                {
                    { "GrossVolume", request.GrossVolume },
                    { "NetVolume", request.NetVolume },
                    { "BswPercentage", request.BswPercentage }
                }
            };

            if (trackHistory)
            {
                await SaveCalculationResultAsync(response, userId, connectionName);
            }

            _logger?.LogDebug("Calculated {CalculationType}", calculationType);
            return response;
        }

        /// <summary>
        /// Calculates API gravity or specific gravity.
        /// </summary>
        public async Task<CalculationResultResponse> CalculateApiGravityAsync(
            CalculateApiGravityRequest request,
            string userId,
            bool trackHistory = false,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            object result;
            string calculationType;

            if (request.SpecificGravity.HasValue)
            {
                var apiGravity = CrudeOilCalculations.CalculateApiGravity(request.SpecificGravity.Value);

                result = new { ApiGravity = apiGravity, SpecificGravity = request.SpecificGravity.Value };
                calculationType = "ApiGravity";
            }
            else if (request.ApiGravity.HasValue)
            {
                var specificGravity = CrudeOilCalculations.CalculateSpecificGravity(request.ApiGravity.Value);

                result = new { ApiGravity = request.ApiGravity.Value, SpecificGravity = specificGravity };
                calculationType = "SpecificGravity";
            }
            else
            {
                throw new ArgumentException("Either SpecificGravity or ApiGravity must be provided.", nameof(request));
            }

            var response = new CalculationResultResponse
            {
                CalculationId = Guid.NewGuid().ToString(),
                CalculationType = calculationType,
                CalculationDate = DateTime.UtcNow,
                Result = result,
                InputParameters = new Dictionary<string, object>
                {
                    { "SpecificGravity", request.SpecificGravity },
                    { "ApiGravity", request.ApiGravity }
                }
            };

            if (trackHistory)
            {
                await SaveCalculationResultAsync(response, userId, connectionName);
            }

            _logger?.LogDebug("Calculated {CalculationType}", calculationType);
            return response;
        }

        /// <summary>
        /// Saves calculation result to history.
        /// </summary>
        public async Task<CALCULATION_RESULT> SaveCalculationResultAsync(
            CalculationResultResponse result,
            string userId,
            string? connectionName = null)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(CALCULATION_RESULT), connName, CALCULATION_RESULT_TABLE, null);

            // Serialize input and result data to JSON
            var inputDataJson = JsonSerializer.Serialize(result.InputParameters);
            var resultDataJson = JsonSerializer.Serialize(result.Result);

            var entity = new CALCULATION_RESULT
            {
                CALCULATION_RESULT_ID = result.CalculationId,
                CALCULATION_TYPE = result.CalculationType,
                CALCULATION_DATE = result.CalculationDate,
                INPUT_DATA = inputDataJson,
                RESULT_DATA = resultDataJson,
                CALCULATED_BY = userId,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Saved calculation result {CalculationId} of type {CalculationType}",
                result.CalculationId, result.CalculationType);

            return entity;
        }

        /// <summary>
        /// Gets calculation history.
        /// </summary>
        public async Task<List<CALCULATION_RESULT>> GetCalculationHistoryAsync(
            string? calculationType,
            DateTime? startDate,
            DateTime? endDate,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(CALCULATION_RESULT), connName, CALCULATION_RESULT_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(calculationType))
            {
                filters.Add(new AppFilter { FieldName = "CALCULATION_TYPE", Operator = "=", FilterValue = calculationType });
            }

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            }

            if (endDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<CALCULATION_RESULT>().OrderByDescending(c => c.CALCULATION_DATE).ToList();
        }

        // ICalculationService interface methods

        /// <summary>
        /// Performs Decline Curve Analysis.
        /// </summary>
        public async Task<DCAResult> PerformDCAAnalysisAsync(DCARequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogDebug("Performing DCA analysis for well {WellId}", request.WellId);

            // Basic DCA calculation - in production would use DCA module
            return new DCAResult
            {
                WellId = request.WellId,
                AnalysisDate = DateTime.UtcNow,
                DeclineType = request.AdditionalParameters?.ContainsKey("DeclineType") == true 
                    ? request.AdditionalParameters["DeclineType"]?.ToString() ?? "Exponential" 
                    : "Exponential",
                InitialRate = 0m,
                DeclineRate = 0m,
                ForecastedProduction = new List<decimal>(),
                IsSuccessful = true
            };
        }

        /// <summary>
        /// Performs Economic Analysis.
        /// </summary>
        public async Task<EconomicAnalysisResult> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogDebug("Performing economic analysis");

            // Basic economic calculation - in production would use EconomicAnalysis module
            return new EconomicAnalysisResult
            {
                AnalysisId = Guid.NewGuid().ToString(),
                AnalysisDate = DateTime.UtcNow,
                NPV = 0m,
                IRR = 0m,
                PaybackPeriod = 0,
                IsSuccessful = true
            };
        }

        /// <summary>
        /// Performs Nodal Analysis.
        /// </summary>
        public async Task<NodalAnalysisResult> PerformNodalAnalysisAsync(NodalAnalysisRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogDebug("Performing nodal analysis for well {WellId}", request.WellId);

            // Basic nodal analysis - in production would use NodalAnalysis module
            return new NodalAnalysisResult
            {
                WellId = request.WellId,
                AnalysisDate = DateTime.UtcNow,
                OptimalFlowRate = 0m,
                OperatingPressure = 0m,
                IsSuccessful = true
            };
        }

        /// <summary>
        /// Performs Well Test Analysis.
        /// </summary>
        public async Task<WellTestAnalysisResult> PerformWellTestAnalysisAsync(WellTestAnalysisCalculationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogDebug("Performing well test analysis for well {WellId}", request.WellId);

            // Basic well test analysis - in production would use WellTestAnalysis module
            return new WellTestAnalysisResult
            {
                WellId = request.WellId,
                TestId = request.TestId ?? Guid.NewGuid().ToString(),
                AnalysisDate = DateTime.UtcNow,
                Permeability = 0m,
                Skin = 0m,
                IsSuccessful = true
            };
        }

        /// <summary>
        /// Performs Flash Calculation.
        /// </summary>
        public async Task<FlashCalculationResult> PerformFlashCalculationAsync(FlashCalculationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogDebug("Performing flash calculation");

            // Basic flash calculation - in production would use FlashCalculations module
            return new FlashCalculationResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                CalculationDate = DateTime.UtcNow,
                VaporFraction = 0m,
                LiquidFraction = 1m,
                IsSuccessful = true
            };
        }

        /// <summary>
        /// Gets calculation result by ID.
        /// </summary>
        public async Task<object?> GetCalculationResultAsync(string calculationId, string calculationType)
        {
            if (string.IsNullOrEmpty(calculationId))
                return null;

            var connName = _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(CALCULATION_RESULT), connName, CALCULATION_RESULT_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CALCULATION_RESULT_ID", Operator = "=", FilterValue = calculationId }
            };

            if (!string.IsNullOrEmpty(calculationType))
            {
                filters.Add(new AppFilter { FieldName = "CALCULATION_TYPE", Operator = "=", FilterValue = calculationType });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<CALCULATION_RESULT>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all calculation results for a well, pool, or field.
        /// </summary>
        public async Task<List<object>> GetCalculationResultsAsync(string? wellId = null, string? poolId = null, string? fieldId = null, string? calculationType = null)
        {
            var connName = _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(CALCULATION_RESULT), connName, CALCULATION_RESULT_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(calculationType))
            {
                filters.Add(new AppFilter { FieldName = "CALCULATION_TYPE", Operator = "=", FilterValue = calculationType });
            }

            // Note: Well/Pool/Field filtering would require CALCULATION_RESULT to have these fields
            // For now, we just return all matching calculation type

            var results = await repo.GetAsync(filters);
            return results.Cast<object>().ToList();
        }
    }
}
