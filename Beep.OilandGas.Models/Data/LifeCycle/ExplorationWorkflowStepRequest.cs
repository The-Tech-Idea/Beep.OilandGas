using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle;

/// <summary>
/// Process instance id, actor, and optional step payload (maps to <c>PROCESS_STEP_DATA</c> via implicit conversion from dictionary).
/// </summary>
public class ExplorationWorkflowStepRequest : ModelEntityBase
{
    private string InstanceIdValue = string.Empty;

    public string InstanceId
    {
        get { return InstanceIdValue; }
        set { SetProperty(ref InstanceIdValue, value); }
    }

    private string UserIdValue = string.Empty;

    public string UserId
    {
        get { return UserIdValue; }
        set { SetProperty(ref UserIdValue, value); }
    }

    public Dictionary<string, object>? StepData { get; set; }
}
