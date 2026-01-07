using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Analytics;
using Beep.OilandGas.Models.DTOs.Analytics;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.ProductionAccounting.Analytics
{
    /// <summary>
    /// Service for managing analytics calculations and results.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<AnalyticsService>? _logger;
        private readonly string _connectionName;
        private const string ANALYTICS_RESULT_TABLE = "ANALYTICS_RESULT";

        public AnalyticsService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AnalyticsService>? logger = null,
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
        /// Calculates production trends.
        /// </summary>
        public async Task<AnalyticsResult> CalculateProductionTrendsAsync(
            ProductionTrendsRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;

            // In a full implementation, would query RUN_TICKET data from database
            // For now, use ProductionAnalytics with empty data (would be populated from database queries)
            var runTickets = new List<RunTicket>();

            // Generate analytics using static ProductionAnalytics class
            var analysis = ProductionAnalytics.AnalyzeProductionTrend(
                runTickets,
                request.StartDate,
                request.EndDate);

            var result = new AnalyticsResult
            {
                AnalyticsId = Guid.NewGuid().ToString(),
                AnalyticsType = "ProductionTrends",
                CalculationDate = DateTime.UtcNow,
                PeriodStart = request.StartDate,
                PeriodEnd = request.EndDate,
                ResultData = analysis
            };

            // Save to database
            await SaveAnalyticsResultAsync(result, userId, connName);

            _logger?.LogDebug("Calculated production trends for period {PeriodStart} to {PeriodEnd}",
                request.StartDate, request.EndDate);

            return result;
        }

        /// <summary>
        /// Calculates revenue trends.
        /// </summary>
        public async Task<AnalyticsResult> CalculateRevenueTrendsAsync(
            RevenueTrendsRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;

            // In a full implementation, would query SALES_TRANSACTION data from database
            // For now, use ProductionAnalytics with empty data
            var transactions = new List<SalesTransaction>();

            // Generate analytics using static ProductionAnalytics class
            var analysis = ProductionAnalytics.AnalyzeProfitability(
                transactions,
                request.StartDate,
                request.EndDate);


            var result = new AnalyticsResult
            {
                AnalyticsId = Guid.NewGuid().ToString(),
                AnalyticsType = "RevenueTrends",
                CalculationDate = DateTime.UtcNow,
                PeriodStart = request.StartDate,
                PeriodEnd = request.EndDate,
                ResultData = analysis
            };

            // Save to database
            await SaveAnalyticsResultAsync(result, userId, connName);

            _logger?.LogDebug("Calculated revenue trends for period {PeriodStart} to {PeriodEnd}",
                request.StartDate, request.EndDate);

            return result;
        }

        /// <summary>
        /// Calculates cost trends.
        /// </summary>
        public async Task<AnalyticsResult> CalculateCostTrendsAsync(
            CostTrendsRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;

            // In a full implementation, would query cost data from database
            // For now, return a basic result
            var result = new AnalyticsResult
            {
                AnalyticsId = Guid.NewGuid().ToString(),
                AnalyticsType = "CostTrends",
                CalculationDate = DateTime.UtcNow,
                PeriodStart = request.StartDate,
                PeriodEnd = request.EndDate,
                ResultData = new { StartDate = request.StartDate, EndDate = request.EndDate }
            };

            // Save to database
            await SaveAnalyticsResultAsync(result, userId, connName);

            _logger?.LogDebug("Calculated cost trends for period {PeriodStart} to {PeriodEnd}",
                request.StartDate, request.EndDate);

            return result;
        }

        /// <summary>
        /// Saves analytics result.
        /// </summary>
        public async Task<ANALYTICS_RESULT> SaveAnalyticsResultAsync(
            AnalyticsResult result,
            string userId,
            string? connectionName = null)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ANALYTICS_RESULT), connName, ANALYTICS_RESULT_TABLE, null);

            // Serialize result data to JSON
            var resultDataJson = JsonSerializer.Serialize(result.ResultData);

            var entity = new ANALYTICS_RESULT
            {
                ANALYTICS_RESULT_ID = result.AnalyticsId,
                ANALYTICS_TYPE = result.AnalyticsType,
                CALCULATION_DATE = result.CalculationDate,
                PERIOD_START = result.PeriodStart,
                PERIOD_END = result.PeriodEnd,
                RESULT_DATA = resultDataJson,
                CALCULATED_BY = userId,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Saved analytics result {AnalyticsId} of type {AnalyticsType}",
                result.AnalyticsId, result.AnalyticsType);

            return entity;
        }

        /// <summary>
        /// Gets analytics history.
        /// </summary>
        public async Task<List<ANALYTICS_RESULT>> GetAnalyticsHistoryAsync(
            string? analyticsType,
            DateTime? startDate,
            DateTime? endDate,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ANALYTICS_RESULT), connName, ANALYTICS_RESULT_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(analyticsType))
            {
                filters.Add(new AppFilter { FieldName = "ANALYTICS_TYPE", Operator = "=", FilterValue = analyticsType });
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
            return results.Cast<ANALYTICS_RESULT>().OrderByDescending(a => a.CALCULATION_DATE).ToList();
        }

        /// <summary>
        /// Gets dashboard data.
        /// </summary>
        public async Task<DashboardData> GetDashboardDataAsync(
            DashboardRequest request,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;

            // In a full implementation, would query data from database
            // For now, return a basic dashboard
            var dashboard = new DashboardData
            {
                TotalProduction = 0m,
                TotalRevenue = 0m,
                TotalCosts = 0m,
                NetProfit = 0m,
                ProfitMargin = 0m,
                ProductionTrends = new List<ProductionTrendData>(),
                RevenueTrends = new List<RevenueTrendData>()
            };

            _logger?.LogDebug("Retrieved dashboard data for period {PeriodStart} to {PeriodEnd}",
                request.StartDate, request.EndDate);

            return dashboard;
        }

        /// <summary>
        /// Gets analytics insights.
        /// </summary>
        public async Task<List<AnalyticsInsight>> GetAnalyticsInsightsAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;

            // In a full implementation, would analyze data and generate insights
            // For now, return empty list
            var insights = new List<AnalyticsInsight>();

            _logger?.LogDebug("Retrieved {Count} analytics insights", insights.Count);

            return insights;
        }
    }
}
