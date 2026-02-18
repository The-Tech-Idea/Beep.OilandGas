using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum SpecializedWellRole
    {
        [Description("Observation (Monitoring)")]
        Observation,
        [Description("Stripper (Low volume)")]
        Stripper,
        [Description("Relief (Blowout intervention)")]
        Relief,
        [Description("Disposal")]
        Disposal,
        [Description("Water Supply")]
        WaterSupply,
        [Description("Stratigraphic")]
        Stratigraphic,
        [Description("None")]
        None
    }
}
