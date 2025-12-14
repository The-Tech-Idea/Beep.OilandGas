using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.DevelopmentPlanning.Services
{
    /// <summary>
    /// Service for managing development plans.
    /// </summary>
    public interface IDevelopmentPlanService
    {
        /// <summary>
        /// Gets all development plans.
        /// </summary>
        Task<List<DevelopmentPlanDto>> GetDevelopmentPlansAsync(string? fieldId = null);

        /// <summary>
        /// Gets a development plan by ID.
        /// </summary>
        Task<DevelopmentPlanDto?> GetDevelopmentPlanAsync(string planId);

        /// <summary>
        /// Creates a new development plan.
        /// </summary>
        Task<DevelopmentPlanDto> CreateDevelopmentPlanAsync(CreateDevelopmentPlanDto createDto);

        /// <summary>
        /// Updates a development plan.
        /// </summary>
        Task<DevelopmentPlanDto> UpdateDevelopmentPlanAsync(string planId, UpdateDevelopmentPlanDto updateDto);

        /// <summary>
        /// Approves a development plan.
        /// </summary>
        Task<DevelopmentPlanDto> ApproveDevelopmentPlanAsync(string planId, string approvedBy);

        /// <summary>
        /// Gets well plans for a development plan.
        /// </summary>
        Task<List<WellPlanDto>> GetWellPlansAsync(string planId);

        /// <summary>
        /// Gets facility plans for a development plan.
        /// </summary>
        Task<List<FacilityPlanDto>> GetFacilityPlansAsync(string planId);

        /// <summary>
        /// Gets permit applications for a development plan.
        /// </summary>
        Task<List<PermitApplicationDto>> GetPermitApplicationsAsync(string planId);
    }
}

