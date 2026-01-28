using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Analytics
{
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
}
