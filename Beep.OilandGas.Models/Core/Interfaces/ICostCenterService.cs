using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for cost center maintenance.
    /// </summary>
    public interface ICostCenterService
    {
        Task<COST_CENTER> CreateCostCenterAsync(COST_CENTER costCenter, string userId, string cn = "PPDM39");
        Task<COST_CENTER?> GetCostCenterAsync(string costCenterId, string cn = "PPDM39");
        Task<List<COST_CENTER>> GetCostCentersAsync(string? fieldId, string cn = "PPDM39");
        Task<COST_CENTER> UpdateCostCenterAsync(COST_CENTER costCenter, string userId, string cn = "PPDM39");
        Task<bool> DeactivateCostCenterAsync(string costCenterId, string userId, string cn = "PPDM39");
    }
}
