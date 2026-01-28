using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class RiskComparisonResult : ModelEntityBase
    {
        private double LowestRiskValue;

        public double LowestRisk

        {

            get { return this.LowestRiskValue; }

            set { SetProperty(ref LowestRiskValue, value); }

        }
        private double HighestRiskValue;

        public double HighestRisk

        {

            get { return this.HighestRiskValue; }

            set { SetProperty(ref HighestRiskValue, value); }

        }
    }
}
