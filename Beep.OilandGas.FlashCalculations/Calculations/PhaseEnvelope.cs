using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.FlashCalculations.Calculations
{
    /// <summary>
    /// Constructs vapour-liquid phase envelopes (bubble and dew point curves) using
    /// either Wilson correlation K-values or EOS-based fugacity coefficients.
    ///
    /// A phase envelope traces the locus of conditions at which a feed mixture first
    /// forms a vapour (bubble point curve) or first condenses a drop of liquid
    /// (dew point curve). Together they delineate the two-phase region in P-T space.
    ///
    /// Algorithm:
    ///   1. For each temperature step, find P (bubble or dew) by nested Newton-Raphson:
    ///      Bubble: Σ Ki(T,P)·zi = 1  (vapour fraction → 0)
    ///      Dew:    Σ zi/Ki(T,P) = 1  (vapour fraction → 1)
    ///   2. Optionally, refine K-values using PR/SRK EOS for accuracy near the
    ///      cricondentherm / cricondenbar.
    ///
    /// References:
    ///   - Whitson, C.H. and Brulé, M.R. (2000) "Phase Behavior of Petroleum Reservoir
    ///     Fluids", SPE Monograph Series, Ch. 3.
    ///   - Michelsen, M.L. (1980) "Calculation of Phase Envelopes and Critical Points
    ///     for Multicomponent Mixtures", Fluid Phase Equilibria 4(1) 1-10.
    /// </summary>
    public static class PhaseEnvelope
    {
        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>A single (T, P) point on the phase envelope.</summary>
        public sealed class EnvelopePoint
        {
            /// <summary>Temperature (°R).</summary>
            public double Temperature { get; set; }

            /// <summary>Pressure (psia).</summary>
            public double Pressure { get; set; }

            /// <summary>"Bubble" or "Dew".</summary>
            public string PointType { get; set; } = string.Empty;

            /// <summary>K-values at this point.</summary>
            public double[] KValues { get; set; } = Array.Empty<double>();

            /// <summary>Whether the solver converged at this point.</summary>
            public bool Converged { get; set; }
        }

        /// <summary>Complete phase envelope calculation result.</summary>
        public sealed class PhaseEnvelopeResult
        {
            /// <summary>Bubble point curve (P vs T).</summary>
            public List<EnvelopePoint> BubbleCurve { get; set; } = new();

            /// <summary>Dew point curve (P vs T).</summary>
            public List<EnvelopePoint> DewCurve { get; set; } = new();

            /// <summary>Estimated cricondentherm temperature (°R) — the maximum temperature on the envelope.</summary>
            public double CricondenthermTemperature { get; set; }

            /// <summary>Estimated cricondentherm pressure (psia).</summary>
            public double CricondenthermPressure { get; set; }

            /// <summary>Estimated cricondenbar pressure (psia) — the maximum pressure on the envelope.</summary>
            public double CricondenbarPressure { get; set; }

            /// <summary>Estimated cricondenbar temperature (°R).</summary>
            public double CricondenbarTemperature { get; set; }

            /// <summary>Estimated critical point temperature (°R) — where bubble and dew curves meet.</summary>
            public double CriticalTemperature { get; set; }

            /// <summary>Estimated critical pressure (psia).</summary>
            public double CriticalPressure { get; set; }

            /// <summary>EOS type used (or "Wilson" for correlation-only).</summary>
            public string EosUsed { get; set; } = "Wilson";
        }

        /// <summary>Result of composition sensitivity on bubble / dew point.</summary>
        public sealed class CompositionSensitivityResult
        {
            /// <summary>Component name that was varied.</summary>
            public string ComponentName { get; set; } = string.Empty;

            /// <summary>Mole fractions tested for the varied component.</summary>
            public List<double> VariedFractions { get; set; } = new();

            /// <summary>Bubble point pressure at each fraction (psia).</summary>
            public List<double> BubblePointPressures { get; set; } = new();

            /// <summary>Dew point pressure at each fraction (psia).</summary>
            public List<double> DewPointPressures { get; set; } = new();

            /// <summary>Temperature used for the sensitivity sweep (°R).</summary>
            public double Temperature { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Wilson K-value (fast, approximate)
        // ─────────────────────────────────────────────────────────────────

        private static double WilsonK(double pressure, double temperature, FLASH_COMPONENT c)
        {
            double tc = (double)c.CRITICAL_TEMPERATURE;
            double pc = (double)c.CRITICAL_PRESSURE;
            double omega = (double)c.ACENTRIC_FACTOR;
            if (tc <= 0 || pc <= 0) return 1.0;
            return (pc / pressure) * Math.Exp(5.37 * (1.0 + omega) * (1.0 - tc / temperature));
        }

        // ─────────────────────────────────────────────────────────────────
        //  Bubble point pressure at fixed T
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Finds the bubble point pressure at a given temperature by Newton-Raphson
        /// on the condition Σ Ki·zi = 1.
        ///
        /// Starting estimate: P_bubble ≈ Σ zi·Pc_i·exp(5.37(1+ωi)(1−Tc_i/T))
        /// which follows directly from Wilson K-values set Ki = 1 for all i.
        /// </summary>
        /// <param name="components">Feed composition.</param>
        /// <param name="temperature">Temperature (°R).</param>
        /// <param name="pInitial">Initial pressure guess (psia). Auto-estimated if ≤ 0.</param>
        /// <param name="useEos">Whether to refine with PR EOS (slower, more accurate).</param>
        /// <param name="kij">Binary interaction parameters (optional, used with EOS).</param>
        /// <returns>Bubble point <see cref="EnvelopePoint"/> or null if not found.</returns>
        public static EnvelopePoint? FindBubblePoint(
            List<FLASH_COMPONENT> components,
            double temperature,
            double pInitial = -1,
            bool useEos = false,
            double[,]? kij = null)
        {
            int n = components.Count;
            double[] z = components.Select(c => (double)c.MOLE_FRACTION).ToArray();

            // Initial estimate: P from Raoult-like Wilson rearrangement
            if (pInitial <= 0)
            {
                double sum = 0;
                foreach (var c in components)
                {
                    double tc = (double)c.CRITICAL_TEMPERATURE;
                    double pc = (double)c.CRITICAL_PRESSURE;
                    double omega = (double)c.ACENTRIC_FACTOR;
                    sum += (double)c.MOLE_FRACTION * pc
                           * Math.Exp(5.37 * (1.0 + omega) * (1.0 - tc / temperature));
                }
                pInitial = Math.Max(1.0, sum);
            }

            double p = pInitial;
            const int maxIter = 80;
            const double tol = 1e-5;

            double[] k = new double[n];
            bool converged = false;

            for (int iter = 0; iter < maxIter; iter++)
            {
                // Compute K-values
                if (useEos)
                {
                    // Use Wilson as initial K and EOS to refine in an outer loop
                    for (int i = 0; i < n; i++) k[i] = WilsonK(p, temperature, components[i]);
                    double[] y = NormalizeK(z, k, 1.0);   // vapour at dew (V→0 so y≈K*z)
                    var eosK = AdvancedEOS.CalculateEosKValues(z, y, z, components, p, temperature,
                        AdvancedEOS.EosType.PengRobinson, kij);
                    for (int i = 0; i < n; i++) k[i] = eosK.KValues[i];
                }
                else
                {
                    for (int i = 0; i < n; i++) k[i] = WilsonK(p, temperature, components[i]);
                }

                double f = 0.0;
                for (int i = 0; i < n; i++) f += z[i] * k[i] - 1.0;

                if (Math.Abs(f) < tol) { converged = true; break; }

                // Numerical dP derivative (forward difference)
                double dp = p * 1e-4;
                double[] k2 = new double[n];
                for (int i = 0; i < n; i++) k2[i] = WilsonK(p + dp, temperature, components[i]);
                double df = 0.0;
                for (int i = 0; i < n; i++) df += z[i] * (k2[i] - k[i]) / dp;

                if (Math.Abs(df) < 1e-30) break;
                double pNew = p - f / df;
                p = Math.Max(0.1, pNew);
            }

            if (!converged) return null;

            return new EnvelopePoint
            {
                Temperature = temperature,
                Pressure = p,
                PointType = "Bubble",
                KValues = k,
                Converged = true
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Dew point pressure at fixed T
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Finds the dew point pressure at a given temperature by Newton-Raphson
        /// on the condition Σ zi/Ki = 1.
        /// </summary>
        public static EnvelopePoint? FindDewPoint(
            List<FLASH_COMPONENT> components,
            double temperature,
            double pInitial = -1,
            bool useEos = false,
            double[,]? kij = null)
        {
            int n = components.Count;
            double[] z = components.Select(c => (double)c.MOLE_FRACTION).ToArray();

            // Initial estimate: P from Wilson dew-point relation Σ zi/Ki = 1 → P ≈ 1/Σ(zi/Pc_i·exp(...))
            if (pInitial <= 0)
            {
                double sum = 0;
                foreach (var c in components)
                {
                    double tc = (double)c.CRITICAL_TEMPERATURE;
                    double pc = (double)c.CRITICAL_PRESSURE;
                    double omega = (double)c.ACENTRIC_FACTOR;
                    if (tc <= 0 || pc <= 0) continue;
                    sum += (double)c.MOLE_FRACTION
                           / (pc * Math.Exp(5.37 * (1.0 + omega) * (1.0 - tc / temperature)));
                }
                pInitial = sum > 1e-20 ? Math.Max(1.0, 1.0 / sum) : 100.0;
            }

            double p = pInitial;
            const int maxIter = 80;
            const double tol = 1e-5;

            double[] k = new double[n];
            bool converged = false;

            for (int iter = 0; iter < maxIter; iter++)
            {
                if (useEos)
                {
                    for (int i = 0; i < n; i++) k[i] = WilsonK(p, temperature, components[i]);
                    double[] x = NormalizeK(z, k, -1.0);  // liquid at bubble (V→1 so x≈z/K)
                    var eosK = AdvancedEOS.CalculateEosKValues(z, z, x, components, p, temperature,
                        AdvancedEOS.EosType.PengRobinson, kij);
                    for (int i = 0; i < n; i++) k[i] = eosK.KValues[i];
                }
                else
                {
                    for (int i = 0; i < n; i++) k[i] = WilsonK(p, temperature, components[i]);
                }

                double f = 0.0;
                for (int i = 0; i < n; i++)
                    f += k[i] > 1e-15 ? z[i] / k[i] - 1.0 / n : 0.0;
                f = 0.0;
                for (int i = 0; i < n; i++) f += (k[i] > 1e-15 ? z[i] / k[i] : 0.0);
                f -= 1.0;

                if (Math.Abs(f) < tol) { converged = true; break; }

                double dp = p * 1e-4;
                double[] k2 = new double[n];
                for (int i = 0; i < n; i++) k2[i] = WilsonK(p + dp, temperature, components[i]);
                double df = 0.0;
                for (int i = 0; i < n; i++)
                    df += k[i] > 1e-15 && k2[i] > 1e-15 ? -z[i] * (k2[i] - k[i]) / (k[i] * k2[i] * dp) : 0.0;

                if (Math.Abs(df) < 1e-30) break;
                double pNew = p - f / df;
                p = Math.Max(0.1, pNew);
            }

            if (!converged) return null;

            return new EnvelopePoint
            {
                Temperature = temperature,
                Pressure = p,
                PointType = "Dew",
                KValues = k,
                Converged = true
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Full phase envelope construction
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Constructs a full phase envelope by sweeping temperature from
        /// <paramref name="minTempR"/> to <paramref name="maxTempR"/> in
        /// <paramref name="steps"/> increments, computing both bubble and dew points.
        ///
        /// The cricondentherm, cricondenbar, and approximate critical point are
        /// estimated from the resulting curve.
        /// </summary>
        /// <param name="components">Feed composition with mole fractions summing to 1.</param>
        /// <param name="minTempR">Minimum temperature of sweep (°R). Default: 200 °R (−260 °F).</param>
        /// <param name="maxTempR">Maximum temperature of sweep (°R). Default: 1500 °R (1040 °F).</param>
        /// <param name="steps">Number of temperature steps. Default: 40.</param>
        /// <param name="useEos">Refine with PR EOS if true.</param>
        public static PhaseEnvelopeResult CalculateEnvelope(
            List<FLASH_COMPONENT> components,
            double minTempR = 400.0,
            double maxTempR = 1200.0,
            int steps = 40,
            bool useEos = false)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components required.", nameof(components));

            double[,]? kij = useEos ? AdvancedEOS.GetBinaryInteractionParameters(components) : null;
            var result = new PhaseEnvelopeResult { EosUsed = useEos ? "PR-EOS" : "Wilson" };

            double dT = (maxTempR - minTempR) / Math.Max(1, steps);
            double prevBubbleP = -1, prevDewP = -1;

            for (int s = 0; s <= steps; s++)
            {
                double T = minTempR + s * dT;

                var bp = FindBubblePoint(components, T, prevBubbleP, useEos, kij);
                if (bp != null) { result.BubbleCurve.Add(bp); prevBubbleP = bp.Pressure; }

                var dp = FindDewPoint(components, T, prevDewP, useEos, kij);
                if (dp != null) { result.DewCurve.Add(dp); prevDewP = dp.Pressure; }
            }

            // Cricondentherm = highest T with a converged point on either curve
            var allPoints = result.BubbleCurve.Concat(result.DewCurve).ToList();
            if (allPoints.Count > 0)
            {
                var maxT = allPoints.OrderByDescending(p => p.Temperature).First();
                result.CricondenthermTemperature = maxT.Temperature;
                result.CricondenthermPressure = maxT.Pressure;

                // Cricondenbar = highest P on either curve
                var maxP = allPoints.OrderByDescending(p => p.Pressure).First();
                result.CricondenbarPressure = maxP.Pressure;
                result.CricondenbarTemperature = maxP.Temperature;

                // Approximate critical point: where bubble and dew pressures are closest at same T
                EstimateCriticalPoint(result);
            }

            return result;
        }

        private static void EstimateCriticalPoint(PhaseEnvelopeResult result)
        {
            if (!result.BubbleCurve.Any() || !result.DewCurve.Any()) return;

            double bestDist = double.MaxValue;
            double critT = 0, critP = 0;

            foreach (var bp in result.BubbleCurve)
            {
                // Find the nearest dew point by temperature
                var dp = result.DewCurve.OrderBy(d => Math.Abs(d.Temperature - bp.Temperature)).FirstOrDefault();
                if (dp == null) continue;

                double dist = Math.Abs(bp.Pressure - dp.Pressure)
                              + Math.Abs(bp.Temperature - dp.Temperature) * 5.0;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    critT = (bp.Temperature + dp.Temperature) / 2.0;
                    critP = (bp.Pressure + dp.Pressure) / 2.0;
                }
            }

            result.CriticalTemperature = critT;
            result.CriticalPressure = critP;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Composition sensitivity
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs a composition sensitivity analysis: varies the mole fraction of one
        /// component and rescales the others proportionally, then computes bubble and dew
        /// point pressures at a fixed temperature.
        ///
        /// Useful for determining how compositional uncertainty affects the phase boundary.
        /// </summary>
        /// <param name="components">Base composition.</param>
        /// <param name="componentName">Name of the component to vary.</param>
        /// <param name="temperature">Temperature (°R).</param>
        /// <param name="minFraction">Minimum mole fraction to test.</param>
        /// <param name="maxFraction">Maximum mole fraction to test.</param>
        /// <param name="fractionSteps">Number of steps in the sweep.</param>
        /// <param name="useEos">Use EOS for K-values if true.</param>
        public static CompositionSensitivityResult CompositionSensitivity(
            List<FLASH_COMPONENT> components,
            string componentName,
            double temperature,
            double minFraction = 0.0,
            double maxFraction = 0.5,
            int fractionSteps = 10,
            bool useEos = false)
        {
            var result = new CompositionSensitivityResult
            {
                ComponentName = componentName,
                Temperature = temperature
            };

            int targetIdx = components.FindIndex(c =>
                string.Equals(c.COMPONENT_NAME, componentName, StringComparison.OrdinalIgnoreCase));
            if (targetIdx < 0) return result;

            double baseOthers = components
                .Where((_, i) => i != targetIdx)
                .Sum(c => (double)c.MOLE_FRACTION);
            if (baseOthers <= 0) return result;

            double dz = fractionSteps > 1 ? (maxFraction - minFraction) / (fractionSteps - 1) : 0;

            for (int s = 0; s < fractionSteps; s++)
            {
                double zTarget = minFraction + s * dz;
                double scale = (1.0 - zTarget) / baseOthers;

                // Build modified composition
                var modified = components.Select((c, i) =>
                {
                    var copy = new FLASH_COMPONENT
                    {
                        COMPONENT_NAME = c.COMPONENT_NAME,
                        MOLE_FRACTION = i == targetIdx ? (decimal)zTarget : (decimal)((double)c.MOLE_FRACTION * scale),
                        CRITICAL_TEMPERATURE = c.CRITICAL_TEMPERATURE,
                        CRITICAL_PRESSURE = c.CRITICAL_PRESSURE,
                        ACENTRIC_FACTOR = c.ACENTRIC_FACTOR,
                        MOLECULAR_WEIGHT = c.MOLECULAR_WEIGHT
                    };
                    return copy;
                }).ToList();

                result.VariedFractions.Add(zTarget);

                var bp = FindBubblePoint(modified, temperature, -1, useEos);
                result.BubblePointPressures.Add(bp?.Pressure ?? double.NaN);

                var dp = FindDewPoint(modified, temperature, -1, useEos);
                result.DewPointPressures.Add(dp?.Pressure ?? double.NaN);
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Normalises K-values to produce a phase composition.
        /// sign = +1 → vapour composition y_i = K_i·z_i / Σ (K_j·z_j)
        /// sign = -1 → liquid composition x_i = (z_i/K_i) / Σ (z_j/K_j)
        /// </summary>
        private static double[] NormalizeK(double[] z, double[] k, double sign)
        {
            int n = z.Length;
            double[] result = new double[n];
            double sum = 0;
            for (int i = 0; i < n; i++)
                sum += sign > 0 ? z[i] * k[i] : (k[i] > 1e-15 ? z[i] / k[i] : 0.0);
            if (sum < 1e-15) { for (int i = 0; i < n; i++) result[i] = 1.0 / n; return result; }
            for (int i = 0; i < n; i++)
                result[i] = sign > 0 ? z[i] * k[i] / sum : (k[i] > 1e-15 ? z[i] / k[i] / sum : 0.0);
            return result;
        }
    }
}
