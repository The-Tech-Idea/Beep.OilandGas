using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.PPDM39.DataManagement.Models;

/// <summary>Company-owned selection of a reviewed shared GHG catalog; this does not approve reporting.</summary>
public sealed class COMPANY_GHG_CATALOG_SELECTION : ModelEntityBase
{
    [Key, Required, MaxLength(64)] public string COMPANY_GHG_CATALOG_SELECTION_ID { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string OWNER_TENANT_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string JURISDICTION_KEY { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string CATALOG_REVISION { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string ENVIRONMENT_REVISION { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string SELECTED_BY { get; set; } = string.Empty;
    [Required, MaxLength(2000)] public string RATIONALE { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string REVISION { get; set; } = string.Empty;
    [MaxLength(64)] public string? PREVIOUS_REVISION { get; set; }
    [Required, MaxLength(64)] public string INTENT_ID { get; set; } = string.Empty;
}
