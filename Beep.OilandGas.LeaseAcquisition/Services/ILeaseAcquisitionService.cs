using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.LeaseAcquisition.Services
{
    /// <summary>
    /// Comprehensive lease acquisition service interface
    /// Provides lease acquisition, management, negotiation, and lifecycle capabilities
    /// </summary>
    public interface ILeaseAcquisitionService
    {
        #region Lease Acquisition

        /// <summary>
        /// Initiates new lease acquisition process
        /// </summary>
        Task<LeaseAcquisitionDto> InitiateLeaseAcquisitionAsync(LeaseAcquisitionRequestDto request, string userId);

        /// <summary>
        /// Searches for available lease prospects
        /// </summary>
        Task<List<LeaseProspectDto>> SearchLeaseProspectsAsync(LeaseSearchCriteriaDto criteria);

        /// <summary>
        /// Evaluates lease acquisition opportunity
        /// </summary>
        Task<LeaseOpportunityEvaluationDto> EvaluateLeaseOpportunityAsync(string leaseId, LeaseEvaluationRequestDto request);

        /// <summary>
        /// Performs competitive analysis for lease acquisition
        /// </summary>
        Task<CompetitiveAnalysisDto> AnalyzeCompetitiveOpportunitiesAsync(string locationId, AnalysisRequestDto request);

        /// <summary>
        /// Calculates lease acquisition costs and returns
        /// </summary>
        Task<LeaseFinancialAnalysisDto> AnalyzeLeaseFinancialsAsync(string leaseId, FinancialAnalysisRequestDto request);

        #endregion

        #region Negotiation and Deal Management

        /// <summary>
        /// Initiates lease negotiation process
        /// </summary>
        Task<LeaseNegotiationDto> InitiateNegotiationAsync(string leaseId, NegotiationInitiationDto request, string userId);

        /// <summary>
        /// Tracks negotiation progress and milestones
        /// </summary>
        Task<NegotiationProgressDto> TrackNegotiationProgressAsync(string negotiationId);

        /// <summary>
        /// Manages term conditions and modifications
        /// </summary>
        Task<LeaseTermsDto> ManageLeaseTermsAsync(string leaseId, LeaseTermsUpdateDto update, string userId);

        /// <summary>
        /// Prepares lease documentation
        /// </summary>
        Task<LeaseDocumentationDto> PrepareLeaseDocumentationAsync(string leaseId, DocumentationRequestDto request);

        /// <summary>
        /// Manages lease agreement execution
        /// </summary>
        Task<ExecutionStatusDto> ExecuteLeaseAgreementAsync(string leaseId, ExecutionDetailsDto details, string userId);

        /// <summary>
        /// Calculates royalty and revenue distribution
        /// </summary>
        Task<RoyaltyCalculationDto> CalculateRoyaltyDistributionAsync(string leaseId, RoyaltyRequestDto request);

        #endregion

        #region Rights and Obligations

        /// <summary>
        /// Manages mineral rights
        /// </summary>
        Task<MineralRightsManagementDto> ManageMineralRightsAsync(string leaseId, MineralRightsRequestDto request, string userId);

        /// <summary>
        /// Manages surface rights
        /// </summary>
        Task<SurfaceRightsManagementDto> ManageSurfaceRightsAsync(string leaseId, SurfaceRightsRequestDto request, string userId);

        /// <summary>
        /// Manages water rights
        /// </summary>
        Task<WaterRightsManagementDto> ManageWaterRightsAsync(string leaseId, WaterRightsRequestDto request, string userId);

        /// <summary>
        /// Tracks environmental obligations
        /// </summary>
        Task<EnvironmentalObligationDto> TrackEnvironmentalObligationsAsync(string leaseId);

        /// <summary>
        /// Manages regulatory compliance requirements
        /// </summary>
        Task<RegulatoryComplianceDto> ManageRegulatoryComplianceAsync(string leaseId, ComplianceRequestDto request);

        /// <summary>
        /// Tracks operational obligations
        /// </summary>
        Task<OperationalObligationDto> TrackOperationalObligationsAsync(string leaseId);

        #endregion

        #region Lease Lifecycle

        /// <summary>
        /// Manages lease renewals
        /// </summary>
        Task<LeaseRenewalDto> InitiateLeaseRenewalAsync(string leaseId, RenewalRequestDto request, string userId);

        /// <summary>
        /// Manages lease extensions
        /// </summary>
        Task<LeaseExtensionDto> ExtendLeaseTermAsync(string leaseId, ExtensionRequestDto request, string userId);

        /// <summary>
        /// Tracks lease expiration schedule
        /// </summary>
        Task<LeaseExpirationScheduleDto> GetLeaseExpirationScheduleAsync(int daysAhead = 90);

        /// <summary>
        /// Manages lease termination process
        /// </summary>
        Task<LeaseTerminationDto> InitiateLeaseTerminationAsync(string leaseId, TerminationRequestDto request, string userId);

        /// <summary>
        /// Manages lease abandonment
        /// </summary>
        Task<LeaseAbandonmentDto> ProcessLeaseAbandonmentAsync(string leaseId, AbandonmentRequestDto request, string userId);

        #endregion

        #region Stakeholder Management

        /// <summary>
        /// Manages landowner relationships
        /// </summary>
        Task<LandownerManagementDto> ManageLandownerAsync(string leaseId, LandownerDetailsDto details, string userId);

        /// <summary>
        /// Manages working interest partners
        /// </summary>
        Task<WorkingInterestManagementDto> ManageWorkingInterestAsync(string leaseId, WorkingInterestRequestDto request, string userId);

        /// <summary>
        /// Manages royalty owners
        /// </summary>
        Task<RoyaltyOwnerManagementDto> ManageRoyaltyOwnerAsync(string leaseId, RoyaltyOwnerDetailsDto details, string userId);

        /// <summary>
        /// Tracks stakeholder communications
        /// </summary>
        Task<StakeholderCommunicationDto> LogCommunicationAsync(string leaseId, CommunicationLogDto log, string userId);

        /// <summary>
        /// Manages stakeholder agreements
        /// </summary>
        Task<StakeholderAgreementDto> ManageStakeholderAgreementAsync(string leaseId, AgreementDetailsDto agreement, string userId);

        #endregion

        #region Financial Management

        /// <summary>
        /// Manages lease acquisition budgets
        /// </summary>
        Task<BudgetManagementDto> ManageBudgetAsync(string leaseId, BudgetRequestDto budget, string userId);

        /// <summary>
        /// Tracks acquisition costs and expenses
        /// </summary>
        Task<CostTrackingDto> TrackAcquisitionCostsAsync(string leaseId, CostEntryDto cost, string userId);

        /// <summary>
        /// Manages lease payments and escrows
        /// </summary>
        Task<LeasePaymentDto> ProcessLeasePaymentAsync(string leaseId, PaymentDetailsDto payment, string userId);

        /// <summary>
        /// Calculates lease value and metrics
        /// </summary>
        Task<LeaseValueMetricsDto> CalculateLeaseValueMetricsAsync(string leaseId);

        /// <summary>
        /// Manages reserve requirements
        /// </summary>
        Task<ReserveRequirementDto> ManageReserveRequirementsAsync(string leaseId, ReserveRequestDto reserve, string userId);

        #endregion

        #region Due Diligence

        /// <summary>
        /// Performs title examination
        /// </summary>
        Task<TitleExaminationDto> PerformTitleExaminationAsync(string leaseId, TitleRequestDto request);

        /// <summary>
        /// Conducts environmental assessment
        /// </summary>
        Task<EnvironmentalAssessmentDto> ConductEnvironmentalAssessmentAsync(string leaseId, AssessmentRequestDto request);

        /// <summary>
        /// Performs regulatory review
        /// </summary>
        Task<RegulatoryReviewDto> PerformRegulatoryReviewAsync(string leaseId, RegulationReviewRequestDto request);

        /// <summary>
        /// Executes geological evaluation
        /// </summary>
        Task<GeologicalEvaluationDto> EvaluateGeologicalProspectsAsync(string leaseId, GeologicalRequestDto request);

        /// <summary>
        /// Conducts engineering feasibility study
        /// </summary>
        Task<EngineeringFeasibilityDto> AssessEngineeringFeasibilityAsync(string leaseId, EngineeringRequestDto request);

        #endregion

        #region Data Management

        /// <summary>
        /// Saves lease acquisition data
        /// </summary>
        Task SaveLeaseAcquisitionAsync(LeaseAcquisitionDto acquisition, string userId);

        /// <summary>
        /// Retrieves lease acquisition data
        /// </summary>
        Task<LeaseAcquisitionDto?> GetLeaseAcquisitionAsync(string leaseId);

        /// <summary>
        /// Updates lease acquisition data
        /// </summary>
        Task UpdateLeaseAcquisitionAsync(LeaseAcquisitionDto acquisition, string userId);

        /// <summary>
        /// Retrieves lease history
        /// </summary>
        Task<List<LeaseHistoryDto>> GetLeaseHistoryAsync(string leaseId);

        #endregion

        #region Reporting and Analysis

        /// <summary>
        /// Generates lease acquisition report
        /// </summary>
        Task<LeaseAcquisitionReportDto> GenerateAcquisitionReportAsync(string leaseId, ReportRequestDto request);

        /// <summary>
        /// Generates portfolio analysis report
        /// </summary>
        Task<PortfolioAnalysisReportDto> GeneratePortfolioAnalysisAsync(PortfolioRequestDto request);

        /// <summary>
        /// Exports lease data
        /// </summary>
        Task<byte[]> ExportLeaseDataAsync(string leaseId, string format = "CSV");

        #endregion
    }

    #region Lease Acquisition DTOs

    /// <summary>
    /// Lease acquisition DTO
    /// </summary>
    public class LeaseAcquisitionDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string LeaseName { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public decimal AcreageSize { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AcquisitionCost { get; set; }
        public string County { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public List<string> Operators { get; set; } = new();
        public List<string> Stakeholders { get; set; } = new();
    }

    /// <summary>
    /// Lease acquisition request DTO
    /// </summary>
    public class LeaseAcquisitionRequestDto
    {
        public string LeaseName { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public decimal AcreageSize { get; set; }
        public string County { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public List<string> ObjectiveFormations { get; set; } = new();
    }

    /// <summary>
    /// Lease prospect DTO
    /// </summary>
    public class LeaseProspectDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public decimal AcreageAvailable { get; set; }
        public string County { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public decimal EstimatedValue { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public List<string> LandOwners { get; set; } = new();
    }

    /// <summary>
    /// Lease search criteria DTO
    /// </summary>
    public class LeaseSearchCriteriaDto
    {
        public string? LocationId { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public decimal? MinAcreage { get; set; }
        public decimal? MaxAcreage { get; set; }
        public List<string> FormationTargets { get; set; } = new();
    }

    /// <summary>
    /// Lease opportunity evaluation DTO
    /// </summary>
    public class LeaseOpportunityEvaluationDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal OpportunityScore { get; set; }
        public string RecommendationStatus { get; set; } = string.Empty;
        public List<EvaluationFactorDto> Factors { get; set; } = new();
        public string EvaluationSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Evaluation factor DTO
    /// </summary>
    public class EvaluationFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lease evaluation request DTO
    /// </summary>
    public class LeaseEvaluationRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<string> EvaluationCriteria { get; set; } = new();
        public bool IncludeFinancialAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Competitive analysis DTO
    /// </summary>
    public class CompetitiveAnalysisDto
    {
        public string LocationId { get; set; } = string.Empty;
        public List<CompetitorBidDto> CompetitorBids { get; set; } = new();
        public decimal AverageBonus { get; set; }
        public decimal AverageRoyaltyRate { get; set; }
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Competitor bid DTO
    /// </summary>
    public class CompetitorBidDto
    {
        public string CompetitorName { get; set; } = string.Empty;
        public decimal BonusAmount { get; set; }
        public decimal RoyaltyRate { get; set; }
        public decimal WorkingInterest { get; set; }
    }

    /// <summary>
    /// Analysis request DTO
    /// </summary>
    public class AnalysisRequestDto
    {
        public string LocationId { get; set; } = string.Empty;
        public int HistoricalYears { get; set; } = 5;
        public bool IncludeGeologicalFactors { get; set; } = true;
    }

    /// <summary>
    /// Lease financial analysis DTO
    /// </summary>
    public class LeaseFinancialAnalysisDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal EstimatedResources { get; set; }
        public decimal ProductionValue { get; set; }
        public decimal NetPresentValue { get; set; }
        public decimal InternalRateOfReturn { get; set; }
        public decimal PaybackPeriod { get; set; }
        public string InvestmentRecommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Financial analysis request DTO
    /// </summary>
    public class FinancialAnalysisRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal OilPrice { get; set; }
        public decimal GasPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public int ProjectionYears { get; set; } = 20;
    }

    #endregion

    #region Negotiation DTOs

    /// <summary>
    /// Lease negotiation DTO
    /// </summary>
    public class LeaseNegotiationDto
    {
        public string NegotiationId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime InitiationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CurrentPhase { get; set; } = string.Empty;
        public List<NegotiationRoundDto> Rounds { get; set; } = new();
    }

    /// <summary>
    /// Negotiation round DTO
    /// </summary>
    public class NegotiationRoundDto
    {
        public int RoundNumber { get; set; }
        public DateTime RoundDate { get; set; }
        public string TopicsFocused { get; set; } = string.Empty;
        public List<string> OpenIssues { get; set; } = new();
        public string Outcome { get; set; } = string.Empty;
    }

    /// <summary>
    /// Negotiation initiation DTO
    /// </summary>
    public class NegotiationInitiationDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string LandownerId { get; set; } = string.Empty;
        public decimal OpeningBonus { get; set; }
        public decimal OpeningRoyaltyRate { get; set; }
        public List<string> NegotiationObjectives { get; set; } = new();
    }

    /// <summary>
    /// Negotiation progress DTO
    /// </summary>
    public class NegotiationProgressDto
    {
        public string NegotiationId { get; set; } = string.Empty;
        public int ProgressPercent { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public List<string> ResolvedIssues { get; set; } = new();
        public List<string> RemainingIssues { get; set; } = new();
        public DateTime? ExpectedClosureDate { get; set; }
    }

    /// <summary>
    /// Lease terms DTO
    /// </summary>
    public class LeaseTermsDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal RoyaltyRate { get; set; }
        public decimal RentAmount { get; set; }
        public decimal MinimumProduction { get; set; }
        public List<TermConditionDto> Conditions { get; set; } = new();
    }

    /// <summary>
    /// Term condition DTO
    /// </summary>
    public class TermConditionDto
    {
        public string ConditionDescription { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public string ConditionStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lease terms update DTO
    /// </summary>
    public class LeaseTermsUpdateDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public DateTime? NewExpirationDate { get; set; }
        public decimal? NewRoyaltyRate { get; set; }
        public decimal? NewRentAmount { get; set; }
        public List<TermConditionDto> ConditionUpdates { get; set; } = new();
    }

    /// <summary>
    /// Lease documentation DTO
    /// </summary>
    public class LeaseDocumentationDto
    {
        public string DocumentId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public List<DocumentPackageDto> DocumentPackages { get; set; } = new();
        public DateTime PreparedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Document package DTO
    /// </summary>
    public class DocumentPackageDto
    {
        public string PackageName { get; set; } = string.Empty;
        public List<string> Documents { get; set; } = new();
        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// Documentation request DTO
    /// </summary>
    public class DocumentationRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<string> RequiredDocuments { get; set; } = new();
    }

    /// <summary>
    /// Execution status DTO
    /// </summary>
    public class ExecutionStatusDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public bool IsExecuted { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public string ExecutionStatus { get; set; } = string.Empty;
        public List<string> SignatoryStatus { get; set; } = new();
    }

    /// <summary>
    /// Execution details DTO
    /// </summary>
    public class ExecutionDetailsDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public List<string> SigningParties { get; set; } = new();
        public string ExecutionLocation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Royalty calculation DTO
    /// </summary>
    public class RoyaltyCalculationDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal ProductionVolume { get; set; }
        public decimal RoyaltyRate { get; set; }
        public decimal CalculatedRoyalty { get; set; }
        public List<RoyaltyShareDto> Shares { get; set; } = new();
    }

    /// <summary>
    /// Royalty share DTO
    /// </summary>
    public class RoyaltyShareDto
    {
        public string RecipientName { get; set; } = string.Empty;
        public decimal SharePercentage { get; set; }
        public decimal ShareAmount { get; set; }
    }

    /// <summary>
    /// Royalty request DTO
    /// </summary>
    public class RoyaltyRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal ProductionVolume { get; set; }
        public decimal ProductPrice { get; set; }
    }

    #endregion

    #region Rights and Obligations DTOs

    /// <summary>
    /// Mineral rights management DTO
    /// </summary>
    public class MineralRightsManagementDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<MineralRightDto> Rights { get; set; } = new();
        public string ManagementStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mineral right DTO
    /// </summary>
    public class MineralRightDto
    {
        public string MineralName { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public decimal OwnershipPercentage { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Mineral rights request DTO
    /// </summary>
    public class MineralRightsRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<MineralRightDto> RightsToAdd { get; set; } = new();
    }

    /// <summary>
    /// Surface rights management DTO
    /// </summary>
    public class SurfaceRightsManagementDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal SurfaceArea { get; set; }
        public string SurfaceOwner { get; set; } = string.Empty;
        public bool AccessGranted { get; set; }
        public List<AccessRightDto> AccessRights { get; set; } = new();
    }

    /// <summary>
    /// Access right DTO
    /// </summary>
    public class AccessRightDto
    {
        public string RightDescription { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    /// <summary>
    /// Surface rights request DTO
    /// </summary>
    public class SurfaceRightsRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string SurfaceOwner { get; set; } = string.Empty;
        public List<AccessRightDto> RequestedRights { get; set; } = new();
    }

    /// <summary>
    /// Water rights management DTO
    /// </summary>
    public class WaterRightsManagementDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal WaterAllocationVolume { get; set; }
        public string WaterSource { get; set; } = string.Empty;
        public bool WaterRightsGranted { get; set; }
        public List<string> Restrictions { get; set; } = new();
    }

    /// <summary>
    /// Water rights request DTO
    /// </summary>
    public class WaterRightsRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string WaterSource { get; set; } = string.Empty;
        public decimal RequestedVolume { get; set; }
    }

    /// <summary>
    /// Environmental obligation DTO
    /// </summary>
    public class EnvironmentalObligationDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<ObligationDto> Obligations { get; set; } = new();
        public string OverallComplianceStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Obligation DTO
    /// </summary>
    public class ObligationDto
    {
        public string ObligationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ComplianceStatus { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// Regulatory compliance DTO
    /// </summary>
    public class RegulatoryComplianceDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<ComplianceItemDto> ComplianceItems { get; set; } = new();
        public string OverallComplianceStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Compliance item DTO
    /// </summary>
    public class ComplianceItemDto
    {
        public string RequirementName { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public bool IsCompliant { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Compliance request DTO
    /// </summary>
    public class ComplianceRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<string> JurisdictionsToReview { get; set; } = new();
    }

    /// <summary>
    /// Operational obligation DTO
    /// </summary>
    public class OperationalObligationDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<OperationalItemDto> ObligationItems { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Operational item DTO
    /// </summary>
    public class OperationalItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsComplete { get; set; }
    }

    #endregion

    #region Lifecycle DTOs

    /// <summary>
    /// Lease renewal DTO
    /// </summary>
    public class LeaseRenewalDto
    {
        public string RenewalId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime RenewalDate { get; set; }
        public DateTime NewExpirationDate { get; set; }
        public decimal RenewalBonus { get; set; }
        public string RenewalStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Renewal request DTO
    /// </summary>
    public class RenewalRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public int RenewalTermYears { get; set; }
        public decimal ProposedBonus { get; set; }
    }

    /// <summary>
    /// Lease extension DTO
    /// </summary>
    public class LeaseExtensionDto
    {
        public string ExtensionId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime ExtensionDate { get; set; }
        public int ExtensionTermYears { get; set; }
        public string ExtensionStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extension request DTO
    /// </summary>
    public class ExtensionRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public int ExtensionYears { get; set; }
        public decimal ExtensionCost { get; set; }
    }

    /// <summary>
    /// Lease expiration schedule DTO
    /// </summary>
    public class LeaseExpirationScheduleDto
    {
        public DateTime ReportDate { get; set; }
        public List<ExpiringLeaseDto> ExpiringLeases { get; set; } = new();
        public int TotalExpiringCount { get; set; }
    }

    /// <summary>
    /// Expiring lease DTO
    /// </summary>
    public class ExpiringLeaseDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string LeaseName { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int DaysUntilExpiration { get; set; }
        public decimal AcreageAtRisk { get; set; }
    }

    /// <summary>
    /// Lease termination DTO
    /// </summary>
    public class LeaseTerminationDto
    {
        public string TerminationId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime TerminationDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string TerminationStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Termination request DTO
    /// </summary>
    public class TerminationRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string TerminationReason { get; set; } = string.Empty;
        public DateTime TerminationDate { get; set; }
    }

    /// <summary>
    /// Lease abandonment DTO
    /// </summary>
    public class LeaseAbandonmentDto
    {
        public string AbandonmentId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime AbandonmentDate { get; set; }
        public string AbandonmentStatus { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
    }

    /// <summary>
    /// Abandonment request DTO
    /// </summary>
    public class AbandonmentRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string AbandonmentReason { get; set; } = string.Empty;
        public List<string> RestorationPlans { get; set; } = new();
    }

    #endregion

    #region Stakeholder Management DTOs

    /// <summary>
    /// Landowner management DTO
    /// </summary>
    public class LandownerManagementDto
    {
        public string LandownerId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string LandownerName { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string ManagementStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Landowner details DTO
    /// </summary>
    public class LandownerDetailsDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string LandownerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Working interest management DTO
    /// </summary>
    public class WorkingInterestManagementDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<WorkingInterestPartnerDto> Partners { get; set; } = new();
        public decimal TotalWorkingInterest { get; set; }
    }

    /// <summary>
    /// Working interest partner DTO
    /// </summary>
    public class WorkingInterestPartnerDto
    {
        public string PartnerName { get; set; } = string.Empty;
        public decimal InterestPercentage { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Working interest request DTO
    /// </summary>
    public class WorkingInterestRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<WorkingInterestPartnerDto> Partners { get; set; } = new();
    }

    /// <summary>
    /// Royalty owner management DTO
    /// </summary>
    public class RoyaltyOwnerManagementDto
    {
        public string RoyaltyOwnerId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal RoyaltyInterest { get; set; }
        public string ManagementStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Royalty owner details DTO
    /// </summary>
    public class RoyaltyOwnerDetailsDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal RoyaltyInterest { get; set; }
        public string PaymentAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// Stakeholder communication DTO
    /// </summary>
    public class StakeholderCommunicationDto
    {
        public string CommunicationId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string StakeholderId { get; set; } = string.Empty;
        public DateTime CommunicationDate { get; set; }
        public string CommunicationType { get; set; } = string.Empty;
        public string CommunicationSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Communication log DTO
    /// </summary>
    public class CommunicationLogDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string StakeholderId { get; set; } = string.Empty;
        public DateTime CommunicationDate { get; set; }
        public string CommunicationType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Stakeholder agreement DTO
    /// </summary>
    public class StakeholderAgreementDto
    {
        public string AgreementId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public List<string> Parties { get; set; } = new();
        public DateTime AgreementDate { get; set; }
        public string AgreementStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Agreement details DTO
    /// </summary>
    public class AgreementDetailsDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<string> Parties { get; set; } = new();
        public string AgreementType { get; set; } = string.Empty;
        public string Terms { get; set; } = string.Empty;
    }

    #endregion

    #region Financial Management DTOs

    /// <summary>
    /// Budget management DTO
    /// </summary>
    public class BudgetManagementDto
    {
        public string BudgetId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string BudgetStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Budget request DTO
    /// </summary>
    public class BudgetRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public List<BudgetLineItemDto> LineItems { get; set; } = new();
    }

    /// <summary>
    /// Budget line item DTO
    /// </summary>
    public class BudgetLineItemDto
    {
        public string ItemDescription { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
    }

    /// <summary>
    /// Cost tracking DTO
    /// </summary>
    public class CostTrackingDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public DateTime DateIncurred { get; set; }
        public decimal Amount { get; set; }
        public string CostCategory { get; set; } = string.Empty;
        public string CostDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cost entry DTO
    /// </summary>
    public class CostEntryDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lease payment DTO
    /// </summary>
    public class LeasePaymentDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payment details DTO
    /// </summary>
    public class PaymentDetailsDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }

    /// <summary>
    /// Lease value metrics DTO
    /// </summary>
    public class LeaseValueMetricsDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal EstimatedValue { get; set; }
        public decimal CostPerAcre { get; set; }
        public decimal ValuePerAcre { get; set; }
        public string ValueAssessment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reserve requirement DTO
    /// </summary>
    public class ReserveRequirementDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal RequiredReserve { get; set; }
        public decimal CurrentReserve { get; set; }
        public string ReserveStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reserve request DTO
    /// </summary>
    public class ReserveRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal RequiredAmount { get; set; }
        public string ReserveType { get; set; } = string.Empty;
    }

    #endregion

    #region Due Diligence DTOs

    /// <summary>
    /// Title examination DTO
    /// </summary>
    public class TitleExaminationDto
    {
        public string ExaminationId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string TitleStatus { get; set; } = string.Empty;
        public List<TitleIssueDto> Issues { get; set; } = new();
        public string OverallAssessment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Title issue DTO
    /// </summary>
    public class TitleIssueDto
    {
        public string IssueName { get; set; } = string.Empty;
        public string IssueSeverity { get; set; } = string.Empty;
        public string ResolutionRequired { get; set; } = string.Empty;
    }

    /// <summary>
    /// Title request DTO
    /// </summary>
    public class TitleRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public bool IncludeHistoricalSearch { get; set; } = true;
    }

    /// <summary>
    /// Environmental assessment DTO
    /// </summary>
    public class EnvironmentalAssessmentDto
    {
        public string AssessmentId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string EnvironmentalStatus { get; set; } = string.Empty;
        public List<EnvironmentalIssueDto> IdentifiedIssues { get; set; } = new();
        public string OverallRisk { get; set; } = string.Empty;
    }

    /// <summary>
    /// Environmental issue DTO
    /// </summary>
    public class EnvironmentalIssueDto
    {
        public string IssueType { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Assessment request DTO
    /// </summary>
    public class AssessmentRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<string> AreasToAssess { get; set; } = new();
    }

    /// <summary>
    /// Regulatory review DTO
    /// </summary>
    public class RegulatoryReviewDto
    {
        public string ReviewId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public List<RegulatoryFindingDto> Findings { get; set; } = new();
        public string OverallComplianceStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Regulatory finding DTO
    /// </summary>
    public class RegulatoryFindingDto
    {
        public string FindingDescription { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public string RequiredAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Regulation review request DTO
    /// </summary>
    public class RegulationReviewRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<string> JurisdictionsToReview { get; set; } = new();
    }

    /// <summary>
    /// Geological evaluation DTO
    /// </summary>
    public class GeologicalEvaluationDto
    {
        public string EvaluationId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string FormationName { get; set; } = string.Empty;
        public decimal EstimatedResources { get; set; }
        public string ResourceQuality { get; set; } = string.Empty;
        public string OverallPotential { get; set; } = string.Empty;
    }

    /// <summary>
    /// Geological request DTO
    /// </summary>
    public class GeologicalRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<string> FormationsToEvaluate { get; set; } = new();
    }

    /// <summary>
    /// Engineering feasibility DTO
    /// </summary>
    public class EngineeringFeasibilityDto
    {
        public string FeasibilityId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public bool IsFeasible { get; set; }
        public decimal FeasibilityScore { get; set; }
        public List<EngineeringConstraintDto> Constraints { get; set; } = new();
    }

    /// <summary>
    /// Engineering constraint DTO
    /// </summary>
    public class EngineeringConstraintDto
    {
        public string ConstraintType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MitigationStrategy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Engineering request DTO
    /// </summary>
    public class EngineeringRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public List<string> FeasibilityAspects { get; set; } = new();
    }

    #endregion

    #region Data Management and Reporting DTOs

    /// <summary>
    /// Lease history DTO
    /// </summary>
    public class LeaseHistoryDto
    {
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lease acquisition report DTO
    /// </summary>
    public class LeaseAcquisitionReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public byte[] ReportContent { get; set; } = Array.Empty<byte>();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public List<string> KeyFindings { get; set; } = new();
    }

    /// <summary>
    /// Portfolio analysis report DTO
    /// </summary>
    public class PortfolioAnalysisReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public int TotalLeases { get; set; }
        public decimal TotalAcreage { get; set; }
        public decimal PortfolioValue { get; set; }
        public string PortfolioHealth { get; set; } = string.Empty;
    }

    /// <summary>
    /// Report request DTO
    /// </summary>
    public class ReportRequestDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Comprehensive";
        public List<string> IncludeSections { get; set; } = new();
        public string Format { get; set; } = "PDF";
    }

    /// <summary>
    /// Portfolio request DTO
    /// </summary>
    public class PortfolioRequestDto
    {
        public int? MinAcreage { get; set; }
        public int? MaxAcreage { get; set; }
        public string? Status { get; set; }
        public List<string> IncludeLeaseIds { get; set; } = new();
    }

    #endregion
}
