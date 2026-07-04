using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

/// <summary>
/// Time-bound temporary role elevation for acting-manager, leave coverage, or emergency access scenarios.
/// Auto-expires at EFFECTIVE_TO. All elevations are audit-logged.
/// </summary>
public class TEMP_ROLE_ELEVATION : ModelEntityBase
{
    [Key]
    public string ELEVATION_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → USER.USER_ID — the user receiving temporary elevation.</summary>
    public string USER_ID { get; set; } = string.Empty;

    /// <summary>Denormalized for display.</summary>
    public string? USER_NAME { get; set; }

    /// <summary>FK → ROLE.ROLE_ID — the temporary role being granted.</summary>
    public string ELEVATED_ROLE_ID { get; set; } = string.Empty;

    /// <summary>Denormalized for display.</summary>
    public string ELEVATED_ROLE_NAME { get; set; } = string.Empty;

    /// <summary>FK → ROLE.ROLE_ID — the user's normal/base role (for audit comparison).</summary>
    public string? BASE_ROLE_ID { get; set; }

    /// <summary>When the elevation becomes active.</summary>
    public DateTime EFFECTIVE_FROM { get; set; } = DateTime.UtcNow;

    /// <summary>When the elevation automatically expires. Required — no permanent elevations.</summary>
    public DateTime EFFECTIVE_TO { get; set; } = DateTime.UtcNow.AddDays(14);

    /// <summary>Business justification (required).</summary>
    public string REASON { get; set; } = string.Empty;

    /// <summary>FK → USER.USER_ID — who requested/approved this elevation.</summary>
    public string? REQUESTED_BY { get; set; }

    /// <summary>ACTIVE, EXPIRED, REVOKED, REJECTED.</summary>
    public string STATUS { get; set; } = "ACTIVE";

    /// <summary>Optional: limit the elevation to specific fields. Comma-separated FIELD_IDs or "*" for all.</summary>
    public string? SCOPE_LIMITATION { get; set; }

    /// <summary>When the elevation was revoked (if STATUS=REVOKED).</summary>
    public DateTime? REVOKED_AT { get; set; }

    /// <summary>FK → USER.USER_ID — who revoked the elevation.</summary>
    public string? REVOKED_BY { get; set; }

    /// <summary>Reason for early revocation.</summary>
    public string? REVOKED_REASON { get; set; }

    /// <summary>Reference to the PROCESS_INSTANCE_ID of the approval workflow that authorized this elevation.</summary>
    public string? APPROVAL_PROCESS_INSTANCE_ID { get; set; }
}
