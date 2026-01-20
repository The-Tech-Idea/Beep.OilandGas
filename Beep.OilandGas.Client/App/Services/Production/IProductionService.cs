using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.Models.Data.DCA;

namespace Beep.OilandGas.Client.App.Services.Production
{
    /// <summary>
    /// Service interface for Production operations
    /// Includes Accounting, Forecasting, and Operations
    /// </summary>
    public interface IProductionService
    {
        #region Accounting

        Task<PRODUCTION_ALLOCATION> GetProductionVolumesAsync(string wellId, DateRangeRequest dateRange, CancellationToken cancellationToken = default);
        Task<ROYALTY_CALCULATION> CalculateRoyaltiesAsync(ROYALTY_INTEREST request, CancellationToken cancellationToken = default);
        Task<COST_ALLOCATION> GetCostAllocationAsync(string entityId, CancellationToken cancellationToken = default);
        Task<REVENUE_DISTRIBUTION> GetRevenueDistributionAsync(string entityId, CancellationToken cancellationToken = default);
        Task<ALLOCATION_RESULT> GetProductionAllocationAsync(PRODUCTION_ALLOCATION request, CancellationToken cancellationToken = default);
        Task<ALLOCATION_RESULT> SaveProductionAllocationAsync(ALLOCATION_RESULT allocation, string? userId = null, CancellationToken cancellationToken = default);

        #endregion

        #region Forecasting

        Task<ProductionForecast> CreateForecastAsync(ReservoirForecastProperties request, CancellationToken cancellationToken = default);
        Task<DCA_FIT_RESULT> GetDeclineCurveAsync(string wellId, CancellationToken cancellationToken = default);
        Task<PRODUCTION_FORECAST> AnalyzeProductionAsync(RESERVOIR_FORECAST_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<List<PRODUCTION_FORECAST>> GetForecastHistoryAsync(string wellId, CancellationToken cancellationToken = default);
        Task<PRODUCTION_FORECAST> GetTypeWellAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<DCA_FIT_RESULT> RunDeclineCurveAnalysisAsync(RESERVOIR_FORECAST_PROPERTIES request, CancellationToken cancellationToken = default);

        #endregion

        #region Operations

        Task<PRODUCTION_COSTS> CreateOperationAsync(PRODUCTION_COSTS request, CancellationToken cancellationToken = default);
        Task<PRODUCTION_COSTS> GetOperationStatusAsync(string operationId, CancellationToken cancellationToken = default);
        Task<PRODUCTION_COSTS> UpdateOperationAsync(string operationId, PRODUCTION_COSTS request, CancellationToken cancellationToken = default);
        Task<PRODUCTION_ALLOCATION> GetProductionDataAsync(string wellId, CancellationToken cancellationToken = default);
        Task<List<PRODUCTION_ALLOCATION>> GetProductionHistoryAsync(string wellId, DateRangeRequest dateRange, CancellationToken cancellationToken = default);
        Task<PRODUCTION_ALLOCATION> RecordProductionAsync(PRODUCTION_ALLOCATION productionRecord, string? userId = null, CancellationToken cancellationToken = default);

        #endregion
    }

    /// <summary>
    /// Request for date range queries
    /// </summary>
    public class DateRangeRequest
    {
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
    }
}
