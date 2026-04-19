using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.ChokeAnalysis.Calculations
{
    /// <summary>
    /// Choke erosion prediction, bean-size sizing, and service-life estimation.
    ///
    /// Erosion models implemented:
    ///  1. API RP 14E (1991) — erosional velocity limit.
    ///  2. DNV RP O501 (simplified) — erosion rate from sand-production flux.
    ///  3. Empirical choke wear model — effective bean-diameter growth over time.
    ///
    /// Bean-size selection models:
    ///  1. Target-rate sizing — select smallest bean that achieves a target flow rate.
    ///  2. Pressure-drop constraint sizing — select largest bean that maintains
    ///     a minimum wellhead back-pressure.
    ///  3. Erosion-life sizing — select bean with a target service life.
    ///
    /// References:
    ///  - API RP 14E (1991) Recommended Practice for Design and Installation of Offshore
    ///    Production Platform Piping Systems.
    ///  - DNV RP O501 (2015) Erosive Wear in Piping Systems.
    ///  - Salama & Venkatesh (1983) OTC-4485 — Examination of API RP 14E Erosional
    ///    Velocity Recommendations.
    ///  - El-Sayed (1997) JPT — Choke performance and sizing.
    /// </summary>
    public static class ChokeErosionAndSizing
    {
        // ─────────────────────────────────────────────────────────────────
        //  Constants
        // ─────────────────────────────────────────────────────────────────

        /// <summary>API RP 14E empirical constant C for clean service (non-corrosive, sand-free).</summary>
        private const double C_API_Clean = 100.0;

        /// <summary>API RP 14E empirical constant C for corrosive or sand-containing service.</summary>
        private const double C_API_Corrosive = 125.0;

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Erosion assessment result for a choke at given flow conditions.</summary>
        public sealed class ErosionAssessmentResult
        {
            /// <summary>Choke bean diameter (in).</summary>
            public double DiameterIn { get; set; }

            /// <summary>Fluid velocity through the choke throat (ft/s).</summary>
            public double ChokeVelocityFtPerSec { get; set; }

            /// <summary>API RP 14E erosional velocity limit Ve (ft/s).</summary>
            public double ErosionalVelocityLimitFtPerSec { get; set; }

            /// <summary>Velocity ratio V/Ve.  >1 indicates erosion risk.</summary>
            public double VelocityRatio { get; set; }

            /// <summary>Erosion risk level.</summary>
            public ErosionRisk Risk { get; set; }

            /// <summary>DNV RP O501 erosion rate (mm/year) if sand data provided.</summary>
            public double? DnvErosionRateMmPerYear { get; set; }

            /// <summary>Estimated bean service life (years) based on allowable wear.</summary>
            public double? EstimatedServiceLifeYears { get; set; }

            /// <summary>Recommended safe flow rate (STB/d or Mscfd) at Ve.</summary>
            public double RecommendedMaxFlowRate { get; set; }

            /// <summary>Narrative description of the assessment.</summary>
            public string Assessment { get; set; } = string.Empty;
        }

        /// <summary>Result of a choke bean-size selection exercise.</summary>
        public sealed class BeanSizingResult
        {
            /// <summary>Target flow rate requested (STB/d or Mscfd).</summary>
            public double TargetFlowRate { get; set; }

            /// <summary>Selected bean size (64ths of an inch).</summary>
            public int SelectedBeanSize64ths { get; set; }

            /// <summary>Selected bean diameter (in).</summary>
            public double SelectedDiameterIn { get; set; }

            /// <summary>Predicted flow rate at the selected bean (STB/d or Mscfd).</summary>
            public double PredictedFlowRate { get; set; }

            /// <summary>Over/under sizing percentage relative to target.</summary>
            public double SizingErrorPercent { get; set; }

            /// <summary>Whether erosion limits were a binding constraint.</summary>
            public bool ErosionLimitBinding { get; set; }

            /// <summary>Velocity ratio V/Ve at the selected size.</summary>
            public double VelocityRatio { get; set; }

            /// <summary>All candidate bean sizes evaluated.</summary>
            public List<BeanCandidate> Candidates { get; set; } = new();
        }

        /// <summary>One candidate bean size evaluated during sizing.</summary>
        public sealed class BeanCandidate
        {
            public int BeanSize64ths { get; set; }
            public double DiameterIn { get; set; }
            public double FlowRate { get; set; }
            public double VelocityRatio { get; set; }
            public bool MeetsTarget { get; set; }
            public bool WithinErosionLimit { get; set; }
        }

        /// <summary>Erosion risk category.</summary>
        public enum ErosionRisk
        {
            /// <summary>V/Ve ≤ 0.7 — well within safe zone.</summary>
            Low,
            /// <summary>V/Ve 0.7–1.0 — acceptable but monitor.</summary>
            Moderate,
            /// <summary>V/Ve 1.0–1.3 — exceeds API RP 14E threshold.</summary>
            High,
            /// <summary>V/Ve > 1.3 — severe erosion expected.</summary>
            Severe
        }

        // ─────────────────────────────────────────────────────────────────
        //  Erosion assessment — API RP 14E + DNV RP O501
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Assesses choke erosion risk using the API RP 14E erosional velocity limit.
        ///
        /// API RP 14E:  Ve = C / √ρ_m
        ///  where:
        ///   Ve   = erosional velocity (ft/s)
        ///   C    = empirical constant (100 clean service, 125 corrosive or sand)
        ///   ρ_m  = mixture density at choke conditions (lb/ft³)
        ///
        /// Mixture density is calculated from the input oil/water/gas rates and
        /// their individual densities, assuming ideal volumetric mixing at upstream
        /// pressure conditions.
        ///
        /// If sand production data are supplied, the DNV RP O501 erosion rate is
        /// also estimated:
        ///   ER = A_F × (ρ_p/ρ_fluid) × V^n × (Q_sand / 86400)
        /// with A_F = 2×10⁻⁷, n = 2.6 (simplified DNV equation for steel elbows;
        /// choke is approximately equivalent in loss coefficient).
        ///
        /// Bean wear model:
        ///   ΔD/Δt [in/year] = k_wear × ER
        ///  → service life = (D_max_wear) / (ΔD/Δt)
        /// where k_wear = 0.04 (in/mm) and D_max_wear = 10% of nominal diameter.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="liquidRateBbld">Total liquid flow rate (STB/d).</param>
        /// <param name="gasRateMscfd">Gas flow rate (Mscfd).</param>
        /// <param name="upstreamPressurePsia">Upstream pressure (psia).</param>
        /// <param name="temperatureF">Temperature (°F).</param>
        /// <param name="oilDensityLbFt3">Oil density (lb/ft³); ~51 for 40 API.</param>
        /// <param name="waterDensityLbFt3">Water density (lb/ft³); ~62.4 fresh, ~64.3 seawater.</param>
        /// <param name="gasDensityLbFt3">Gas density at upstream conditions (lb/ft³); ~1.5 typical.</param>
        /// <param name="waterCutFraction">Water cut (fraction, 0–1).</param>
        /// <param name="corrosiveService">True if service is corrosive or sand-bearing (uses C=125).</param>
        /// <param name="sandProductionLbPerDay">Sand production rate (lb/d); null = no sand data.</param>
        public static ErosionAssessmentResult Assess(
            CHOKE_PROPERTIES choke,
            double liquidRateBbld,
            double gasRateMscfd,
            double upstreamPressurePsia,
            double temperatureF,
            double oilDensityLbFt3 = 51.0,
            double waterDensityLbFt3 = 64.3,
            double gasDensityLbFt3 = 1.5,
            double waterCutFraction = 0.0,
            bool corrosiveService = false,
            double? sandProductionLbPerDay = null)
        {
            if (choke == null) throw new ArgumentNullException(nameof(choke));

            double dIn = (double)choke.CHOKE_DIAMETER;
            if (dIn <= 0) throw new ArgumentOutOfRangeException(nameof(choke), "CHOKE_DIAMETER must be > 0");

            double areaFt2 = Math.PI * (dIn / 12.0) * (dIn / 12.0) / 4.0;

            // ── Mixture density (lb/ft³) ───────────────────────────────
            double qOilFt3d  = liquidRateBbld * (1.0 - waterCutFraction) * 5.61458;
            double qWaterFt3d = liquidRateBbld * waterCutFraction         * 5.61458;
            double qGasFt3d   = gasRateMscfd   * 1000.0 / (upstreamPressurePsia / 14.73) * (520.0 / (temperatureF + 459.67)); // actual ft³/d

            double totalQFt3d = qOilFt3d + qWaterFt3d + qGasFt3d;
            if (totalQFt3d <= 0) throw new ArgumentException("Total flow rate must be > 0.");

            double massOil   = qOilFt3d   * oilDensityLbFt3;
            double massWater = qWaterFt3d * waterDensityLbFt3;
            double massGas   = qGasFt3d   * gasDensityLbFt3;
            double totalMass = massOil + massWater + massGas;

            double rhoMix = totalMass / totalQFt3d;   // lb/ft³

            // ── Choke throat velocity ─────────────────────────────────
            double qFt3s = totalQFt3d / 86400.0;      // ft³/s
            double v = areaFt2 > 0 ? qFt3s / areaFt2 : 0.0;  // ft/s

            // ── API RP 14E erosional velocity ─────────────────────────
            double c  = corrosiveService ? C_API_Corrosive : C_API_Clean;
            double ve = rhoMix > 0 ? c / Math.Sqrt(rhoMix) : double.MaxValue;

            double vRatio = ve > 0 ? v / ve : 0;

            ErosionRisk risk = vRatio switch
            {
                <= 0.7 => ErosionRisk.Low,
                <= 1.0 => ErosionRisk.Moderate,
                <= 1.3 => ErosionRisk.High,
                _      => ErosionRisk.Severe
            };

            // ── Recommended max flow rate at Ve ───────────────────────
            double qMaxFt3s = ve * areaFt2;
            double qMaxFt3d = qMaxFt3s * 86400.0;
            // Convert to liquid-equivalent bbl/d
            double liquidFraction = totalQFt3d > 0 ? (qOilFt3d + qWaterFt3d) / totalQFt3d : 1.0;
            double qMaxLiqBbld = qMaxFt3d * liquidFraction / 5.61458;

            // ── DNV RP O501 erosion rate (optional) ───────────────────
            double? dnvEr = null;
            double? serviceLife = null;

            if (sandProductionLbPerDay.HasValue && sandProductionLbPerDay.Value > 0)
            {
                // Simplified DNV model (steel, choke plug/seat)
                double sandFluxKgS = sandProductionLbPerDay.Value * 0.453592 / 86400.0; // kg/s
                double af = 2e-7;
                double n  = 2.6;
                double erMmPerS = af * (2650.0 / rhoMix) * Math.Pow(v, n) * sandFluxKgS;
                dnvEr = erMmPerS * 1000.0 * 86400.0 * 365.25; // mm/year

                // Wear model: allow 10% diameter growth before replacement
                double dMaxWearIn = dIn * 0.10;
                double kWear = 0.04; // in per mm erosion depth
                double erInPerYear = dnvEr.Value * kWear;
                serviceLife = erInPerYear > 0 ? dMaxWearIn / erInPerYear : null;
            }

            // ── Assessment narrative ───────────────────────────────────
            string assessment = risk switch
            {
                ErosionRisk.Low      => $"Velocity {v:F1} ft/s is {(1 - vRatio) * 100:F0}% below erosional limit. No erosion concern.",
                ErosionRisk.Moderate => $"Velocity {v:F1} ft/s approaches erosional limit ({ve:F1} ft/s). Monitor choke wear.",
                ErosionRisk.High     => $"Velocity {v:F1} ft/s exceeds API RP 14E limit ({ve:F1} ft/s) by {(vRatio - 1) * 100:F0}%. Reduce rate or upsize choke.",
                _                   => $"Velocity {v:F1} ft/s is SEVERELY above limit ({ve:F1} ft/s). Immediate choke replacement risk."
            };

            return new ErosionAssessmentResult
            {
                DiameterIn                    = dIn,
                ChokeVelocityFtPerSec         = Math.Round(v, 2),
                ErosionalVelocityLimitFtPerSec= Math.Round(ve, 2),
                VelocityRatio                 = Math.Round(vRatio, 3),
                Risk                          = risk,
                DnvErosionRateMmPerYear       = dnvEr.HasValue ? Math.Round(dnvEr.Value, 3) : null,
                EstimatedServiceLifeYears     = serviceLife.HasValue ? Math.Round(serviceLife.Value, 1) : null,
                RecommendedMaxFlowRate        = Math.Round(qMaxLiqBbld, 0),
                Assessment                    = assessment
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Bean-size selection
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Selects the optimum standard API bean size to achieve a target liquid flow
        /// rate using the Gilbert (1954) multiphase choke correlation, subject to the
        /// API RP 14E erosional velocity constraint.
        ///
        /// Sizing logic:
        ///  1. For each standard bean size, predict flow rate via Gilbert.
        ///  2. Evaluate erosional velocity ratio V/Ve.
        ///  3. Select the smallest bean that meets the target rate AND has V/Ve ≤ 1.0.
        ///  4. If no bean satisfies both constraints, flag the nearest oversized bean
        ///     (prioritises erosion safety) or the smallest safe bean (conservative).
        /// </summary>
        /// <param name="targetLiquidRateBbld">Target liquid production rate (STB/d).</param>
        /// <param name="upstreamPressurePsia">Wellhead/upstream pressure (psia).</param>
        /// <param name="glrScfPerBbl">Gas-liquid ratio (scf/STB).</param>
        /// <param name="fluidDensityLbFt3">Wellstream mixture density at choke conditions (lb/ft³).</param>
        /// <param name="dischargeCd">Discharge coefficient (default 0.85).</param>
        /// <param name="corrosiveService">True = use C=125 in erosion check.</param>
        public static BeanSizingResult SelectBeanForTargetRate(
            double targetLiquidRateBbld,
            double upstreamPressurePsia,
            double glrScfPerBbl,
            double fluidDensityLbFt3 = 50.0,
            double dischargeCd = 0.85,
            bool corrosiveService = false)
        {
            double glr = Math.Max(glrScfPerBbl, 1.0);
            double c   = corrosiveService ? C_API_Corrosive : C_API_Clean;
            double ve  = fluidDensityLbFt3 > 0 ? c / Math.Sqrt(fluidDensityLbFt3) : double.MaxValue;

            var candidates = new List<BeanCandidate>();

            foreach (int bean in ChokePerformanceCurveCalculator.StandardBeanSizes64ths)
            {
                double dIn   = bean / 64.0;
                double s     = (double)bean;
                double aFt2  = Math.PI * (dIn / 12.0) * (dIn / 12.0) / 4.0;

                // Gilbert: q = P × S^1.89 / (435 × GLR^0.546)
                double q = upstreamPressurePsia * Math.Pow(s, 1.89) / (435.0 * Math.Pow(glr, 0.546));

                // Velocity at choke throat
                double qFt3s  = q * 5.61458 / 86400.0;  // assuming liquid dominant
                double v      = aFt2 > 0 ? qFt3s / aFt2 : 0;
                double vRatio = ve > 0 ? v / ve : 0;

                candidates.Add(new BeanCandidate
                {
                    BeanSize64ths     = bean,
                    DiameterIn        = dIn,
                    FlowRate          = Math.Round(q, 1),
                    VelocityRatio     = Math.Round(vRatio, 3),
                    MeetsTarget       = q >= targetLiquidRateBbld,
                    WithinErosionLimit= vRatio <= 1.0
                });
            }

            // Find smallest bean that meets target AND is within erosion limit
            var viable = candidates.Where(c2 => c2.MeetsTarget && c2.WithinErosionLimit)
                                   .OrderBy(c2 => c2.BeanSize64ths)
                                   .ToList();

            BeanCandidate? selected = viable.FirstOrDefault();
            bool erosionBinding = false;

            if (selected == null)
            {
                // Relax: smallest bean that meets target regardless of erosion
                var meetTarget = candidates.Where(c2 => c2.MeetsTarget).OrderBy(c2 => c2.BeanSize64ths).ToList();
                selected      = meetTarget.FirstOrDefault();
                erosionBinding = selected != null && selected.VelocityRatio > 1.0;

                // If even the largest bean can't meet target, return largest
                if (selected == null)
                    selected = candidates.OrderByDescending(c2 => c2.BeanSize64ths).First();
            }

            return new BeanSizingResult
            {
                TargetFlowRate       = targetLiquidRateBbld,
                SelectedBeanSize64ths= selected.BeanSize64ths,
                SelectedDiameterIn   = selected.DiameterIn,
                PredictedFlowRate    = selected.FlowRate,
                SizingErrorPercent   = targetLiquidRateBbld > 0
                    ? Math.Round((selected.FlowRate - targetLiquidRateBbld) / targetLiquidRateBbld * 100.0, 1)
                    : 0,
                ErosionLimitBinding  = erosionBinding,
                VelocityRatio        = selected.VelocityRatio,
                Candidates           = candidates
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Pressure-drop constraint sizing
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Selects the largest standard bean size that maintains at least
        /// <paramref name="minWellheadPressurePsia"/> at the given flow rate.
        ///
        /// Using Gilbert: P₁ = 435 × q × GLR^0.546 / S^1.89
        /// → S = [435 × q × GLR^0.546 / P₁_min]^(1/1.89)
        ///
        /// The largest standard bean with S ≤ S_max is selected.
        /// </summary>
        /// <param name="liquidRateBbld">Expected liquid flow rate (STB/d).</param>
        /// <param name="glrScfPerBbl">Gas-liquid ratio (scf/STB).</param>
        /// <param name="minWellheadPressurePsia">Minimum required wellhead pressure (psia).</param>
        public static BeanSizingResult SelectBeanForMinPressure(
            double liquidRateBbld,
            double glrScfPerBbl,
            double minWellheadPressurePsia)
        {
            double glr = Math.Max(glrScfPerBbl, 1.0);

            // Maximum allowable bean size (64ths) from Gilbert inversion
            double sMax = Math.Pow(435.0 * liquidRateBbld * Math.Pow(glr, 0.546) / minWellheadPressurePsia, 1.0 / 1.89);

            // Largest standard bean ≤ sMax
            var candidates = new List<BeanCandidate>();
            BeanCandidate? selected = null;

            foreach (int bean in ChokePerformanceCurveCalculator.StandardBeanSizes64ths)
            {
                double s = (double)bean;
                double q = minWellheadPressurePsia * Math.Pow(s, 1.89) / (435.0 * Math.Pow(glr, 0.546));

                candidates.Add(new BeanCandidate
                {
                    BeanSize64ths      = bean,
                    DiameterIn         = bean / 64.0,
                    FlowRate           = Math.Round(q, 1),
                    VelocityRatio      = 0,   // not computed in this mode
                    MeetsTarget        = s <= sMax,
                    WithinErosionLimit = true  // not evaluated in this mode
                });

                if (s <= sMax)
                    selected = candidates.Last();
            }

            selected ??= candidates.First(); // fallback to smallest

            return new BeanSizingResult
            {
                TargetFlowRate        = liquidRateBbld,
                SelectedBeanSize64ths = selected.BeanSize64ths,
                SelectedDiameterIn    = selected.DiameterIn,
                PredictedFlowRate     = selected.FlowRate,
                SizingErrorPercent    = liquidRateBbld > 0
                    ? Math.Round((selected.FlowRate - liquidRateBbld) / liquidRateBbld * 100.0, 1)
                    : 0,
                ErosionLimitBinding   = false,
                VelocityRatio         = 0,
                Candidates            = candidates
            };
        }
    }
}
