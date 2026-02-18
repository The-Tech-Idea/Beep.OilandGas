using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Analytics
{
    public enum InsightSeverity
    {
        [Description("Low")]
        Low,
        [Description("Medium")]
        Medium,
        [Description("High")]
        High,
        [Description("Critical")]
        Critical,
        [Description("Unknown")]
        Unknown
    }
}
