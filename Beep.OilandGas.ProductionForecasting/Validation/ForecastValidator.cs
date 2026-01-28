using System;

using Beep.OilandGas.ProductionForecasting.Constants;
using Beep.OilandGas.ProductionForecasting.Exceptions;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Validation
{
    /// <summary>
    /// Provides validation for production forecasting calculations.
    /// </summary>
    public static class ForecastValidator
    {
        /// <summary>
        /// Validates reservoir properties.
        /// </summary>
        public static void ValidateReservoirProperties(RESERVOIR_FORECAST_PROPERTIES reservoir)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (reservoir.INITIAL_PRESSURE <= 0)
                throw new InvalidReservoirPropertiesException("Initial pressure must be greater than zero.");

            if (reservoir.PERMEABILITY <= 0)
                throw new InvalidReservoirPropertiesException("Permeability must be greater than zero.");

            if (reservoir.THICKNESS <= 0)
                throw new InvalidReservoirPropertiesException("Thickness must be greater than zero.");

            if (reservoir.DRAINAGE_RADIUS <= 0)
                throw new InvalidReservoirPropertiesException("Drainage radius must be greater than zero.");

            if (reservoir.WELLBORE_RADIUS <= 0)
                throw new InvalidReservoirPropertiesException("Wellbore radius must be greater than zero.");

            if (reservoir.DRAINAGE_RADIUS <= reservoir.WELLBORE_RADIUS)
                throw new InvalidReservoirPropertiesException("Drainage radius must be greater than wellbore radius.");

            if (reservoir.FORMATION_VOLUME_FACTOR <= 0)
                throw new InvalidReservoirPropertiesException("Formation volume factor must be greater than zero.");

            if (reservoir.OIL_VISCOSITY <= 0)
                throw new InvalidReservoirPropertiesException("Oil viscosity must be greater than zero.");

            if (reservoir.TOTAL_COMPRESSIBILITY <= 0)
                throw new InvalidReservoirPropertiesException("Total compressibility must be greater than zero.");

            if (reservoir.POROSITY <= 0 || reservoir.POROSITY >= 1)
                throw new InvalidReservoirPropertiesException("Porosity must be between 0 and 1.");

            if (reservoir.TEMPERATURE <= 0)
                throw new InvalidReservoirPropertiesException("Temperature must be greater than zero.");
        }

        /// <summary>
        /// Validates forecast duration.
        /// </summary>
        public static void ValidateForecastDuration(decimal forecastDuration)
        {
            if (forecastDuration < ForecastConstants.MinimumForecastDuration)
                throw new ForecastParameterOutOfRangeException(
                    nameof(forecastDuration),
                    $"Forecast duration ({forecastDuration} days) is below minimum ({ForecastConstants.MinimumForecastDuration} days).");

            if (forecastDuration > ForecastConstants.MaximumForecastDuration)
                throw new ForecastParameterOutOfRangeException(
                    nameof(forecastDuration),
                    $"Forecast duration ({forecastDuration} days) exceeds maximum ({ForecastConstants.MaximumForecastDuration} days).");
        }

        /// <summary>
        /// Validates time steps.
        /// </summary>
        public static void ValidateTimeSteps(int timeSteps)
        {
            if (timeSteps < ForecastConstants.MinimumTimeSteps)
                throw new ForecastParameterOutOfRangeException(
                    nameof(timeSteps),
                    $"Number of time steps ({timeSteps}) is below minimum ({ForecastConstants.MinimumTimeSteps}).");

            if (timeSteps > ForecastConstants.MaximumTimeSteps)
                throw new ForecastParameterOutOfRangeException(
                    nameof(timeSteps),
                    $"Number of time steps ({timeSteps}) exceeds maximum ({ForecastConstants.MaximumTimeSteps}).");
        }

        /// <summary>
        /// Validates bottom hole pressure.
        /// </summary>
        public static void ValidateBottomHolePressure(decimal bottomHolePressure, decimal initialPressure)
        {
            if (bottomHolePressure <= 0)
                throw new ForecastParameterOutOfRangeException(
                    nameof(bottomHolePressure),
                    "Bottom hole pressure must be greater than zero.");

            if (bottomHolePressure >= initialPressure)
                throw new ForecastParameterOutOfRangeException(
                    nameof(bottomHolePressure),
                    $"Bottom hole pressure ({bottomHolePressure} psia) must be less than initial pressure ({initialPressure} psia).");
        }

        /// <summary>
        /// Validates all forecast parameters.
        /// </summary>
        public static void ValidateForecastParameters(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal bottomHolePressure,
            decimal forecastDuration,
            int timeSteps)
        {
            ValidateReservoirProperties(reservoir);
            ValidateBottomHolePressure(bottomHolePressure, reservoir.INITIAL_PRESSURE);
            ValidateForecastDuration(forecastDuration);
            ValidateTimeSteps(timeSteps);
        }
    }
}

