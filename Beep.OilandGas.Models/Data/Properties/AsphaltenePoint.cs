using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AsphaltenePoint : ModelEntityBase
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
        private decimal AsphaltenePrecipitatedValue;

        public decimal AsphaltenePrecipitated

        {

            get { return this.AsphaltenePrecipitatedValue; }

            set { SetProperty(ref AsphaltenePrecipitatedValue, value); }

        }
        private decimal OpticalDensityValue;

        public decimal OpticalDensity

        {

            get { return this.OpticalDensityValue; }

            set { SetProperty(ref OpticalDensityValue, value); }

        }
    }
}
