using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class SeparatorStage : ModelEntityBase
    {
        private int StageNumberValue;

        public int StageNumber

        {

            get { return this.StageNumberValue; }

            set { SetProperty(ref StageNumberValue, value); }

        }
        private decimal StagePressureValue;

        public decimal StagePressure

        {

            get { return this.StagePressureValue; }

            set { SetProperty(ref StagePressureValue, value); }

        }
        private decimal StageTemperatureValue;

        public decimal StageTemperature

        {

            get { return this.StageTemperatureValue; }

            set { SetProperty(ref StageTemperatureValue, value); }

        }
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
        private decimal LiquidRecoveryFractionValue;

        public decimal LiquidRecoveryFraction

        {

            get { return this.LiquidRecoveryFractionValue; }

            set { SetProperty(ref LiquidRecoveryFractionValue, value); }

        }
    }
}
