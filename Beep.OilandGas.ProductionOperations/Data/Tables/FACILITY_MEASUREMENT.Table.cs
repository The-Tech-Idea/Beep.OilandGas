using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations;

/// <summary>
/// Vertical facility/equipment measurement history (for example tank level, pressure, temperature).
/// </summary>
public partial class FACILITY_MEASUREMENT : ModelEntityBase
{
    private string _measurementId = string.Empty;
    public string MEASUREMENT_ID
    {
        get => _measurementId;
        set => SetProperty(ref _measurementId, value);
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

    private string _measurementType = string.Empty;
    public string MEASUREMENT_TYPE
    {
        get => _measurementType;
        set => SetProperty(ref _measurementType, value);
    }

    private decimal _measuredValue;
    public decimal MEASURED_VALUE
    {
        get => _measuredValue;
        set => SetProperty(ref _measuredValue, value);
    }

    private string _measuredUom = string.Empty;
    public string MEASURED_UOM
    {
        get => _measuredUom;
        set => SetProperty(ref _measuredUom, value);
    }

    private DateTime? _measuredDate;
    public DateTime? MEASURED_DATE
    {
        get => _measuredDate;
        set => SetProperty(ref _measuredDate, value);
    }

    private string _qualityCode = string.Empty;
    public string QUALITY_CODE
    {
        get => _qualityCode;
        set => SetProperty(ref _qualityCode, value);
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

