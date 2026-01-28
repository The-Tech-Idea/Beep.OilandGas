using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class MeasurementRecord : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the measurement identifier.
        /// </summary>
        private string MeasurementIdValue = string.Empty;

        public string MeasurementId

        {

            get { return this.MeasurementIdValue; }

            set { SetProperty(ref MeasurementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the measurement date and time.
        /// </summary>
        private DateTime MeasurementDateTimeValue;

        public DateTime MeasurementDateTime

        {

            get { return this.MeasurementDateTimeValue; }

            set { SetProperty(ref MeasurementDateTimeValue, value); }

        }

        /// <summary>
        /// Gets or sets the measurement method.
        /// </summary>
        private MeasurementMethod MethodValue;

        public MeasurementMethod Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the measurement standard.
        /// </summary>
        private MeasurementStandard StandardValue = MeasurementStandard.API;

        public MeasurementStandard Standard

        {

            get { return this.StandardValue; }

            set { SetProperty(ref StandardValue, value); }

        }

        /// <summary>
        /// Gets or sets the gross volume in barrels.
        /// </summary>
        private decimal GrossVolumeValue;

        public decimal GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the BS&W percentage (0-100).
        /// </summary>
        private decimal BSWValue;

        public decimal BSW

        {

            get { return this.BSWValue; }

            set { SetProperty(ref BSWValue, value); }

        }

        /// <summary>
        /// Gets the net volume in barrels.
        /// </summary>
        public decimal NetVolume => GrossVolume * (1m - BSW / 100m);

        /// <summary>
        /// Gets or sets the temperature in degrees Fahrenheit.
        /// </summary>
        private decimal TemperatureValue = 60m;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }

        /// <summary>
        /// Gets or sets the pressure in psi.
        /// </summary>
        private decimal? PressureValue;

        public decimal? Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        /// <summary>
        /// Gets or sets the API gravity.
        /// </summary>
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }

        /// <summary>
        /// Gets or sets the crude oil properties.
        /// </summary>
        private CrudeOilProperties? PropertiesValue;

        public CrudeOilProperties? Properties

        {

            get { return this.PropertiesValue; }

            set { SetProperty(ref PropertiesValue, value); }

        }

        /// <summary>
        /// Gets or sets the measurement accuracy percentage.
        /// </summary>
        private decimal? AccuracyValue;

        public decimal? Accuracy

        {

            get { return this.AccuracyValue; }

            set { SetProperty(ref AccuracyValue, value); }

        }

        /// <summary>
        /// Gets or sets the measurement device or equipment used.
        /// </summary>
        private string? MeasurementDeviceValue;

        public string? MeasurementDevice

        {

            get { return this.MeasurementDeviceValue; }

            set { SetProperty(ref MeasurementDeviceValue, value); }

        }

        /// <summary>
        /// Gets or sets the operator or person who performed the measurement.
        /// </summary>
        private string? OperatorValue;

        public string? Operator

        {

            get { return this.OperatorValue; }

            set { SetProperty(ref OperatorValue, value); }

        }

        /// <summary>
        /// Gets or sets any notes or comments.
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
