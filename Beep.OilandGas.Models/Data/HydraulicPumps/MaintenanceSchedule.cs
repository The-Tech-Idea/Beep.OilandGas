using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class MaintenanceSchedule : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime NextMaintenanceDateValue;

        public DateTime NextMaintenanceDate

        {

            get { return this.NextMaintenanceDateValue; }

            set { SetProperty(ref NextMaintenanceDateValue, value); }

        }
    }
}
