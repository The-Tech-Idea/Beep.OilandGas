using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PortfolioWellDecommissioning : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private double EstimatedCostValue;
        public double EstimatedCost
        {
            get { return this.EstimatedCostValue; }
            set { SetProperty(ref EstimatedCostValue, value); }
        }

        private int EstimatedDaysValue;
        public int EstimatedDays
        {
            get { return this.EstimatedDaysValue; }
            set { SetProperty(ref EstimatedDaysValue, value); }
        }
    }
}
