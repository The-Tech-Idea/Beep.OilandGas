using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    /// <summary>
    /// One revision of a facility decommissioning cost estimate (DEC-03).
    /// </summary>
    /// <remarks>
    /// The owner held the facility's estimated and actual cost on <c>DECOMMISSIONING_STATUS</c>, which can only ever hold the
    /// current figure. A revision must not mutate the version it replaces (DEC-03 AC-2), so each revision is its own record
    /// and the status row's figures are the current revision's, not the only copy.
    /// </remarks>
    public partial class DECOMMISSIONING_COST_REVISION : ModelEntityBase
    {
        [Key, MaxLength(128)]
        public string DECOMMISSIONING_COST_REVISION_ID { get; set; } = string.Empty;

        /// <summary>The facility decommissioning record the estimate belongs to.</summary>
        [MaxLength(128)]
        public string FACILITY_DECOMMISSIONING_ID { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? FACILITY_ID { get; set; }

        [MaxLength(128)]
        public string? FIELD_ID { get; set; }

        /// <summary>1 for the first estimate, then the next number - never reused.</summary>
        public decimal REVISION_NO { get; set; }

        /// <summary>The revision this one replaces, so the chain can be walked back.</summary>
        [MaxLength(128)]
        public string? PREVIOUS_REVISION_ID { get; set; }

        /// <summary>Why this revision exists. Required from the second revision on.</summary>
        [MaxLength(512)]
        public string? REASON { get; set; }

        /// <summary>The line items, as a JSON array of {category, amount} - the base cost is their sum.</summary>
        [MaxLength(4000)]
        public string? LINES_JSON { get; set; }

        [Column(TypeName = "REAL")]
        public decimal BASE_COST { get; set; }

        /// <summary>Annual escalation as a fraction (0.03 is 3 % a year).</summary>
        [Column(TypeName = "REAL")]
        public decimal ESCALATION_RATE { get; set; }

        /// <summary>How many years the escalation is applied for. Zero means no escalation basis.</summary>
        public decimal ESCALATION_YEARS { get; set; }

        /// <summary>Contingency as a percentage of the escalated base (10 is 10 %).</summary>
        [Column(TypeName = "REAL")]
        public decimal CONTINGENCY_PCT { get; set; }

        [Column(TypeName = "REAL")]
        public decimal TOTAL_COST { get; set; }

        [MaxLength(8)]
        public string CURRENCY { get; set; } = string.Empty;

        /// <summary>DRAFT or APPROVED.</summary>
        [MaxLength(16)]
        public string STATUS { get; set; } = "DRAFT";

        [MaxLength(128)]
        public string? AUTHOR { get; set; }

        [MaxLength(128)]
        public string? APPROVED_BY { get; set; }

        public DateTime? APPROVED_DATE { get; set; }

        /// <summary>The conditions the approval was given under (DEC-03 AC-6).</summary>
        [MaxLength(1000)]
        public string? APPROVAL_CONDITIONS { get; set; }
    }
}
