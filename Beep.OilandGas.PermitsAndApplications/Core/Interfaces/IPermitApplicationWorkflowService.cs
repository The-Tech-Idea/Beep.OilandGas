using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.Models.Data;
using ApplicBa = Beep.OilandGas.Models.Data.PermitsAndApplications.APPLIC_BA;
using ApplicDesc = Beep.OilandGas.Models.Data.PermitsAndApplications.APPLIC_DESC;
using ApplicRemark = Beep.OilandGas.Models.Data.PermitsAndApplications.APPLIC_REMARK;
using BaPermit = Beep.OilandGas.Models.Data.PermitsAndApplications.BA_PERMIT;
using FacilityLicense = Beep.OilandGas.Models.Data.PermitsAndApplications.FACILITY_LICENSE;
using WellPermitType = Beep.OilandGas.Models.Data.PermitsAndApplications.WELL_PERMIT_TYPE;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for permit and application management services.
    /// Provides comprehensive permit application lifecycle management with PPDM compliance.
    /// </summary>
    public interface IPermitApplicationWorkflowService
    {
        /// <summary>
        /// Creates a new permit application.
        /// </summary>
        /// <param name="application">The permit application to create.</param>
        /// <param name="userId">The user creating the application.</param>
        /// <returns>The created permit application.</returns>
        Task<PERMIT_APPLICATION> CreatePermitApplicationAsync(
            PERMIT_APPLICATION application,
            string userId);

        /// <summary>
        /// Updates an existing permit application.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="application">The updated application data.</param>
        /// <param name="userId">The user updating the application.</param>
        /// <returns>The updated permit application.</returns>
        Task<PERMIT_APPLICATION> UpdatePermitApplicationAsync(
            string applicationId,
            PERMIT_APPLICATION application,
            string userId);

        /// <summary>
        /// Retrieves a permit application by ID.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <returns>The permit application.</returns>
        Task<PERMIT_APPLICATION> GetPermitApplicationAsync(string applicationId);

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
        /// Submits a permit application for review.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="userId">The user submitting the application.</param>
        /// <returns>The updated application.</returns>
        Task<PERMIT_APPLICATION> SubmitPermitApplicationAsync(
            string applicationId,
            string userId);

        /// <summary>
        /// Approves or rejects a permit application.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="decision">The decision (Approved/Rejected).</param>
        /// <param name="decisionRemarks">Remarks for the decision.</param>
        /// <param name="userId">The user making the decision.</param>
        /// <returns>The updated application.</returns>
        Task<PERMIT_APPLICATION> ProcessPermitDecisionAsync(
            string applicationId,
            string decision,
            string decisionRemarks,
            string userId);

        /// <summary>
        /// Adds an attachment to a permit application.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="attachment">The attachment to add.</param>
        /// <param name="userId">The user adding the attachment.</param>
        /// <returns>The created attachment.</returns>
        Task<APPLICATION_ATTACHMENT> AddApplicationAttachmentAsync(
            string applicationId,
            APPLICATION_ATTACHMENT attachment,
            string userId);

        /// <summary>
        /// Retrieves attachments for a permit application.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <returns>List of attachments.</returns>
        Task<List<APPLICATION_ATTACHMENT>> GetApplicationAttachmentsAsync(string applicationId);

        /// <summary>
        /// Creates a drilling permit application.
        /// </summary>
        /// <param name="drillingApplication">The drilling application.</param>
        /// <param name="userId">The user creating the application.</param>
        /// <returns>The created drilling application.</returns>
        Task<DRILLING_PERMIT_APPLICATION> CreateDrillingPermitApplicationAsync(
            DRILLING_PERMIT_APPLICATION drillingApplication,
            string userId);

        /// <summary>
        /// Creates an environmental permit application.
        /// </summary>
        /// <param name="environmentalApplication">The environmental application.</param>
        /// <param name="userId">The user creating the application.</param>
        /// <returns>The created environmental application.</returns>
        Task<ENVIRONMENTAL_PERMIT_APPLICATION> CreateEnvironmentalPermitApplicationAsync(
            ENVIRONMENTAL_PERMIT_APPLICATION environmentalApplication,
            string userId);

        /// <summary>
        /// Creates an injection permit application.
        /// </summary>
        /// <param name="injectionApplication">The injection application.</param>
        /// <param name="userId">The user creating the application.</param>
        /// <returns>The created injection application.</returns>
        Task<INJECTION_PERMIT_APPLICATION> CreateInjectionPermitApplicationAsync(
            INJECTION_PERMIT_APPLICATION injectionApplication,
            string userId);

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
        /// <returns>The jurisdiction requirements.</returns>
        Task<JURISDICTION_REQUIREMENTS> GetJurisdictionRequirementsAsync(
            string country,
            string stateProvince,
            string authority);

        /// <summary>
        /// Generates required forms for a permit application.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="userId">The user generating the forms.</param>
        /// <returns>List of required forms.</returns>
        Task<List<REQUIRED_FORM>> GenerateRequiredFormsAsync(
            string applicationId,
            string userId);

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

        /// <summary>
        /// Generates and stores form payload files as attachments for an application.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="userId">The user generating the payloads.</param>
        /// <param name="outputDirectory">Output directory for payload files.</param>
        /// <param name="configDirectory">Optional config directory for jurisdiction templates.</param>
        /// <returns>List of created attachments.</returns>
        Task<IReadOnlyList<APPLICATION_ATTACHMENT>> GenerateFormPayloadAttachmentsAsync(
            string applicationId,
            string userId,
            string outputDirectory,
            string? configDirectory = null);

        /// <summary>
        /// Generates JSON and HTML packets for forms and stores them as attachments.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <param name="userId">The user generating the packets.</param>
        /// <param name="outputDirectory">Output directory for packet files.</param>
        /// <param name="configDirectory">Optional config directory for jurisdiction templates.</param>
        /// <returns>List of created attachments.</returns>
        Task<IReadOnlyList<APPLICATION_ATTACHMENT>> GenerateFormPacketAttachmentsAsync(
            string applicationId,
            string userId,
            string outputDirectory,
            string? configDirectory = null);

        /// <summary>
        /// Adds a business associate record for a permit application.
        /// </summary>
        Task<ApplicBa> AddApplicBusinessAssociateAsync(
            string applicationId,
            ApplicBa associate,
            string userId);

        /// <summary>
        /// Retrieves business associate records for a permit application.
        /// </summary>
        Task<List<ApplicBa>> GetApplicBusinessAssociatesAsync(string applicationId);

        /// <summary>
        /// Adds a description record for a permit application.
        /// </summary>
        Task<ApplicDesc> AddApplicDescriptionAsync(
            string applicationId,
            ApplicDesc description,
            string userId);

        /// <summary>
        /// Retrieves description records for a permit application.
        /// </summary>
        Task<List<ApplicDesc>> GetApplicDescriptionsAsync(string applicationId);

        /// <summary>
        /// Adds a remark record for a permit application.
        /// </summary>
        Task<ApplicRemark> AddApplicRemarkAsync(
            string applicationId,
            ApplicRemark remark,
            string userId);

        /// <summary>
        /// Retrieves remark records for a permit application.
        /// </summary>
        Task<List<ApplicRemark>> GetApplicRemarksAsync(string applicationId);

        /// <summary>
        /// Adds a business associate permit record.
        /// </summary>
        Task<BaPermit> AddBusinessAssociatePermitAsync(BaPermit permit, string userId);

        /// <summary>
        /// Retrieves business associate permits for a business associate.
        /// </summary>
        Task<List<BaPermit>> GetBusinessAssociatePermitsAsync(string businessAssociateId);

        /// <summary>
        /// Adds a facility license record.
        /// </summary>
        Task<FacilityLicense> AddFacilityLicenseAsync(
            string facilityId,
            FacilityLicense license,
            string userId);

        /// <summary>
        /// Retrieves facility licenses for a facility.
        /// </summary>
        Task<List<FacilityLicense>> GetFacilityLicensesAsync(string facilityId);

        /// <summary>
        /// Adds a well permit type record.
        /// </summary>
        Task<WellPermitType> AddWellPermitTypeAsync(WellPermitType permitType, string userId);

        /// <summary>
        /// Retrieves well permit types, optionally filtered by granting authority.
        /// </summary>
        Task<List<WellPermitType>> GetWellPermitTypesAsync(string? grantedByBaId = null);
    }

    /// <summary>
    /// Validation result for permit applications.
    /// </summary>
    
}
