using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class Measurement : ModelEntityBase
    {
        private string MeasurementIdValue = string.Empty;

        public string MeasurementId

        {

            get { return this.MeasurementIdValue; }

            set { SetProperty(ref MeasurementIdValue, value); }

        }
        private DateTime MeasurementDateTimeValue;

        public DateTime MeasurementDateTime

        {

            get { return this.MeasurementDateTimeValue; }

            set { SetProperty(ref MeasurementDateTimeValue, value); }

        }
        private string MethodValue = string.Empty;

        public string Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }
        private string StandardValue = string.Empty;

        public string Standard

        {

            get { return this.StandardValue; }

            set { SetProperty(ref StandardValue, value); }

        }
        private decimal GrossVolumeValue;

        public decimal GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }
        private decimal BSWValue;

        public decimal BSW

        {

            get { return this.BSWValue; }

            set { SetProperty(ref BSWValue, value); }

        }
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal? PressureValue;

        public decimal? Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
    }
}
