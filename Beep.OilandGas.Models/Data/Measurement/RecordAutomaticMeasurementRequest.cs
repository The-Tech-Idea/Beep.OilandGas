using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Measurement
{
    public class RecordAutomaticMeasurementRequest : ModelEntityBase
    {
        private string WellIdValue;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string LeaseIdValue;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string MeterIdValue;

        public string MeterId

        {

            get { return this.MeterIdValue; }

            set { SetProperty(ref MeterIdValue, value); }

        }
        private decimal MeterReadingValue;

        public decimal MeterReading

        {

            get { return this.MeterReadingValue; }

            set { SetProperty(ref MeterReadingValue, value); }

        }
        private decimal MeterFactorValue;

        public decimal MeterFactor

        {

            get { return this.MeterFactorValue; }

            set { SetProperty(ref MeterFactorValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal BswValue;

        public decimal Bsw

        {

            get { return this.BswValue; }

            set { SetProperty(ref BswValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
        private string MeasurementDeviceValue;

        public string MeasurementDevice

        {

            get { return this.MeasurementDeviceValue; }

            set { SetProperty(ref MeasurementDeviceValue, value); }

        }
        private string NotesValue;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
