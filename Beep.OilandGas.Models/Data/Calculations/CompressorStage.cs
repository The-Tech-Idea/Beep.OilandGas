using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorStage : ModelEntityBase
    {
        private int StageNumberValue;

        public int StageNumber

        {

            get { return this.StageNumberValue; }

            set { SetProperty(ref StageNumberValue, value); }

        }
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal StageCompressionRatioValue;

        public decimal StageCompressionRatio

        {

            get { return this.StageCompressionRatioValue; }

            set { SetProperty(ref StageCompressionRatioValue, value); }

        }
        private decimal StagePowerValue;

        public decimal StagePower

        {

            get { return this.StagePowerValue; }

            set { SetProperty(ref StagePowerValue, value); }

        }
        private decimal StageEfficiencyValue;

        public decimal StageEfficiency

        {

            get { return this.StageEfficiencyValue; }

            set { SetProperty(ref StageEfficiencyValue, value); }

        }
    }
}
