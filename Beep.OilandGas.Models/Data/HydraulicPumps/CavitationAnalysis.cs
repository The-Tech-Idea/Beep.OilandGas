using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CavitationAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string CavitationRiskValue = string.Empty;

        public string CavitationRisk

        {

            get { return this.CavitationRiskValue; }

            set { SetProperty(ref CavitationRiskValue, value); }

        }
    }
}
