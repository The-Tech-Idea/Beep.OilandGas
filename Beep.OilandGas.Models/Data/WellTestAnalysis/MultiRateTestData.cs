using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class MultiRateTestData : ModelEntityBase
    {
        private List<RateChange> RateChangesValue = new();

        public List<RateChange> RateChanges

        {

            get { return this.RateChangesValue; }

            set { SetProperty(ref RateChangesValue, value); }

        }
        private List<PRESSURE_TIME_POINT> PressureDataValue = new();

        public List<PRESSURE_TIME_POINT> PressureData

        {

            get { return this.PressureDataValue; }

            set { SetProperty(ref PressureDataValue, value); }

        }
    }
}
