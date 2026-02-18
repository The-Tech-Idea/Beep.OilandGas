using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum DensityUnit
    {
        [Description("Pounds per Gallon (ppg)")]
        Ppg,
        [Description("Specific Gravity (SG)")]
        SpecificGravity,
        [Description("API Gravity")]
        APIGravity,
        [Description("Kilogram per Cubic Meter (kg/m3)")]
        KilogramPerCubicMeter,
        [Description("Unknown")]
        Unknown
    }
}
