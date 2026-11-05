using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Web.Services;

public class PermitServiceClient : IPermitServiceClient
{
    public async Task<PermitApplicationCatalog> GetCatalogAsync(CancellationToken cancellationToken = default) =>
        await _apiClient.GetAsync<PermitApplicationCatalog>("/api/field/current/permits/application-catalog", cancellationToken)
        ?? throw new InvalidOperationException("The permit application catalog was unavailable.");
    private readonly ApiClient _apiClient;

    public PermitServiceClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<PermitApplicationSummary>> GetAllAsync(string? status = null, string? authority = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrWhiteSpace(authority)) queryParams.Add($"authority={Uri.EscapeDataString(authority)}");
        var url = "/api/field/current/permits" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
        return await _apiClient.GetAsync<List<PermitApplicationSummary>>(url, cancellationToken) ?? throw new InvalidOperationException("The permit population was unavailable.");
    }

    public async Task<PermitApplicationDetail?> GetByIdAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        if (!LeaseSummaryValidator.Identifier(applicationId)) throw new InvalidOperationException("An exact permit identity is required.");
        var detail = await _apiClient.GetAsync<PermitApplicationDetail>("/api/field/current/permits/application?applicationId=" + Uri.EscapeDataString(applicationId), cancellationToken);
        if (detail is not null) PermitApplicationReadValidator.Require(detail, applicationId);
        return detail;
    }

    public async Task<PermitApplicationInboxReview> ReadInboxAsync(FieldSelectionReview expected,CancellationToken cancellationToken=default)
    {
        PermitApplicationSourceReview.RequireTarget("permit-inbox",expected);
        var result=await _apiClient.GetAsync<PermitApplicationInboxReview>("/api/field/current/permits/application-inbox",cancellationToken)
            ??throw new InvalidOperationException("The permit inbox population is unavailable.");
        result.RequireFor(expected);return result;
    }
    public async Task<PermitApplicationCreationReview> ReviewCreationAsync(PermitApplicationCreationProposal proposal, FieldSelectionReview expected, CancellationToken cancellationToken = default)
    {
        PermitApplicationCreationReview.RequireTarget(proposal,expected);
        var result=await _apiClient.PostAsync<PermitApplicationCreationProposal,PermitApplicationCreationReview>("/api/field/current/permits/application-creation-review",proposal,cancellationToken)
            ??throw new InvalidOperationException("The creation source review is unavailable.");
        result.RequireFor(proposal,expected);return result;
    }

    public async Task<PermitApplicationSourceReview> ReviewSourceAsync(string applicationId, FieldSelectionReview expected, CancellationToken cancellationToken = default)
    {
        PermitApplicationSourceReview.RequireTarget(applicationId,expected);
        var result = await _apiClient.GetAsync<PermitApplicationSourceReview>("/api/field/current/permits/application-review?applicationId="+Uri.EscapeDataString(applicationId),cancellationToken)
            ?? throw new InvalidOperationException("The permit source review is unavailable.");
        result.RequireFor(applicationId,expected);
        return result;
    }

    public async Task<PermitApplicationCommandOutcome> RecordCommandAsync(PermitApplicationCommand original, CancellationToken cancellationToken = default)
    {
        original.Require();
        var result = await _apiClient.PostAsync<PermitApplicationCommand,PermitApplicationCommandOutcome>("/api/field/current/permits/application-command",original,cancellationToken)
            ?? throw new InvalidOperationException("The permit outcome is unconfirmed. Retain the original for recovery.");
        result.Require();
        if (result.Original != original) throw new InvalidOperationException("The permit outcome belongs to another retained original.");
        return result;
    }

    public async Task<PermitApplicationCommandRecovery> RecoverCommandAsync(PermitApplicationCommand original, CancellationToken cancellationToken = default)
    {
        original.Require();
        var result = await _apiClient.PostAsync<PermitApplicationCommand,PermitApplicationCommandRecovery>("/api/field/current/permits/application-command/recovery",original,cancellationToken)
            ?? throw new InvalidOperationException("The permit recovery response is unavailable. Retain the original.");
        result.Require(original); return result;
    }

    public async Task<PermitApplicationCheckResult> CheckComplianceAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        if (!LeaseSummaryValidator.Identifier(applicationId)) throw new InvalidOperationException("An exact permit identity is required.");
        var result = await _apiClient.GetAsync<PermitApplicationCheckResult>("/api/field/current/permits/application-check?applicationId=" + Uri.EscapeDataString(applicationId), cancellationToken);
        PermitApplicationReadValidator.RequireCheck(result, applicationId);
        return result!;
    }

    public async Task<PermitDocumentStageResponse> StageDocumentAsync(PermitDocumentStageRequest request,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(request);request.Require();
        var result=await _apiClient.PostAsync<PermitDocumentStageRequest,PermitDocumentStageResponse>("/api/field/current/permits/application-document-stage",request,cancellationToken)??throw new InvalidOperationException("The document response is unconfirmed; retain its exact original.");
        result.Require(request.Original);
        if(result.Receipt is null)throw new InvalidOperationException("Document staging is unconfirmed; retain the exact original for recovery.");
        if(result.Receipt is null)throw new InvalidOperationException("The document staging result is unconfirmed; retain the original.");
        return result;
    }
    public async Task<PermitDocumentStageResponse> RecoverDocumentStageAsync(PermitDocumentStageOriginal request,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(request);request.Require();
        var result=await _apiClient.PostAsync<PermitDocumentStageOriginal,PermitDocumentStageResponse>("/api/field/current/permits/application-document-stage/recovery",request,cancellationToken)??throw new InvalidOperationException("The document response is unconfirmed; retain its exact original.");
        result.Require(request);
        return result;
    }
    public async Task<PermitAttachmentContent> ReadAttachmentContentAsync(PermitAttachmentContentQuery request,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(request);request.Require();
        var result=await _apiClient.PostAsync<PermitAttachmentContentQuery,PermitAttachmentContent>("/api/field/current/permits/application-attachment-content",request,cancellationToken)??throw new InvalidOperationException("The document response is unconfirmed; retain its exact original.");
        result.Require(request);
        return result;
    }

}
