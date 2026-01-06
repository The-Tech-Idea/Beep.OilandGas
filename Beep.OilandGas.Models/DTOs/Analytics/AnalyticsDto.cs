using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Analytics
{
    public class ProductionTrendsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string>? WellIds { get; set; }
        public List<string>? LeaseIds { get; set; }
    }

    public class RevenueTrendsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string>? PropertyIds { get; set; }
    }

    public class CostTrendsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string>? PropertyIds { get; set; }
    }

    public class AnalyticsResult
    {
        public string AnalyticsId { get; set; }
        public string AnalyticsType { get; set; }
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public object ResultData { get; set; }
    }

    public class DashboardRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? PropertyIds { get; set; }
    }

    public class DashboardData
    {
        public decimal TotalProduction { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public List<ProductionTrendData> ProductionTrends { get; set; } = new();
        public List<RevenueTrendData> RevenueTrends { get; set; } = new();
    }

    public class ProductionTrendData
    {
        public DateTime Date { get; set; }
        public decimal Production { get; set; }
    }

    public class RevenueTrendData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class AnalyticsInsight
    {
        public string InsightId { get; set; }
        public string InsightType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    }
}




