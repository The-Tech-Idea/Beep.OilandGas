using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

public class AppPermission : ModelEntityBase
{
    [Key]
    public string PERMISSION_ID { get; set; } = Guid.NewGuid().ToString();
    public string PERMISSION_KEY { get; set; } = string.Empty;
    public string? DESCRIPTION { get; set; }
    public string? RISK_LEVEL { get; set; }
}
