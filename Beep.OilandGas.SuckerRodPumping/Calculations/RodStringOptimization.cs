using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.SuckerRodPumping;

namespace Beep.OilandGas.SuckerRodPumping.Calculations
{
    /// <summary>
    /// Rod string design optimization for sucker rod pump installations.
    ///
    /// Optimization objectives:
    ///   1. Minimize peak polished rod load (PPRL) → smaller surface unit.
    ///   2. Stay within allowable Goodman stress for each rod section.
    ///   3. Minimize capital cost (rod material × length × unit cost).
    ///   4. Maintain a safe buoyancy-corrected string weight.
    ///
    /// The classic API RP 11L taper design distributes rod sections such that each
    /// section carries roughly the same percentage of tensile stress utilization.
    /// Equal-stress design yields:
    ///   A_top / A_bottom = (F_surface / F_pump_end)
    /// which is achieved by choosing a taper ratio R = (D₂/D₁)² where D₁ is the
    /// largest (top) diameter.
    ///
    /// API RP 11BR material grades and allowable rod stress (psi):
    ///   Grade C:   σ_allow ≈ 31,000 psi (0–70 °F); Su = 90,000 psi
    ///   Grade D:   σ_allow ≈ 40,000 psi;            Su = 115,000 psi
    ///   Grade HL:  σ_allow ≈ 47,000 psi;            Su = 140,000 psi
    ///   Grade HY:  σ_allow ≈ 54,000 psi;            Su = 160,000 psi
    ///
    /// Stress utilization (Goodman):
    ///   U = Sa/Se + Sm/Su ≤ 1.0
    ///   Sa = (σmax − σmin)/2,  Sm = (σmax + σmin)/2
    ///
    /// Cost model:
    ///   Cost = Σ (unit_cost[grade] × weight[section])
    ///   Typical unit costs (USD/lb): Grade C ≈ 0.50, D ≈ 0.60, HL ≈ 0.75, HY ≈ 0.90
    ///
    /// References:
    ///   - API RP 11L (2019) "Design Calculations for Sucker Rod Pumping Systems"
    ///   - API RP 11BR (2015) "Care and Handling of Sucker Rods"
    ///   - Nolen, K.B. and Gibbs, S.G. (1990) SPE-20685
    /// </summary>
    public static class RodStringOptimization
    {
        // ─────────────────────────────────────────────────────────────────
        //  Constants and material database
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Standard API sucker rod diameters (inches).</summary>
        public static readonly IReadOnlyList<double> StandardRodDiameters =
            new[] { 0.625, 0.750, 0.875, 1.000, 1.125 };

        /// <summary>Material grade properties (Su psi, Se psi, cost USD/lb).</summary>
        private static readonly IReadOnlyDictionary<string, RodMaterial> Materials =
            new Dictionary<string, RodMaterial>(StringComparer.OrdinalIgnoreCase)
            {
                ["C"]  = new RodMaterial("C",   90_000, 23_000, 0.50),
                ["D"]  = new RodMaterial("D",  115_000, 29_750, 0.60),
                ["HL"] = new RodMaterial("HL", 140_000, 35_000, 0.75),
                ["HY"] = new RodMaterial("HY", 160_000, 40_000, 0.90),
            };

        private const double SteelDensityLbPerFt3 = 490.0;
        private const double InchToFt = 1.0 / 12.0;

        // ─────────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Designs an optimized taper rod string using the equal-stress method.
        ///
        /// The algorithm:
        ///  1. Start with the requested number of taper sizes.
        ///  2. Assign sections bottom-to-top by equal stress-utilization.
        ///  3. Verify each section against the Goodman criterion.
        ///  4. Report total cost and safety margin per section.
        /// </summary>
        /// <param name="totalDepth">Total well depth (ft).</param>
        /// <param name="pumpLoadLbs">Net pump load = fluid load + pump friction (lbf).</param>
        /// <param name="strokesPerMinute">Pumping speed (spm).</param>
        /// <param name="strokeLengthInches">Surface stroke length (in).</param>
        /// <param name="fluidDensityLbFt3">Produced fluid density (lb/ft³). ~55 for typical crude.</param>
        /// <param name="numberOfTapers">Number of rod sections in the taper (1–4).</param>
        /// <param name="gradePreference">Preferred API grade ("C","D","HL","HY") or "AUTO".</param>
        /// <returns><see cref="RodStringOptimizationResult"/>.</returns>
        public static RodStringOptimizationResult OptimizeTaperDesign(
            double totalDepth,
            double pumpLoadLbs,
            double strokesPerMinute,
            double strokeLengthInches,
            double fluidDensityLbFt3 = 55.0,
            int numberOfTapers = 3,
            string gradePreference = "AUTO")
        {
            if (totalDepth <= 0) throw new ArgumentOutOfRangeException(nameof(totalDepth));
            if (pumpLoadLbs <= 0) throw new ArgumentOutOfRangeException(nameof(pumpLoadLbs));
            numberOfTapers = Math.Max(1, Math.Min(numberOfTapers, 4));

            // Select candidate diameters (smallest required up-to standard sizes)
            double minArea    = pumpLoadLbs / GetAllowableStress(gradePreference == "AUTO" ? "D" : gradePreference);
            double minDiameter = Math.Sqrt(4.0 * minArea / Math.PI);
            var candidateDiameters = StandardRodDiameters
                .Where(d => d >= minDiameter)
                .Take(numberOfTapers)
                .ToArray();

            if (candidateDiameters.Length == 0)
                candidateDiameters = new[] { StandardRodDiameters.Last() };

            string materialGrade = gradePreference == "AUTO"
                ? SelectOptimalGrade(pumpLoadLbs, totalDepth, candidateDiameters.First())
                : gradePreference;

            if (!Materials.TryGetValue(materialGrade, out var material))
                material = Materials["D"];

            // Distribute section lengths using equal-stress taper fractions
            // API taper fraction (percent of depth) for typical well depths:
            //   1-taper: 100%
            //   2-taper: 57% / 43%  (approx.; exact from charts)
            //   3-taper: 45% / 33% / 22%
            //   4-taper: 36% / 27% / 20% / 17%
            double[] taperFractions = numberOfTapers switch
            {
                1 => new[] { 1.00 },
                2 => new[] { 0.57, 0.43 },
                3 => new[] { 0.45, 0.33, 0.22 },
                _ => new[] { 0.36, 0.27, 0.20, 0.17 },
            };

            var sections   = new List<OptimizedRodSection>();
            double cumulativeLoad = pumpLoadLbs;  // bottommost section carries pump load

            for (int i = candidateDiameters.Length - 1; i >= 0; i--)
            {
                double dia    = candidateDiameters[i];
                double area   = Math.PI * dia * dia / 4.0;
                double frac   = i < taperFractions.Length ? taperFractions[i] : 0.10;
                double length = totalDepth * frac;

                // Rod weight in this section (air)
                double weightAir = area * length * SteelDensityLbPerFt3 * (dia < 1.0 ? 1.0 : 1.0);
                // Buoyancy correction
                double buoyancy   = 1.0 - fluidDensityLbFt3 / SteelDensityLbPerFt3;
                double weightFluid = weightAir * buoyancy;

                // Loads at top of this section
                double sigmaMax = (cumulativeLoad + weightFluid) / area;
                double sigmaMin = cumulativeLoad > weightFluid
                    ? (cumulativeLoad - weightFluid) / area
                    : 0;

                double Sa = (sigmaMax - sigmaMin) / 2.0;
                double Sm = (sigmaMax + sigmaMin) / 2.0;
                double utilization = Sa / material.EnduranceLimit + Sm / material.UltimateTensileStrength;

                sections.Insert(0, new OptimizedRodSection
                {
                    SectionIndex     = i + 1,
                    DiameterInches   = dia,
                    LengthFt         = length,
                    MaterialGrade    = materialGrade,
                    WeightInAirLbs   = weightAir,
                    WeightInFluidLbs = weightFluid,
                    MaxStressPsi     = sigmaMax,
                    MinStressPsi     = sigmaMin,
                    StressAmplitudePsi = Sa,
                    MeanStressPsi    = Sm,
                    GoodmanUtilization = utilization,
                    IsWithinAllowable  = utilization <= 1.0,
                    SectionCost        = weightAir * material.UnitCostPerLb,
                });

                cumulativeLoad += weightFluid;
            }

            double totalWeight = sections.Sum(s => s.WeightInAirLbs);
            double totalCost   = sections.Sum(s => s.SectionCost);
            double pprl        = cumulativeLoad;  // load at polished rod

            return new RodStringOptimizationResult
            {
                Sections                  = sections,
                TotalWeightInAirLbs       = totalWeight,
                TotalWeightInFluidLbs     = sections.Sum(s => s.WeightInFluidLbs),
                EstimatedPPRL             = pprl,
                TotalCapitalCost          = totalCost,
                MaterialGrade             = materialGrade,
                AllSectionsWithinAllowable = sections.All(s => s.IsWithinAllowable),
                OverallMinSafetyFactor    = sections.Min(s => s.GoodmanUtilization > 0
                                                ? 1.0 / s.GoodmanUtilization : double.MaxValue),
            };
        }

        /// <summary>
        /// Evaluates an existing rod string for stress/fatigue and cost, suggesting
        /// section-by-section upgrade candidates where utilization > 1.0.
        /// </summary>
        public static RodStringEvaluationResult EvaluateExistingString(
            SUCKER_ROD_STRING rodString,
            SUCKER_ROD_SYSTEM_PROPERTIES systemProps)
        {
            if (rodString == null) throw new ArgumentNullException(nameof(rodString));
            if (systemProps == null) throw new ArgumentNullException(nameof(systemProps));

            double oilDensity   = (141.5 / (131.5 + (double)systemProps.OIL_GRAVITY)) * 62.4;
            double fluidDensity = oilDensity * (1.0 - (double)systemProps.WATER_CUT)
                                + 62.4 * (double)systemProps.WATER_CUT;
            double buoyancy     = 1.0 - fluidDensity / SteelDensityLbPerFt3;

            var evaluations = new List<SectionEvaluation>();
            // Derive pump polished-rod load: F = ΔP × A_plunger  (psi × in² = lbf)
            double pumpDiaIn    = (double)systemProps.PUMP_DIAMETER;
            double pumpAreaIn2  = Math.PI * pumpDiaIn * pumpDiaIn / 4.0;
            double diffPressure = Math.Max(0, (double)systemProps.BOTTOM_HOLE_PRESSURE
                                             - (double)systemProps.WELLHEAD_PRESSURE);
            double cumulativeLoad = diffPressure * pumpAreaIn2; // lbf

            foreach (var sec in rodString.SECTIONS?.OrderBy(s => s.SECTION_ORDER) ?? Enumerable.Empty<ROD_SECTION>())
            {
                double dia    = (double)sec.DIAMETER;
                double length = (double)sec.LENGTH;
                double area   = Math.PI * dia * dia / 4.0;
                double wt     = (double)sec.WEIGHT > 0
                    ? (double)sec.WEIGHT
                    : area * length * SteelDensityLbPerFt3;
                double wtFluid = wt * buoyancy;

                double sigmaMax = area > 0 ? (cumulativeLoad + wtFluid) / area : 0;
                double sigmaMin = area > 0 && cumulativeLoad > wtFluid
                    ? (cumulativeLoad - wtFluid) / area : 0;

                double Sa = (sigmaMax - sigmaMin) / 2.0;
                double Sm = (sigmaMax + sigmaMin) / 2.0;

                // Assume Grade D if not specified
                var mat = Materials["D"];
                double utilization = Sa / mat.EnduranceLimit + Sm / mat.UltimateTensileStrength;

                string recommendation = utilization > 1.0
                    ? $"Upgrade to Grade HL/HY or increase diameter (utilization = {utilization:F2})"
                    : utilization > 0.85
                        ? $"Marginal (utilization = {utilization:F2}); consider upgrade for longer service life"
                        : "Acceptable";

                evaluations.Add(new SectionEvaluation
                {
                    SectionId         = sec.ROD_SECTION_ID,
                    DiameterInches    = dia,
                    LengthFt          = length,
                    GoodmanUtilization = utilization,
                    IsWithinAllowable  = utilization <= 1.0,
                    Recommendation     = recommendation,
                });

                cumulativeLoad += wtFluid;
            }

            return new RodStringEvaluationResult
            {
                SectionEvaluations    = evaluations,
                AllSectionsAcceptable = evaluations.All(e => e.IsWithinAllowable),
                CriticalSections      = evaluations.Where(e => !e.IsWithinAllowable).ToList(),
                EstimatedPPRL         = cumulativeLoad,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────

        private static double GetAllowableStress(string grade)
        {
            return Materials.TryGetValue(grade, out var m) ? m.EnduranceLimit : 29_750;
        }

        private static string SelectOptimalGrade(double pumpLoad, double depth, double topDiameter)
        {
            // Use lightest (cheapest) grade whose utilization < 0.80 at the top section
            double area = Math.PI * topDiameter * topDiameter / 4.0;
            double approxSigma = pumpLoad / area + depth * SteelDensityLbPerFt3 * area / area;
            foreach (var kv in Materials.OrderBy(m => m.Value.UnitCostPerLb))
            {
                if (approxSigma < kv.Value.EnduranceLimit * 0.80)
                    return kv.Key;
            }
            return "HY";
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result / helper types (sealed — not persisted to DB)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Full optimization result for a new taper-string design.</summary>
        public sealed class RodStringOptimizationResult
        {
            public List<OptimizedRodSection> Sections { get; set; } = new();
            public double TotalWeightInAirLbs  { get; set; }
            public double TotalWeightInFluidLbs { get; set; }
            public double EstimatedPPRL         { get; set; }
            public double TotalCapitalCost      { get; set; }
            public string MaterialGrade         { get; set; } = string.Empty;
            public bool   AllSectionsWithinAllowable { get; set; }
            public double OverallMinSafetyFactor     { get; set; }
        }

        /// <summary>One optimized taper section.</summary>
        public sealed class OptimizedRodSection
        {
            public int    SectionIndex       { get; set; }
            public double DiameterInches     { get; set; }
            public double LengthFt           { get; set; }
            public string MaterialGrade      { get; set; } = string.Empty;
            public double WeightInAirLbs     { get; set; }
            public double WeightInFluidLbs   { get; set; }
            public double MaxStressPsi        { get; set; }
            public double MinStressPsi        { get; set; }
            public double StressAmplitudePsi  { get; set; }
            public double MeanStressPsi       { get; set; }
            public double GoodmanUtilization  { get; set; }
            public bool   IsWithinAllowable   { get; set; }
            public double SectionCost         { get; set; }
        }

        /// <summary>Evaluation result for an existing rod string.</summary>
        public sealed class RodStringEvaluationResult
        {
            public List<SectionEvaluation> SectionEvaluations   { get; set; } = new();
            public bool   AllSectionsAcceptable { get; set; }
            public List<SectionEvaluation> CriticalSections      { get; set; } = new();
            public double EstimatedPPRL         { get; set; }
        }

        /// <summary>Per-section evaluation record.</summary>
        public sealed class SectionEvaluation
        {
            public string SectionId          { get; set; } = string.Empty;
            public double DiameterInches     { get; set; }
            public double LengthFt           { get; set; }
            public double GoodmanUtilization  { get; set; }
            public bool   IsWithinAllowable   { get; set; }
            public string Recommendation      { get; set; } = string.Empty;
        }

        /// <summary>Rod material grade data.</summary>
        private sealed class RodMaterial
        {
            public RodMaterial(string grade, double su, double se, double costPerLb)
            {
                Grade                  = grade;
                UltimateTensileStrength = su;
                EnduranceLimit         = se;
                UnitCostPerLb          = costPerLb;
            }
            public string Grade                   { get; }
            public double UltimateTensileStrength  { get; }
            public double EnduranceLimit           { get; }
            public double UnitCostPerLb            { get; }
        }
    }
}
