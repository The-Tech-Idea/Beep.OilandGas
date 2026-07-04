using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Defines a Segregation of Duties (SoD) rule — two permissions that must not be held
/// by the same user. Critical for SOX, SEC, and ISO 27001 compliance in oil & gas operations.
/// Part of Phase 4 governance & compliance.
/// </summary>
public class SOD_RULE : ModelEntityBase
{
    [Key]
    public string SOD_RULE_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable rule name, e.g. "AFE_CREATE_APPROVE".</summary>
    public string RULE_NAME { get; set; } = string.Empty;

    /// <summary>Category: FINANCIAL, OPERATIONAL, SAFETY, SECURITY.</summary>
    public string RULE_CATEGORY { get; set; } = "FINANCIAL";

    /// <summary>First conflicting permission code.</summary>
    public string CONFLICTING_PERMISSION_A { get; set; } = string.Empty;

    /// <summary>Second conflicting permission code.</summary>
    public string CONFLICTING_PERMISSION_B { get; set; } = string.Empty;

    /// <summary>Human-readable explanation of why this combination is prohibited.</summary>
    public string CONFLICT_DESCRIPTION { get; set; } = string.Empty;

    /// <summary>Severity: CRITICAL, HIGH, MEDIUM, LOW.</summary>
    public string SEVERITY { get; set; } = "HIGH";

    /// <summary>Regulatory reference, e.g. "SOX 404", "SEC Rule 13b2-2", "ISO 27001 A.9.2.3".</summary>
    public string? REGULATION_REFERENCE { get; set; }

    /// <summary>When true, the system blocks role assignment. When false, warning only.</summary>
    public string IS_BLOCKING { get; set; } = "Y";

    /// <summary>Scope: GLOBAL, FIELD, ENTITY.</summary>
    public string SCOPE_TYPE { get; set; } = "GLOBAL";

    /// <summary>Guidance on how to mitigate if this conflict is unavoidable.</summary>
    public string? MITIGATION_GUIDANCE { get; set; }
}
