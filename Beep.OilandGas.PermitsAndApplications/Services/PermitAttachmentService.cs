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
    /// Handles document and attachment management for permit applications.
    /// </summary>
    public class PermitAttachmentService : PermitsServiceBase, IPermitAttachmentService
    {
        private readonly ILogger<PermitAttachmentService> _logger;

        public PermitAttachmentService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PermitAttachmentService> logger = null,
            string connectionName = "PPDM39")
            : base(editor, commonColumnHandler, defaults, metadata, logger, connectionName)
        {
            _logger = logger;
        }

        public async Task<APPLICATION_ATTACHMENT> AddAttachmentAsync(
            string applicationId,
            APPLICATION_ATTACHMENT attachment,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            attachment.PERMIT_APPLICATION_ID = applicationId;
            attachment.ACTIVE_IND = "Y";
            attachment.UPLOAD_DATE = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(attachment.APPLICATION_ATTACHMENT_ID))
            {
                attachment.APPLICATION_ATTACHMENT_ID = GenerateAttachmentId();
            }

            SetAuditFields(attachment, userId);

            var repo = await CreateRepositoryAsync<APPLICATION_ATTACHMENT>("APPLICATION_ATTACHMENT");
            await repo.InsertAsync(attachment, userId);

            _logger?.LogInformation("Attachment added to application {ApplicationId}", applicationId);
            return attachment;
        }

        public async Task<IReadOnlyList<APPLICATION_ATTACHMENT>> GetAttachmentsAsync(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));

            var repo = await CreateRepositoryAsync<APPLICATION_ATTACHMENT>("APPLICATION_ATTACHMENT");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results
                .Select(r => r as APPLICATION_ATTACHMENT)
                .Where(r => r != null)
                .ToList();
        }

        public async Task RemoveAttachmentAsync(string attachmentId, string userId)
        {
            if (string.IsNullOrWhiteSpace(attachmentId))
                throw new ArgumentNullException(nameof(attachmentId));

            var repo = await CreateRepositoryAsync<APPLICATION_ATTACHMENT>("APPLICATION_ATTACHMENT");
            var attachment = await repo.GetByIdAsync(attachmentId) as APPLICATION_ATTACHMENT;
            if (attachment == null)
                throw new InvalidOperationException($"Attachment not found: {attachmentId}");

            attachment.ACTIVE_IND = "N";
            SetAuditFields(attachment, userId);
            await repo.UpdateAsync(attachment, userId);
        }

        private string GenerateAttachmentId()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"ATT-{timestamp}";
        }

    }
}
