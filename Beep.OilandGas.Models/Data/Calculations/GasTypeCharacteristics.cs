using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class GasTypeCharacteristics : ModelEntityBase
    {
        private string GasTypeValue;

        public string GasType

        {

            get { return this.GasTypeValue; }

            set { SetProperty(ref GasTypeValue, value); }

        }
        private double DensityValue;

        public double Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }
        private double CriticalTemperatureValue;

        public double CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }
        private double CriticalPressureValue;

        public double CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }
        private string MiscibilityAdvantageValue;

        public string MiscibilityAdvantage

        {

            get { return this.MiscibilityAdvantageValue; }

            set { SetProperty(ref MiscibilityAdvantageValue, value); }

        }
    }
}
