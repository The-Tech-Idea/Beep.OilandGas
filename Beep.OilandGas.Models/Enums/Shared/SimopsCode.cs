using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum SimopsCode
    {
        [Description("Allowed (Green)")] Allowed,
        [Description("Restricted (Yellow)")] Restricted,
        [Description("Prohibited (Red)")] Prohibited,
        [Description("Unknown")] Unknown
    }
}
