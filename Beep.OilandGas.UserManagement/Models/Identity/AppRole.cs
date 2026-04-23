using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

public class AppRole : ModelEntityBase
{
    [Key]
    public string ROLE_ID { get; set; } = Guid.NewGuid().ToString();
    public string ROLE_NAME { get; set; } = string.Empty;
    public string? ROLE_CATEGORY { get; set; }
    public string SYSTEM_ROLE_IND { get; set; } = "N";
    public string SENSITIVE_ROLE_IND { get; set; } = "N";
    public DateTime CREATED_UTC { get; set; } = DateTime.UtcNow;
}
