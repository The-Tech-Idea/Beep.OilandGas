using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

/// <summary>Native company/book period authority; checklist results cannot substitute for this record.</summary>
public sealed class COMPANY_LEDGER_PERIOD:ModelEntityBase
{
    [Key,Required,MaxLength(32)] public string COMPANY_LEDGER_PERIOD_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string OWNER_TENANT_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string BOOK_ID {get;set;}=string.Empty;
    [Required,MaxLength(16)] public string CURRENCY_CODE {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string CALENDAR_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string PERIOD_CODE {get;set;}=string.Empty;
    [Required] public DateTime START_DATE {get;set;}
    [Required] public DateTime END_DATE {get;set;}
    [Required,MaxLength(16)] public string STATUS {get;set;}=string.Empty;
    [Required,MaxLength(65536)] public string AUTHORITY_JSON {get;set;}=string.Empty;
    [MaxLength(2097152),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? STATE_HISTORY_JSON {get;set;}
}
