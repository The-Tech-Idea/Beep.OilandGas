namespace Beep.OilandGas.Decommissioning.Constants
{
    /// <summary>
    /// Default cost parameters for decommissioning parametric estimates.
    /// These are US Gulf of Mexico onshore defaults. For production use,
    /// load jurisdiction-specific rates from PPDM reference data or config.
    ///
    /// All costs in USD (2026 basis).
    /// </summary>
    public static class DecommissioningCostDefaults
    {
        /// <summary>Base well plugging and abandonment cost per well.</summary>
        public const decimal WellAbandonmentRate = 250_000m;

        /// <summary>Base facility decommissioning cost per facility.</summary>
        public const decimal FacilityDecommissioningRate = 500_000m;

        /// <summary>Base site restoration cost per well location.</summary>
        public const decimal SiteRestorationRate = 50_000m;

        /// <summary>Base environmental assessment and remediation cost.</summary>
        public const decimal EnvironmentalBaseRate = 100_000m;

        /// <summary>Contingency percentage applied to subtotal (20%).</summary>
        public const decimal ContingencyPercent = 0.20m;

        /// <summary>Wellhead removal — onshore.</summary>
        public const decimal WellheadRemovalOnshore = 35_000m;

        /// <summary>Wellhead removal — offshore (platform).</summary>
        public const decimal WellheadRemovalOffshore = 250_000m;

        /// <summary>Default casing/cement plugging cost per foot.</summary>
        public const decimal PluggingCostPerFoot = 150m;

        /// <summary>Deep-well plugging cost per foot (depth &gt; 10,000 ft).</summary>
        public const decimal PluggingCostPerFootDeep = 180m;

        /// <summary>Default depth threshold for deep-well rate (feet).</summary>
        public const decimal DeepWellDepthThreshold = 10_000m;
    }
}
