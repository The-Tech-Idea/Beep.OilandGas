using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Documents a compensating control that mitigates a Segregation of Duties violation.
/// Compensating controls are time-bound (max 90 days), require independent approval,
/// and must be regularly reviewed. Required by SOX auditors for any SoD exception.
/// Part of Phase 4 governance & compliance.
/// </summary>
public class COMPENSATING_CONTROL : ModelEntityBase
{
    [Key]
    public string CONTROL_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → SOD_CONFLICT.SOD_CONFLICT_ID.</summary>
    public string SOD_CONFLICT_ID { get; set; } = string.Empty;

    /// <summary>FK → USER.USER_ID — user covered by this compensating control.</summary>
    public string USER_ID { get; set; } = string.Empty;

    /// <summary>Type: MANAGER_REVIEW, AUDIT_LOG_REVIEW, DUAL_APPROVAL, SUPERVISOR_OVERSIGHT.</summary>
    public string CONTROL_TYPE { get; set; } = "MANAGER_REVIEW";

    /// <summary>Detailed description of the compensating control.</summary>
    public string CONTROL_DESCRIPTION { get; set; } = string.Empty;

    /// <summary>Who approved this exception.</summary>
    public string APPROVED_BY { get; set; } = string.Empty;

    /// <summary>When the exception was approved.</summary>
    public DateTime APPROVED_DATE { get; set; } = DateTime.UtcNow;

    /// <summary>When the control becomes effective.</summary>
    public DateTime EFFECTIVE_FROM { get; set; } = DateTime.UtcNow;

    /// <summary>When the control expires (max 90 days from effective date).</summary>
    public DateTime EFFECTIVE_TO { get; set; } = DateTime.UtcNow.AddDays(90);

    /// <summary>How often the control must be reviewed: WEEKLY, MONTHLY, QUARTERLY.</summary>
    public string REVIEW_FREQUENCY { get; set; } = "MONTHLY";

    /// <summary>When the control was last reviewed.</summary>
    public DateTime? LAST_REVIEWED_DATE { get; set; }

    /// <summary>Who performed the last review.</summary>
    public string? LAST_REVIEWED_BY { get; set; }

    /// <summary>Status: ACTIVE, EXPIRED, REVOKED.</summary>
    public string STATUS { get; set; } = "ACTIVE";

    /// <summary>Reference to the approval process instance that authorized this control.</summary>
    public string? APPROVAL_PROCESS_INSTANCE_ID { get; set; }
}
