using System;
using Beep.OilandGas.Accounting.Operational.Production;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.Operational.Production
{
    /// <summary>
    /// Provides automatic measurement methods (flow meters, LACT units).
    /// </summary>
    public static class AutomaticMeasurement
    {
        /// <summary>
        /// Performs automatic metering measurement.
        /// </summary>
        /// <param name="meterReading">The meter reading in barrels.</param>
        /// <param name="meterFactor">The meter calibration factor.</param>
        /// <param name="temperature">The temperature in degrees Fahrenheit.</param>
        /// <param name="bsw">BS&W percentage (0-100).</param>
        /// <param name="apiGravity">API gravity.</param>
        /// <returns>Measurement record.</returns>
        public static MeasurementRecord PerformAutomaticMetering(
            decimal meterReading,
            decimal meterFactor,
            decimal temperature,
            decimal bsw,
            decimal? apiGravity = null)
        {
            if (meterReading < 0)
                throw new InvalidMeasurementDataException(nameof(meterReading), "Meter reading cannot be negative.");

            if (meterFactor <= 0)
                throw new InvalidMeasurementDataException(nameof(meterFactor), "Meter factor must be greater than zero.");

            // Apply meter factor
            decimal correctedVolume = meterReading * meterFactor;

            // Apply temperature correction
            decimal temperatureCorrection = CalculateTemperatureCorrection(temperature, apiGravity ?? 40m);
            decimal grossVolume = correctedVolume * temperatureCorrection;

            var record = new MeasurementRecord
            {
                MeasurementId = Guid.NewGuid().ToString(),
                MeasurementDateTime = DateTime.Now,
                Method = MeasurementMethod.Automatic,
                GrossVolume = grossVolume,
                BSW = bsw,
                Temperature = temperature,
                ApiGravity = apiGravity,
                MeasurementDevice = "Flow Meter",
                Notes = $"Automatic metering with meter factor {meterFactor}"
            };

            return record;
        }

        /// <summary>
        /// Performs LACT (Lease Automatic Custody Transfer) measurement.
        /// </summary>
        public static MeasurementRecord PerformLACTMeasurement(
            Storage.LACTUnit lactUnit,
            decimal meterReading,
            decimal bsw,
            decimal temperature,
            decimal? apiGravity = null)
        {
            if (lactUnit == null)
                throw new ArgumentNullException(nameof(lactUnit));

            if (!lactUnit.IsActive)
                throw new InvalidOperationException("LACT unit is not active.");

            // Use LACT meter configuration
            decimal meterFactor = lactUnit.MeterConfiguration.MeterFactor;
            var record = PerformAutomaticMetering(meterReading, meterFactor, temperature, bsw, apiGravity);
            record.Method = MeasurementMethod.LACT;
            record.MeasurementDevice = $"LACT Unit {lactUnit.LACTId}";

            // Check quality limits
            if (lactUnit.QualityMeasurement.AutomaticShutoff && 
                bsw > lactUnit.QualityMeasurement.MaximumAllowedBSW)
            {
                record.Notes += $" - WARNING: BS&W {bsw}% exceeds maximum {lactUnit.QualityMeasurement.MaximumAllowedBSW}%";
            }

            return record;
        }

        /// <summary>
        /// Calculates temperature correction factor.
        /// </summary>
        private static decimal CalculateTemperatureCorrection(decimal temperature, decimal apiGravity)
        {
            // Simplified temperature correction
            // Full implementation would use ASTM Table 6
            decimal standardTemp = 60m;
            decimal tempDiff = temperature - standardTemp;
            decimal correctionFactor = 1m - (tempDiff * 0.00036m); // Approximate
            return correctionFactor;
        }

        /// <summary>
        /// Validates measurement accuracy.
        /// </summary>
        public static bool ValidateAccuracy(MeasurementRecord record, MeasurementAccuracy requirements)
        {
            if (record.Accuracy == null)
                return false;

            return record.Accuracy >= requirements.MinimumAccuracy;
        }
    }
}

