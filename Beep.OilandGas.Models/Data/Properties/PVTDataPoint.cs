using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PVTDataPoint : ModelEntityBase
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
        private decimal VolumeValue;

        public decimal Volume

        {

            get { return this.VolumeValue; }

            set { SetProperty(ref VolumeValue, value); }

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
        private decimal CompressibilityValue;

        public decimal Compressibility

        {

            get { return this.CompressibilityValue; }

            set { SetProperty(ref CompressibilityValue, value); }

        }
        private decimal GORValue;

        public decimal GOR

        {

            get { return this.GORValue; }

            set { SetProperty(ref GORValue, value); }

        }
        private string MeasurementTypeValue = string.Empty;

        public string MeasurementType

        {

            get { return this.MeasurementTypeValue; }

            set { SetProperty(ref MeasurementTypeValue, value); }

        }
    }
}
