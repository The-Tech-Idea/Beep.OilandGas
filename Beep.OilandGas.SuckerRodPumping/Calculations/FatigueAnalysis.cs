using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.SuckerRodPumping;

namespace Beep.OilandGas.SuckerRodPumping.Calculations
{
    /// <summary>
    /// Fatigue life analysis for sucker rod strings using the modified Goodman diagram
    /// (API RP 11BR) and Miner's cumulative damage rule.
    ///
    /// The modified Goodman criterion for rod fatigue is:
    ///   Sa/Se + Sm/Su ≤ 1/SF
    ///   Sa = stress amplitude = (σmax − σmin) / 2
    ///   Sm = mean stress      = (σmax + σmin) / 2
    ///   Se = corrected endurance limit (psi), Su = ultimate tensile strength (psi)
    ///   SF = safety factor (typically 1.0 for factor calculation; margin = 1/LHS)
    ///
    /// API RP 11BR material grades and allowable stresses:
    ///   Grade C    Su ≈ 90,000 psi,  Se ≈ 23,000 psi  (K = 0.5625)
    ///   Grade D    Su ≈ 115,000 psi, Se ≈ 29,750 psi  (K = 0.5625)
    ///   Grade HL   Su ≈ 140,000 psi, Se ≈ 35,000 psi
    ///   Grade HY   Su ≈ 160,000 psi, Se ≈ 40,000 psi
    ///
    /// Basquin's law (S-N curve) for cycle-to-failure estimate:
    ///   N = (Se/Sa)^b  where b ≈ 8 for steel rods (API).
    ///
    /// References:
    ///   - API RP 11BR (2015) "Recommended Practice for Care and Handling of Sucker Rods"
    ///   - Dougherty, R.L. (1988) SPE-17726 "Fatigue Analysis of Sucker Rods"
    ///   - Langer (1937) modified Goodman diagram as adopted by API.
    /// </summary>
    public static class FatigueAnalysis
    {
        // ─────────────────────────────────────────────────────────────────
        //  Material database (API RP 11BR grades)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Rod material grade properties.</summary>
        public sealed class RodMaterial
        {
            public string Grade { get; }
            /// <summary>Ultimate tensile strength (psi).</summary>
            public double UltimateStrength { get; }
            /// <summary>Yield strength (psi).</summary>
            public double YieldStrength { get; }
            /// <summary>Corrected endurance limit at R = 0 (psi).</summary>
            public double EnduranceLimit { get; }
            /// <summary>Basquin exponent b for S-N curve.</summary>
            public double BasquinExponent { get; }

            public RodMaterial(string grade, double su, double sy, double se, double b = 8.0)
            {
                Grade = grade;
                UltimateStrength = su;
                YieldStrength = sy;
                EnduranceLimit = se;
                BasquinExponent = b;
            }
        }

        /// <summary>Built-in API RP 11BR material grades.</summary>
        public static readonly IReadOnlyDictionary<string, RodMaterial> Materials =
            new Dictionary<string, RodMaterial>(StringComparer.OrdinalIgnoreCase)
            {
                ["C"]  = new RodMaterial("C",   90_000, 60_000, 23_000),
                ["D"]  = new RodMaterial("D",  115_000, 80_000, 29_750),
                ["K"]  = new RodMaterial("K",  115_000, 80_000, 29_750),
                ["HL"] = new RodMaterial("HL", 140_000, 120_000, 35_000),
                ["HY"] = new RodMaterial("HY", 160_000, 140_000, 40_000),
                ["KD"] = new RodMaterial("KD", 150_000, 130_000, 37_500),
            };

        // ─────────────────────────────────────────────────────────────────
        //  Result type
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Per-section fatigue result (fills existing <see cref="RodFatigueAnalysis"/> model).</summary>
        public sealed class SectionFatigueResult
        {
            /// <summary>Section order (1 = topmost).</summary>
            public int SectionOrder { get; set; }

            /// <summary>Rod diameter (in).</summary>
            public double Diameter { get; set; }

            /// <summary>Maximum stress at this section (psi).</summary>
            public double MaxStress { get; set; }

            /// <summary>Minimum stress at this section (psi).</summary>
            public double MinStress { get; set; }

            /// <summary>Stress amplitude Sa = (σmax − σmin)/2 (psi).</summary>
            public double StressAmplitude { get; set; }

            /// <summary>Mean stress Sm = (σmax + σmin)/2 (psi).</summary>
            public double MeanStress { get; set; }

            /// <summary>Modified Goodman ratio (LHS); ≤1 = safe; >1 = fatigue failure predicted.</summary>
            public double GoodmanRatio { get; set; }

            /// <summary>Fatigue safety factor = 1/GoodmanRatio.</summary>
            public double SafetyFactor { get; set; }

            /// <summary>Predicted cycles to failure (Basquin S-N curve).</summary>
            public double CyclesToFailure { get; set; }

            /// <summary>Life consumed at the given cumulative cycles (%).</summary>
            public double LifeConsumedPercent { get; set; }

            /// <summary>Estimated remaining life (months) at current SPM and daily hours.</summary>
            public double RemainingLifeMonths { get; set; }

            /// <summary>Risk category: "Safe" / "Monitor" / "High Risk" / "Failure Predicted".</summary>
            public string RiskCategory { get; set; } = string.Empty;

            /// <summary>Recommended action.</summary>
            public string RecommendedAction { get; set; } = string.Empty;

            /// <summary>Filled <see cref="RodFatigueAnalysis"/> model for persistence.</summary>
            public RodFatigueAnalysis Model { get; set; } = new();
        }

        /// <summary>Full rod-string fatigue analysis result.</summary>
        public sealed class StringFatigueResult
        {
            /// <summary>Per-section results ordered top to bottom.</summary>
            public List<SectionFatigueResult> Sections { get; set; } = new();

            /// <summary>Critical (worst) section index.</summary>
            public int CriticalSectionOrder { get; set; }

            /// <summary>Minimum safety factor across all sections.</summary>
            public double MinSafetyFactor { get; set; }

            /// <summary>Maximum Goodman ratio across all sections.</summary>
            public double MaxGoodmanRatio { get; set; }

            /// <summary>Overall risk assessment for the string.</summary>
            public string OverallRisk { get; set; } = string.Empty;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Main analysis methods
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs a full fatigue life assessment on every section of the rod string.
        ///
        /// The stress at each section is calculated from the force envelope of the
        /// load result (<see cref="SUCKER_ROD_LOAD_RESULT"/>), using the polished rod
        /// as the maximum force and the minimum load as the minimum force.
        ///
        /// Stress at section i:
        ///   The load decreases from the polished rod down; at section i:
        ///   σmax_i = PPRL / A_i  (conservative: full PPRL transmitted to each section)
        ///   σmin_i = MPRL / A_i
        ///
        /// For a tapered string, the actual cross-section area A_i of each section is
        /// used directly.
        /// </summary>
        /// <param name="loadResult">Output of <see cref="SuckerRodLoadCalculator.CalculateLoads"/>.</param>
        /// <param name="rodString">Rod string definition.</param>
        /// <param name="materialGrade">API rod material grade (C, D, HL, HY, KD…).</param>
        /// <param name="systemProperties">System properties for SPM and operating hours.</param>
        /// <param name="cumulativeCycles">Cycles accumulated so far (0 if unknown).</param>
        /// <param name="dailyOperatingHours">Daily operating hours for remaining-life estimate.</param>
        public static StringFatigueResult AnalyzeString(
            SUCKER_ROD_LOAD_RESULT loadResult,
            SUCKER_ROD_STRING rodString,
            string materialGrade,
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            double cumulativeCycles = 0.0,
            double dailyOperatingHours = 20.0)
        {
            if (loadResult == null) throw new ArgumentNullException(nameof(loadResult));
            if (rodString?.SECTIONS == null) throw new ArgumentNullException(nameof(rodString));
            if (!Materials.TryGetValue(materialGrade ?? "D", out var mat))
                mat = Materials["D"];

            double pprl = (double)loadResult.PEAK_LOAD;
            double mprl = (double)loadResult.MINIMUM_LOAD;
            double spm  = (double)systemProperties.STROKES_PER_MINUTE;
            double cyclesPerDay = spm * 60.0 * dailyOperatingHours;

            var stringResult = new StringFatigueResult();
            double minSF = double.MaxValue;
            double maxGR = 0;
            int critSection = 1;

            int order = 1;
            foreach (var sec in rodString.SECTIONS.OrderBy(s => s.SECTION_ORDER ?? order))
            {
                double d = (double)sec.DIAMETER;
                double area = Math.PI * d * d / 4.0;  // in²
                if (area < 1e-10) { order++; continue; }

                double sigMax = pprl / area;
                double sigMin = mprl / area;

                var secResult = AnalyzeSection(
                    sigMax, sigMin, mat, cumulativeCycles, cyclesPerDay, sec.SECTION_ORDER ?? order);
                secResult.Diameter = d;

                stringResult.Sections.Add(secResult);
                if (secResult.SafetyFactor < minSF)
                {
                    minSF = secResult.SafetyFactor;
                    maxGR = secResult.GoodmanRatio;
                    critSection = secResult.SectionOrder;
                }
                order++;
            }

            stringResult.CriticalSectionOrder = critSection;
            stringResult.MinSafetyFactor = Math.Round(minSF == double.MaxValue ? 0 : minSF, 3);
            stringResult.MaxGoodmanRatio = Math.Round(maxGR, 3);
            stringResult.OverallRisk = ClassifyRisk(minSF);

            return stringResult;
        }

        /// <summary>
        /// Analyses a single rod section given its stress envelope.
        /// </summary>
        /// <param name="maxStressPsi">Maximum stress (psi) at this section.</param>
        /// <param name="minStressPsi">Minimum stress (psi) at this section.</param>
        /// <param name="material">Rod material properties.</param>
        /// <param name="cumulativeCycles">Cycles accumulated so far.</param>
        /// <param name="cyclesPerDay">Cycles per day at current SPM and hours.</param>
        /// <param name="sectionOrder">Section index (1 = topmost).</param>
        public static SectionFatigueResult AnalyzeSection(
            double maxStressPsi,
            double minStressPsi,
            RodMaterial material,
            double cumulativeCycles,
            double cyclesPerDay,
            int sectionOrder = 1)
        {
            double sa = (maxStressPsi - minStressPsi) / 2.0;
            double sm = (maxStressPsi + minStressPsi) / 2.0;

            // Modified Goodman: Sa/Se + Sm/Su ≤ 1
            double goodmanRatio = sa / material.EnduranceLimit + sm / material.UltimateStrength;
            double sf = goodmanRatio > 1e-12 ? 1.0 / goodmanRatio : 999.0;

            // Basquin S-N: N_f = (Se / Sa)^b
            double cyclesToFail = sa > 1e-6
                ? Math.Pow(material.EnduranceLimit / sa, material.BasquinExponent)
                : 1e18;

            // Miner's rule: life consumed
            double lifeConsumed = cyclesToFail > 1e-10 ? (cumulativeCycles / cyclesToFail) * 100.0 : 0.0;

            // Remaining life: (N_f − N_cum) / cycles_per_day / 30
            double remainingDays = cyclesPerDay > 0
                ? Math.Max(0, cyclesToFail - cumulativeCycles) / cyclesPerDay
                : 0;
            double remainingMonths = remainingDays / 30.0;

            string risk = ClassifyRisk(sf);
            string action = sf >= 1.5 ? "Continue operating — schedule routine inspection."
                          : sf >= 1.0 ? "Monitor closely — plan pull and inspection within 90 days."
                          : sf >= 0.7 ? "High risk — schedule immediate rod pull and inspection."
                                       : "Failure predicted — replace rod string immediately.";

            var model = new RodFatigueAnalysis
            {
                MaximumStress          = maxStressPsi,
                MinimumStress          = minStressPsi,
                MeanStress             = sm,
                StressRange            = maxStressPsi - minStressPsi,
                YieldStrength          = material.YieldStrength,
                FatigueStrength        = material.EnduranceLimit,
                StressConcentrationFactor = 1.0,
                FatigueMargin          = sf,
                CyclesToFailure        = cyclesToFail < 1e17 ? cyclesToFail : 0,
                CumulativeCycles       = cumulativeCycles,
                LifeConsumedPercent    = Math.Min(100, lifeConsumed),
                RemainingServiceLife   = remainingMonths > 9999 ? 9999 : remainingMonths,
                RiskAssessment         = risk,
                RecommendedAction      = action,
                AnalysisMethod         = "Modified Goodman (API RP 11BR) + Basquin S-N"
            };

            return new SectionFatigueResult
            {
                SectionOrder       = sectionOrder,
                MaxStress          = Math.Round(maxStressPsi, 1),
                MinStress          = Math.Round(minStressPsi, 1),
                StressAmplitude    = Math.Round(sa, 1),
                MeanStress         = Math.Round(sm, 1),
                GoodmanRatio       = Math.Round(goodmanRatio, 4),
                SafetyFactor       = Math.Round(sf, 3),
                CyclesToFailure    = cyclesToFail < 1e17 ? Math.Round(cyclesToFail, 0) : double.PositiveInfinity,
                LifeConsumedPercent= Math.Round(Math.Min(100, lifeConsumed), 2),
                RemainingLifeMonths= Math.Round(Math.Min(9999, remainingMonths), 1),
                RiskCategory       = risk,
                RecommendedAction  = action,
                Model              = model
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Sensitivity: optimal taper to equalise fatigue life across sections
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluates what percentage of each section's load is consumed by fatigue for
        /// multiple standard rod diameters, and recommends the optimal taper cut-off
        /// depths to equalise Goodman ratios across the string.
        ///
        /// This is a heuristic guide (not a rigorous optimiser) based on the principle
        /// that equal Goodman ratios minimise total weight while keeping all sections safe.
        ///
        /// Reference: Dougherty (1988) SPE-17726, Table 3.
        /// </summary>
        /// <param name="loadResult">Load analysis result.</param>
        /// <param name="totalDepthFt">Total pump setting depth (ft).</param>
        /// <param name="materialGrade">Rod material grade.</param>
        /// <param name="candidateDiametersIn">Candidate diameters to evaluate (e.g., [0.75, 0.875, 1.0]).</param>
        public static List<(double DiameterIn, double GoodmanRatio, double WeightLbPerFt, bool IsSafe)>
            EvaluateTaperOptions(
                SUCKER_ROD_LOAD_RESULT loadResult,
                double totalDepthFt,
                string materialGrade,
                double[] candidateDiametersIn)
        {
            if (!Materials.TryGetValue(materialGrade ?? "D", out var mat)) mat = Materials["D"];
            double steelDensityLbIn3 = 0.2833;

            double pprl = (double)loadResult.PEAK_LOAD;
            double mprl = (double)loadResult.MINIMUM_LOAD;

            var results = new List<(double, double, double, bool)>();

            foreach (double d in candidateDiametersIn)
            {
                double area = Math.PI * d * d / 4.0;
                double sigMax = pprl / area;
                double sigMin = mprl / area;
                double sa = (sigMax - sigMin) / 2.0;
                double sm = (sigMax + sigMin) / 2.0;
                double gr = sa / mat.EnduranceLimit + sm / mat.UltimateStrength;
                double wtPerFt = area * 12.0 * steelDensityLbIn3;
                results.Add((d, Math.Round(gr, 4), Math.Round(wtPerFt, 3), gr <= 1.0));
            }

            return results.OrderBy(r => r.Item2).ToList();
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────

        private static string ClassifyRisk(double safetyFactor) =>
            safetyFactor >= 1.5  ? "Safe"
          : safetyFactor >= 1.0  ? "Monitor"
          : safetyFactor >= 0.7  ? "High Risk"
                                  : "Failure Predicted";
    }
}
