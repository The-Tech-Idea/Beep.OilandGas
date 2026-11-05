using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Web.Services;

public interface IPermitServiceClient
{
    Task<PermitApplicationInboxReview> ReadInboxAsync(FieldSelectionReview expected, CancellationToken cancellationToken = default);
    Task<PermitApplicationCatalog> GetCatalogAsync(CancellationToken cancellationToken = default);
    Task<List<PermitApplicationSummary>> GetAllAsync(string? status = null, string? authority = null, CancellationToken cancellationToken = default);
    Task<PermitApplicationDetail?> GetByIdAsync(string applicationId, CancellationToken cancellationToken = default);
    Task<PermitApplicationCreationReview> ReviewCreationAsync(PermitApplicationCreationProposal proposal, FieldSelectionReview expected, CancellationToken cancellationToken = default);
    Task<PermitApplicationSourceReview> ReviewSourceAsync(string applicationId, FieldSelectionReview expected, CancellationToken cancellationToken = default);
    Task<PermitApplicationCommandOutcome> RecordCommandAsync(PermitApplicationCommand original, CancellationToken cancellationToken = default);
    Task<PermitApplicationCommandRecovery> RecoverCommandAsync(PermitApplicationCommand original, CancellationToken cancellationToken = default);
    Task<PermitApplicationCheckResult> CheckComplianceAsync(string applicationId, CancellationToken cancellationToken = default);
    Task<PermitDocumentStageResponse> StageDocumentAsync(PermitDocumentStageRequest request,CancellationToken cancellationToken=default);
    Task<PermitDocumentStageResponse> RecoverDocumentStageAsync(PermitDocumentStageOriginal request,CancellationToken cancellationToken=default);
    Task<PermitAttachmentContent> ReadAttachmentContentAsync(PermitAttachmentContentQuery request,CancellationToken cancellationToken=default);
}
