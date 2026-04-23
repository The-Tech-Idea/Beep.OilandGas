using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Profile;

public class PersonaViewPreference : ModelEntityBase
{
    [Key]
    public string PREFERENCE_ID { get; set; } = string.Empty;
    public string USER_ID { get; set; } = string.Empty;
    public string PERSONA_CODE { get; set; } = string.Empty;
    public string VIEW_KEY { get; set; } = string.Empty;
    public string? VIEW_VALUE { get; set; }
    public DateTime UPDATED_UTC { get; set; } = DateTime.UtcNow;
}
