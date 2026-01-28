using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class MaintenanceCostEstimate : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal EstimatedCostValue;

        public decimal EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
    }
}
