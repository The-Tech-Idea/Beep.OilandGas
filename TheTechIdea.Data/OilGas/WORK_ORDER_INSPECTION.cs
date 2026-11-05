using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.WorkOrder;

[Table("WORK_ORDER_INSPECTION")]
public sealed class WORK_ORDER_INSPECTION : ModelEntityBase
{
    [MaxLength(32),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? APPLICABLE_STAGE {get;set;}
    [MaxLength(8*1024*1024),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? DOCUMENT_ACCEPTANCE_DATA_JSON {get;set;}
    [MaxLength(64),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? DOCUMENT_ACCEPTANCE_REVISION {get;set;}
    [MaxLength(8*1024*1024),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? PERSONNEL_ACCEPTANCE_DATA_JSON {get;set;}
    [MaxLength(64),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? PERSONNEL_ACCEPTANCE_REVISION {get;set;}
    [Key, Required, MaxLength(128)]
    public string INSPECTION_ID { get => INSPECTION_IDValue; set => SetProperty(ref INSPECTION_IDValue,value); }
    private string INSPECTION_IDValue = string.Empty;
    [Required, MaxLength(128)]
    public string WORK_ORDER_ID { get => WORK_ORDER_IDValue; set => SetProperty(ref WORK_ORDER_IDValue,value); }
    private string WORK_ORDER_IDValue = string.Empty;
    [Required, MaxLength(128)]
    public string FIELD_ID { get => FIELD_IDValue; set => SetProperty(ref FIELD_IDValue,value); }
    private string FIELD_IDValue = string.Empty;
    public int COND_SEQ { get => COND_SEQValue; set => SetProperty(ref COND_SEQValue,value); }
    private int COND_SEQValue;
    [Required, MaxLength(128)]
    public string COND_TYPE { get => COND_TYPEValue; set => SetProperty(ref COND_TYPEValue,value); }
    private string COND_TYPEValue = string.Empty;
    [Required, MaxLength(4096)]
    public string REQUIRED_TEXT { get => REQUIRED_TEXTValue; set => SetProperty(ref REQUIRED_TEXTValue,value); }
    private string REQUIRED_TEXTValue = string.Empty;
    [Required, MaxLength(256)]
    public string JURISDICTION { get => JURISDICTIONValue; set => SetProperty(ref JURISDICTIONValue,value); }
    private string JURISDICTIONValue = string.Empty;
    [MaxLength(2048)]
    public string REGULATORY_REF { get => REGULATORY_REFValue; set => SetProperty(ref REGULATORY_REFValue,value); }
    private string REGULATORY_REFValue = string.Empty;
    [MaxLength(32),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? PERMIT_PREREQUISITE_KIND {get;set;}
    [Required, MaxLength(128)]
    public string REQUIREMENT_ACTOR { get => REQUIREMENT_ACTORValue; set => SetProperty(ref REQUIREMENT_ACTORValue,value); }
    private string REQUIREMENT_ACTORValue = string.Empty;
    [MaxLength(16)]
    public string RESULT_STATUS { get => RESULT_STATUSValue; set => SetProperty(ref RESULT_STATUSValue,value); }
    private string RESULT_STATUSValue = string.Empty;
    [MaxLength(4096)]
    public string RESULT_NOTES { get => RESULT_NOTESValue; set => SetProperty(ref RESULT_NOTESValue,value); }
    private string RESULT_NOTESValue = string.Empty;
    [MaxLength(128)]
    public string INSPECTOR { get => INSPECTORValue; set => SetProperty(ref INSPECTORValue,value); }
    private string INSPECTORValue = string.Empty;
    public DateTime? INSPECTED_AT_UTC { get => INSPECTED_AT_UTCValue; set => SetProperty(ref INSPECTED_AT_UTCValue,value); }
    private DateTime? INSPECTED_AT_UTCValue;
    [MaxLength(TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialAcceptanceCommand.MaximumBodyLength),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? MATERIAL_ACCEPTANCE_DATA_JSON {get;set;}
    [MaxLength(64),System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? MATERIAL_ACCEPTANCE_REVISION {get;set;}
}
