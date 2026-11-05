using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.Models.Data;
using ApplicBa = Beep.OilandGas.PPDM39.Models.APPLIC_BA;
using ApplicDesc = Beep.OilandGas.PPDM39.Models.APPLIC_DESC;
using ApplicRemark = Beep.OilandGas.PPDM39.Models.APPLIC_REMARK;
using BaPermit = Beep.OilandGas.PPDM39.Models.BA_PERMIT;
using FacilityLicense = Beep.OilandGas.PPDM39.Models.FACILITY_LICENSE;
using WellPermitType = Beep.OilandGas.PPDM39.Models.WELL_PERMIT_TYPE;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Company-bound permit reads, validation and form projection.
    /// Mutations belong to the reviewed native lifecycle command owner.
    /// </summary>
    public interface IPermitApplicationWorkflowService
    {

        /// <summary>
        /// Retrieves a permit application by ID.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <returns>The permit application, or null if not found.</returns>
        Task<PERMIT_APPLICATION?> GetPermitApplicationAsync(string applicationId);

        /// <summary>
        /// Retrieves permit applications by status.
        /// </summary>
        /// <param name="status">The application status.</param>
        /// <returns>List of permit applications with the specified status.</returns>
        Task<List<PERMIT_APPLICATION>> GetPermitApplicationsByStatusAsync(string status);

        /// <summary>
        /// Retrieves permit applications by regulatory authority.
        /// </summary>
        /// <param name="authority">The regulatory authority.</param>
        /// <returns>List of permit applications for the authority.</returns>
        Task<List<PERMIT_APPLICATION>> GetPermitApplicationsByAuthorityAsync(string authority);

        /// <summary>
        /// Retrieves attachments for a permit application.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <returns>List of attachments.</returns>
        Task<List<APPLICATION_ATTACHMENT>> GetApplicationAttachmentsAsync(string applicationId);

        /// <summary>
        /// Validates a permit application for completeness.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="configDirectory">Optional config directory for jurisdiction templates.</param>
        /// <returns>Validation result with any errors or warnings.</returns>
        Task<PermitValidationResult> ValidatePermitApplicationAsync(string applicationId, string? configDirectory = null);

        /// <summary>
        /// Retrieves jurisdiction requirements for a specific authority.
        /// </summary>
        /// <param name="country">The country.</param>
        /// <param name="stateProvince">The state or province.</param>
        /// <param name="authority">The regulatory authority.</param>
        /// <returns>The jurisdiction requirements, or null if none are configured.</returns>
        Task<JURISDICTION_REQUIREMENTS?> GetJurisdictionRequirementsAsync(
            string country,
            string stateProvince,
            string authority);

        /// <summary>
        /// Calculates application fees based on jurisdiction and application type.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <returns>The calculated fees.</returns>
        Task<decimal> CalculateApplicationFeesAsync(string applicationId);

        /// <summary>
        /// Generates JSON payloads for required forms by application ID.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="configDirectory">Optional config directory for jurisdiction templates.</param>
        /// <returns>JSON payload for required forms.</returns>
        Task<string> GenerateFormPayloadJsonAsync(string applicationId, string? configDirectory = null);

    }

    /// <summary>
    /// Validation result for permit applications.
    /// </summary>
    
}
