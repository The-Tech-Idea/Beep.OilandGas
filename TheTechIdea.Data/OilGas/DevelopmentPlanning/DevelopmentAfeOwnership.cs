using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Models.Data.ProductionAccounting;
public partial class AFE
{
    [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? FDP_BASELINE_ID{get;set;}
    [MaxLength(4*1024*1024),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? DEVELOPMENT_BUDGET_JSON{get;set;}
    [MaxLength(64),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? DEVELOPMENT_BUDGET_REVISION{get;set;}
}
