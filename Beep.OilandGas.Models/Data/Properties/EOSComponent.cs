using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EOSComponent : ModelEntityBase
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
        private decimal CriticalVolumeValue;

        public decimal CriticalVolume

        {

            get { return this.CriticalVolumeValue; }

            set { SetProperty(ref CriticalVolumeValue, value); }

        }
        private decimal AcentricFactorValue;

        public decimal AcentricFactor

        {

            get { return this.AcentricFactorValue; }

            set { SetProperty(ref AcentricFactorValue, value); }

        }
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
        private decimal OmegaAValue;

        public decimal OmegaA

        {

            get { return this.OmegaAValue; }

            set { SetProperty(ref OmegaAValue, value); }

        }
        private decimal OmegaBValue;

        public decimal OmegaB

        {

            get { return this.OmegaBValue; }

            set { SetProperty(ref OmegaBValue, value); }

        }
    }
}
