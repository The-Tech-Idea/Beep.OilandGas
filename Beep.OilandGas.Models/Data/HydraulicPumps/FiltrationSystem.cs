using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FiltrationSystem : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string FilterStatusValue = string.Empty;

        public string FilterStatus

        {

            get { return this.FilterStatusValue; }

            set { SetProperty(ref FilterStatusValue, value); }

        }
    }
}
