using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Process workflow definition. Defines reusable workflow templates with steps, transitions, and metadata.
/// </summary>
public class PROCESS_DEFINITION : ModelEntityBase
{
    private string PROCESS_DEFINITION_IDValue = string.Empty;
    public string PROCESS_DEFINITION_ID
    {
        get => PROCESS_DEFINITION_IDValue;
        set => SetProperty(ref PROCESS_DEFINITION_IDValue, value);
    }

    private string PROCESS_NAMEValue = string.Empty;
    public string PROCESS_NAME
    {
        get => PROCESS_NAMEValue;
        set => SetProperty(ref PROCESS_NAMEValue, value);
    }

    private string PROCESS_TYPEValue = string.Empty;
    public string PROCESS_TYPE
    {
        get => PROCESS_TYPEValue;
        set => SetProperty(ref PROCESS_TYPEValue, value);
    }

    private string? ENTITY_TYPEValue;
    public string? ENTITY_TYPE
    {
        get => ENTITY_TYPEValue;
        set => SetProperty(ref ENTITY_TYPEValue, value);
    }

    private string? DESCRIPTIONValue;
    public string? DESCRIPTION
    {
        get => DESCRIPTIONValue;
        set => SetProperty(ref DESCRIPTIONValue, value);
    }

    private string? VERSIONValue;
    public string? VERSION
    {
        get => VERSIONValue;
        set => SetProperty(ref VERSIONValue, value);
    }

    private string? IS_ACTIVEValue;
    public string? IS_ACTIVE
    {
        get => IS_ACTIVEValue;
        set => SetProperty(ref IS_ACTIVEValue, value);
    }

    private string? PROCESS_CONFIG_JSONValue;
    public string? PROCESS_CONFIG_JSON
    {
        get => PROCESS_CONFIG_JSONValue;
        set => SetProperty(ref PROCESS_CONFIG_JSONValue, value);
    }

    private string? REQUIRED_ROLEValue;
    public string? REQUIRED_ROLE
    {
        get => REQUIRED_ROLEValue;
        set => SetProperty(ref REQUIRED_ROLEValue, value);
    }

    public PROCESS_DEFINITION() { }
}
