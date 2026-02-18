using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum OffshoreStructureType
    {
        [Description("Fixed Platform (Bottom-Supported)")]
        FixedPlatform,
        [Description("Compliant Tower (Bottom-Supported)")]
        CompliantTower,
        [Description("Jack-Up Rig (Bottom-Supported)")]
        JackUpRig,
        [Description("Tension Leg Platform (TLP - Floating)")]
        TensionLegPlatform,
        [Description("Spar Platform (Floating)")]
        SparPlatform,
        [Description("FPSO (Floating)")]
        FPSO,
        [Description("Drillship (Floating)")]
        Drillship,
        [Description("Semi-Submersible (Floating)")]
        SemiSubmersible,
        [Description("None")]
        None
    }
}
