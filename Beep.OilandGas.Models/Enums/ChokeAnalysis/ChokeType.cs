using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.ChokeAnalysis
{
    public enum ChokeType
    {
        [Description("Positive")]
        Positive,
        [Description("Adjustable")]
        Adjustable,
        [Description("Bean")]
        Bean,
        [Description("Cage")]
        Cage,
        [Description("Needle")]
        Needle,
        [Description("Plug")]
        Plug,
        [Description("Unknown")]
        Unknown
    }
}
