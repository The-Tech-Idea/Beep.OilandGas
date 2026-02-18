using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.WellTest
{
    public enum WellTestCategory
    {
        [Description("Productivity")] Productivity,
        [Description("Reservoir (PTA)")] Reservoir,
        [Description("Early Life")] EarlyLife,
        [Description("Mechanical")] Mechanical,
        [Description("Subsurface")] Subsurface,
        [Description("Fluid Analysis")] FluidAnalysis,
        [Description("Inflow Test")] InflowTest,
        [Description("Extended (EWT)")] Extended,
        [Description("Unknown")] Unknown
    }
}
