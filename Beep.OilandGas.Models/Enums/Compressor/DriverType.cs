using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Compressor
{
    public enum DriverType
    {
        [Description("Electric Motor")]
        ElectricMotor,
        [Description("Gas Engine")]
        GasEngine,
        [Description("Gas Turbine")]
        GasTurbine,
        [Description("Steam Turbine")]
        SteamTurbine,
        [Description("Unknown")]
        Unknown
    }
}
