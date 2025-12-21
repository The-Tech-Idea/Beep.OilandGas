using System;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.Operational.Production
{
    /// <summary>
    /// Provides manual measurement methods (tank gauging).
    /// </summary>
    public static class ManualMeasurement
    {
        /// <summary>
        /// Performs tank gauging measurement.
        /// </summary>
        /// <param name="tank">The tank to measure.</param>
        /// <param name="gaugeHeight">The gauge height in inches.</param>
        /// <param name="temperature">The temperature in degrees Fahrenheit.</param>
        /// <param name="bswSample">BS&W sample percentage (0-100).</param>
        /// <returns>Measurement record.</returns>
        public static MeasurementRecord PerformTankGauging(
            Storage.Tank tank,
            decimal gaugeHeight,
            decimal temperature,
            decimal bswSample)
        {
            if (tank == null)
                throw new ArgumentNullException(nameof(tank));

            if (gaugeHeight < 0)
                throw new InvalidMeasurementDataException(nameof(gaugeHeight), "Gauge height cannot be negative.");

            // Calculate volume from gauge height using tank strapping table
            // Simplified calculation - full implementation would use actual strapping table
            decimal grossVolume = CalculateVolumeFromGaugeHeight(tank, gaugeHeight);

            var record = new MeasurementRecord
            {
                MeasurementId = Guid.NewGuid().ToString(),
                MeasurementDateTime = DateTime.Now,
                Method = MeasurementMethod.Manual,
                GrossVolume = grossVolume,
                BSW = bswSample,
                Temperature = temperature,
                ApiGravity = tank.ApiGravity,
                MeasurementDevice = "Tank Gauge",
                Notes = $"Manual tank gauging at {gaugeHeight} inches"
            };

            return record;
        }

        /// <summary>
        /// Calculates volume from gauge height using tank strapping table.
        /// Simplified implementation - full version would use actual strapping data.
        /// </summary>
        private static decimal CalculateVolumeFromGaugeHeight(Storage.Tank tank, decimal gaugeHeight)
        {
            // Simplified: assume cylindrical tank
            // Full implementation would use tank strapping table
            // Volume = (π * r² * h) / 231 (cubic inches to gallons) / 42 (gallons to barrels)
            // For now, use linear approximation based on capacity
            decimal maxHeight = 20m; // Assume 20 feet max height
            decimal heightRatio = gaugeHeight / (maxHeight * 12m); // Convert to inches
            return tank.Capacity * heightRatio;
        }

        /// <summary>
        /// Performs manual sampling for quality measurement.
        /// </summary>
        public static CrudeOilProperties PerformManualSampling(
            decimal apiGravity,
            decimal sulfurContent,
            decimal bsw,
            decimal temperature)
        {
            return new CrudeOilProperties
            {
                ApiGravity = apiGravity,
                SulfurContent = sulfurContent,
                BSW = bsw,
                MeasurementTemperature = temperature,
                MeasurementDate = DateTime.Now
            };
        }
    }
}

