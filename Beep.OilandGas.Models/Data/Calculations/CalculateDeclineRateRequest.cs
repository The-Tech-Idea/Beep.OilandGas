using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CalculateDeclineRateRequest : ModelEntityBase
    {
        private decimal InitialRateValue;

        public decimal InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }
        private decimal CurrentRateValue;

        public decimal CurrentRate

        {

            get { return this.CurrentRateValue; }

            set { SetProperty(ref CurrentRateValue, value); }

        }
        private decimal TimePeriodValue;

        public decimal TimePeriod

        {

            get { return this.TimePeriodValue; }

            set { SetProperty(ref TimePeriodValue, value); }

        }
        private string DeclineTypeValue = "Exponential";

        public string DeclineType

        {

            get { return this.DeclineTypeValue; }

            set { SetProperty(ref DeclineTypeValue, value); }

        }
    }
}
