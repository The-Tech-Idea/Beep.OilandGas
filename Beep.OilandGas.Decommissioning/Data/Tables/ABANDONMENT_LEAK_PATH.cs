using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    /// <summary>
    /// One identified leak path of a well's abandonment (DEC-02), and whether it has been addressed.
    /// </summary>
    /// <remarks>
    /// The owner held no leak-path record either, so this is the plan's own contract: a leak path that cannot be recorded
    /// cannot be shown to be addressed, and completion would have nothing to refuse on.
    /// </remarks>
    public partial class ABANDONMENT_LEAK_PATH : ModelEntityBase
    {
        [Key, MaxLength(128)]
        public string ABANDONMENT_LEAK_PATH_ID { get; set; } = string.Empty;

        [MaxLength(128)]
        public string WELL_ABANDONMENT_ID { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? UWI { get; set; }

        [MaxLength(512)]
        public string DESCRIPTION { get; set; } = string.Empty;

        /// <summary>OPEN or ADDRESSED.</summary>
        [MaxLength(32)]
        public string STATUS { get; set; } = "OPEN";

        [MaxLength(512)]
        public string? ADDRESSED_HOW { get; set; }

        [MaxLength(128)]
        public string? ADDRESSED_BY { get; set; }

        public DateTime? ADDRESSED_DATE { get; set; }
    }
}
