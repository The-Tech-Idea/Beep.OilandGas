using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// One saved anti-collision run (ENG-01, AC-6): what was scanned, with which settings, and what came out.
/// </summary>
/// <remarks>
/// The owner keeps surveys and the separation rule but no record of a scan having been made, so a run - and with it the error
/// model, the sigma and the environment it was made under - had nowhere to live. This table is the plan's own contract for that
/// evidence, and it stores the settings as they were at the time rather than as they are now.
/// </remarks>
public partial class ANTICOLLISION_RUN : ModelEntityBase
{
    [Key, MaxLength(128)]
    public string ANTICOLLISION_RUN_ID { get; set; } = string.Empty;

    /// <summary>The reference well's UWI.</summary>
    [Required, MaxLength(64)]
    public string REFERENCE_WELL { get; set; } = string.Empty;

    /// <summary>The offsets that were scanned, comma separated.</summary>
    [MaxLength(1000)]
    public string? OFFSET_WELLS { get; set; }

    /// <summary>The offsets that were left out because they had no survey, comma separated.</summary>
    [MaxLength(1000)]
    public string? EXCLUDED_WELLS { get; set; }

    /// <summary>The error model the uncertainty came from.</summary>
    [Required, MaxLength(128)]
    public string ERROR_MODEL { get; set; } = string.Empty;

    /// <summary>The sigma the uncertainty is quoted at (1 = one standard deviation).</summary>
    public decimal SIGMA { get; set; } = 1m;

    [Column(TypeName = "REAL")]
    public decimal SCALING_FACTOR { get; set; }

    [Column(TypeName = "REAL")]
    public decimal SURFACE_MARGIN { get; set; }

    [Column(TypeName = "REAL")]
    public decimal PROJECT_AHEAD { get; set; }

    [Column(TypeName = "REAL")]
    public decimal SCAN_STEP { get; set; }

    /// <summary>The environment the rule and units came from, so a run can be reproduced.</summary>
    [Required, MaxLength(128)]
    public string ENVIRONMENT { get; set; } = string.Empty;

    public int STATION_COUNT { get; set; }

    public int VIOLATION_COUNT { get; set; }

    /// <summary>The levels and settings, as JSON, exactly as they were applied.</summary>
    [MaxLength(4000)]
    public string? SETTINGS_JSON { get; set; }

    [MaxLength(128)]
    public string RUN_BY { get; set; } = string.Empty;

    public DateTime RUN_AT_UTC { get; set; }
}
