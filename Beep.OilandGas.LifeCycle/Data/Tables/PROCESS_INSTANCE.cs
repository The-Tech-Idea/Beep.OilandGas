using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Active process workflow instance. Tracks the execution state of a process definition against a specific entity.
/// </summary>
public class PROCESS_INSTANCE : ModelEntityBase
{
    private string PROCESS_INSTANCE_IDValue = string.Empty;
    public string PROCESS_INSTANCE_ID
    {
        get => PROCESS_INSTANCE_IDValue;
        set => SetProperty(ref PROCESS_INSTANCE_IDValue, value);
    }

    private string PROCESS_DEFINITION_IDValue = string.Empty;
    public string PROCESS_DEFINITION_ID
    {
        get => PROCESS_DEFINITION_IDValue;
        set => SetProperty(ref PROCESS_DEFINITION_IDValue, value);
    }

    private string ENTITY_TYPEValue = string.Empty;
    public string ENTITY_TYPE
    {
        get => ENTITY_TYPEValue;
        set => SetProperty(ref ENTITY_TYPEValue, value);
    }

    private string ENTITY_IDValue = string.Empty;
    public string ENTITY_ID
    {
        get => ENTITY_IDValue;
        set => SetProperty(ref ENTITY_IDValue, value);
    }

    private string? FIELD_IDValue;
    public string? FIELD_ID
    {
        get => FIELD_IDValue;
        set => SetProperty(ref FIELD_IDValue, value);
    }

    private string? CURRENT_STATEValue;
    public string? CURRENT_STATE
    {
        get => CURRENT_STATEValue;
        set => SetProperty(ref CURRENT_STATEValue, value);
    }

    private string STATUSValue = "NOT_STARTED";
    public string STATUS
    {
        get => STATUSValue;
        set => SetProperty(ref STATUSValue, value);
    }

    private DateTime? STARTED_DATEValue;
    public DateTime? STARTED_DATE
    {
        get => STARTED_DATEValue;
        set => SetProperty(ref STARTED_DATEValue, value);
    }

    private DateTime? COMPLETED_DATEValue;
    public DateTime? COMPLETED_DATE
    {
        get => COMPLETED_DATEValue;
        set => SetProperty(ref COMPLETED_DATEValue, value);
    }

    private string? COMPLETED_BYValue;
    public string? COMPLETED_BY
    {
        get => COMPLETED_BYValue;
        set => SetProperty(ref COMPLETED_BYValue, value);
    }

    private string? CURRENT_STEP_IDValue;
    public string? CURRENT_STEP_ID
    {
        get => CURRENT_STEP_IDValue;
        set => SetProperty(ref CURRENT_STEP_IDValue, value);
    }

    private int? CURRENT_STEP_SEQUENCEValue;
    public int? CURRENT_STEP_SEQUENCE
    {
        get => CURRENT_STEP_SEQUENCEValue;
        set => SetProperty(ref CURRENT_STEP_SEQUENCEValue, value);
    }

    private string? INSTANCE_DATA_JSONValue;
    public string? INSTANCE_DATA_JSON
    {
        get => INSTANCE_DATA_JSONValue;
        set => SetProperty(ref INSTANCE_DATA_JSONValue, value);
    }

    private string? PARENT_PROCESS_INSTANCE_IDValue;
    public string? PARENT_PROCESS_INSTANCE_ID
    {
        get => PARENT_PROCESS_INSTANCE_IDValue;
        set => SetProperty(ref PARENT_PROCESS_INSTANCE_IDValue, value);
    }

    public PROCESS_INSTANCE() { }
}
