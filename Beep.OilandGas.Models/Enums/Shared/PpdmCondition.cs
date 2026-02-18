using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum PpdmCondition
    {
        [Description("Permitted")] Permitted,
        [Description("Cancelled")] Cancelled,
        [Description("Spudded")] Spudded,
        [Description("Active")] Active,
        [Description("Cased / Drilled")] Cased,
        [Description("Shut-In")] ShutIn,
        [Description("Mechanically Plugged")] MechanicallyPlugged,
        [Description("Permanently Sealed")] PermanentlySealed,
        [Description("Dry")] Dry,
        [Description("Mechanical Failure")] MechanicalFailure,
        [Description("Unknown")] Unknown
    }
}
