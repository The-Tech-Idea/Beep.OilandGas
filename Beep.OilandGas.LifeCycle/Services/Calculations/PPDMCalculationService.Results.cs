using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;
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

                switch (calculationType.ToUpper())
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

                // Convert Entity or Dictionary to DTO
                return result; // For now return as is, implementation of conversion will be added if needed
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

            switch (calculationType.ToUpper())
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
    }
}
