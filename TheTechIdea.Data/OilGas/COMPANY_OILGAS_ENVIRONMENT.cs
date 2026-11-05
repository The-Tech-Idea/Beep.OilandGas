using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace TheTechIdea.Data.OilGas;

/// <summary>Company-owned environment settings; schema is created through the system setup Beep migration.</summary>
public sealed class COMPANY_OILGAS_ENVIRONMENT : ModelEntityBase
{
    [Key, Required, MaxLength(64)] public string COMPANY_OILGAS_ENVIRONMENT_ID { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string OWNER_TENANT_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = string.Empty;
    [Required, MaxLength(2)] public string COUNTRY_CODE { get; set; } = string.Empty;
    [Required, MaxLength(65536)] public string OVERRIDES_JSON { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string REVISION { get; set; } = string.Empty;
    [MaxLength(64)] public string? PREVIOUS_REVISION { get; set; }
    [Required, MaxLength(64)] public string INTENT_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string TOOLS_VERSION { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string REVIEWED_DRAFT_REVISION { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string REVIEWED_SYSTEM_REVISION { get; set; } = string.Empty;
    [MaxLength(64)] public string? REVIEWED_COUNTRY_REVISION { get; set; }
}
