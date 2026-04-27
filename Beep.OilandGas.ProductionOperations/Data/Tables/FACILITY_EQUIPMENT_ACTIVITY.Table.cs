using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations;

/// <summary>
/// Equipment installation lifecycle history (install, uninstall, move, replace) for facilities.
/// </summary>
public partial class FACILITY_EQUIPMENT_ACTIVITY : ModelEntityBase
{
    private string _activityId = string.Empty;
    public string ACTIVITY_ID
    {
        get => _activityId;
        set => SetProperty(ref _activityId, value);
    }

    private string _facilityId = string.Empty;
    public string FACILITY_ID
    {
        get => _facilityId;
        set => SetProperty(ref _facilityId, value);
    }

    private string _facilityType = string.Empty;
    public string FACILITY_TYPE
    {
        get => _facilityType;
        set => SetProperty(ref _facilityType, value);
    }

    private string _equipmentId = string.Empty;
    public string EQUIPMENT_ID
    {
        get => _equipmentId;
        set => SetProperty(ref _equipmentId, value);
    }

    private string _activityType = string.Empty;
    public string ACTIVITY_TYPE
    {
        get => _activityType;
        set => SetProperty(ref _activityType, value);
    }

    private DateTime? _activityDate;
    public DateTime? ACTIVITY_DATE
    {
        get => _activityDate;
        set => SetProperty(ref _activityDate, value);
    }

    private DateTime? _endDate;
    public DateTime? END_DATE
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    private decimal _installSequence;
    public decimal INSTALL_SEQUENCE
    {
        get => _installSequence;
        set => SetProperty(ref _installSequence, value);
    }

    private string _locationDesc = string.Empty;
    public string LOCATION_DESC
    {
        get => _locationDesc;
        set => SetProperty(ref _locationDesc, value);
    }

    private string _positionDesc = string.Empty;
    public string POSITION_DESC
    {
        get => _positionDesc;
        set => SetProperty(ref _positionDesc, value);
    }

    private string _reasonText = string.Empty;
    public string REASON_TEXT
    {
        get => _reasonText;
        set => SetProperty(ref _reasonText, value);
    }

    private string _sourceSystem = string.Empty;
    public string SOURCE_SYSTEM
    {
        get => _sourceSystem;
        set => SetProperty(ref _sourceSystem, value);
    }

    private string _detailsJson = string.Empty;
    public string DETAILS_JSON
    {
        get => _detailsJson;
        set => SetProperty(ref _detailsJson, value);
    }
}

