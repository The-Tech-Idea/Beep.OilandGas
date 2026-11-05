using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning;

public partial class FIELD_DEVELOPMENT_PLAN
{
    [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? SOURCE_DRAFT_FDP_ID{get;set;}
    [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? SOURCE_REVIEW_PROCESS_ID{get;set;}
    [MaxLength(36),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? RELEASE_INTENT_ID{get;set;}
    [MaxLength(64),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? RELEASE_REQUEST_REVISION{get;set;}
    [MaxLength(1024*1024),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? RELEASE_COMMAND_JSON{get;set;}
}
