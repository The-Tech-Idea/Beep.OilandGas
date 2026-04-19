using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DecommissioningPhase : ModelEntityBase
    {
        private string PhaseValue = string.Empty;
        public string Phase
        {
            get { return this.PhaseValue; }
            set { SetProperty(ref PhaseValue, value); }
        }

        private int DurationValue;
        public int Duration
        {
            get { return this.DurationValue; }
            set { SetProperty(ref DurationValue, value); }
        }
    }
}
