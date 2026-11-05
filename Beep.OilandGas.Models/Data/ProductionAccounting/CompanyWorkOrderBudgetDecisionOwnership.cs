using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Models.Data.ProductionAccounting;
public partial class APPROVAL_WORKFLOW
{
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? OWNER_TENANT_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? OWNER_COMPANY_BA_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? FIELD_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? WORK_ORDER_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? BOOK_ID{get;set;}
    [MaxLength(16)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? CURRENCY_CODE{get;set;}
    [MaxLength(64)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? ORIGINAL_AFE_REVISION{get;set;}
    [MaxLength(4*1024*1024)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? WORK_ORDER_DECISION_JSON{get;set;}
    [MaxLength(64)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? WORK_ORDER_DECISION_REVISION{get;set;}
}
