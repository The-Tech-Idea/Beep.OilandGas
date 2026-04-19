using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.PipelineAnalysis;

namespace Beep.OilandGas.PipelineAnalysis.Calculations
{
    /// <summary>
    /// Pipeline looping and compression-economics analysis.
    ///
    /// When a pipeline cannot meet throughput or delivery-pressure targets, two main
    /// options exist:
    ///  1. <b>Looping</b> — install a parallel pipe over part or all of the pipeline length
    ///     to reduce effective flow resistance.
    ///  2. <b>Intermediate compression</b> — install a compressor station to boost the gas
    ///     pressure mid-line, allowing higher throughput.
    ///
    /// This module provides:
    ///  • Equivalent-diameter method for partial / full looping (Weymouth-based).
    ///  • Required compression power and station economics (capital + operating).
    ///  • Head-to-head NPV comparison of looping vs. compression over a project life.
    ///
    /// Gas-flow equation used: Weymouth (most common for transmission-line sizing):
    ///   Q = C × D^(8/3) × √[(P₁² − P₂²) / (G × T × L × Z)]
    ///   C = 433.5 × T_b/P_b   (base-condition factor)
    ///
    /// Compressor power (isothermal efficiency basis):
    ///   HP = (Q_Mscfd × 0.0857) × (k / (k-1)) × T₁ × [(P₂/P₁)^((k-1)/k) − 1] / η
    ///   (Simplified form; full derivation in gas-compression texts.)
    ///
    /// All monetary values in USD.  Discount rate in decimal (e.g., 0.10 = 10%).
    ///
    /// References:
    ///   - Weymouth, T.R. (1912) Trans. ASME 34, pp 185-201.
    ///   - Menon, E.S. (2005) "Gas Pipeline Hydraulics", CRC Press.
    ///   - Campbell, J.M. (1992) "Gas Conditioning and Processing", Vol. 2.
    /// </summary>
    public static class LoopingAndCompressionEconomics
    {
        private const double WeymouthC_factor = 433.5; // for Tb/Pb in °R/psia

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Loop design result for a given throughput target.</summary>
        public sealed class LoopDesignResult
        {
            /// <summary>Whether a full loop achieves the target; otherwise partial loop.</summary>
            public bool FullLoopRequired { get; set; }

            /// <summary>Loop length required (miles) for partial loop; equals pipeline length for full.</summary>
            public double LoopLengthMiles { get; set; }

            /// <summary>Fraction of pipeline length looped (0–1).</summary>
            public double LoopFraction { get; set; }

            /// <summary>Diameter of the parallel loop pipe (in); same as original pipe.</summary>
            public double LoopDiameterIn { get; set; }

            /// <summary>Effective diameter of the looped section (in) — larger than single-pipe equivalent.</summary>
            public double EffectiveDiameterIn { get; set; }

            /// <summary>Achievable throughput at design pressures with the proposed loop (Mscfd).</summary>
            public double AchievableThroughputMscfd { get; set; }

            /// <summary>Estimated loop installed cost (USD).</summary>
            public double InstalledCostUsd { get; set; }

            /// <summary>Annual operating and maintenance cost (USD/year).</summary>
            public double AnnualOpexUsd { get; set; }
        }

        /// <summary>Intermediate compressor station design result.</summary>
        public sealed class CompressorStationResult
        {
            /// <summary>Required suction pressure at the compressor station (psia).</summary>
            public double SuctionPressurePsia { get; set; }

            /// <summary>Required discharge pressure (psia).</summary>
            public double DischargePressurePsia { get; set; }

            /// <summary>Compression ratio P₂/P₁.</summary>
            public double CompressionRatio { get; set; }

            /// <summary>Required brake horsepower (HP).</summary>
            public double RequiredBhp { get; set; }

            /// <summary>Installed capital cost (USD).</summary>
            public double CapitalCostUsd { get; set; }

            /// <summary>Annual fuel gas consumption (Mscfd equivalent).</summary>
            public double AnnualFuelGasMscfd { get; set; }

            /// <summary>Annual operating cost (fuel + O&amp;M, USD/year).</summary>
            public double AnnualOpexUsd { get; set; }
        }

        /// <summary>Head-to-head NPV comparison of looping vs. compression.</summary>
        public sealed class EconomicComparisonResult
        {
            /// <summary>NPV of looping option over project life (USD).</summary>
            public double LoopNpvUsd { get; set; }

            /// <summary>NPV of compression option over project life (USD).</summary>
            public double CompressionNpvUsd { get; set; }

            /// <summary>Recommended option.</summary>
            public string RecommendedOption { get; set; } = string.Empty;

            /// <summary>NPV advantage of the recommended option (USD).</summary>
            public double NpvAdvantageUsd { get; set; }

            /// <summary>Simple payback for looping (years).</summary>
            public double LoopPaybackYears { get; set; }

            /// <summary>Simple payback for compression (years).</summary>
            public double CompressionPaybackYears { get; set; }

            /// <summary>Year-by-year cost table.</summary>
            public List<YearlyComparison> YearlyCosts { get; set; } = new();
        }

        /// <summary>Annual cost for a single option in a given year.</summary>
        public sealed class YearlyComparison
        {
            public int Year { get; set; }
            public double LoopCumulativeCostUsd { get; set; }
            public double CompressionCumulativeCostUsd { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Looping design
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the required loop length and installed cost to achieve a target
        /// throughput without changing operating pressures.
        ///
        /// Method — equivalent-length approach (Weymouth):
        ///
        ///  Single-pipe flow: Q ∝ D^(8/3) / √L
        ///  With a loop of length L_loop on a pipe of total length L_total:
        ///   Resistance of unlooped section: r1 = (L_total − L_loop) / D^(8/3)
        ///   Resistance of looped section: r2 = L_loop / (2 × D^(8/3)) [two parallel pipes]
        ///   Total resistance: r_total = r1 + r2
        ///   Q_new / Q_old = √(r_old / r_new)
        ///
        /// The method iterates L_loop from 0 to L_total to find the value that achieves
        /// Q_target.
        ///
        /// Installed cost is estimated at a user-supplied unit cost (USD/in-diameter-mile),
        /// defaulting to $500,000 / inch-diameter / mile (typical onshore).
        /// </summary>
        /// <param name="pipeline">Existing pipeline properties.</param>
        /// <param name="currentThroughputMscfd">Current throughput (Mscfd).</param>
        /// <param name="targetThroughputMscfd">Target throughput (Mscfd).</param>
        /// <param name="unitCostUsdPerInMile">Installed loop cost (USD per inch-diameter per mile).</param>
        /// <param name="opexFractionOfCapex">Annual O&amp;M as a fraction of CAPEX (default 2%).</param>
        public static LoopDesignResult DesignLoop(
            PIPELINE_PROPERTIES pipeline,
            double currentThroughputMscfd,
            double targetThroughputMscfd,
            double unitCostUsdPerInMile = 500_000.0,
            double opexFractionOfCapex = 0.02)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            double dIn    = (double)pipeline.DIAMETER;        // in
            double lenMi  = (double)pipeline.LENGTH / 5280.0; // ft → miles

            if (lenMi <= 0 || dIn <= 0 || currentThroughputMscfd <= 0)
                return new LoopDesignResult();

            double flowRatioTarget = targetThroughputMscfd / currentThroughputMscfd;
            // Q ∝ 1/√r_total  →  r_old/r_new = (Q_new/Q_old)²
            double resistanceRatio = flowRatioTarget * flowRatioTarget;

            // Original resistance: r_old ∝ L / D^(8/3)  (omit constant as it cancels)
            // Normalise by D^(8/3): just use length terms
            double r_old = lenMi;

            // We need r_new = r_old / resistanceRatio
            double r_new = r_old / resistanceRatio;

            // r_new = (L - L_loop) + L_loop/2  = L - L_loop/2
            //   → L_loop = 2 × (L - r_new)
            double loopLen = 2.0 * (lenMi - r_new);
            bool fullLoop = loopLen >= lenMi;
            loopLen = Math.Min(loopLen, lenMi);
            loopLen = Math.Max(0, loopLen);

            // Achievable throughput with the computed loop
            double r_actual = lenMi - loopLen / 2.0;
            double qAchievable = currentThroughputMscfd * Math.Sqrt(r_old / Math.Max(r_actual, 1e-9));

            // Effective diameter of the looped section (two parallel pipes of same D)
            // 1/D_eff^(8/3) = 2/D^(8/3)  →  D_eff = D × 2^(3/8)
            double dEff = dIn * Math.Pow(2.0, 3.0 / 8.0);

            double capex = unitCostUsdPerInMile * dIn * loopLen;
            double opex  = capex * opexFractionOfCapex;

            return new LoopDesignResult
            {
                FullLoopRequired          = fullLoop,
                LoopLengthMiles           = Math.Round(loopLen, 2),
                LoopFraction              = Math.Round(loopLen / lenMi, 4),
                LoopDiameterIn            = dIn,
                EffectiveDiameterIn       = Math.Round(dEff, 2),
                AchievableThroughputMscfd = Math.Round(qAchievable, 1),
                InstalledCostUsd          = Math.Round(capex, 0),
                AnnualOpexUsd             = Math.Round(opex, 0)
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Compressor station design
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Designs an intermediate compressor station to achieve a target throughput.
        ///
        /// Approach:
        ///  1. Split pipeline into two halves at the compressor station location.
        ///  2. The first half delivers gas at the suction pressure of the station.
        ///  3. The compressor boosts pressure back to inlet pressure for the second half.
        ///  4. Power required (polytropic):
        ///       HP = [k/(k−1)] × (P₁×144 / 33000) × Z × T₁ × Q × [(r)^((k-1)/k) − 1] / η
        ///     where Q = actual flow (MMscfd), r = P₂/P₁, k = Cp/Cv.
        ///  5. Fuel gas: approximately 10 HP-hr / Mscf for a gas-turbine driver.
        ///
        /// Capital cost estimate: USD 2,500 per installed HP (onshore average, ±30%).
        /// </summary>
        /// <param name="pipeline">Pipeline properties.</param>
        /// <param name="throughputMscfd">Target throughput (Mscfd).</param>
        /// <param name="inletPressurePsia">Pipeline inlet pressure (psia).</param>
        /// <param name="deliveryPressurePsia">Required delivery pressure at pipeline end (psia).</param>
        /// <param name="stationLocationFraction">Station location as fraction of total length (default 0.5 = midpoint).</param>
        /// <param name="temperatureF">Average gas temperature (°F).</param>
        /// <param name="gasSg">Gas specific gravity.</param>
        /// <param name="zFactor">Gas Z-factor.</param>
        /// <param name="k">Gas Cp/Cv (default 1.28 for typical natural gas).</param>
        /// <param name="efficiency">Compressor mechanical efficiency (0–1, default 0.80).</param>
        /// <param name="gasPriceUsdPerMscf">Gas price for fuel-cost valuation (USD/Mscf).</param>
        /// <param name="opsHoursPerYear">Compressor operating hours per year (default 8000).</param>
        /// <param name="capexUsdPerHp">Installed capital cost per HP (USD/HP, default 2500).</param>
        /// <param name="opexFractionOfCapex">Annual O&amp;M as fraction of CAPEX (default 5%).</param>
        public static CompressorStationResult DesignCompressorStation(
            PIPELINE_PROPERTIES pipeline,
            double throughputMscfd,
            double inletPressurePsia,
            double deliveryPressurePsia,
            double stationLocationFraction = 0.5,
            double temperatureF = 100.0,
            double gasSg = 0.65,
            double zFactor = 0.9,
            double k = 1.28,
            double efficiency = 0.80,
            double gasPriceUsdPerMscf = 3.50,
            double opsHoursPerYear = 8000.0,
            double capexUsdPerHp = 2500.0,
            double opexFractionOfCapex = 0.05)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            double dIn   = (double)pipeline.DIAMETER;
            double lenFt = (double)pipeline.LENGTH;
            double lenMi = lenFt / 5280.0;
            double tR    = temperatureF + 459.67;
            double baseTR = 520.0;    // 60°F
            double basePPsia = 14.73;

            // Weymouth constant C
            double C = WeymouthC_factor * baseTR / basePPsia;

            // Pressure at compressor station suction:
            // First segment (inlet → station): length = f × L
            double len1Mi = stationLocationFraction * lenMi;
            // Q = C × D^(8/3) × √[(P1² - P_suct²) / (G × T × L × Z)]
            // → P_suct² = P1² − (Q/C/D^8/3)² × G × T × L × Z
            double dTerm = Math.Pow(dIn, 8.0 / 3.0);
            double qOverC = throughputMscfd / (C * dTerm);
            double pSuct2 = inletPressurePsia * inletPressurePsia
                          - qOverC * qOverC * gasSg * tR * len1Mi * zFactor;
            double pSuct = pSuct2 > 0 ? Math.Sqrt(pSuct2) : deliveryPressurePsia;

            // After compression, discharge pressure must enable the second segment to reach delivery
            double len2Mi = (1 - stationLocationFraction) * lenMi;
            double pDisc2 = deliveryPressurePsia * deliveryPressurePsia
                          + qOverC * qOverC * gasSg * tR * len2Mi * zFactor;
            double pDisc = Math.Sqrt(Math.Max(pDisc2, deliveryPressurePsia * deliveryPressurePsia));

            double ratio = pDisc / Math.Max(pSuct, 1.0);

            // Polytropic compression power (HP):
            // HP = (k/(k-1)) × Z × T₁ × Q_Mscfd × [(r)^((k-1)/k) - 1] / η × conversion
            // Conversion: 1 HP = 33,000 ft·lbf/min; standard gas ~ 0.0693 HP per Mscfd basis
            double exponent = (k - 1.0) / k;
            double hpPerMscfd = (k / (k - 1.0)) * zFactor * tR * (Math.Pow(ratio, exponent) - 1.0)
                              / efficiency * 0.00695;  // empirical HP/Mscfd conversion factor
            double bhp = hpPerMscfd * throughputMscfd;

            // Fuel gas: ~10 HP-hr / Mscf for gas turbine
            double fuelMscfd = bhp * opsHoursPerYear / 1e6 * 10.0 / 365.0;

            double capex = bhp * capexUsdPerHp;
            double opex  = capex * opexFractionOfCapex + fuelMscfd * 365.0 * gasPriceUsdPerMscf;

            return new CompressorStationResult
            {
                SuctionPressurePsia  = Math.Round(pSuct, 1),
                DischargePressurePsia= Math.Round(pDisc, 1),
                CompressionRatio     = Math.Round(ratio, 3),
                RequiredBhp          = Math.Round(bhp, 0),
                CapitalCostUsd       = Math.Round(capex, 0),
                AnnualFuelGasMscfd   = Math.Round(fuelMscfd, 3),
                AnnualOpexUsd        = Math.Round(opex, 0)
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  NPV comparison: looping vs. compression
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes a head-to-head NPV comparison between looping and compression alternatives
        /// over a specified project life.
        ///
        /// Each year's cash flow = additional revenue − annual OPEX.
        /// Additional revenue from the capacity increase is the same for both options
        /// (they achieve the same throughput), so the comparison reduces to:
        ///   NPV_loop       = −CAPEX_loop  + Σ(−OPEX_loop / (1+r)^t)
        ///   NPV_compression= −CAPEX_comp  + Σ(−OPEX_comp / (1+r)^t)
        ///
        /// The option with the less negative (or most positive) NPV is recommended.
        /// </summary>
        /// <param name="loopDesign">Loop design output.</param>
        /// <param name="compressorDesign">Compressor station design output.</param>
        /// <param name="projectLifeYears">Economic project life (years).</param>
        /// <param name="discountRate">Annual discount rate (decimal, e.g., 0.10).</param>
        /// <param name="annualRevenueUsd">Annual revenue from additional capacity (USD/year).</param>
        public static EconomicComparisonResult CompareOptions(
            LoopDesignResult loopDesign,
            CompressorStationResult compressorDesign,
            int projectLifeYears = 20,
            double discountRate = 0.10,
            double annualRevenueUsd = 0.0)
        {
            var result = new EconomicComparisonResult();
            double loopNpv  = -loopDesign.InstalledCostUsd;
            double compNpv  = -compressorDesign.CapitalCostUsd;
            double loopCum  = loopDesign.InstalledCostUsd;
            double compCum  = compressorDesign.CapitalCostUsd;
            double loopPayback = 0, compPayback = 0;
            bool loopPaidBack = false, compPaidBack = false;

            for (int t = 1; t <= projectLifeYears; t++)
            {
                double df = Math.Pow(1 + discountRate, t);
                double loopNet = annualRevenueUsd - loopDesign.AnnualOpexUsd;
                double compNet = annualRevenueUsd - compressorDesign.AnnualOpexUsd;

                loopNpv += loopNet / df;
                compNpv += compNet / df;

                loopCum += loopDesign.AnnualOpexUsd - annualRevenueUsd;
                compCum += compressorDesign.AnnualOpexUsd - annualRevenueUsd;

                if (!loopPaidBack && loopCum <= 0) { loopPayback = t; loopPaidBack = true; }
                if (!compPaidBack && compCum <= 0) { compPayback = t; compPaidBack = true; }

                result.YearlyCosts.Add(new YearlyComparison
                {
                    Year = t,
                    LoopCumulativeCostUsd        = Math.Round(loopCum,  0),
                    CompressionCumulativeCostUsd = Math.Round(compCum, 0)
                });
            }

            result.LoopNpvUsd        = Math.Round(loopNpv, 0);
            result.CompressionNpvUsd = Math.Round(compNpv, 0);
            result.LoopPaybackYears  = loopPaidBack  ? loopPayback  : projectLifeYears + 1;
            result.CompressionPaybackYears = compPaidBack ? compPayback : projectLifeYears + 1;

            bool loopWins = loopNpv >= compNpv;
            result.RecommendedOption = loopWins ? "Looping" : "Intermediate Compression";
            result.NpvAdvantageUsd   = Math.Round(Math.Abs(loopNpv - compNpv), 0);

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Sensitivity: throughput vs. compression ratio
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a throughput sensitivity table showing achievable Mscfd for a range
        /// of compression ratios at fixed pipeline geometry and delivery pressure.
        ///
        /// Useful for sizing a compressor station for future capacity growth.
        /// </summary>
        /// <param name="pipeline">Pipeline properties.</param>
        /// <param name="inletPressurePsia">Inlet pressure (psia).</param>
        /// <param name="deliveryPressurePsia">Required delivery pressure (psia).</param>
        /// <param name="temperatureF">Operating temperature (°F).</param>
        /// <param name="gasSg">Gas specific gravity.</param>
        /// <param name="zFactor">Z-factor.</param>
        /// <param name="minRatio">Minimum compression ratio to evaluate.</param>
        /// <param name="maxRatio">Maximum compression ratio to evaluate.</param>
        /// <param name="steps">Number of evaluation points.</param>
        public static List<(double CompressionRatio, double BoostPressurePsia, double ThroughputMscfd)>
            CompressionRatioSensitivity(
                PIPELINE_PROPERTIES pipeline,
                double inletPressurePsia,
                double deliveryPressurePsia,
                double temperatureF = 100.0,
                double gasSg = 0.65,
                double zFactor = 0.9,
                double minRatio = 1.0,
                double maxRatio = 2.5,
                int steps = 10)
        {
            double dIn  = (double)pipeline.DIAMETER;
            double lenMi = (double)pipeline.LENGTH / 5280.0;
            double tR   = temperatureF + 459.67;
            double C    = WeymouthC_factor * 520.0 / 14.73;
            double dTerm = Math.Pow(dIn, 8.0 / 3.0);

            var results = new List<(double, double, double)>();
            double step = steps > 1 ? (maxRatio - minRatio) / (steps - 1) : 0;

            for (int i = 0; i < steps; i++)
            {
                double ratio = minRatio + i * step;
                // Boosted inlet pressure to the second segment = inletPressure × ratio
                double pBoosted = inletPressurePsia * ratio;
                // Weymouth throughput with boosted pressure
                double pDiff2 = pBoosted * pBoosted - deliveryPressurePsia * deliveryPressurePsia;
                double q = pDiff2 > 0
                    ? C * dTerm * Math.Sqrt(pDiff2 / (gasSg * tR * lenMi * zFactor))
                    : 0;
                results.Add((Math.Round(ratio, 2), Math.Round(pBoosted, 1), Math.Round(q, 1)));
            }

            return results;
        }
    }
}
