using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IInspectionService
{
    /// <summary>
    /// Seed the standard checklist for the WO type.
    /// Called automatically when a WO transitions to PLANNED.
    /// </summary>
    Task SeedChecklistAsync(string instanceId, string woType,
        string jurisdiction, string userId);

    /// <summary>Record the result (PASS/FAIL/NA) for a single inspection item.</summary>
    Task RecordResultAsync(string instanceId, int condSeq,
        string result, string? notes, string userId);

    /// <summary>
    /// Returns true when all items of the given condTypes are in status PASS.
    /// Used as a state machine guard.
    /// </summary>
    Task<bool> AllConditionsPassedAsync(string instanceId, IEnumerable<string> condTypes);

    Task<List<InspectionCondition>> GetChecklistAsync(string instanceId);
}
