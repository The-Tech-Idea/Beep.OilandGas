using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum WellboreInterface
    {
        [Description("Open Hole (Barefoot)")]
        OpenHole,
        [Description("Cased Hole (Perforated)")]
        CasedHole,
        [Description("Liner (Slotted/PreDrilled/Cemented)")]
        Liner,
        [Description("Intelligent Completion")]
        IntelligentCompletion,
        [Description("Unknown")]
        Unknown
    }
}
