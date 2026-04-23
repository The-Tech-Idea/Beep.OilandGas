using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

public class AppPermission : ModelEntityBase
{
    [Key]
    public string PERMISSION_ID { get; set; } = Guid.NewGuid().ToString();
    /// <summary>Canonical key in resource.action.scope format.</summary>
    public string PERMISSION_KEY { get; set; } = string.Empty;
    public string? RESOURCE_KEY { get; set; }
    public string? ACTION_KEY { get; set; }
    public string? SCOPE_KEY { get; set; }
    public string? DOMAIN_KEY { get; set; }
    public string? POLICY_MAPPING_KEY { get; set; }
    public string? DESCRIPTION { get; set; }
    public string? RISK_LEVEL { get; set; }
}
