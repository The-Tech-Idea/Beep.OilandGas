using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ContaminationLevel : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string LevelValue = string.Empty;

        public string Level

        {

            get { return this.LevelValue; }

            set { SetProperty(ref LevelValue, value); }

        }
    }
}
