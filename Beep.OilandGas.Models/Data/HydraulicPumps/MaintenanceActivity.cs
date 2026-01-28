using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class MaintenanceActivity : ModelEntityBase
    {
        private string ActivityIdValue = string.Empty;

        public string ActivityId

        {

            get { return this.ActivityIdValue; }

            set { SetProperty(ref ActivityIdValue, value); }

        }
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime ActivityDateValue;

        public DateTime ActivityDate

        {

            get { return this.ActivityDateValue; }

            set { SetProperty(ref ActivityDateValue, value); }

        }
        private string ActivityTypeValue = string.Empty;

        public string ActivityType

        {

            get { return this.ActivityTypeValue; }

            set { SetProperty(ref ActivityTypeValue, value); }

        }
    }
}
