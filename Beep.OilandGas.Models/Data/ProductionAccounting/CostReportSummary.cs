using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CostReportSummary : ModelEntityBase
    {
        private decimal TotalLiftingCostsValue;

        public decimal TotalLiftingCosts

        {

            get { return this.TotalLiftingCostsValue; }

            set { SetProperty(ref TotalLiftingCostsValue, value); }

        }
        private decimal TotalOperatingCostsValue;

        public decimal TotalOperatingCosts

        {

            get { return this.TotalOperatingCostsValue; }

            set { SetProperty(ref TotalOperatingCostsValue, value); }

        }
        private decimal TotalMarketingCostsValue;

        public decimal TotalMarketingCosts

        {

            get { return this.TotalMarketingCostsValue; }

            set { SetProperty(ref TotalMarketingCostsValue, value); }

        }
        public decimal TotalCosts => TotalLiftingCosts + TotalOperatingCosts + TotalMarketingCosts;
    }
}
