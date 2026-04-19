using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Calculations
{
    /// <summary>
    /// Stage-by-stage analysis for multistage centrifugal and reciprocating compressors.
    ///
    /// In a multistage compressor the total compression ratio is divided among N stages.
    /// For minimum total power, the compression ratio is distributed equally across all stages
    /// (assuming equal efficiencies and perfect intercooling):
    ///
    ///   r_stage = (P_discharge / P_suction)^(1 / N)
    ///
    /// With intercoolers the gas is cooled back to approximately suction temperature before
    /// entering the next stage, reducing specific volume and therefore stage work.
    ///
    /// References:
    ///   - API 617 (8th ed.), Section 3 — Multistage Centrifugal Compressors
    ///   - Campbell, J.M. (2004) "Gas Conditioning and Processing", Vol. 2, Ch. 13
    ///   - Ludtke, K.H. (2004) "Process Centrifugal Compressors", Springer, Ch. 4
    /// </summary>
    public static class MultistageCompressor
    {
        // ─────────────────────────────────────────────────────────────────
        //  Data structures for stage results
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Results for a single compressor stage.
        /// </summary>
        public sealed class StageResult
        {
            /// <summary>Stage number (1-based).</summary>
            public int StageNumber { get; set; }

            /// <summary>Suction pressure of this stage (psia).</summary>
            public decimal SuctionPressure { get; set; }

            /// <summary>Discharge pressure of this stage (psia).</summary>
            public decimal DischargePressure { get; set; }

            /// <summary>Suction temperature entering this stage (°R).</summary>
            public decimal SuctionTemperature { get; set; }

            /// <summary>Discharge temperature leaving this stage (°R).</summary>
            public decimal DischargeTemperature { get; set; }

            /// <summary>Temperature leaving the intercooler after this stage (°R).</summary>
            public decimal CoolerOutletTemperature { get; set; }

            /// <summary>Per-stage compression ratio (P_out / P_in).</summary>
            public decimal CompressionRatio { get; set; }

            /// <summary>Per-stage polytropic head (ft-lbf/lbm).</summary>
            public decimal PolytropicHead { get; set; }

            /// <summary>Brake horsepower for this stage (HP).</summary>
            public decimal BrakeHorsepower { get; set; }

            /// <summary>Z-factor at stage average conditions.</summary>
            public decimal ZFactor { get; set; }

            /// <summary>Polytropic efficiency for this stage.</summary>
            public decimal PolytropicEfficiency { get; set; }
        }

        /// <summary>
        /// Overall multistage compressor result.
        /// </summary>
        public sealed class MultistageResult
        {
            /// <summary>Per-stage detail results (ordered stage 1 → N).</summary>
            public List<StageResult> Stages { get; set; } = new();

            /// <summary>Total brake horsepower (sum of all stages).</summary>
            public decimal TotalBrakeHorsepower { get; set; }

            /// <summary>Total motor horsepower including mechanical losses.</summary>
            public decimal TotalMotorHorsepower { get; set; }

            /// <summary>Total power consumption (kW).</summary>
            public decimal TotalPowerKw { get; set; }

            /// <summary>Overall polytropic efficiency (harmonic mean of stage efficiencies).</summary>
            public decimal OverallPolytropicEfficiency { get; set; }

            /// <summary>Intercooler duty per cooler (BTU/hr) — same for all coolers when perfectly intercooled.</summary>
            public decimal IntercoolerDutyPerStage { get; set; }

            /// <summary>Total intercooler heat duty (BTU/hr).</summary>
            public decimal TotalIntercoolerDuty { get; set; }

            /// <summary>Final discharge temperature after the last stage (°R).</summary>
            public decimal FinalDischargeTemperature { get; set; }

            /// <summary>Per-stage compression ratio (equal for all stages when optimally distributed).</summary>
            public decimal StageCompressionRatio { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Core analysis methods
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs full stage-by-stage analysis of a multistage centrifugal compressor
        /// with intermediate intercooling between stages.
        ///
        /// For equal compression ratio distribution (minimum work criterion):
        ///   r_stage = (P_discharge / P_suction)^(1 / N)
        ///
        /// With perfect intercooling each stage inlet temperature returns to T_suction.
        /// With imperfect intercooling (approach temperature ΔT_approach) the inlet
        /// temperature to stage k+1 is:
        ///   T_in(k+1) = T_suction + ΔT_approach
        ///
        /// Power per stage:
        ///   BHP_k = (W · H_poly_k) / (33000 · η_polytropic)
        ///
        /// Intercooler duty per stage:
        ///   Q_cooler = W · Cp · (T_discharge_k - T_inlet_{k+1})   [BTU/hr]
        ///
        /// Reference: Campbell (2004), Vol. 2, p. 13-1 to 13-14.
        /// </summary>
        /// <param name="compressorProperties">Compressor properties (efficiency, k, speed).</param>
        /// <param name="numberOfStages">Number of compression stages.</param>
        /// <param name="intercoolerApproachTemp">
        ///   Temperature above suction that each intercooler achieves (°R).
        ///   0 = perfect intercooling; 20–50 °R is typical for air-cooled exchangers.
        /// </param>
        /// <returns><see cref="MultistageResult"/> with per-stage and overall results.</returns>
        public static MultistageResult AnalyzeMultistage(
            CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
            int numberOfStages,
            decimal intercoolerApproachTemp = 20m)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));
            if (compressorProperties.OPERATING_CONDITIONS == null)
                throw new ArgumentNullException(nameof(compressorProperties.OPERATING_CONDITIONS));
            if (numberOfStages < 1 || numberOfStages > 12)
                throw new ArgumentOutOfRangeException(nameof(numberOfStages), "Stages must be 1–12.");

            var conditions = compressorProperties.OPERATING_CONDITIONS;
            decimal k = compressorProperties.SPECIFIC_HEAT_RATIO;
            decimal etaP = compressorProperties.POLYTROPIC_EFFICIENCY;
            decimal etaMech = conditions.MECHANICAL_EFFICIENCY;
            decimal gamma = conditions.GAS_SPECIFIC_GRAVITY;
            decimal mw = gamma * 28.9645m;
            decimal R = 1545m;  // ft-lbf/(lbmol·°R)

            // Flow rate in scf/min → weight flow in lbm/min
            decimal qScfMin = conditions.GAS_FLOW_RATE * 1000m / 1440m;
            decimal weightFlow = qScfMin * mw / 379.0m;  // lbm/min

            // Optimal equal stage compression ratio
            decimal overallRatio = conditions.DISCHARGE_PRESSURE / conditions.SUCTION_PRESSURE;
            decimal stageRatio = (decimal)Math.Pow((double)overallRatio, 1.0 / numberOfStages);

            var result = new MultistageResult { StageCompressionRatio = Math.Round(stageRatio, 4) };
            decimal totalBHP = 0m;
            decimal totalIntercoolerDuty = 0m;
            decimal pIn = conditions.SUCTION_PRESSURE;
            decimal tIn = conditions.SUCTION_TEMPERATURE;

            for (int s = 1; s <= numberOfStages; s++)
            {
                decimal pOut = pIn * stageRatio;
                decimal pAvg = (pIn + pOut) / 2m;
                decimal z = ZFactorCalculator.CalculateBrillBeggs(pAvg, tIn, gamma);

                // Polytropic exponent
                decimal n = etaP > 0.999m ? k : (k * etaP) / (k - etaP * (k - 1m));

                // Discharge temperature
                decimal tOut = tIn * (decimal)Math.Pow((double)stageRatio, (double)((n - 1m) / n));

                // Polytropic head
                decimal head = (z * R * tIn / mw) *
                               (n / (n - 1m)) *
                               ((decimal)Math.Pow((double)stageRatio, (double)((n - 1m) / n)) - 1m);
                head = Math.Max(0m, head);

                // Stage BHP
                decimal bhp = (weightFlow * head) / 33000m / etaP;
                bhp = Math.Max(0m, bhp);

                // Intercooler outlet temperature (next stage inlet)
                decimal coolerOut = (s < numberOfStages)
                    ? conditions.SUCTION_TEMPERATURE + intercoolerApproachTemp
                    : tOut;  // last stage has no intercooler

                // Intercooler duty: Q = W * Cp * ΔT  (Cp_gas ≈ Cp_air * SG, ~0.55 BTU/(lbm·°R) for gas)
                decimal cpGas = 0.55m;  // BTU/(lbm·°R) typical for methane-rich gas
                decimal intercoolerDuty = (s < numberOfStages)
                    ? weightFlow * 60m * cpGas * (tOut - coolerOut)  // BTU/hr
                    : 0m;
                intercoolerDuty = Math.Max(0m, intercoolerDuty);

                result.Stages.Add(new StageResult
                {
                    StageNumber = s,
                    SuctionPressure = Math.Round(pIn, 2),
                    DischargePressure = Math.Round(pOut, 2),
                    SuctionTemperature = Math.Round(tIn, 1),
                    DischargeTemperature = Math.Round(tOut, 1),
                    CoolerOutletTemperature = Math.Round(coolerOut, 1),
                    CompressionRatio = Math.Round(stageRatio, 4),
                    PolytropicHead = Math.Round(head, 0),
                    BrakeHorsepower = Math.Round(bhp, 1),
                    ZFactor = Math.Round(z, 4),
                    PolytropicEfficiency = etaP
                });

                totalBHP += bhp;
                totalIntercoolerDuty += intercoolerDuty;

                // Advance to next stage
                pIn = pOut;
                tIn = coolerOut;

                // Track final discharge temperature (last stage, no cooling)
                if (s == numberOfStages)
                    result.FinalDischargeTemperature = Math.Round(tOut, 1);
            }

            decimal totalMHP = totalBHP / etaMech;

            result.TotalBrakeHorsepower = Math.Round(totalBHP, 1);
            result.TotalMotorHorsepower = Math.Round(totalMHP, 1);
            result.TotalPowerKw = Math.Round(totalMHP * 0.746m, 1);
            result.OverallPolytropicEfficiency = etaP;  // uniform per stage in this model
            result.IntercoolerDutyPerStage = numberOfStages > 1
                ? Math.Round(totalIntercoolerDuty / (numberOfStages - 1), 0)
                : 0m;
            result.TotalIntercoolerDuty = Math.Round(totalIntercoolerDuty, 0);

            return result;
        }

        /// <summary>
        /// Determines the optimal number of stages for a given overall compression ratio
        /// by minimising total power (accounting for intercooler cost vs compression savings).
        ///
        /// Rule of thumb: each stage compression ratio should stay in the range 1.5–4.0 for
        /// centrifugal machines. Above 4 per stage, discharge temperatures become excessive.
        ///
        /// This method evaluates N = 1 … maxStages and returns the configuration with the
        /// lowest total BHP, subject to:
        ///   1. r_stage ≤ maxRatioPerStage
        ///   2. T_discharge_stage ≤ maxDischargeTemp (°R)
        ///
        /// Reference: Campbell (2004), Vol. 2, p. 13-8; API 617, Annex F.
        /// </summary>
        /// <param name="compressorProperties">Compressor properties.</param>
        /// <param name="maxStages">Maximum stages to evaluate (default 6).</param>
        /// <param name="maxRatioPerStage">Maximum allowable per-stage compression ratio (default 4.0).</param>
        /// <param name="maxDischargeTemp">Maximum allowable discharge temperature per stage (°R, default 660 = 200 °F).</param>
        /// <returns>
        /// List of (N, TotalBHP, StageRatio, MaxStageTemp) tuples ordered from fewest to most stages,
        /// including a flag indicating whether each option satisfies the constraints.
        /// </returns>
        public static List<(int Stages, decimal TotalBHP, decimal StageRatio, decimal MaxTemp, bool ConstraintsSatisfied)>
            OptimiseStageCount(
                CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
                int maxStages = 6,
                decimal maxRatioPerStage = 4.0m,
                decimal maxDischargeTemp = 660m)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));

            var results = new List<(int, decimal, decimal, decimal, bool)>();

            for (int n = 1; n <= maxStages; n++)
            {
                var msResult = AnalyzeMultistage(compressorProperties, n);
                decimal maxTemp = 0m;
                foreach (var stage in msResult.Stages)
                    if (stage.DischargeTemperature > maxTemp) maxTemp = stage.DischargeTemperature;

                bool ok = msResult.StageCompressionRatio <= maxRatioPerStage
                       && maxTemp <= maxDischargeTemp;

                results.Add((n, msResult.TotalBrakeHorsepower, msResult.StageCompressionRatio, maxTemp, ok));
            }

            return results;
        }

        /// <summary>
        /// Calculates the benefit of intercooling vs. single-stage compression in terms of
        /// power savings and heat load.
        ///
        /// Power savings arise because cooling reduces specific volume before the next stage.
        /// Savings are proportional to the temperature reduction ratio:
        ///   Savings fraction ≈ 1 - (T_cooled / T_discharge_stage)^((n-1)/n) per stage pair.
        /// </summary>
        /// <param name="compressorProperties">Compressor properties.</param>
        /// <param name="numberOfStages">Number of stages to evaluate.</param>
        /// <returns>
        /// Tuple: (PowerSavingBHP — savings vs no intercooling, SavingsFraction, TotalCoolerDutyBtuHr).
        /// </returns>
        public static (decimal PowerSavingBHP, decimal SavingsFraction, decimal TotalCoolerDutyBtuHr)
            CalculateIntercoolingBenefit(CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties, int numberOfStages)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));

            // Compare: staged with intercooling vs single-stage at same overall ratio
            var multiResult = AnalyzeMultistage(compressorProperties, numberOfStages, intercoolerApproachTemp: 20m);
            var singleResult = AnalyzeMultistage(compressorProperties, 1, intercoolerApproachTemp: 0m);

            decimal savings = singleResult.TotalBrakeHorsepower - multiResult.TotalBrakeHorsepower;
            decimal fraction = singleResult.TotalBrakeHorsepower > 0
                ? savings / singleResult.TotalBrakeHorsepower
                : 0m;

            return (Math.Round(savings, 1), Math.Round(fraction, 4), multiResult.TotalIntercoolerDuty);
        }
    }
}
