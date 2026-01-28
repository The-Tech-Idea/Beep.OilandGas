using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class TestMeasurement : ModelEntityBase
    {
        private string ParameterValue = string.Empty;

        public string Parameter

        {

            get { return this.ParameterValue; }

            set { SetProperty(ref ParameterValue, value); }

        }
        private decimal ValueValue;

        public decimal Value

        {

            get { return this.ValueValue; }

            set { SetProperty(ref ValueValue, value); }

        }
        private string UnitValue = string.Empty;

        public string Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }
        private decimal? UncertaintyValue;

        public decimal? Uncertainty

        {

            get { return this.UncertaintyValue; }

            set { SetProperty(ref UncertaintyValue, value); }

        }
        private string MethodValue = string.Empty;

        public string Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
    }
}
