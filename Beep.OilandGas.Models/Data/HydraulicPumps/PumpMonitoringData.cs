using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpMonitoringData : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool MonitoringActiveValue;

        public bool MonitoringActive

        {

            get { return this.MonitoringActiveValue; }

            set { SetProperty(ref MonitoringActiveValue, value); }

        }
    }
}
