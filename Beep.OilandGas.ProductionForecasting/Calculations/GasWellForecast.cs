using System;
using System.Collections.Generic;
using System.Linq;

using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Calculations
{
    /// <summary>
    /// Provides gas well production forecasting methods.
    /// </summary>
    public static class GasWellForecast
    {
        /// <summary>
        /// Generates gas well production forecast.
        /// </summary>
        /// <param name="reservoir">Reservoir properties.</param>
        /// <param name="bottomHolePressure">Bottom hole pressure in psia.</param>
        /// <param name="forecastDuration">Forecast duration in days.</param>
        /// <param name="timeSteps">Number of time steps.</param>
        /// <returns>Production forecast (rates in Mscf/day).</returns>
        public static PRODUCTION_FORECAST GenerateGasWellForecast(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal bottomHolePressure,
            decimal forecastDuration,
            int timeSteps = 100)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (bottomHolePressure <= 0)
                throw new ArgumentException("Bottom hole pressure must be greater than zero.", nameof(bottomHolePressure));

            var forecast = new PRODUCTION_FORECAST
            {
                FORECAST_TYPE = ForecastType.GasWell,
                FORECAST_DURATION = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = (decimal)reservoir.INITIAL_PRESSURE;
            decimal cumulativeProduction = 0m;

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                // Calculate Z-factor at current conditions
                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    currentPressure, (decimal)reservoir.TEMPERATURE, reservoir.GAS_SPECIFIC_GRAVITY);

                // Calculate gas production rate using pseudo-steady state equation
                decimal productionRate = CalculateGasRate(
                    reservoir, currentPressure, bottomHolePressure, zFactor);

                if (productionRate < 0)
                    productionRate = 0;

                // Calculate pressure decline
                decimal pressureDecline = CalculateGasPressureDecline(
                    reservoir, productionRate, timeStep, cumulativeProduction, zFactor, currentPressure);

                currentPressure = Math.Max(bottomHolePressure, currentPressure - pressureDecline);

                cumulativeProduction += productionRate * timeStep;

                forecast.FORECAST_POINTS.Add(new FORECAST_POINT
                {
                    TIME = time,
                    PRODUCTION_RATE = productionRate,
                    CUMULATIVE_PRODUCTION = cumulativeProduction,
                    RESERVOIR_PRESSURE = currentPressure,
                    BOTTOM_HOLE_PRESSURE = bottomHolePressure
                });

                if (i == 0)
                    forecast.INITIAL_PRODUCTION_RATE = productionRate;
            }

            forecast.FINAL_PRODUCTION_RATE = forecast.FORECAST_POINTS.Last().PRODUCTION_RATE;
            forecast.TOTAL_CUMULATIVE_PRODUCTION = cumulativeProduction;

            return forecast;
        }

        // Helper methods

        private static decimal CalculateGasRate(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal reservoirPressure,
            decimal bottomHolePressure,
            decimal zFactor)
        {
            // Gas well deliverability equation (pseudo-steady state)
            // q_g = (0.703 * k * h * (Pr² - Pwf²)) / (μ_g * Z * T * (ln(re/rw) - 0.75 + S))

            decimal permeability = (decimal)reservoir.PERMEABILITY;
            decimal thickness = (decimal)reservoir.THICKNESS;
            decimal pressureSquaredDiff = reservoirPressure * reservoirPressure - 
                                         bottomHolePressure * bottomHolePressure;
            decimal temperature = (decimal)reservoir.TEMPERATURE;

            // Gas viscosity (simplified - would use gas properties library)
            decimal gasViscosity = 0.02m; // cp (approximate)

            decimal re_rw = (decimal)(reservoir.DRAINAGE_RADIUS / reservoir.WELLBORE_RADIUS);
            decimal ln_re_rw = (decimal)Math.Log((double)re_rw);

            decimal denominator = (decimal)(gasViscosity * zFactor * temperature * 
                                (ln_re_rw - 0.75m + reservoir.SKIN_FACTOR));

            if (denominator <= 0)
                denominator = 0.001m;

            decimal productionRate = (0.703m * permeability * thickness * pressureSquaredDiff) / denominator;

            // Convert to Mscf/day (assuming standard conditions)
            return Math.Max(0m, productionRate / 1000m);
        }

        private static decimal CalculateGasPressureDecline(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal productionRate,
            decimal timeStep,
            decimal cumulativeProduction,
            decimal zFactor,
            decimal currentPressure)
        {
            // Material balance for gas: P/Z = (Pi/Zi) * (1 - Gp/Gi)
            // Simplified pressure decline calculation

            decimal poreVolume = (decimal)((decimal)Math.PI * reservoir.DRAINAGE_RADIUS * reservoir.DRAINAGE_RADIUS *
                                reservoir.THICKNESS * reservoir.POROSITY);

            // Gas formation volume factor
            decimal gasFormationVolumeFactor = CalculateGasFormationVolumeFactor(
                currentPressure, (decimal)reservoir.TEMPERATURE, zFactor);

            // Pressure decline
            decimal pressureDecline = (decimal)((productionRate * gasFormationVolumeFactor * timeStep) /
                                     (reservoir.TOTAL_COMPRESSIBILITY * poreVolume));

            return Math.Max(0m, pressureDecline);
        }

        private static decimal CalculateGasFormationVolumeFactor(
            decimal pressure,
            decimal temperature,
            decimal zFactor)
        {
            // Bg = 0.02827 * Z * T / P (in ft³/scf)
            // Convert to appropriate units
            decimal bg = 0.02827m * zFactor * temperature / pressure;
            return bg;
        }
    }
}

