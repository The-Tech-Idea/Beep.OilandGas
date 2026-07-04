using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Analytics;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for analytics operations.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Calculates production trends.
        /// </summary>
        Task<ANALYTICS_RESULT> CalculateProductionTrendsAsync(ProductionTrendsRequest request, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Calculates revenue trends.
        /// </summary>
        Task<ANALYTICS_RESULT> CalculateRevenueTrendsAsync(RevenueTrendsRequest request, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Calculates cost trends.
        /// </summary>
        Task<ANALYTICS_RESULT> CalculateCostTrendsAsync(CostTrendsRequest request, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Saves analytics result.
        /// </summary>
        Task<ANALYTICS_RESULT> SaveAnalyticsResultAsync(ANALYTICS_RESULT result, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Gets analytics history.
        /// </summary>
        Task<List<ANALYTICS_RESULT>> GetAnalyticsHistoryAsync(string? analyticsType, DateTime? startDate, DateTime? endDate, string connectionName = "PPDM39");
        
        /// <summary>
        /// Gets dashboard data.
        /// </summary>
        Task<DashboardData> GetDashboardDataAsync(DashboardRequest request, string connectionName = "PPDM39");
        
        /// <summary>
        /// Gets analytics insights.
        /// </summary>
        Task<List<AnalyticsInsight>> GetAnalyticsInsightsAsync(string connectionName = "PPDM39");
    }
}




