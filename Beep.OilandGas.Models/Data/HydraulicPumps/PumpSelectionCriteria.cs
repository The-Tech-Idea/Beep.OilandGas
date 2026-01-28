using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpSelectionCriteria : ModelEntityBase
    {
        private List<string> PriorityFactorsValue = new();

        public List<string> PriorityFactors

        {

            get { return this.PriorityFactorsValue; }

            set { SetProperty(ref PriorityFactorsValue, value); }

        }
    }
}
