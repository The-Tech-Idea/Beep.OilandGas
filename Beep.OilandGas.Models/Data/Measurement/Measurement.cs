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

    public class MeasurementValidationResult : ModelEntityBase
    {
        private string MeasurementIdValue;

        public string MeasurementId

        {

            get { return this.MeasurementIdValue; }

            set { SetProperty(ref MeasurementIdValue, value); }

        }
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }
        private List<string> ValidationErrorsValue = new();

        public List<string> ValidationErrors

        {

            get { return this.ValidationErrorsValue; }

            set { SetProperty(ref ValidationErrorsValue, value); }

        }
        private List<string> ValidationWarningsValue = new();

        public List<string> ValidationWarnings

        {

            get { return this.ValidationWarningsValue; }

            set { SetProperty(ref ValidationWarningsValue, value); }

        }
        private DateTime ValidationDateValue = DateTime.UtcNow;

        public DateTime ValidationDate

        {

            get { return this.ValidationDateValue; }

            set { SetProperty(ref ValidationDateValue, value); }

        }
        private string ValidatedByValue;

        public string ValidatedBy

        {

            get { return this.ValidatedByValue; }

            set { SetProperty(ref ValidatedByValue, value); }

        }
    }

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








