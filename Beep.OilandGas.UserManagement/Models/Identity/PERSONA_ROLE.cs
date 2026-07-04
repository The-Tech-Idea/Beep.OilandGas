using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

/// <summary>
/// Explicit bridge between the Persona system (UI/UX layer) and the Role system (API authorization layer).
/// When a user selects a persona, the roles mapped here become their effective permissions.
/// </summary>
public class PERSONA_ROLE : ModelEntityBase
{
    [Key]
    public string PERSONA_ROLE_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → PERSONA_DEFINITION.PERSONA_ID</summary>
    public string PERSONA_ID { get; set; } = string.Empty;

    /// <summary>Denormalized for fast lookup without JOIN</summary>
    public string PERSONA_CODE { get; set; } = string.Empty;

    /// <summary>FK → ROLE.ROLE_ID (PPDM) or AppRole.ROLE_ID</summary>
    public string ROLE_ID { get; set; } = string.Empty;

    /// <summary>Denormalized for fast lookup without JOIN</summary>
    public string ROLE_NAME { get; set; } = string.Empty;

    /// <summary>When true, this is the primary role for the persona. A persona must have at least one primary role.</summary>
    public string IS_PRIMARY { get; set; } = "N";

    /// <summary>Order in which roles are applied when resolving permissions (lower = first).</summary>
    public int PRIORITY { get; set; }

    /// <summary>Scope constraint: "GLOBAL", "FIELD", or "ASSET". When FIELD, the role is limited to the user's assigned fields.</summary>
    public string? EFFECTIVE_SCOPE { get; set; }
}
