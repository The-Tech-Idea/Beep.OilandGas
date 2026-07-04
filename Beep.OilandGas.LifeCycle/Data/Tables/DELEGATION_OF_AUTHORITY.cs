using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Delegation of Authority — financial threshold-based approval routing.
/// Defines approval levels triggered by entity field values (e.g., AFE amount > $500K requires Executive approval).
/// Part of Phase 2 workflow engine enhancement.
/// </summary>
public class DELEGATION_OF_AUTHORITY : ModelEntityBase
{
    [Key]
    public string DOA_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable name for this DOA rule set, e.g. "AFE Standard Approval Limits".</summary>
    public string DOA_NAME { get; set; } = string.Empty;

    /// <summary>Entity type this rule applies to, e.g. "AFE", "COST_TRANSACTION".</summary>
    public string ENTITY_TYPE { get; set; } = string.Empty;

    /// <summary>Entity field to evaluate, e.g. "ESTIMATED_COST", "AMOUNT".</summary>
    public string FIELD_NAME { get; set; } = string.Empty;

    /// <summary>Operator: GREATER_THAN, LESS_THAN, GREATER_THAN_OR_EQUAL, LESS_THAN_OR_EQUAL, BETWEEN.</summary>
    public string COMPARISON_OPERATOR { get; set; } = "GREATER_THAN";

    /// <summary>The threshold value that triggers this approval level.</summary>
    public decimal THRESHOLD_VALUE { get; set; }

    /// <summary>Upper bound for BETWEEN operator.</summary>
    public decimal? THRESHOLD_VALUE_MAX { get; set; }

    /// <summary>ISO 4217 currency code, e.g. "USD".</summary>
    public string? CURRENCY_CODE { get; set; }

    /// <summary>Approval level identifier: LEVEL_1 through LEVEL_5.</summary>
    public string APPROVAL_LEVEL { get; set; } = "LEVEL_1";

    /// <summary>Role required to approve at this level.</summary>
    public string REQUIRED_ROLE { get; set; } = string.Empty;

    /// <summary>Position in the approval chain (1 = first approver).</summary>
    public int APPROVAL_SEQUENCE { get; set; } = 1;

    /// <summary>When true, ALL approvers at this level must approve (not just one).</summary>
    public string REQUIRES_UNANIMOUS { get; set; } = "N";

    /// <summary>Backup role if primary approver is unavailable.</summary>
    public string? ESCALATION_ROLE { get; set; }

    /// <summary>Hours before auto-escalation to backup role.</summary>
    public int? ESCALATION_HOURS { get; set; }

    /// <summary>Which process type this DOA applies to, e.g. "AFE_APPROVAL".</summary>
    public string PROCESS_TYPE { get; set; } = string.Empty;

    /// <summary>Optional notes on regulatory or policy basis for this threshold.</summary>
    public string? NOTES { get; set; }
}
