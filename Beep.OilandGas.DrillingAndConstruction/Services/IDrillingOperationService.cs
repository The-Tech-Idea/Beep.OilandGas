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

    /// <summary>
    /// DTO for updating a drilling operation.
    /// </summary>
    public class UpdateDrillingOperationDto
    {
        public string? Status { get; set; }
        public decimal? CurrentDepth { get; set; }
        public decimal? DailyCost { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    /// <summary>
    /// DTO for creating a drilling report.
    /// </summary>
    public class CreateDrillingReportDto
    {
        public DateTime ReportDate { get; set; }
        public decimal? Depth { get; set; }
        public string? Activity { get; set; }
        public decimal? Hours { get; set; }
        public string? Remarks { get; set; }
    }
}

