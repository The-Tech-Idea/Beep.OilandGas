using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Analytics
{
    public class ProductionTrendsRequest : ModelEntityBase
    {
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private List<string>? WellIdsValue;

        public List<string>? WellIds

        {

            get { return this.WellIdsValue; }

            set { SetProperty(ref WellIdsValue, value); }

        }
        private List<string>? LeaseIdsValue;

        public List<string>? LeaseIds

        {

            get { return this.LeaseIdsValue; }

            set { SetProperty(ref LeaseIdsValue, value); }

        }
    }

    public class RevenueTrendsRequest : ModelEntityBase
    {
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private List<string>? PropertyIdsValue;

        public List<string>? PropertyIds

        {

            get { return this.PropertyIdsValue; }

            set { SetProperty(ref PropertyIdsValue, value); }

        }
    }

    public class CostTrendsRequest : ModelEntityBase
    {
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private List<string>? PropertyIdsValue;

        public List<string>? PropertyIds

        {

            get { return this.PropertyIdsValue; }

            set { SetProperty(ref PropertyIdsValue, value); }

        }
    }

    public class AnalyticsResult : ModelEntityBase
    {
        private string AnalyticsIdValue;

        public string AnalyticsId

        {

            get { return this.AnalyticsIdValue; }

            set { SetProperty(ref AnalyticsIdValue, value); }

        }
        private string AnalyticsTypeValue;

        public string AnalyticsType

        {

            get { return this.AnalyticsTypeValue; }

            set { SetProperty(ref AnalyticsTypeValue, value); }

        }
        private DateTime CalculationDateValue = DateTime.UtcNow;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private object ResultDataValue;

        public object ResultData

        {

            get { return this.ResultDataValue; }

            set { SetProperty(ref ResultDataValue, value); }

        }
    }

    public class DashboardRequest : ModelEntityBase
    {
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private List<string>? PropertyIdsValue;

        public List<string>? PropertyIds

        {

            get { return this.PropertyIdsValue; }

            set { SetProperty(ref PropertyIdsValue, value); }

        }
    }

    public class DashboardData : ModelEntityBase
    {
        private decimal TotalProductionValue;

        public decimal TotalProduction

        {

            get { return this.TotalProductionValue; }

            set { SetProperty(ref TotalProductionValue, value); }

        }
        private decimal TotalRevenueValue;

        public decimal TotalRevenue

        {

            get { return this.TotalRevenueValue; }

            set { SetProperty(ref TotalRevenueValue, value); }

        }
        private decimal TotalCostsValue;

        public decimal TotalCosts

        {

            get { return this.TotalCostsValue; }

            set { SetProperty(ref TotalCostsValue, value); }

        }
        private decimal NetProfitValue;

        public decimal NetProfit

        {

            get { return this.NetProfitValue; }

            set { SetProperty(ref NetProfitValue, value); }

        }
        private decimal ProfitMarginValue;

        public decimal ProfitMargin

        {

            get { return this.ProfitMarginValue; }

            set { SetProperty(ref ProfitMarginValue, value); }

        }
        private List<ProductionTrendData> ProductionTrendsValue = new();

        public List<ProductionTrendData> ProductionTrends

        {

            get { return this.ProductionTrendsValue; }

            set { SetProperty(ref ProductionTrendsValue, value); }

        }
        private List<RevenueTrendData> RevenueTrendsValue = new();

        public List<RevenueTrendData> RevenueTrends

        {

            get { return this.RevenueTrendsValue; }

            set { SetProperty(ref RevenueTrendsValue, value); }

        }
    }

    public class ProductionTrendData : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private decimal ProductionValue;

        public decimal Production

        {

            get { return this.ProductionValue; }

            set { SetProperty(ref ProductionValue, value); }

        }
    }

    public class RevenueTrendData : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private decimal RevenueValue;

        public decimal Revenue

        {

            get { return this.RevenueValue; }

            set { SetProperty(ref RevenueValue, value); }

        }
    }

    public class AnalyticsInsight : ModelEntityBase
    {
        private string InsightIdValue;

        public string InsightId

        {

            get { return this.InsightIdValue; }

            set { SetProperty(ref InsightIdValue, value); }

        }
        private string InsightTypeValue;

        public string InsightType

        {

            get { return this.InsightTypeValue; }

            set { SetProperty(ref InsightTypeValue, value); }

        }
        private string TitleValue;

        public string Title

        {

            get { return this.TitleValue; }

            set { SetProperty(ref TitleValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string SeverityValue;

        public string Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
        private DateTime GeneratedDateValue = DateTime.UtcNow;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
    }
}








