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
        public static ProductionForecast GenerateGasWellForecast(
            ReservoirForecastProperties reservoir,
            decimal bottomHolePressure,
            decimal forecastDuration,
            int timeSteps = 100)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (bottomHolePressure <= 0)
                throw new ArgumentException("Bottom hole pressure must be greater than zero.", nameof(bottomHolePressure));

            var forecast = new ProductionForecast
            {
                ForecastType = ForecastType.GasWell,
                ForecastDuration = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = reservoir.InitialPressure;
            decimal cumulativeProduction = 0m;

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                // Calculate Z-factor at current conditions
                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    currentPressure, reservoir.Temperature, reservoir.GasSpecificGravity);

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

        private static decimal CalculateGasRate(
            ReservoirForecastProperties reservoir,
            decimal reservoirPressure,
            decimal bottomHolePressure,
            decimal zFactor)
        {
            // Gas well deliverability equation (pseudo-steady state)
            // q_g = (0.703 * k * h * (Pr² - Pwf²)) / (μ_g * Z * T * (ln(re/rw) - 0.75 + S))

            decimal permeability = reservoir.Permeability;
            decimal thickness = reservoir.Thickness;
            decimal pressureSquaredDiff = reservoirPressure * reservoirPressure - 
                                         bottomHolePressure * bottomHolePressure;
            decimal temperature = reservoir.Temperature;

            // Gas viscosity (simplified - would use gas properties library)
            decimal gasViscosity = 0.02m; // cp (approximate)

            decimal re_rw = reservoir.DrainageRadius / reservoir.WellboreRadius;
            decimal ln_re_rw = (decimal)Math.Log((double)re_rw);

            decimal denominator = gasViscosity * zFactor * temperature * 
                                (ln_re_rw - 0.75m + reservoir.SkinFactor);

            if (denominator <= 0)
                denominator = 0.001m;

            decimal productionRate = (0.703m * permeability * thickness * pressureSquaredDiff) / denominator;

            // Convert to Mscf/day (assuming standard conditions)
            return Math.Max(0m, productionRate / 1000m);
        }

        private static decimal CalculateGasPressureDecline(
            ReservoirForecastProperties reservoir,
            decimal productionRate,
            decimal timeStep,
            decimal cumulativeProduction,
            decimal zFactor,
            decimal currentPressure)
        {
            // Material balance for gas: P/Z = (Pi/Zi) * (1 - Gp/Gi)
            // Simplified pressure decline calculation

            decimal poreVolume = (decimal)Math.PI * reservoir.DrainageRadius * reservoir.DrainageRadius *
                                reservoir.Thickness * reservoir.Porosity;

            // Gas formation volume factor
            decimal gasFormationVolumeFactor = CalculateGasFormationVolumeFactor(
                currentPressure, reservoir.Temperature, zFactor);

            // Pressure decline
            decimal pressureDecline = (productionRate * gasFormationVolumeFactor * timeStep) /
                                     (reservoir.TotalCompressibility * poreVolume);

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

