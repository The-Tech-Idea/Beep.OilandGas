using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.ChokeAnalysis.Calculations
{
    /// <summary>
    /// Generates choke performance curves and IPR/choke intersection analysis.
    ///
    /// A choke performance curve (CPC) maps upstream or downstream pressure against
    /// flow rate for a fixed choke size and upstream conditions.  When superimposed on
    /// the well Inflow Performance Relationship (IPR), the intersection gives the
    /// natural flow rate for that choke size.
    ///
    /// Supported modes:
    ///  1. Gas CPC    — uses the critical/subcritical gas-flow equations from
    ///                  <see cref="GasChokeCalculator"/>.
    ///  2. Multiphase CPC — uses the Gilbert, Ros, Achong, and Pilehvari correlations
    ///                  from <see cref="MultiphaseChokeCalculator"/>.
    ///  3. Bean-size sweep — generates a CPC for every standard bean size at once
    ///                  (convenient for selecting the optimum choke size).
    ///  4. IPR-choke intersection — bisection search for the operating point.
    ///
    /// Standard API bean sizes (64ths of an inch):
    ///   8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40, 48, 56, 64
    ///
    /// References:
    ///  - Gilbert, W.E. (1954) API Drilling and Production Practice, pp 126-157.
    ///  - Ros, N.C.J. (1961) Revue de l'Institut Français du Pétrole, XVI(8), pp 1035-1061.
    ///  - Achong, I.B. (1961) Shell internal report.
    ///  - Pilehvari, A.A. (1987) SPE-16649.
    ///  - Vogel, J.V. (1968) JPT, pp 83-92 (IPR model).
    /// </summary>
    public static class ChokePerformanceCurveCalculator
    {
        // ─────────────────────────────────────────────────────────────────
        //  Standard bean sizes (64ths of an inch)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Standard API choke bean sizes (64ths of an inch).</summary>
        public static readonly int[] StandardBeanSizes64ths =
            { 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40, 48, 56, 64 };

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Single point on a choke performance curve.</summary>
        public sealed class CpcPoint
        {
            /// <summary>Upstream pressure (psia).</summary>
            public double UpstreamPressurePsia { get; set; }

            /// <summary>Downstream pressure (psia).</summary>
            public double DownstreamPressurePsia { get; set; }

            /// <summary>Flow rate (Mscfd for gas; STB/d for multiphase liquid).</summary>
            public double FlowRate { get; set; }

            /// <summary>Flow regime at this point.</summary>
            public string Regime { get; set; } = string.Empty;

            /// <summary>Pressure ratio P₂/P₁.</summary>
            public double PressureRatio { get; set; }
        }

        /// <summary>Full choke performance curve for one bean size.</summary>
        public sealed class ChokePerformanceCurveResult
        {
            /// <summary>Choke diameter (in).</summary>
            public double DiameterIn { get; set; }

            /// <summary>Bean size (64ths of an inch).</summary>
            public int BeanSize64ths { get; set; }

            /// <summary>Upstream pressure used (psia).</summary>
            public double UpstreamPressurePsia { get; set; }

            /// <summary>Critical (sonic) threshold pressure ratio.</summary>
            public double CriticalPressureRatio { get; set; }

            /// <summary>Maximum (sonic) flow rate at this choke size (Mscfd or STB/d).</summary>
            public double MaxFlowRate { get; set; }

            /// <summary>Ordered curve points from sonic to zero flow.</summary>
            public List<CpcPoint> Points { get; set; } = new();
        }

        /// <summary>IPR–choke intersection (natural operating point).</summary>
        public sealed class OperatingPoint
        {
            /// <summary>Choke size (64ths).</summary>
            public int BeanSize64ths { get; set; }

            /// <summary>Flow rate at the operating point (Mscfd or STB/d).</summary>
            public double FlowRate { get; set; }

            /// <summary>Flowing wellhead pressure at the operating point (psia).</summary>
            public double WellheadPressurePsia { get; set; }

            /// <summary>Flowing bottomhole pressure at the operating point (psia).</summary>
            public double FlowingBhpPsia { get; set; }

            /// <summary>Drawdown at the operating point (psia).</summary>
            public double DrawdownPsia { get; set; }

            /// <summary>Whether the choke is in sonic (critical) flow at this point.</summary>
            public bool IsSonicFlow { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Gas choke performance curve
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a gas choke performance curve by sweeping downstream pressure from
        /// 0 to upstream pressure at a fixed choke size and upstream conditions.
        ///
        /// For each downstream pressure:
        ///  • If P₂/P₁ ≤ r_crit → sonic (critical) flow; flow rate = constant = Q_sonic
        ///  • If P₂/P₁ > r_crit → subsonic flow; flow rate decreases toward zero at P₂ = P₁
        ///
        /// Critical pressure ratio: r_crit = [2/(k+1)]^(k/(k−1))
        /// Sonic flow rate (ISO 9300, simplified):
        ///   Q_sonic = C_d × A × P₁ × √(k / (Z × T₁)) × [2/(k+1)]^((k+1)/(2(k-1)))
        ///
        /// Subsonic flow rate (isentropic orifice, AGA-3 / ISO 5167 form):
        ///   Q_sub = C_d × A × √[2 × k / (k−1) × P₁ × ρ₁ × ((r)^(2/k) − (r)^((k+1)/k))]
        /// where r = P₂/P₁.
        /// </summary>
        /// <param name="choke">Choke properties (CHOKE_DIAMETER in inches, DISCHARGE_COEFFICIENT).</param>
        /// <param name="gasProps">Gas properties (UPSTREAM_PRESSURE, TEMPERATURE, GAS_SPECIFIC_GRAVITY).</param>
        /// <param name="steps">Number of curve points to generate.</param>
        /// <param name="k">Gas Cp/Cv (default 1.28 for natural gas).</param>
        /// <param name="zFactor">Gas compressibility Z (default 0.9).</param>
        public static ChokePerformanceCurveResult GenerateGasCurve(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProps,
            int steps = 40,
            double k = 1.28,
            double zFactor = 0.9)
        {
            if (choke == null)   throw new ArgumentNullException(nameof(choke));
            if (gasProps == null) throw new ArgumentNullException(nameof(gasProps));

            double dIn  = (double)choke.CHOKE_DIAMETER;          // in
            double cd   = (double)(choke.DISCHARGE_COEFFICIENT > 0 ? choke.DISCHARGE_COEFFICIENT : 0.85m);
            double p1   = (double)gasProps.UPSTREAM_PRESSURE;    // psia
            double tR   = (double)gasProps.TEMPERATURE + 459.67; // °R
            double sg   = (double)gasProps.GAS_SPECIFIC_GRAVITY;

            double area   = Math.PI * (dIn / 12.0) * (dIn / 12.0) / 4.0;  // ft²
            double rCrit  = Math.Pow(2.0 / (k + 1.0), k / (k - 1.0));

            // Gas density at upstream conditions (lb/ft³)
            double mwGas = sg * 28.97;
            double rho1  = mwGas * p1 / (zFactor * 10.7316 * tR);  // lb/ft³

            // Sonic flow rate (ft³/s actual) → convert to Mscfd
            double sonicTerm = Math.Pow(2.0 / (k + 1.0), (k + 1.0) / (2.0 * (k - 1.0)));
            double qSonicFt3s = cd * area * p1 * 144.0 * sonicTerm *
                                Math.Sqrt(k * 32.174 / (p1 * 144.0 * rho1));
            // Convert actual ft³/s → standard scfd → Mscfd
            double pFactor = p1 / 14.73 * (520.0 / tR) / zFactor;
            double qSonicMscfd = qSonicFt3s * pFactor * 86400.0 / 1000.0;

            int beanSize = (int)Math.Round(dIn * 64.0);

            var result = new ChokePerformanceCurveResult
            {
                DiameterIn            = dIn,
                BeanSize64ths         = beanSize,
                UpstreamPressurePsia  = p1,
                CriticalPressureRatio = Math.Round(rCrit, 4),
                MaxFlowRate           = Math.Round(qSonicMscfd, 2)
            };

            double stepSize = p1 / steps;
            for (int i = 0; i <= steps; i++)
            {
                double p2 = i * stepSize;
                double r  = p2 / p1;
                double q;
                string regime;

                if (r <= rCrit)
                {
                    q = qSonicMscfd;
                    regime = "Sonic";
                }
                else
                {
                    // Isentropic subsonic
                    double term1 = Math.Pow(r, 2.0 / k) - Math.Pow(r, (k + 1.0) / k);
                    double qFt3s = term1 > 0
                        ? cd * area * Math.Sqrt(2.0 * k / (k - 1.0) * p1 * 144.0 / rho1 * term1 * rho1 * rho1 / rho1)
                        : 0;
                    qFt3s = Math.Abs(qFt3s);
                    q = qFt3s * pFactor * 86400.0 / 1000.0;
                    regime = "Subsonic";
                }

                result.Points.Add(new CpcPoint
                {
                    UpstreamPressurePsia   = Math.Round(p1, 1),
                    DownstreamPressurePsia = Math.Round(p2, 1),
                    FlowRate      = Math.Round(q, 3),
                    Regime        = regime,
                    PressureRatio = Math.Round(r, 4)
                });
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Multiphase choke performance curve (Gilbert correlation)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a multiphase choke performance curve using the Gilbert (1954) correlation.
        ///
        /// Gilbert: P₁ = 435 × Q_L × (GLR)^0.546 / S^1.89
        ///  → Q_L = P₁ × S^1.89 / (435 × GLR^0.546)
        ///
        /// The curve is generated by sweeping liquid flow rate from 0 to the maximum
        /// that the given upstream pressure can support.
        ///
        /// For the bean-size sweep the upstream pressure is fixed (wellhead constraint).
        /// </summary>
        /// <param name="upstreamPressurePsia">Wellhead or upstream pressure (psia).</param>
        /// <param name="glrScfPerBbl">Gas-liquid ratio (scf/STB).</param>
        /// <param name="chokeDiameterIn">Choke bean diameter (in).</param>
        /// <param name="steps">Number of points on the curve.</param>
        public static ChokePerformanceCurveResult GenerateMultiphaseCurveGilbert(
            double upstreamPressurePsia,
            double glrScfPerBbl,
            double chokeDiameterIn,
            int steps = 40)
        {
            double s = chokeDiameterIn * 64.0;  // 64ths
            double glr = Math.Max(glrScfPerBbl, 1.0);

            // Max liquid rate at this upstream pressure (Gilbert)
            double qMax = upstreamPressurePsia * Math.Pow(s, 1.89) / (435.0 * Math.Pow(glr, 0.546));

            int beanSize = (int)Math.Round(chokeDiameterIn * 64.0);
            var result = new ChokePerformanceCurveResult
            {
                DiameterIn           = chokeDiameterIn,
                BeanSize64ths        = beanSize,
                UpstreamPressurePsia = upstreamPressurePsia,
                MaxFlowRate          = Math.Round(qMax, 1)
            };

            for (int i = 0; i <= steps; i++)
            {
                double qL = qMax * i / steps;
                // Back-calculate required upstream pressure at this flow rate
                double pRequired = qL > 0
                    ? 435.0 * qL * Math.Pow(glr, 0.546) / Math.Pow(s, 1.89)
                    : 0.0;

                result.Points.Add(new CpcPoint
                {
                    UpstreamPressurePsia   = Math.Round(pRequired, 1),
                    DownstreamPressurePsia = 0,        // not applicable for multiphase form
                    FlowRate    = Math.Round(qL, 1),
                    Regime      = "Multiphase (Gilbert)",
                    PressureRatio = 0
                });
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Bean-size sweep
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates multiphase choke performance curves for all standard API bean sizes
        /// at a fixed upstream pressure and GLR.  Each curve represents one bean size.
        ///
        /// The caller can overlay these on an IPR curve to identify the bean size that
        /// places the operating point at the desired flow rate.
        /// </summary>
        /// <param name="upstreamPressurePsia">Fixed upstream (wellhead) pressure (psia).</param>
        /// <param name="glrScfPerBbl">Gas-liquid ratio (scf/STB).</param>
        /// <param name="maxQlBbld">Upper flow-rate axis limit (STB/d) for the plot.</param>
        /// <param name="steps">Points per curve.</param>
        public static List<ChokePerformanceCurveResult> BeanSizeSweep(
            double upstreamPressurePsia,
            double glrScfPerBbl,
            double maxQlBbld = 5000.0,
            int steps = 30)
        {
            var curves = new List<ChokePerformanceCurveResult>();
            foreach (int b in StandardBeanSizes64ths)
            {
                double dIn = b / 64.0;
                var curve = GenerateMultiphaseCurveGilbert(upstreamPressurePsia, glrScfPerBbl, dIn, steps);
                // Clip to maxQlBbld for display purposes
                curve.Points = curve.Points.Where(p => p.FlowRate <= maxQlBbld).ToList();
                curves.Add(curve);
            }
            return curves;
        }

        // ─────────────────────────────────────────────────────────────────
        //  IPR–choke intersection (Vogel IPR)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Finds the well operating point where the choke performance curve intersects
        /// the Vogel (1968) IPR.
        ///
        /// Vogel IPR:
        ///   q/q_max = 1 − 0.2 × (Pwf/Pr) − 0.8 × (Pwf/Pr)²
        ///
        /// where q_max = AOF (absolute open flow), Pr = reservoir pressure.
        ///
        /// The choke relates wellhead pressure to flow rate via Gilbert:
        ///   Pwh = 435 × qL × GLR^0.546 / S^1.89
        ///
        /// The wellhead pressure Pwh is converted to BHP using a simple gradient:
        ///   Pwf = Pwh + gradient_psi_per_ft × depth_ft
        ///
        /// The bisection search finds qL such that:
        ///   Pwf_ipr(qL) = Pwf_choke(qL)
        /// </summary>
        /// <param name="reservoirPressurePsia">Static reservoir pressure Pr (psia).</param>
        /// <param name="aofBbld">Absolute open flow q_max (STB/d).</param>
        /// <param name="glrScfPerBbl">Gas-liquid ratio (scf/STB).</param>
        /// <param name="depthFt">Well depth — pump setting or perforations (ft).</param>
        /// <param name="wellboreGradientPsiPerFt">Average wellbore fluid gradient (psi/ft); ~0.35 for oil.</param>
        /// <param name="beanSizes64ths">Bean sizes to evaluate (null = all standard sizes).</param>
        /// <param name="tolerance">Flow-rate convergence tolerance (STB/d).</param>
        public static List<OperatingPoint> FindIprChokeIntersection(
            double reservoirPressurePsia,
            double aofBbld,
            double glrScfPerBbl,
            double depthFt,
            double wellboreGradientPsiPerFt = 0.35,
            int[]? beanSizes64ths = null,
            double tolerance = 1.0)
        {
            var points = new List<OperatingPoint>();
            var sizes = beanSizes64ths ?? StandardBeanSizes64ths;
            double glr = Math.Max(glrScfPerBbl, 1.0);

            foreach (int bean in sizes)
            {
                double s   = bean;           // 64ths
                double dIn = bean / 64.0;

                // Bisection: find qL in [0, aofBbld] where f(qL) = 0
                // f(qL) = Pwf_ipr(qL) − Pwf_choke(qL)
                double lo = 0.0, hi = aofBbld;
                double qStar = 0, pwfStar = 0;
                bool converged = false;

                for (int iter = 0; iter < 100; iter++)
                {
                    double mid = (lo + hi) / 2.0;

                    // IPR: Pwf from Vogel
                    double pwfIpr = VogelBhp(mid, reservoirPressurePsia, aofBbld);

                    // Choke: Pwh from Gilbert, then add gradient
                    double pwhChoke = mid > 0
                        ? 435.0 * mid * Math.Pow(glr, 0.546) / Math.Pow(s, 1.89)
                        : 0.0;
                    double pwfChoke = pwhChoke + wellboreGradientPsiPerFt * depthFt;

                    double f = pwfIpr - pwfChoke;

                    if (Math.Abs(hi - lo) < tolerance) { qStar = mid; pwfStar = pwfIpr; converged = true; break; }
                    if (f > 0) lo = mid; else hi = mid;
                }

                if (!converged) { qStar = (lo + hi) / 2.0; pwfStar = VogelBhp(qStar, reservoirPressurePsia, aofBbld); }

                double pwhOp    = qStar > 0 ? 435.0 * qStar * Math.Pow(glr, 0.546) / Math.Pow(s, 1.89) : 0;
                double rCrit    = Math.Pow(2.0 / (1.28 + 1.0), 1.28 / (1.28 - 1.0));  // gas critical ratio estimate
                bool sonic = pwhOp > 0 && (pwfStar / pwhOp) < rCrit;

                points.Add(new OperatingPoint
                {
                    BeanSize64ths        = bean,
                    FlowRate             = Math.Round(qStar, 1),
                    WellheadPressurePsia = Math.Round(pwhOp, 1),
                    FlowingBhpPsia       = Math.Round(pwfStar, 1),
                    DrawdownPsia         = Math.Round(reservoirPressurePsia - pwfStar, 1),
                    IsSonicFlow          = sonic
                });
            }

            return points.OrderBy(p => p.BeanSize64ths).ToList();
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helper: Vogel BHP
        // ─────────────────────────────────────────────────────────────────

        private static double VogelBhp(double q, double pr, double qMax)
        {
            // Solve: q/qMax = 1 − 0.2×x − 0.8×x²  for x = Pwf/Pr
            // 0.8x² + 0.2x + (q/qMax − 1) = 0
            if (qMax <= 0) return pr;
            double ratio = Math.Min(1.0, q / qMax);
            double a = 0.8, b = 0.2, c = ratio - 1.0;
            double disc = b * b - 4 * a * c;
            if (disc < 0) return 0;
            double x = (-b + Math.Sqrt(disc)) / (2 * a);
            x = Math.Max(0, Math.Min(1, x));
            return x * pr;
        }
    }
}
