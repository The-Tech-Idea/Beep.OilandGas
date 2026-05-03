using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning;

public partial class WELL_SERVICE_JOB : ModelEntityBase
{
    private string JOB_IDValue = string.Empty;
    public string JOB_ID
    {
        get => JOB_IDValue;
        set => SetProperty(ref JOB_IDValue, value);
    }

    private string FDP_IDValue = string.Empty;
    public string FDP_ID
    {
        get => FDP_IDValue;
        set => SetProperty(ref FDP_IDValue, value);
    }

    private string UWIValue = string.Empty;
    public string UWI
    {
        get => UWIValue;
        set => SetProperty(ref UWIValue, value);
    }

    private string JOB_TYPEValue = string.Empty;
    public string JOB_TYPE
    {
        get => JOB_TYPEValue;
        set => SetProperty(ref JOB_TYPEValue, value);
    }

    private string JOB_STATUSValue = string.Empty;
    public string JOB_STATUS
    {
        get => JOB_STATUSValue;
        set => SetProperty(ref JOB_STATUSValue, value);
    }

    private string PRIORITYValue = string.Empty;
    public string PRIORITY
    {
        get => PRIORITYValue;
        set => SetProperty(ref PRIORITYValue, value);
    }

    private string SERVICE_BA_IDValue = string.Empty;
    public string SERVICE_BA_ID
    {
        get => SERVICE_BA_IDValue;
        set => SetProperty(ref SERVICE_BA_IDValue, value);
    }

    private string BA_SERVICE_TYPEValue = string.Empty;
    public string BA_SERVICE_TYPE
    {
        get => BA_SERVICE_TYPEValue;
        set => SetProperty(ref BA_SERVICE_TYPEValue, value);
    }

    private DateTime? PLANNED_START_DATEValue;
    public DateTime? PLANNED_START_DATE
    {
        get => PLANNED_START_DATEValue;
        set => SetProperty(ref PLANNED_START_DATEValue, value);
    }

    private DateTime? PLANNED_END_DATEValue;
    public DateTime? PLANNED_END_DATE
    {
        get => PLANNED_END_DATEValue;
        set => SetProperty(ref PLANNED_END_DATEValue, value);
    }

    private string LINKED_ACTIVITY_IDValue = string.Empty;
    public string LINKED_ACTIVITY_ID
    {
        get => LINKED_ACTIVITY_IDValue;
        set => SetProperty(ref LINKED_ACTIVITY_IDValue, value);
    }
}
