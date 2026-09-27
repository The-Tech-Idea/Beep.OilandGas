using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle;

/// <summary>
/// Process instance id and optional step payload (maps to <c>PROCESS_STEP_DATA</c> via implicit conversion from dictionary).
/// </summary>
public class ExplorationWorkflowStepRequest : ModelEntityBase
{
    private string InstanceIdValue = string.Empty;

    public string InstanceId
    {
        get { return InstanceIdValue; }
        set { SetProperty(ref InstanceIdValue, value); }
    }

    public Dictionary<string, object>? StepData { get; set; }
}
