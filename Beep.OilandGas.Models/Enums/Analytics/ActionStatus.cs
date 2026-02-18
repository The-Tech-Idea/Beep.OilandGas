using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Analytics
{
    public enum ActionStatus
    {
        [Description("Pending")]
        Pending,
        [Description("In Progress")]
        InProgress,
        [Description("Completed")]
        Completed,
        [Description("Cancelled")]
        Cancelled,
        [Description("Unknown")]
        Unknown
    }
}
