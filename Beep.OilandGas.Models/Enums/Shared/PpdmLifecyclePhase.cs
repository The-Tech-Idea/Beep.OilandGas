using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum PpdmLifecyclePhase
    {
        [Description("Proposed")] Proposed,
        [Description("Active")] Active,
        [Description("Suspended")] Suspended,
        [Description("Production")] Production,
        [Description("Injection")] Injection,
        [Description("Monitoring")] Monitoring,
        [Description("Exploration")] Exploration,
        [Description("Service")] Service,
        [Description("Abandoned")] Abandoned,
        [Description("Unknown")] Unknown
    }
}
