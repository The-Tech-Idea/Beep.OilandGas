using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.LeaseAcquisition.Data.Lease.Projections;

namespace Beep.OilandGas.LeaseAcquisition.Services;

/// <summary>
/// Advanced / staged lease acquisition surface (negotiation, lifecycle staging, reporting helpers).
/// Canonical persistence for API consumers remains <see cref="Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService"/>.
/// </summary>
public interface ILeaseAcquisitionAdvancedService
{
    #region Lease Acquisition

    Task<LeaseAcquisitionDetail> InitiateLeaseAcquisitionAsync(LeaseAcquisitionRequest request, string userId);

    Task<List<LeaseProspect>> SearchLeaseProspectsAsync(LeaseSearchCriteria criteria);

    Task<LeaseOpportunityEvaluation> EvaluateLeaseOpportunityAsync(string leaseId, LeaseEvaluationRequest request);

    Task<CompetitiveAnalysis> AnalyzeCompetitiveOpportunitiesAsync(string locationId, AnalysisRequest request);

    Task<LeaseFinancialAnalysis> AnalyzeLeaseFinancialsAsync(string leaseId, FinancialAnalysisRequest request);

    #endregion

    #region Negotiation and Deal Management

    Task<LeaseNegotiation> InitiateNegotiationAsync(string leaseId, NegotiationInitiation request, string userId);

    Task<NegotiationProgress> TrackNegotiationProgressAsync(string negotiationId);

    Task<LeaseTerms> ManageLeaseTermsAsync(string leaseId, LeaseTermsUpdate update, string userId);

    Task<LeaseDocumentation> PrepareLeaseDocumentationAsync(string leaseId, DocumentationRequest request);

    Task<ExecutionStatus> ExecuteLeaseAgreementAsync(string leaseId, ExecutionDetails details, string userId);

    Task<ROYALTY_CALCULATION> CalculateRoyaltyDistributionAsync(string leaseId, RoyaltyRequest request);

    #endregion

    #region Rights and Obligations

    Task<MineralRightsManagement> ManageMineralRightsAsync(string leaseId, MineralRightsRequest request, string userId);

    Task<SurfaceRightsManagement> ManageSurfaceRightsAsync(string leaseId, SurfaceRightsRequest request, string userId);

    Task<WaterRightsManagement> ManageWaterRightsAsync(string leaseId, WaterRightsRequest request, string userId);

    Task<EnvironmentalObligation> TrackEnvironmentalObligationsAsync(string leaseId);

    Task<RegulatoryCompliance> ManageRegulatoryComplianceAsync(string leaseId, ComplianceRequest request);

    Task<OperationalObligation> TrackOperationalObligationsAsync(string leaseId);

    #endregion

    #region Lease Lifecycle

    Task<LeaseRenewal> InitiateLeaseRenewalAsync(string leaseId, RenewalRequest request, string userId);

    Task<LeaseExtension> ExtendLeaseTermAsync(string leaseId, ExtensionRequest request, string userId);

    Task<LeaseExpirationSchedule> GetLeaseExpirationScheduleAsync(int daysAhead = 90);

    Task<LeaseTermination> InitiateLeaseTerminationAsync(string leaseId, TerminationRequest request, string userId);

    Task<LeaseAbandonment> ProcessLeaseAbandonmentAsync(string leaseId, AbandonmentRequest request, string userId);

    #endregion

    #region Stakeholder Management

    Task<LandownerManagement> ManageLandownerAsync(string leaseId, LandownerDetails details, string userId);

    Task<WorkingInterestManagement> ManageWorkingInterestAsync(string leaseId, WorkingInterestRequest request, string userId);

    Task<RoyaltyOwnerManagement> ManageRoyaltyOwnerAsync(string leaseId, RoyaltyOwnerDetails details, string userId);

    Task<StakeholderCommunication> LogCommunicationAsync(string leaseId, CommunicationLog log, string userId);

    Task<StakeholderAgreement> ManageStakeholderAgreementAsync(string leaseId, AgreementDetails agreement, string userId);

    #endregion

    #region Financial Management

    Task<BudgetManagement> ManageBudgetAsync(string leaseId, BudgetRequest budget, string userId);

    Task<CostTracking> TrackAcquisitionCostsAsync(string leaseId, CostEntry cost, string userId);

    Task<LeasePaymentDetail> ProcessLeasePaymentAsync(string leaseId, PaymentDetails payment, string userId);

    Task<LeaseValueMetrics> CalculateLeaseValueMetricsAsync(string leaseId);

    Task<ReserveRequirement> ManageReserveRequirementsAsync(string leaseId, ReserveRequest reserve, string userId);

    #endregion

    #region Due Diligence

    Task<TitleExamination> PerformTitleExaminationAsync(string leaseId, TitleRequest request);

    Task<EnvironmentalAssessment> ConductEnvironmentalAssessmentAsync(string leaseId, AssessmentRequest request);

    Task<RegulatoryReview> PerformRegulatoryReviewAsync(string leaseId, RegulationReviewRequest request);

    Task<GeologicalEvaluation> EvaluateGeologicalProspectsAsync(string leaseId, GeologicalRequest request);

    Task<EngineeringFeasibility> AssessEngineeringFeasibilityAsync(string leaseId, EngineeringRequest request);

    #endregion

    #region Data Management

    Task SaveLeaseAcquisitionAsync(LeaseAcquisitionDetail acquisition, string userId);

    Task<LeaseAcquisitionDetail?> GetLeaseAcquisitionAsync(string leaseId);

    Task UpdateLeaseAcquisitionAsync(LeaseAcquisitionDetail acquisition, string userId);

    Task<List<LeaseHistory>> GetLeaseHistoryAsync(string leaseId);

    #endregion

    #region Reporting and Analysis

    Task<LeaseAcquisitionReport> GenerateAcquisitionReportAsync(string leaseId, ReportRequest request);

    Task<PortfolioAnalysisReport> GeneratePortfolioAnalysisAsync(PortfolioRequest request);

    Task<byte[]> ExportLeaseDataAsync(string leaseId, string format = "CSV");

    #endregion
}
