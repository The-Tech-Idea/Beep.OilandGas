using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

public class AppUser : ModelEntityBase
{
    [Key]
    public string USER_ID { get; set; } = Guid.NewGuid().ToString();
    public string USERNAME { get; set; } = string.Empty;
    public string? EMAIL { get; set; }
    public string? EXTERNAL_IDENTITY_KEY { get; set; }
    public string USER_LIFECYCLE_STATE { get; set; } = "Active";
    public string LOCKED_IND { get; set; } = "N";
    public DateTime? LAST_LOGIN_UTC { get; set; }
    public DateTime? LAST_PASSWORD_CHANGE_UTC { get; set; }
    public DateTime CREATED_UTC { get; set; } = DateTime.UtcNow;
    public DateTime UPDATED_UTC { get; set; } = DateTime.UtcNow;
    // Lockout tracking
    public int FAILED_LOGIN_COUNT { get; set; } = 0;
    public DateTime? LOCKOUT_UNTIL_UTC { get; set; }
    // Password policy metadata
    public string? PASSWORD_POLICY_KEY { get; set; }
    public int? MAX_PASSWORD_AGE_DAYS { get; set; }
    public string MUST_CHANGE_PASSWORD_IND { get; set; } = "N";
    // Lifecycle state transition audit
    public DateTime? STATE_CHANGED_UTC { get; set; }
    public string? STATE_CHANGED_BY_USER_ID { get; set; }
    // ── PPDM 3.9 linkage ──────────────────────────────────────────────
    /// <summary>Optional link to PPDM BUSINESS_ASSOCIATE for this user's org-person record.</summary>
    public string? BA_ID { get; set; }
    public string? EMPLOYEE_ID { get; set; }
}
