using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ReliabilityAssessment : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal ReliabilityScoreValue;

        public decimal ReliabilityScore

        {

            get { return this.ReliabilityScoreValue; }

            set { SetProperty(ref ReliabilityScoreValue, value); }

        }
    }
}
