using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface ICostCaptureService
{
    /// <summary>Create or update the AFE finance record for a work order.</summary>
    Task UpsertAFEAsync(string instanceId, decimal budgetAmount, string userId);

    /// <summary>Add a cost code line item to a work order's AFE.</summary>
    Task AddCostLineAsync(string instanceId, string compCode,
        decimal budgetAmt, string description, string userId);

    /// <summary>Record actual cost against a cost code line.</summary>
    Task UpdateActualCostAsync(string instanceId, string compCode,
        decimal actualAmt, string userId);

    Task<List<CostVarianceLine>> GetVarianceSummaryAsync(string instanceId);

    /// <summary>True if a FINANCE record exists for this WO (AFE guard).</summary>
    Task<bool> HasAFEAsync(string instanceId);

    /// <summary>Sum of BudgetAmt across all FIN_COMPONENT rows for this WO.</summary>
    Task<decimal> GetEstimatedCostAsync(string instanceId);
}
