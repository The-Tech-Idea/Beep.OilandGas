using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PartsInventory : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private int PartCountValue;

        public int PartCount

        {

            get { return this.PartCountValue; }

            set { SetProperty(ref PartCountValue, value); }

        }
    }
}
