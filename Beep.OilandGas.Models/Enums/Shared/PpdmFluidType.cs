using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum PpdmFluidType
    {
        [Description("Mud")] Mud,
        [Description("Oil")] Oil,
        [Description("Gas")] Gas,
        [Description("MultiPhase")] MultiPhase,
        [Description("Coal Bed Methane")] CoalBedMethane,
        [Description("Condensate")] Condensate,
        [Description("Bitumen")] Bitumen,
        [Description("Water")] Water,
        [Description("Brine (Waste Water)")] Brine,
        [Description("CO2")] CO2,
        [Description("H2S")] H2S,
        [Description("Steam")] Steam,
        [Description("None")] None,
        [Description("Unknown")] Unknown
    }
}
