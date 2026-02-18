using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.WellTest
{
    public enum TestQuality
    {
        [Description("Good")]
        Good,
        [Description("Fair")]
        Fair,
        [Description("Poor")]
        Poor,
        [Description("Invalid")]
        Invalid,
        [Description("Unknown")]
        Unknown
    }
}
