using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum PressureUnit
    {
        [Description("Pounds per square inch (psi)")]
        Psi,
        [Description("Bar")]
        Bar,
        [Description("Pascal (Pa)")]
        Pascal,
        [Description("Kilo Pascal (kPa)")]
        KiloPascal,
        [Description("Mega Pascal (MPa)")]
        MegaPascal,
        [Description("Atmosphere (atm)")]
        Atmosphere,
        [Description("Unknown")]
        Unknown
    }
}
