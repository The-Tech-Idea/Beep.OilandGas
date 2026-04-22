using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        public async Task<object?> GetCalculationResultAsync(string calculationId, string calculationType)
        {
            try
            {
                PPDMGenericRepository repository;
                Type resultType;

                switch (NormalizeCalculationType(calculationType))
                {
                    case "DCA":
                        repository = await GetDCAResultRepositoryAsync();
                        resultType = typeof(DCAResult);
                        break;
                    case "ECONOMIC":
                        repository = await GetEconomicResultRepositoryAsync();
                        resultType = typeof(EconomicAnalysisResult);
                        break;
                    case "NODAL":
                        repository = await GetNodalResultRepositoryAsync();
                        resultType = typeof(NodalAnalysisResult);
                        break;
                    case "WELLTEST":
                        repository = await GetWellTestResultRepositoryAsync();
                        resultType = typeof(WELL_TEST_ANALYSIS_RESULT);
                        break;
                    default:
                        throw new ArgumentException($"Unknown calculation type: {calculationType}");
                }

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "CALCULATION_ID", Operator = "=", FilterValue = calculationId }
                };

                var results = await repository.GetAsync(filters);
                var result = results.FirstOrDefault();

                if (result == null)
                    return null;

                if (result is WELL_TEST_ANALYSIS_RESULT wellTestResult)
                    return HydrateWellTestAnalysisResult(wellTestResult);

                // Return the entity cast to the expected result type if possible
                if (resultType.IsInstanceOfType(result))
                    return result;

                // Fallback: return raw entity (caller is responsible for casting via object?)
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting calculation result");
                throw;
            }
        }

        public async Task<CalculationResultsResponse> GetCalculationResultsAsync(string? wellId = null, string? poolId = null, string? fieldId = null, string? calculationType = null)
        {
            try
            {
                var response = new CalculationResultsResponse
                {
                    CalculationType = calculationType
                };

                // Query relevant results
                if (string.IsNullOrEmpty(calculationType) || calculationType.Equals("DCA", StringComparison.OrdinalIgnoreCase))
                {
                    var results = await GetCalculationResultsByTypeAsync("DCA", wellId, poolId, fieldId);
                    response.DcaResults = results.Cast<DCAResult>().ToList();
                }

                if (string.IsNullOrEmpty(calculationType) || calculationType.Equals("ECONOMIC", StringComparison.OrdinalIgnoreCase))
                {
                    var results = await GetCalculationResultsByTypeAsync("ECONOMIC", wellId, poolId, fieldId);
                    response.EconomicResults = results.Cast<EconomicAnalysisResult>().ToList();
                }

                if (string.IsNullOrEmpty(calculationType) || calculationType.Equals("NODAL", StringComparison.OrdinalIgnoreCase))
                {
                    var results = await GetCalculationResultsByTypeAsync("NODAL", wellId, poolId, fieldId);
                    response.NodalResults = results.Cast<NodalAnalysisResult>().ToList();
                }

                if (string.IsNullOrEmpty(calculationType) || NormalizeCalculationType(calculationType) == "WELLTEST")
                {
                    var results = await GetCalculationResultsByTypeAsync("WELLTEST", wellId, poolId, fieldId);
                    response.WellTestResults = results
                        .OfType<WELL_TEST_ANALYSIS_RESULT>()
                        .Select(HydrateWellTestAnalysisResult)
                        .ToList();
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting calculation results");
                throw;
            }
        }

        private async Task<List<object>> GetCalculationResultsByTypeAsync(string calculationType, string? wellId, string? poolId, string? fieldId)
        {
            PPDMGenericRepository repository;

            switch (NormalizeCalculationType(calculationType))
            {
                case "DCA":
                    repository = await GetDCAResultRepositoryAsync();
                    break;
                case "ECONOMIC":
                    repository = await GetEconomicResultRepositoryAsync();
                    break;
                case "NODAL":
                    repository = await GetNodalResultRepositoryAsync();
                    break;
                case "WELLTEST":
                    repository = await GetWellTestResultRepositoryAsync();
                    break;
                default:
                    return new List<object>();
            }

            var filters = new List<AppFilter>();

            if (!string.IsNullOrEmpty(wellId))
                filters.Add(new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId });

            if (!string.IsNullOrEmpty(poolId))
                filters.Add(new AppFilter { FieldName = "POOL_ID", Operator = "=", FilterValue = poolId });

            if (!string.IsNullOrEmpty(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });

            var results = await repository.GetAsync(filters);
            return results.Cast<object>().ToList();
        }

        private static string NormalizeCalculationType(string calculationType)
        {
            return calculationType
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToUpperInvariant();
        }

        private WELL_TEST_ANALYSIS_RESULT HydrateWellTestAnalysisResult(WELL_TEST_ANALYSIS_RESULT result)
        {
            result.DiagnosticPoints ??= DeserializeWellTestDataPoints(result.DIAGNOSTIC_DATA_JSON);
            result.DerivativePoints ??= DeserializeWellTestDataPoints(result.DERIVATIVE_DATA_JSON);

            if (string.IsNullOrWhiteSpace(result.IDENTIFIED_MODEL) && !string.IsNullOrWhiteSpace(result.IDENTIFIED_MODEL_STRING))
            {
                result.IDENTIFIED_MODEL = result.IDENTIFIED_MODEL_STRING;
            }

            return result;
        }

        private List<WellTestDataPoint> DeserializeWellTestDataPoints(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<WellTestDataPoint>();

            try
            {
                return JsonSerializer.Deserialize<List<WellTestDataPoint>>(json) ?? new List<WellTestDataPoint>();
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Unable to deserialize stored well test data points");
                return new List<WellTestDataPoint>();
            }
        }
    }
}
