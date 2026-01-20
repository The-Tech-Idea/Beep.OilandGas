using System.Collections.Generic;
using System.Threading.Tasks;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for exploration phase service, field-scoped
    /// </summary>
    public interface IFieldExplorationService
    {
        /// <summary>
        /// Get prospects for a specific field
        /// </summary>
        Task<List<PROSPECT>> GetProspectsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create a prospect for a specific field
        /// </summary>
        Task<PROSPECT> CreateProspectForFieldAsync(string fieldId, ProspectRequest prospectData, string userId);

        /// <summary>
        /// Update a prospect (must belong to the specified field)
        /// </summary>
        Task<PROSPECT> UpdateProspectForFieldAsync(string fieldId, string prospectId, ProspectRequest prospectData, string userId);

        /// <summary>
        /// Get prospect by ID (validates it belongs to the field)
        /// </summary>
        Task<PROSPECT?> GetProspectForFieldAsync(string fieldId, string prospectId);

        /// <summary>
        /// Soft delete a prospect (must belong to the specified field)
        /// </summary>
        Task<bool> DeleteProspectForFieldAsync(string fieldId, string prospectId, string userId);

        /// <summary>
        /// Get seismic surveys for a specific field
        /// </summary>
        Task<List<SEIS_ACQTN_SURVEY>> GetSeismicSurveysForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create a seismic survey for a specific field
        /// </summary>
        Task<SEIS_ACQTN_SURVEY> CreateSeismicSurveyForFieldAsync(string fieldId, SeismicSurveyRequest surveyData, string userId);

        /// <summary>
        /// Get exploratory wells for a specific field
        /// </summary>
        Task<List<WELL>> GetExploratoryWellsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Get seismic lines for a seismic survey
        /// </summary>
        Task<List<SEIS_LINE>> GetSeismicLinesForSurveyAsync(string surveyId, List<AppFilter>? additionalFilters = null);
    }
}




