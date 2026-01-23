using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for permit and application management services.
    /// Provides comprehensive permit application lifecycle management with PPDM compliance.
    /// </summary>
    public interface IPermitsAndApplicationsService
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
        /// <returns>Validation result with any errors or warnings.</returns>
        Task<PermitValidationResult> ValidatePermitApplicationAsync(string applicationId);

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
    }

    /// <summary>
    /// Validation result for permit applications.
    /// </summary>
    
}
