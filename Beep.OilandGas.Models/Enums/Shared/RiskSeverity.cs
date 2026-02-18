using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum RiskSeverity
    {
        [Description("Insignificant (Level 1)")] Insignificant,
        [Description("Minor (Level 2)")] Minor,
        [Description("Moderate (Level 3)")] Moderate,
        [Description("Major (Level 4)")] Major,
        [Description("Catastrophic (Level 5)")] Catastrophic,
        [Description("Unknown")] Unknown
    }
}
