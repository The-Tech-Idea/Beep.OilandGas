using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Drilling;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing drilling operations.
    /// Provides comprehensive drilling operation management including planning, execution, reporting, and cost tracking.
    /// </summary>
    public interface IDrillingOperationService
    {
        /// <summary>
        /// Gets all drilling operations, optionally filtered by well UWI.
        /// </summary>
        /// <param name="wellUWI">Optional well UWI to filter operations</param>
        /// <returns>List of drilling operations</returns>
        Task<List<DRILLING_OPERATION>> GetDrillingOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Gets a drilling operation by ID (UWI).
        /// </summary>
        /// <param name="operationId">The operation ID (UWI)</param>
        /// <returns>The drilling operation, or null if not found</returns>
        Task<DRILLING_OPERATION?> GetDrillingOperationAsync(string operationId);

        /// <summary>
        /// Creates a new drilling operation.
        /// </summary>
        /// <param name="createDto">The drilling operation creation data</param>
        /// <returns>The created drilling operation</returns>
        Task<DRILLING_OPERATION> CreateDrillingOperationAsync(CREATE_DRILLING_OPERATION createDto);

        /// <summary>
        /// Updates an existing drilling operation.
        /// </summary>
        /// <param name="operationId">The operation ID (UWI)</param>
        /// <param name="updateDto">The drilling operation update data</param>
        /// <returns>The updated drilling operation</returns>
        Task<DRILLING_OPERATION> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperation updateDto);

        /// <summary>
        /// Gets all drilling reports for a specific operation.
        /// </summary>
        /// <param name="operationId">The operation ID (UWI)</param>
        /// <returns>List of drilling reports</returns>
        Task<List<DRILLING_REPORT>> GetDrillingReportsAsync(string operationId);

        /// <summary>
        /// Creates a new drilling report for an operation.
        /// </summary>
        /// <param name="operationId">The operation ID (UWI)</param>
        /// <param name="createDto">The drilling report creation data</param>
        /// <returns>The created drilling report</returns>
        Task<DRILLING_REPORT> CreateDrillingReportAsync(string operationId, CreateDrillingReport createDto);
    }
}




