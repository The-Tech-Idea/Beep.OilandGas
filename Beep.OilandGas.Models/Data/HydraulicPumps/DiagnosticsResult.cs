using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DiagnosticsResult : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string DiagnosticsStatusValue = string.Empty;

        public string DiagnosticsStatus

        {

            get { return this.DiagnosticsStatusValue; }

            set { SetProperty(ref DiagnosticsStatusValue, value); }

        }
    }
}
