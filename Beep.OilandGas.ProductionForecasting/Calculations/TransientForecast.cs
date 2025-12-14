using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionForecasting.Models;

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
        public static ProductionForecast GenerateTransientForecast(
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
                ForecastType = ForecastType.Transient,
                ForecastDuration = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal currentPressure = reservoir.InitialPressure;
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

        private static decimal CalculateTransientRate(
            ReservoirForecastProperties reservoir,
            decimal reservoirPressure,
            decimal bottomHolePressure,
            decimal time)
        {
            // Transient flow equation
            // q = (0.00708 * k * h * (Pr - Pwf)) / (B * μ * (ln(t) + 0.80907 + 2*S))
            // Simplified version

            decimal permeability = reservoir.Permeability;
            decimal thickness = reservoir.Thickness;
            decimal pressureDiff = reservoirPressure - bottomHolePressure;
            decimal formationVolumeFactor = reservoir.FormationVolumeFactor;
            decimal viscosity = reservoir.OilViscosity;

            if (time <= 0)
                time = 0.001m; // Avoid log(0)

            decimal lnTime = (decimal)Math.Log((double)time);
            decimal denominator = formationVolumeFactor * viscosity * (lnTime + 0.80907m + 2m * reservoir.SkinFactor);

            if (denominator <= 0)
                denominator = 0.001m;

            decimal productionRate = (0.00708m * permeability * thickness * pressureDiff) / denominator;

            return Math.Max(0m, productionRate);
        }

        private static decimal CalculateTransientPressureDecline(
            ReservoirForecastProperties reservoir,
            decimal productionRate,
            decimal timeStep,
            decimal time)
        {
            // Transient pressure decline
            // Uses exponential integral (Ei) approximation for early time
            // Simplified using logarithmic approximation

            decimal diffusivity = reservoir.Permeability / 
                                (reservoir.Porosity * reservoir.TotalCompressibility * reservoir.OilViscosity);

            decimal timeInSeconds = time * 86400m; // Convert days to seconds
            decimal radiusSquared = reservoir.WellboreRadius * reservoir.WellboreRadius;

            // Dimensionless time
            decimal tD = (0.0002637m * reservoir.Permeability * timeInSeconds) /
                        (reservoir.Porosity * reservoir.TotalCompressibility * reservoir.OilViscosity * radiusSquared);

            // Pressure decline using line source solution approximation
            decimal pressureDecline = 0m;

            if (tD > 100m)
            {
                // Pseudo-steady state approximation
                decimal poreVolume = (decimal)Math.PI * reservoir.DrainageRadius * reservoir.DrainageRadius *
                                    reservoir.Thickness * reservoir.Porosity;
                pressureDecline = (productionRate * reservoir.FormationVolumeFactor * timeStep) /
                                (reservoir.TotalCompressibility * poreVolume);
            }
            else
            {
                // Transient approximation
                decimal ln_tD = (decimal)Math.Log((double)tD);
                decimal pressureDrop = (162.6m * productionRate * reservoir.FormationVolumeFactor * reservoir.OilViscosity) /
                                     (reservoir.Permeability * reservoir.Thickness) *
                                     (ln_tD + 0.80907m + 2m * reservoir.SkinFactor);

                pressureDecline = pressureDrop * (timeStep / time);
            }

            return Math.Max(0m, pressureDecline);
        }
    }
}

