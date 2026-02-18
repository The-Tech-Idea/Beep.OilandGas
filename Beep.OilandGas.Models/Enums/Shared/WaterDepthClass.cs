using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum WaterDepthClass
    {
        [Description("Shallow Water (< 500ft)")]
        ShallowWater,
        [Description("Deepwater (1000-5000ft)")]
        Deepwater,
        [Description("Ultra-Deepwater (> 5000ft)")]
        UltraDeepwater,
        [Description("Onshore")]
        Onshore,
        [Description("Unknown")]
        Unknown
    }
}
