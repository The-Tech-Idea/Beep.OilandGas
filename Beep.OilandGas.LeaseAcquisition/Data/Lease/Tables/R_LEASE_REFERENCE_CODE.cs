using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease;

/// <summary>
/// PPDM-style reference list for lease / land-right operational and workflow codes.
/// </summary>
public class R_LEASE_REFERENCE_CODE : ModelEntityBase
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
}
