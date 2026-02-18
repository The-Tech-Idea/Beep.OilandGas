using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum WellStatus
    {
        [Description("Active - Actively producing or injecting")]
        Active,
        [Description("Drilling - Drilling operations underway")]
        Drilling,
        [Description("Completed - Equipped not yet producing")]
        Completed,
        [Description("ShutIn - Capable of production but closed")]
        ShutIn,
        [Description("Suspended - Operations halted, maintained")]
        Suspended,
        [Description("Temporarily Abandoned - Sealed, potential to reactivate")]
        TemporarilyAbandoned,
        [Description("Plugged and Abandoned - Permanently sealed")]
        PluggedAndAbandoned,
        [Description("Dry Hole - No commercial hydrocarbons")]
        DryHole,
        [Description("Permit - Licensed but not drilled")]
        Permit,
        [Description("Injection - Injection Well")]
        Injection,
        [Description("Waiting on Completion")]
        WaitingOnCompletion,
        [Description("Abandoned Location")]
        AbandonedLocation,
        [Description("Unknown")]
        Unknown
    }
}
