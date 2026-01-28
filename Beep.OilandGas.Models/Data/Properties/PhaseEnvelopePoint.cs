using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PhaseEnvelopePoint : ModelEntityBase
    {
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal LiquidFractionValue;

        public decimal LiquidFraction

        {

            get { return this.LiquidFractionValue; }

            set { SetProperty(ref LiquidFractionValue, value); }

        }
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
        private string PhaseTypeValue = string.Empty;

        public string PhaseType

        {

            get { return this.PhaseTypeValue; }

            set { SetProperty(ref PhaseTypeValue, value); }

        }
    }
}
