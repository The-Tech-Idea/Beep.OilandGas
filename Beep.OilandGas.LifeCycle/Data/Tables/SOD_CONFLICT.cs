using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Records a detected Segregation of Duties conflict for a specific user.
/// Created by SodEvaluationEngine when a user's permission set triggers an SoD rule.
/// Part of Phase 4 governance & compliance.
/// </summary>
public class SOD_CONFLICT : ModelEntityBase
{
    [Key]
    public string SOD_CONFLICT_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → SOD_RULE.SOD_RULE_ID.</summary>
    public string SOD_RULE_ID { get; set; } = string.Empty;

    /// <summary>Denormalized for reporting.</summary>
    public string RULE_NAME { get; set; } = string.Empty;

    /// <summary>FK → USER.USER_ID — the user with the conflict.</summary>
    public string USER_ID { get; set; } = string.Empty;

    /// <summary>The role that holds permission A.</summary>
    public string? ROLE_A { get; set; }

    /// <summary>The role that holds permission B.</summary>
    public string? ROLE_B { get; set; }

    /// <summary>When the conflict was detected.</summary>
    public DateTime DETECTED_DATE { get; set; } = DateTime.UtcNow;

    /// <summary>Status: ACTIVE, MITIGATED, RESOLVED, WAIVED.</summary>
    public string CONFLICT_STATUS { get; set; } = "ACTIVE";

    /// <summary>FK → COMPENSATING_CONTROL.CONTROL_ID if mitigated.</summary>
    public string? COMPENSATING_CONTROL_ID { get; set; }

    /// <summary>When the conflict was resolved (role removed or compensating control applied).</summary>
    public DateTime? RESOLVED_DATE { get; set; }

    /// <summary>Who resolved it.</summary>
    public string? RESOLVED_BY { get; set; }

    /// <summary>Resolution notes.</summary>
    public string? RESOLUTION_NOTES { get; set; }
}
