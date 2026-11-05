using Beep.OilandGas.DrillingAndConstruction.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.DrillingAndConstruction.Models;

/// <summary>An attributable observed reporting period on one recorded company execution.</summary>
public sealed class DRILLING_DAILY_REPORT : ModelEntityBase
{
    [Key, Required, MaxLength(128)] public string DDR_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_TENANT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = "";
    [Required, MaxLength(128)] public string FIELD_ID { get; set; } = "";
    [Required, MaxLength(128)] public string WELL_UWI { get; set; } = "";
    [Required, MaxLength(128)] public string PROGRAM_ID { get; set; } = "";
    [Required, MaxLength(128)] public string DRILLING_OPERATION_ID { get; set; } = "";
    [Required, MaxLength(64)] public string EXECUTION_START_REVISION { get; set; } = "";
    [Required] public int REPORT_NUMBER { get; set; }
    [Required] public DateTime? PERIOD_START_UTC { get; set; }
    [Required] public DateTime? PERIOD_END_UTC { get; set; }
    [Required] public decimal? DEPTH_START_MD { get; set; }
    [Required] public decimal? DEPTH_END_MD { get; set; }
    [Required, MaxLength(32)] public string DEPTH_OUOM { get; set; } = "";
    public decimal? ROTATING_HOURS { get; set; }
    public decimal? NPT_HOURS { get; set; }
    [MaxLength(1600)] public string? NPT_REASON { get; set; }
    [Required, MaxLength(1600)] public string REPORT_PROVENANCE { get; set; } = "";
    [Required, MaxLength(1600)] public string DAILY_OPERATIONS_SUMMARY { get; set; } = "";
    [MaxLength(1600)] public string? NEXT_PERIOD_PLAN { get; set; }
    [MaxLength(128), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CORRECTS_REPORT_ID { get; set; }
    [MaxLength(64), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CORRECTS_REPORT_REVISION { get; set; }
    [MaxLength(1600), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CORRECTION_REASON { get; set; }
    [MaxLength(DrillingCompletionReport.MaximumBodyLength), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? REPORTED_COMPLETION_JSON { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public DateTime? REPORTED_COMPLETION_UTC { get; set; }
    [Required, MaxLength(128)] public string RECORDER_ROLE { get; set; } = "";
    [Required, MaxLength(64)] public string RECORDER_PERSONA_CODE { get; set; } = "";
    [Required, MaxLength(32)] public string REPORT_STATUS { get; set; } = "";
    [Required, MaxLength(DrillingDailyReportInput.MaximumBodyLength)] public string REPORT_ORIGINAL_JSON { get; set; } = "";
    [Required, MaxLength(64)] public string REPORT_REVISION { get; set; } = "";
}
