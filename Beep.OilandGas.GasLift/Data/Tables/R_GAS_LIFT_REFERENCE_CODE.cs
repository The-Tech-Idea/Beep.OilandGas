using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.GasLift.Data;

/// <summary>
/// Extension reference list for gas lift LOVs (port sizes, operating mode, design method, valve role).
/// Seeded by <see cref="T:Beep.OilandGas.GasLift.Modules.GasLiftModule"/>. Physical DDL follows entity-driven tooling — see root <c>CLAUDE.md</c>.
/// </summary>
public partial class R_GAS_LIFT_REFERENCE_CODE : ModelEntityBase
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
