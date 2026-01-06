using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

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
        Task<List<DrillingOperationDto>> GetDrillingOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Gets a drilling operation by ID (UWI).
        /// </summary>
        /// <param name="operationId">The operation ID (UWI)</param>
        /// <returns>The drilling operation, or null if not found</returns>
        Task<DrillingOperationDto?> GetDrillingOperationAsync(string operationId);

        /// <summary>
        /// Creates a new drilling operation.
        /// </summary>
        /// <param name="createDto">The drilling operation creation data</param>
        /// <returns>The created drilling operation</returns>
        Task<DrillingOperationDto> CreateDrillingOperationAsync(CreateDrillingOperationDto createDto);

        /// <summary>
        /// Updates an existing drilling operation.
        /// </summary>
        /// <param name="operationId">The operation ID (UWI)</param>
        /// <param name="updateDto">The drilling operation update data</param>
        /// <returns>The updated drilling operation</returns>
        Task<DrillingOperationDto> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperationDto updateDto);

        /// <summary>
        /// Gets all drilling reports for a specific operation.
        /// </summary>
        /// <param name="operationId">The operation ID (UWI)</param>
        /// <returns>List of drilling reports</returns>
        Task<List<DrillingReportDto>> GetDrillingReportsAsync(string operationId);

        /// <summary>
        /// Creates a new drilling report for an operation.
        /// </summary>
        /// <param name="operationId">The operation ID (UWI)</param>
        /// <param name="createDto">The drilling report creation data</param>
        /// <returns>The created drilling report</returns>
        Task<DrillingReportDto> CreateDrillingReportAsync(string operationId, CreateDrillingReportDto createDto);
    }
}




