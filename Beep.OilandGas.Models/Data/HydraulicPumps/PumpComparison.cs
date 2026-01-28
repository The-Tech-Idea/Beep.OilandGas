using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpComparison : ModelEntityBase
    {
        private string ComparisonIdValue = string.Empty;

        public string ComparisonId

        {

            get { return this.ComparisonIdValue; }

            set { SetProperty(ref ComparisonIdValue, value); }

        }
        private int PumpCountValue;

        public int PumpCount

        {

            get { return this.PumpCountValue; }

            set { SetProperty(ref PumpCountValue, value); }

        }
    }
}
