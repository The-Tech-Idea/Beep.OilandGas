namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Records the outcome of a single seed stage during demo database initialisation.
    /// Returned as part of <see cref="DemoDatabaseStatusResult"/>.
    /// </summary>
    public class DemoSeedStageResult : ModelEntityBase
    {
        /// <summary>Logical name of the stage, e.g. "reference-data", "well-status-facets", "demo-data".</summary>
        public string Stage { get; set; } = string.Empty;

        /// <summary>Whether this stage must succeed before the database is considered ready.</summary>
        public bool IsRequired { get; set; }

        /// <summary>Whether the stage completed without error.</summary>
        public bool Success { get; set; }

        /// <summary>Number of rows inserted (idempotent runs may report 0 for already-seeded stages).</summary>
        public int RecordsInserted { get; set; }

        /// <summary>Human-readable outcome or error summary.</summary>
        public string Message { get; set; } = string.Empty;
    }
}
