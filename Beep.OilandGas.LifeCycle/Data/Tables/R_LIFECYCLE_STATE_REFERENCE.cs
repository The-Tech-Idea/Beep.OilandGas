using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Extension reference list for lifecycle LOVs (well states, field phases, reservoir states,
/// process statuses, step statuses, approval statuses, transition conditions).
/// Seeded by <see cref="LifeCycleModule"/>. Physical DDL follows entity-driven tooling.
/// </summary>
public partial class R_LIFECYCLE_STATE_REFERENCE : ModelEntityBase
{
    private string REFERENCE_SETValue = string.Empty;

    public string REFERENCE_SET
    {
        get => REFERENCE_SETValue;
        set => SetProperty(ref REFERENCE_SETValue, value);
    }

    private string REFERENCE_CODEValue = string.Empty;

    public string REFERENCE_CODE
    {
        get => REFERENCE_CODEValue;
        set => SetProperty(ref REFERENCE_CODEValue, value);
    }

    private string LONG_NAMEValue = string.Empty;

    public string LONG_NAME
    {
        get => LONG_NAMEValue;
        set => SetProperty(ref LONG_NAMEValue, value);
    }
}
