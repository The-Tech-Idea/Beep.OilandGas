using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Measurement
{
    public class RecordManualMeasurementRequest : ModelEntityBase
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
        private string TankIdValue;

        public string TankId

        {

            get { return this.TankIdValue; }

            set { SetProperty(ref TankIdValue, value); }

        }
        private decimal GaugeHeightValue;

        public decimal GaugeHeight

        {

            get { return this.GaugeHeightValue; }

            set { SetProperty(ref GaugeHeightValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal BswSampleValue;

        public decimal BswSample

        {

            get { return this.BswSampleValue; }

            set { SetProperty(ref BswSampleValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
        private string OperatorValue;

        public string Operator

        {

            get { return this.OperatorValue; }

            set { SetProperty(ref OperatorValue, value); }

        }
        private string NotesValue;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
