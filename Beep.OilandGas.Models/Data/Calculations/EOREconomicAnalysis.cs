using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EOREconomicAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private double EstimatedIncrementalOilValue;

        public double EstimatedIncrementalOil

        {

            get { return this.EstimatedIncrementalOilValue; }

            set { SetProperty(ref EstimatedIncrementalOilValue, value); }

        }
        private double OilPriceValue;

        public double OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private double CapitalCostValue;

        public double CapitalCost

        {

            get { return this.CapitalCostValue; }

            set { SetProperty(ref CapitalCostValue, value); }

        }
        private double OperatingCostPerBarrelValue;

        public double OperatingCostPerBarrel

        {

            get { return this.OperatingCostPerBarrelValue; }

            set { SetProperty(ref OperatingCostPerBarrelValue, value); }

        }
        private int ProjectLifeYearsValue;

        public int ProjectLifeYears

        {

            get { return this.ProjectLifeYearsValue; }

            set { SetProperty(ref ProjectLifeYearsValue, value); }

        }
        private double DiscountRateValue;

        public double DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double GrossRevenueValue;

        public double GrossRevenue

        {

            get { return this.GrossRevenueValue; }

            set { SetProperty(ref GrossRevenueValue, value); }

        }
        private double TotalOperatingCostValue;

        public double TotalOperatingCost

        {

            get { return this.TotalOperatingCostValue; }

            set { SetProperty(ref TotalOperatingCostValue, value); }

        }
        private double NetPresentValueValue;

        public double NetPresentValue

        {

            get { return this.NetPresentValueValue; }

            set { SetProperty(ref NetPresentValueValue, value); }

        }
        private double InternalRateOfReturnValue;

        public double InternalRateOfReturn

        {

            get { return this.InternalRateOfReturnValue; }

            set { SetProperty(ref InternalRateOfReturnValue, value); }

        }
        private double PaybackPeriodYearsValue;

        public double PaybackPeriodYears

        {

            get { return this.PaybackPeriodYearsValue; }

            set { SetProperty(ref PaybackPeriodYearsValue, value); }

        }
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }
        private double NpvAt20PercentOilPriceValue;

        public double NpvAt20PercentOilPrice

        {

            get { return this.NpvAt20PercentOilPriceValue; }

            set { SetProperty(ref NpvAt20PercentOilPriceValue, value); }

        }
        private double NpvAt50PercentOilPriceValue;

        public double NpvAt50PercentOilPrice

        {

            get { return this.NpvAt50PercentOilPriceValue; }

            set { SetProperty(ref NpvAt50PercentOilPriceValue, value); }

        }
    }
}
