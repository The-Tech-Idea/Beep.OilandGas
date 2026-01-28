using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChemicalParameters : ModelEntityBase
    {
        private string ChemicalTypeValue;

        public string ChemicalType

        {

            get { return this.ChemicalTypeValue; }

            set { SetProperty(ref ChemicalTypeValue, value); }

        }
        private double OptimalTemperatureValue;

        public double OptimalTemperature

        {

            get { return this.OptimalTemperatureValue; }

            set { SetProperty(ref OptimalTemperatureValue, value); }

        }
        private double OptimalSalinityValue;

        public double OptimalSalinity

        {

            get { return this.OptimalSalinityValue; }

            set { SetProperty(ref OptimalSalinityValue, value); }

        }
        private double DegradationRateValue;

        public double DegradationRate

        {

            get { return this.DegradationRateValue; }

            set { SetProperty(ref DegradationRateValue, value); }

        }
        private double AdsorptionFactorValue;

        public double AdsorptionFactor

        {

            get { return this.AdsorptionFactorValue; }

            set { SetProperty(ref AdsorptionFactorValue, value); }

        }
    }
}
