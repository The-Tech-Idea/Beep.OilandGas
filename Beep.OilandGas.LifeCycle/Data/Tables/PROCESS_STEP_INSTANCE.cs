using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Individual step within a process instance. Tracks execution state, assignment, and outcomes.
/// </summary>
public class PROCESS_STEP_INSTANCE : ModelEntityBase
{
    private string PROCESS_STEP_INSTANCE_IDValue = string.Empty;
    public string PROCESS_STEP_INSTANCE_ID
    {
        get => PROCESS_STEP_INSTANCE_IDValue;
        set => SetProperty(ref PROCESS_STEP_INSTANCE_IDValue, value);
    }

    private string PROCESS_INSTANCE_IDValue = string.Empty;
    public string PROCESS_INSTANCE_ID
    {
        get => PROCESS_INSTANCE_IDValue;
        set => SetProperty(ref PROCESS_INSTANCE_IDValue, value);
    }

    private string STEP_IDValue = string.Empty;
    public string STEP_ID
    {
        get => STEP_IDValue;
        set => SetProperty(ref STEP_IDValue, value);
    }

    private string STEP_NAMEValue = string.Empty;
    public string STEP_NAME
    {
        get => STEP_NAMEValue;
        set => SetProperty(ref STEP_NAMEValue, value);
    }

    private int STEP_SEQUENCEValue;
    public int STEP_SEQUENCE
    {
        get => STEP_SEQUENCEValue;
        set => SetProperty(ref STEP_SEQUENCEValue, value);
    }

    private string STATUSValue = "PENDING";
    public string STATUS
    {
        get => STATUSValue;
        set => SetProperty(ref STATUSValue, value);
    }

    private string? ASSIGNED_TOValue;
    public string? ASSIGNED_TO
    {
        get => ASSIGNED_TOValue;
        set => SetProperty(ref ASSIGNED_TOValue, value);
    }

    private DateTime? STARTED_DATEValue;
    public DateTime? STARTED_DATE
    {
        get => STARTED_DATEValue;
        set => SetProperty(ref STARTED_DATEValue, value);
    }

    private DateTime? COMPLETION_DATEValue;
    public DateTime? COMPLETION_DATE
    {
        get => COMPLETION_DATEValue;
        set => SetProperty(ref COMPLETION_DATEValue, value);
    }

    private string? COMPLETED_BYValue;
    public string? COMPLETED_BY
    {
        get => COMPLETED_BYValue;
        set => SetProperty(ref COMPLETED_BYValue, value);
    }

    private string? STEP_DATA_JSONValue;
    public string? STEP_DATA_JSON
    {
        get => STEP_DATA_JSONValue;
        set => SetProperty(ref STEP_DATA_JSONValue, value);
    }

    private string? OUTCOMEValue;
    public string? OUTCOME
    {
        get => OUTCOMEValue;
        set => SetProperty(ref OUTCOMEValue, value);
    }

    private string? NOTESValue;
    public string? NOTES
    {
        get => NOTESValue;
        set => SetProperty(ref NOTESValue, value);
    }

    private string? REQUIRED_ROLEValue;
    public string? REQUIRED_ROLE
    {
        get => REQUIRED_ROLEValue;
        set => SetProperty(ref REQUIRED_ROLEValue, value);
    }

    private double? SLA_HOURSValue;
    public double? SLA_HOURS
    {
        get => SLA_HOURSValue;
        set => SetProperty(ref SLA_HOURSValue, value);
    }

    private bool APPROVAL_REQUIREDValue;
    public bool APPROVAL_REQUIRED
    {
        get => APPROVAL_REQUIREDValue;
        set => SetProperty(ref APPROVAL_REQUIREDValue, value);
    }

    public PROCESS_STEP_INSTANCE() { }
}
