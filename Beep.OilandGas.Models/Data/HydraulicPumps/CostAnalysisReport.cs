using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CostAnalysisReport : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal TotalCostValue;

        public decimal TotalCost

        {

            get { return this.TotalCostValue; }

            set { SetProperty(ref TotalCostValue, value); }

        }
        private decimal OperatingCostValue;

        public decimal OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        }
        private decimal MaintenanceCostValue;

        public decimal MaintenanceCost

        {

            get { return this.MaintenanceCostValue; }

            set { SetProperty(ref MaintenanceCostValue, value); }

        }
    }
}
