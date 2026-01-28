using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class PropertyValue : ModelEntityBase
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
        private decimal ZFactorValue;

        public decimal ZFactor

        {

            get { return this.ZFactorValue; }

            set { SetProperty(ref ZFactorValue, value); }

        }
        private decimal DensityValue;

        public decimal Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }
        private decimal ThermalConductivityValue;

        public decimal ThermalConductivity

        {

            get { return this.ThermalConductivityValue; }

            set { SetProperty(ref ThermalConductivityValue, value); }

        }
        private decimal CompressibilityFactorValue;

        public decimal CompressibilityFactor

        {

            get { return this.CompressibilityFactorValue; }

            set { SetProperty(ref CompressibilityFactorValue, value); }

        }
    }
}
