using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Models.Data.ProductionAccounting;
public partial class JOURNAL_ENTRY
{
    [MaxLength(67108864)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? POSTING_WORKFLOW_JSON{get;set;}
    [MaxLength(32)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? SALES_JOURNAL_POSTING_INTENT_ID{get;set;}
    [MaxLength(33554432)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? REVIEW_WORKFLOW_JSON{get;set;}
    [MaxLength(8388608)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? PREPARATION_WORKFLOW_JSON{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? FIELD_ID{get;set;}
    [MaxLength(16)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? CURRENCY_CODE{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? BOOK_ID{get;set;}
    [MaxLength(32)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? SOURCE_AR_INVOICE_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? OWNER_TENANT_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? OWNER_COMPANY_BA_ID{get;set;}
    [MaxLength(32)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? COMPANY_LEDGER_PERIOD_ID{get;set;}
}
public partial class JOURNAL_ENTRY_LINE
{
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? FIELD_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? BOOK_ID{get;set;}
    [MaxLength(32)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? SOURCE_AR_INVOICE_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? OWNER_TENANT_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? OWNER_COMPANY_BA_ID{get;set;}
    [MaxLength(32)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? COMPANY_LEDGER_PERIOD_ID{get;set;}
    [MaxLength(128)][JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? MAPPING_KEY{get;set;}
}

