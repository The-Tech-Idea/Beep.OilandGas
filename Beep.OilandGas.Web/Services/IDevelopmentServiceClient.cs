using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Web.Services;

public interface IDevelopmentServiceClient
{
    Task<DevelopmentDashboardSummary?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<List<DevelopmentWellStatusDto>> GetDashboardWellsAsync(CancellationToken cancellationToken = default);
    Task<List<WELL>> GetWellsAsync(CancellationToken cancellationToken = default, string? fieldId = null);
    Task<WELL?> GetWellAsync(string uwi, CancellationToken cancellationToken = default, string? fieldId = null);
    Task AssignRigAsync(string uwi, DevelopmentAssignRigRequest request, CancellationToken cancellationToken = default);
    Task<DevelopmentReviewCatalog> GetDraftReviewWorkflowsAsync(string id,string sourceRevision,CancellationToken cancellationToken = default);
    Task<DevelopmentPlanBaselineReview> GetDraftBaselineAsync(string processId,string reviewRevision,CancellationToken cancellationToken=default);
    Task<DevelopmentPlanReleaseReceipt> ApproveDraftBaselineAsync(DevelopmentPlanRelease original,CancellationToken cancellationToken=default);
    Task<DevelopmentPlanReleaseReceipt> RecoverDraftBaselineAsync(DevelopmentPlanRelease original,CancellationToken cancellationToken=default);
    Task<DevelopmentReviewProgress> GetDraftReviewProgressAsync(string processId,CancellationToken cancellationToken=default);
    Task<DevelopmentPlanReviewStatus> GetDraftReviewStatusAsync(string id,string sourceRevision,CancellationToken cancellationToken=default);
    Task<DevelopmentAssignedReview> GetAssignedDraftReviewAsync(string processId,CancellationToken cancellationToken=default);
    Task<DevelopmentReviewDecisionReceipt> DecideDraftReviewAsync(DevelopmentReviewDecision command,CancellationToken cancellationToken=default);
    Task<DevelopmentReviewDecisionReceipt> RecoverDraftReviewDecisionAsync(DevelopmentReviewDecision original,CancellationToken cancellationToken=default);
    Task<DevelopmentReviewStart> PrepareDraftReviewAsync(string id,DevelopmentReviewSelection selection,CancellationToken cancellationToken=default);
    Task<DevelopmentReviewStartReceipt> SubmitDraftReviewAsync(DevelopmentReviewStart original,CancellationToken cancellationToken=default);
    Task<DevelopmentReviewStartReceipt> RecoverDraftReviewAsync(DevelopmentReviewStart original,CancellationToken cancellationToken=default);
    Task<DevelopmentPlanSourceReview> GetDraftPlanAsync(string id,CancellationToken cancellationToken = default);
    Task<DevelopmentPlanDraftList> GetDraftPlansAsync(CancellationToken cancellationToken = default);
    Task<DevelopmentPlanDraftOptions> GetDraftOptionsAsync(CancellationToken cancellationToken = default);
    Task<FIELD_DEVELOPMENT_PLAN> SaveDraftPlanAsync(CreateDevelopmentPlan request,CancellationToken cancellationToken = default);
    Task<DevelopmentPlanDraftRecovery> RecoverDraftPlanAsync(CreateDevelopmentPlan original,CancellationToken cancellationToken = default);
    Task<List<POOL>> GetPoolsAsync(CancellationToken cancellationToken = default);
    Task<POOL?> CreatePoolAsync(PoolRequest request, CancellationToken cancellationToken = default);
    Task<POOL?> UpdatePoolAsync(string poolId, PoolRequest request, CancellationToken cancellationToken = default);
    Task<List<FACILITY>> GetFacilitiesAsync(CancellationToken cancellationToken = default, string? fieldId = null);
    Task<DevelopmentEquipmentInstallationReceipt> SaveEquipmentInstallationAsync(DevelopmentEquipmentInstallation original,CancellationToken cancellationToken=default);
    Task<DevelopmentEquipmentInstallationReceipt> RecoverEquipmentInstallationAsync(DevelopmentEquipmentInstallation original,CancellationToken cancellationToken=default);
    Task<DevelopmentEquipmentInstallationReview> ReviewEquipmentInstallationAsync(DevelopmentEquipmentInstallationProposal proposal,CancellationToken cancellationToken=default);
    Task<List<DevelopmentFacilityEquipmentItem>> GetFacilityEquipmentAsync(string facilityId,string facilityType,string fieldId,CancellationToken cancellationToken = default);
    Task<DevelopmentFacilityEditReview> ReviewFacilityEditAsync(string facilityId,string facilityType,string editorRole,string fieldId,CancellationToken cancellationToken = default);
    Task<DevelopmentFacilityCreationReview> ReviewFacilityCreationAsync(DevelopmentFacilityCreationDraft draft,CancellationToken cancellationToken = default);
    Task<DevelopmentFacilityCreationReceipt> SaveFacilityCreationAsync(DevelopmentFacilityCreation original,CancellationToken cancellationToken = default);
    Task<DevelopmentFacilityCreationReceipt> RecoverFacilityCreationAsync(DevelopmentFacilityCreation original,CancellationToken cancellationToken = default);
    Task<DevelopmentFacilityEditReceipt> SaveFacilityEditAsync(DevelopmentFacilityEdit original,CancellationToken cancellationToken = default);
    Task<DevelopmentFacilityEditReceipt> RecoverFacilityEditAsync(DevelopmentFacilityEdit original,CancellationToken cancellationToken = default);
}

public sealed class DevelopmentAssignRigRequest
{
    public string RigName { get; set; } = string.Empty;
    public DateTime? MobDate { get; set; }
}
