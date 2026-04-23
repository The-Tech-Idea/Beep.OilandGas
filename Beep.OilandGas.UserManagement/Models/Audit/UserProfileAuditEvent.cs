using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Audit;

public class UserProfileAuditEvent : ModelEntityBase
{
    [Key]
    public string EVENT_ID { get; set; } = string.Empty;
    public string USER_ID { get; set; } = string.Empty;
    public string CHANGED_BY_USER_ID { get; set; } = string.Empty;
    public DateTime EVENT_UTC { get; set; } = DateTime.UtcNow;
    public string? BEFORE_JSON { get; set; }
    public string? AFTER_JSON { get; set; }
    public string? CHANGE_TYPE { get; set; }
    public string? CORRELATION_ID { get; set; }
}
