using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IWorkOrderService
{
    Task<WorkOrderSummary> CreateAsync(CreateWorkOrderRequest request, string userId);

    Task<WorkOrderDetailModel?> GetByIdAsync(string fieldId, string instanceId);

    Task<List<WorkOrderSummary>> GetByFieldAsync(
        string fieldId, string? state = null, string? woSubType = null);

    /// <summary>
    /// Transition the work order to a new state, enforcing guards.
    /// </summary>
    Task<WorkOrderSummary> TransitionStateAsync(
        string fieldId, string instanceId, string toState, string userId, string? notes = null);

    Task DeleteAsync(string fieldId, string instanceId, string userId);

    /// <summary>
    /// Returns valid next states from the current state for the given WO sub-type.
    /// </summary>
    List<string> GetAvailableTransitions(string currentState, string woSubType);
}
