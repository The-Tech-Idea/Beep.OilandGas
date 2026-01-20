using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Decommissioning.Services
{
    /// <summary>
    /// Request for creating a well plugging operation.
    /// </summary>
    public class CreateWellPluggingRequest
    {
        public string? WellUWI { get; set; }
    }

    /// <summary>
    /// Service for managing well plugging operations.
    /// </summary>
    public interface IWellPluggingService
    {
        /// <summary>
        /// Gets all well plugging operations.
        /// </summary>
        Task<List<WELL_PLUGBACK>> GetWellPluggingOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Gets a well plugging operation by ID.
        /// </summary>
        Task<WELL_PLUGBACK?> GetWellPluggingOperationAsync(string pluggingId);

        /// <summary>
        /// Creates a new well plugging operation.
        /// </summary>
        Task<WELL_PLUGBACK> CreateWellPluggingOperationAsync(CreateWellPluggingRequest createRequest);

        /// <summary>
        /// Verifies well plugging.
        /// </summary>
        Task<WELL_PLUGBACK> VerifyWellPluggingAsync(string pluggingId, string verifiedBy, bool passed);

        /// <summary>
        /// Gets facility decommissioning operations.
        /// </summary>
        Task<List<FacilityDecommissioningResponse>> GetFacilityDecommissioningOperationsAsync(string? facilityId = null);

        /// <summary>
        /// Gets site restoration operations.
        /// </summary>
        Task<List<EnvironmentalRestorationResponse>> GetSiteRestorationOperationsAsync(string? siteId = null);

        /// <summary>
        /// Gets abandonment operations.
        /// </summary>
        Task<List<WellAbandonmentResponse>> GetAbandonmentOperationsAsync(string? wellUWI = null);
    }
}

