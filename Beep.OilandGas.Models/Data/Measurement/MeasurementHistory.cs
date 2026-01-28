using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Measurement
{
    public class MeasurementHistory : ModelEntityBase
    {
        private string MeasurementIdValue;

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
        private string MeasurementMethodValue;

        public string MeasurementMethod

        {

            get { return this.MeasurementMethodValue; }

            set { SetProperty(ref MeasurementMethodValue, value); }

        }
        private decimal GrossVolumeValue;

        public decimal GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
        private string ValidationStatusValue;

        public string ValidationStatus

        {

            get { return this.ValidationStatusValue; }

            set { SetProperty(ref ValidationStatusValue, value); }

        }
    }
}
