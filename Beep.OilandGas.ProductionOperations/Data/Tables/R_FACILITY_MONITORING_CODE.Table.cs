using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations;

/// <summary>
/// Monitoring reference-code catalog for measurement and activity types when PPDM standard sets are not sufficient.
/// </summary>
public partial class R_FACILITY_MONITORING_CODE : ModelEntityBase
{
    private string _referenceSet = string.Empty;
    public string REFERENCE_SET
    {
        get => _referenceSet;
        set => SetProperty(ref _referenceSet, value);
    }

    private string _referenceCode = string.Empty;
    public string REFERENCE_CODE
    {
        get => _referenceCode;
        set => SetProperty(ref _referenceCode, value);
    }

    private string _longName = string.Empty;
    public string LONG_NAME
    {
        get => _longName;
        set => SetProperty(ref _longName, value);
    }

    private string _shortName = string.Empty;
    public string SHORT_NAME
    {
        get => _shortName;
        set => SetProperty(ref _shortName, value);
    }
}

