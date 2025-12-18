using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.DrillingAndConstruction.Services
{
    /// <summary>
    /// Service for managing drilling operations.
    /// </summary>
    public interface IDrillingOperationService
    {
        /// <summary>
        /// Gets all drilling operations.
        /// </summary>
        Task<List<DrillingOperationDto>> GetDrillingOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Gets a drilling operation by ID.
        /// </summary>
        Task<DrillingOperationDto?> GetDrillingOperationAsync(string operationId);

        /// <summary>
        /// Creates a new drilling operation.
        /// </summary>
        Task<DrillingOperationDto> CreateDrillingOperationAsync(CreateDrillingOperationDto createDto);

        /// <summary>
        /// Updates a drilling operation.
        /// </summary>
        Task<DrillingOperationDto> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperationDto updateDto);

        /// <summary>
        /// Gets drilling reports for an operation.
        /// </summary>
        Task<List<DrillingReportDto>> GetDrillingReportsAsync(string operationId);

        /// <summary>
        /// Creates a drilling report.
        /// </summary>
        Task<DrillingReportDto> CreateDrillingReportAsync(string operationId, CreateDrillingReportDto createDto);
    }
}

