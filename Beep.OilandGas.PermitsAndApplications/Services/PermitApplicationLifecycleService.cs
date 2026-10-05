using Beep.OilandGas.PermitsAndApplications.Exceptions;
using Beep.OilandGas.PPDM39.Core;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
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
        private readonly ILogger<PermitApplicationLifecycleService>? _logger;
        private readonly DataMapping.PermitApplicationMapper _mapper = new();

        public PermitApplicationLifecycleService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PermitApplicationLifecycleService>? logger = null,
            string connectionName = "PPDM39")
            : base(editor, commonColumnHandler, defaults, metadata, logger, connectionName)
        {
            _logger = logger;
        }

        public async Task<PERMIT_APPLICATION> CreateAsync(PERMIT_APPLICATION application, string userId)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var data = _mapper.MapToData(application);
            SetAuditFields(data, userId);
            data.ACTIVE_IND = "Y";
            // New filings always enter the workflow in Draft (ignore client-supplied status on create).
            data.STATUS = PermitApplicationStatus.Draft;
            data.SUBMISSION_COMPLETE_IND = "N";
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

        public async Task<PERMIT_APPLICATION> UpdateAsync(string applicationId, PERMIT_APPLICATION application, string userId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new InvalidApplicationException("The permit application ID is required.");
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var existing = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            if (existing == null)
                throw new PermitNotFoundException($"Permit application {applicationId} was not found.", applicationId);

            // The status change is judged before anything is written: the update had been saved and the refusal
            // thrown after it, so a refused status was stored anyway.
            var previousStatus = existing.STATUS;
            var data = _mapper.MapToData(application, existing);
            var statusChanged = EnsureStatusChangeAllowed(previousStatus, data.STATUS);
            SetAuditFields(data, userId);

            await repo.UpdateAsync(data, userId);
            if (statusChanged)
                await AddStatusHistoryAsync(applicationId, data.STATUS, "Status updated", userId);
            return _mapper.MapToDomain(data);
        }

        public async Task<PERMIT_APPLICATION?> GetByIdAsync(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new InvalidApplicationException("The permit application ID is required.");

            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var result = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            return result == null ? null : _mapper.MapToDomain(result);
        }

        public async Task<IReadOnlyList<PERMIT_APPLICATION>> GetByStatusAsync(PermitApplicationStatus status)
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = PermitApplicationStatusCodes.ToStorageKey(status) },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results
                .OfType<PERMIT_APPLICATION>()
                .Select(r => _mapper.MapToDomain(r))
                .ToList();
        }

        public async Task<PERMIT_APPLICATION> SubmitAsync(string applicationId, string userId)
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var application = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            if (application == null)
                throw new PermitNotFoundException($"Permit application {applicationId} was not found.", applicationId);

            ValidateStatusTransition(application.STATUS, nameof(PermitApplicationStatus.Submitted));
            application.STATUS = PermitApplicationStatus.Submitted;
            application.SUBMITTED_DATE = DateTime.UtcNow;
            application.SUBMISSION_COMPLETE_IND = "Y";
            SetAuditFields(application, userId);

            await repo.UpdateAsync(application, userId);
            await AddStatusHistoryAsync(applicationId, application.STATUS, "Application submitted", userId);
            return _mapper.MapToDomain(application);
        }

        public async Task<PERMIT_APPLICATION> ProcessDecisionAsync(
            string applicationId,
            string decision,
            string decisionRemarks,
            string userId)
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var application = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            if (application == null)
                throw new PermitNotFoundException($"Permit application {applicationId} was not found.", applicationId);

            application.DECISION = decision;
            application.DECISION_DATE = DateTime.UtcNow;
            application.REMARKS = decisionRemarks;

            if (string.Equals(decision, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                ValidateStatusTransition(application.STATUS, nameof(PermitApplicationStatus.Approved));
                application.STATUS = PermitApplicationStatus.Approved;
                application.EFFECTIVE_DATE = DateTime.UtcNow;
            }
            else if (string.Equals(decision, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                ValidateStatusTransition(application.STATUS, nameof(PermitApplicationStatus.Rejected));
                application.STATUS = PermitApplicationStatus.Rejected;
            }
            else
            {
                throw new InvalidApplicationException(
                    "The regulator's decision must be \"Approved\" or \"Rejected\".", applicationId);
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

        /// <summary>
        /// Refuses a status change the rules do not allow; answers whether the status changes at all.
        /// </summary>
        private static bool EnsureStatusChangeAllowed(PermitApplicationStatus? previousStatus, PermitApplicationStatus? nextStatus)
        {
            var normalizedPrevious = PermitStatusTransitionRules.Normalize(previousStatus?.ToString());
            var normalizedNext = PermitStatusTransitionRules.Normalize(nextStatus?.ToString());

            if (string.Equals(normalizedPrevious, normalizedNext, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!PermitStatusTransitionRules.IsTransitionAllowed(normalizedPrevious, normalizedNext))
                throw PermitStatusTransitionRules.RefuseTransition(normalizedPrevious, normalizedNext);

            return true;
        }

        private async Task AddStatusHistoryAsync(string applicationId, PermitApplicationStatus? status, string? remarks, string userId)
        {
            var historyRepo = await CreateRepositoryAsync<PERMIT_STATUS_HISTORY>("PERMIT_STATUS_HISTORY");
            var history = new PERMIT_STATUS_HISTORY
            {
                PERMIT_STATUS_HISTORY_ID = GenerateStatusHistoryId(),
                PERMIT_APPLICATION_ID = applicationId,
                STATUS = PermitApplicationStatusCodes.ToStorageKey(status),
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

        /// <summary>
        /// Validates a transition using the same rules as <see cref="PermitApplicationWorkflowService"/>
        /// and <see cref="PermitStatusHistoryService"/> (agency workflow gate).
        /// </summary>
        /// <param name="currentStatus">Current stored status.</param>
        /// <param name="nextStatusEnumName">Name of the <see cref="PermitApplicationStatus"/> member (e.g. Submitted, Approved).</param>
        private static void ValidateStatusTransition(PermitApplicationStatus? currentStatus, string nextStatusEnumName)
        {
            if (string.IsNullOrWhiteSpace(nextStatusEnumName))
                throw new ArgumentException("Next status is required.", nameof(nextStatusEnumName));

            if (!Enum.TryParse<PermitApplicationStatus>(nextStatusEnumName, ignoreCase: true, out var nextEnum))
                throw new InvalidOperationException($"Unknown permit status: {nextStatusEnumName}");

            var normalizedCurrent = PermitStatusTransitionRules.Normalize(currentStatus?.ToString());
            var normalizedNext = PermitStatusTransitionRules.Normalize(nextEnum.ToString());

            if (!PermitStatusTransitionRules.IsTransitionAllowed(normalizedCurrent, normalizedNext))
                throw PermitStatusTransitionRules.RefuseTransition(normalizedCurrent, normalizedNext);
        }
    }
}
