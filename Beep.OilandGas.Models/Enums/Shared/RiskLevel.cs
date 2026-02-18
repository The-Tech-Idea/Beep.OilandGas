using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum RiskLevel
    {
        [Description("Low")] Low,
        [Description("Medium")] Medium,
        [Description("High")] High,
        [Description("Critical")] Critical,
        [Description("Unknown")] Unknown
    }
}
