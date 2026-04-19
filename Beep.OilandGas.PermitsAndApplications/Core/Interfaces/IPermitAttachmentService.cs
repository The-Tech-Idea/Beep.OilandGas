using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for managing application documents and attachments.
    /// </summary>
    public interface IPermitAttachmentService
    {
        Task<APPLICATION_ATTACHMENT> AddAttachmentAsync(string applicationId, APPLICATION_ATTACHMENT attachment, string userId);
        Task<IReadOnlyList<APPLICATION_ATTACHMENT>> GetAttachmentsAsync(string applicationId);
        Task RemoveAttachmentAsync(string attachmentId, string userId);
    }
}
