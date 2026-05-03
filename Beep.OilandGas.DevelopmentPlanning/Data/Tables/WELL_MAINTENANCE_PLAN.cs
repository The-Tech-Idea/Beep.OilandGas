using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning;

public partial class WELL_MAINTENANCE_PLAN : ModelEntityBase
{
    private string MAINT_PLAN_IDValue = string.Empty;
    public string MAINT_PLAN_ID
    {
        get => MAINT_PLAN_IDValue;
        set => SetProperty(ref MAINT_PLAN_IDValue, value);
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

    private string MAINTENANCE_TYPEValue = string.Empty;
    public string MAINTENANCE_TYPE
    {
        get => MAINTENANCE_TYPEValue;
        set => SetProperty(ref MAINTENANCE_TYPEValue, value);
    }

    private string MAINTENANCE_STATUSValue = string.Empty;
    public string MAINTENANCE_STATUS
    {
        get => MAINTENANCE_STATUSValue;
        set => SetProperty(ref MAINTENANCE_STATUSValue, value);
    }

    private string PRIORITYValue = string.Empty;
    public string PRIORITY
    {
        get => PRIORITYValue;
        set => SetProperty(ref PRIORITYValue, value);
    }

    private string TRIGGER_BASISValue = string.Empty;
    public string TRIGGER_BASIS
    {
        get => TRIGGER_BASISValue;
        set => SetProperty(ref TRIGGER_BASISValue, value);
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

    private string SERVICE_BA_IDValue = string.Empty;
    public string SERVICE_BA_ID
    {
        get => SERVICE_BA_IDValue;
        set => SetProperty(ref SERVICE_BA_IDValue, value);
    }
}
