using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Profile;

public class PersonaDefinition : ModelEntityBase
{
    [Key]
    public string PERSONA_ID { get; set; } = string.Empty;
    public string PERSONA_CODE { get; set; } = string.Empty;
    public string PERSONA_NAME { get; set; } = string.Empty;
    public string? PERSONA_CATEGORY { get; set; }
    public string? DESCRIPTION { get; set; }
    public string ACTIVE_FLAG { get; set; } = "Y";
    public string? DEFAULT_LANDING_ROUTE { get; set; }
    public string? ALLOWED_WORKFLOWS_JSON { get; set; }
    public int? DISPLAY_SORT_ORDER { get; set; }
}
