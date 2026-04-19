using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.PipelineAnalysis;

namespace Beep.OilandGas.PipelineAnalysis.Calculations
{
    /// <summary>
    /// Pipeline pigging analysis: velocity, timing, scheduling, efficiency, and
    /// wax-removal cost evaluation.
    ///
    /// Pigging is used to:
    ///  • Remove wax, scale, and debris from the pipeline bore
    ///  • Displace liquid slugs (liquid management)
    ///  • Carry out in-line inspection (ILI / smart pigging)
    ///  • Batch separate different products
    ///
    /// The pig velocity in a gas pipeline is equal to the gas velocity at the pig's
    /// location.  For liquid pipelines the pig travels at the average liquid velocity.
    ///
    /// Regulatory references:
    ///  • ASME B31.4 / B31.8 — operational pigging requirements
    ///  • API 1160 — managing pipeline integrity with smart pigs
    ///  • NACE SP0169 — corrosion-control pigging frequency guidelines
    ///
    /// Units:
    ///   Diameter (in), Length (miles or ft — noted per parameter),
    ///   Pressure (psia), Flow rate (Mscfd for gas; bbl/d for liquid),
    ///   Velocity (ft/s), Time (hours).
    /// </summary>
    public static class PiggingAnalysis
    {
        // ─────────────────────────────────────────────────────────────────
        //  Pig type enumeration
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Type of pig / intelligent tool.</summary>
        public enum PigType
        {
            /// <summary>Simple foam or sphere pig — cleaning / liquid displacement.</summary>
            Foam,
            /// <summary>Brush / scraper pig — wax and scale removal.</summary>
            Scraper,
            /// <summary>Gel pig — corrosion-inhibitor or biocide delivery.</summary>
            Gel,
            /// <summary>Magnetic flux leakage (MFL) smart pig — wall-thickness measurement.</summary>
            MagneticFluxLeakage,
            /// <summary>Ultrasonic (UT) smart pig — crack and corrosion detection.</summary>
            Ultrasonic,
            /// <summary>Caliper / geometry pig — bore deformation detection.</summary>
            Caliper,
            /// <summary>Batch-separator pig between two products.</summary>
            BatchSeparator
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Pig run velocity and timing analysis result.</summary>
        public sealed class PigRunResult
        {
            /// <summary>Average pig velocity (ft/s).</summary>
            public double AvgVelocityFtS { get; set; }

            /// <summary>Minimum pig velocity at outlet end (lowest-pressure point, ft/s).</summary>
            public double MinVelocityFtS { get; set; }

            /// <summary>Maximum pig velocity at inlet (highest-pressure point, ft/s).</summary>
            public double MaxVelocityFtS { get; set; }

            /// <summary>Transit time for the pig to travel the full length (hours).</summary>
            public double TransitTimeHours { get; set; }

            /// <summary>Estimated liquid slug volume pushed ahead of pig (bbl).</summary>
            public double LiquidSlugBbl { get; set; }

            /// <summary>Whether pig velocity is within recommended range for the pig type.</summary>
            public bool VelocityAcceptable { get; set; }

            /// <summary>Recommended minimum velocity for pig type (ft/s).</summary>
            public double RecommendedMinVelocityFtS { get; set; }

            /// <summary>Recommended maximum velocity for pig type (ft/s).</summary>
            public double RecommendedMaxVelocityFtS { get; set; }

            /// <summary>Notes and warnings.</summary>
            public List<string> Notes { get; set; } = new();
        }

        /// <summary>Pigging programme schedule recommendation.</summary>
        public sealed class PiggingSchedule
        {
            /// <summary>Cleaning pig (scraper) run interval (days).</summary>
            public double CleaningIntervalDays { get; set; }

            /// <summary>Corrosion-inhibitor batch pig interval (days).</summary>
            public double InhibitorIntervalDays { get; set; }

            /// <summary>In-line inspection (ILI/smart pig) interval (years).</summary>
            public double IliIntervalYears { get; set; }

            /// <summary>Next recommended cleaning pig date (from reference date).</summary>
            public string NextCleaningPigDate { get; set; } = string.Empty;

            /// <summary>Next recommended ILI date (from reference date).</summary>
            public string NextIliDate { get; set; } = string.Empty;

            /// <summary>Scheduling basis notes.</summary>
            public List<string> Basis { get; set; } = new();
        }

        /// <summary>Wax-removal cost-benefit analysis result.</summary>
        public sealed class WaxRemovalResult
        {
            /// <summary>Current throughput reduction due to wax (% of clean-pipe capacity).</summary>
            public double ThroughputReductionPct { get; set; }

            /// <summary>Annual revenue lost due to wax-related throughput reduction (USD/year).</summary>
            public double AnnualRevenueLossUsd { get; set; }

            /// <summary>Cost per pig run (USD).</summary>
            public double CostPerPigRunUsd { get; set; }

            /// <summary>Optimal pig run frequency (runs/year) that minimises total cost.</summary>
            public double OptimalRunsPerYear { get; set; }

            /// <summary>Net annual saving vs. baseline (no pigging) at optimal frequency (USD/year).</summary>
            public double NetAnnualSavingUsd { get; set; }

            /// <summary>Simple payback period at optimal frequency (months).</summary>
            public double PaybackMonths { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Pig run velocity and timing
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates pig transit velocity and time for a gas pipeline.
        ///
        /// In a gas pipeline the pig velocity equals the local gas velocity.
        /// The gas velocity varies along the pipe as pressure falls.
        ///
        /// At any location x:
        ///   v_gas(x) = Q_sc × Z × T / (A × P(x)) × base_ratio
        ///
        /// A simple two-point (inlet, outlet) average is used here.
        ///
        /// For liquid lines, the pig travels at the average liquid velocity.
        /// </summary>
        /// <param name="gasMscfd">Gas flow rate (Mscfd) — set 0 for liquid pipeline.</param>
        /// <param name="liquidBbld">Liquid flow rate (bbl/d) — set 0 for gas pipeline.</param>
        /// <param name="diameterIn">Pipe inside diameter (in).</param>
        /// <param name="lengthMiles">Pipeline length (miles).</param>
        /// <param name="inletPressurePsia">Inlet pressure (psia).</param>
        /// <param name="outletPressurePsia">Outlet pressure (psia).</param>
        /// <param name="temperatureF">Average temperature (°F).</param>
        /// <param name="zFactor">Gas compressibility Z.</param>
        /// <param name="pigType">Type of pig being run.</param>
        /// <param name="liquidHoldupFraction">Liquid holdup fraction along the pipeline (for slug volume estimate).</param>
        public static PigRunResult CalculatePigRun(
            double gasMscfd,
            double liquidBbld,
            double diameterIn,
            double lengthMiles,
            double inletPressurePsia,
            double outletPressurePsia,
            double temperatureF = 100.0,
            double zFactor = 0.9,
            PigType pigType = PigType.Scraper,
            double liquidHoldupFraction = 0.05)
        {
            var result = new PigRunResult();
            double dFt   = diameterIn / 12.0;
            double area  = Math.PI * dFt * dFt / 4.0;  // ft²
            double lenFt = lengthMiles * 5280.0;
            double tR    = temperatureF + 459.67;

            // Recommended velocity range per pig type
            (double vMin, double vMax) = PigVelocityLimits(pigType);
            result.RecommendedMinVelocityFtS = vMin;
            result.RecommendedMaxVelocityFtS = vMax;

            double vAvg;
            if (gasMscfd > 0 && liquidBbld < gasMscfd * 0.1)
            {
                // Gas pipeline
                double qScfd = gasMscfd * 1000.0;
                // Actual volumetric flow at inlet and outlet
                double qInFt3s  = qScfd * zFactor * (tR / 520.0) * (14.7 / inletPressurePsia)  / 86400.0;
                double qOutFt3s = qScfd * zFactor * (tR / 520.0) * (14.7 / outletPressurePsia) / 86400.0;
                double vIn  = area > 0 ? qInFt3s  / area : 0;
                double vOut = area > 0 ? qOutFt3s / area : 0;
                vAvg = (vIn + vOut) / 2.0;
                result.MinVelocityFtS = Math.Round(vIn, 2);
                result.MaxVelocityFtS = Math.Round(vOut, 2);
            }
            else
            {
                // Liquid pipeline
                double qlFt3s = liquidBbld * 5.615 / 86400.0;
                vAvg = area > 0 ? qlFt3s / area : 0;
                result.MinVelocityFtS = Math.Round(vAvg, 2);
                result.MaxVelocityFtS = Math.Round(vAvg, 2);
            }

            result.AvgVelocityFtS = Math.Round(vAvg, 2);
            result.TransitTimeHours = vAvg > 1e-9 ? Math.Round(lenFt / vAvg / 3600.0, 2) : 0;

            // Liquid slug volume: accumulated holdup ahead of pig
            double pipeVolumeBbl = area * lenFt * liquidHoldupFraction / 5.615;
            result.LiquidSlugBbl = Math.Round(pipeVolumeBbl, 1);

            result.VelocityAcceptable = vAvg >= vMin && vAvg <= vMax;

            if (vAvg < vMin)
                result.Notes.Add($"Pig velocity {vAvg:F1} ft/s below minimum {vMin:F1} ft/s — pig may stall. Increase flow rate or use lighter pig.");
            if (vAvg > vMax)
                result.Notes.Add($"Pig velocity {vAvg:F1} ft/s exceeds maximum {vMax:F1} ft/s — risk of pig damage / by-pass. Reduce flow rate.");
            if (result.LiquidSlugBbl > 1000)
                result.Notes.Add($"Large liquid slug estimated ({result.LiquidSlugBbl:F0} bbl) — ensure receiver can handle slug volume and slug catcher is sized adequately.");

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Pigging schedule recommendation
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Recommends a pigging schedule based on pipeline service, fluid, and corrosion risk.
        ///
        /// Schedule is based on industry best-practice guidelines:
        ///  • High-wax or high-CO₂ service: cleaning every 14–30 days
        ///  • Low-wax / dry gas:             cleaning every 60–90 days
        ///  • ILI interval: 5 years (standard); 3 years (high-risk / corrosive)
        ///  • Inhibitor batching: same frequency as cleaning
        ///
        /// References: NACE SP0169, API 1160, operator best-practice manuals.
        /// </summary>
        /// <param name="pipeline">Pipeline metadata.</param>
        /// <param name="waxContent">Wax content of fluid (0–1 scale: 0 = none, 1 = very heavy).</param>
        /// <param name="co2Fraction">CO₂ mole fraction in gas (0–1).</param>
        /// <param name="h2sFraction">H₂S mole fraction in gas (0–1).</param>
        /// <param name="referenceDate">Reference date for "next run" computation (default = today).</param>
        public static PiggingSchedule RecommendSchedule(
            PIPELINE_PROPERTIES pipeline,
            double waxContent = 0.05,
            double co2Fraction = 0.02,
            double h2sFraction = 0.001,
            DateTime? referenceDate = null)
        {
            var today = referenceDate ?? DateTime.Today;
            var schedule = new PiggingSchedule();
            var notes = new List<string>();

            // Cleaning interval
            double cleanDays;
            if (waxContent >= 0.15)
            { cleanDays = 14;  notes.Add("High-wax service: 14-day cleaning cycle recommended."); }
            else if (waxContent >= 0.05)
            { cleanDays = 30;  notes.Add("Moderate-wax service: 30-day cleaning cycle recommended."); }
            else
            { cleanDays = 60;  notes.Add("Low-wax service: 60-day cleaning cycle recommended."); }

            // Corrosive service — tighten schedule
            bool corrosive = co2Fraction > 0.03 || h2sFraction > 0.001;
            if (corrosive)
            {
                cleanDays /= 2.0;
                notes.Add("Corrosive fluid (CO₂/H₂S) — cleaning interval halved.");
            }

            // Inhibitor batching — same interval as cleaning for corrosive, else 2×
            double inhibitorDays = corrosive ? cleanDays : cleanDays * 2.0;

            // ILI interval
            double iliYears = corrosive || waxContent >= 0.15 ? 3.0 : 5.0;
            notes.Add($"ILI (smart pig) interval: {iliYears:F0} years per API 1160.");

            schedule.CleaningIntervalDays   = Math.Round(cleanDays, 0);
            schedule.InhibitorIntervalDays  = Math.Round(inhibitorDays, 0);
            schedule.IliIntervalYears       = iliYears;
            schedule.NextCleaningPigDate    = today.AddDays(cleanDays).ToString("yyyy-MM-dd");
            schedule.NextIliDate            = today.AddYears((int)iliYears).ToString("yyyy-MM-dd");
            schedule.Basis                  = notes;

            return schedule;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Wax-removal cost-benefit
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Determines the economically optimal pigging frequency for wax-affected pipelines
        /// by minimising total cost = revenue loss + pigging cost.
        ///
        /// Model assumptions:
        ///  • Wax deposition reduces effective bore; throughput ∝ D_eff^4.8 (Weymouth exponent for gas).
        ///  • Wax build-up is linear with time between pig runs.
        ///  • After each pig run the pipeline is restored to clean-bore condition.
        ///  • Average throughput reduction over interval T = 0.5 × max_reduction_at_interval_T.
        ///
        /// Cost function:
        ///   C_total(n) = revenue_loss(n) + n × cost_per_run
        ///   revenue_loss(n) = 0.5 × annual_wax_reduction(1/n) × daily_revenue × 365
        ///
        /// Minimised numerically over integer runs/year range 1–52.
        /// </summary>
        /// <param name="pipelineLengthMiles">Pipeline length (miles).</param>
        /// <param name="diameterIn">Nominal pipe inside diameter (in).</param>
        /// <param name="maxWaxThicknessIn">Maximum wax thickness at one-year no-pig interval (in).</param>
        /// <param name="dailyRevenueUsd">Daily revenue from produced/transported oil or gas (USD/day).</param>
        /// <param name="costPerPigRunUsd">All-in cost per pig run including mob/demob (USD).</param>
        public static WaxRemovalResult OptimiseWaxPiggingFrequency(
            double pipelineLengthMiles,
            double diameterIn,
            double maxWaxThicknessIn,
            double dailyRevenueUsd,
            double costPerPigRunUsd)
        {
            var result = new WaxRemovalResult { CostPerPigRunUsd = costPerPigRunUsd };

            double cleanD  = diameterIn;
            double maxWax  = maxWaxThicknessIn;

            // Annual revenue at clean-bore baseline
            double annualRevenue = dailyRevenueUsd * 365.0;

            double bestNetSaving = double.MinValue;
            double optRuns = 1;

            for (int runs = 1; runs <= 52; runs++)
            {
                // Average wax thickness over interval T = 1/runs years
                // Wax grows linearly; at end of interval: wax = maxWax / runs
                double waxAtEnd = maxWax / runs;
                double avgWax   = waxAtEnd / 2.0;

                // Effective ID
                double effD = Math.Max(0.1, cleanD - 2 * avgWax);

                // Throughput ratio ∝ (D_eff / D_clean)^4.8 (Weymouth gas exponent)
                double throughputRatio = Math.Pow(effD / cleanD, 4.8);
                double revenueLoss = annualRevenue * (1 - throughputRatio);

                double piggingCost = runs * costPerPigRunUsd;
                double netSaving   = annualRevenue * (1 - throughputRatio) - piggingCost;

                // Maximise net saving = minimise total cost vs. no-pigging
                if (netSaving > bestNetSaving)
                {
                    bestNetSaving = netSaving;
                    optRuns = runs;
                }
            }

            // Final values at optimum
            double optWaxEnd = maxWax / optRuns;
            double optAvgWax = optWaxEnd / 2.0;
            double optEffD   = Math.Max(0.1, cleanD - 2 * optAvgWax);
            double optTRatio = Math.Pow(optEffD / cleanD, 4.8);

            result.ThroughputReductionPct = Math.Round((1 - optTRatio) * 100.0, 1);
            result.AnnualRevenueLossUsd   = Math.Round(annualRevenue * (1 - optTRatio), 0);
            result.OptimalRunsPerYear     = optRuns;
            result.NetAnnualSavingUsd     = Math.Round(Math.Max(0, bestNetSaving), 0);
            result.PaybackMonths          = costPerPigRunUsd > 0 && bestNetSaving > 0
                ? Math.Round(optRuns * costPerPigRunUsd / bestNetSaving * 12.0, 1)
                : 0;

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Returns recommended velocity range (min, max) in ft/s for each pig type.</summary>
        private static (double min, double max) PigVelocityLimits(PigType pigType) =>
            pigType switch
            {
                PigType.Foam               => (1.0, 10.0),
                PigType.Scraper            => (1.0, 8.0),
                PigType.Gel                => (0.5, 4.0),
                PigType.MagneticFluxLeakage=> (1.5, 6.0),
                PigType.Ultrasonic         => (1.0, 4.0),
                PigType.Caliper            => (1.0, 5.0),
                PigType.BatchSeparator     => (2.0, 10.0),
                _                          => (1.0, 8.0)
            };
    }
}
