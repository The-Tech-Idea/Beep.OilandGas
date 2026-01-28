using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class RateChange : ModelEntityBase
    {
        private DateTime ChangeTimeValue;

        public DateTime ChangeTime

        {

            get { return this.ChangeTimeValue; }

            set { SetProperty(ref ChangeTimeValue, value); }

        }
        private double NewFlowRateValue;

        public double NewFlowRate

        {

            get { return this.NewFlowRateValue; }

            set { SetProperty(ref NewFlowRateValue, value); }

        }
        private string? ReasonValue;

        public string? Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }
}
