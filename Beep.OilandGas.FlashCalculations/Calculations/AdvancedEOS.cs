using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.FlashCalculations.Calculations
{
    /// <summary>
    /// Peng-Robinson (PR) and Soave-Redlich-Kwong (SRK) cubic equations of state.
    ///
    /// Both EOSs are solved in the form:
    ///   Z³ + c2·Z² + c1·Z + c0 = 0
    ///
    /// and fugacity coefficients are computed from the departure function.
    ///
    /// Units: psia (pressure), °R (temperature), ft³/lb-mol (volume), lb-mol (quantity).
    ///   R = 10.7316 psia·ft³ / (lb-mol·°R)
    ///
    /// References:
    ///   - Peng, D.Y. and Robinson, D.B. (1976) IEC Fundamentals 15(1) 59-64.
    ///   - Soave, G. (1972) Chem. Eng. Sci. 27 1197-1203.
    ///   - Michelsen, M.L. and Mollerup, J.M. (2007) "Thermodynamic Models", 2nd ed.
    /// </summary>
    public static class AdvancedEOS
    {
        // ─────────────────────────────────────────────────────────────────
        //  Constants
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Gas constant in psia·ft³/(lb-mol·°R).</summary>
        public const double R = 10.7316;

        // PR EOS omega parameters
        private const double PR_OmegaA = 0.45724;
        private const double PR_OmegaB = 0.07780;

        // SRK EOS omega parameters
        private const double SRK_OmegaA = 0.42748;
        private const double SRK_OmegaB = 0.08664;

        // ─────────────────────────────────────────────────────────────────
        //  EOS Type
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Cubic equation of state to use.</summary>
        public enum EosType { PengRobinson, SoaveRedlichKwong }

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Z-factor and phase identification result.</summary>
        public sealed class ZFactorResult
        {
            /// <summary>Vapour-phase compressibility factor.</summary>
            public double ZVapour { get; set; }

            /// <summary>Liquid-phase compressibility factor.</summary>
            public double ZLiquid { get; set; }

            /// <summary>True if two real roots found (two-phase region).</summary>
            public bool TwoPhase { get; set; }

            /// <summary>EOS type used.</summary>
            public EosType Eos { get; set; }
        }

        /// <summary>Fugacity coefficient set for all components at a given phase composition.</summary>
        public sealed class FugacityResult
        {
            /// <summary>ln(φ_i) for each component.</summary>
            public double[] LnFugacityCoefficients { get; set; } = Array.Empty<double>();

            /// <summary>φ_i for each component.</summary>
            public double[] FugacityCoefficients { get; set; } = Array.Empty<double>();

            /// <summary>Mixture compressibility factor used.</summary>
            public double Z { get; set; }

            /// <summary>Whether the root was selected as vapour (true) or liquid (false).</summary>
            public bool IsVapour { get; set; }
        }

        /// <summary>EOS K-value flash result replacing Wilson correlation.</summary>
        public sealed class EosKValueResult
        {
            /// <summary>EOS K-value Ki = φ_L_i / φ_V_i for each component.</summary>
            public double[] KValues { get; set; } = Array.Empty<double>();

            /// <summary>Vapour fugacity result.</summary>
            public FugacityResult VapourPhase { get; set; } = new();

            /// <summary>Liquid fugacity result.</summary>
            public FugacityResult LiquidPhase { get; set; } = new();
        }

        // ─────────────────────────────────────────────────────────────────
        //  EOS parameters (a, b) per component
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the temperature-dependent attractive parameter a(T) for a single component.
        ///
        /// PR:  a(T) = Ω_a·R²·Tc²/Pc · α(T),  α = [1 + m·(1-√Tr)]²
        ///      m = 0.37464 + 1.54226ω − 0.26992ω²
        ///
        /// SRK: a(T) = Ω_a·R²·Tc²/Pc · α(T),  α = [1 + m·(1-√Tr)]²
        ///      m = 0.480 + 1.574ω − 0.176ω²
        /// </summary>
        public static double CalcA(double tc, double pc, double omega, double temperature, EosType eos)
        {
            double omegaA = eos == EosType.PengRobinson ? PR_OmegaA : SRK_OmegaA;
            double tr = temperature / tc;
            double m = eos == EosType.PengRobinson
                ? 0.37464 + 1.54226 * omega - 0.26992 * omega * omega
                : 0.480 + 1.574 * omega - 0.176 * omega * omega;
            double alpha = Math.Pow(1.0 + m * (1.0 - Math.Sqrt(tr)), 2);
            return omegaA * R * R * tc * tc / pc * alpha;
        }

        /// <summary>Calculates the repulsion parameter b for a single component (temperature-independent).</summary>
        public static double CalcB(double tc, double pc, EosType eos)
        {
            double omegaB = eos == EosType.PengRobinson ? PR_OmegaB : SRK_OmegaB;
            return omegaB * R * tc / pc;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Mixture combining rules  (van der Waals one-fluid)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates mixture a and b using van der Waals one-fluid mixing rules:
        ///   a_mix = ΣΣ yi·yj·√(ai·aj)·(1 − kij)
        ///   b_mix = Σ yi·bi
        ///
        /// Binary interaction parameters kij default to 0 when not supplied.
        /// </summary>
        /// <param name="y">Mole fractions (must sum to 1).</param>
        /// <param name="a">Per-component a values.</param>
        /// <param name="b">Per-component b values.</param>
        /// <param name="kij">
        ///   Binary interaction parameters [i,j].
        ///   Pass null to use kij = 0 for all pairs.
        /// </param>
        public static (double aMix, double bMix, double[] aij)
            MixingRules(double[] y, double[] a, double[] b, double[,]? kij = null)
        {
            int n = y.Length;
            double aMix = 0.0, bMix = 0.0;
            double[] aijFlat = new double[n];  // Σ_j yj*sqrt(ai*aj)*(1-kij) — the cross term

            for (int i = 0; i < n; i++)
            {
                bMix += y[i] * b[i];
                double sum = 0.0;
                for (int j = 0; j < n; j++)
                {
                    double kij_val = kij != null ? kij[i, j] : 0.0;
                    double aij_val = Math.Sqrt(a[i] * a[j]) * (1.0 - kij_val);
                    sum += y[j] * aij_val;
                    aMix += y[i] * y[j] * aij_val;
                }
                aijFlat[i] = sum;
            }

            return (aMix, bMix, aijFlat);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Cubic EOS solver
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Solves the cubic Z-factor equation for PR or SRK EOS.
        ///
        /// PR:  Z³ − (1−B)Z² + (A−3B²−2B)Z − (AB−B²−B³) = 0
        /// SRK: Z³ − Z²    + (A−B−B²)Z    − AB = 0
        ///
        /// Returns up to three real roots; physical roots are ≥ B+1/1000.
        /// </summary>
        public static ZFactorResult SolveZFactor(double aMix, double bMix, double pressure, double temperature, EosType eos)
        {
            double A = aMix * pressure / (R * R * temperature * temperature);
            double B = bMix * pressure / (R * temperature);

            double c2, c1, c0;
            if (eos == EosType.PengRobinson)
            {
                c2 = -(1.0 - B);
                c1 = A - 3.0 * B * B - 2.0 * B;
                c0 = -(A * B - B * B - B * B * B);
            }
            else // SRK
            {
                c2 = -1.0;
                c1 = A - B - B * B;
                c0 = -A * B;
            }

            var roots = SolveCubic(c2, c1, c0);

            double minZ = B + 1e-6;
            var realRoots = roots.Where(z => z > minZ).OrderBy(z => z).ToList();

            double zV = realRoots.Count > 0 ? realRoots.Last() : 1.0;
            double zL = realRoots.Count > 1 ? realRoots.First() : zV;

            return new ZFactorResult
            {
                ZVapour = zV,
                ZLiquid = zL,
                TwoPhase = realRoots.Count >= 2,
                Eos = eos
            };
        }

        /// <summary>Solves Z³ + c2·Z² + c1·Z + c0 = 0 via the analytical cubic formula.</summary>
        private static List<double> SolveCubic(double c2, double c1, double c0)
        {
            // Depress: let Z = t − c2/3
            double p = c1 - c2 * c2 / 3.0;
            double q = 2.0 * c2 * c2 * c2 / 27.0 - c2 * c1 / 3.0 + c0;
            double disc = q * q / 4.0 + p * p * p / 27.0;

            var results = new List<double>();
            double offset = -c2 / 3.0;

            if (disc > 1e-10) // One real root
            {
                double sqrtDisc = Math.Sqrt(disc);
                double u = CbrtSigned(-q / 2.0 + sqrtDisc);
                double v = CbrtSigned(-q / 2.0 - sqrtDisc);
                results.Add(u + v + offset);
            }
            else if (disc < -1e-10) // Three distinct real roots
            {
                double r = Math.Sqrt(-p * p * p / 27.0);
                double theta = Math.Acos(Math.Clamp(-q / (2.0 * r), -1.0, 1.0));
                double m = 2.0 * Math.Pow(r, 1.0 / 3.0);
                results.Add(m * Math.Cos(theta / 3.0) + offset);
                results.Add(m * Math.Cos((theta + 2.0 * Math.PI) / 3.0) + offset);
                results.Add(m * Math.Cos((theta + 4.0 * Math.PI) / 3.0) + offset);
            }
            else // Repeated roots
            {
                double u = CbrtSigned(-q / 2.0);
                results.Add(2.0 * u + offset);
                results.Add(-u + offset);
            }

            return results;
        }

        private static double CbrtSigned(double x) =>
            x >= 0 ? Math.Pow(x, 1.0 / 3.0) : -Math.Pow(-x, 1.0 / 3.0);

        // ─────────────────────────────────────────────────────────────────
        //  Fugacity coefficients
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates fugacity coefficients for all components in a mixture at a given phase composition.
        ///
        /// PR departure function (vapour root):
        ///   ln φ_i = (b_i/b_mix)(Z−1) − ln(Z−B) − A/(2√2·B) · [2Σ_j(y_j·a_ij)/a_mix − b_i/b_mix]
        ///            · ln[(Z+(1+√2)B) / (Z+(1−√2)B)]
        ///
        /// SRK departure function:
        ///   ln φ_i = (b_i/b_mix)(Z−1) − ln(Z−B) − (A/B) · [2Σ_j(y_j·a_ij)/a_mix − b_i/b_mix]
        ///            · ln(1 + B/Z)
        ///
        /// Reference: Michelsen and Mollerup (2007), pp. 54-58.
        /// </summary>
        /// <param name="y">Phase mole fractions.</param>
        /// <param name="a">Per-component a_i(T).</param>
        /// <param name="b">Per-component b_i.</param>
        /// <param name="pressure">Pressure (psia).</param>
        /// <param name="temperature">Temperature (°R).</param>
        /// <param name="eos">EOS type.</param>
        /// <param name="isVapour">If true, use the high-Z root; else use the low-Z root.</param>
        /// <param name="kij">Binary interaction parameters (optional).</param>
        public static FugacityResult CalculateFugacityCoefficients(
            double[] y,
            double[] a,
            double[] b,
            double pressure,
            double temperature,
            EosType eos,
            bool isVapour,
            double[,]? kij = null)
        {
            var (aMix, bMix, aijRow) = MixingRules(y, a, b, kij);
            var zResult = SolveZFactor(aMix, bMix, pressure, temperature, eos);
            double z = isVapour ? zResult.ZVapour : zResult.ZLiquid;

            double A = aMix * pressure / (R * R * temperature * temperature);
            double B = bMix * pressure / (R * temperature);

            int n = y.Length;
            double[] lnPhi = new double[n];
            double[] phi = new double[n];

            for (int i = 0; i < n; i++)
            {
                double biOverB = bMix > 1e-12 ? b[i] / bMix : 0.0;
                double aijTerm = aMix > 1e-12 ? 2.0 * aijRow[i] / aMix : 0.0;

                double lnPhiI;
                if (eos == EosType.PengRobinson)
                {
                    double sqrt2 = Math.Sqrt(2.0);
                    double num = z + (1.0 + sqrt2) * B;
                    double den = z + (1.0 - sqrt2) * B;
                    double logTerm = den > 1e-12 ? Math.Log(Math.Max(1e-12, num / den)) : 0.0;
                    lnPhiI = biOverB * (z - 1.0) - Math.Log(Math.Max(1e-12, z - B))
                             - A / (2.0 * sqrt2 * B) * (aijTerm - biOverB) * logTerm;
                }
                else // SRK
                {
                    double logTerm = B > 1e-12 ? Math.Log(Math.Max(1e-12, 1.0 + B / z)) : 0.0;
                    lnPhiI = biOverB * (z - 1.0) - Math.Log(Math.Max(1e-12, z - B))
                             - (A / B) * (aijTerm - biOverB) * logTerm;
                }

                lnPhi[i] = lnPhiI;
                phi[i] = Math.Exp(lnPhiI);
            }

            return new FugacityResult
            {
                LnFugacityCoefficients = lnPhi,
                FugacityCoefficients = phi,
                Z = z,
                IsVapour = isVapour
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  EOS-based K-values
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates K-values from EOS fugacity coefficients:
        ///   Ki = φ_L_i / φ_V_i
        ///
        /// Requires an initial phase composition estimate (e.g., from Wilson K-values).
        /// </summary>
        /// <param name="overallComposition">Feed mole fractions z_i.</param>
        /// <param name="vaporComposition">Initial vapour mole fractions y_i.</param>
        /// <param name="liquidComposition">Initial liquid mole fractions x_i.</param>
        /// <param name="components">Component properties (Tc, Pc, ω, MW).</param>
        /// <param name="pressure">Pressure (psia).</param>
        /// <param name="temperature">Temperature (°R).</param>
        /// <param name="eos">EOS type.</param>
        /// <param name="kij">Binary interaction parameters (optional).</param>
        public static EosKValueResult CalculateEosKValues(
            double[] overallComposition,
            double[] vaporComposition,
            double[] liquidComposition,
            List<FLASH_COMPONENT> components,
            double pressure,
            double temperature,
            EosType eos = EosType.PengRobinson,
            double[,]? kij = null)
        {
            int n = components.Count;
            double[] a = new double[n];
            double[] b = new double[n];

            for (int i = 0; i < n; i++)
            {
                double tc = (double)components[i].CRITICAL_TEMPERATURE;
                double pc = (double)components[i].CRITICAL_PRESSURE;
                double omega = (double)components[i].ACENTRIC_FACTOR;
                a[i] = CalcA(tc, pc, omega, temperature, eos);
                b[i] = CalcB(tc, pc, eos);
            }

            var vapourFugacity = CalculateFugacityCoefficients(
                vaporComposition, a, b, pressure, temperature, eos, isVapour: true, kij);
            var liquidFugacity = CalculateFugacityCoefficients(
                liquidComposition, a, b, pressure, temperature, eos, isVapour: false, kij);

            double[] kValues = new double[n];
            for (int i = 0; i < n; i++)
            {
                kValues[i] = liquidFugacity.FugacityCoefficients[i] /
                             Math.Max(1e-12, vapourFugacity.FugacityCoefficients[i]);
            }

            return new EosKValueResult
            {
                KValues = kValues,
                VapourPhase = vapourFugacity,
                LiquidPhase = liquidFugacity
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Built-in BIP table for common hydrocarbons
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns binary interaction parameters (kij) from the built-in table for common
        /// hydrocarbon-gas pairs (PR EOS default values, Knapp et al. 1982).
        ///
        /// Components must be identified by their <see cref="FLASH_COMPONENT.COMPONENT_NAME"/>.
        /// If a pair is not found, kij defaults to 0.
        /// </summary>
        public static double[,] GetBinaryInteractionParameters(List<FLASH_COMPONENT> components)
        {
            int n = components.Count;
            double[,] kij = new double[n, n];

            // Non-zero BIP table (symmetric): component name pairs → kij value
            // Source: Knapp et al. (1982), modified by Whitson and Brulé (2000).
            var table = new Dictionary<(string, string), double>(StringComparer.OrdinalIgnoreCase.GetHashCode())
            {
                [("N2",  "CH4")]  = 0.031,  [("CH4", "N2")]  = 0.031,
                [("N2",  "C2H6")] = 0.050,  [("C2H6","N2")]  = 0.050,
                [("N2",  "C3H8")] = 0.080,  [("C3H8","N2")]  = 0.080,
                [("N2",  "CO2")]  = -0.020, [("CO2", "N2")]  = -0.020,
                [("CO2", "CH4")]  = 0.092,  [("CH4", "CO2")] = 0.092,
                [("CO2", "C2H6")] = 0.130,  [("C2H6","CO2")] = 0.130,
                [("CO2", "C3H8")] = 0.135,  [("C3H8","CO2")] = 0.135,
                [("H2S", "CH4")]  = 0.070,  [("CH4", "H2S")] = 0.070,
                [("H2S", "C2H6")] = 0.085,  [("C2H6","H2S")] = 0.085,
                [("H2S", "CO2")]  = 0.099,  [("CO2", "H2S")] = 0.099,
            };

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) { kij[i, j] = 0.0; continue; }
                    var key = (components[i].COMPONENT_NAME ?? "", components[j].COMPONENT_NAME ?? "");
                    kij[i, j] = table.TryGetValue(key, out double val) ? val : 0.0;
                }
            }

            return kij;
        }
    }
}
