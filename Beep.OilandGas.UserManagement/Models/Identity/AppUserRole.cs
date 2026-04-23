using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

public class AppUserRole : ModelEntityBase
{
    [Key]
    public string USER_ROLE_ID { get; set; } = Guid.NewGuid().ToString();
    public string USER_ID { get; set; } = string.Empty;
    public string ROLE_ID { get; set; } = string.Empty;
    public string? GRANTED_BY_USER_ID { get; set; }
    public string? ASSIGNMENT_REASON { get; set; }
    public DateTime EFFECTIVE_FROM_UTC { get; set; } = DateTime.UtcNow;
    public DateTime? EFFECTIVE_TO_UTC { get; set; }
}
