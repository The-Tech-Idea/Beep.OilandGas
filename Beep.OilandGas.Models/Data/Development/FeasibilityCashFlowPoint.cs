using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FeasibilityCashFlowPoint : ModelEntityBase
    {
        private int YearValue;

        public int Year

        {

            get { return this.YearValue; }

            set { SetProperty(ref YearValue, value); }

        }
        private decimal? RevenueValue;

        public decimal? Revenue

        {

            get { return this.RevenueValue; }

            set { SetProperty(ref RevenueValue, value); }

        }
        private decimal? CapitalCostsValue;

        public decimal? CapitalCosts

        {

            get { return this.CapitalCostsValue; }

            set { SetProperty(ref CapitalCostsValue, value); }

        }
        private decimal? OperatingCostsValue;

        public decimal? OperatingCosts

        {

            get { return this.OperatingCostsValue; }

            set { SetProperty(ref OperatingCostsValue, value); }

        }
        private decimal? TaxesValue;

        public decimal? Taxes

        {

            get { return this.TaxesValue; }

            set { SetProperty(ref TaxesValue, value); }

        }
        private decimal? RoyaltiesValue;

        public decimal? Royalties

        {

            get { return this.RoyaltiesValue; }

            set { SetProperty(ref RoyaltiesValue, value); }

        }
        private decimal? NetCashFlowValue;

        public decimal? NetCashFlow

        {

            get { return this.NetCashFlowValue; }

            set { SetProperty(ref NetCashFlowValue, value); }

        }
        private decimal? CumulativeCashFlowValue;

        public decimal? CumulativeCashFlow

        {

            get { return this.CumulativeCashFlowValue; }

            set { SetProperty(ref CumulativeCashFlowValue, value); }

        }
        private decimal? DiscountedCashFlowValue;

        public decimal? DiscountedCashFlow

        {

            get { return this.DiscountedCashFlowValue; }

            set { SetProperty(ref DiscountedCashFlowValue, value); }

        }
        private decimal? CumulativeDiscountedCashFlowValue;

        public decimal? CumulativeDiscountedCashFlow

        {

            get { return this.CumulativeDiscountedCashFlowValue; }

            set { SetProperty(ref CumulativeDiscountedCashFlowValue, value); }

        }
    }
}
