using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RebuildAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool RebuildRequiredValue;

        public bool RebuildRequired

        {

            get { return this.RebuildRequiredValue; }

            set { SetProperty(ref RebuildRequiredValue, value); }

        }
    }
}
