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
            decimal currentPressure = reservoir.InitialPressure;
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

                forecast.ForecastPoints.Add(new ForecastPoint
                {
                    Time = time,
                    ProductionRate = productionRate,
                    CumulativeProduction = cumulativeProduction,
                    ReservoirPressure = currentPressure,
                    BottomHolePressure = bottomHolePressure
                });

                if (i == 0)
                    forecast.InitialProductionRate = productionRate;
            }

            forecast.FinalProductionRate = forecast.ForecastPoints.Last().ProductionRate;
            forecast.TotalCumulativeProduction = cumulativeProduction;

            return forecast;
        }

        /// <summary>
        /// Generates two-phase pseudo-steady state production forecast.
        /// </summary>
        public static ProductionForecast GenerateTwoPhaseForecast(
            ReservoirForecastProperties reservoir,
            decimal bottomHolePressure,
            decimal bubblePointPressure,
            decimal forecastDuration,
            int timeSteps = 100)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (bubblePointPressure <= 0)
                throw new ArgumentException("Bubble point pressure must be greater than zero.", nameof(bubblePointPressure));

            var forecast = new ProductionForecast
            {
                ForecastType = ForecastType.PseudoSteadyStateTwoPhase,
                ForecastDuration = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = reservoir.InitialPressure;
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

                forecast.ForecastPoints.Add(new ForecastPoint
                {
                    Time = time,
                    ProductionRate = productionRate,
                    CumulativeProduction = cumulativeProduction,
                    ReservoirPressure = currentPressure,
                    BottomHolePressure = bottomHolePressure
                });

                if (i == 0)
                    forecast.InitialProductionRate = productionRate;
            }

            forecast.FinalProductionRate = forecast.ForecastPoints.Last().ProductionRate;
            forecast.TotalCumulativeProduction = cumulativeProduction;

            return forecast;
        }

        // Helper methods

        private static decimal CalculateProductivityIndex(ReservoirForecastProperties reservoir)
        {
            // Pseudo-steady state productivity index
            // J = (0.00708 * k * h) / (B * μ * (ln(re/rw) - 0.75 + S))
            decimal re_rw = reservoir.DrainageRadius / reservoir.WellboreRadius;
            decimal ln_re_rw = (decimal)Math.Log((double)re_rw);

            decimal productivityIndex = (0.00708m * reservoir.Permeability * reservoir.Thickness) /
                                       (reservoir.FormationVolumeFactor * reservoir.OilViscosity *
                                        (ln_re_rw - 0.75m + reservoir.SkinFactor));

            return Math.Max(0.001m, productivityIndex);
        }

        private static decimal CalculatePressureDecline(
            ReservoirForecastProperties reservoir,
            decimal productionRate,
            decimal timeStep,
            decimal cumulativeProduction)
        {
            // Material balance: dP = -q * B / (c_t * V_p)
            decimal poreVolume = (decimal)Math.PI * reservoir.DrainageRadius * reservoir.DrainageRadius *
                                reservoir.Thickness * reservoir.Porosity;

            decimal pressureDecline = (productionRate * reservoir.FormationVolumeFactor * timeStep) /
                                     (reservoir.TotalCompressibility * poreVolume);

            return pressureDecline;
        }

        private static decimal CalculateTwoPhaseRate(
            ReservoirForecastProperties reservoir,
            decimal reservoirPressure,
            decimal bottomHolePressure,
            decimal bubblePointPressure)
        {
            // Vogel equation for two-phase flow
            // q = q_max * [1 - 0.2*(Pwf/Pb) - 0.8*(Pwf/Pb)²]
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
            ReservoirForecastProperties reservoir,
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
                decimal twoPhaseCompressibility = reservoir.TotalCompressibility * 1.5m;
                decimal poreVolume = (decimal)Math.PI * reservoir.DrainageRadius * reservoir.DrainageRadius *
                                    reservoir.Thickness * reservoir.Porosity;

                baseDecline = (productionRate * reservoir.FormationVolumeFactor * timeStep) /
                             (twoPhaseCompressibility * poreVolume);
            }

            return baseDecline;
        }
    }
}

