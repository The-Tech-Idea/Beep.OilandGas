using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PerformanceTrends : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private List<decimal> EfficiencyTrendValue = new();

        public List<decimal> EfficiencyTrend

        {

            get { return this.EfficiencyTrendValue; }

            set { SetProperty(ref EfficiencyTrendValue, value); }

        }
        private List<decimal> FlowTrendValue = new();

        public List<decimal> FlowTrend

        {

            get { return this.FlowTrendValue; }

            set { SetProperty(ref FlowTrendValue, value); }

        }
    }
}
