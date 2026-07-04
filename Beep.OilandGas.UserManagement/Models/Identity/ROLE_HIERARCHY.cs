using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

/// <summary>
/// Defines parent-child relationships between roles for permission inheritance.
/// A child role inherits all permissions of its parent role(s).
/// Supports selective inheritance (only specific domains) and deny-override.
/// </summary>
public class ROLE_HIERARCHY : ModelEntityBase
{
    [Key]
    public string ROLE_HIERARCHY_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → ROLE.ROLE_ID — the role that grants its permissions downward.</summary>
    public string PARENT_ROLE_ID { get; set; } = string.Empty;

    /// <summary>Denormalized for fast lookup.</summary>
    public string PARENT_ROLE_NAME { get; set; } = string.Empty;

    /// <summary>FK → ROLE.ROLE_ID — the role that inherits permissions from the parent.</summary>
    public string CHILD_ROLE_ID { get; set; } = string.Empty;

    /// <summary>Denormalized for fast lookup.</summary>
    public string CHILD_ROLE_NAME { get; set; } = string.Empty;

    /// <summary>
    /// FULL = child inherits ALL parent permissions.
    /// SELECTIVE = child inherits only permissions matching the DOMAIN_FILTER.
    /// DENY = child explicitly does NOT inherit parent permissions (override).
    /// </summary>
    public string INHERITANCE_TYPE { get; set; } = "FULL";

    /// <summary>
    /// When INHERITANCE_TYPE is SELECTIVE, comma-separated list of domain prefixes to inherit.
    /// Example: "HSE, Environmental, Regulatory"
    /// </summary>
    public string? DOMAIN_FILTER { get; set; }

    /// <summary>Sort order when multiple parents exist (lower = evaluated first).</summary>
    public int PRIORITY { get; set; }
}
