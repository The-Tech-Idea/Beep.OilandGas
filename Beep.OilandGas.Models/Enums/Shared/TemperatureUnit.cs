using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum TemperatureUnit
    {
        [Description("Fahrenheit (°F)")]
        Fahrenheit,
        [Description("Celsius (°C)")]
        Celsius,
        [Description("Rankine (°R)")]
        Rankine,
        [Description("Kelvin (K)")]
        Kelvin,
        [Description("Unknown")]
        Unknown
    }
}
