using System;
using System.Collections.Generic;

namespace Beep.OilandGas.LeaseAcquisition.Data.Lease.Projections;

#region Lease Acquisition DTOs

/// <summary>
/// Lease acquisition projection (advanced workflow). Renamed to avoid clashing with root namespace <c>Beep.OilandGas.LeaseAcquisition</c>.
/// </summary>
public class LeaseAcquisitionDetail
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
public class LeaseAcquisitionRequest
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
public class LeaseProspect
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
public class LeaseSearchCriteria
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
public class LeaseOpportunityEvaluation
{
    public string LeaseId { get; set; } = string.Empty;
    public decimal OpportunityScore { get; set; }
    public string RecommendationStatus { get; set; } = string.Empty;
    public List<EvaluationFactor> Factors { get; set; } = new();
    public string EvaluationSummary { get; set; } = string.Empty;
}

/// <summary>
/// Evaluation factor DTO
/// </summary>
public class EvaluationFactor
{
    public string FactorName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Lease evaluation request DTO
/// </summary>
public class LeaseEvaluationRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<string> EvaluationCriteria { get; set; } = new();
    public bool IncludeFinancialAnalysis { get; set; } = true;
}

/// <summary>
/// Competitive analysis DTO
/// </summary>
public class CompetitiveAnalysis
{
    public string LocationId { get; set; } = string.Empty;
    public List<CompetitorBid> CompetitorBids { get; set; } = new();
    public decimal AverageBonus { get; set; }
    public decimal AverageRoyaltyRate { get; set; }
    public string AnalysisSummary { get; set; } = string.Empty;
}

/// <summary>
/// Competitor bid DTO
/// </summary>
public class CompetitorBid
{
    public string CompetitorName { get; set; } = string.Empty;
    public decimal BonusAmount { get; set; }
    public decimal RoyaltyRate { get; set; }
    public decimal WorkingInterest { get; set; }
}

/// <summary>
/// Analysis request DTO
/// </summary>
public class AnalysisRequest
{
    public string LocationId { get; set; } = string.Empty;
    public int HistoricalYears { get; set; } = 5;
    public bool IncludeGeologicalFactors { get; set; } = true;
}

/// <summary>
/// Lease financial analysis DTO
/// </summary>
public class LeaseFinancialAnalysis
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
public class FinancialAnalysisRequest
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
public class LeaseNegotiation
{
    public string NegotiationId { get; set; } = string.Empty;
    public string LeaseId { get; set; } = string.Empty;
    public DateTime InitiationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrentPhase { get; set; } = string.Empty;
    public List<NegotiationRound> Rounds { get; set; } = new();
}

/// <summary>
/// Negotiation round DTO
/// </summary>
public class NegotiationRound
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
public class NegotiationInitiation
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
public class NegotiationProgress
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
public class LeaseTerms
{
    public string LeaseId { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal RoyaltyRate { get; set; }
    public decimal RentAmount { get; set; }
    public decimal MinimumProduction { get; set; }
    public List<TermCondition> Conditions { get; set; } = new();
}

/// <summary>
/// Term condition DTO
/// </summary>
public class TermCondition
{
    public string ConditionDescription { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string ConditionStatus { get; set; } = string.Empty;
}

/// <summary>
/// Lease terms update DTO
/// </summary>
public class LeaseTermsUpdate
{
    public string LeaseId { get; set; } = string.Empty;
    public DateTime? NewExpirationDate { get; set; }
    public decimal? NewRoyaltyRate { get; set; }
    public decimal? NewRentAmount { get; set; }
    public List<TermCondition> ConditionUpdates { get; set; } = new();
}

/// <summary>
/// Lease documentation DTO
/// </summary>
public class LeaseDocumentation
{
    public string DocumentId { get; set; } = string.Empty;
    public string LeaseId { get; set; } = string.Empty;
    public List<DocumentPackage> DocumentPackages { get; set; } = new();
    public DateTime PreparedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Document package DTO
/// </summary>
public class DocumentPackage
{
    public string PackageName { get; set; } = string.Empty;
    public List<string> Documents { get; set; } = new();
    public bool IsComplete { get; set; }
}

/// <summary>
/// Documentation request DTO
/// </summary>
public class DocumentationRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<string> RequiredDocuments { get; set; } = new();
}

/// <summary>
/// Execution status DTO
/// </summary>
public class ExecutionStatus
{
    public string LeaseId { get; set; } = string.Empty;
    public bool IsExecuted { get; set; }
    public DateTime? ExecutionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> SignatoryStatus { get; set; } = new();
}

/// <summary>
/// Execution details DTO
/// </summary>
public class ExecutionDetails
{
    public string LeaseId { get; set; } = string.Empty;
    public DateTime ExecutionDate { get; set; }
    public List<string> SigningParties { get; set; } = new();
    public string ExecutionLocation { get; set; } = string.Empty;
}

/// <summary>
/// Royalty calculation DTO
/// </summary>
public class ROYALTY_CALCULATION
{
    public string LeaseId { get; set; } = string.Empty;
    public decimal ProductionVolume { get; set; }
    public decimal RoyaltyRate { get; set; }
    public decimal CalculatedRoyalty { get; set; }
    public List<RoyaltyShare> Shares { get; set; } = new();
}

/// <summary>
/// Royalty share DTO
/// </summary>
public class RoyaltyShare
{
    public string RecipientName { get; set; } = string.Empty;
    public decimal SharePercentage { get; set; }
    public decimal ShareAmount { get; set; }
}

/// <summary>
/// Royalty request DTO
/// </summary>
public class RoyaltyRequest
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
public class MineralRightsManagement
{
    public string LeaseId { get; set; } = string.Empty;
    public List<MineralRightEntry> Rights { get; set; } = new();
    public string ManagementStatus { get; set; } = string.Empty;
}

/// <summary>
/// Mineral right projection (advanced workflow). Distinct from <see cref="Beep.OilandGas.Models.Data.Lease.MineralRight"/> table model.
/// </summary>
public class MineralRightEntry
{
    public string MineralName { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public decimal OwnershipPercentage { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Mineral rights request DTO
/// </summary>
public class MineralRightsRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<MineralRightEntry> RightsToAdd { get; set; } = new();
}

/// <summary>
/// Surface rights management DTO
/// </summary>
public class SurfaceRightsManagement
{
    public string LeaseId { get; set; } = string.Empty;
    public decimal SurfaceArea { get; set; }
    public string SurfaceOwner { get; set; } = string.Empty;
    public bool AccessGranted { get; set; }
    public List<AccessRight> AccessRights { get; set; } = new();
}

/// <summary>
/// Access right DTO
/// </summary>
public class AccessRight
{
    public string RightDescription { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public DateTime EffectiveDate { get; set; }
}

/// <summary>
/// Surface rights request DTO
/// </summary>
public class SurfaceRightsRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public string SurfaceOwner { get; set; } = string.Empty;
    public List<AccessRight> RequestedRights { get; set; } = new();
}

/// <summary>
/// Water rights management DTO
/// </summary>
public class WaterRightsManagement
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
public class WaterRightsRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public string WaterSource { get; set; } = string.Empty;
    public decimal RequestedVolume { get; set; }
}

/// <summary>
/// Environmental obligation DTO
/// </summary>
public class EnvironmentalObligation
{
    public string LeaseId { get; set; } = string.Empty;
    public List<Obligation> Obligations { get; set; } = new();
    public string OverallComplianceStatus { get; set; } = string.Empty;
}

/// <summary>
/// Obligation DTO
/// </summary>
public class Obligation
{
    public string ObligationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ComplianceStatus { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Regulatory compliance DTO
/// </summary>
public class RegulatoryCompliance
{
    public string LeaseId { get; set; } = string.Empty;
    public List<ComplianceItem> ComplianceItems { get; set; } = new();
    public string OverallComplianceStatus { get; set; } = string.Empty;
}

/// <summary>
/// Compliance item DTO
/// </summary>
public class ComplianceItem
{
    public string RequirementName { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Compliance request DTO
/// </summary>
public class ComplianceRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<string> JurisdictionsToReview { get; set; } = new();
}

/// <summary>
/// Operational obligation DTO
/// </summary>
public class OperationalObligation
{
    public string LeaseId { get; set; } = string.Empty;
    public List<OperationalItem> ObligationItems { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Operational item DTO
/// </summary>
public class OperationalItem
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
public class LeaseRenewal
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
public class RenewalRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public int RenewalTermYears { get; set; }
    public decimal ProposedBonus { get; set; }
}

/// <summary>
/// Lease extension DTO
/// </summary>
public class LeaseExtension
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
public class ExtensionRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public int ExtensionYears { get; set; }
    public decimal ExtensionCost { get; set; }
}

/// <summary>
/// Lease expiration schedule DTO
/// </summary>
public class LeaseExpirationSchedule
{
    public DateTime ReportDate { get; set; }
    public List<ExpiringLease> ExpiringLeases { get; set; } = new();
    public int TotalExpiringCount { get; set; }
}

/// <summary>
/// Expiring lease DTO
/// </summary>
public class ExpiringLease
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
public class LeaseTermination
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
public class TerminationRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public string TerminationReason { get; set; } = string.Empty;
    public DateTime TerminationDate { get; set; }
}

/// <summary>
/// Lease abandonment DTO
/// </summary>
public class LeaseAbandonment
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
public class AbandonmentRequest
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
public class LandownerManagement
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
public class LandownerDetails
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
public class WorkingInterestManagement
{
    public string LeaseId { get; set; } = string.Empty;
    public List<WorkingInterestPartner> Partners { get; set; } = new();
    public decimal TotalWorkingInterest { get; set; }
}

/// <summary>
/// Working interest partner DTO
/// </summary>
public class WorkingInterestPartner
{
    public string PartnerName { get; set; } = string.Empty;
    public decimal InterestPercentage { get; set; }
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Working interest request DTO
/// </summary>
public class WorkingInterestRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<WorkingInterestPartner> Partners { get; set; } = new();
}

/// <summary>
/// Royalty owner management DTO
/// </summary>
public class RoyaltyOwnerManagement
{
    public string RoyaltyOwnerId { get; set; } = string.Empty;
    public string LeaseId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal ROYALTY_INTEREST { get; set; }
    public string ManagementStatus { get; set; } = string.Empty;
}

/// <summary>
/// Royalty owner details DTO
/// </summary>
public class RoyaltyOwnerDetails
{
    public string LeaseId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal ROYALTY_INTEREST { get; set; }
    public string PaymentAddress { get; set; } = string.Empty;
}

/// <summary>
/// Stakeholder communication DTO
/// </summary>
public class StakeholderCommunication
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
public class CommunicationLog
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
public class StakeholderAgreement
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
public class AgreementDetails
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
public class BudgetManagement
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
public class BudgetRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
    public List<BudgetLineItem> LineItems { get; set; } = new();
}

/// <summary>
/// Budget line item DTO
/// </summary>
public class BudgetLineItem
{
    public string ItemDescription { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
}

/// <summary>
/// Cost tracking DTO
/// </summary>
public class CostTracking
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
public class CostEntry
{
    public string LeaseId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Lease payment projection for advanced workflow (distinct from <see cref="Beep.OilandGas.Models.Data.Lease.LeasePayment"/> table model).
/// </summary>
public class LeasePaymentDetail
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
public class PaymentDetails
{
    public string LeaseId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}

/// <summary>
/// Lease value metrics DTO
/// </summary>
public class LeaseValueMetrics
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
public class ReserveRequirement
{
    public string LeaseId { get; set; } = string.Empty;
    public decimal RequiredReserve { get; set; }
    public decimal CurrentReserve { get; set; }
    public string ReserveStatus { get; set; } = string.Empty;
}

/// <summary>
/// Reserve request DTO
/// </summary>
public class ReserveRequest
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
public class TitleExamination
{
    public string ExaminationId { get; set; } = string.Empty;
    public string LeaseId { get; set; } = string.Empty;
    public string TitleStatus { get; set; } = string.Empty;
    public List<TitleIssue> Issues { get; set; } = new();
    public string OverallAssessment { get; set; } = string.Empty;
}

/// <summary>
/// Title issue DTO
/// </summary>
public class TitleIssue
{
    public string IssueName { get; set; } = string.Empty;
    public string IssueSeverity { get; set; } = string.Empty;
    public string ResolutionRequired { get; set; } = string.Empty;
}

/// <summary>
/// Title request DTO
/// </summary>
public class TitleRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public bool IncludeHistoricalSearch { get; set; } = true;
}

/// <summary>
/// Environmental assessment DTO
/// </summary>
public class EnvironmentalAssessment
{
    public string AssessmentId { get; set; } = string.Empty;
    public string LeaseId { get; set; } = string.Empty;
    public string EnvironmentalStatus { get; set; } = string.Empty;
    public List<EnvironmentalIssue> IdentifiedIssues { get; set; } = new();
    public string OverallRisk { get; set; } = string.Empty;
}

/// <summary>
/// Environmental issue DTO
/// </summary>
public class EnvironmentalIssue
{
    public string IssueType { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
}

/// <summary>
/// Assessment request DTO
/// </summary>
public class AssessmentRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<string> AreasToAssess { get; set; } = new();
}

/// <summary>
/// Regulatory review DTO
/// </summary>
public class RegulatoryReview
{
    public string ReviewId { get; set; } = string.Empty;
    public string LeaseId { get; set; } = string.Empty;
    public List<RegulatoryFinding> Findings { get; set; } = new();
    public string OverallComplianceStatus { get; set; } = string.Empty;
}

/// <summary>
/// Regulatory finding DTO
/// </summary>
public class RegulatoryFinding
{
    public string FindingDescription { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public string RequiredAction { get; set; } = string.Empty;
}

/// <summary>
/// Regulation review request DTO
/// </summary>
public class RegulationReviewRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<string> JurisdictionsToReview { get; set; } = new();
}

/// <summary>
/// Geological evaluation DTO
/// </summary>
public class GeologicalEvaluation
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
public class GeologicalRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<string> FormationsToEvaluate { get; set; } = new();
}

/// <summary>
/// Engineering feasibility DTO
/// </summary>
public class EngineeringFeasibility
{
    public string FeasibilityId { get; set; } = string.Empty;
    public string LeaseId { get; set; } = string.Empty;
    public bool IsFeasible { get; set; }
    public decimal FeasibilityScore { get; set; }
    public List<EngineeringConstraint> Constraints { get; set; } = new();
}

/// <summary>
/// Engineering constraint DTO
/// </summary>
public class EngineeringConstraint
{
    public string ConstraintType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MitigationStrategy { get; set; } = string.Empty;
}

/// <summary>
/// Engineering request DTO
/// </summary>
public class EngineeringRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public List<string> FeasibilityAspects { get; set; } = new();
}

#endregion

#region Data Management and Reporting DTOs

/// <summary>
/// Lease history DTO
/// </summary>
public class LeaseHistory
{
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
}

/// <summary>
/// Lease acquisition report DTO
/// </summary>
public class LeaseAcquisitionReport
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
public class PortfolioAnalysisReport
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
public class ReportRequest
{
    public string LeaseId { get; set; } = string.Empty;
    public string ReportType { get; set; } = "Comprehensive";
    public List<string> IncludeSections { get; set; } = new();
    public string Format { get; set; } = "PDF";
}

/// <summary>
/// Portfolio request DTO
/// </summary>
public class PortfolioRequest
{
    public int? MinAcreage { get; set; }
    public int? MaxAcreage { get; set; }
    public string? Status { get; set; }
    public List<string> IncludeLeaseIds { get; set; } = new();
}

#endregion