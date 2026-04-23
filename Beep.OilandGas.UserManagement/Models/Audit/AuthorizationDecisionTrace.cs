using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Audit;

public class AuthorizationDecisionTrace : ModelEntityBase
{
    [Key]
    public string TRACE_ID { get; set; } = string.Empty;
    public string USER_ID { get; set; } = string.Empty;
    public string POLICY_KEY { get; set; } = string.Empty;
    public string ACTION { get; set; } = string.Empty;
    public string RESOURCE { get; set; } = string.Empty;
    public string DECISION { get; set; } = string.Empty;
    public DateTime EVALUATED_UTC { get; set; } = DateTime.UtcNow;
    public string? CONTEXT_JSON { get; set; }
    public string? SCOPE_CONTEXT { get; set; }
    public string? DENIAL_REASON_CODE { get; set; }
    public string? EVALUATED_POLICIES_JSON { get; set; }
}
