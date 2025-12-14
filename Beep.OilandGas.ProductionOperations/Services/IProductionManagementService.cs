using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.ProductionOperations.Services
{
    /// <summary>
    /// Service for managing production operations.
    /// </summary>
    public interface IProductionManagementService
    {
        /// <summary>
        /// Gets production operations.
        /// </summary>
        Task<List<ProductionOperationDto>> GetProductionOperationsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets a production operation by ID.
        /// </summary>
        Task<ProductionOperationDto?> GetProductionOperationAsync(string operationId);

        /// <summary>
        /// Creates a new production operation.
        /// </summary>
        Task<ProductionOperationDto> CreateProductionOperationAsync(CreateProductionOperationDto createDto);

        /// <summary>
        /// Gets production reports.
        /// </summary>
        Task<List<ProductionReportDto>> GetProductionReportsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets well operations.
        /// </summary>
        Task<List<WellOperationDto>> GetWellOperationsAsync(string wellUWI);

        /// <summary>
        /// Gets facility operations.
        /// </summary>
        Task<List<FacilityOperationDto>> GetFacilityOperationsAsync(string facilityId);
    }
}

