using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IContractorManagementService
{
    Task<ContractorAssignment> AssignContractorAsync(
        string instanceId, string stepId, string baId,
        string roleCode, string userId);

    Task RemoveContractorAsync(
        string instanceId, string stepId, string baId, string userId);

    Task<List<ContractorAssignment>> GetAssignmentsAsync(string instanceId);

    /// <summary>
    /// Validates that the contractor has required licenses for the WO type and jurisdiction.
    /// </summary>
    Task<ContractorQualificationResult> ValidateContractorAsync(
        string baId, string woType, string jurisdiction);

    /// <summary>
    /// Returns true if at least one contractor is assigned to the given WO instance.
    /// Used as a state machine guard.
    /// </summary>
    Task<bool> HasContractorAssignedAsync(string instanceId);
}
