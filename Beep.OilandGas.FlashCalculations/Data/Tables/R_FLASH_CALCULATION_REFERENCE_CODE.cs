using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.FlashCalculations.Data;

/// <summary>
/// Extension reference list for flash/PVT LOVs (EOS model, calculation category, solver labels).
/// Seeded by <c>FlashCalculationsModule</c>. Align picklists with <c>FlashCalculationOptions.EquationOfState</c> and orchestration wire types.
/// </summary>
public partial class R_FLASH_CALCULATION_REFERENCE_CODE : ModelEntityBase
{
    private string REFERENCE_SETValue = string.Empty;

    public string REFERENCE_SET
    {
        get => REFERENCE_SETValue;
        set => SetProperty(ref REFERENCE_SETValue, value);
    }

    private string REFERENCE_CODEValue = string.Empty;

    public string REFERENCE_CODE
    {
        get => REFERENCE_CODEValue;
        set => SetProperty(ref REFERENCE_CODEValue, value);
    }

    private string LONG_NAMEValue = string.Empty;

    public string LONG_NAME
    {
        get => LONG_NAMEValue;
        set => SetProperty(ref LONG_NAMEValue, value);
    }
}
