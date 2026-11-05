using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed class COMPANY_LEDGER_BOOK:ModelEntityBase
{
    [Key,Required,MaxLength(32)] public string COMPANY_LEDGER_BOOK_ID{get;set;}=string.Empty;
    [Required,MaxLength(128)] public string OWNER_TENANT_ID{get;set;}=string.Empty;
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID{get;set;}=string.Empty;
    [Required,MaxLength(128)] public string BOOK_ID{get;set;}=string.Empty;
    [Required,MaxLength(256)] public string BOOK_NAME{get;set;}=string.Empty;
    [Required,MaxLength(16)] public string FUNCTIONAL_CURRENCY{get;set;}=string.Empty;
    [Required] public int CURRENCY_DECIMALS{get;set;}
    [Required,MaxLength(32)] public string ROUNDING_POLICY{get;set;}=string.Empty;
    [Required,MaxLength(128)] public string CALENDAR_ID{get;set;}=string.Empty;
    [Required,MaxLength(128)] public string TIME_ZONE_ID{get;set;}=string.Empty;
    [Required,MaxLength(65536)] public string CONFIGURATION_JSON{get;set;}=string.Empty;
}
