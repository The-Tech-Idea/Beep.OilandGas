using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Process approval record with full workflow support (delegation, escalation, sequencing).
/// Extends the base PROCESS_APPROVAL schema with additional columns for advanced approval workflows.
/// </summary>
public class PROCESS_APPROVAL : ModelEntityBase
{
    private string PROCESS_APPROVAL_IDValue = string.Empty;
    public string PROCESS_APPROVAL_ID
    {
        get => PROCESS_APPROVAL_IDValue;
        set => SetProperty(ref PROCESS_APPROVAL_IDValue, value);
    }

    private string? PROCESS_INSTANCE_IDValue;
    public string? PROCESS_INSTANCE_ID
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

    private string? APPROVAL_TYPEValue;
    public string? APPROVAL_TYPE
    {
        get => APPROVAL_TYPEValue;
        set => SetProperty(ref APPROVAL_TYPEValue, value);
    }

    private string? APPROVAL_STATUSValue;
    public string? APPROVAL_STATUS
    {
        get => APPROVAL_STATUSValue;
        set => SetProperty(ref APPROVAL_STATUSValue, value);
    }

    private string? APPROVER_USER_IDValue;
    public string? APPROVER_USER_ID
    {
        get => APPROVER_USER_IDValue;
        set => SetProperty(ref APPROVER_USER_IDValue, value);
    }

    private int? APPROVER_SEQUENCEValue;
    public int? APPROVER_SEQUENCE
    {
        get => APPROVER_SEQUENCEValue;
        set => SetProperty(ref APPROVER_SEQUENCEValue, value);
    }

    private string? REQUIRED_ACTIONValue;
    public string? REQUIRED_ACTION
    {
        get => REQUIRED_ACTIONValue;
        set => SetProperty(ref REQUIRED_ACTIONValue, value);
    }

    private int? ESCALATION_AFTER_HOURSValue;
    public int? ESCALATION_AFTER_HOURS
    {
        get => ESCALATION_AFTER_HOURSValue;
        set => SetProperty(ref ESCALATION_AFTER_HOURSValue, value);
    }

    private DateTime? APPROVAL_DATEValue;
    public DateTime? APPROVAL_DATE
    {
        get => APPROVAL_DATEValue;
        set => SetProperty(ref APPROVAL_DATEValue, value);
    }

    private string? APPROVED_BYValue;
    public string? APPROVED_BY
    {
        get => APPROVED_BYValue;
        set => SetProperty(ref APPROVED_BYValue, value);
    }

    private string? DELEGATED_BYValue;
    public string? DELEGATED_BY
    {
        get => DELEGATED_BYValue;
        set => SetProperty(ref DELEGATED_BYValue, value);
    }

    private string? APPROVAL_NOTESValue;
    public string? APPROVAL_NOTES
    {
        get => APPROVAL_NOTESValue;
        set => SetProperty(ref APPROVAL_NOTESValue, value);
    }

    // Delegation support
    private string? DELEGATED_TOValue;
    public string? DELEGATED_TO
    {
        get => DELEGATED_TOValue;
        set => SetProperty(ref DELEGATED_TOValue, value);
    }

    private string? DELEGATED_FROMValue;
    public string? DELEGATED_FROM
    {
        get => DELEGATED_FROMValue;
        set => SetProperty(ref DELEGATED_FROMValue, value);
    }

    private string? DELEGATION_REASONValue;
    public string? DELEGATION_REASON
    {
        get => DELEGATION_REASONValue;
        set => SetProperty(ref DELEGATION_REASONValue, value);
    }

    private DateTime? DELEGATION_DATEValue;
    public DateTime? DELEGATION_DATE
    {
        get => DELEGATION_DATEValue;
        set => SetProperty(ref DELEGATION_DATEValue, value);
    }

    // Escalation support
    private string? ESCALATED_TOValue;
    public string? ESCALATED_TO
    {
        get => ESCALATED_TOValue;
        set => SetProperty(ref ESCALATED_TOValue, value);
    }

    private DateTime? ESCALATION_DATEValue;
    public DateTime? ESCALATION_DATE
    {
        get => ESCALATION_DATEValue;
        set => SetProperty(ref ESCALATION_DATEValue, value);
    }

    private string? ESCALATION_REASONValue;
    public string? ESCALATION_REASON
    {
        get => ESCALATION_REASONValue;
        set => SetProperty(ref ESCALATION_REASONValue, value);
    }

    private DateTime? REQUESTED_DATEValue;
    public DateTime? REQUESTED_DATE
    {
        get => REQUESTED_DATEValue;
        set => SetProperty(ref REQUESTED_DATEValue, value);
    }

    private string? REQUESTED_BYValue;
    public string? REQUESTED_BY
    {
        get => REQUESTED_BYValue;
        set => SetProperty(ref REQUESTED_BYValue, value);
    }

    public PROCESS_APPROVAL() { }
}
