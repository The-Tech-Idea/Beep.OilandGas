using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Measurement standard organization.
    /// </summary>


    /// <summary>
    /// Method of measurement.
    /// </summary>


    /// <summary>
    /// Represents a measurement class (DTO for calculations/reporting).
    /// </summary>
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

    /// <summary>
    /// Represents measurement accuracy requirements.
    /// </summary>
    public class MeasurementAccuracy : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the minimum required accuracy percentage.
        /// </summary>
        private decimal MinimumAccuracyValue = 99.5m;

        public decimal MinimumAccuracy

        {

            get { return this.MinimumAccuracyValue; }

            set { SetProperty(ref MinimumAccuracyValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum allowed error percentage.
        /// </summary>
        private decimal MaximumErrorValue = 0.5m;

        public decimal MaximumError

        {

            get { return this.MaximumErrorValue; }

            set { SetProperty(ref MaximumErrorValue, value); }

        }

        /// <summary>
        /// Gets or sets whether calibration is required.
        /// </summary>
        private bool CalibrationRequiredValue = true;

        public bool CalibrationRequired

        {

            get { return this.CalibrationRequiredValue; }

            set { SetProperty(ref CalibrationRequiredValue, value); }

        }

        /// <summary>
        /// Gets or sets the calibration frequency in days.
        /// </summary>
        private int CalibrationFrequencyDaysValue = 90;

        public int CalibrationFrequencyDays

        {

            get { return this.CalibrationFrequencyDaysValue; }

            set { SetProperty(ref CalibrationFrequencyDaysValue, value); }

        }
    }

    /// <summary>
    /// Represents measurement corrections.
    /// </summary>
    public class MeasurementCorrections : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the temperature correction factor.
        /// </summary>
        private decimal TemperatureCorrectionFactorValue = 1.0m;

        public decimal TemperatureCorrectionFactor

        {

            get { return this.TemperatureCorrectionFactorValue; }

            set { SetProperty(ref TemperatureCorrectionFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the pressure correction factor.
        /// </summary>
        private decimal PressureCorrectionFactorValue = 1.0m;

        public decimal PressureCorrectionFactor

        {

            get { return this.PressureCorrectionFactorValue; }

            set { SetProperty(ref PressureCorrectionFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the meter factor (calibration correction).
        /// </summary>
        private decimal MeterFactorValue = 1.0m;

        public decimal MeterFactor

        {

            get { return this.MeterFactorValue; }

            set { SetProperty(ref MeterFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the shrinkage factor.
        /// </summary>
        private decimal ShrinkageFactorValue = 1.0m;

        public decimal ShrinkageFactor

        {

            get { return this.ShrinkageFactorValue; }

            set { SetProperty(ref ShrinkageFactorValue, value); }

        }

        /// <summary>
        /// Applies all corrections to a volume.
        /// </summary>
        public decimal ApplyCorrections(decimal volume)
        {
            return volume * TemperatureCorrectionFactor
                         * PressureCorrectionFactor
                         * MeterFactor
                         * ShrinkageFactor;
        }
    }
}









