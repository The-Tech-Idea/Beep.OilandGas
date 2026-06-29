namespace Beep.OilandGas.DevelopmentPlanning.Constants
{
    /// <summary>
    /// Default parameters for development planning analysis.
    /// These are screening-level defaults. For production use, load
    /// field-specific values from PPDM reference data or configuration.
    /// </summary>
    public static class DevelopmentPlanningDefaults
    {
        // ── GHG Emissions ──────────────────────────────────────────────────
        /// <summary>Default GHG emissions estimate (tCO2e/year) when measured data is unavailable.</summary>
        public const double DefaultGHGEmissionsTCO2e = 10000;

        // ── Equipment ──────────────────────────────────────────────────────
        /// <summary>Default equipment reliability (fraction) when measured data is unavailable.</summary>
        public const double DefaultEquipmentReliability = 0.98;

        // ── Cost Factors ───────────────────────────────────────────────────
        /// <summary>Completion cost as fraction of drilling cost (rule of thumb).</summary>
        public const double CompletionCostFraction = 0.30;

        /// <summary>Default contingency factor applied to cost estimates.</summary>
        public const double DefaultContingencyFactor = 0.15;

        // ── Environmental ──────────────────────────────────────────────────
        /// <summary>Default mitigation cost per measure ($ million).</summary>
        public const double MitigationCostPerMeasureMillion = 2.5;

        // ── Drilling Program ───────────────────────────────────────────────
        /// <summary>Default rig operating cost ($/day).</summary>
        public const double DefaultRigDayRate = 75_000;

        /// <summary>Default drilling rate (ft/day).</summary>
        public const double DefaultDrillingRateFtPerDay = 150;

        // ── Production ─────────────────────────────────────────────────────
        /// <summary>Default decline rate for production forecasting (fraction/year).</summary>
        public const double DefaultAnnualDeclineRate = 0.10;

        // ── Facility ──────────────────────────────────────────────────────
        /// <summary>Production threshold for large facility sizing (bbl/d).</summary>
        public const double LargeFacilityThresholdBblPerDay = 100_000;

        /// <summary>Large facility pipe diameter (inches).</summary>
        public const int LargeFacilityPipeDiameterInches = 16;

        /// <summary>Standard facility pipe diameter (inches).</summary>
        public const int StandardFacilityPipeDiameterInches = 12;

        // ── Reserves ───────────────────────────────────────────────────────
        /// <summary>Large reserves threshold (MMBOE) for phase count.</summary>
        public const double LargeReservesThresholdMmboe = 1000;

        /// <summary>Medium reserves threshold (MMBOE).</summary>
        public const double MediumReservesThresholdMmboe = 500;

        /// <summary>Small reserves threshold (MMBOE).</summary>
        public const double SmallReservesThresholdMmboe = 100;
    }
}
