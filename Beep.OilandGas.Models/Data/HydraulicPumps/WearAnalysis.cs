using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WearAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal WearRateValue;

        public decimal WearRate

        {

            get { return this.WearRateValue; }

            set { SetProperty(ref WearRateValue, value); }

        }
    }
}
