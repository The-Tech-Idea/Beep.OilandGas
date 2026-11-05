using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;
/// <summary>One original company/program execution start. Native workflow approval and WELL state remain separate.</summary>
public sealed class DRILLING_OPERATION:ModelEntityBase
{
    [Key,Required,MaxLength(128)]public string DRILLING_OPERATION_ID{get;set;}="";
    [Required,MaxLength(128)]public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)]public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)]public string FIELD_ID{get;set;}="";
    [Required,MaxLength(128)]public string WELL_UWI{get;set;}="";
    [Required,MaxLength(128)]public string PROGRAM_ID{get;set;}="";
    [Required,MaxLength(64)]public string PROGRAM_DRAFT_REVISION{get;set;}="";
    [Required,MaxLength(128)]public string PROGRAM_REVIEW_INSTANCE_ID{get;set;}="";
    [Required,MaxLength(64)]public string PROGRAM_REVIEW_NATIVE_REVISION{get;set;}="";
    [Required,MaxLength(64)]public string EXECUTION_START_REVISION{get;set;}="";
    [Required,MaxLength(TheTechIdea.Data.OilGas.DrillingExecutionStart.MaximumBodyLength)]public string EXECUTION_START_JSON{get;set;}="";
    [Required,MaxLength(128)]public string RECORDER_ROLE{get;set;}="";
    [Required,MaxLength(64)]public string RECORDER_PERSONA_CODE{get;set;}="";
    [Required,MaxLength(32)]public string STATUS{get;set;}="";
    [Required]public DateTime? SPUD_DATE{get;set;}
}
