using System;
using System.Collections.Generic;
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
    }

    /// <summary>
    /// Service for managing production operations.
    /// </summary>
    public interface IProductionManagementService
    {
        /// <summary>
        /// Gets production operations.
        /// </summary>
        Task<List<PDEN>> GetProductionOperationsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets a production operation by ID.
        /// </summary>
        Task<PDEN?> GetProductionOperationAsync(string operationId);

        /// <summary>
        /// Creates a new production operation.
        /// </summary>
        Task<PDEN> CreateProductionOperationAsync(CreateProductionOperationRequest createRequest);

        /// <summary>
        /// Gets production reports.
        /// </summary>
        Task<List<PDEN>> GetProductionReportsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets well operations.
        /// </summary>
        Task<List<PDEN>> GetWellOperationsAsync(string wellUWI);

        /// <summary>
        /// Gets facility operations.
        /// </summary>
        Task<List<FACILITY>> GetFacilityOperationsAsync(string facilityId);
    }
}

