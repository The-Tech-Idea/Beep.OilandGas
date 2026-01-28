using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FluidAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string FluidConditionValue = string.Empty;

        public string FluidCondition

        {

            get { return this.FluidConditionValue; }

            set { SetProperty(ref FluidConditionValue, value); }

        }
    }
}
