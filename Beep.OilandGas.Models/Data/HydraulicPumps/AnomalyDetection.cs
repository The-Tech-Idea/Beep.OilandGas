using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AnomalyDetection : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool AnomaliesDetectedValue;

        public bool AnomaliesDetected

        {

            get { return this.AnomaliesDetectedValue; }

            set { SetProperty(ref AnomaliesDetectedValue, value); }

        }
    }
}
