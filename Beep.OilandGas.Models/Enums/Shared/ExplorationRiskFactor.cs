using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum ExplorationRiskFactor
    {
        [Description("Source Rock")] SourceRock,
        [Description("Reservoir")] Reservoir,
        [Description("Trap")] Trap,
        [Description("Seal")] Seal,
        [Description("Migration")] Migration,
        [Description("None")] None
    }
}
