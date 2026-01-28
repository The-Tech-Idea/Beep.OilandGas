using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ScheduleComparisonResult : ModelEntityBase
    {
        private int ShortestDurationValue;

        public int ShortestDuration

        {

            get { return this.ShortestDurationValue; }

            set { SetProperty(ref ShortestDurationValue, value); }

        }
        private int LongestDurationValue;

        public int LongestDuration

        {

            get { return this.LongestDurationValue; }

            set { SetProperty(ref LongestDurationValue, value); }

        }
    }
}
