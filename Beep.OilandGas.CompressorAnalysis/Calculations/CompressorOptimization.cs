using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Calculations
{
    /// <summary>
    /// Compressor optimisation — compression ratio distribution, cost-benefit analysis,
    /// and compressor selection methodology.
    ///
    /// Covers:
    /// <list type="bullet">
    ///   <item>Optimal compression ratio distribution across stages (equal-work vs minimum-cost).</item>
    ///   <item>Operating cost vs. capital cost trade-off (life-cycle cost).</item>
    ///   <item>Compressor type selection guide (reciprocating vs centrifugal vs screw).</item>
    ///   <item>Sensitivity analysis: effect of number of stages on total power and capital cost.</item>
    /// </list>
    ///
    /// References:
    ///   - Campbell, J.M. (2004) "Gas Conditioning and Processing", Vol. 2, Ch. 13
    ///   - Arnold, K. and Stewart, M. (1999) "Surface Production Operations", Vol. 2, Ch. 11
    ///   - API 614 / API 617 (8th ed.) selection guidance
    ///   - Ludwig, E.E. (2001) "Applied Process Design", Vol. 3, Ch. 12
    /// </summary>
    public static class CompressorOptimization
    {
        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Result of an optimal compression ratio distribution calculation.
        /// </summary>
        public sealed class RatioDistributionResult
        {
            /// <summary>Optimal compression ratio for each stage (equal-work criterion).</summary>
            public List<decimal> StageRatios { get; set; } = new();

            /// <summary>Suction pressure at each stage inlet (psia).</summary>
            public List<decimal> StageSuctionPressures { get; set; } = new();

            /// <summary>Discharge pressure at each stage outlet (psia).</summary>
            public List<decimal> StageDischargePressures { get; set; } = new();

            /// <summary>BHP required for each stage.</summary>
            public List<decimal> StageBHP { get; set; } = new();

            /// <summary>Total BHP for the distribution.</summary>
            public decimal TotalBHP { get; set; }

            /// <summary>Maximum per-stage discharge temperature (°R).</summary>
            public decimal MaxDischargeTemperature { get; set; }

            /// <summary>Whether this distribution satisfies all operating constraints.</summary>
            public bool ConstraintsSatisfied { get; set; }

            /// <summary>Notes on constraint violations or recommendations.</summary>
            public string Notes { get; set; } = string.Empty;
        }

        /// <summary>
        /// Life-cycle cost assessment for a compressor configuration.
        /// </summary>
        public sealed class LifeCycleCostResult
        {
            /// <summary>Estimated capital cost (USD).</summary>
            public decimal CapitalCostUsd { get; set; }

            /// <summary>Annual operating cost — power only (USD/yr).</summary>
            public decimal AnnualPowerCostUsd { get; set; }

            /// <summary>Annual maintenance cost estimate (USD/yr).</summary>
            public decimal AnnualMaintenanceCostUsd { get; set; }

            /// <summary>Total annual operating cost (USD/yr).</summary>
            public decimal TotalAnnualCostUsd { get; set; }

            /// <summary>Life-cycle cost over <c>ProjectLifeYears</c> at <c>DiscountRate</c> (NPV basis, USD).</summary>
            public decimal LifeCycleCostNpvUsd { get; set; }

            /// <summary>Project life used for NPV (years).</summary>
            public int ProjectLifeYears { get; set; }

            /// <summary>Discount rate used for NPV (fraction).</summary>
            public decimal DiscountRate { get; set; }

            /// <summary>Number of compression stages in this option.</summary>
            public int NumberOfStages { get; set; }

            /// <summary>Compressor type label.</summary>
            public string CompressorType { get; set; } = string.Empty;
        }

        /// <summary>
        /// Compressor selection recommendation.
        /// </summary>
        public sealed class SelectionRecommendation
        {
            /// <summary>Recommended compressor type.</summary>
            public string RecommendedType { get; set; } = string.Empty;

            /// <summary>Primary reason for recommendation.</summary>
            public string PrimaryReason { get; set; } = string.Empty;

            /// <summary>Alternative type to consider.</summary>
            public string AlternativeType { get; set; } = string.Empty;

            /// <summary>Confidence in recommendation (High / Medium / Low).</summary>
            public string Confidence { get; set; } = string.Empty;

            /// <summary>List of supporting rationale bullets.</summary>
            public List<string> Rationale { get; set; } = new();

            /// <summary>Constraints that influenced the selection.</summary>
            public List<string> ConstraintsConsidered { get; set; } = new();
        }

        // ─────────────────────────────────────────────────────────────────
        //  Optimal compression ratio distribution
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the optimal (equal-work) compression ratio distribution across stages.
        ///
        /// For minimum total power with equal efficiencies and perfect intercooling, each
        /// stage does equal work, which corresponds to an equal compression ratio:
        ///   r_k = (P_discharge / P_suction)^(1 / N)
        ///
        /// For unequal efficiencies or partial intercooling, the equal-power criterion
        /// yields slightly different ratios. This method also supports a custom ratio
        /// array for evaluating non-uniform distributions.
        ///
        /// Reference: Campbell (2004), Vol. 2, p. 13-9; Arnold &amp; Stewart (1999), p. 11-42.
        /// </summary>
        /// <param name="compressorProperties">Compressor operating conditions and efficiency.</param>
        /// <param name="numberOfStages">Number of stages.</param>
        /// <param name="customRatios">
        ///   Optional custom per-stage ratios. If null, equal ratios are used.
        ///   Array must have <paramref name="numberOfStages"/> elements and their product
        ///   must equal the overall compression ratio.
        /// </param>
        /// <returns><see cref="RatioDistributionResult"/> with stage-by-stage pressures and power.</returns>
        public static RatioDistributionResult OptimiseRatioDistribution(
            CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
            int numberOfStages,
            decimal[]? customRatios = null)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));
            if (compressorProperties.OPERATING_CONDITIONS == null)
                throw new ArgumentNullException(nameof(compressorProperties.OPERATING_CONDITIONS));
            if (numberOfStages < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfStages));

            var conditions = compressorProperties.OPERATING_CONDITIONS;
            decimal overallRatio = conditions.DISCHARGE_PRESSURE / conditions.SUCTION_PRESSURE;
            decimal k = compressorProperties.SPECIFIC_HEAT_RATIO;
            decimal etaP = compressorProperties.POLYTROPIC_EFFICIENCY;
            decimal mw = conditions.GAS_SPECIFIC_GRAVITY * 28.9645m;
            decimal R = 1545m;
            decimal qScfMin = conditions.GAS_FLOW_RATE * 1000m / 1440m;
            decimal weightFlow = qScfMin * mw / 379m;

            // Use equal ratios if no custom array given
            decimal[] ratios;
            if (customRatios != null)
            {
                if (customRatios.Length != numberOfStages)
                    throw new ArgumentException("customRatios length must match numberOfStages.");
                ratios = customRatios;
            }
            else
            {
                decimal r = (decimal)Math.Pow((double)overallRatio, 1.0 / numberOfStages);
                ratios = new decimal[numberOfStages];
                for (int i = 0; i < numberOfStages; i++) ratios[i] = r;
            }

            var result = new RatioDistributionResult();
            decimal pIn = conditions.SUCTION_PRESSURE;
            decimal tIn = conditions.SUCTION_TEMPERATURE;
            decimal maxTemp = 0m;
            decimal totalBHP = 0m;

            for (int s = 0; s < numberOfStages; s++)
            {
                decimal r = ratios[s];
                decimal pOut = pIn * r;
                decimal pAvg = (pIn + pOut) / 2m;
                decimal z = ZFactorCalculator.CalculateBrillBeggs(pAvg, tIn, conditions.GAS_SPECIFIC_GRAVITY);

                decimal n = etaP > 0.999m ? k : (k * etaP) / (k - etaP * (k - 1m));
                decimal tOut = tIn * (decimal)Math.Pow((double)r, (double)((n - 1m) / n));
                decimal head = (z * R * tIn / mw) * (n / (n - 1m)) *
                               ((decimal)Math.Pow((double)r, (double)((n - 1m) / n)) - 1m);
                head = Math.Max(0m, head);
                decimal bhp = (weightFlow * head) / 33000m / etaP;
                bhp = Math.Max(0m, bhp);

                result.StageRatios.Add(Math.Round(r, 4));
                result.StageSuctionPressures.Add(Math.Round(pIn, 2));
                result.StageDischargePressures.Add(Math.Round(pOut, 2));
                result.StageBHP.Add(Math.Round(bhp, 1));

                totalBHP += bhp;
                if (tOut > maxTemp) maxTemp = tOut;

                // Next stage: assume perfect intercooling back to suction temp
                pIn = pOut;
                tIn = conditions.SUCTION_TEMPERATURE + 20m;  // 20 °R approach temperature
            }

            result.TotalBHP = Math.Round(totalBHP, 1);
            result.MaxDischargeTemperature = Math.Round(maxTemp, 1);

            // Check constraints
            bool ratioOk = ratios.All(r => r <= 4.0m);
            bool tempOk = maxTemp <= 660m;  // 200 °F = 660 °R
            result.ConstraintsSatisfied = ratioOk && tempOk;
            result.Notes = result.ConstraintsSatisfied
                ? "All stage constraints satisfied."
                : (!ratioOk ? "Warning: per-stage ratio exceeds 4.0. Consider adding a stage. " : "") +
                  (!tempOk ? $"Warning: max discharge temperature {maxTemp:F0} °R exceeds 660 °R (200 °F)." : "");

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Life-cycle cost analysis
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates the life-cycle cost (LCC) for a multistage compressor option.
        ///
        /// Capital cost is estimated from the Brown-root correlation for field gas compressors:
        ///   CAPEX ≈ 1500 · BHP^0.82 · F_stages   (USD, 2024 basis)
        ///   F_stages = 1 + 0.15 · (N - 1)  (intercooler, piping, foundation surcharge)
        ///
        /// Annual power cost = BHP · 0.746 · kWh_price · operating_hours
        ///
        /// NPV of OPEX over project life:
        ///   NPV_OPEX = Annual_cost · (1 - (1+r)^-n) / r
        ///
        /// Reference: Arnold &amp; Stewart (1999), p. 11-67; Ludwig (2001), Ch. 12.
        /// </summary>
        /// <param name="compressorProperties">Compressor properties and operating conditions.</param>
        /// <param name="numberOfStages">Number of compression stages.</param>
        /// <param name="compressorType">Label: "Centrifugal", "Reciprocating", "Screw".</param>
        /// <param name="electricityPriceKwh">Power cost in USD/kWh (default 0.08).</param>
        /// <param name="operatingHoursPerYear">Annual operating hours (default 8000).</param>
        /// <param name="projectLifeYears">Project life for NPV (default 20).</param>
        /// <param name="discountRate">Discount rate for NPV (default 0.10 = 10 %).</param>
        /// <returns><see cref="LifeCycleCostResult"/> with cost breakdown.</returns>
        public static LifeCycleCostResult CalculateLifeCycleCost(
            CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
            int numberOfStages,
            string compressorType = "Centrifugal",
            decimal electricityPriceKwh = 0.08m,
            decimal operatingHoursPerYear = 8000m,
            int projectLifeYears = 20,
            decimal discountRate = 0.10m)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));

            var msResult = MultistageCompressor.AnalyzeMultistage(compressorProperties, numberOfStages);
            decimal totalBHP = msResult.TotalMotorHorsepower;

            // Capital cost estimate
            decimal fStages = 1m + 0.15m * (numberOfStages - 1m);
            decimal capex = 1500m * (decimal)Math.Pow((double)totalBHP, 0.82) * fStages;

            // Reciprocating compressors are about 1.3× more expensive per HP than centrifugal
            if (compressorType.Equals("Reciprocating", StringComparison.OrdinalIgnoreCase))
                capex *= 1.3m;
            else if (compressorType.Equals("Screw", StringComparison.OrdinalIgnoreCase))
                capex *= 1.1m;

            // Annual power cost
            decimal annualPowerCost = totalBHP * 0.746m * electricityPriceKwh * operatingHoursPerYear;

            // Annual maintenance: ~2 % of CAPEX for centrifugal; ~3.5 % for reciprocating
            decimal maintFraction = compressorType.Equals("Reciprocating", StringComparison.OrdinalIgnoreCase)
                ? 0.035m : 0.020m;
            decimal annualMaint = capex * maintFraction;
            decimal annualTotal = annualPowerCost + annualMaint;

            // NPV of OPEX
            decimal npvOpex = discountRate > 0
                ? annualTotal * (1m - (decimal)Math.Pow((double)(1m + discountRate), -projectLifeYears)) / discountRate
                : annualTotal * projectLifeYears;

            return new LifeCycleCostResult
            {
                CapitalCostUsd = Math.Round(capex, 0),
                AnnualPowerCostUsd = Math.Round(annualPowerCost, 0),
                AnnualMaintenanceCostUsd = Math.Round(annualMaint, 0),
                TotalAnnualCostUsd = Math.Round(annualTotal, 0),
                LifeCycleCostNpvUsd = Math.Round(capex + npvOpex, 0),
                ProjectLifeYears = projectLifeYears,
                DiscountRate = discountRate,
                NumberOfStages = numberOfStages,
                CompressorType = compressorType
            };
        }

        /// <summary>
        /// Compares multiple stage-count options and returns the one with the lowest
        /// life-cycle cost, along with a sensitivity table for all options evaluated.
        /// </summary>
        /// <param name="compressorProperties">Compressor properties.</param>
        /// <param name="maxStages">Maximum stages to evaluate (default 6).</param>
        /// <param name="electricityPriceKwh">Power cost (USD/kWh).</param>
        /// <param name="projectLifeYears">Project life for NPV (years).</param>
        /// <param name="discountRate">Discount rate.</param>
        /// <returns>
        /// Tuple: (BestOption, AllOptions ordered by life-cycle cost ascending).
        /// </returns>
        public static (LifeCycleCostResult BestOption, List<LifeCycleCostResult> AllOptions)
            OptimiseStageCountByCost(
                CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
                int maxStages = 6,
                decimal electricityPriceKwh = 0.08m,
                int projectLifeYears = 20,
                decimal discountRate = 0.10m)
        {
            var options = new List<LifeCycleCostResult>();
            for (int n = 1; n <= maxStages; n++)
            {
                try
                {
                    var lcc = CalculateLifeCycleCost(
                        compressorProperties, n, "Centrifugal",
                        electricityPriceKwh, 8000m, projectLifeYears, discountRate);
                    options.Add(lcc);
                }
                catch
                {
                    // Skip infeasible configurations
                }
            }

            var sorted = options.OrderBy(o => o.LifeCycleCostNpvUsd).ToList();
            return (sorted.First(), sorted);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Compressor type selection
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Recommends a compressor type based on flow rate, compression ratio, and gas properties.
        ///
        /// Selection logic is based on industry rules of thumb:
        /// <list type="table">
        ///   <listheader><term>Scenario</term><description>Recommended Type</description></listheader>
        ///   <item><term>Q &lt; 500 Mscf/day or r &gt; 10</term><description>Reciprocating</description></item>
        ///   <item><term>500–5000 Mscf/day, r &lt; 6</term><description>Centrifugal</description></item>
        ///   <item><term>Sour gas (H₂S &gt; 5 %)</term><description>Centrifugal (no valves)</description></item>
        ///   <item><term>Variable speed needed</term><description>Reciprocating or Variable-speed Centrifugal</description></item>
        ///   <item><term>Q &lt; 200 Mscf/day small field</term><description>Rotary Screw</description></item>
        /// </list>
        ///
        /// Reference: API 617 Annex F; Arnold &amp; Stewart (1999), Table 11-3.
        /// </summary>
        /// <param name="flowRateMscfDay">Gas flow rate (Mscf/day).</param>
        /// <param name="overallCompressionRatio">Overall P_discharge / P_suction.</param>
        /// <param name="isSourGas">Whether gas contains significant H₂S (&gt;5 mol%).</param>
        /// <param name="variableFlowRequired">Whether the application requires variable-speed or variable-flow operation.</param>
        /// <param name="requiresContinuousOperation">Whether 100 % availability is required (no planned stops).</param>
        /// <returns><see cref="SelectionRecommendation"/> with recommended type and rationale.</returns>
        public static SelectionRecommendation SelectCompressorType(
            decimal flowRateMscfDay,
            decimal overallCompressionRatio,
            bool isSourGas = false,
            bool variableFlowRequired = false,
            bool requiresContinuousOperation = false)
        {
            var rec = new SelectionRecommendation();
            rec.ConstraintsConsidered.Add($"Flow rate: {flowRateMscfDay:F0} Mscf/day");
            rec.ConstraintsConsidered.Add($"Overall compression ratio: {overallCompressionRatio:F2}");
            rec.ConstraintsConsidered.Add($"Sour gas: {isSourGas}");
            rec.ConstraintsConsidered.Add("Variable flow: " + variableFlowRequired);
            rec.ConstraintsConsidered.Add($"Continuous operation: {requiresContinuousOperation}");

            // Decision tree
            if (flowRateMscfDay < 200m && overallCompressionRatio <= 6m)
            {
                rec.RecommendedType = "Rotary Screw";
                rec.PrimaryReason = "Low flow rate — screw compressors are efficient in this range.";
                rec.AlternativeType = "Reciprocating";
                rec.Confidence = "High";
                rec.Rationale.Add("Screw compressors have low maintenance (no inlet/outlet valves).");
                rec.Rationale.Add("Suitable for small field gathering at modest ratios.");
            }
            else if (flowRateMscfDay < 500m || overallCompressionRatio > 10m)
            {
                rec.RecommendedType = "Reciprocating";
                rec.PrimaryReason = flowRateMscfDay < 500m
                    ? "Low flow rate favours reciprocating machines."
                    : "High compression ratio (>10) requires reciprocating machine.";
                rec.AlternativeType = "Rotary Screw (if ratio ≤ 4)";
                rec.Confidence = "High";
                rec.Rationale.Add("Reciprocating compressors handle high ratios with multiple cylinders.");
                rec.Rationale.Add("Wide turndown capability for variable flow operations.");
                if (isSourGas)
                    rec.Rationale.Add("Note: for sour gas, materials (17-4PH, Inconel) must be specified.");
            }
            else if (isSourGas)
            {
                rec.RecommendedType = "Centrifugal";
                rec.PrimaryReason = "Centrifugal preferred for sour gas — no reciprocating valves to corrode.";
                rec.AlternativeType = "Reciprocating with corrosion-resistant materials";
                rec.Confidence = "High";
                rec.Rationale.Add("Centrifugal compressors have no contact between gas and oil in the cylinder.");
                rec.Rationale.Add("Lower maintenance frequency in corrosive service.");
            }
            else if (flowRateMscfDay >= 500m && flowRateMscfDay <= 5000m && overallCompressionRatio <= 6m)
            {
                rec.RecommendedType = "Centrifugal";
                rec.PrimaryReason = "Mid-range flow and modest ratio — centrifugal is the most efficient choice.";
                rec.AlternativeType = variableFlowRequired ? "Reciprocating (better turndown)" : "Reciprocating";
                rec.Confidence = "High";
                rec.Rationale.Add("Centrifugal compressors offer continuous, pulsation-free flow.");
                rec.Rationale.Add("Lower vibration — suitable for offshore/subsea applications.");
                if (variableFlowRequired)
                    rec.Rationale.Add("Consider variable-speed drive (VSD) for flow control.");
                if (requiresContinuousOperation)
                    rec.Rationale.Add("For 100 % availability, specify hot-standby unit.");
            }
            else
            {
                // Large flow, high ratio
                rec.RecommendedType = "Centrifugal (Multistage)";
                rec.PrimaryReason = "Large flow with moderate ratio — multistage centrifugal with intercooling.";
                rec.AlternativeType = "Axial-centrifugal combination";
                rec.Confidence = "Medium";
                rec.Rationale.Add("Evaluate number of stages using MultistageCompressor.OptimiseStageCount.");
                rec.Rationale.Add("Intercooling between stages reduces total power by 10–25 %.");
            }

            return rec;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Sensitivity analysis
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs a sensitivity analysis of total BHP and life-cycle cost against
        /// the number of compression stages (1–<paramref name="maxStages"/>).
        ///
        /// Useful for generating a table or chart to support stage-count decisions.
        /// </summary>
        /// <param name="compressorProperties">Compressor properties.</param>
        /// <param name="maxStages">Maximum stages to evaluate.</param>
        /// <param name="electricityPriceKwh">Power cost (USD/kWh).</param>
        /// <param name="projectLifeYears">Project life for NPV.</param>
        /// <param name="discountRate">Discount rate.</param>
        /// <returns>
        /// List of (N, TotalBHP, StageRatio, MaxTemp_R, CAPEX, LCC_NPV, ConstraintsOk) tuples.
        /// </returns>
        public static List<(int N, decimal TotalBHP, decimal StageRatio, decimal MaxTempR,
                             decimal CapexUsd, decimal LccNpvUsd, bool ConstraintsOk)>
            SensitivityAnalysisStageCount(
                CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
                int maxStages = 6,
                decimal electricityPriceKwh = 0.08m,
                int projectLifeYears = 20,
                decimal discountRate = 0.10m)
        {
            var results = new List<(int, decimal, decimal, decimal, decimal, decimal, bool)>();

            for (int n = 1; n <= maxStages; n++)
            {
                var msResult = MultistageCompressor.AnalyzeMultistage(compressorProperties, n);
                var lcc = CalculateLifeCycleCost(
                    compressorProperties, n, "Centrifugal",
                    electricityPriceKwh, 8000m, projectLifeYears, discountRate);

                decimal maxTemp = msResult.Stages.Max(s => s.DischargeTemperature);
                bool ok = msResult.StageCompressionRatio <= 4.0m && maxTemp <= 660m;

                results.Add((n, msResult.TotalBrakeHorsepower, msResult.StageCompressionRatio,
                             maxTemp, lcc.CapitalCostUsd, lcc.LifeCycleCostNpvUsd, ok));
            }

            return results;
        }
    }
}
