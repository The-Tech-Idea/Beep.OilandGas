using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum PpdmDirection
    {
        [Description("Up (Production)")] Up,
        [Description("Down (Injection)")] Down,
        [Description("None")] None,
        [Description("Unknown")] Unknown
    }
}
