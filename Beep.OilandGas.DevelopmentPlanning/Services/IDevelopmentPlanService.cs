using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

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
        Task<List<DevelopmentPlan>> GetDevelopmentPlansAsync(string? fieldId = null);

        /// <summary>
        /// Gets a development plan by ID.
        /// </summary>
        Task<DevelopmentPlan?> GetDevelopmentPlanAsync(string planId);

        /// <summary>
        /// Creates a new development plan.
        /// </summary>
        Task<DevelopmentPlan> CreateDevelopmentPlanAsync(CreateDevelopmentPlan createDto);

        /// <summary>
        /// Updates a development plan.
        /// </summary>
        Task<DevelopmentPlan> UpdateDevelopmentPlanAsync(string planId, UpdateDevelopmentPlan updateDto);

        /// <summary>
        /// Approves a development plan.
        /// </summary>
        Task<DevelopmentPlan> ApproveDevelopmentPlanAsync(string planId, string approvedBy);

        /// <summary>
        /// Gets well plans for a development plan.
        /// </summary>
        Task<List<WellPlan>> GetWellPlansAsync(string planId);

        /// <summary>
        /// Gets facility plans for a development plan.
        /// </summary>
        Task<List<FacilityPlan>> GetFacilityPlansAsync(string planId);

        /// <summary>
        /// Gets permit applications for a development plan.
        /// </summary>
        Task<List<PermitApplication>> GetPermitApplicationsAsync(string planId);
    }
}

