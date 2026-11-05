using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.PPDM39.DataManagement.Models;

/// <summary>Versioned installation-wide PPDM or OilandGasTools setup data shared across companies.</summary>
public sealed class SYSTEM_SETUP_RECORD : ModelEntityBase
{
    [Key, Required, MaxLength(64)] public string SYSTEM_SETUP_RECORD_ID { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string CATEGORY { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string SETUP_KEY { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string VERSION { get; set; } = string.Empty;
    [Required, MaxLength(SystemSetupIdentity.MaximumSettingsLength)] public string SETTINGS_JSON { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string REVISION { get; set; } = string.Empty;
    [MaxLength(64)] public string? PREVIOUS_REVISION { get; set; }
    [Required, MaxLength(64)] public string INTENT_ID { get; set; } = string.Empty;
}
