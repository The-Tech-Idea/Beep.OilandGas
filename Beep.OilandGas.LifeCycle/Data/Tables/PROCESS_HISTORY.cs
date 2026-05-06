using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Audit trail for process execution events. Records all state changes, actions, and transitions.
/// </summary>
public class PROCESS_HISTORY : ModelEntityBase
{
    private string PROCESS_HISTORY_IDValue = string.Empty;
    public string PROCESS_HISTORY_ID
    {
        get => PROCESS_HISTORY_IDValue;
        set => SetProperty(ref PROCESS_HISTORY_IDValue, value);
    }

    private string PROCESS_INSTANCE_IDValue = string.Empty;
    public string PROCESS_INSTANCE_ID
    {
        get => PROCESS_INSTANCE_IDValue;
        set => SetProperty(ref PROCESS_INSTANCE_IDValue, value);
    }

    private string? PROCESS_STEP_INSTANCE_IDValue;
    public string? PROCESS_STEP_INSTANCE_ID
    {
        get => PROCESS_STEP_INSTANCE_IDValue;
        set => SetProperty(ref PROCESS_STEP_INSTANCE_IDValue, value);
    }

    private string EVENT_TYPEValue = string.Empty;
    public string EVENT_TYPE
    {
        get => EVENT_TYPEValue;
        set => SetProperty(ref EVENT_TYPEValue, value);
    }

    private DateTime? EVENT_DATEValue;
    public DateTime? EVENT_DATE
    {
        get => EVENT_DATEValue;
        set => SetProperty(ref EVENT_DATEValue, value);
    }

    private string USER_IDValue = string.Empty;
    public string USER_ID
    {
        get => USER_IDValue;
        set => SetProperty(ref USER_IDValue, value);
    }

    private string? DETAILSValue;
    public string? DETAILS
    {
        get => DETAILSValue;
        set => SetProperty(ref DETAILSValue, value);
    }

    private string? EVENT_DATA_JSONValue;
    public string? EVENT_DATA_JSON
    {
        get => EVENT_DATA_JSONValue;
        set => SetProperty(ref EVENT_DATA_JSONValue, value);
    }

    private string? FROM_STATUSValue;
    public string? FROM_STATUS
    {
        get => FROM_STATUSValue;
        set => SetProperty(ref FROM_STATUSValue, value);
    }

    private string? TO_STATUSValue;
    public string? TO_STATUS
    {
        get => TO_STATUSValue;
        set => SetProperty(ref TO_STATUSValue, value);
    }

    private string? IP_ADDRESSValue;
    public string? IP_ADDRESS
    {
        get => IP_ADDRESSValue;
        set => SetProperty(ref IP_ADDRESSValue, value);
    }

    public PROCESS_HISTORY() { }
}
