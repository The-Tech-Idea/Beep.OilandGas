using System;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Measurement standard organization.
    /// </summary>
    public enum MeasurementStandard
    {
        /// <summary>
        /// American Petroleum Institute standards.
        /// </summary>
        API,

        /// <summary>
        /// American Gas Association standards.
        /// </summary>
        AGA,

        /// <summary>
        /// International Organization for Standardization.
        /// </summary>
        ISO
    }

    /// <summary>
    /// Method of measurement.
    /// </summary>
    public enum MeasurementMethod
    {
        /// <summary>
        /// Manual measurement (tank gauging, manual sampling).
        /// </summary>
        Manual,

        /// <summary>
        /// Automatic metering (flow meters).
        /// </summary>
        Automatic,

        /// <summary>
        /// Automatic Custody Transfer.
        /// </summary>
        ACT,

        /// <summary>
        /// Lease Automatic Custody Transfer.
        /// </summary>
        LACT
    }

    /// <summary>
    /// Represents a measurement record (DTO for calculations/reporting).
    /// </summary>
    public class MeasurementRecordDto
    {
        /// <summary>
        /// Gets or sets the measurement identifier.
        /// </summary>
        public string MeasurementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the measurement date and time.
        /// </summary>
        public DateTime MeasurementDateTime { get; set; }

        /// <summary>
        /// Gets or sets the measurement method.
        /// </summary>
        public MeasurementMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the measurement standard.
        /// </summary>
        public MeasurementStandard Standard { get; set; } = MeasurementStandard.API;

        /// <summary>
        /// Gets or sets the gross volume in barrels.
        /// </summary>
        public decimal GrossVolume { get; set; }

        /// <summary>
        /// Gets or sets the BS&W percentage (0-100).
        /// </summary>
        public decimal BSW { get; set; }

        /// <summary>
        /// Gets the net volume in barrels.
        /// </summary>
        public decimal NetVolume => GrossVolume * (1m - BSW / 100m);

        /// <summary>
        /// Gets or sets the temperature in degrees Fahrenheit.
        /// </summary>
        public decimal Temperature { get; set; } = 60m;

        /// <summary>
        /// Gets or sets the pressure in psi.
        /// </summary>
        public decimal? Pressure { get; set; }

        /// <summary>
        /// Gets or sets the API gravity.
        /// </summary>
        public decimal? ApiGravity { get; set; }

        /// <summary>
        /// Gets or sets the crude oil properties.
        /// </summary>
        public CrudeOilPropertiesDto? Properties { get; set; }

        /// <summary>
        /// Gets or sets the measurement accuracy percentage.
        /// </summary>
        public decimal? Accuracy { get; set; }

        /// <summary>
        /// Gets or sets the measurement device or equipment used.
        /// </summary>
        public string? MeasurementDevice { get; set; }

        /// <summary>
        /// Gets or sets the operator or person who performed the measurement.
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// Gets or sets any notes or comments.
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Represents measurement accuracy requirements.
    /// </summary>
    public class MeasurementAccuracyDto
    {
        /// <summary>
        /// Gets or sets the minimum required accuracy percentage.
        /// </summary>
        public decimal MinimumAccuracy { get; set; } = 99.5m;

        /// <summary>
        /// Gets or sets the maximum allowed error percentage.
        /// </summary>
        public decimal MaximumError { get; set; } = 0.5m;

        /// <summary>
        /// Gets or sets whether calibration is required.
        /// </summary>
        public bool CalibrationRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the calibration frequency in days.
        /// </summary>
        public int CalibrationFrequencyDays { get; set; } = 90;
    }

    /// <summary>
    /// Represents measurement corrections.
    /// </summary>
    public class MeasurementCorrectionsDto
    {
        /// <summary>
        /// Gets or sets the temperature correction factor.
        /// </summary>
        public decimal TemperatureCorrectionFactor { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the pressure correction factor.
        /// </summary>
        public decimal PressureCorrectionFactor { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the meter factor (calibration correction).
        /// </summary>
        public decimal MeterFactor { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the shrinkage factor.
        /// </summary>
        public decimal ShrinkageFactor { get; set; } = 1.0m;

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




