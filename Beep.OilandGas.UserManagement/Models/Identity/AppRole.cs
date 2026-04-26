using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

public class AppRole : ModelEntityBase
{
    [Key]
    public string ROLE_ID { get; set; } = Guid.NewGuid().ToString();
    public string ROLE_NAME { get; set; } = string.Empty;
    public string? DESCRIPTION { get; set; }
    public string? ROLE_TYPE { get; set; }
    public string? ROLE_CATEGORY { get; set; }
    public string SYSTEM_ROLE_IND { get; set; } = "N";
    public string SENSITIVE_ROLE_IND { get; set; } = "N";
    public string SOD_FLAG { get; set; } = "N";
    public int? DISPLAY_SORT_ORDER { get; set; }
    public DateTime CREATED_UTC { get; set; } = DateTime.UtcNow;
    // ── O&G field-scope constraint ─────────────────────────────────────
    /// <summary>When set, restricts this role to a specific field or field-pattern ('*' = all).</summary>
    public string? VALID_FIELD_SCOPE { get; set; }
}
