using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Calculations
{
    /// <summary>
    /// Provides transient and boundary-dominated flow production forecasting methods.
    /// Implements well test pressure transient analysis for production estimation.
    /// </summary>
    /// <remarks>
    /// This class uses pressure transient analysis methods to forecast production in early-time
    /// (transient) and middle-time (boundary-dominated) flow regimes.
    ///
    /// Flow regimes and their characteristics:
    /// 1. Early Transient: Well depletion, wellbore storage dominated
    /// 2. Transition: Movement toward outer boundary effects
    /// 3. Boundary-Dominated: Reservoir boundary effects dominate
    ///
    /// Applicable to:
    /// - New wells with limited production history
    /// - Pressure-limited decline (constant Pwf)
    /// - Wells with observed boundary effects
    /// - Enhanced Oil Recovery (EOR) projects
    ///
    /// References:
    /// - Well Testing: "Pressure Analysis Methods" (Earlougher, 1977)
    /// - SPE-26892: "Pressure Transient Analysis in Layered Reservoirs"
    /// - SPE-34882: "Practical Application of Well Testing Theory"
    /// </remarks>
    public static class TransientForecastEnhanced
    {
        #region Transient Flow Forecasting

        /// <summary>
        /// Generates production forecast during early transient flow period.
        /// Used for first months/years before boundary effects appear.
        /// </summary>
        /// <param name="reservoir">Reservoir properties.</param>
        /// <param name="well">Well properties.</param>
        /// <param name="initialPressure">Initial reservoir pressure in psia.</param>
        /// <param name="bottomHolePressure">Constant bottom hole flowing pressure in psia.</param>
        /// <param name="forecastDuration">Forecast duration in days.</param>
        /// <param name="timeSteps">Number of time steps (default 100).</param>
        /// <returns>Production forecast during transient period.</returns>
        /// <remarks>
        /// Early transient flow characteristics:
        /// - Constant rate behavior (for constant Pwf operation)
        /// - Linear pressure decline with sqrt(t)
        /// - Wellbore storage and skin effects important
        /// - No boundary effects yet
        ///
        /// Production equation for constant Pwf:
        /// q = (0.0002637 * k * h) / (Î¼ * B * (ln(r_ed/r_w) + 0.8686*S - 0.25))
        /// where r_ed = sqrt(k*t / (0.0002637*phi*mu*ct))
        /// </remarks>
        public static PRODUCTION_FORECAST GenerateTransientForecast(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            WellForecastProperties well,
            decimal initialPressure,
            decimal bottomHolePressure,
            decimal forecastDuration,
            int timeSteps = 100)
        {
            ValidateForecastInputs(reservoir, well, initialPressure, bottomHolePressure, forecastDuration);

            var forecast = new PRODUCTION_FORECAST
            {
                FORECAST_TYPE = ForecastType.Transient,
                FORECAST_DURATION = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = initialPressure;
            decimal cumulativeProduction = 0m;

            // Calculate constant transient production rate (assuming constant Pwf)
            decimal constantProductionRate = CalculateTransientProductionRate(
                reservoir, well, initialPressure, bottomHolePressure);

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                // Check if still in transient regime
                bool isTransient = IsTransientRegime(reservoir, well, time);

                decimal productionRate;
                if (isTransient)
                {
                    // Transient: constant rate (for constant Pwf)
                    productionRate = constantProductionRate;
                    
                    // Pressure decline during transient
                    decimal pressureDecline = CalculateTransientPressureDecline(
                        reservoir, well, time, constantProductionRate);
                    
                    currentPressure = Math.Max(bottomHolePressure, initialPressure - pressureDecline);
                }
                else
                {
                    // Transition to boundary-dominated: start declining
                    productionRate = constantProductionRate * (decimal)Math.Pow(
                        (double)(1.0m / (1.0m + 0.1m * (time - GetTransientEndTime(reservoir, well)))),
                        0.5);
                }

                cumulativeProduction += productionRate * timeStep;

                forecast.FORECAST_POINTS.Add(new FORECAST_POINT
                {
                     TIME = time,
                    PRODUCTION_RATE = Math.Max(0, productionRate),
                    CUMULATIVE_PRODUCTION = cumulativeProduction,
                    RESERVOIR_PRESSURE = currentPressure,
                    BOTTOM_HOLE_PRESSURE = bottomHolePressure,
                    FORECAST_METHOD = ForecastType.Transient
                });

                if (i == 0)
                    forecast.INITIAL_PRODUCTION_RATE = productionRate;
            }

            if (forecast.FORECAST_POINTS.Count > 0)
            {
                forecast.FINAL_PRODUCTION_RATE = forecast.FORECAST_POINTS.Last().PRODUCTION_RATE;
                forecast.TOTAL_CUMULATIVE_PRODUCTION = forecast.FORECAST_POINTS.Last().CUMULATIVE_PRODUCTION;
            }

            return forecast;
        }

        /// <summary>
        /// Generates forecast during boundary-dominated flow period.
        /// Used after transient effects dissipate.
        /// </summary>
        /// <param name="reservoir">Reservoir properties.</param>
        /// <param name="well">Well properties.</param>
        /// <param name="initialPressure">Initial reservoir pressure for this period.</param>
        /// <param name="bottomHolePressure">Bottom hole pressure in psia.</param>
        /// <param name="forecastDuration">Forecast duration in days.</param>
        /// <param name="timeSteps">Number of time steps.</param>
        /// <returns>Production forecast during boundary-dominated period.</returns>
        /// <remarks>
        /// Boundary-dominated flow characteristics:
        /// - Pseudo-steady state flow
        /// - Production rate declines with pressure (variable rate)
        /// - Reservoir geometry defines boundary effects
        /// - Linear pressure decline with time
        /// </remarks>
        public static PRODUCTION_FORECAST GenerateBoundaryDominatedForecast(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            WellForecastProperties well,
            decimal initialPressure,
            decimal bottomHolePressure,
            decimal forecastDuration,
            int timeSteps = 100)
        {
            ValidateForecastInputs(reservoir, well, initialPressure, bottomHolePressure, forecastDuration);

            var forecast = new PRODUCTION_FORECAST
            {
                FORECAST_TYPE = ForecastType.Transient,
                FORECAST_DURATION = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = initialPressure;
            decimal cumulativeProduction = 0m;

            // Calculate productivity index (constant in pseudo-steady state)
            decimal productivityIndex = CalculatePseudoSteadyStateProductivityIndex(reservoir, well);

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                // Production rate proportional to pressure drop (Darcy equation)
                decimal pressureDrop = Math.Max(0, currentPressure - bottomHolePressure);
                decimal productionRate = productivityIndex * pressureDrop;

                // Material balance pressure decline
                decimal pressureDecline = CalculateMaterialBalancePressureDecline(
                    reservoir, productionRate, timeStep);

                currentPressure = Math.Max(bottomHolePressure, currentPressure - pressureDecline);

                cumulativeProduction += productionRate * timeStep;

                forecast.FORECAST_POINTS.Add(new FORECAST_POINT
                {
                     TIME = time,
                    PRODUCTION_RATE = Math.Max(0, productionRate),
                    CUMULATIVE_PRODUCTION = cumulativeProduction,
                    RESERVOIR_PRESSURE = currentPressure,
                    BOTTOM_HOLE_PRESSURE = bottomHolePressure,
                    FORECAST_METHOD = ForecastType.BoundaryDominated
                });

                if (i == 0)
                    forecast.INITIAL_PRODUCTION_RATE = productionRate;

                // Stop when production reaches zero
                if (productionRate <= 0)
                    break;
            }

            if (forecast.FORECAST_POINTS.Count > 0)
            {
                forecast.FINAL_PRODUCTION_RATE = forecast.FORECAST_POINTS.Last().PRODUCTION_RATE;
                forecast.TOTAL_CUMULATIVE_PRODUCTION = forecast.FORECAST_POINTS.Last().CUMULATIVE_PRODUCTION;
            }

            return forecast;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates transient forecast inputs.
        /// </summary>
        private static void ValidateForecastInputs(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            WellForecastProperties well,
            decimal initialPressure,
            decimal bottomHolePressure,
            decimal forecastDuration)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (well == null)
                throw new ArgumentNullException(nameof(well));

            if (initialPressure <= 0)
                throw new ArgumentException("Initial pressure must be positive.", nameof(initialPressure));

            if (bottomHolePressure < 0 || bottomHolePressure >= initialPressure)
                throw new ArgumentException("Bottom hole pressure must be positive and less than initial pressure.", nameof(bottomHolePressure));

            if (forecastDuration <= 0)
                throw new ArgumentException("Forecast duration must be positive.", nameof(forecastDuration));
        }

        /// <summary>
        /// Calculates constant production rate during transient flow.
        /// For constant Pwf operation, production rate is initially constant.
        /// </summary>
        private static decimal CalculateTransientProductionRate(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            WellForecastProperties well,
            decimal initialPressure,
            decimal bottomHolePressure)
        {
            // Modified Darcy equation for transient flow
            // q = (0.0002637 * k * h * (Pi - Pwf)) / (Î¼ * B * (ln(r_ed/r_w) + 0.8686*S))
            
            decimal permeability = reservoir.PERMEABILITY;
            decimal thickness = reservoir.THICKNESS;
            decimal viscosity = reservoir.OIL_VISCOSITY;
            decimal formationVolumeFactor = reservoir.FORMATION_VOLUME_FACTOR;
            decimal skinFactor = reservoir.SKIN_FACTOR;
            decimal wellRadius = well.WellboreRadius;

            // For transient, use approximate external radius
            decimal drainageRadius = well.DrainageRadius;
            decimal ln_re_rw = (decimal)Math.Log((double)(drainageRadius / wellRadius));

            // Transient flow factor
            decimal denominator = viscosity * formationVolumeFactor * 
                                (ln_re_rw + 0.8686m * skinFactor);

            if (denominator <= 0)
                denominator = 0.001m;

            decimal productionRate = (0.0002637m * permeability * thickness * 
                                    (initialPressure - bottomHolePressure)) / denominator;

            return Math.Max(0, productionRate);
        }

        /// <summary>
        /// Calculates pressure decline during transient flow.
        /// </summary>
        private static decimal CalculateTransientPressureDecline(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            WellForecastProperties well,
            decimal time,
            decimal productionRate)
        {
            if (time == 0)
                return 0m;

            // Pressure decline = q * B / (phi * ct * V_p) * sqrt(t)
            decimal poreVolume = (decimal)Math.PI * 
                                (well.DrainageRadius * well.DrainageRadius) *
                                reservoir.THICKNESS * 
                                reservoir.POROSITY;

            decimal timeEffect = (decimal)Math.Sqrt((double)time) / 100m; // Normalize

            decimal pressureDecline = (productionRate * reservoir.FORMATION_VOLUME_FACTOR * timeEffect) /
                                     (reservoir.TOTAL_COMPRESSIBILITY * poreVolume);

            return Math.Max(0, pressureDecline);
        }

        /// <summary>
        /// Determines if well is still in transient flow regime.
        /// </summary>
        private static bool IsTransientRegime(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            WellForecastProperties well,
            decimal time)
        {
            decimal transientEndTime = GetTransientEndTime(reservoir, well);
            return time < transientEndTime;
        }

        /// <summary>
        /// Estimates end of transient period using diffusivity equation.
        /// Transient ends approximately at: t_td = 0.0002637 * k * t_transient / (phi * mu * ct * r_wÂ²)
        /// </summary>
        private static decimal GetTransientEndTime(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            WellForecastProperties well)
        {
            // Dimensionless time at transition: ~100-200
            decimal dimensionlessTimeTransition = 150m;

            // Transient time = (phi * mu * ct * r_wÂ² * t_dimensionless) / (0.0002637 * k)
            decimal transientTime = 
                (reservoir.POROSITY * reservoir.OIL_VISCOSITY * reservoir.TOTAL_COMPRESSIBILITY * 
                 well.WellboreRadius * well.WellboreRadius * dimensionlessTimeTransition) /
                (0.0002637m * reservoir.PERMEABILITY);

            return Math.Max(1m, transientTime);  // At least 1 day
        }

        /// <summary>
        /// Calculates pseudo-steady state productivity index.
        /// </summary>
        private static decimal CalculatePseudoSteadyStateProductivityIndex(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            WellForecastProperties well)
        {
            // J = 0.0002637 * k * h / (Î¼ * B * (ln(r_e/r_w) + 0.8686*S))
            decimal permeability = reservoir.PERMEABILITY;
            decimal thickness = reservoir.THICKNESS;
            decimal viscosity = reservoir.OIL_VISCOSITY;
            decimal formationVolumeFactor = reservoir.FORMATION_VOLUME_FACTOR;
            decimal skinFactor = reservoir.SKIN_FACTOR;

            decimal ln_re_rw = (decimal)Math.Log((double)(well.DrainageRadius / well.WellboreRadius));

            decimal denominator = viscosity * formationVolumeFactor * 
                                (ln_re_rw + 0.8686m * skinFactor);

            if (denominator <= 0)
                return 0.001m;

            return (0.0002637m * permeability * thickness) / denominator;
        }

        /// <summary>
        /// Calculates material balance pressure decline.
        /// dP = -(q * B * dt) / (ct * Vp)
        /// </summary>
        private static decimal CalculateMaterialBalancePressureDecline(
            RESERVOIR_FORECAST_PROPERTIES reservoir,
            decimal productionRate,
            decimal timeStep)
        {
            if (productionRate == 0)
                return 0m;

            decimal poreVolume = (decimal)Math.PI * 
                                (reservoir.DRAINAGE_RADIUS * reservoir.DRAINAGE_RADIUS) *
                                reservoir.THICKNESS * 
                                reservoir.POROSITY;

            decimal pressureDecline = (productionRate * reservoir.FORMATION_VOLUME_FACTOR * timeStep) /
                                     (reservoir.TOTAL_COMPRESSIBILITY * poreVolume);

            return Math.Max(0, pressureDecline);
        }

        #endregion
    }

    /// <summary>
    /// Represents well properties for transient flow analysis.
    /// </summary>
    public class WellForecastProperties
    {
        /// <summary>
        /// Wellbore radius in feet.
        /// </summary>
        public decimal WellboreRadius { get; set; }

        /// <summary>
        /// Drainage radius (distance to boundary or another well) in feet.
        /// </summary>
        public decimal DrainageRadius { get; set; }

        /// <summary>
        /// Skin factor (dimensionless).
        /// Negative values = stimulation, positive = damage.
        /// </summary>
        public decimal SkinFactor { get; set; } = 0m;

        /// <summary>
        /// Total depth of well in feet.
        /// </summary>
        public decimal TotalDepth { get; set; }
    }
}
