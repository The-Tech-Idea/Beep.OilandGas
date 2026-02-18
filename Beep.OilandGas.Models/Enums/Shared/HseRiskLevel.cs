using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum HseRiskLevel
    {
        [Description("Level 1 (Insignificant)")] Level1,
        [Description("Level 2 (Minor)")] Level2,
        [Description("Level 3 (Moderate)")] Level3,
        [Description("Level 4 (Major)")] Level4,
        [Description("Level 5 (Catastrophic)")] Level5,
        [Description("Unknown")] Unknown
    }
}
