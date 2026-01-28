using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class TransientPoint : ModelEntityBase
    {
        private decimal TimeElapsedValue;

        public decimal TimeElapsed

        {

            get { return this.TimeElapsedValue; }

            set { SetProperty(ref TimeElapsedValue, value); }

        } // hours
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
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
    }
}
