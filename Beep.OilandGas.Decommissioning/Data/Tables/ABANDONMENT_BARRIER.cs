using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    /// <summary>
    /// One barrier of a well's plug-and-abandonment programme (DEC-02), with the evidence that verifies it.
    /// </summary>
    /// <remarks>
    /// The owner held no barrier record: the abandonment status table carries the state and the plug-back depth, and the
    /// barrier set was only ever computed in memory from a schematic. A barrier that cannot be recorded cannot be verified, so
    /// this table is the plan's own contract (DEC-02's <c>OgAbandonmentBarrier</c>) implemented in the module that owns
    /// decommissioning.
    /// </remarks>
    public partial class ABANDONMENT_BARRIER : ModelEntityBase
    {
        [Key, MaxLength(128)]
        public string ABANDONMENT_BARRIER_ID { get; set; } = string.Empty;

        /// <summary>The abandonment record this barrier belongs to (the well programme).</summary>
        [MaxLength(128)]
        public string WELL_ABANDONMENT_ID { get; set; } = string.Empty;

        /// <summary>The well, so a barrier can be listed without the programme.</summary>
        [MaxLength(64)]
        public string? UWI { get; set; }

        /// <summary>The envelope the owner's template requires it in: Primary, Secondary or Environmental.</summary>
        [MaxLength(32)]
        public string ENVELOPE { get; set; } = string.Empty;

        /// <summary>The template element type it satisfies (for example CEMENT_PLUG).</summary>
        [MaxLength(64)]
        public string BARRIER_TYPE { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? DESCRIPTION { get; set; }

        [Column(TypeName = "REAL")]
        public decimal? TOP_DEPTH { get; set; }

        [Column(TypeName = "REAL")]
        public decimal? BASE_DEPTH { get; set; }

        /// <summary>The unit the two depths are in, because a depth without its unit is not a depth (DEC-02 AC-4).</summary>
        [MaxLength(16)]
        public string? DEPTH_UOM { get; set; }

        /// <summary>How the barrier is verified (for example pressure test, tag, cement bond log).</summary>
        [MaxLength(128)]
        public string? VERIFICATION_METHOD { get; set; }

        /// <summary>The evidence reference - a report, a log, a ticket. Required before the barrier counts as verified.</summary>
        [MaxLength(256)]
        public string? EVIDENCE_REF { get; set; }

        [MaxLength(128)]
        public string? VERIFIED_BY { get; set; }

        public DateTime? VERIFIED_DATE { get; set; }

        /// <summary>Y when the barrier was executed and verified; N while it is planned.</summary>
        [MaxLength(1)]
        public string? VERIFIED_IND { get; set; }
    }
}
