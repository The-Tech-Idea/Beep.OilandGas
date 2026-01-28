using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RodStringRequest : ModelEntityBase
    {
        private decimal WellDepthValue;

        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }
        private decimal PumpingLoadValue;

        public decimal PumpingLoad

        {

            get { return this.PumpingLoadValue; }

            set { SetProperty(ref PumpingLoadValue, value); }

        }
    }
}
