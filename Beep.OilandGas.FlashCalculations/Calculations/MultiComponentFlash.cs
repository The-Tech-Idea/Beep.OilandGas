using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.FlashCalculations.Calculations
{
    /// <summary>
    /// Enhanced multi-component flash calculations:
    /// <list type="bullet">
    ///   <item>Isothermal VL flash with accelerated successive substitution (GDEM) and
    ///         improved near-critical convergence.</item>
    ///   <item>Three-phase vapour-liquid-liquid (VLLE) flash for hydrocarbon + aqueous systems.</item>
    ///   <item>Azeotrope detection at a given (T, P) condition.</item>
    /// </list>
    ///
    /// K-values are obtained from either:
    ///   - Wilson correlation (fast, suitable for initial estimates), or
    ///   - PR EOS fugacity coefficients (accurate, required for near-critical and VLLE).
    ///
    /// All convergence loops use the Rachford-Rice objective function from
    /// <see cref="FlashCalculator"/> and fugacity coefficients from <see cref="AdvancedEOS"/>.
    ///
    /// References:
    ///   - Michelsen, M.L. (1982) "The Isothermal Flash Problem. Part I: Stability",
    ///     Fluid Phase Equilibria 9(1) 1-19.
    ///   - Michelsen, M.L. (1982) "The Isothermal Flash Problem. Part II: Phase-Split
    ///     Calculation", Fluid Phase Equilibria 9(1) 21-40.
    ///   - Nghiem, L.X. and Li, Y-K. (1984) "Computation of Multiphase Equilibrium Phenomena
    ///     with an Equation of State", Fluid Phase Equilibria 17 77-95.
    ///   - Whitson, C.H. and Brulé, M.R. (2000) "Phase Behavior of Petroleum Reservoir
    ///     Fluids", SPE Monograph Series, Ch. 4.
    /// </summary>
    public static class MultiComponentFlash
    {
        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Result of an improved two-phase isothermal flash.</summary>
        public sealed class EnhancedFlashResult
        {
            /// <summary>Vapour fraction β (0–1).</summary>
            public double VaporFraction { get; set; }

            /// <summary>Liquid fraction 1-β.</summary>
            public double LiquidFraction { get; set; }

            /// <summary>Vapour phase mole fractions.</summary>
            public double[] VaporComposition { get; set; } = Array.Empty<double>();

            /// <summary>Liquid phase mole fractions.</summary>
            public double[] LiquidComposition { get; set; } = Array.Empty<double>();

            /// <summary>K-values (Ki = yi/xi).</summary>
            public double[] KValues { get; set; } = Array.Empty<double>();

            /// <summary>Fugacity balance residual norm at convergence.</summary>
            public double ConvergenceResidual { get; set; }

            /// <summary>Number of outer fugacity iterations.</summary>
            public int Iterations { get; set; }

            /// <summary>Whether the calculation converged.</summary>
            public bool Converged { get; set; }

            /// <summary>Stability test result before split calculation.</summary>
            public string StabilityResult { get; set; } = string.Empty;

            /// <summary>EOS type used.</summary>
            public string EosUsed { get; set; } = string.Empty;
        }

        /// <summary>Result of a three-phase VLLE flash.</summary>
        public sealed class ThreePhaseFlashResult
        {
            /// <summary>Vapour fraction.</summary>
            public double VaporFraction { get; set; }

            /// <summary>Hydrocarbon-rich liquid fraction.</summary>
            public double LiquidHcFraction { get; set; }

            /// <summary>Aqueous liquid fraction.</summary>
            public double LiquidAqueousFraction { get; set; }

            /// <summary>Vapour phase mole fractions.</summary>
            public double[] VaporComposition { get; set; } = Array.Empty<double>();

            /// <summary>HC-liquid mole fractions.</summary>
            public double[] LiquidHcComposition { get; set; } = Array.Empty<double>();

            /// <summary>Aqueous-liquid mole fractions.</summary>
            public double[] LiquidAqueousComposition { get; set; } = Array.Empty<double>();

            /// <summary>K-values vapour/HC-liquid.</summary>
            public double[] KVL { get; set; } = Array.Empty<double>();

            /// <summary>K-values HC-liquid/aqueous-liquid.</summary>
            public double[] KLL { get; set; } = Array.Empty<double>();

            public bool Converged { get; set; }
            public int Iterations { get; set; }
            public string Notes { get; set; } = string.Empty;
        }

        /// <summary>Azeotrope detection result at a single (T, P) condition.</summary>
        public sealed class AzeotropeResult
        {
            /// <summary>Whether an azeotrope is detected (Ki ≈ 1 for two or more components simultaneously).</summary>
            public bool AzeotropeDetected { get; set; }

            /// <summary>Components with Ki ≈ 1 (within tolerance).</summary>
            public List<string> AzeotropeComponents { get; set; } = new();

            /// <summary>K-values for all components at this (T, P).</summary>
            public double[] KValues { get; set; } = Array.Empty<double>();

            /// <summary>Relative volatility deviation: max |Ki − 1| among azeotrope-suspected components.</summary>
            public double MaxDeviation { get; set; }

            /// <summary>Temperature used (°R).</summary>
            public double Temperature { get; set; }

            /// <summary>Pressure used (psia).</summary>
            public double Pressure { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────
        //  EOS-based two-phase flash with stability test (Michelsen 1982)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs a rigorous isothermal VL flash using PR EOS fugacity coefficients with
        /// an outer successive-substitution loop accelerated by GDEM (General Dominant
        /// Eigenvalue Method, Michelsen 1986).
        ///
        /// Step 1: Stability test — checks whether the single-phase feed is stable by
        ///         minimising the tangent-plane distance (TPD) function.
        /// Step 2: If unstable, solve Rachford-Rice for β.
        /// Step 3: Update K-values from φL/φV until Σ|ln(φV_i·yi) − ln(φL_i·xi)| &lt; 1e-9.
        /// Step 4: Apply GDEM acceleration every 3 iterations to improve near-critical
        ///         convergence (K-value log-space update).
        ///
        /// Reference: Michelsen (1982a,b); Nghiem &amp; Li (1984).
        /// </summary>
        /// <param name="components">Feed composition.</param>
        /// <param name="pressure">Pressure (psia).</param>
        /// <param name="temperature">Temperature (°R).</param>
        /// <param name="eos">EOS type (default: PengRobinson).</param>
        /// <param name="kij">Binary interaction parameters (null → use built-in table).</param>
        /// <param name="maxOuterIter">Maximum outer fugacity iterations.</param>
        public static EnhancedFlashResult PerformEnhancedFlash(
            List<FLASH_COMPONENT> components,
            double pressure,
            double temperature,
            AdvancedEOS.EosType eos = AdvancedEOS.EosType.PengRobinson,
            double[,]? kij = null,
            int maxOuterIter = 100)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components required.", nameof(components));

            int n = components.Count;
            double[] z = components.Select(c => (double)c.MOLE_FRACTION).ToArray();
            kij ??= AdvancedEOS.GetBinaryInteractionParameters(components);

            // ── Stability test ─────────────────────────────────────────
            bool stable = StabilityTest(z, components, pressure, temperature, eos, kij,
                out double[] kInit);

            if (stable)
            {
                // Single-phase — compute Z only
                double[] a = components.Select(c => AdvancedEOS.CalcA(
                    (double)c.CRITICAL_TEMPERATURE, (double)c.CRITICAL_PRESSURE,
                    (double)c.ACENTRIC_FACTOR, temperature, eos)).ToArray();
                double[] b = components.Select(c => AdvancedEOS.CalcB(
                    (double)c.CRITICAL_TEMPERATURE, (double)c.CRITICAL_PRESSURE, eos)).ToArray();
                var fug = AdvancedEOS.CalculateFugacityCoefficients(z, a, b, pressure, temperature,
                    eos, isVapour: true, kij);

                return new EnhancedFlashResult
                {
                    VaporFraction = 1.0,
                    LiquidFraction = 0.0,
                    VaporComposition = z,
                    LiquidComposition = z,
                    KValues = Enumerable.Repeat(1.0, n).ToArray(),
                    Converged = true,
                    StabilityResult = "Stable single-phase",
                    EosUsed = eos.ToString()
                };
            }

            // ── Phase split ────────────────────────────────────────────
            double[] k = kInit;
            double beta = 0.5;  // initial vapour fraction guess
            double[] y = new double[n], x = new double[n];

            // History arrays for GDEM (store last 3 lnK increments)
            var lnKHistory = new Queue<double[]>();

            double[] a2 = components.Select(c => AdvancedEOS.CalcA(
                (double)c.CRITICAL_TEMPERATURE, (double)c.CRITICAL_PRESSURE,
                (double)c.ACENTRIC_FACTOR, temperature, eos)).ToArray();
            double[] b2 = components.Select(c => AdvancedEOS.CalcB(
                (double)c.CRITICAL_TEMPERATURE, (double)c.CRITICAL_PRESSURE, eos)).ToArray();

            bool converged = false;
            double residual = double.MaxValue;
            int iter;

            for (iter = 0; iter < maxOuterIter; iter++)
            {
                // Solve Rachford-Rice for β
                beta = SolveRachfordRiceDouble(z, k, beta);

                // Compute phase compositions
                for (int i = 0; i < n; i++)
                {
                    x[i] = z[i] / (1.0 + beta * (k[i] - 1.0));
                    y[i] = k[i] * x[i];
                }
                NormalizeInPlace(x);
                NormalizeInPlace(y);

                // Fugacity coefficients
                var fugV = AdvancedEOS.CalculateFugacityCoefficients(y, a2, b2, pressure, temperature,
                    eos, isVapour: true, kij);
                var fugL = AdvancedEOS.CalculateFugacityCoefficients(x, a2, b2, pressure, temperature,
                    eos, isVapour: false, kij);

                // Fugacity balance residual
                double[] R_vec = new double[n];
                for (int i = 0; i < n; i++)
                    R_vec[i] = Math.Log(fugL.FugacityCoefficients[i] * x[i])
                               - Math.Log(fugV.FugacityCoefficients[i] * y[i]);
                residual = R_vec.Sum(r => r * r);

                if (residual < 1e-14) { converged = true; break; }

                // Standard successive substitution: ln K_i(new) = ln φL_i - ln φV_i
                double[] lnKNew = new double[n];
                for (int i = 0; i < n; i++)
                    lnKNew[i] = fugL.LnFugacityCoefficients[i] - fugV.LnFugacityCoefficients[i];

                // GDEM acceleration every 3 iterations
                lnKHistory.Enqueue(lnKNew);
                if (lnKHistory.Count > 3) lnKHistory.Dequeue();

                double[] lnKStep = iter >= 2 && lnKHistory.Count >= 3
                    ? GdemAccelerate(lnKHistory.ToArray())
                    : lnKNew;

                for (int i = 0; i < n; i++)
                    k[i] = Math.Exp(lnKStep[i]);
            }

            return new EnhancedFlashResult
            {
                VaporFraction = beta,
                LiquidFraction = 1.0 - beta,
                VaporComposition = y,
                LiquidComposition = x,
                KValues = k,
                ConvergenceResidual = Math.Sqrt(residual),
                Iterations = iter,
                Converged = converged,
                StabilityResult = "Two-phase",
                EosUsed = eos.ToString()
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Three-phase VLLE flash (Nghiem & Li, 1984)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Solves a three-phase vapour-liquid-liquid equilibrium (VLLE) flash for a
        /// hydrocarbon + aqueous system.
        ///
        /// Two independent Rachford-Rice equations are solved simultaneously for the
        /// vapour fraction β_V and the aqueous-phase fraction β_W:
        ///   Σ z_i(K1_i − 1) / D_i = 0    [vapour relative to HC-liquid]
        ///   Σ z_i(K2_i − 1) / D_i = 0    [aqueous relative to HC-liquid]
        ///   D_i = 1 + β_V(K1_i−1) + β_W(K2_i−1)
        ///
        /// K1_i = y_i / x_HC_i  (vapour / HC-liquid)
        /// K2_i = x_W_i / x_HC_i (aqueous / HC-liquid)
        ///
        /// Initial K1 from Wilson; K2 estimated from Henry's-law type split:
        ///   K2_i = 1 for water, 0.001 for hydrocarbons.
        ///
        /// Reference: Nghiem &amp; Li (1984), Fluid Phase Equilibria 17, 77-95.
        /// </summary>
        /// <param name="components">Feed composition. Include water (COMPONENT_NAME = "H2O" or "Water").</param>
        /// <param name="pressure">Pressure (psia).</param>
        /// <param name="temperature">Temperature (°R).</param>
        public static ThreePhaseFlashResult PerformThreePhaseFlash(
            List<FLASH_COMPONENT> components,
            double pressure,
            double temperature)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components required.", nameof(components));

            int n = components.Count;
            double[] z = components.Select(c => (double)c.MOLE_FRACTION).ToArray();

            // Initial K-values
            double[] k1 = components.Select(c => (double)FlashCalculator.CalculateWilsonKValue(
                (decimal)pressure, (decimal)temperature, c)).ToArray();

            double[] k2 = components.Select(c =>
            {
                string name = c.COMPONENT_NAME ?? "";
                return name.StartsWith("H2O", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("Water", StringComparison.OrdinalIgnoreCase)
                    ? 100.0   // water strongly prefers aqueous phase
                    : 0.001;  // hydrocarbons strongly prefer HC-liquid
            }).ToArray();

            double betaV = 0.3, betaW = 0.2;
            const int maxIter = 100;
            const double tol = 1e-8;
            bool converged = false;
            int iter;

            for (iter = 0; iter < maxIter; iter++)
            {
                // Compute D_i = 1 + βV(K1i-1) + βW(K2i-1)
                double[] d = new double[n];
                for (int i = 0; i < n; i++)
                    d[i] = 1.0 + betaV * (k1[i] - 1.0) + betaW * (k2[i] - 1.0);

                // Two Rachford-Rice residuals
                double f1 = 0, f2 = 0;
                for (int i = 0; i < n; i++)
                {
                    f1 += z[i] * (k1[i] - 1.0) / d[i];
                    f2 += z[i] * (k2[i] - 1.0) / d[i];
                }

                if (Math.Abs(f1) < tol && Math.Abs(f2) < tol) { converged = true; break; }

                // Jacobian
                double j11 = 0, j12 = 0, j21 = 0, j22 = 0;
                for (int i = 0; i < n; i++)
                {
                    double di2 = d[i] * d[i];
                    j11 -= z[i] * (k1[i] - 1.0) * (k1[i] - 1.0) / di2;
                    j12 -= z[i] * (k1[i] - 1.0) * (k2[i] - 1.0) / di2;
                    j21 -= z[i] * (k2[i] - 1.0) * (k1[i] - 1.0) / di2;
                    j22 -= z[i] * (k2[i] - 1.0) * (k2[i] - 1.0) / di2;
                }

                double det = j11 * j22 - j12 * j21;
                if (Math.Abs(det) < 1e-20) break;

                double dBetaV = -(j22 * f1 - j12 * f2) / det;
                double dBetaW = -(j11 * f2 - j21 * f1) / det;

                betaV = Math.Max(0.001, Math.Min(0.999, betaV + dBetaV));
                betaW = Math.Max(0.001, Math.Min(0.999 - betaV, betaW + dBetaW));

                // Update compositions
                double[] xHC = new double[n], xW = new double[n], y = new double[n];
                for (int i = 0; i < n; i++)
                {
                    xHC[i] = z[i] / d[i];
                    y[i] = k1[i] * xHC[i];
                    xW[i] = k2[i] * xHC[i];
                }
                NormalizeInPlace(xHC);
                NormalizeInPlace(xW);
                NormalizeInPlace(y);

                // Update K-values using fugacity successively (Wilson only here for stability)
                for (int i = 0; i < n; i++)
                {
                    k1[i] = xHC[i] > 1e-15 ? y[i] / xHC[i] : k1[i];
                    k2[i] = xHC[i] > 1e-15 ? xW[i] / xHC[i] : k2[i];
                }
            }

            // Final compositions
            double[] xHCFinal = new double[n], xWFinal = new double[n], yFinal = new double[n];
            double[] dFinal = new double[n];
            for (int i = 0; i < n; i++)
                dFinal[i] = 1.0 + betaV * (k1[i] - 1.0) + betaW * (k2[i] - 1.0);
            for (int i = 0; i < n; i++)
            {
                xHCFinal[i] = z[i] / dFinal[i];
                yFinal[i] = k1[i] * xHCFinal[i];
                xWFinal[i] = k2[i] * xHCFinal[i];
            }
            NormalizeInPlace(xHCFinal);
            NormalizeInPlace(xWFinal);
            NormalizeInPlace(yFinal);

            double betaHC = Math.Max(0, 1.0 - betaV - betaW);

            return new ThreePhaseFlashResult
            {
                VaporFraction = betaV,
                LiquidHcFraction = betaHC,
                LiquidAqueousFraction = betaW,
                VaporComposition = yFinal,
                LiquidHcComposition = xHCFinal,
                LiquidAqueousComposition = xWFinal,
                KVL = k1,
                KLL = k2,
                Converged = converged,
                Iterations = iter,
                Notes = converged ? "Three-phase VLLE converged." : "Warning: VLLE did not fully converge — check initial estimates."
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Azeotrope detection
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects whether the mixture exhibits azeotrope-like behaviour at (T, P) by
        /// checking if EOS K-values for two or more components are simultaneously close to 1.
        ///
        /// A binary azeotrope is characterised by K_i → 1 at the azeotropic condition.
        /// For multicomponent mixtures, the Michelsen stability criterion shows that the
        /// mixture is at its azeotropic point when the Gibbs energy surface is flat along
        /// the direction of composition change — practically, when ≥2 K-values ≈ 1.
        ///
        /// Reference: Michelsen (1980), Fluid Phase Equilibria 4(1), Appendix B.
        /// </summary>
        /// <param name="components">Feed composition.</param>
        /// <param name="pressure">Pressure (psia).</param>
        /// <param name="temperature">Temperature (°R).</param>
        /// <param name="eos">EOS type.</param>
        /// <param name="tolerance">Tolerance for |Ki − 1|; default 0.05 (5 %).</param>
        /// <param name="kij">Binary interaction parameters (optional).</param>
        public static AzeotropeResult DetectAzeotrope(
            List<FLASH_COMPONENT> components,
            double pressure,
            double temperature,
            AdvancedEOS.EosType eos = AdvancedEOS.EosType.PengRobinson,
            double tolerance = 0.05,
            double[,]? kij = null)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components required.", nameof(components));

            int n = components.Count;
            double[] z = components.Select(c => (double)c.MOLE_FRACTION).ToArray();
            kij ??= AdvancedEOS.GetBinaryInteractionParameters(components);

            double[] a = components.Select(c => AdvancedEOS.CalcA(
                (double)c.CRITICAL_TEMPERATURE, (double)c.CRITICAL_PRESSURE,
                (double)c.ACENTRIC_FACTOR, temperature, eos)).ToArray();
            double[] b = components.Select(c => AdvancedEOS.CalcB(
                (double)c.CRITICAL_TEMPERATURE, (double)c.CRITICAL_PRESSURE, eos)).ToArray();

            // Feed fugacity both as "vapour" and as "liquid"
            var fugV = AdvancedEOS.CalculateFugacityCoefficients(z, a, b, pressure, temperature,
                eos, isVapour: true, kij);
            var fugL = AdvancedEOS.CalculateFugacityCoefficients(z, a, b, pressure, temperature,
                eos, isVapour: false, kij);

            double[] kValues = new double[n];
            var azeotropeComponents = new List<string>();
            double maxDev = 0;

            for (int i = 0; i < n; i++)
            {
                kValues[i] = fugL.FugacityCoefficients[i] /
                             Math.Max(1e-15, fugV.FugacityCoefficients[i]);
                double dev = Math.Abs(kValues[i] - 1.0);
                if (dev < tolerance)
                    azeotropeComponents.Add(components[i].COMPONENT_NAME ?? $"Component{i}");
                if (dev > maxDev) maxDev = dev;
            }

            bool detected = azeotropeComponents.Count >= 2;

            return new AzeotropeResult
            {
                AzeotropeDetected = detected,
                AzeotropeComponents = azeotropeComponents,
                KValues = kValues,
                MaxDeviation = maxDev,
                Temperature = temperature,
                Pressure = pressure
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Stability test (Michelsen TPD)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Michelsen tangent-plane distance stability test.
        /// Returns true if the single-phase mixture is stable (no incipient phase detected).
        /// Sets <paramref name="kInit"/> to the K-values for the incipient phase if unstable.
        /// </summary>
        private static bool StabilityTest(
            double[] z,
            List<FLASH_COMPONENT> components,
            double pressure,
            double temperature,
            AdvancedEOS.EosType eos,
            double[,] kij,
            out double[] kInit)
        {
            int n = z.Length;
            kInit = new double[n];

            double[] a = components.Select(c => AdvancedEOS.CalcA(
                (double)c.CRITICAL_TEMPERATURE, (double)c.CRITICAL_PRESSURE,
                (double)c.ACENTRIC_FACTOR, temperature, eos)).ToArray();
            double[] b = components.Select(c => AdvancedEOS.CalcB(
                (double)c.CRITICAL_TEMPERATURE, (double)c.CRITICAL_PRESSURE, eos)).ToArray();

            // Feed fugacity as liquid
            var fugFeed = AdvancedEOS.CalculateFugacityCoefficients(z, a, b, pressure, temperature,
                eos, isVapour: false, kij);

            // Trial phase: vapour-like (K from Wilson)
            double[] kTrial = components.Select(c =>
            {
                double tc = (double)c.CRITICAL_TEMPERATURE;
                double pc = (double)c.CRITICAL_PRESSURE;
                double omega = (double)c.ACENTRIC_FACTOR;
                if (tc <= 0 || pc <= 0) return 1.0;
                return (pc / pressure) * Math.Exp(5.37 * (1.0 + omega) * (1.0 - tc / temperature));
            }).ToArray();

            // Successive substitution to find stationary point of TPD
            for (int iter = 0; iter < 50; iter++)
            {
                double[] w = new double[n];
                double sumW = 0;
                for (int i = 0; i < n; i++) { w[i] = z[i] * kTrial[i]; sumW += w[i]; }
                for (int i = 0; i < n; i++) w[i] /= sumW;

                var fugTrial = AdvancedEOS.CalculateFugacityCoefficients(w, a, b, pressure, temperature,
                    eos, isVapour: true, kij);

                double tpd = 0;
                for (int i = 0; i < n; i++)
                {
                    double h = Math.Log(w[i]) + fugTrial.LnFugacityCoefficients[i]
                               - Math.Log(z[i]) - fugFeed.LnFugacityCoefficients[i];
                    tpd += w[i] * h;
                    kTrial[i] *= Math.Exp(-h);
                }

                if (Math.Abs(tpd) < 1e-8 || tpd > 0) break;
            }

            // Evaluate TPD at converged w
            double[] wFinal = new double[n];
            double sumWF = 0;
            for (int i = 0; i < n; i++) { wFinal[i] = z[i] * kTrial[i]; sumWF += wFinal[i]; }
            for (int i = 0; i < n; i++) wFinal[i] /= sumWF;

            var fugFinal = AdvancedEOS.CalculateFugacityCoefficients(wFinal, a, b, pressure, temperature,
                eos, isVapour: true, kij);
            double tpdFinal = 0;
            for (int i = 0; i < n; i++)
            {
                double h = Math.Log(wFinal[i]) + fugFinal.LnFugacityCoefficients[i]
                           - Math.Log(z[i]) - fugFeed.LnFugacityCoefficients[i];
                tpdFinal += wFinal[i] * h;
            }

            if (tpdFinal < -1e-6)
            {
                // Unstable: incipient phase found
                Array.Copy(kTrial, kInit, n);
                return false;
            }

            // Stable
            for (int i = 0; i < n; i++) kInit[i] = 1.0;
            return true;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Rachford-Rice solver (double precision)
        // ─────────────────────────────────────────────────────────────────

        private static double SolveRachfordRiceDouble(double[] z, double[] k, double betaGuess)
        {
            // Bounds to keep all 1 + β(Ki-1) > 0
            double betaMin = double.NegativeInfinity, betaMax = double.PositiveInfinity;
            for (int i = 0; i < z.Length; i++)
            {
                if (k[i] > 1.0) betaMax = Math.Min(betaMax, 1.0 / (1.0 - 1.0 / k[i]));
                else if (k[i] < 1.0) betaMin = Math.Max(betaMin, 1.0 / (1.0 - 1.0 / k[i]));
            }
            betaMin = Math.Max(0.0, betaMin + 1e-6);
            betaMax = Math.Min(1.0, betaMax - 1e-6);

            double beta = Math.Max(betaMin, Math.Min(betaMax, betaGuess));

            for (int iter = 0; iter < 60; iter++)
            {
                double f = 0, df = 0;
                for (int i = 0; i < z.Length; i++)
                {
                    double den = 1.0 + beta * (k[i] - 1.0);
                    if (Math.Abs(den) < 1e-15) continue;
                    f += z[i] * (k[i] - 1.0) / den;
                    df -= z[i] * (k[i] - 1.0) * (k[i] - 1.0) / (den * den);
                }
                if (Math.Abs(f) < 1e-12) break;
                if (Math.Abs(df) < 1e-30) break;

                double betaNew = beta - f / df;
                betaNew = Math.Max(betaMin, Math.Min(betaMax, betaNew));
                if (Math.Abs(betaNew - beta) < 1e-12) break;
                beta = betaNew;
            }

            return beta;
        }

        // ─────────────────────────────────────────────────────────────────
        //  GDEM acceleration (Michelsen 1986)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// General Dominant Eigenvalue Method (GDEM) accelerated lnK update.
        /// Uses the last 3 lnK vectors to extrapolate toward the fixed point.
        /// Reference: Michelsen (1986), Fluid Phase Equilibria 30 75-83.
        /// </summary>
        private static double[] GdemAccelerate(double[][] history)
        {
            // history[0] = oldest, history[2] = newest
            int n = history[0].Length;
            double[] d1 = new double[n], d2 = new double[n];
            for (int i = 0; i < n; i++) d1[i] = history[1][i] - history[0][i];
            for (int i = 0; i < n; i++) d2[i] = history[2][i] - history[1][i];

            double num = 0, den = 0;
            for (int i = 0; i < n; i++) { num += d1[i] * d2[i]; den += d1[i] * d1[i]; }
            double lambda = den > 1e-20 ? num / den : 1.0;
            lambda = Math.Max(-5.0, Math.Min(5.0, lambda));

            double[] result = new double[n];
            for (int i = 0; i < n; i++)
                result[i] = history[2][i] + lambda * d2[i];
            return result;
        }

        private static void NormalizeInPlace(double[] x)
        {
            double sum = x.Sum();
            if (sum < 1e-15) { for (int i = 0; i < x.Length; i++) x[i] = 1.0 / x.Length; return; }
            for (int i = 0; i < x.Length; i++) x[i] /= sum;
        }
    }
}
