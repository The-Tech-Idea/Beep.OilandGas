using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PipelineAnalysis.Calculations
{
    /// <summary>
    /// Pipeline erosion prediction and corrosion assessment.
    ///
    /// Methods implemented:
    ///  1. <b>API RP 14E Erosional Velocity</b>
    ///       Ve = C / √ρ_m
    ///       Ve  = erosional velocity (ft/s)
    ///       C   = empirical constant (100 for solid-free service; 125 for inhibited; 150 for
    ///             clean continuous service per API RP 14E 2007, §2.5)
    ///       ρ_m = in-situ mixture density (lb/ft³)
    ///     Operating velocity must stay below Ve; derating factor applied for sand presence.
    ///
    ///  2. <b>Salama-Venkatesh Sand Erosion Model</b> (1983 OTC-4485)
    ///       E_rate = F_p × Q_s^1.73 × d_p^0.25 / (ρ_m^0.5 × D^2)   (mpy)
    ///       where F_p = penetration factor (material-dependent), Q_s = sand rate (bbl/d),
    ///       d_p = particle diameter (micron), D = pipe ID (in).
    ///
    ///  3. <b>DNV GL RP-O501 Sand Erosion</b>
    ///       E_rate = K × U_m^n × sand_rate × (1/ρ_m)^0.5
    ///       n = 2.6 for bends; 2.0 for straight sections.
    ///
    ///  4. <b>General Corrosion Allowance</b>
    ///       CA = corrosion_rate × design_life + safety_margin
    ///       Gives minimum required wall thickness addition.
    ///
    /// Units: ft, in, psi, lb/ft³, ft/s, bbl/d, US field unless noted.
    ///
    /// References:
    ///   - API RP 14E (2007), "Offshore Production Platform Piping Systems"
    ///   - Salama, M.M. and Venkatesh, E.S. (1983) OTC-4485
    ///   - DNV GL RP-O501 (2015) "Managing Sand Production and Erosion"
    ///   - NORSOK P-002 (2020) for corrosion allowance guidance
    /// </summary>
    public static class ErosionPrediction
    {
        // ─────────────────────────────────────────────────────────────────
        //  API RP 14E C-factors
        // ─────────────────────────────────────────────────────────────────

        /// <summary>C = 100 — solid or sand-laden service (API RP 14E conservative).</summary>
        public const double C_SolidService = 100.0;
        /// <summary>C = 125 — inhibited or intermittent sand service.</summary>
        public const double C_InhibitedService = 125.0;
        /// <summary>C = 150 — clean, non-corrosive, sand-free continuous service.</summary>
        public const double C_CleanService = 150.0;

        // ─────────────────────────────────────────────────────────────────
        //  1. API RP 14E Erosional Velocity
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the API RP 14E erosional velocity and checks the operating velocity.
        ///
        ///   Ve = C / √ρ_m  (ft/s)
        ///   ρ_m = mixture density (lb/ft³) at flowing conditions.
        /// </summary>
        /// <param name="mixtureGasGravity">Gas specific gravity (air=1).</param>
        /// <param name="liquidFraction">Liquid volume fraction at pipe conditions (0–1).</param>
        /// <param name="liquidDensityLbFt3">Liquid density (lb/ft³). Typical oil ≈ 50, water ≈ 62.4.</param>
        /// <param name="operatingPressurePsia">Flowing pressure (psia) for gas density calculation.</param>
        /// <param name="operatingTempF">Flowing temperature (°F).</param>
        /// <param name="cFactor">C-factor from API RP 14E (default 100 for solid service).</param>
        /// <param name="actualVelocityFtS">Actual in-situ mixture velocity (ft/s).</param>
        /// <returns><see cref="ErosionalVelocityResult"/>.</returns>
        public static ErosionalVelocityResult CalculateErosionalVelocity(
            double mixtureGasGravity,
            double liquidFraction,
            double liquidDensityLbFt3,
            double operatingPressurePsia,
            double operatingTempF,
            double cFactor,
            double actualVelocityFtS)
        {
            double T_R = operatingTempF + 459.67;
            // Approximation: Z ≈ 1 for typical transmission pressures (add full Z-factor if available)
            double gasDensity = mixtureGasGravity * 28.97 * operatingPressurePsia
                                / (10.73 * T_R);  // lb/ft³ via ideal gas approx
            double gasFraction = 1.0 - liquidFraction;
            double mixtureDensity = gasFraction * gasDensity + liquidFraction * liquidDensityLbFt3;
            if (mixtureDensity <= 0) mixtureDensity = gasDensity;

            double erosionalVelocity = cFactor / Math.Sqrt(mixtureDensity);
            double velocityRatio     = actualVelocityFtS / erosionalVelocity;

            return new ErosionalVelocityResult
            {
                ErosionalVelocityFtS   = erosionalVelocity,
                ActualVelocityFtS      = actualVelocityFtS,
                MixtureDensityLbFt3    = mixtureDensity,
                VelocityRatio          = velocityRatio,
                IsWithinLimit          = velocityRatio <= 1.0,
                CFactor                = cFactor,
                Recommendation         = BuildVelocityRecommendation(velocityRatio),
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  2. Salama-Venkatesh Sand Erosion Model
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates sand erosion rate using the Salama-Venkatesh (1983) model.
        ///
        ///   E = F_p × Q_sand^1.73 × d_p^0.25 / (ρ_m^0.5 × D^2)   (mpy)
        ///
        /// Material penetration factors F_p:
        ///   Carbon steel: 1.0, 13Cr stainless: 0.6, Duplex SS: 0.4, Corrosion-resistant alloy: 0.2
        /// </summary>
        /// <param name="sandRateBblPerDay">Sand production rate (bbl/d).</param>
        /// <param name="particleDiameterMicron">Median sand particle diameter (µm). Typical 100–400 µm.</param>
        /// <param name="mixtureDensityLbFt3">Mixture density at pipe conditions (lb/ft³).</param>
        /// <param name="pipeIdInches">Pipe internal diameter (in).</param>
        /// <param name="materialFactor">Penetration factor F_p (1.0 for carbon steel).</param>
        /// <returns>Sand erosion rate in mils per year (mpy).</returns>
        public static SandErosionResult CalculateSalamaErosionRate(
            double sandRateBblPerDay,
            double particleDiameterMicron,
            double mixtureDensityLbFt3,
            double pipeIdInches,
            double materialFactor = 1.0)
        {
            if (mixtureDensityLbFt3 <= 0) throw new ArgumentOutOfRangeException(nameof(mixtureDensityLbFt3));
            if (pipeIdInches <= 0)        throw new ArgumentOutOfRangeException(nameof(pipeIdInches));

            double erosionRateMpy = materialFactor
                * Math.Pow(sandRateBblPerDay, 1.73)
                * Math.Pow(particleDiameterMicron, 0.25)
                / (Math.Sqrt(mixtureDensityLbFt3) * pipeIdInches * pipeIdInches);

            return new SandErosionResult
            {
                ErosionRateMpy           = erosionRateMpy,
                ErosionRateMmPerYear     = erosionRateMpy * 0.0254,
                SandRateBblPerDay        = sandRateBblPerDay,
                ParticleDiameterMicron   = particleDiameterMicron,
                MixtureDensityLbFt3      = mixtureDensityLbFt3,
                PipeIdInches             = pipeIdInches,
                MaterialFactor           = materialFactor,
                SeverityClassification   = ClassifyErosionSeverity(erosionRateMpy),
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  3. DNV GL RP-O501 Erosion Rate
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates erosion using the DNV GL RP-O501 model.
        ///
        ///   E = K × U_m^n × (sand_rate / ρ_m^0.5)
        ///   n = 2.6 for bends/elbows, 2.0 for straight pipe sections.
        ///   K = geometry constant (typical: 2×10⁻⁹ for bends, 1×10⁻⁹ for straight).
        /// </summary>
        /// <param name="mixtureVelocityFtS">Mixture velocity (ft/s).</param>
        /// <param name="sandMassRateLbPerSec">Sand mass flow rate (lb/s).</param>
        /// <param name="mixtureDensityLbFt3">Mixture density (lb/ft³).</param>
        /// <param name="isBend">True for bend/elbow geometry; false for straight pipe.</param>
        /// <returns>Erosion rate (mm/year).</returns>
        public static double CalculateDnvErosionRate(
            double mixtureVelocityFtS,
            double sandMassRateLbPerSec,
            double mixtureDensityLbFt3,
            bool isBend = true)
        {
            double K = isBend ? 2e-9 : 1e-9;
            double n = isBend ? 2.6 : 2.0;
            double erosionMmS = K * Math.Pow(mixtureVelocityFtS, n)
                                * sandMassRateLbPerSec / Math.Sqrt(mixtureDensityLbFt3);
            return erosionMmS * 3.1536e7;  // convert s⁻¹ → year⁻¹
        }

        // ─────────────────────────────────────────────────────────────────
        //  4. Corrosion Allowance
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates required corrosion allowance (CA) for a pipeline.
        ///
        ///   CA = corrosion_rate (mpy) × design_life_years × 0.001  (inches)
        ///      + safety_margin (in)
        ///
        /// Typical corrosion rates:
        ///   Protected / inhibited:  1–3 mpy
        ///   Moderately corrosive:   5–10 mpy
        ///   Aggressive H₂S/CO₂:    15–30 mpy
        /// </summary>
        public static CorrosionAllowanceResult CalculateCorrosionAllowance(
            double corrosionRateMpy,
            double designLifeYears,
            double safetyMarginInches = 0.050)
        {
            double caInches = corrosionRateMpy * designLifeYears * 0.001 + safetyMarginInches;
            double caMm     = caInches * 25.4;

            return new CorrosionAllowanceResult
            {
                CorrosionRateMpy          = corrosionRateMpy,
                DesignLifeYears           = designLifeYears,
                SafetyMarginInches        = safetyMarginInches,
                RequiredAllowanceInches   = caInches,
                RequiredAllowanceMm       = caMm,
                CorrosionSeverity         = ClassifyCorrosionSeverity(corrosionRateMpy),
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  5. Comprehensive pipeline erosion / corrosion report
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a comprehensive erosion and corrosion assessment for a pipeline segment.
        /// Combines API RP 14E, Salama-Venkatesh, and corrosion allowance into one report.
        /// </summary>
        public static PipelineErosionReport GenerateComprehensiveReport(
            double pipeIdInches,
            double actualVelocityFtS,
            double mixtureDensityLbFt3,
            double sandRateBblPerDay,
            double particleDiameterMicron,
            double corrosionRateMpy,
            double designLifeYears,
            double gasFraction,
            double cFactor = C_SolidService,
            double materialFactor = 1.0)
        {
            var erosionalVelocityResult = CalculateErosionalVelocity(
                mixtureGasGravity: 0.65,
                liquidFraction: 1.0 - gasFraction,
                liquidDensityLbFt3: 55.0,
                operatingPressurePsia: 500,
                operatingTempF: 100,
                cFactor: cFactor,
                actualVelocityFtS: actualVelocityFtS);

            // Override with actual density if provided
            erosionalVelocityResult = new ErosionalVelocityResult
            {
                ErosionalVelocityFtS  = cFactor / Math.Sqrt(mixtureDensityLbFt3),
                ActualVelocityFtS     = actualVelocityFtS,
                MixtureDensityLbFt3   = mixtureDensityLbFt3,
                VelocityRatio         = actualVelocityFtS / (cFactor / Math.Sqrt(mixtureDensityLbFt3)),
                IsWithinLimit         = actualVelocityFtS <= cFactor / Math.Sqrt(mixtureDensityLbFt3),
                CFactor               = cFactor,
                Recommendation        = BuildVelocityRecommendation(
                                            actualVelocityFtS / (cFactor / Math.Sqrt(mixtureDensityLbFt3))),
            };

            var sandErosion = sandRateBblPerDay > 0
                ? CalculateSalamaErosionRate(sandRateBblPerDay, particleDiameterMicron,
                    mixtureDensityLbFt3, pipeIdInches, materialFactor)
                : null;

            var corrosionAllowance = CalculateCorrosionAllowance(corrosionRateMpy, designLifeYears);

            bool overallSafe = erosionalVelocityResult.IsWithinLimit
                && (sandErosion == null || sandErosion.ErosionRateMpy < 10.0);

            return new PipelineErosionReport
            {
                ErosionalVelocity  = erosionalVelocityResult,
                SandErosion        = sandErosion,
                CorrosionAllowance = corrosionAllowance,
                OverallRisk        = overallSafe ? "LOW" : "HIGH",
                OverallRecommendation = overallSafe
                    ? "Pipeline operating within acceptable limits."
                    : "Velocity or sand erosion exceeds limits — review pipe diameter or production rate.",
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────────

        private static string BuildVelocityRecommendation(double ratio)
        {
            if (ratio <= 0.80) return "Operating well below erosional velocity — safe.";
            if (ratio <= 1.00) return "Approaching erosional limit — monitor closely.";
            if (ratio <= 1.20) return "Exceeds erosional limit by <20% — reduce velocity or upgrade material.";
            return $"Velocity ratio {ratio:F2} — immediate remediation required.";
        }

        private static string ClassifyErosionSeverity(double mpy)
        {
            if (mpy < 1.0)  return "Negligible";
            if (mpy < 5.0)  return "Low";
            if (mpy < 10.0) return "Moderate";
            if (mpy < 20.0) return "High";
            return "Severe";
        }

        private static string ClassifyCorrosionSeverity(double mpy)
        {
            if (mpy < 2.0)  return "Non-corrosive";
            if (mpy < 5.0)  return "Mildly corrosive";
            if (mpy < 10.0) return "Moderately corrosive";
            if (mpy < 20.0) return "Highly corrosive";
            return "Severely corrosive";
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Result of API RP 14E erosional velocity check.</summary>
        public sealed class ErosionalVelocityResult
        {
            public double ErosionalVelocityFtS  { get; set; }
            public double ActualVelocityFtS     { get; set; }
            public double MixtureDensityLbFt3   { get; set; }
            public double VelocityRatio         { get; set; }
            public bool   IsWithinLimit         { get; set; }
            public double CFactor               { get; set; }
            public string Recommendation        { get; set; } = string.Empty;
        }

        /// <summary>Sand erosion prediction result.</summary>
        public sealed class SandErosionResult
        {
            public double ErosionRateMpy         { get; set; }
            public double ErosionRateMmPerYear    { get; set; }
            public double SandRateBblPerDay       { get; set; }
            public double ParticleDiameterMicron  { get; set; }
            public double MixtureDensityLbFt3     { get; set; }
            public double PipeIdInches            { get; set; }
            public double MaterialFactor          { get; set; }
            public string SeverityClassification  { get; set; } = string.Empty;
        }

        /// <summary>Corrosion allowance sizing result.</summary>
        public sealed class CorrosionAllowanceResult
        {
            public double CorrosionRateMpy         { get; set; }
            public double DesignLifeYears          { get; set; }
            public double SafetyMarginInches       { get; set; }
            public double RequiredAllowanceInches  { get; set; }
            public double RequiredAllowanceMm      { get; set; }
            public string CorrosionSeverity        { get; set; } = string.Empty;
        }

        /// <summary>Comprehensive pipeline erosion and corrosion report.</summary>
        public sealed class PipelineErosionReport
        {
            public ErosionalVelocityResult  ErosionalVelocity  { get; set; } = new();
            public SandErosionResult?       SandErosion        { get; set; }
            public CorrosionAllowanceResult CorrosionAllowance { get; set; } = new();
            public string OverallRisk           { get; set; } = string.Empty;
            public string OverallRecommendation { get; set; } = string.Empty;
        }
    }
}
