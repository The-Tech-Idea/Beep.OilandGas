using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum PermitStatus
    {
        [Description("Draft")] Draft,
        [Description("Open")] Open,
        [Description("Suspended")] Suspended,
        [Description("Closed")] Closed,
        [Description("Cancelled")] Cancelled,
        [Description("Unknown")] Unknown
    }
}
