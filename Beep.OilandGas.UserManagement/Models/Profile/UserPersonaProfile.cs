using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Profile;

public class UserPersonaProfile : ModelEntityBase
{
    [Key]
    public string PROFILE_ID { get; set; } = string.Empty;
    public string USER_ID { get; set; } = string.Empty;
    public string PRIMARY_PERSONA { get; set; } = string.Empty;
    public string? SECONDARY_PERSONAS { get; set; }
    public string? DEFAULT_ROUTE { get; set; }
    public string? DEFAULT_FIELD_ID { get; set; }
    public string? DEFAULT_ASSET_ID { get; set; }
    public string? LOCALE { get; set; }
    public string? TIME_ZONE { get; set; }
    public string? PREFERENCES_JSON { get; set; }
    public string? EFFECTIVE_ACCESS_CONTEXT_JSON { get; set; }
    public string? ROW_VERSION { get; set; }
    // Lifecycle state for the profile itself
    public string PROFILE_LIFECYCLE_STATE { get; set; } = "Active";
    public DateTime? LAST_ACCESSED_UTC { get; set; }
    public int PROFILE_VERSION { get; set; } = 1;
    // ── O&G field-scope defaults ──────────────────────────────────────
    public string? DEFAULT_UNIT_SYSTEM { get; set; }
}
