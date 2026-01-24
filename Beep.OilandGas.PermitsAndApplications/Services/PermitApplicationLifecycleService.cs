using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PermitsAndApplications.Services
{
    /// <summary>
    /// Domain-level permit application service.
    /// </summary>
    public class PermitApplicationLifecycleService : PermitsServiceBase, IPermitApplicationLifecycleService
    {
        private readonly ILogger<PermitApplicationLifecycleService> _logger;
        private readonly DataMapping.PermitApplicationMapper _mapper = new();

        public PermitApplicationLifecycleService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PermitApplicationLifecycleService> logger = null,
            string connectionName = "PPDM39")
            : base(editor, commonColumnHandler, defaults, metadata, logger, connectionName)
        {
            _logger = logger;
        }

        public async Task<PermitApplication> CreateAsync(PermitApplication application, string userId)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var data = _mapper.MapToData(application);
            SetAuditFields(data, userId);
            data.ACTIVE_IND = "Y";
            data.STATUS = string.IsNullOrWhiteSpace(data.STATUS) ? "DRAFT" : data.STATUS;
            data.CREATED_DATE ??= DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(data.PERMIT_APPLICATION_ID))
            {
                data.PERMIT_APPLICATION_ID = GeneratePermitApplicationId();
            }

            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            await repo.InsertAsync(data, userId);

            await AddStatusHistoryAsync(data.PERMIT_APPLICATION_ID, data.STATUS, "Application created", userId);

            _logger?.LogInformation("Permit application created (domain) with ID {ApplicationId}", data.PERMIT_APPLICATION_ID);
            return _mapper.MapToDomain(data);
        }

        public async Task<PermitApplication> UpdateAsync(string applicationId, PermitApplication application, string userId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var existing = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            if (existing == null)
                throw new InvalidOperationException($"Permit application not found: {applicationId}");

            var data = _mapper.MapToData(application, existing);
            SetAuditFields(data, userId);

            await repo.UpdateAsync(data, userId);
            await AddStatusHistoryIfChangedAsync(existing.STATUS, data.STATUS, applicationId, userId);
            return _mapper.MapToDomain(data);
        }

        public async Task<PermitApplication?> GetByIdAsync(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));

            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var result = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            return result == null ? null : _mapper.MapToDomain(result);
        }

        public async Task<IReadOnlyList<PermitApplication>> GetByStatusAsync(PermitApplicationStatus status)
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = MapStatusToString(status) },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results
                .Select(r => r as PERMIT_APPLICATION)
                .Where(r => r != null)
                .Select(r => _mapper.MapToDomain(r))
                .ToList();
        }

        public async Task<PermitApplication> SubmitAsync(string applicationId, string userId)
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var application = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            if (application == null)
                throw new InvalidOperationException($"Permit application not found: {applicationId}");

            application.STATUS = "SUBMITTED";
            application.SUBMITTED_DATE = DateTime.UtcNow;
            application.SUBMISSION_COMPLETE_IND = "Y";
            SetAuditFields(application, userId);

            await repo.UpdateAsync(application, userId);
            await AddStatusHistoryAsync(applicationId, application.STATUS, "Application submitted", userId);
            return _mapper.MapToDomain(application);
        }

        public async Task<PermitApplication> ProcessDecisionAsync(
            string applicationId,
            string decision,
            string decisionRemarks,
            string userId)
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var application = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            if (application == null)
                throw new InvalidOperationException($"Permit application not found: {applicationId}");

            application.DECISION = decision;
            application.DECISION_DATE = DateTime.UtcNow;
            application.REMARKS = decisionRemarks;

            if (string.Equals(decision, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                application.STATUS = "APPROVED";
                application.EFFECTIVE_DATE = DateTime.UtcNow;
            }
            else if (string.Equals(decision, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                application.STATUS = "REJECTED";
            }

            SetAuditFields(application, userId);
            await repo.UpdateAsync(application, userId);
            await AddStatusHistoryAsync(applicationId, application.STATUS, decisionRemarks, userId);
            return _mapper.MapToDomain(application);
        }

        private string GeneratePermitApplicationId()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"PA-{timestamp}";
        }

        private string MapStatusToString(PermitApplicationStatus status)
        {
            return status switch
            {
                PermitApplicationStatus.Draft => "DRAFT",
                PermitApplicationStatus.Submitted => "SUBMITTED",
                PermitApplicationStatus.UnderReview => "UNDER_REVIEW",
                PermitApplicationStatus.AdditionalInformationRequired => "ADDITIONAL_INFO_REQUIRED",
                PermitApplicationStatus.Approved => "APPROVED",
                PermitApplicationStatus.Rejected => "REJECTED",
                PermitApplicationStatus.Withdrawn => "WITHDRAWN",
                PermitApplicationStatus.Expired => "EXPIRED",
                PermitApplicationStatus.Renewed => "RENEWED",
                _ => "DRAFT"
            };
        }

        private async Task AddStatusHistoryIfChangedAsync(string? previousStatus, string? nextStatus, string applicationId, string userId)
        {
            var normalizedPrevious = PermitStatusTransitionRules.Normalize(previousStatus);
            var normalizedNext = PermitStatusTransitionRules.Normalize(nextStatus);

            if (string.Equals(normalizedPrevious, normalizedNext, StringComparison.OrdinalIgnoreCase))
                return;

            if (!PermitStatusTransitionRules.IsTransitionAllowed(normalizedPrevious, normalizedNext))
                throw new InvalidOperationException($"Invalid status transition: {normalizedPrevious} -> {normalizedNext}");

            await AddStatusHistoryAsync(applicationId, normalizedNext, "Status updated", userId);
        }

        private async Task AddStatusHistoryAsync(string applicationId, string? status, string? remarks, string userId)
        {
            var historyRepo = await CreateRepositoryAsync<PERMIT_STATUS_HISTORY>("PERMIT_STATUS_HISTORY");
            var history = new PERMIT_STATUS_HISTORY
            {
                PERMIT_STATUS_HISTORY_ID = GenerateStatusHistoryId(),
                PERMIT_APPLICATION_ID = applicationId,
                STATUS = PermitStatusTransitionRules.Normalize(status),
                STATUS_DATE = DateTime.UtcNow,
                STATUS_REMARKS = remarks,
                UPDATED_BY = userId,
                ACTIVE_IND = "Y"
            };

            SetAuditFields(history, userId);
            await historyRepo.InsertAsync(history, userId);
        }

        private string GenerateStatusHistoryId()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"PSH-{timestamp}";
        }
    }
}
