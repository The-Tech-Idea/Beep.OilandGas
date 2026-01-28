using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaPhaseTransition : ModelEntityBase
    {
        /// <summary>
        /// Month of transition
        /// </summary>
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }

        /// <summary>
        /// Change in decline rate
        /// </summary>
        private double DeclineRateChangeValue;

        public double DeclineRateChange

        {

            get { return this.DeclineRateChangeValue; }

            set { SetProperty(ref DeclineRateChangeValue, value); }

        }
    }
}
