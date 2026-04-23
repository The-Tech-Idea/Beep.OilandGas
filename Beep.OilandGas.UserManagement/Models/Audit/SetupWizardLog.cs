using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Audit;

public class SetupWizardLog : ModelEntityBase
{
    [Key]
    public string LOG_ID { get; set; } = string.Empty;
    public string EVENT_NAME { get; set; } = string.Empty;
    public string? EVENT_STATUS { get; set; }
    public DateTime LOGGED_UTC { get; set; } = DateTime.UtcNow;
    public string? DETAILS_JSON { get; set; }
}
