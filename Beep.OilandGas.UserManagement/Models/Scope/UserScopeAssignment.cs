using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Scope;

public class UserScopeAssignment : ModelEntityBase
{
    [Key]
    public string ASSIGNMENT_ID { get; set; } = string.Empty;
    public string USER_ID { get; set; } = string.Empty;
    public string SCOPE_TYPE { get; set; } = string.Empty;
    public string SCOPE_VALUE { get; set; } = string.Empty;
    public string? GRANTED_BY { get; set; }
    public DateTime EFFECTIVE_FROM_UTC { get; set; } = DateTime.UtcNow;
    public DateTime? EFFECTIVE_TO_UTC { get; set; }
}
