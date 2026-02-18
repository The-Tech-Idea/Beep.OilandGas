using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum LengthUnit
    {
        [Description("Meter (m)")]
        Meter,
        [Description("Foot (ft)")]
        Foot,
        [Description("Inch (in)")]
        Inch,
        [Description("Kilometer (km)")]
        Kilometer,
        [Description("Mile (mi)")]
        Mile,
        [Description("Unknown")]
        Unknown
    }
}
