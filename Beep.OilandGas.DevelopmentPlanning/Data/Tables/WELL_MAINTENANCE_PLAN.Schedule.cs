using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning;

/// <summary>
/// The scheduling columns of a well maintenance plan (WRK-03), added to the plan record the owner already keeps.
/// </summary>
/// <remarks>
/// The owner's plan records the asset, the maintenance type, a status, a priority and a planned window - and no frequency at
/// all, so "next due" had no source. The stage's open question asks for one owner of the plan record, and this is it:
/// <c>WELL_MAINTENANCE_PLAN</c> in DevelopmentPlanning carries the schedule, while PPDM's <c>WORK_ORDER_INSPECTION</c> keeps the
/// inspection results. The columns are additive, so every recorded plan stays readable and simply reports no frequency.
/// </remarks>
public partial class WELL_MAINTENANCE_PLAN
{
    /// <summary>How often the plan repeats, in <see cref="FREQUENCY_UNIT"/>. Zero or absent means it is not scheduled.</summary>
    public int? FREQUENCY
    {
        get => FrequencyValue;
        set => SetProperty(ref FrequencyValue, value);
    }

    private int? FrequencyValue;

    /// <summary>The unit the frequency is expressed in - DAY, WEEK, MONTH or YEAR. A frequency without it is refused.</summary>
    [MaxLength(16)]
    public string? FREQUENCY_UNIT
    {
        get => FrequencyUnitValue;
        set => SetProperty(ref FrequencyUnitValue, value);
    }

    private string? FrequencyUnitValue;

    /// <summary>When the work was last done; the next due date is computed from it, never stored.</summary>
    public DateTime? LAST_DONE_DATE
    {
        get => LastDoneValue;
        set => SetProperty(ref LastDoneValue, value);
    }

    private DateTime? LastDoneValue;

    /// <summary>The plan's tasks, one per line.</summary>
    [MaxLength(4000)]
    public string? TASK_LIST
    {
        get => TaskListValue;
        set => SetProperty(ref TaskListValue, value);
    }

    private string? TaskListValue;

    /// <summary>The person who last edited the schedule, so a generated job can be traced back to a decision.</summary>
    [MaxLength(128)]
    public string? SCHEDULE_EDITED_BY
    {
        get => EditedByValue;
        set => SetProperty(ref EditedByValue, value);
    }

    private string? EditedByValue;

    public DateTime? SCHEDULE_EDITED_DATE
    {
        get => EditedDateValue;
        set => SetProperty(ref EditedDateValue, value);
    }

    private DateTime? EditedDateValue;
}
