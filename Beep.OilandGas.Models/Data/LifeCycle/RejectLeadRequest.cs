using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle;

/// <summary>
/// Completes the <c>LEAD_APPROVAL</c> step with outcome <c>REJECTED</c>.
/// </summary>
public class RejectLeadRequest : ModelEntityBase
{
    private string InstanceIdValue = string.Empty;

    public string InstanceId
    {
        get { return InstanceIdValue; }
        set { SetProperty(ref InstanceIdValue, value); }
    }

    /// <summary>Optional notes (logged today; reserved for process history).</summary>
    public string? Reason { get; set; }
}
