using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Operations
{
    public class AnalyzeEOREconomicsRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

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
        private double CapitalCostMmValue;

        public double CapitalCostMm

        {

            get { return this.CapitalCostMmValue; }

            set { SetProperty(ref CapitalCostMmValue, value); }

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
        private double DiscountRatePctValue;

        public double DiscountRatePct

        {

            get { return this.DiscountRatePctValue; }

            set { SetProperty(ref DiscountRatePctValue, value); }

        }
    }
}