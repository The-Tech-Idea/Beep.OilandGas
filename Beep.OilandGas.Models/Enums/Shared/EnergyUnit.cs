using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum EnergyUnit
    {
        [Description("British Thermal Unit (BTU)")]
        BTU,
        [Description("Million BTU (MMBtu)")]
        MMBtu,
        [Description("Therm (100,000 BTU)")]
        Therm,
        [Description("Joule (J)")]
        Joule,
        [Description("Kilowatt Hour (kWh)")]
        KilowattHour,
        [Description("Unknown")]
        Unknown
    }
}
