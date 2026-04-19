using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.WellTestAnalysis.Constants;
using Beep.OilandGas.Models.Data.WellTestAnalysis;

namespace Beep.OilandGas.WellTestAnalysis.Calculations
{
    /// <summary>
    /// Provides a library of standard type curves for pressure-transient analysis
    /// (PTA) and automated type-curve matching.
    ///
    /// Supported models:
    /// <list type="bullet">
    ///   <item>Infinite-acting homogeneous reservoir (Ei-function / log approximation)</item>
    ///   <item>Finite closed-boundary circular reservoir (volumetric)</item>
    ///   <item>Constant pressure boundary (aquifer support)</item>
    ///   <item>Dual-porosity (Warren-Root pseudo-steady state interchange)</item>
    ///   <item>Hydraulic fracture — infinite conductivity (half-linear flow)</item>
    /// </list>
    ///
    /// All dimensionless variables follow the oilfield convention used in
    /// Earlougher (1977), Lee (1982) and Bourdet (2002):
    ///
    ///   tD = (0.0002637 * k * t) / (phi * mu * ct * rw²)
    ///   pD = (k * h * ΔP) / (141.2 * q * B * mu)
    ///
    /// Matching workflow:
    /// 1. Compute dimensionless observed data with <see cref="NormaliseObservedData"/>.
    /// 2. Call <see cref="MatchBestCurve"/> to rank all library curves by RMS misfit.
    /// 3. Use the best match to read off permeability, skin, and storage coefficient.
    /// </summary>
    public static class TypeCurveLibrary
    {
        // ─────────────────────────────────────────────────────────────────
        //  Dimensionless variable helpers
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts real-time (hr) to dimensionless time (tD).
        ///   tD = (0.0002637 · k · t) / (φ · μ · ct · rw²)
        /// </summary>
        public static double TimeToDimensionless(double t_hr, double k_md, double phi,
            double mu_cp, double ct_1psi, double rw_ft)
        {
            if (phi <= 0 || mu_cp <= 0 || ct_1psi <= 0 || rw_ft <= 0)
                throw new ArgumentException("Reservoir parameters must all be positive.");
            return 0.0002637 * k_md * t_hr / (phi * mu_cp * ct_1psi * rw_ft * rw_ft);
        }

        /// <summary>
        /// Converts measured pressure change (psi) to dimensionless pressure (pD).
        ///   pD = (k · h · ΔP) / (141.2 · q · B · μ)
        /// </summary>
        public static double PressureToDimensionless(double deltaP_psi, double k_md,
            double h_ft, double q_stbday, double B_resbbl, double mu_cp)
        {
            double denom = 141.2 * q_stbday * B_resbbl * mu_cp;
            if (Math.Abs(denom) < 1e-10)
                throw new ArgumentException("Flow rate, FVF and viscosity must all be non-zero.");
            return k_md * h_ft * deltaP_psi / denom;
        }

        /// <summary>
        /// Converts observed pressure-time data into dimensionless coordinates using
        /// formation and fluid properties.
        /// </summary>
        /// <param name="data">Observed well test data (drawdown or build-up).</param>
        /// <param name="k_md">Permeability guess (md) — use IARF result if available.</param>
        /// <returns>List of (tD, pD) pairs for type-curve matching.</returns>
        public static List<(double tD, double pD)> NormaliseObservedData(
            WELL_TEST_DATA data, double k_md)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            double phi = (double)(data.POROSITY ?? 0.2m);
            double mu = (double)(data.OIL_VISCOSITY ?? 1m);
            double ct = (double)(data.TOTAL_COMPRESSIBILITY ?? 1e-6m);
            double rw = (double)(data.WELLBORE_RADIUS ?? 0.25m);
            double h = (double)(data.FORMATION_THICKNESS == 0m ? 1m : data.FORMATION_THICKNESS);
            double q = (double)data.FLOW_RATE;
            double B = (double)(data.OIL_FORMATION_VOLUME_FACTOR == 0m ? 1m : data.OIL_FORMATION_VOLUME_FACTOR);
            double pi = (double)(data.INITIAL_RESERVOIR_PRESSURE > 0
                ? data.INITIAL_RESERVOIR_PRESSURE
                : (decimal)(data.Pressure.Any() ? data.Pressure.First() : 0));

            var result = new List<(double tD, double pD)>();
            int count = Math.Min(data.Time.Count, data.Pressure.Count);

            for (int i = 0; i < count; i++)
            {
                double t = data.Time[i];
                double p = data.Pressure[i];
                if (t <= 0) continue;

                double tD = TimeToDimensionless(t, k_md, phi, mu, ct, rw);
                double deltaP = pi - p;
                double pD = PressureToDimensionless(Math.Max(deltaP, 0), k_md, h, q, B, mu);
                result.Add((tD, pD));
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Type curve generators
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates dimensionless pressure values for an infinite-acting homogeneous
        /// reservoir (Ei-function or log approximation).
        ///
        ///   For tD &gt; 25 (log approximation):
        ///     pD ≈ 0.5 · [ln(tD) + 0.80907]
        ///
        ///   For tD ≤ 25 (Ei-function):
        ///     pD = -0.5 · Ei(-1 / (4·tD))    [where Ei is the exponential integral]
        ///
        /// Reference: Earlougher (1977) Monograph 5, Table C-1.
        /// </summary>
        /// <param name="tDValues">Dimensionless time values to evaluate at.</param>
        /// <returns>Corresponding dimensionless pressure values pD(tD).</returns>
        public static List<(double tD, double pD)> InfiniteActingHomogeneous(IEnumerable<double> tDValues)
        {
            var result = new List<(double tD, double pD)>();
            foreach (double tD in tDValues)
            {
                if (tD <= 0) continue;
                double pD = tD > 25.0
                    ? 0.5 * (Math.Log(tD) + 0.80907)
                    : -0.5 * ExponentialIntegral(-1.0 / (4.0 * tD));
                result.Add((tD, pD));
            }
            return result;
        }

        /// <summary>
        /// Generates the closed-boundary (finite volumetric) reservoir type curve.
        ///
        /// At early times: identical to infinite-acting (Ei-function).
        /// At late times (pseudo-steady state, tD &gt; 0.1·reD²):
        ///   pD ≈ 2π·tD/reD² + ln(reD) - 0.75
        ///
        /// Reference: Craft, Hawkins, Terry (1991), Ch. 10.
        /// </summary>
        /// <param name="tDValues">Dimensionless time values.</param>
        /// <param name="reD">Dimensionless drainage radius = re / rw (default 1000).</param>
        public static List<(double tD, double pD)> ClosedBoundary(
            IEnumerable<double> tDValues, double reD = 1000)
        {
            if (reD <= 1)
                throw new ArgumentException("reD must be > 1.", nameof(reD));

            var result = new List<(double tD, double pD)>();
            double pssBoundary = 0.1 * reD * reD;

            foreach (double tD in tDValues)
            {
                if (tD <= 0) continue;
                double pD;
                if (tD < pssBoundary)
                {
                    pD = tD > 25.0
                        ? 0.5 * (Math.Log(tD) + 0.80907)
                        : -0.5 * ExponentialIntegral(-1.0 / (4.0 * tD));
                }
                else
                {
                    // PSS approximation
                    pD = 2 * Math.PI * tD / (reD * reD) + Math.Log(reD) - 0.75;
                }
                result.Add((tD, pD));
            }
            return result;
        }

        /// <summary>
        /// Generates the constant-pressure boundary (steady-state aquifer) type curve.
        ///
        /// At late times the pressure stabilises, so pD approaches a finite limit.
        /// Approximated here as:
        ///   pD ≈ ln(reD) · (1 - exp(-tD / (0.5·reD²)))
        ///
        /// Reference: Hurst &amp; van Everdingen (1949).
        /// </summary>
        /// <param name="tDValues">Dimensionless time values.</param>
        /// <param name="reD">Dimensionless drainage radius = re / rw (default 1000).</param>
        public static List<(double tD, double pD)> ConstantPressureBoundary(
            IEnumerable<double> tDValues, double reD = 1000)
        {
            if (reD <= 1)
                throw new ArgumentException("reD must be > 1.", nameof(reD));

            var result = new List<(double tD, double pD)>();
            double pDSteadyState = Math.Log(reD);
            double tau = 0.5 * reD * reD;

            foreach (double tD in tDValues)
            {
                if (tD <= 0) continue;
                double pD = pDSteadyState * (1.0 - Math.Exp(-tD / tau));
                result.Add((tD, pD));
            }
            return result;
        }

        /// <summary>
        /// Generates the Warren-Root dual-porosity type curve (pseudo-steady state
        /// matrix-fracture interchange).
        ///
        /// In dual-porosity media the dimensionless pressure exhibits a characteristic
        /// "dip" in the Bourdet derivative. This is captured by superimposing the
        /// fracture-only response with a delayed matrix contribution:
        ///
        ///   pD ≈ pD_fracture(tD, ω) + pD_matrix(tD, λ, ω)
        ///
        /// where ω = storativity ratio (ω ≪ 1 for most naturally fractured reservoirs)
        /// and λ = interporosity flow parameter.
        ///
        /// Reference: Warren, J.E. and Root, P.J. (1963) SPEJ; Bourdet et al. (1984).
        /// </summary>
        /// <param name="tDValues">Dimensionless time values.</param>
        /// <param name="omega">Storativity ratio (fracture porosity / total porosity). Typical: 0.01–0.3.</param>
        /// <param name="lambda">Interporosity flow parameter. Typical: 1e-8 – 1e-4.</param>
        public static List<(double tD, double pD)> DualPorosity(
            IEnumerable<double> tDValues, double omega = 0.1, double lambda = 1e-6)
        {
            if (omega <= 0 || omega >= 1)
                throw new ArgumentException("Omega must be between 0 and 1.", nameof(omega));
            if (lambda <= 0)
                throw new ArgumentException("Lambda must be positive.", nameof(lambda));

            var result = new List<(double tD, double pD)>();

            foreach (double tD in tDValues)
            {
                if (tD <= 0) continue;

                // Fracture-only response at early time
                double pD_frac = tD > 25.0
                    ? 0.5 * (Math.Log(tD) + 0.80907)
                    : -0.5 * ExponentialIntegral(-1.0 / (4.0 * tD));

                // Matrix contribution — transition via storativity ratio
                // Approximation: at transition tD_t = omega*(1-omega)/lambda
                double tD_transition = omega * (1.0 - omega) / lambda;
                double transitionFactor = 1.0 / (1.0 + Math.Exp(-(Math.Log(tD + 1e-20) - Math.Log(tD_transition)) * 3));

                double pD_total = pD_frac * (omega + (1.0 - omega) * transitionFactor);
                result.Add((tD, pD_total));
            }
            return result;
        }

        /// <summary>
        /// Generates the infinite-conductivity hydraulic fracture type curve.
        ///
        /// Dimensionless fracture half-length xfD = xf / rw.
        /// At early times: linear flow → pD ∝ √tDxf (half-slope on log-log).
        /// At late times: converges to radial flow.
        ///
        ///   tDxf = tD / xfD²   (dimensionless time based on fracture half-length)
        ///   For √tDxf &lt; 0.01: pD ≈ π · √tDxf
        ///   For √tDxf &gt; 0.2:  pD ≈ (0.5 · ln(tDxf)) + ln(xfD) + 0.404
        ///
        /// Reference: Gringarten et al. (1974) SPEJ; Earlougher (1977).
        /// </summary>
        /// <param name="tDValues">Dimensionless time values.</param>
        /// <param name="xfD">Dimensionless fracture half-length (xf / rw). Default 100 (100-ft frac, 0.25-ft rw).</param>
        public static List<(double tD, double pD)> HydraulicFractureInfinite(
            IEnumerable<double> tDValues, double xfD = 100)
        {
            if (xfD <= 0)
                throw new ArgumentException("xfD must be positive.", nameof(xfD));

            var result = new List<(double tD, double pD)>();

            foreach (double tD in tDValues)
            {
                if (tD <= 0) continue;
                double tDxf = tD / (xfD * xfD);
                double sqrtTDxf = Math.Sqrt(tDxf);
                double pD;

                if (sqrtTDxf < 0.01)
                    pD = Math.PI * sqrtTDxf;  // pure linear flow
                else if (sqrtTDxf > 0.2)
                    pD = 0.5 * Math.Log(tDxf) + Math.Log(xfD) + 0.404;  // radial flow
                else
                {
                    // Transition: blend linear and radial
                    double pD_lin = Math.PI * sqrtTDxf;
                    double pD_rad = 0.5 * Math.Log(tDxf) + Math.Log(xfD) + 0.404;
                    double blend = (sqrtTDxf - 0.01) / (0.2 - 0.01);
                    pD = pD_lin * (1 - blend) + pD_rad * blend;
                }

                result.Add((tD, pD));
            }
            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Automated matching
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Ranks all built-in type curves against the observed (dimensionless) data and
        /// returns a list of <see cref="TypeCurveMatch"/> items ordered from best to worst
        /// match quality (lowest RMS misfit first).
        ///
        /// Matching uses the RMS of (log pD_observed − log pD_curve) at the tD values
        /// present in the observed data.
        /// </summary>
        /// <param name="observedDimensionless">
        ///   Dimensionless (tD, pD) pairs from <see cref="NormaliseObservedData"/>.
        /// </param>
        /// <param name="reD">Drainage radius for bounded-reservoir curves (re/rw).</param>
        /// <param name="omega">Dual-porosity storativity ratio.</param>
        /// <param name="lambda">Dual-porosity interporosity flow parameter.</param>
        /// <param name="xfD">Fracture dimensionless half-length for fracture curve.</param>
        /// <returns>Ordered list of type-curve match results, best match first.</returns>
        public static List<TypeCurveMatch> MatchBestCurve(
            List<(double tD, double pD)> observedDimensionless,
            double reD = 1000,
            double omega = 0.1,
            double lambda = 1e-6,
            double xfD = 100)
        {
            if (observedDimensionless == null || observedDimensionless.Count < 3)
                return new List<TypeCurveMatch>();

            var tDObs = observedDimensionless.Where(p => p.tD > 0 && p.pD > 0).ToList();
            if (tDObs.Count < 3)
                return new List<TypeCurveMatch>();

            var curves = new Dictionary<string, (ReservoirModel model, List<(double tD, double pD)> points)>
            {
                ["Infinite Acting Homogeneous"] = (ReservoirModel.InfiniteActing,
                    InfiniteActingHomogeneous(tDObs.Select(p => p.tD))),

                ["Closed Boundary (reD=" + reD.ToString("F0") + ")"] = (ReservoirModel.ClosedBoundary,
                    ClosedBoundary(tDObs.Select(p => p.tD), reD)),

                ["Constant Pressure Boundary (reD=" + reD.ToString("F0") + ")"] = (ReservoirModel.ConstantPressureBoundary,
                    ConstantPressureBoundary(tDObs.Select(p => p.tD), reD)),

                ["Dual Porosity (ω=" + omega + ", λ=" + lambda + ")"] = (ReservoirModel.DualPorosity,
                    DualPorosity(tDObs.Select(p => p.tD), omega, lambda)),

                ["Hydraulic Fracture (xfD=" + xfD.ToString("F0") + ")"] = (ReservoirModel.InfiniteActing,
                    HydraulicFractureInfinite(tDObs.Select(p => p.tD), xfD)),
            };

            var matches = new List<TypeCurveMatch>();

            foreach (var curve in curves)
            {
                double rms = ComputeRmsLogMisfit(tDObs, curve.Value.points);
                double matchQuality = Math.Max(0, 1.0 - rms / 2.0);  // 0–1 normalised quality

                string confidence = matchQuality >= 0.85 ? "High"
                    : matchQuality >= 0.70 ? "Medium"
                    : "Low";

                matches.Add(new TypeCurveMatch
                {
                    TypeCurveName = curve.Key,
                    ReservoirModel = curve.Value.model.ToString(),
                    MatchQuality = matchQuality,
                    ConfidenceLevel = confidence,
                    Notes = $"RMS log-misfit = {rms:F4}. Dimensionless match from {tDObs.Count} observed points."
                });
            }

            return matches.OrderByDescending(m => m.MatchQuality).ToList();
        }

        /// <summary>
        /// Performs automated type-curve matching and back-calculates formation properties
        /// from the best match. Returns the fully populated <see cref="TypeCurveMatchResult"/>.
        /// </summary>
        /// <param name="data">Raw well test data.</param>
        /// <param name="kGuess_md">
        ///   Initial permeability estimate (md) from a semi-log analysis, used to
        ///   normalise observed data before matching.
        /// </param>
        /// <param name="reD">Drainage radius for bounded curves.</param>
        /// <param name="omega">Dual-porosity storativity ratio.</param>
        /// <param name="lambda">Dual-porosity interporosity flow parameter.</param>
        /// <param name="xfD">Fracture dimensionless half-length.</param>
        /// <returns><see cref="TypeCurveMatchResult"/> for the best-matching type curve.</returns>
        public static TypeCurveMatchResult AutoMatch(
            WELL_TEST_DATA data,
            double kGuess_md,
            double reD = 1000,
            double omega = 0.1,
            double lambda = 1e-6,
            double xfD = 100)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var dimless = NormaliseObservedData(data, kGuess_md);
            var ranked = MatchBestCurve(dimless, reD, omega, lambda, xfD);

            var best = ranked.FirstOrDefault();
            if (best == null)
            {
                return new TypeCurveMatchResult
                {
                    MatchId = Guid.NewGuid().ToString(),
                    TypeCurveName = "No match found",
                    MatchQuality = 0,
                    ConfidenceIndicators = new List<string> { "Insufficient data" }
                };
            }

            // Back-calculate permeability from the match-point approach:
            // k = (141.2 * q * B * mu * pD_match) / (h * ΔP_match)   at a representative point
            double phi = (double)(data.POROSITY ?? 0.2m);
            double mu = (double)(data.OIL_VISCOSITY ?? 1m);
            double ct = (double)(data.TOTAL_COMPRESSIBILITY ?? 1e-6m);
            double rw = (double)(data.WELLBORE_RADIUS ?? 0.25m);
            double h = (double)(data.FORMATION_THICKNESS == 0m ? 1m : data.FORMATION_THICKNESS);
            double q = (double)data.FLOW_RATE;
            double B = (double)(data.OIL_FORMATION_VOLUME_FACTOR == 0m ? 1m : data.OIL_FORMATION_VOLUME_FACTOR);
            double pi = (double)(data.INITIAL_RESERVOIR_PRESSURE > 0
                ? data.INITIAL_RESERVOIR_PRESSURE
                : (decimal)(data.Pressure.Any() ? data.Pressure.First() : 0));

            // Use mid-point of observed data as match point
            var midDimless = dimless[dimless.Count / 2];
            double tMatch = data.Time[data.Time.Count / 2];
            double dpMatch = pi - data.Pressure[data.Pressure.Count / 2];

            double kMatch = kGuess_md;  // refined from match
            double sMatch = 0;

            if (midDimless.pD > 0 && dpMatch > 0 && midDimless.tD > 0)
            {
                // k from pD match: pD = kh*ΔP/(141.2*q*B*mu) → k = pD*141.2*q*B*mu/(h*ΔP)
                kMatch = midDimless.pD * 141.2 * q * B * mu / (h * dpMatch);
                kMatch = Math.Max(kMatch, 1e-6);

                // Skin from tD match: tD = 0.0002637*k*t/(phi*mu*ct*rw²) → already consistent
                // s from IARF approximation: pD_observed = 0.5*(ln(tD)+0.80907) + s  at IARF
                if (midDimless.tD > 25)
                {
                    double pD_IARF = 0.5 * (Math.Log(midDimless.tD) + 0.80907);
                    sMatch = midDimless.pD - pD_IARF;
                }
            }

            var confidenceIndicators = new List<string>();
            if (best.MatchQuality >= 0.85)
                confidenceIndicators.Add("Strong match — high confidence in reservoir model.");
            else if (best.MatchQuality >= 0.70)
                confidenceIndicators.Add("Moderate match — consider testing alternative models.");
            else
                confidenceIndicators.Add("Weak match — review data quality and model assumptions.");

            if (ranked.Count > 1 && ranked[1].MatchQuality >= 0.70)
                confidenceIndicators.Add($"Alternative: '{ranked[1].TypeCurveName}' quality = {ranked[1].MatchQuality:P0}.");

            return new TypeCurveMatchResult
            {
                MatchId = Guid.NewGuid().ToString(),
                TypeCurveName = best.TypeCurveName,
                ReservoirModel = Enum.TryParse<ReservoirModel>(
                    best.ReservoirModel, out var rm) ? rm : ReservoirModel.InfiniteActing,
                MatchQuality = best.MatchQuality ?? 0,
                Permeability = kMatch,
                SkinFactor = sMatch,
                InitialPressure = pi,
                ConfidenceIndicators = confidenceIndicators
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the Ei exponential integral using series/asymptotic approximation.
        /// Ei(-x) ≈ γ + ln(x) + Σ (x^n / (n·n!))   for small x;
        ///          ≈ (e^-x / x) · (1 - 1/x + 2/x² - ...)   for large x.
        /// </summary>
        private static double ExponentialIntegral(double x)
        {
            // Ei(-x) for x > 0 (x = |argument|)
            if (x >= 0) return double.NaN;  // Ei defined for negative argument

            double absX = -x;  // x is negative; absX = |x|

            if (absX < 1e-10) return double.PositiveInfinity;

            if (absX < 40)
            {
                // Series: Ei(-u) = γ + ln(u) + Σ_{n=1}^∞ (-u)^n / (n·n!)
                const double gamma = 0.5772156649015328;
                double sum = 0;
                double term = 1;
                for (int n = 1; n <= 100; n++)
                {
                    term *= -absX / n;
                    sum += term / n;
                    if (Math.Abs(term / n) < 1e-15 * Math.Abs(sum + 1)) break;
                }
                return gamma + Math.Log(absX) + sum;
            }
            else
            {
                // Asymptotic: e^(-u)/u * (1 - 1/u + 2/u^2 - 6/u^3 + ...)
                // Return negative of E1(u)
                double eu = Math.Exp(-absX) / absX;
                double series = 1;
                double t = 1;
                for (int n = 1; n <= 20; n++)
                {
                    t *= -n / absX;
                    series += t;
                    if (Math.Abs(t) < 1e-15) break;
                }
                return -eu * series;
            }
        }

        private static double ComputeRmsLogMisfit(
            List<(double tD, double pD)> observed,
            List<(double tD, double pD)> curve)
        {
            if (curve.Count == 0) return double.MaxValue;

            // Interpolate curve at observed tD values
            var sortedCurve = curve.OrderBy(c => c.tD).ToList();
            double sumSq = 0;
            int count = 0;

            foreach (var (tD, pD) in observed)
            {
                if (pD <= 0) continue;
                double curvePD = InterpolateLogLog(sortedCurve, tD);
                if (curvePD <= 0) continue;
                double logMisfit = Math.Log10(pD) - Math.Log10(curvePD);
                sumSq += logMisfit * logMisfit;
                count++;
            }

            return count > 0 ? Math.Sqrt(sumSq / count) : double.MaxValue;
        }

        private static double InterpolateLogLog(List<(double tD, double pD)> curve, double tD)
        {
            if (curve.Count == 0) return 0;
            if (tD <= curve[0].tD) return curve[0].pD;
            if (tD >= curve[^1].tD) return curve[^1].pD;

            for (int i = 1; i < curve.Count; i++)
            {
                if (curve[i].tD >= tD)
                {
                    double lnT1 = Math.Log(curve[i - 1].tD);
                    double lnT2 = Math.Log(curve[i].tD);
                    double lnP1 = Math.Log(Math.Max(curve[i - 1].pD, 1e-20));
                    double lnP2 = Math.Log(Math.Max(curve[i].pD, 1e-20));
                    double t = (Math.Log(tD) - lnT1) / (lnT2 - lnT1);
                    return Math.Exp(lnP1 + t * (lnP2 - lnP1));
                }
            }
            return curve[^1].pD;
        }
    }
}
