using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle;

/// <summary>
/// Executes the <c>PROSPECT_CREATION</c> process step and, on success, completes the step and runs <c>ILeadExplorationService</c> persistence.
/// </summary>
public class PromoteLeadToProspectRequest : ModelEntityBase
{
    private string InstanceIdValue = string.Empty;

    public string InstanceId
    {
        get => InstanceIdValue;
        set => SetProperty(ref InstanceIdValue, value);
    }

    private string UserIdValue = string.Empty;

    public string UserId
    {
        get => UserIdValue;
        set => SetProperty(ref UserIdValue, value);
    }

    /// <summary>Optional payload for <c>PROSPECT_CREATION</c> (serialized to <c>PROCESS_STEP_DATA.DataJson</c>).</summary>
    public Dictionary<string, object>? ProspectData { get; set; }
}
