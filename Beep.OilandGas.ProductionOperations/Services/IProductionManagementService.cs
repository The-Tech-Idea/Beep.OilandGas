using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionOperations.Services
{
    /// <summary>
    /// Request for creating a production operation.
    /// </summary>
    public class CreateProductionOperationRequest
    {
        public string? WellUWI { get; set; }
        public DateTime? OperationDate { get; set; }
        public string? OperationType { get; set; }
        public string? Status { get; set; }
        public string? AssignedTo { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Service for managing production operations.
    /// </summary>
    public interface IProductionManagementService
    {
        /// <summary>
        /// Gets production operations.
        /// </summary>
        Task<List<PDEN>> GetProductionOperationsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a production operation by ID.
        /// </summary>
        Task<PDEN?> GetProductionOperationAsync(string operationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new production operation.
        /// </summary>
        Task<PDEN> CreateProductionOperationAsync(CreateProductionOperationRequest createRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets production reports.
        /// </summary>
        Task<List<PDEN>> GetProductionReportsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets well operations.
        /// </summary>
        Task<List<PDEN>> GetWellOperationsAsync(string wellUWI, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets facility master row(s) for the given facility identifier.
        /// </summary>
        Task<List<FACILITY>> GetFacilityOperationsAsync(string facilityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists PDEN rows used for facility-level production reporting (<c>PDEN_SUBTYPE = FACILITY</c>).
        /// </summary>
        Task<IReadOnlyList<PDEN>> ListFacilityPdenDeclarationsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    }
}

