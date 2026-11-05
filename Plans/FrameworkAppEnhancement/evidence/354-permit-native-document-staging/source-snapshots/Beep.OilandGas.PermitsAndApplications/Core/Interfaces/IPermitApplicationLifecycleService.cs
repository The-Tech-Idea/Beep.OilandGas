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
        Task<TheTechIdea.Data.OilGas.PermitDocumentStagingProof> StageDocumentAsync(TheTechIdea.Data.OilGas.PermitDocumentStageOriginal original,byte[] content);
        Task<TheTechIdea.Data.OilGas.PermitDocumentStagingProof?> RecoverDocumentStageAsync(TheTechIdea.Data.OilGas.PermitDocumentStageOriginal original);
        Task<PermitApplicationModel?> GetByIdAsync(string applicationId);
        Task<TheTechIdea.Data.OilGas.PermitApplicationSourceReview> ReviewSourceAsync(string applicationId);
        Task<TheTechIdea.Data.OilGas.PermitApplicationCommandOutcome> RecordCommandAsync(TheTechIdea.Data.OilGas.PermitApplicationCommand original);
        Task<TheTechIdea.Data.OilGas.PermitApplicationCommandOutcome?> RecoverCommandAsync(TheTechIdea.Data.OilGas.PermitApplicationCommand original);
        Task<IReadOnlyList<PermitApplicationModel>> GetApplicationsAsync();
        Task<IReadOnlyList<PermitApplicationModel>> GetByStatusAsync(PermitApplicationStatus status);
    }
}
