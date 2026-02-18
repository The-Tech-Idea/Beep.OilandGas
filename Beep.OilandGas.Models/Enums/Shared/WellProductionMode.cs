using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum WellProductionMode
    {
        [Description("Single Zone")]
        SingleZone,
        [Description("Multi Zone (Dual/Triple)")]
        MultiZone,
        [Description("Tubingless")]
        Tubingless,
        [Description("Intelligent (Real-time control)")]
        Intelligent,
        [Description("Unknown")]
        Unknown
    }
}
