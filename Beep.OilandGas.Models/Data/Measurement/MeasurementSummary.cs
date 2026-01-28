using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Measurement
{
    public class MeasurementSummary : ModelEntityBase
    {
        private string WellIdValue;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private int MeasurementCountValue;

        public int MeasurementCount

        {

            get { return this.MeasurementCountValue; }

            set { SetProperty(ref MeasurementCountValue, value); }

        }
        private decimal TotalGrossVolumeValue;

        public decimal TotalGrossVolume

        {

            get { return this.TotalGrossVolumeValue; }

            set { SetProperty(ref TotalGrossVolumeValue, value); }

        }
        private decimal TotalNetVolumeValue;

        public decimal TotalNetVolume

        {

            get { return this.TotalNetVolumeValue; }

            set { SetProperty(ref TotalNetVolumeValue, value); }

        }
        private decimal AverageApiGravityValue;

        public decimal AverageApiGravity

        {

            get { return this.AverageApiGravityValue; }

            set { SetProperty(ref AverageApiGravityValue, value); }

        }
        private decimal AverageBswValue;

        public decimal AverageBsw

        {

            get { return this.AverageBswValue; }

            set { SetProperty(ref AverageBswValue, value); }

        }
        private int ValidatedCountValue;

        public int ValidatedCount

        {

            get { return this.ValidatedCountValue; }

            set { SetProperty(ref ValidatedCountValue, value); }

        }
        private int PendingValidationCountValue;

        public int PendingValidationCount

        {

            get { return this.PendingValidationCountValue; }

            set { SetProperty(ref PendingValidationCountValue, value); }

        }
    }
}
