using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.ProductionOperations.Services
{
    /// <summary>
    /// Advanced/staged production operations contract.
    /// This surface is intentionally split from the canonical service to keep API-facing behavior strict.
    /// </summary>
    public interface IProductionOperationsAdvancedService
    {
        #region Cost Analysis & Reporting (staged)

        Task RecordOperationalCostsAsync(OperationalCosts costs, string userId);

        Task<List<OperationalCosts>> GetOperationalCostsAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null);

        Task<CostAnalysis> CalculateCostAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate);

        Task<OperationsReport> GenerateOperationsReportAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null);

        #endregion

        #region Production Optimization (staged)

        Task<List<OptimizationOpportunity>> IdentifyOptimizationOpportunitiesAsync(string wellUWI);

        Task ImplementOptimizationAsync(string opportunityId, string userId);

        Task<OptimizationEffectiveness> MonitorOptimizationEffectivenessAsync(string opportunityId);

        #endregion

        #region Data Management (staged)

        Task<ProductionOperationsSummary> GetProductionOperationsSummaryAsync(DateTime startDate, DateTime endDate);

        Task<byte[]> ExportOperationsDataAsync(string dataType, DateTime startDate, DateTime endDate, string format = "CSV");

        Task<DataValidationResult> ValidateOperationsDataAsync(string dataType, DateTime startDate, DateTime endDate);

        #endregion
    }
}
