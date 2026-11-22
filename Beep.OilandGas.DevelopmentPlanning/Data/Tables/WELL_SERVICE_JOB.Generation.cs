using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning;

/// <summary>
/// The generation columns of a well service job (WRK-03): which plan it came from and when it was generated.
/// </summary>
/// <remarks>
/// The owner's service job records the asset, the job type, a status and a planned window - but no link back to the plan that
/// produced it, so a generated job could not be traced to the schedule it came from (WRK-03 AC-3). Both columns are additive.
/// </remarks>
public partial class WELL_SERVICE_JOB
{
    /// <summary>The maintenance plan this job was generated from, when it was generated rather than raised by hand.</summary>
    [MaxLength(128)]
    public string? MAINT_PLAN_ID
    {
        get => PlanIdValue;
        set => SetProperty(ref PlanIdValue, value);
    }

    private string? PlanIdValue;

    /// <summary>The date the job was generated from its plan.</summary>
    public DateTime? GENERATED_ON
    {
        get => GeneratedValue;
        set => SetProperty(ref GeneratedValue, value);
    }

    private DateTime? GeneratedValue;
}
