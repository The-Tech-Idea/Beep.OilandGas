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
    /// Tracks permit status and workflow history.
    /// </summary>
    public class PermitStatusHistoryService : PermitsServiceBase, IPermitStatusHistoryService
    {
        private readonly ILogger<PermitStatusHistoryService> _logger;

        public PermitStatusHistoryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PermitStatusHistoryService> logger = null,
            string connectionName = "PPDM39")
            : base(editor, commonColumnHandler, defaults, metadata, logger, connectionName)
        {
            _logger = logger;
        }

        public async Task<PERMIT_APPLICATION?> GetCurrentAsync(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));

            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            return await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
        }

        public async Task<IReadOnlyList<PERMIT_STATUS_HISTORY>> GetHistoryAsync(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));

            var repo = await CreateRepositoryAsync<PERMIT_STATUS_HISTORY>("PERMIT_STATUS_HISTORY");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results
                .Select(r => r as PERMIT_STATUS_HISTORY)
                .Where(r => r != null)
                .OrderByDescending(r => r.STATUS_DATE)
                .ToList();
        }

        public async Task<PERMIT_APPLICATION> UpdateStatusAsync(
            string applicationId,
            string status,
            string? remarks,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentNullException(nameof(status));

            var applicationRepo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var application = await applicationRepo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            if (application == null)
                throw new InvalidOperationException($"Permit application not found: {applicationId}");

            var currentStatus = PermitStatusTransitionRules.Normalize(application.STATUS);
            var nextStatus = PermitStatusTransitionRules.Normalize(status);
            if (!PermitStatusTransitionRules.IsTransitionAllowed(currentStatus, nextStatus))
                throw new InvalidOperationException($"Invalid status transition: {currentStatus} -> {nextStatus}");

            application.STATUS = status;
            if (status.Equals("SUBMITTED", StringComparison.OrdinalIgnoreCase))
            {
                application.SUBMITTED_DATE ??= DateTime.UtcNow;
            }

            SetAuditFields(application, userId);
            await applicationRepo.UpdateAsync(application, userId);

            var historyRepo = await CreateRepositoryAsync<PERMIT_STATUS_HISTORY>("PERMIT_STATUS_HISTORY");
            var history = new PERMIT_STATUS_HISTORY
            {
                PERMIT_STATUS_HISTORY_ID = GenerateStatusHistoryId(),
                PERMIT_APPLICATION_ID = applicationId,
                STATUS = status,
                STATUS_DATE = DateTime.UtcNow,
                STATUS_REMARKS = remarks,
                UPDATED_BY = userId,
                ACTIVE_IND = "Y"
            };

            SetAuditFields(history, userId);
            await historyRepo.InsertAsync(history, userId);

            _logger?.LogInformation("Updated permit status to {Status} for {ApplicationId}", status, applicationId);
            return application;
        }

        private string GenerateStatusHistoryId()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"PSH-{timestamp}";
        }

    }
}
