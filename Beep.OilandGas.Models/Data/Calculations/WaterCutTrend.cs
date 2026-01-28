using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class WaterCutTrend : ModelEntityBase
    {
        private double InitialWaterCutValue;

        public double InitialWaterCut

        {

            get { return this.InitialWaterCutValue; }

            set { SetProperty(ref InitialWaterCutValue, value); }

        }
        private double FinalWaterCutValue;

        public double FinalWaterCut

        {

            get { return this.FinalWaterCutValue; }

            set { SetProperty(ref FinalWaterCutValue, value); }

        }
        private double RateOfIncreasePerMonthValue;

        public double RateOfIncreasePerMonth

        {

            get { return this.RateOfIncreasePerMonthValue; }

            set { SetProperty(ref RateOfIncreasePerMonthValue, value); }

        }
        private double TimeToHighWaterCutValue;

        public double TimeToHighWaterCut

        {

            get { return this.TimeToHighWaterCutValue; }

            set { SetProperty(ref TimeToHighWaterCutValue, value); }

        }
    }
}
