using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum WellTrajectory
    {
        [Description("Vertical")]
        Vertical,
        [Description("Directional (Deviated)")]
        Directional,
        [Description("Horizontal")]
        Horizontal,
        [Description("Multilateral")]
        Multilateral,
        [Description("Unknown")]
        Unknown
    }
}
