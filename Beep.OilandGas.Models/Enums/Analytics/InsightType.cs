using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Analytics
{
    public enum InsightType
    {
        [Description("Anomaly")]
        Anomaly,
        [Description("Trend")]
        Trend,
        [Description("Forecast")]
        Forecast,
        [Description("Operational")]
        Operational,
        [Description("Unknown")]
        Unknown
    }
}
