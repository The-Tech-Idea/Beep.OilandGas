using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum CompletionStatus
    {
        [Description("Active Producer")]
        ActiveProducer,
        [Description("Active Injector")]
        ActiveInjector,
        [Description("Active Disposal")]
        ActiveDisposal,
        [Description("Active Observation")]
        ActiveObservation,
        [Description("Active Gas Storage")]
        ActiveGasStorage,
        [Description("Shut-In")]
        ShutIn,
        [Description("Drilling")]
        Drilling,
        [Description("Waiting on Completion")]
        WaitingOnCompletion,
        [Description("Drilled Uncompleted")]
        DrilledUncompleted,
        [Description("Permitted Location")]
        PermittedLocation,
        [Description("Inactive")]
        Inactive,
        [Description("Temporarily Abandoned")]
        TemporarilyAbandoned,
        [Description("Dry Hole")]
        DryHole,
        [Description("Plugged and Abandoned")]
        PluggedAndAbandoned,
        [Description("Abandoned and Junked")]
        AbandonedAndJunked,
        [Description("Cancelled Permit")]
        CancelledPermit,
        [Description("Completed - Not Active")]
        CompletedNotActive,
        [Description("Suspended Operations")]
        SuspendedOperations,
        [Description("Capped")]
        Capped,
        [Description("Proposed")]
        Proposed,
        [Description("Unknown")]
        Unknown
    }
}
