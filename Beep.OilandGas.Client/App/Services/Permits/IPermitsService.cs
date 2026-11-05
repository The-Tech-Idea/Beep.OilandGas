using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
using System.Threading;
using System.Threading.Tasks;
namespace Beep.OilandGas.Client.App.Services.Permits;

/// <summary>Reviewed permit workflows through the current company ApiService gateway.</summary>
public interface IPermitsService
{
    Task<PermitApplicationInboxReview> ReadInboxAsync(FieldSelectionReview expected, CancellationToken cancellationToken = default);
    Task<PermitApplicationCreationReview> ReviewCreationAsync(PermitApplicationCreationProposal proposal, FieldSelectionReview expected, CancellationToken cancellationToken = default);
    Task<PermitApplicationSourceReview> ReviewSourceAsync(string applicationId, FieldSelectionReview expected, CancellationToken cancellationToken = default);
    Task<PermitApplicationCommandOutcome> RecordCommandAsync(PermitApplicationCommand original, CancellationToken cancellationToken = default);
    Task<PermitApplicationCommandRecovery> RecoverCommandAsync(PermitApplicationCommand original, CancellationToken cancellationToken = default);
    Task<PermitDocumentStageResponse> StageDocumentAsync(PermitDocumentStageRequest request,CancellationToken cancellationToken=default);
    Task<PermitDocumentStageResponse> RecoverDocumentStageAsync(PermitDocumentStageOriginal request,CancellationToken cancellationToken=default);
    Task<PermitAttachmentContent> ReadAttachmentContentAsync(PermitAttachmentContentQuery request,CancellationToken cancellationToken=default);
}
