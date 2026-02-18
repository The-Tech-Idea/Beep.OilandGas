using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum RiskMatrixLevel
    {
        [Description("Low (Green)")] Low,
        [Description("Medium (Yellow)")] Medium,
        [Description("High (Red)")] High,
        [Description("Unknown")] Unknown
    }
}
