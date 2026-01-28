using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EfficiencyImprovement : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal PotentialImprovementValue;

        public decimal PotentialImprovement

        {

            get { return this.PotentialImprovementValue; }

            set { SetProperty(ref PotentialImprovementValue, value); }

        }
    }
}
