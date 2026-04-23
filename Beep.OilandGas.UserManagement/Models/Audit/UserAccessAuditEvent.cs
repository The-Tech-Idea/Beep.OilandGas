using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Audit;

public class UserAccessAuditEvent : ModelEntityBase
{
    [Key]
    public string EVENT_ID { get; set; } = string.Empty;
    public string USER_ID { get; set; } = string.Empty;
    public string EVENT_TYPE { get; set; } = string.Empty;
    public string? TARGET_RESOURCE { get; set; }
    public string? RESULT { get; set; }
    public DateTime EVENT_UTC { get; set; } = DateTime.UtcNow;
    public string? DETAILS_JSON { get; set; }
    public string? CORRELATION_ID { get; set; }
    public string? CLIENT_IP { get; set; }
    public string? SESSION_ID { get; set; }
    public string? SCOPE_CONTEXT { get; set; }
}
