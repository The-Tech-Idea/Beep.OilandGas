using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.AdvancedDeclineMethods;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Calculations
{
    /// <summary>
    /// Provides decline curve-based production forecasting using Arps decline methods.
    /// Integrates decline curve analysis (DCA) with production forecasting for more accurate predictions.
    /// </summary>
    /// <remarks>
    /// This class uses industry-standard Arps decline curves to generate production forecasts.
    /// Unlike pseudo-steady state methods which assume material balance, decline curves are purely
    /// empirical fits to historical production data and can capture actual well behavior more accurately.
    ///
    /// Decline curves are defined by three parameters:
    /// - qi: Initial production rate (at t=0)
    /// - Di: Initial decline rate (slope of production at t=0)
    /// - b: Decline exponent (determines how decline rate changes over time)
    ///
    /// References:
    /// - Arps, J. J., "Analysis of Decline Curves," Transactions AIME, Vol. 160, pp. 228-247
    /// - SPE-137033: "Decline Curves: What Do They Really Mean?"
    /// </remarks>
    public static class DeclineForecast
    {
        #region Main Forecasting Methods

        /// <summary>
        /// Generates production forecast using exponential decline.
        /// Used for wells in early transient flow or constant percentage decline.
        /// </summary>
        /// <param name="qi">Initial production rate (bbl/day, Mscf/day, etc.).</param>
        /// <param name="di">Initial decline rate (1/time, e.g., 1/day).</param>
        /// <param name="forecastDuration">Forecast duration (days, months, years - must match Di units).</param>
        /// <param name="timeSteps">Number of time steps for forecast points (default 100).</param>
        /// <returns>Production forecast object with detailed forecast points.</returns>
        /// <exception cref="ArgumentException">Thrown when input parameters are invalid.</exception>
        /// <remarks>
        /// Exponential decline formula: q(t) = qi * exp(-Di*t)
        /// 
        /// Characteristics:
        /// - Steepest decline curve
        /// - Constant percentage decline rate
        /// - Asymptotically approaches zero production
        /// - Reaches economic limit in infinite time (theoretical)
        /// 
        /// Best used for:
        /// - Early production (transient flow period)
        /// - Constant Pwf wells
        /// - Natural decline without stimulation
        /// </remarks>
        public static PRODUCTION_FORECAST GenerateExponentialDeclineForecast(
            decimal qi,
            decimal di,
            decimal forecastDuration,
            int timeSteps = 100)
        {
            ValidateForecastInputs(qi, di, forecastDuration, timeSteps);

            var forecast = new PRODUCTION_FORECAST
            {
                ForecastType = ForecastType.Decline,
                ForecastDuration = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal cumulativeProduction = 0m;

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;
                
                // Calculate production rate using exponential decline
                double q_double = ArpsDeclineMethods.ExponentialDecline((double)qi, (double)di, (double)time);
                decimal productionRate = (decimal)q_double;

                // Calculate cumulative production using analytical formula
                double np_double = ArpsDeclineMethods.ExponentialCumulativeProduction((double)qi, (double)di, (double)time);
                cumulativeProduction = (decimal)np_double;

                forecast.ForecastPoints.Add(new FORECAST_POINT
                {
                    Time = time,
                    ProductionRate = Math.Max(0, productionRate),
                    CumulativeProduction = cumulativeProduction,
                    DeclineExponent = 0m,
                    ForecastMethod = "Exponential Decline"
                });

                if (i == 0)
                    forecast.INITIAL_PRODUCTION_RATE = productionRate;
            }

            if (forecast.ForecastPoints.Count > 0)
            {
                forecast.FINAL_PRODUCTION_RATE = forecast.ForecastPoints.Last().PRODUCTION_RATE;
                forecast.TOTAL_CUMULATIVE_PRODUCTION = forecast.ForecastPoints.Last().CUMULATIVE_PRODUCTION;
            }

            return forecast;
        }

        /// <summary>
        /// Generates production forecast using harmonic decline.
        /// Used for wells transitioning to boundary-dominated flow.
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="forecastDuration">Forecast duration.</param>
        /// <param name="economicLimit">Economic limit production rate (optional, for reserve calculation).</param>
        /// <param name="timeSteps">Number of time steps.</param>
        /// <returns>Production forecast with harmonic decline.</returns>
        /// <remarks>
        /// Harmonic decline formula: q(t) = qi / (1 + Di*t)
        /// This is a special case of hyperbolic decline where b = 1.
        ///
        /// Characteristics:
        /// - Decline rate decreases over time
        /// - Reaches economic limit in finite time
        /// - Linear relationship on log-log plot
        /// - Represents boundary-dominated flow (pseudo-steady state)
        ///
        /// Best used for:
        /// - Mature wells in late life
        /// - Boundary-dominated flow periods
        /// - Depletion-driven production
        /// </remarks>
        public static PRODUCTION_FORECAST GenerateHarmonicDeclineForecast(
            decimal qi,
            decimal di,
            decimal forecastDuration,
            decimal? economicLimit = null,
            int timeSteps = 100)
        {
            ValidateForecastInputs(qi, di, forecastDuration, timeSteps);

            var forecast = new PRODUCTION_FORECAST
            {
                ForecastType = ForecastType.Decline,
                ForecastDuration = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal cumulativeProduction = 0m;
            decimal maxTime = forecastDuration;

            // If economic limit specified, calculate reserves
            decimal reserves = 0m;
            if (economicLimit.HasValue && economicLimit > 0 && economicLimit < qi)
            {
                reserves = (decimal)ArpsDeclineMethods.HarmonicReserves((double)qi, (double)di, (double)economicLimit);
            }

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                // Calculate production rate using harmonic decline
                double q_double = ArpsDeclineMethods.HarmonicDecline((double)qi, (double)di, (double)time);
                decimal productionRate = (decimal)q_double;

                // Stop at economic limit if specified
                if (economicLimit.HasValue && productionRate < economicLimit)
                {
                    productionRate = 0m;
                    // Calculate actual time to economic limit
                    maxTime = time;
                }

                // Calculate cumulative production
                double np_double = ArpsDeclineMethods.HarmonicCumulativeProduction((double)qi, (double)di, (double)time);
                cumulativeProduction = (decimal)np_double;

                forecast.ForecastPoints.Add(new FORECAST_POINT
                {
                    Time = time,
                    ProductionRate = Math.Max(0, productionRate),
                    CumulativeProduction = cumulativeProduction,
                    DeclineExponent = 1m,
                    ForecastMethod = "Harmonic Decline"
                });

                if (i == 0)
                    forecast.INITIAL_PRODUCTION_RATE = productionRate;

                if (productionRate == 0)
                    break;
            }

            if (forecast.ForecastPoints.Count > 0)
            {
                forecast.FINAL_PRODUCTION_RATE = forecast.ForecastPoints.Last().PRODUCTION_RATE;
                forecast.TOTAL_CUMULATIVE_PRODUCTION = forecast.ForecastPoints.Last().CUMULATIVE_PRODUCTION;
                forecast.FORECAST_DURATION = maxTime;
            }

            return forecast;
        }

        /// <summary>
        /// Generates production forecast using hyperbolic decline (most general Arps method).
        /// Suitable for wells in any flow regime with appropriate b value selection.
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="b">Decline exponent (0 â‰¤ b â‰¤ 1). Range: 0.0 (exponential) to 1.0 (harmonic).</param>
        /// <param name="forecastDuration">Forecast duration.</param>
        /// <param name="economicLimit">Economic limit production rate (optional).</param>
        /// <param name="timeSteps">Number of time steps.</param>
        /// <returns>Production forecast with hyperbolic decline.</returns>
        /// <remarks>
        /// Hyperbolic decline formula: q(t) = qi / (1 + b*Di*t)^(1/b)
        /// This is the most general Arps equation; exponential and harmonic are special cases.
        ///
        /// Decline exponent (b) selection guidelines:
        /// - b = 0.0: Exponential decline (early transient)
        /// - b = 0.2-0.4: Early transient to transition
        /// - b = 0.4-0.7: Mid-life production
        /// - b = 0.7-1.0: Late-life/boundary-dominated
        /// - b = 1.0: Harmonic decline (steady-state)
        ///
        /// Typical industry observations:
        /// - Unconventional wells: b = 0.3-0.5 (due to boundary expansion)
        /// - Conventional wells: b = 0.5-1.0 (time-dependent decline)
        /// - Fractured reservoirs: b = 0.4-0.7
        /// - Homogeneous reservoirs: b = 0.8-1.0
        /// </remarks>
        public static PRODUCTION_FORECAST GenerateHyperbolicDeclineForecast(
            decimal qi,
            decimal di,
            decimal b,
            decimal forecastDuration,
            decimal? economicLimit = null,
            int timeSteps = 100)
        {
            ValidateForecastInputs(qi, di, forecastDuration, timeSteps);

            if (b < 0 || b > 1)
                throw new ArgumentException("Decline exponent b must be between 0 and 1.", nameof(b));

            var forecast = new PRODUCTION_FORECAST
            {
                ForecastType = ForecastType.Decline,
                ForecastDuration = forecastDuration
            };

            decimal timeStep = forecastDuration / timeSteps;
            decimal cumulativeProduction = 0m;
            decimal maxTime = forecastDuration;

            // If economic limit specified, calculate reserves
            decimal reserves = 0m;
            if (economicLimit.HasValue && economicLimit > 0 && economicLimit < qi)
            {
                reserves = (decimal)ArpsDeclineMethods.HyperbolicReserves((double)qi, (double)di, (double)b, (double)economicLimit);
            }

            for (int i = 0; i <= timeSteps; i++)
            {
                decimal time = i * timeStep;

                // Calculate production rate using hyperbolic decline
                double q_double = ArpsDeclineMethods.HyperbolicDecline((double)qi, (double)di, (double)time, (double)b);
                decimal productionRate = (decimal)q_double;

                // Stop at economic limit if specified
                if (economicLimit.HasValue && productionRate < economicLimit)
                {
                    productionRate = 0m;
                    maxTime = time;
                }

                // Calculate cumulative production
                double np_double = ArpsDeclineMethods.HyperbolicCumulativeProduction((double)qi, (double)di, (double)time, (double)b);
                cumulativeProduction = (decimal)np_double;

                forecast.ForecastPoints.Add(new FORECAST_POINT
                {
                    Time = time,
                    ProductionRate = Math.Max(0, productionRate),
                    CumulativeProduction = cumulativeProduction,
                    DeclineExponent = b,
                    ForecastMethod = $"Hyperbolic Decline (b={b:F2})"
                });

                if (i == 0)
                    forecast.INITIAL_PRODUCTION_RATE = productionRate;

                if (productionRate == 0)
                    break;
            }

            if (forecast.ForecastPoints.Count > 0)
            {
                forecast.FINAL_PRODUCTION_RATE = forecast.ForecastPoints.Last().PRODUCTION_RATE;
                forecast.TOTAL_CUMULATIVE_PRODUCTION = forecast.ForecastPoints.Last().CUMULATIVE_PRODUCTION;
                forecast.FORECAST_DURATION = maxTime;
            }

            return forecast;
        }

        /// <summary>
        /// Generates production forecast using best-fit decline method selected automatically.
        /// Analyzes flow regime indicators to select optimal decline exponent.
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="wellType">Type of well: "oil", "gas", "unconventional", or "conventional".</param>
        /// <param name="forecastDuration">Forecast duration.</param>
        /// <param name="economicLimit">Economic limit production rate (optional).</param>
        /// <param name="timeSteps">Number of time steps.</param>
        /// <returns>Production forecast with automatically selected decline method.</returns>
        /// <remarks>
        /// This method selects the decline exponent based on well type and industry practice:
        /// - Oil wells (conventional): b = 0.8 (transition to boundary-dominated)
        /// - Oil wells (unconventional): b = 0.5 (longer transient period)
        /// - Gas wells (conventional): b = 0.9 (typically boundary-dominated)
        /// - Gas wells (unconventional): b = 0.4 (extended transient)
        /// </remarks>
        public static PRODUCTION_FORECAST GenerateAutoSelectDeclineForecast(
            decimal qi,
            decimal di,
            string wellType = "conventional",
            decimal forecastDuration = 0,
            decimal? economicLimit = null,
            int timeSteps = 100)
        {
            // Select decline exponent based on well type
            decimal b = SelectDeclineExponent(wellType);

            // If no forecast duration specified, use 30 years as default
            if (forecastDuration == 0)
                forecastDuration = 30m * 365.25m; // 30 years in days

            return GenerateHyperbolicDeclineForecast(qi, di, b, forecastDuration, economicLimit, timeSteps);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates forecast input parameters.
        /// </summary>
        private static void ValidateForecastInputs(decimal qi, decimal di, decimal forecastDuration, int timeSteps)
        {
            if (qi <= 0)
                throw new ArgumentException("Initial production rate (qi) must be positive.", nameof(qi));

            if (di <= 0 || di > 2.0m)
                throw new ArgumentException("Decline rate (di) must be positive and â‰¤ 2.0.", nameof(di));

            if (forecastDuration <= 0)
                throw new ArgumentException("Forecast duration must be positive.", nameof(forecastDuration));

            if (timeSteps < 2)
                throw new ArgumentException("Time steps must be at least 2.", nameof(timeSteps));
        }

        /// <summary>
        /// Selects appropriate decline exponent based on well type.
        /// </summary>
        private static decimal SelectDeclineExponent(string wellType)
        {
            return (wellType ?? "conventional").ToLowerInvariant() switch
            {
                "oil" or "oil_conventional" => 0.8m,      // Oil: mostly boundary-dominated
                "oil_unconventional" or "shale" => 0.5m,  // Shale: longer transient
                "gas" or "gas_conventional" => 0.9m,      // Gas: stronger boundary effects
                "gas_unconventional" => 0.4m,              // Tight gas: extended transient
                _ => 0.6m  // Default: mid-range hyperbolic
            };
        }

        /// <summary>
        /// Calculates estimated reserves to economic limit for a decline forecast.
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Decline rate.</param>
        /// <param name="b">Decline exponent.</param>
        /// <param name="economicLimit">Economic limit production rate.</param>
        /// <returns>Estimated reserves to economic limit.</returns>
        public static decimal CalculateDeclineReserves(decimal qi, decimal di, decimal b, decimal economicLimit)
        {
            if (economicLimit <= 0 || economicLimit >= qi)
                throw new ArgumentException("Economic limit must be positive and less than qi.");

            if (b < 0 || b > 1)
                throw new ArgumentException("Decline exponent must be between 0 and 1.");

            // Use hyperbolic method for all cases
            double reserves = ArpsDeclineMethods.HyperbolicReserves(
                (double)qi, (double)di, (double)b, (double)economicLimit);

            return (decimal)reserves;
        }

        /// <summary>
        /// Gets the decline exponent description (e.g., "Early Transient Hyperbolic").
        /// </summary>
        /// <param name="b">Decline exponent.</param>
        /// <returns>Human-readable description.</returns>
        public static string GetDeclineTypeDescription(decimal b)
        {
            return ArpsDeclineMethods.GetDeclineTypeName((double)b);
        }

        #endregion
    }
}
