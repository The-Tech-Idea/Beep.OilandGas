using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class MTBFCalculation : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal MTBF_HoursValue;

        public decimal MTBF_Hours

        {

            get { return this.MTBF_HoursValue; }

            set { SetProperty(ref MTBF_HoursValue, value); }

        }
    }
}
