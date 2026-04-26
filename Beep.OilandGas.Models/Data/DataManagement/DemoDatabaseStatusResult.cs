using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Typed status snapshot for a demo database — covers schema completeness, seeding
    /// stage outcomes, and retention state.
    /// Returned by <c>IDemoDatabaseService.GetDemoDatabaseStatusAsync</c>.
    /// </summary>
    public class DemoDatabaseStatusResult : ModelEntityBase
    {
        /// <summary>Whether a demo database with this connection name exists in the metadata store.</summary>
        public bool Exists { get; set; }

        /// <summary>Connection name of the demo database.</summary>
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>Whether schema migration completed successfully.</summary>
        public bool SchemaComplete { get; set; }

        /// <summary>Migration plan ID that produced the schema, if available.</summary>
        public string SchemaPlanId { get; set; } = string.Empty;

        /// <summary>Whether all required seed stages have been marked complete.</summary>
        public bool SeedingComplete { get; set; }

        /// <summary>Per-stage seed outcomes.  Populated when status was recorded during creation.</summary>
        public List<DemoSeedStageResult> SeedStages { get; set; } = new();

        /// <summary>The seed option chosen at creation time (none / reference-only / reference-sample / full-demo).</summary>
        public string SeedDataOption { get; set; } = string.Empty;

        /// <summary>UTC timestamp when the database was created.</summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>UTC timestamp after which the database is eligible for cleanup.</summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>Whether the database has passed its expiry date.</summary>
        public bool IsExpired { get; set; }
    }
}
