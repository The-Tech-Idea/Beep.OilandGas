using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Calculations
{
    /// <summary>
    /// Provides pseudo-steady state production forecasting methods.
    /// </summary>
    public static class PseudoSteadyStateForecast
    {
        /// <summary>
        /// Generates single-phase pseudo-steady state production forecast.
        /// </summary>
        /// <param name="reservoir">Reservoir properties.</param>
        /// <param name="bottomHolePressure">Bottom hole pressure in psia.</param>
        /// <param name="forecastDuration">Forecast duration in days.</param>
        /// <param name="timeSteps">Number of time steps.</param>
        /// <returns>Production forecast.</returns>
        public static PRODUCTION_FORECAST GenerateSinglePhaseForecast(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal bottomHolePressure,
            decimal forecastDuration,
            int timeSteps = 100)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (bottomHolePressure <= 0)
                throw new ArgumentException("Bottom hole pressure must be greater than zero.", nameof(bottomHolePressure));

            if (forecastDuration <= 0)
                throw new ArgumentException("Forecast duration must be greater than zero.", nameof(forecastDuration));

            var forecast = new PRODUCTION_FORECAST
            {
                FORECAST_TYPE = ForecastType.PseudoSteadyStateSinglePhase,
                FORECAST_DURATION = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = (decimal)reservoir.INITIAL_PRESSURE;
            decimal cumulativeProduction = 0m;

            // Calculate productivity index
            decimal productivityIndex = CalculateProductivityIndex(reservoir);

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                // Calculate production rate using pseudo-steady state equation
                decimal productionRate = productivityIndex * (currentPressure - bottomHolePressure);

                if (productionRate < 0)
                    productionRate = 0;

                // Calculate pressure decline
                decimal pressureDecline = CalculatePressureDecline(
                    reservoir, productionRate, timeStep, cumulativeProduction);

                currentPressure = Math.Max(bottomHolePressure, currentPressure - pressureDecline);

                // Update cumulative production
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

        /// <summary>
        /// Generates two-phase pseudo-steady state production forecast.
        /// </summary>
        public static PRODUCTION_FORECAST GenerateTwoPhaseForecast(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal bottomHolePressure,
            decimal bubblePointPressure,
            decimal forecastDuration,
            int timeSteps = 100)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (bubblePointPressure <= 0)
                throw new ArgumentException("Bubble point pressure must be greater than zero.", nameof(bubblePointPressure));

            var forecast = new PRODUCTION_FORECAST
            {
                ForecastType = ForecastType.PseudoSteadyStateTwoPhase,
                ForecastDuration = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = (decimal)reservoir.INITIAL_PRESSURE;
            decimal cumulativeProduction = 0m;

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                decimal productionRate;

                if (currentPressure > bubblePointPressure)
                {
                    // Single-phase flow (above bubble point)
                    decimal productivityIndex = CalculateProductivityIndex(reservoir);
                    productionRate = productivityIndex * (currentPressure - bottomHolePressure);
                }
                else
                {
                    // Two-phase flow (below bubble point) - use Vogel equation
                    productionRate = CalculateTwoPhaseRate(
                        reservoir, currentPressure, bottomHolePressure, bubblePointPressure);
                }

                if (productionRate < 0)
                    productionRate = 0;

                // Calculate pressure decline (accounting for two-phase effects)
                decimal pressureDecline = CalculateTwoPhasePressureDecline(
                    reservoir, productionRate, timeStep, cumulativeProduction, currentPressure, bubblePointPressure);

                currentPressure = Math.Max(bottomHolePressure, currentPressure - pressureDecline);

                cumulativeProduction += productionRate * timeStep;

                forecast.ForecastPoints.Add(new FORECAST_POINT
                {
                    Time = time,
                    ProductionRate = productionRate,
                    CumulativeProduction = cumulativeProduction,
                    ReservoirPressure = currentPressure,
                    BottomHolePressure = bottomHolePressure
                });

                if (i == 0)
                    forecast.INITIAL_PRODUCTION_RATE = productionRate;
            }

            forecast.FINAL_PRODUCTION_RATE = forecast.ForecastPoints.Last().PRODUCTION_RATE;
            forecast.TOTAL_CUMULATIVE_PRODUCTION = cumulativeProduction;

            return forecast;
        }

        // Helper methods

        private static decimal CalculateProductivityIndex(RESERVOIR_FORECAST_PROPERTIES reservoir)
        {
            // Pseudo-steady state productivity index
            // J = (0.00708 * k * h) / (B * Î¼ * (ln(re/rw) - 0.75 + S))
            decimal re_rw = (decimal)(reservoir.DRAINAGE_RADIUS / reservoir.WELLBORE_RADIUS);
            decimal ln_re_rw = (decimal)Math.Log((double)re_rw);

            decimal productivityIndex = (decimal)((0.00708m * reservoir.PERMEABILITY * reservoir.THICKNESS) /
                                       (reservoir.FORMATION_VOLUME_FACTOR * reservoir.OIL_VISCOSITY *
                                        (ln_re_rw - 0.75m + reservoir.SKIN_FACTOR)));

            return Math.Max(0.001m, productivityIndex);
        }

        private static decimal CalculatePressureDecline(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal productionRate,
            decimal timeStep,
            decimal cumulativeProduction)
        {
            // Material balance: dP = -q * B / (c_t * V_p)
            decimal poreVolume = (decimal)((decimal)Math.PI * reservoir.DRAINAGE_RADIUS * reservoir.DRAINAGE_RADIUS    *
                                reservoir.THICKNESS * reservoir.POROSITY);

            decimal pressureDecline = (decimal)((productionRate * reservoir.FORMATION_VOLUME_FACTOR * timeStep) /
                                     (reservoir.TOTAL_COMPRESSIBILITY * poreVolume));

            return pressureDecline;
        }

        private static decimal CalculateTwoPhaseRate(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal reservoirPressure,
            decimal bottomHolePressure,
            decimal bubblePointPressure)
        {
            // Vogel equation for two-phase flow
            // q = q_max * [1 - 0.2*(Pwf/Pb) - 0.8*(Pwf/Pb)Â²]
            // where q_max is at Pwf = 0

            decimal productivityIndex = CalculateProductivityIndex(reservoir);
            decimal qMax = productivityIndex * bubblePointPressure / 1.8m;

            decimal pwfRatio = bottomHolePressure / bubblePointPressure;
            if (pwfRatio > 1.0m)
                pwfRatio = 1.0m;

            decimal qRatio = 1.0m - 0.2m * pwfRatio - 0.8m * pwfRatio * pwfRatio;
            decimal productionRate = qMax * qRatio * (reservoirPressure / bubblePointPressure);

            return Math.Max(0m, productionRate);
        }

        private static decimal CalculateTwoPhasePressureDecline(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal productionRate,
            decimal timeStep,
            decimal cumulativeProduction,
            decimal currentPressure,
            decimal bubblePointPressure)
        {
            // Enhanced pressure decline calculation for two-phase flow
            decimal baseDecline = CalculatePressureDecline(reservoir, productionRate, timeStep, cumulativeProduction);

            // Adjust for two-phase effects
            if (currentPressure < bubblePointPressure)
            {
                // Increased compressibility in two-phase region
                decimal twoPhaseCompressibility = (decimal)(reservoir.TOTAL_COMPRESSIBILITY * 1.5m);
                decimal poreVolume = (decimal)((decimal)Math.PI * reservoir.DRAINAGE_RADIUS * reservoir.DRAINAGE_RADIUS *
                                    reservoir.THICKNESS * reservoir.POROSITY);

                baseDecline = (decimal)(productionRate * reservoir.FORMATION_VOLUME_FACTOR * timeStep /
                             (twoPhaseCompressibility * poreVolume));
            }

            return baseDecline;
        }
    }
}

