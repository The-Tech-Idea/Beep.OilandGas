
using Beep.OilandGas.ProductionAccounting.Measurement;

namespace Beep.OilandGas.ProductionAccounting.Measurement
{
    /// <summary>
    /// Provides measurement standards and requirements.
    /// </summary>
    public static class MeasurementStandards
    {
        /// <summary>
        /// Gets API measurement standards requirements.
        /// </summary>
        public static MeasurementAccuracy GetApiStandards()
        {
            return new MeasurementAccuracy
            {
                MinimumAccuracy = 99.5m,
                MaximumError = 0.5m,
                CalibrationRequired = true,
                CalibrationFrequencyDays = 90
            };
        }

        /// <summary>
        /// Gets AGA measurement standards requirements.
        /// </summary>
        public static MeasurementAccuracy GetAgaStandards()
        {
            return new MeasurementAccuracy
            {
                MinimumAccuracy = 99.0m,
                MaximumError = 1.0m,
                CalibrationRequired = true,
                CalibrationFrequencyDays = 90
            };
        }

        /// <summary>
        /// Gets ISO measurement standards requirements.
        /// </summary>
        public static MeasurementAccuracy GetIsoStandards()
        {
            return new MeasurementAccuracy
            {
                MinimumAccuracy = 99.5m,
                MaximumError = 0.5m,
                CalibrationRequired = true,
                CalibrationFrequencyDays = 90
            };
        }

        /// <summary>
        /// Gets measurement standards for a specific standard organization.
        /// </summary>
        public static MeasurementAccuracy GetStandards(MeasurementStandard standard)
        {
            return standard switch
            {
                MeasurementStandard.API => GetApiStandards(),
                MeasurementStandard.AGA => GetAgaStandards(),
                MeasurementStandard.ISO => GetIsoStandards(),
                _ => GetApiStandards() // Default to API
            };
        }

        /// <summary>
        /// Validates measurement against standards.
        /// </summary>
        public static bool ValidateMeasurement(MeasurementRecord record, MeasurementStandard standard)
        {
            var requirements = GetStandards(standard);
            return AutomaticMeasurement.ValidateAccuracy(record, requirements);
        }
    }
}
