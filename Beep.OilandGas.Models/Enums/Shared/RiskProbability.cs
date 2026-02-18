using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum RiskProbability
    {
        [Description("Rare")] Rare,
        [Description("Unlikely")] Unlikely,
        [Description("Possible")] Possible,
        [Description("Likely")] Likely,
        [Description("Almost Certain")] AlmostCertain,
        [Description("Unknown")] Unknown
    }
}
