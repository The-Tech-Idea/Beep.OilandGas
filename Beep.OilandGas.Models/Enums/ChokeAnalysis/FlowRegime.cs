using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.ChokeAnalysis
{
    public enum FlowRegime
    {
        [Description("Critical")]
        Critical,
        [Description("Sub-Critical")]
        SubCritical,
        [Description("Sonic")]
        Sonic,
        [Description("Sub-Sonic")]
        SubSonic,
        [Description("Unknown")]
        Unknown
    }
}
