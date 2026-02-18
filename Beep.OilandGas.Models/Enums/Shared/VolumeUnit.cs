using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum VolumeUnit
    {
        [Description("Barrel (bbl)")]
        Barrel,
        [Description("Standard Cubic Foot (scf)")]
        StandardCubicFoot,
        [Description("Cubic Meter (m3)")]
        CubicMeter,
        [Description("Thousand Barrels (Mbbl)")]
        ThousandBarrels,
        [Description("Million Standard Cubic Feet (MMscf)")]
        MillionScf,
        [Description("Gallon")]
        Gallon,
        [Description("Liter")]
        Liter,
        [Description("Unknown")]
        Unknown
    }
}
