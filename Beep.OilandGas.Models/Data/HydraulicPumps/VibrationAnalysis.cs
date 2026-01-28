using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class VibrationAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string VibrationLevelValue = string.Empty;

        public string VibrationLevel

        {

            get { return this.VibrationLevelValue; }

            set { SetProperty(ref VibrationLevelValue, value); }

        }
    }
}
