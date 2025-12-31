using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Analytics;
using Beep.OilandGas.Models.DTOs.Analytics;

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
        Task<AnalyticsResult> CalculateProductionTrendsAsync(ProductionTrendsRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Calculates revenue trends.
        /// </summary>
        Task<AnalyticsResult> CalculateRevenueTrendsAsync(RevenueTrendsRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Calculates cost trends.
        /// </summary>
        Task<AnalyticsResult> CalculateCostTrendsAsync(CostTrendsRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Saves analytics result.
        /// </summary>
        Task<ANALYTICS_RESULT> SaveAnalyticsResultAsync(AnalyticsResult result, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets analytics history.
        /// </summary>
        Task<List<ANALYTICS_RESULT>> GetAnalyticsHistoryAsync(string? analyticsType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Gets dashboard data.
        /// </summary>
        Task<DashboardData> GetDashboardDataAsync(DashboardRequest request, string? connectionName = null);
        
        /// <summary>
        /// Gets analytics insights.
        /// </summary>
        Task<List<AnalyticsInsight>> GetAnalyticsInsightsAsync(string? connectionName = null);
    }
}

