using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.Decommissioning.Services
{
    /// <summary>
    /// Service for managing well plugging operations.
    /// </summary>
    public interface IWellPluggingService
    {
        /// <summary>
        /// Gets all well plugging operations.
        /// </summary>
        Task<List<WellPluggingDto>> GetWellPluggingOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Gets a well plugging operation by ID.
        /// </summary>
        Task<WellPluggingDto?> GetWellPluggingOperationAsync(string pluggingId);

        /// <summary>
        /// Creates a new well plugging operation.
        /// </summary>
        Task<WellPluggingDto> CreateWellPluggingOperationAsync(CreateWellPluggingDto createDto);

        /// <summary>
        /// Verifies well plugging.
        /// </summary>
        Task<WellPluggingDto> VerifyWellPluggingAsync(string pluggingId, string verifiedBy, bool passed);

        /// <summary>
        /// Gets facility decommissioning operations.
        /// </summary>
        Task<List<FacilityDecommissioningDto>> GetFacilityDecommissioningOperationsAsync(string? facilityId = null);

        /// <summary>
        /// Gets site restoration operations.
        /// </summary>
        Task<List<SiteRestorationDto>> GetSiteRestorationOperationsAsync(string? siteId = null);

        /// <summary>
        /// Gets abandonment operations.
        /// </summary>
        Task<List<AbandonmentDto>> GetAbandonmentOperationsAsync(string? wellUWI = null);
    }
}

