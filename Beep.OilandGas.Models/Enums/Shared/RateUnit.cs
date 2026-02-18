using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum RateUnit
    {
        [Description("Barrels of Oil Per Day (BOPD)")]
        BOPD,
        [Description("Barrels of Water Per Day (BWPD)")]
        BWPD,
        [Description("Thousand Cubic Feet Per Day (MCFD)")]
        MCFD,
        [Description("Million Cubic Feet Per Day (MMCFD)")]
        MMCFD,
        [Description("Barrels of Oil Equivalent (BOE)")]
        BOE,
        [Description("Cubic Meters Per Day (m3/d)")]
        CubicMetersPerDay,
        [Description("Unknown")]
        Unknown
    }
}
