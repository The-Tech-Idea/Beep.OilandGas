using System.Collections.Generic;
using System.Threading.Tasks;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for decommissioning phase service, field-scoped
    /// </summary>
    public interface IFieldDecommissioningService
    {
        /// <summary>
        /// Get abandoned wells for a specific field
        /// </summary>
        Task<List<WellAbandonmentResponse>> GetAbandonedWellsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Record well abandonment for a well (must belong to the specified field)
        /// </summary>
        Task<WellAbandonmentResponse> AbandonWellForFieldAsync(string fieldId, string wellId, WellAbandonmentRequest abandonmentData, string userId);

        /// <summary>
        /// Get well abandonment record by ID (validates it belongs to the field)
        /// </summary>
        Task<WellAbandonmentResponse?> GetWellAbandonmentForFieldAsync(string fieldId, string abandonmentId);

        /// <summary>
        /// Get decommissioned facilities for a specific field
        /// </summary>
        Task<List<FacilityDecommissioningResponse>> GetDecommissionedFacilitiesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Decommission a facility (must belong to the specified field)
        /// </summary>
        Task<FacilityDecommissioningResponse> DecommissionFacilityForFieldAsync(string fieldId, string facilityId, FacilityDecommissioningRequest decommissionData, string userId);

        /// <summary>
        /// Get facility decommissioning record by ID (validates it belongs to the field)
        /// </summary>
        Task<FacilityDecommissioningResponse?> GetFacilityDecommissioningForFieldAsync(string fieldId, string decommissioningId);

        /// <summary>
        /// Get environmental restoration activities for a specific field
        /// </summary>
        Task<List<EnvironmentalRestorationResponse>> GetEnvironmentalRestorationsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create environmental restoration activity for a specific field
        /// </summary>
        Task<EnvironmentalRestorationResponse> CreateEnvironmentalRestorationForFieldAsync(string fieldId, EnvironmentalRestorationRequest restorationData, string userId);

        /// <summary>
        /// Get decommissioning costs for a specific field
        /// </summary>
        Task<List<DecommissioningCostResponse>> GetDecommissioningCostsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Estimate decommissioning costs for a specific field
        /// Returns DecommissioningCostEstimateResponse DTO
        /// </summary>
        Task<DecommissioningCostEstimateResponse> EstimateCostsForFieldAsync(string fieldId);
    }
}




