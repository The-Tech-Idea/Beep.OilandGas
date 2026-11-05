using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Work-order execution permit proposal and its original issue intent; regulatory filings are separate.</summary>
[Table("WORK_ORDER_EXECUTION_PERMIT")]
public sealed class WORK_ORDER_EXECUTION_PERMIT : ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string PERMIT_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string TENANT_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string BUSINESS_ASSOCIATE_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string WORK_ORDER_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID {get;set;}=string.Empty;
    [Required,MaxLength(36)] public string ISSUE_INTENT_ID {get;set;}=string.Empty;
    [Required,MaxLength(128)] public string ISSUED_BY {get;set;}=string.Empty;
    [Required,MaxLength(32)] public string STATUS {get;set;}=string.Empty;
    [Required,MaxLength(64)] public string INSPECTION_REVISION {get;set;}=string.Empty;
    [Required,MaxLength(32)] public string PERMIT_TYPE {get;set;}=string.Empty;
    [Required,MaxLength(256)] public string JURISDICTION {get;set;}=string.Empty;
    [Required,MaxLength(2048)] public string SCOPE_DESCRIPTION {get;set;}=string.Empty;
    [Required,MaxLength(2048)] public string ISOLATION_REFERENCE {get;set;}=string.Empty;
    [Required,MaxLength(2048)] public string RISK_REFERENCE {get;set;}=string.Empty;
    [Required,MaxLength(2048)] public string PERSONNEL_REFERENCE {get;set;}=string.Empty;
    [Required,MaxLength(2048)] public string MATERIAL_REFERENCE {get;set;}=string.Empty;
    public DateTime VALID_FROM_UTC {get;set;}
    public DateTime VALID_TO_UTC {get;set;}
    [Required,MaxLength(131072)] public string ISSUE_DATA_JSON {get;set;}=string.Empty;
    [MaxLength(131072)]
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? INSPECTION_DATA_JSON {get;set;}
    [MaxLength(36),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? DECISION_INTENT_ID {get;set;}
    [MaxLength(128),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? DECIDED_BY {get;set;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public DateTime? DECIDED_AT_UTC {get;set;}
    [MaxLength(524288),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? DECISION_DATA_JSON {get;set;}
}
