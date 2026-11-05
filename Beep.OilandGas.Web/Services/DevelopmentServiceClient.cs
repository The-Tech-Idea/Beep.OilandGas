using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class DevelopmentServiceClient : IDevelopmentServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<DevelopmentServiceClient> _logger;

    public DevelopmentServiceClient(ApiClient apiClient, ILogger<DevelopmentServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DevelopmentDashboardSummary?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<DevelopmentDashboardSummary>(
                "/api/field/current/development/dashboard/summary",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development dashboard summary.");
            throw;
        }
    }

    public async Task<List<DevelopmentWellStatusDto>> GetDashboardWellsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<DevelopmentWellStatusDto>>(
                "/api/field/current/development/dashboard/wells",
                cancellationToken);

            return result ?? new List<DevelopmentWellStatusDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development dashboard wells.");
            throw;
        }
    }

    public async Task<List<WELL>> GetWellsAsync(CancellationToken cancellationToken = default, string? fieldId = null)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<WELL>>(
                "/api/field/current/development/wells" + FieldQuery(fieldId),
                cancellationToken);

            return result ?? throw new InvalidOperationException("Development well evidence was unavailable.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development wells.");
            throw;
        }
    }

    public async Task<WELL?> GetWellAsync(string uwi, CancellationToken cancellationToken = default, string? fieldId = null)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        try
        {
            return await _apiClient.GetAsync<WELL>(
                $"/api/field/current/development/wells/{Uri.EscapeDataString(uwi)}" + FieldQuery(fieldId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development well {Uwi}.", uwi);
            throw;
        }
    }

    private static string FieldQuery(string? fieldId) => fieldId is null ? "" :
        !string.IsNullOrWhiteSpace(fieldId) ? "?fieldId=" + Uri.EscapeDataString(fieldId) :
        throw new ArgumentException("A reviewed FIELD identity is required.", nameof(fieldId));

    public async Task AssignRigAsync(string uwi, DevelopmentAssignRigRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _apiClient.PutAsync(
                $"/api/field/current/development/wells/{Uri.EscapeDataString(uwi)}/rig",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning rig to well {Uwi}.", uwi);
            throw;
        }
    }

    public async Task<DevelopmentPlanBaselineReview> GetDraftBaselineAsync(string processId,string reviewRevision,CancellationToken cancellationToken=default)
        =>await _apiClient.GetAsync<DevelopmentPlanBaselineReview>("/api/field/current/development-planning/review-handoffs/"+Uri.EscapeDataString(processId)+"/baseline?expectedReviewRevision="+Uri.EscapeDataString(reviewRevision),cancellationToken)
            ??throw new InvalidOperationException("The original FDP baseline lookup is unavailable.");
    public async Task<DevelopmentPlanReleaseReceipt> ApproveDraftBaselineAsync(DevelopmentPlanRelease original,CancellationToken cancellationToken=default)
        =>await _apiClient.PostAsync<DevelopmentPlanRelease,DevelopmentPlanReleaseReceipt>("/api/field/current/development-planning/baselines",original,cancellationToken)
            ??throw new InvalidOperationException("The original FDP baseline approval outcome is unconfirmed.");
    public async Task<DevelopmentPlanReleaseReceipt> RecoverDraftBaselineAsync(DevelopmentPlanRelease original,CancellationToken cancellationToken=default)
        =>await _apiClient.PostAsync<DevelopmentPlanRelease,DevelopmentPlanReleaseReceipt>("/api/field/current/development-planning/baselines/recover",original,cancellationToken)
            ??throw new InvalidOperationException("The original FDP baseline recovery is unconfirmed.");
    public async Task<DevelopmentReviewProgress> GetDraftReviewProgressAsync(string processId,CancellationToken cancellationToken=default)
        =>await _apiClient.GetAsync<DevelopmentReviewProgress>("/api/field/current/development-planning/review-handoffs/"+Uri.EscapeDataString(processId)+"/history",cancellationToken)
            ??throw new InvalidOperationException("The recorded development review history is unavailable.");
    public async Task<DevelopmentPlanReviewStatus> GetDraftReviewStatusAsync(string id,string sourceRevision,CancellationToken cancellationToken=default)
        =>await _apiClient.GetAsync<DevelopmentPlanReviewStatus>("/api/field/current/development-planning/plans/"+Uri.EscapeDataString(id)+"/review-status?expectedSourceRevision="+Uri.EscapeDataString(sourceRevision),cancellationToken)
            ??throw new InvalidOperationException("The recorded draft review status is unavailable.");
    public async Task<DevelopmentAssignedReview> GetAssignedDraftReviewAsync(string processId,CancellationToken cancellationToken=default)=>
        await _apiClient.GetAsync<DevelopmentAssignedReview>("/api/field/current/development-planning/review-handoffs/"+Uri.EscapeDataString(processId)+"/assigned",cancellationToken)
            ?? throw new InvalidOperationException("The assigned development review source is unavailable.");
    public async Task<DevelopmentReviewDecisionReceipt> DecideDraftReviewAsync(DevelopmentReviewDecision command,CancellationToken cancellationToken=default)=>
        await _apiClient.PostAsync<DevelopmentReviewDecision,DevelopmentReviewDecisionReceipt>("/api/field/current/development-planning/review-decisions",command,cancellationToken)
            ?? throw new InvalidOperationException("The original development review decision is unconfirmed.");
    public async Task<DevelopmentReviewDecisionReceipt> RecoverDraftReviewDecisionAsync(DevelopmentReviewDecision original,CancellationToken cancellationToken=default)=>
        await _apiClient.PostAsync<DevelopmentReviewDecision,DevelopmentReviewDecisionReceipt>("/api/field/current/development-planning/review-decisions/recover",original,cancellationToken)
            ?? throw new InvalidOperationException("The original development decision recovery is unconfirmed.");
    public async Task<DevelopmentReviewStart> PrepareDraftReviewAsync(string id,DevelopmentReviewSelection selection,CancellationToken cancellationToken=default)=>
        await _apiClient.PostAsync<DevelopmentReviewSelection,DevelopmentReviewStart>("/api/field/current/development-planning/plans/"+Uri.EscapeDataString(id)+"/review-handoffs/prepare",selection,cancellationToken)
            ?? throw new InvalidOperationException("The independent development review handoff could not be prepared.");
    public async Task<DevelopmentReviewStartReceipt> SubmitDraftReviewAsync(DevelopmentReviewStart original,CancellationToken cancellationToken=default)=>
        await _apiClient.PostAsync<DevelopmentReviewStart,DevelopmentReviewStartReceipt>("/api/field/current/development-planning/review-handoffs",original,cancellationToken)
            ?? throw new InvalidOperationException("The original development review submission outcome is unconfirmed.");
    public async Task<DevelopmentReviewStartReceipt> RecoverDraftReviewAsync(DevelopmentReviewStart original,CancellationToken cancellationToken=default)=>
        await _apiClient.PostAsync<DevelopmentReviewStart,DevelopmentReviewStartReceipt>("/api/field/current/development-planning/review-handoffs/recover",original,cancellationToken)
            ?? throw new InvalidOperationException("The original development review recovery is unconfirmed.");
    public async Task<DevelopmentReviewCatalog> GetDraftReviewWorkflowsAsync(string id,string sourceRevision,CancellationToken cancellationToken = default)
    {
        if(!LeaseSummaryValidator.Identifier(id) || !ProcessDefinitionRevision.IsValid(sourceRevision))throw new ArgumentException("Reload the recorded draft before choosing review workflows.");
        return await _apiClient.GetAsync<DevelopmentReviewCatalog>("/api/field/current/development-planning/plans/"+Uri.EscapeDataString(id)+
            "/review-workflows?expectedSourceRevision="+Uri.EscapeDataString(sourceRevision),cancellationToken)
            ?? throw new InvalidOperationException("The development review workflow choices are unavailable.");
    }

    public async Task<DevelopmentPlanSourceReview> GetDraftPlanAsync(string id,CancellationToken cancellationToken = default)
    {
        if(!LeaseSummaryValidator.Identifier(id))throw new ArgumentException("A recorded development draft identity is required.",nameof(id));
        return await _apiClient.GetAsync<DevelopmentPlanSourceReview>("/api/field/current/development-planning/plans/"+Uri.EscapeDataString(id),cancellationToken)
            ?? throw new InvalidOperationException("The recorded development draft could not be confirmed.");
    }

    public async Task<DevelopmentPlanDraftList> GetDraftPlansAsync(CancellationToken cancellationToken = default) =>
        await _apiClient.GetAsync<DevelopmentPlanDraftList>("/api/field/current/development-planning/plans",cancellationToken)
        ?? throw new InvalidOperationException("Development draft evidence was unavailable.");

    public async Task<DevelopmentPlanDraftOptions> GetDraftOptionsAsync(CancellationToken cancellationToken = default) =>
        await _apiClient.GetAsync<DevelopmentPlanDraftOptions>("/api/field/current/development-planning/plans/options",cancellationToken)
        ?? throw new InvalidOperationException("Development draft options were unavailable.");

    public async Task<FIELD_DEVELOPMENT_PLAN> SaveDraftPlanAsync(CreateDevelopmentPlan request,CancellationToken cancellationToken = default) =>
        await _apiClient.PostAsync<CreateDevelopmentPlan,FIELD_DEVELOPMENT_PLAN>("/api/field/current/development-planning/plans",request,cancellationToken)
        ?? throw new InvalidOperationException("The original development draft save could not be confirmed. Recover its original intent.");

    public async Task<DevelopmentPlanDraftRecovery> RecoverDraftPlanAsync(CreateDevelopmentPlan original,CancellationToken cancellationToken = default) =>
        await _apiClient.PostAsync<CreateDevelopmentPlan,DevelopmentPlanDraftRecovery>("/api/field/current/development-planning/plans/recover",original,cancellationToken)
        ?? throw new InvalidOperationException("The original development draft outcome could not be confirmed.");

    public async Task<List<POOL>> GetPoolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<POOL>>(
                "/api/field/current/development/pools",
                cancellationToken);

            return result ?? new List<POOL>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development pools.");
            throw;
        }
    }

    public async Task<POOL?> CreatePoolAsync(PoolRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<PoolRequest, POOL>(
                "/api/field/current/development/pools",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating development pool {PoolName}.", request.PoolName);
            throw;
        }
    }

    public async Task<POOL?> UpdatePoolAsync(string poolId, PoolRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(poolId))
            throw new ArgumentException("Pool ID is required.", nameof(poolId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PutAsync<PoolRequest, POOL>(
                $"/api/field/current/development/pools/{Uri.EscapeDataString(poolId)}",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating development pool {PoolId}.", poolId);
            throw;
        }
    }

    public async Task<DevelopmentFacilityCreationReview> ReviewFacilityCreationAsync(DevelopmentFacilityCreationDraft draft,CancellationToken cancellationToken=default)
    {
        draft.Require();var result=await _apiClient.PostAsync<DevelopmentFacilityCreationDraft,DevelopmentFacilityCreationReview>("/api/field/current/development/facilities/creation-review?fieldId="+Uri.EscapeDataString(draft.Context.FieldId!),draft,cancellationToken)
            ?? throw new InvalidOperationException("The facility creation review was unavailable.");
        result.Require();if(System.Text.Json.JsonSerializer.Serialize(result.Draft)!=System.Text.Json.JsonSerializer.Serialize(draft))throw new InvalidOperationException("The reviewed facility proposal differs from the original.");return result;
    }
    public Task<DevelopmentFacilityCreationReceipt> SaveFacilityCreationAsync(DevelopmentFacilityCreation original,CancellationToken cancellationToken=default)=>FacilityCreation(original,false,cancellationToken);
    public Task<DevelopmentFacilityCreationReceipt> RecoverFacilityCreationAsync(DevelopmentFacilityCreation original,CancellationToken cancellationToken=default)=>FacilityCreation(original,true,cancellationToken);
    private async Task<DevelopmentFacilityCreationReceipt> FacilityCreation(DevelopmentFacilityCreation original,bool recover,CancellationToken token)
    {
        original.Require();var path="/api/field/current/development/facilities/creation"+(recover?"/recover":"")+"?fieldId="+Uri.EscapeDataString(original.Review.Draft.Context.FieldId!);
        var result=await _apiClient.PostAsync<DevelopmentFacilityCreation,DevelopmentFacilityCreationReceipt>(path,original,token)??throw new InvalidOperationException("The original facility creation outcome is unconfirmed. Preserve its retained input.");result.Require(original);return result;
    }

    public Task<DevelopmentFacilityEditReview> ReviewFacilityEditAsync(string facilityId,string facilityType,string editorRole,string fieldId,CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(facilityId) || string.IsNullOrWhiteSpace(facilityType) || string.IsNullOrWhiteSpace(editorRole) || string.IsNullOrWhiteSpace(fieldId))
            throw new ArgumentException("The original facility, type, role and FIELD are required.");
        var path="/api/field/current/development/facilities/edit-review?facilityId="+Uri.EscapeDataString(facilityId)+"&facilityType="+Uri.EscapeDataString(facilityType)+"&editorRole="+Uri.EscapeDataString(editorRole)+"&fieldId="+Uri.EscapeDataString(fieldId);
        return Review();
        async Task<DevelopmentFacilityEditReview> Review()=>await _apiClient.GetAsync<DevelopmentFacilityEditReview>(path,cancellationToken) ?? throw new InvalidOperationException("Facility edit review was unavailable.");
    }

    public Task<DevelopmentFacilityEditReceipt> SaveFacilityEditAsync(DevelopmentFacilityEdit original,CancellationToken cancellationToken=default)=>FacilityEdit(original,false,cancellationToken);
    public Task<DevelopmentFacilityEditReceipt> RecoverFacilityEditAsync(DevelopmentFacilityEdit original,CancellationToken cancellationToken=default)=>FacilityEdit(original,true,cancellationToken);
    private async Task<DevelopmentFacilityEditReceipt> FacilityEdit(DevelopmentFacilityEdit original,bool recover,CancellationToken token)
    {
        original.Require();var result=await _apiClient.PostAsync<DevelopmentFacilityEdit,DevelopmentFacilityEditReceipt>("/api/field/current/development/facilities/edit"+(recover?"/recover":""),original,token)
            ??throw new InvalidOperationException("The original facility edit outcome is unconfirmed. Preserve its retained input.");
        result.Require(original);return result;
    }

    public Task<DevelopmentEquipmentInstallationReceipt> SaveEquipmentInstallationAsync(DevelopmentEquipmentInstallation original,CancellationToken cancellationToken=default)=>SendInstallationAsync(original,false,cancellationToken);
    public Task<DevelopmentEquipmentInstallationReceipt> RecoverEquipmentInstallationAsync(DevelopmentEquipmentInstallation original,CancellationToken cancellationToken=default)=>SendInstallationAsync(original,true,cancellationToken);
    private async Task<DevelopmentEquipmentInstallationReceipt> SendInstallationAsync(DevelopmentEquipmentInstallation original,bool recover,CancellationToken token)
    {
        original.Require();var path="/api/field/current/development/facilities/equipment/installation"+(recover?"/recover":"")+"?fieldId="+Uri.EscapeDataString(original.Review.Proposal.Context.FieldId!);
        var result=await _apiClient.PostAsync<DevelopmentEquipmentInstallation,DevelopmentEquipmentInstallationReceipt>(path,original,token)??throw new InvalidOperationException("The original installation outcome is unconfirmed.");result.Require(original);return result;
    }

    public async Task<DevelopmentEquipmentInstallationReview> ReviewEquipmentInstallationAsync(DevelopmentEquipmentInstallationProposal proposal,CancellationToken cancellationToken=default)
    {
        proposal.Require();var result=await _apiClient.PostAsync<DevelopmentEquipmentInstallationProposal,DevelopmentEquipmentInstallationReview>("/api/field/current/development/facilities/equipment/installation-review?fieldId="+Uri.EscapeDataString(proposal.Context.FieldId!),proposal,cancellationToken) ?? throw new InvalidOperationException("Equipment installation evidence was unavailable.");
        result.Require();if(result.Proposal!=proposal)throw new InvalidOperationException("The equipment review differs from the original proposal.");return result;
    }

    public async Task<List<DevelopmentFacilityEquipmentItem>> GetFacilityEquipmentAsync(string facilityId,string facilityType,string fieldId,CancellationToken cancellationToken=default)
    {
        var path="/api/field/current/development/facilities/equipment?facilityId="+Uri.EscapeDataString(facilityId)+"&facilityType="+Uri.EscapeDataString(facilityType)+"&fieldId="+Uri.EscapeDataString(fieldId);
        return await _apiClient.GetAsync<List<DevelopmentFacilityEquipmentItem>>(path,cancellationToken) ?? throw new InvalidOperationException("Facility equipment evidence was unavailable.");
    }

    public async Task<List<FACILITY>> GetFacilitiesAsync(CancellationToken cancellationToken = default, string? fieldId = null)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<FACILITY>>(
                "/api/field/current/development/facilities" + FieldQuery(fieldId),
                cancellationToken);

            return result ?? throw new InvalidOperationException("Development facility evidence was unavailable.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development facilities.");
            throw;
        }
    }

}
