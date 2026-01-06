using System.Collections.Generic;
using System.Threading.Tasks;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PipelineAnalysis;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for development phase service, field-scoped
    /// </summary>
    public interface IFieldDevelopmentService
    {
        /// <summary>
        /// Get pools for a specific field
        /// </summary>
        Task<List<POOL>> GetPoolsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create a pool for a specific field
        /// </summary>
        Task<POOL> CreatePoolForFieldAsync(string fieldId, PoolRequest poolData, string userId);

        /// <summary>
        /// Update a pool (must belong to the specified field)
        /// </summary>
        Task<POOL> UpdatePoolForFieldAsync(string fieldId, string poolId, PoolRequest poolData, string userId);

        /// <summary>
        /// Get pool by ID (validates it belongs to the field)
        /// </summary>
        Task<POOL?> GetPoolForFieldAsync(string fieldId, string poolId);

        /// <summary>
        /// Get development wells for a specific field
        /// </summary>
        Task<List<WELL>> GetDevelopmentWellsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create a development well for a specific field
        /// </summary>
        Task<WELL> CreateDevelopmentWellForFieldAsync(string fieldId, DevelopmentWellRequest wellData, string userId);

        /// <summary>
        /// Get wellbores for a well (must belong to the specified field)
        /// Wellbores are WELL table records with specific well_level_type, linked via WELL_XREF
        /// </summary>
        Task<List<WELL>> GetWellboresForWellAsync(string fieldId, string wellId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Get facilities for a specific field
        /// </summary>
        Task<List<FACILITY>> GetFacilitiesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create a facility for a specific field
        /// </summary>
        Task<FacilityResponse> CreateFacilityForFieldAsync(string fieldId, FacilityRequest facilityData, string userId);

        /// <summary>
        /// Get pipelines for a specific field
        /// </summary>
        Task<List<PIPELINE>> GetPipelinesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create a pipeline for a specific field
        /// </summary>
        Task<PIPELINE> CreatePipelineForFieldAsync(string fieldId, PipelineRequest pipelineData, string userId);
    }
}




