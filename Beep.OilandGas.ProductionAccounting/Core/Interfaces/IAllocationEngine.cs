using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Allocation engine for distributing production volumes.
    /// Works directly with ALLOCATION_RESULT and ALLOCATION_DETAIL entities.
    /// </summary>
    public interface IAllocationEngine
    {
        /// <summary>
        /// Allocates production from RUN_TICKET to ALLOCATION_RESULT/DETAIL.
        /// </summary>
        Task<ALLOCATION_RESULT> AllocateAsync(
            RUN_TICKET RUN_TICKET,
            string allocationMethod,
            string userId,
            string connectionName = "PPDM39");

        /// <summary>
        /// Gets allocation result by ID.
        /// </summary>
        Task<ALLOCATION_RESULT?> GetAllocationAsync(
            string allocationId,
            string connectionName = "PPDM39");

        /// <summary>
        /// Gets allocation details for a result.
        /// </summary>
        Task<List<ALLOCATION_DETAIL>> GetAllocationDetailsAsync(
            string allocationId,
            string connectionName = "PPDM39");

        /// <summary>
        /// Validates allocation business rules.
        /// </summary>
        Task<bool> ValidateAsync(
            ALLOCATION_RESULT allocation,
            string connectionName = "PPDM39");
    }
}
