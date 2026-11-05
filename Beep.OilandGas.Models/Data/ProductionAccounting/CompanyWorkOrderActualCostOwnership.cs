using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Models.Data.ProductionAccounting;
public partial class ACCOUNTING_COST
{
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? OWNER_TENANT_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? OWNER_COMPANY_BA_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? WORK_ORDER_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? BOOK_ID{get;set;}
    [MaxLength(16)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? CURRENCY_CODE{get;set;}
    [MaxLength(64)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? SOURCE_KIND{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? SOURCE_ID{get;set;}
    [MaxLength(64)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? SOURCE_REVISION{get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public decimal? SOURCE_QUANTITY{get;set;}
    [MaxLength(32)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? SOURCE_UNIT{get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public decimal? UNIT_RATE{get;set;}
    [MaxLength(4*1024*1024)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? WORK_ORDER_COST_JSON{get;set;}
    [MaxLength(64)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? WORK_ORDER_COST_REVISION{get;set;}
}
