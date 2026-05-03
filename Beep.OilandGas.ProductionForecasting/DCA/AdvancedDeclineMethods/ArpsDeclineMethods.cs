using System;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Validation;
using InvalidDataException = Beep.OilandGas.DCA.Exceptions.InvalidDataException;

namespace Beep.OilandGas.DCA.AdvancedDeclineMethods
{
    /// <summary>
    /// Provides Arps decline curve methods for decline curve analysis.
    /// Implements the industry-standard decline methods: exponential, hyperbolic, and harmonic.
    /// Based on Arps, J. J., "Analysis of Decline Curves," Transactions AIME, Vol. 160, pp. 228-247.
    /// </summary>
    /// <remarks>
    /// The Arps decline equation is: q(t) = qi / (1 + b*Di*t)^(1/b)
    /// where:
    ///   - q(t) = production rate at time t
    ///   - qi = initial production rate
    ///   - Di = initial decline rate (1/time)
    ///   - b = decline exponent (0 ≤ b ≤ 1)
    ///   - t = time since start of production
    ///
    /// Special cases:
    ///   - b = 0: Exponential decline: q(t) = qi * exp(-Di*t)
    ///   - b = 1: Harmonic decline: q(t) = qi / (1 + Di*t)
    ///   - 0 < b < 1: Hyperbolic decline
    ///
    /// Reference: 
    /// SPE-26423: "The Exponential Decline Curve in Reservoir Performance Analysis"
    /// SPE-5567: "Decline Curves as a Development Tool"
    /// </remarks>
    public static class ArpsDeclineMethods
    {
        #region Exponential Decline

        /// <summary>
        /// Calculates production rate using exponential (constant percentage) decline.
        /// Formula: q(t) = qi * exp(-Di*t)
        /// This represents constant percentage decline rate.
        /// </summary>
        /// <param name="qi">Initial production rate (bbl/day, Mscf/day, etc.).</param>
        /// <param name="di">Initial decline rate (1/time, e.g., 1/day). Valid range: 0 &lt; Di ≤ 2.0.</param>
        /// <param name="t">Time since start of production (days, months, years - must be consistent with Di units).</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        /// <remarks>
        /// Exponential decline is used when:
        /// - Wells are in early transient flow period
        /// - Decline is due to reservoir pressure depletion without boundary effects
        /// - Well is producing at constant flowing bottom hole pressure
        ///
        /// Characteristics:
        /// - Produces the steepest decline curve
        /// - Constant production decline rate
        /// - Never reaches economic limit in finite time (asymptotic to horizontal axis)
        /// </remarks>
        public static double ExponentialDecline(double qi, double di, double t)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (t < 0)
            {
                throw new InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            if (di > 2.0)
            {
                throw new InvalidDataException(
                    $"Decline rate Di for exponential decline must be ≤ 2.0 (per {GetTimeUnitFromContext()}). Provided: {di}.");
            }

            return qi * Math.Exp(-di * t);
        }

        /// <summary>
        /// Calculates cumulative production using exponential decline.
        /// Formula: Np(t) = (qi / Di) * (1 - exp(-Di*t))
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="t">Time since start of production.</param>
        /// <returns>Cumulative production from t=0 to time t.</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        public static double ExponentialCumulativeProduction(double qi, double di, double t)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (t < 0)
            {
                throw new InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            if (Math.Abs(di) < DCAConstants.Epsilon)
            {
                throw new InvalidDataException($"Decline rate cannot be zero for exponential decline.");
            }

            // Cumulative: Np(t) = (qi / Di) * (1 - exp(-Di*t))
            return (qi / di) * (1.0 - Math.Exp(-di * t));
        }

        /// <summary>
        /// Estimates reserves to economic limit using exponential decline.
        /// Formula: Np(∞) = qi / Di
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <returns>Total recoverable reserves (theoretical, to infinite time).</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        /// <remarks>
        /// Exponential decline produces infinite reserves at infinite time because the decline curve
        /// asymptotically approaches zero production but never reaches it. In practice, reserves are
        /// limited to the economic limit time or reservoir depletion, whichever comes first.
        ///
        /// This method returns the mathematical infinite-time limit, which may not be realistic.
        /// Use ExponentialCumulativeProduction with a specific economic limit time instead.
        /// </remarks>
        public static double ExponentialReserves(double qi, double di)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (Math.Abs(di) < DCAConstants.Epsilon)
            {
                throw new InvalidDataException($"Decline rate cannot be zero for exponential reserves calculation.");
            }

            // Theoretical reserves at infinite time: qi / Di
            // Note: This is infinite for exponential decline; practical reserves require economic limit
            return qi / di;
        }

        #endregion

        #region Harmonic Decline

        /// <summary>
        /// Calculates production rate using harmonic decline.
        /// Formula: q(t) = qi / (1 + Di*t)
        /// This is a special case of hyperbolic decline where b = 1.
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate (1/time).</param>
        /// <param name="t">Time since start of production.</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        /// <remarks>
        /// Harmonic decline is used when:
        /// - Well has transitioned from transient to pseudo-steady state flow
        /// - Boundary-dominated flow with constant-pressure boundary
        /// - Well is producing at constant rate (constant Pwf)
        ///
        /// Characteristics:
        /// - Decline rate decreases over time (gentler decline than exponential)
        /// - Reaches economic limit in finite time
        /// - Linear relationship on semi-log plot for log(q) vs 1/t
        /// - Less commonly used than hyperbolic decline
        ///
        /// References:
        /// - SPE-26423: Harmonic decline derivation and limitations
        /// </remarks>
        public static double HarmonicDecline(double qi, double di, double t)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (t < 0)
            {
                throw new InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            double denominator = 1.0 + di * t;
            if (Math.Abs(denominator) < DCAConstants.Epsilon)
            {
                throw new InvalidDataException(
                    $"Division by zero risk in harmonic decline: 1 + Di*t ≈ 0. di={di}, t={t}.");
            }

            return qi / denominator;
        }

        /// <summary>
        /// Calculates cumulative production using harmonic decline.
        /// Formula: Np(t) = (qi / Di) * ln(1 + Di*t)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="t">Time since start of production.</param>
        /// <returns>Cumulative production from t=0 to time t.</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        public static double HarmonicCumulativeProduction(double qi, double di, double t)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (t < 0)
            {
                throw new InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            if (Math.Abs(di) < DCAConstants.Epsilon)
            {
                throw new InvalidDataException($"Decline rate cannot be zero for harmonic decline.");
            }

            double argument = 1.0 + di * t;
            if (argument <= 0)
            {
                throw new InvalidDataException(
                    $"Invalid argument for logarithm in harmonic cumulative: 1 + Di*t = {argument}.");
            }

            // Cumulative: Np(t) = (qi / Di) * ln(1 + Di*t)
            return (qi / di) * Math.Log(argument);
        }

        /// <summary>
        /// Estimates reserves to economic limit using harmonic decline.
        /// Formula: Np(t_econ) = (qi / Di) * ln(1 + Di*t_econ)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="economicLimit">Economic limit production rate (bbl/day, Mscf/day, etc.).</param>
        /// <returns>Cumulative production at economic limit.</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        /// <remarks>
        /// For harmonic decline, we solve for time at economic limit and calculate cumulative.
        /// q_econ = qi / (1 + Di * t_econ)
        /// t_econ = (qi / q_econ - 1) / Di
        /// Np = (qi / Di) * ln(qi / q_econ)
        /// </remarks>
        public static double HarmonicReserves(double qi, double di, double economicLimit)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (economicLimit <= 0 || economicLimit > qi)
            {
                throw new InvalidDataException(
                    $"Economic limit must be positive and less than initial production rate. Provided: {economicLimit}, qi: {qi}.");
            }

            if (Math.Abs(di) < DCAConstants.Epsilon)
            {
                throw new InvalidDataException($"Decline rate cannot be zero for harmonic reserves calculation.");
            }

            // For harmonic decline: Np = (qi / Di) * ln(qi / q_econ)
            double ratioLog = Math.Log(qi / economicLimit);
            return (qi / di) * ratioLog;
        }

        #endregion

        #region Hyperbolic Decline

        /// <summary>
        /// Calculates production rate using hyperbolic decline (general Arps equation).
        /// Formula: q(t) = qi / (1 + b*Di*t)^(1/b)
        /// This is the most general form; exponential (b=0) and harmonic (b=1) are special cases.
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate (1/time).</param>
        /// <param name="t">Time since start of production.</param>
        /// <param name="b">Decline exponent. Valid range: 0 ≤ b ≤ 1. Recommended: 0.3 to 0.8.</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        /// <remarks>
        /// Hyperbolic decline is the most commonly used decline model in petroleum engineering.
        /// 
        /// Characteristics:
        /// - Most versatile: encompasses both exponential and harmonic as special cases
        /// - Decline rate decreases with time (less steep than exponential)
        /// - Typically used for wells transitioning through multiple flow regimes
        /// - "b-factor" represents the transition between flow regimes
        ///
        /// Physical interpretation of b:
        /// - b = 0: Exponential decline (constant decline rate)
        /// - b = 0.5: Often observed in naturally fractured reservoirs
        /// - b = 0.8-1.0: Typical for boundary-dominated flow
        /// - b = 1: Harmonic decline (1/2 transient, 1/2 pseudo-steady state)
        ///
        /// Selection guidelines:
        /// - Early transient flow: use smaller b (0.2-0.4)
        /// - Mid-life production: use moderate b (0.4-0.7)
        /// - Late-life/boundary-dominated: use larger b (0.7-1.0)
        ///
        /// References:
        /// SPE-137033: "Decline Curves: What Do They Really Mean?"
        /// SPE-110510: "Hyperbolic Decline: An Outdated Concept or Still the Workhorse?"
        /// </remarks>
        public static double HyperbolicDecline(double qi, double di, double t, double b)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));
            DataValidator.ValidateDeclineExponent(b, nameof(b));

            if (t < 0)
            {
                throw new InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            // For b very close to 0, use exponential approximation
            if (Math.Abs(b) < DCAConstants.Epsilon)
            {
                return ExponentialDecline(qi, di, t);
            }

            // For b close to 1, use harmonic approximation
            if (Math.Abs(b - 1.0) < DCAConstants.Epsilon)
            {
                return HarmonicDecline(qi, di, t);
            }

            double denominator = 1.0 + b * di * t;
            if (denominator <= 0)
            {
                throw new InvalidDataException(
                    $"Invalid denominator in hyperbolic decline: 1 + b*Di*t = {denominator}. b={b}, di={di}, t={t}.");
            }

            return qi / Math.Pow(denominator, 1.0 / b);
        }

        /// <summary>
        /// Calculates cumulative production using hyperbolic decline.
        /// Formula: Np(t) = (qi^b / Di(1-b)) * ((1 + b*Di*t)^((1-b)/b) - 1)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="t">Time since start of production.</param>
        /// <param name="b">Decline exponent.</param>
        /// <returns>Cumulative production from t=0 to time t.</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        /// <remarks>
        /// Special cases:
        /// - When b → 0 (exponential): Np = (qi / Di) * (1 - exp(-Di*t))
        /// - When b = 1 (harmonic): Np = (qi / Di) * ln(1 + Di*t)
        /// - 0 < b < 1: Use the general formula below
        /// </remarks>
        public static double HyperbolicCumulativeProduction(double qi, double di, double t, double b)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));
            DataValidator.ValidateDeclineExponent(b, nameof(b));

            if (t < 0)
            {
                throw new InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            // Handle special case: b ≈ 0 (exponential)
            if (Math.Abs(b) < DCAConstants.Epsilon)
            {
                return ExponentialCumulativeProduction(qi, di, t);
            }

            // Handle special case: b ≈ 1 (harmonic)
            if (Math.Abs(b - 1.0) < DCAConstants.Epsilon)
            {
                return HarmonicCumulativeProduction(qi, di, t);
            }

            double argument = 1.0 + b * di * t;
            if (argument <= 0)
            {
                throw new InvalidDataException(
                    $"Invalid argument in hyperbolic cumulative: 1 + b*Di*t = {argument}. b={b}, di={di}, t={t}.");
            }

            // General hyperbolic formula: Np = (qi^b / Di(1-b)) * ((1 + b*Di*t)^((1-b)/b) - 1)
            double exponent = (1.0 - b) / b;
            double term = Math.Pow(argument, exponent) - 1.0;
            return (Math.Pow(qi, b) / (di * (1.0 - b))) * term;
        }

        /// <summary>
        /// Estimates reserves to economic limit using hyperbolic decline.
        /// Solves: q_econ = qi / (1 + b*Di*t_econ)^(1/b)
        /// Then calculates: Np = HyperbolicCumulativeProduction(qi, di, t_econ, b)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="b">Decline exponent.</param>
        /// <param name="economicLimit">Economic limit production rate.</param>
        /// <returns>Cumulative production at economic limit.</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        /// <remarks>
        /// The economic limit is the production rate below which continued operation is uneconomical.
        /// This method solves for the time at which production reaches this limit, then calculates
        /// cumulative production to that point.
        ///
        /// Solution steps:
        /// 1. Solve for t_econ: q_econ = qi / (1 + b*Di*t_econ)^(1/b)
        ///    t_econ = ((qi / q_econ)^b - 1) / (b * Di)
        /// 2. Calculate cumulative at t_econ
        /// </remarks>
        public static double HyperbolicReserves(double qi, double di, double b, double economicLimit)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));
            DataValidator.ValidateDeclineExponent(b, nameof(b));

            if (economicLimit <= 0 || economicLimit > qi)
            {
                throw new InvalidDataException(
                    $"Economic limit must be positive and less than initial production rate. Provided: {economicLimit}, qi: {qi}.");
            }

            // Special cases handled
            if (Math.Abs(b) < DCAConstants.Epsilon)
            {
                return ExponentialReserves(qi, di);
            }

            if (Math.Abs(b - 1.0) < DCAConstants.Epsilon)
            {
                return HarmonicReserves(qi, di, economicLimit);
            }

            // Solve for time at economic limit
            // q_econ = qi / (1 + b*Di*t_econ)^(1/b)
            // (1 + b*Di*t_econ)^(1/b) = qi / q_econ
            // 1 + b*Di*t_econ = (qi / q_econ)^b
            // t_econ = ((qi / q_econ)^b - 1) / (b * Di)

            double ratio = qi / economicLimit;
            double ratioToPowerB = Math.Pow(ratio, b);
            
            if (Math.Abs(di) < DCAConstants.Epsilon)
            {
                throw new InvalidDataException($"Decline rate cannot be zero for hyperbolic reserves calculation.");
            }

            double t_econ = (ratioToPowerB - 1.0) / (b * di);

            if (t_econ < 0)
            {
                throw new InvalidDataException(
                    $"Calculated economic limit time is negative. This may indicate invalid parameters.");
            }

            // Calculate cumulative production at economic limit
            return HyperbolicCumulativeProduction(qi, di, t_econ, b);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Validates that decline exponent is within acceptable range for hyperbolic decline.
        /// </summary>
        /// <param name="b">Decline exponent to validate.</param>
        /// <returns>True if valid; false otherwise.</returns>
        public static bool IsValidDeclineExponent(double b)
        {
            return b >= 0.0 && b <= 1.0;
        }

        /// <summary>
        /// Validates that decline rate is within acceptable range.
        /// </summary>
        /// <param name="di">Decline rate to validate.</param>
        /// <returns>True if valid; false otherwise.</returns>
        public static bool IsValidDeclineRate(double di)
        {
            return di > 0.0 && di <= 2.0;
        }

        /// <summary>
        /// Estimates decline exponent based on reservoir characteristics.
        /// This provides a starting point for parameter estimation.
        /// </summary>
        /// <param name="flowRegime">Description of flow regime (e.g., "transient", "transition", "boundary-dominated").</param>
        /// <returns>Recommended decline exponent range as a tuple (min, typical, max).</returns>
        /// <remarks>
        /// Based on industry practice and published correlations.
        /// See: SPE-137033, SPE-110510 for detailed derivations.
        /// </remarks>
        public static (double min, double typical, double max) RecommendedDeclineExponent(string flowRegime)
        {
            return (flowRegime ?? "").ToLowerInvariant() switch
            {
                "transient" or "early" => (0.1, 0.3, 0.5),
                "transition" or "mid-life" => (0.4, 0.6, 0.8),
                "boundary-dominated" or "late" or "steady-state" => (0.7, 0.85, 1.0),
                "harmonic" => (1.0, 1.0, 1.0),
                "exponential" => (0.0, 0.0, 0.0),
                _ => (0.2, 0.5, 0.9)  // Default if flow regime not recognized
            };
        }

        /// <summary>
        /// Gets a descriptive name for a decline exponent value.
        /// </summary>
        /// <param name="b">Decline exponent.</param>
        /// <returns>Human-readable description of the decline type.</returns>
        public static string GetDeclineTypeName(double b)
        {
            if (Math.Abs(b) < 0.05)
                return "Exponential (b≈0)";
            else if (b < 0.3)
                return "Early Transient Hyperbolic (b<0.3)";
            else if (b < 0.7)
                return "Mid-Life Hyperbolic (0.3≤b<0.7)";
            else if (b < 0.95)
                return "Late-Life Hyperbolic (0.7≤b<0.95)";
            else if (Math.Abs(b - 1.0) < 0.05)
                return "Harmonic (b≈1)";
            else
                return $"Custom (b={b:F3})";
        }

        /// <summary>
        /// Helper method to determine time unit from context (for error messages).
        /// </summary>
        private static string GetTimeUnitFromContext()
        {
            return "per day"; // Assumes daily rates; can be parameterized if needed
        }

        #endregion
    }
}
