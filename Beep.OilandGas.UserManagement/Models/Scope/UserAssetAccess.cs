using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Scope;

public class UserAssetAccess : ModelEntityBase
{
    [Key]
    public string ACCESS_ID { get; set; } = string.Empty;
    public string USER_ID { get; set; } = string.Empty;
    public string ASSET_ID { get; set; } = string.Empty;
    public string ASSET_TYPE { get; set; } = string.Empty;
    public string ACCESS_LEVEL { get; set; } = "read";
    public string? SCOPE_SOURCE { get; set; }
    public DateTime? ACCESS_EXPIRES_UTC { get; set; }
}
