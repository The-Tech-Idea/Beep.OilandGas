using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpUpgradeRecommendation : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool UpgradeRecommendedValue;

        public bool UpgradeRecommended

        {

            get { return this.UpgradeRecommendedValue; }

            set { SetProperty(ref UpgradeRecommendedValue, value); }

        }
    }
}
