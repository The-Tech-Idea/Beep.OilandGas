using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace Beep.OilandGas.Client.App.Services.Permits;

internal sealed class PermitsService(BeepOilandGasApp app, ILogger<PermitsService>? logger = null) : ServiceBase(app, logger), IPermitsService
{
    private void RequireApi()
    {
        if (AccessMode != ServiceAccessMode.Remote)
            throw new InvalidOperationException("Permit application workflows require an authenticated ApiService connection.");
    }
    public async Task<PermitApplicationInboxReview> ReadInboxAsync(FieldSelectionReview expected,CancellationToken cancellationToken=default)
    {
        RequireApi();PermitApplicationSourceReview.RequireTarget("permit-inbox",expected);
        var result=await GetAsync<PermitApplicationInboxReview>("/api/field/current/permits/application-inbox",cancellationToken)
            ??throw new InvalidOperationException("The permit inbox population is unavailable.");
        result.RequireFor(expected);return result;
    }
    public async Task<PermitApplicationCreationReview> ReviewCreationAsync(PermitApplicationCreationProposal proposal, FieldSelectionReview expected, CancellationToken cancellationToken = default)
    {
        RequireApi();PermitApplicationCreationReview.RequireTarget(proposal,expected);
        var result=await PostAsync<PermitApplicationCreationProposal,PermitApplicationCreationReview>("/api/field/current/permits/application-creation-review",proposal,cancellationToken)
            ??throw new InvalidOperationException("The creation source review is unavailable.");
        result.RequireFor(proposal,expected);return result;
    }
    public async Task<PermitApplicationSourceReview> ReviewSourceAsync(string applicationId, FieldSelectionReview expected, CancellationToken cancellationToken = default)
    {
        RequireApi(); PermitApplicationSourceReview.RequireTarget(applicationId,expected);
        var result = await GetAsync<PermitApplicationSourceReview>("/api/field/current/permits/application-review?applicationId="+Uri.EscapeDataString(applicationId),cancellationToken)
            ?? throw new InvalidOperationException("The permit source review is unavailable.");
        result.RequireFor(applicationId,expected); return result;
    }
    public async Task<PermitApplicationCommandOutcome> RecordCommandAsync(PermitApplicationCommand original, CancellationToken cancellationToken = default)
    {
        RequireApi(); ArgumentNullException.ThrowIfNull(original); original.Require();
        var result = await PostAsync<PermitApplicationCommand,PermitApplicationCommandOutcome>("/api/field/current/permits/application-command",original,cancellationToken)
            ?? throw new InvalidOperationException("The permit outcome is unconfirmed. Retain the original for recovery.");
        result.Require();
        if (result.Original != original) throw new InvalidOperationException("The permit outcome belongs to another retained original.");
        return result;
    }
    public async Task<PermitApplicationCommandRecovery> RecoverCommandAsync(PermitApplicationCommand original, CancellationToken cancellationToken = default)
    {
        RequireApi(); ArgumentNullException.ThrowIfNull(original); original.Require();
        var result = await PostAsync<PermitApplicationCommand,PermitApplicationCommandRecovery>("/api/field/current/permits/application-command/recovery",original,cancellationToken)
            ?? throw new InvalidOperationException("The permit recovery response is unavailable. Retain the original.");
        result.Require(original); return result;
    }
    public async Task<PermitDocumentStageResponse> StageDocumentAsync(PermitDocumentStageRequest request,CancellationToken cancellationToken=default)
    {
        RequireApi(); ArgumentNullException.ThrowIfNull(request);request.Require();
        var result=await PostAsync<PermitDocumentStageRequest,PermitDocumentStageResponse>("/api/field/current/permits/application-document-stage",request,cancellationToken)??throw new InvalidOperationException("The document response is unconfirmed; retain its exact original.");
        result.Require(request.Original);
        if(result.Receipt is null)throw new InvalidOperationException("Document staging is unconfirmed; retain the exact original for recovery.");
        if(result.Receipt is null)throw new InvalidOperationException("The document staging result is unconfirmed; retain the original.");
        return result;
    }
    public async Task<PermitDocumentStageResponse> RecoverDocumentStageAsync(PermitDocumentStageOriginal request,CancellationToken cancellationToken=default)
    {
        RequireApi(); ArgumentNullException.ThrowIfNull(request);request.Require();
        var result=await PostAsync<PermitDocumentStageOriginal,PermitDocumentStageResponse>("/api/field/current/permits/application-document-stage/recovery",request,cancellationToken)??throw new InvalidOperationException("The document response is unconfirmed; retain its exact original.");
        result.Require(request);
        return result;
    }
    public async Task<PermitAttachmentContent> ReadAttachmentContentAsync(PermitAttachmentContentQuery request,CancellationToken cancellationToken=default)
    {
        RequireApi(); ArgumentNullException.ThrowIfNull(request);request.Require();
        var result=await PostAsync<PermitAttachmentContentQuery,PermitAttachmentContent>("/api/field/current/permits/application-attachment-content",request,cancellationToken)??throw new InvalidOperationException("The document response is unconfirmed; retain its exact original.");
        result.Require(request);
        return result;
    }

}
