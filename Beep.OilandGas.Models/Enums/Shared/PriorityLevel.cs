using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum PriorityLevel
    {
        [Description("Low")] Low,
        [Description("Medium")] Medium,
        [Description("High")] High,
        [Description("Urgent")] Urgent,
        [Description("Unknown")] Unknown
    }
}
