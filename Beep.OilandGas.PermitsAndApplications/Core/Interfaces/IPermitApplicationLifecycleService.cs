using Beep.OilandGas.PermitsAndApplications.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using PermitApplicationModel = Beep.OilandGas.PermitsAndApplications.Data.PermitTables.PERMIT_APPLICATION;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for managing permit applications using domain models.
    /// </summary>
    public interface IPermitApplicationLifecycleService
    {
        Task<Beep.OilandGas.PermitsAndApplications.Models.PermitDocumentStagingProof> StageDocumentAsync(Beep.OilandGas.PermitsAndApplications.Models.PermitDocumentStageOriginal original,byte[] content);
        Task<Beep.OilandGas.PermitsAndApplications.Models.PermitDocumentStagingProof?> RecoverDocumentStageAsync(Beep.OilandGas.PermitsAndApplications.Models.PermitDocumentStageOriginal original);
        Task<Beep.OilandGas.PermitsAndApplications.Models.PermitAttachmentContent> ReadAttachmentContentAsync(Beep.OilandGas.PermitsAndApplications.Models.PermitAttachmentContentQuery query);
        Task<PermitApplicationModel?> GetByIdAsync(string applicationId);
        Task<Beep.OilandGas.PermitsAndApplications.Models.PermitApplicationSourceReview> ReviewSourceAsync(string applicationId);
        Task<Beep.OilandGas.PermitsAndApplications.Models.PermitApplicationCommandOutcome> RecordCommandAsync(Beep.OilandGas.PermitsAndApplications.Models.PermitApplicationCommand original);
        Task<Beep.OilandGas.PermitsAndApplications.Models.PermitApplicationCommandOutcome?> RecoverCommandAsync(Beep.OilandGas.PermitsAndApplications.Models.PermitApplicationCommand original);
        Task<IReadOnlyList<PermitApplicationModel>> GetApplicationsAsync();
        Task<IReadOnlyList<PermitApplicationModel>> GetByStatusAsync(PermitApplicationStatus status);
    }
}
