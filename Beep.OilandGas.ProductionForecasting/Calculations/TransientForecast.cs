using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ProductionForecasting;


namespace Beep.OilandGas.ProductionForecasting.Calculations
{
    /// <summary>
    /// Provides transient production forecasting methods.
    /// </summary>
    public static class TransientForecast
    {
        /// <summary>
        /// Generates transient production forecast.
        /// </summary>
        /// <param name="reservoir">Reservoir properties.</param>
        /// <param name="bottomHolePressure">Bottom hole pressure in psia.</param>
        /// <param name="forecastDuration">Forecast duration in days.</param>
        /// <param name="timeSteps">Number of time steps.</param>
        /// <returns>Production forecast.</returns>
        public static PRODUCTION_FORECAST GenerateTransientForecast(
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
                FORECAST_TYPE = ForecastType.Transient,
                FORECAST_DURATION = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = (decimal)reservoir.INITIAL_PRESSURE;
            decimal cumulativeProduction = 0m;

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                // Calculate production rate using transient flow equation
                decimal productionRate = CalculateTransientRate(
                    reservoir, currentPressure, bottomHolePressure, time);

                if (productionRate < 0)
                    productionRate = 0;

                // Calculate pressure decline
                decimal pressureDecline = CalculateTransientPressureDecline(
                    reservoir, productionRate, timeStep, time);

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

        private static decimal CalculateTransientRate(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal reservoirPressure,
            decimal bottomHolePressure,
            decimal time)
        {
            // Transient flow equation
            // q = (0.00708 * k * h * (Pr - Pwf)) / (B * μ * (ln(t) + 0.80907 + 2*S))
            // Simplified version

            decimal permeability = (decimal)reservoir.PERMEABILITY;
            decimal thickness = (decimal)reservoir.THICKNESS;
            decimal pressureDiff = reservoirPressure - bottomHolePressure;
            decimal formationVolumeFactor = (decimal)reservoir.FORMATION_VOLUME_FACTOR;
            decimal viscosity = (decimal)reservoir.OIL_VISCOSITY;

            if (time <= 0)
                time = 0.001m; // Avoid log(0)

            decimal lnTime = (decimal)Math.Log((double)time);
            decimal denominator = (decimal)(formationVolumeFactor * viscosity * (lnTime + 0.80907m + 2m * reservoir.SKIN_FACTOR));

            if (denominator <= 0)
                denominator = 0.001m;

            decimal productionRate = (0.00708m * permeability * thickness * pressureDiff) / denominator;

            return Math.Max(0m, productionRate);
        }

        private static decimal CalculateTransientPressureDecline(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal productionRate,
            decimal timeStep,
            decimal time)
        {
            // Transient pressure decline
            // Uses exponential integral (Ei) approximation for early time
            // Simplified using logarithmic approximation

            decimal diffusivity = (decimal)(reservoir.PERMEABILITY / 
                                (reservoir.POROSITY * reservoir.TOTAL_COMPRESSIBILITY * reservoir.OIL_VISCOSITY));

            decimal timeInSeconds = time * 86400m; // Convert days to seconds
            decimal radiusSquared = (decimal)(reservoir.WELLBORE_RADIUS * reservoir.WELLBORE_RADIUS);

            // Dimensionless time
            decimal tD = (decimal)(0.0002637m * reservoir.PERMEABILITY * timeInSeconds /
                        (reservoir.POROSITY * reservoir.TOTAL_COMPRESSIBILITY * reservoir.OIL_VISCOSITY * radiusSquared));

            // Pressure decline using line source solution approximation
            decimal pressureDecline = 0m;

            if (tD > 100m)
            {
                // Pseudo-steady state approximation
                decimal poreVolume = (decimal)((decimal)Math.PI * reservoir.DRAINAGE_RADIUS * reservoir.DRAINAGE_RADIUS *
                                    reservoir.THICKNESS * reservoir.POROSITY);
                pressureDecline = (decimal)(productionRate * reservoir.FORMATION_VOLUME_FACTOR * timeStep /
                                (reservoir.TOTAL_COMPRESSIBILITY * poreVolume));
            }
            else
            {
                // Transient approximation
                decimal ln_tD = (decimal)Math.Log((double)tD);
                decimal pressureDrop = (decimal)(162.6m * productionRate * reservoir.FORMATION_VOLUME_FACTOR * reservoir.OIL_VISCOSITY /
                                     (reservoir.PERMEABILITY * reservoir.THICKNESS) *
                                     (ln_tD + 0.80907m + 2m * reservoir.SKIN_FACTOR));

                pressureDecline = pressureDrop * (timeStep / time);
            }

            return Math.Max(0m, pressureDecline);
        }
    }
}

