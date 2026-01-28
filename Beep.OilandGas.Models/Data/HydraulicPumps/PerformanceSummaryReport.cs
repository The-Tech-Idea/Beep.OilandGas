using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PerformanceSummaryReport : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal AverageEfficiencyValue;

        public decimal AverageEfficiency

        {

            get { return this.AverageEfficiencyValue; }

            set { SetProperty(ref AverageEfficiencyValue, value); }

        }
        private decimal AverageFlowRateValue;

        public decimal AverageFlowRate

        {

            get { return this.AverageFlowRateValue; }

            set { SetProperty(ref AverageFlowRateValue, value); }

        }
    }
}
