using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

public class AppRolePermission : ModelEntityBase
{
    [Key]
    public string ROLE_PERMISSION_ID { get; set; } = Guid.NewGuid().ToString();
    public string ROLE_ID { get; set; } = string.Empty;
    public string PERMISSION_ID { get; set; } = string.Empty;
    public DateTime EFFECTIVE_FROM_UTC { get; set; } = DateTime.UtcNow;
    public DateTime? EFFECTIVE_TO_UTC { get; set; }
    public string? SOURCE_SYSTEM { get; set; }
    public string? APPROVED_BY_USER_ID { get; set; }
    public DateTime? APPROVED_AT_UTC { get; set; }
}
