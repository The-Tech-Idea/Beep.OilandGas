using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FluidComponent : ModelEntityBase
    {
        private string ComponentNameValue = string.Empty;

        public string ComponentName

        {

            get { return this.ComponentNameValue; }

            set { SetProperty(ref ComponentNameValue, value); }

        }
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }
        private decimal MassFractionValue;

        public decimal MassFraction

        {

            get { return this.MassFractionValue; }

            set { SetProperty(ref MassFractionValue, value); }

        }
        private decimal VolumeFractionValue;

        public decimal VolumeFraction

        {

            get { return this.VolumeFractionValue; }

            set { SetProperty(ref VolumeFractionValue, value); }

        }
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
        private decimal BoilingPointValue;

        public decimal BoilingPoint

        {

            get { return this.BoilingPointValue; }

            set { SetProperty(ref BoilingPointValue, value); }

        }
        private decimal CriticalPressureValue;

        public decimal CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }
        private decimal CriticalTemperatureValue;

        public decimal CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }
        private decimal AcentricFactorValue;

        public decimal AcentricFactor

        {

            get { return this.AcentricFactorValue; }

            set { SetProperty(ref AcentricFactorValue, value); }

        }
    }
}
