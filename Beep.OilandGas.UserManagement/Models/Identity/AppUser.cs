using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

public class AppUser : ModelEntityBase
{
    [Key]
    public string USER_ID { get; set; } = Guid.NewGuid().ToString();
    public string USERNAME { get; set; } = string.Empty;
    public string? EMAIL { get; set; }
    public string? EXTERNAL_IDENTITY_KEY { get; set; }
    public string USER_LIFECYCLE_STATE { get; set; } = "Active";
    public string LOCKED_IND { get; set; } = "N";
    public DateTime? LAST_LOGIN_UTC { get; set; }
    public DateTime? LAST_PASSWORD_CHANGE_UTC { get; set; }
    public DateTime CREATED_UTC { get; set; } = DateTime.UtcNow;
    public DateTime UPDATED_UTC { get; set; } = DateTime.UtcNow;
}
