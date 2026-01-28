using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpPerformanceHistory : ModelEntityBase
    {
        private DateTime PerformanceDateValue;

        public DateTime PerformanceDate

        {

            get { return this.PerformanceDateValue; }

            set { SetProperty(ref PerformanceDateValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private decimal PowerConsumptionValue;

        public decimal PowerConsumption

        {

            get { return this.PowerConsumptionValue; }

            set { SetProperty(ref PowerConsumptionValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
