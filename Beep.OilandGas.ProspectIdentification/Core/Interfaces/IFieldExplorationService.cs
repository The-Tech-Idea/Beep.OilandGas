using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using PROSPECT = Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT;

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
        /// Updates only <c>PROSPECT_STATUS</c> for a prospect in the field (e.g. gate decisions).
        /// </summary>
        Task UpdateProspectStatusAsync(string fieldId, string prospectId, string newStatus, string userId);

        /// <summary>
        /// Get prospect by ID (validates it belongs to the field)
        /// </summary>
        Task<PROSPECT?> GetProspectForFieldAsync(string fieldId, string prospectId);

        /// <summary>
        /// Returns a prospect in the field already linked to the given lead (<c>PROSPECT.LEAD_ID</c>), if any.
        /// </summary>
        Task<PROSPECT?> GetProspectForFieldByLeadIdAsync(string fieldId, string leadId);

        /// <summary>
        /// True when <c>LEAD</c> exists and <c>LEAD.FIELD_ID</c> matches the given field (same identifier shape as <see cref="GetProspectsForFieldAsync"/> field scope).
        /// When <c>LEAD.FIELD_ID</c> is unset, implementations may allow the lead so workflows can start (instance still carries <c>FieldId</c>); populate <c>FIELD_ID</c> at lead creation for strict binding.
        /// </summary>
        Task<bool> IsLeadInFieldAsync(string fieldId, string leadId);

        /// <summary>
        /// One round-trip for lead-to-prospect start: returns <c>true</c> when the lead exists for this field (either <c>LEAD.FIELD_ID</c> already matches, or it was unset and is written to <paramref name="fieldId"/> using <paramref name="userId"/> for audit). Returns <c>false</c> when the lead is missing or belongs to another field.
        /// </summary>
        Task<bool> EnsureLeadInFieldForWorkflowStartAsync(string fieldId, string leadId, string userId);

        /// <summary>
        /// True when a <c>PROSPECT_DISCOVERY</c> row exists for <paramref name="discoveryId"/> and its <c>PROSPECT_ID</c> belongs to <paramref name="fieldId"/>.
        /// </summary>
        Task<bool> IsProspectDiscoveryInFieldAsync(string fieldId, string discoveryId);

        /// <summary>
        /// Sets <c>LEAD.LEAD_STATUS</c> for the given lead (e.g. after promotion to prospect). Status must exist in <c>R_LEAD_STATUS</c> when FK is enforced.
        /// </summary>
        Task UpdateLeadStatusAsync(string leadId, string leadStatus, string userId);

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




