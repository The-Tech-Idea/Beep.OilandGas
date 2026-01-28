using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FluidChangeRecommendation : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool ChangeRequiredValue;

        public bool ChangeRequired

        {

            get { return this.ChangeRequiredValue; }

            set { SetProperty(ref ChangeRequiredValue, value); }

        }
    }
}
