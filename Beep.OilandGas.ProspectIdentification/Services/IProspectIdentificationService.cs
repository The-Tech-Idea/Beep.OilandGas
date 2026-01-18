using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    /// <summary>
    /// Comprehensive prospect identification service interface
    /// Orchestrates the complete prospect identification workflow from data acquisition to final evaluation
    /// </summary>
    public interface IProspectIdentificationService
    {
        #region Workflow Management

        /// <summary>
        /// Initiates a new prospect identification project
        /// </summary>
        Task<ProspectIdentificationProjectDto> CreateProjectAsync(CreateProjectDto createDto, string userId);

        /// <summary>
        /// Gets project status and progress
        /// </summary>
        Task<ProspectIdentificationProjectDto> GetProjectAsync(string projectId);

        /// <summary>
        /// Updates project status
        /// </summary>
        Task<ProspectIdentificationProjectDto> UpdateProjectStatusAsync(string projectId, ProjectStatus newStatus, string userId);

        /// <summary>
        /// Gets all projects with filtering
        /// </summary>
        Task<List<ProspectIdentificationProjectDto>> GetProjectsAsync(string? basinId = null, ProjectStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);

        #endregion

        #region Data Acquisition & Integration

        /// <summary>
        /// Imports seismic data into the project
        /// </summary>
        Task<SeismicDataImportDto> ImportSeismicDataAsync(string projectId, SeismicDataImportRequestDto request, string userId);

        /// <summary>
        /// Imports well data for calibration
        /// </summary>
        Task<WellDataImportDto> ImportWellDataAsync(string projectId, WellDataImportRequestDto request, string userId);

        /// <summary>
        /// Integrates multiple data sources
        /// </summary>
        Task<DataIntegrationDto> IntegrateDataSourcesAsync(string projectId, DataIntegrationRequestDto request, string userId);

        /// <summary>
        /// Performs quality control on imported data
        /// </summary>
        Task<DataQualityDto> PerformDataQualityControlAsync(string projectId, DataQualityRequestDto request);

        #endregion

        #region Prospect Generation

        /// <summary>
        /// Generates prospects from seismic interpretation
        /// </summary>
        Task<List<GeneratedProspectDto>> GenerateProspectsFromSeismicAsync(string projectId, SeismicProspectGenerationRequestDto request, string userId);

        /// <summary>
        /// Generates prospects from geological analysis
        /// </summary>
        Task<List<GeneratedProspectDto>> GenerateProspectsFromGeologyAsync(string projectId, GeologicalProspectGenerationRequestDto request, string userId);

        /// <summary>
        /// Generates prospects from play-based analysis
        /// </summary>
        Task<List<GeneratedProspectDto>> GenerateProspectsFromPlayAsync(string projectId, PlayBasedProspectGenerationRequestDto request, string userId);

        /// <summary>
        /// Merges duplicate or overlapping prospects
        /// </summary>
        Task<List<GeneratedProspectDto>> MergeProspectsAsync(string projectId, ProspectMergeRequestDto request, string userId);

        #endregion

        #region Prospect Evaluation Pipeline

        /// <summary>
        /// Runs complete evaluation pipeline on prospects
        /// </summary>
        Task<EvaluationPipelineResultDto> RunEvaluationPipelineAsync(string projectId, EvaluationPipelineRequestDto request, string userId);

        /// <summary>
        /// Performs rapid screening of generated prospects
        /// </summary>
        Task<RapidScreeningResultDto> PerformRapidScreeningAsync(string projectId, RapidScreeningRequestDto request);

        /// <summary>
        /// Performs detailed evaluation of selected prospects
        /// </summary>
        Task<DetailedEvaluationResultDto> PerformDetailedEvaluationAsync(string projectId, DetailedEvaluationRequestDto request, string userId);

        /// <summary>
        /// Updates prospect evaluation status
        /// </summary>
        Task UpdateProspectEvaluationStatusAsync(string projectId, string prospectId, EvaluationStatus newStatus, string userId);

        #endregion

        #region Portfolio Management

        /// <summary>
        /// Creates prospect portfolio for the project
        /// </summary>
        Task<ProspectPortfolioDto> CreatePortfolioAsync(string projectId, PortfolioCreationRequestDto request, string userId);

        /// <summary>
        /// Optimizes prospect portfolio
        /// </summary>
        Task<PortfolioOptimizationDto> OptimizePortfolioAsync(string projectId, PortfolioOptimizationRequestDto request, string userId);

        /// <summary>
        /// Performs portfolio risk analysis
        /// </summary>
        Task<PortfolioRiskAnalysisDto> AnalyzePortfolioRiskAsync(string projectId, PortfolioRiskRequestDto request);

        /// <summary>
        /// Generates drilling schedule recommendations
        /// </summary>
        Task<DrillingScheduleDto> GenerateDrillingScheduleAsync(string projectId, DrillingScheduleRequestDto request);

        #endregion

        #region Collaboration & Review

        /// <summary>
        /// Assigns prospects to evaluators
        /// </summary>
        Task AssignProspectsAsync(string projectId, ProspectAssignmentRequestDto request, string userId);

        /// <summary>
        /// Gets evaluation progress and status
        /// </summary>
        Task<EvaluationProgressDto> GetEvaluationProgressAsync(string projectId);

        /// <summary>
        /// Performs quality assurance review
        /// </summary>
        Task<QualityAssuranceDto> PerformQualityAssuranceAsync(string projectId, QualityAssuranceRequestDto request, string userId);

        /// <summary>
        /// Escalates prospects requiring senior review
        /// </summary>
        Task EscalateProspectsAsync(string projectId, ProspectEscalationRequestDto request, string userId);

        #endregion

        #region Reporting & Analytics

        /// <summary>
        /// Generates project status report
        /// </summary>
        Task<ProjectReportDto> GenerateProjectReportAsync(string projectId, ProjectReportRequestDto request);

        /// <summary>
        /// Generates prospect inventory report
        /// </summary>
        Task<ProspectInventoryDto> GenerateProspectInventoryAsync(string projectId, ProspectInventoryRequestDto request);

        /// <summary>
        /// Performs project analytics
        /// </summary>
        Task<ProjectAnalyticsDto> PerformProjectAnalyticsAsync(string projectId, ProjectAnalyticsRequestDto request);

        /// <summary>
        /// Exports project data
        /// </summary>
        Task<byte[]> ExportProjectDataAsync(string projectId, ExportRequestDto request);

        #endregion

        #region Decision Support

        /// <summary>
        /// Provides decision recommendations
        /// </summary>
        Task<DecisionRecommendationDto> GetDecisionRecommendationsAsync(string projectId, DecisionRequestDto request);

        /// <summary>
        /// Performs scenario analysis
        /// </summary>
        Task<ScenarioAnalysisDto> PerformScenarioAnalysisAsync(string projectId, ScenarioAnalysisRequestDto request);

        /// <summary>
        /// Calculates value of information
        /// </summary>
        Task<ValueOfInformationDto> CalculateValueOfInformationAsync(string projectId, ValueOfInformationRequestDto request);

        /// <summary>
        /// Generates management presentation
        /// </summary>
        Task<ManagementPresentationDto> GenerateManagementPresentationAsync(string projectId, PresentationRequestDto request);

        #endregion
    }

    #region Project Management DTOs

    /// <summary>
    /// Prospect identification project DTO
    /// </summary>
    public class ProspectIdentificationProjectDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string BasinId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProjectManager { get; set; } = string.Empty;
        public List<string> TeamMembers { get; set; } = new();
        public decimal Budget { get; set; }
        public string BudgetCurrency { get; set; } = string.Empty;
        public ProjectMetricsDto Metrics { get; set; } = new();
        public List<ProjectMilestoneDto> Milestones { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Project status enumeration
    /// </summary>
    public enum ProjectStatus
    {
        Planning,
        DataAcquisition,
        ProspectGeneration,
        Evaluation,
        PortfolioOptimization,
        DecisionMaking,
        Completed,
        OnHold,
        Cancelled
    }

    /// <summary>
    /// Create project DTO
    /// </summary>
    public class CreateProjectDto
    {
        public string ProjectName { get; set; } = string.Empty;
        public string BasinId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProjectManager { get; set; } = string.Empty;
        public List<string> TeamMembers { get; set; } = new();
        public decimal Budget { get; set; }
        public string BudgetCurrency { get; set; } = string.Empty;
        public List<ProjectMilestoneDto> Milestones { get; set; } = new();
    }

    /// <summary>
    /// Project metrics DTO
    /// </summary>
    public class ProjectMetricsDto
    {
        public int TotalProspects { get; set; }
        public int EvaluatedProspects { get; set; }
        public int DrilledProspects { get; set; }
        public int SuccessfulWells { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal TotalExpectedVolume { get; set; }
        public decimal TotalActualVolume { get; set; }
        public decimal BudgetUtilization { get; set; }
        public int DaysToComplete { get; set; }
    }

    /// <summary>
    /// Project milestone DTO
    /// </summary>
    public class ProjectMilestoneDto
    {
        public string MilestoneId { get; set; } = string.Empty;
        public string MilestoneName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime TargetDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public bool IsCompleted { get; set; }
        public decimal ProgressPercentage { get; set; }
    }

    #endregion

    #region Data Management DTOs

    /// <summary>
    /// Seismic data import DTO
    /// </summary>
    public class SeismicDataImportDto
    {
        public string ImportId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public List<SeismicSurveyDto> ImportedSurveys { get; set; } = new();
        public int TotalFiles { get; set; }
        public long TotalSizeBytes { get; set; }
        public string ImportStatus { get; set; } = string.Empty;
        public List<string> ImportErrors { get; set; } = new();
    }

    /// <summary>
    /// Well data import DTO
    /// </summary>
    public class WellDataImportDto
    {
        public string ImportId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public List<WellDto> ImportedWells { get; set; } = new();
        public int TotalWells { get; set; }
        public int TotalLogs { get; set; }
        public string ImportStatus { get; set; } = string.Empty;
        public List<string> ImportErrors { get; set; } = new();
    }

    /// <summary>
    /// Well DTO
    /// </summary>
    public class WellDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal TotalDepth { get; set; }
        public string WellType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data integration DTO
    /// </summary>
    public class DataIntegrationDto
    {
        public string IntegrationId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime IntegrationDate { get; set; }
        public List<string> IntegratedDataTypes { get; set; } = new();
        public int TotalRecords { get; set; }
        public int MatchedRecords { get; set; }
        public int UnmatchedRecords { get; set; }
        public string IntegrationStatus { get; set; } = string.Empty;
        public List<string> IntegrationIssues { get; set; } = new();
    }

    /// <summary>
    /// Data quality DTO
    /// </summary>
    public class DataQualityDto
    {
        public string QualityCheckId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime CheckDate { get; set; }
        public decimal OverallQualityScore { get; set; }
        public Dictionary<string, decimal> QualityByDataType { get; set; } = new();
        public List<DataQualityIssueDto> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Data quality issue DTO
    /// </summary>
    public class DataQualityIssueDto
    {
        public string IssueId { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int AffectedRecords { get; set; }
    }

    #endregion

    #region Prospect Generation DTOs

    /// <summary>
    /// Generated prospect DTO
    /// </summary>
    public class GeneratedProspectDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string GenerationMethod { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? TargetDepth { get; set; }
        public string TrapType { get; set; } = string.Empty;
        public string ReservoirType { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string SealType { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public List<string> SupportingEvidence { get; set; } = new();
        public DateTime GeneratedDate { get; set; }
        public EvaluationStatus EvaluationStatus { get; set; }
    }

    /// <summary>
    /// Evaluation status enumeration
    /// </summary>
    public enum EvaluationStatus
    {
        Generated,
        Screening,
        DetailedEvaluation,
        Accepted,
        Rejected,
        Drilled,
        Success,
        Failure
    }

    #endregion

    #region Evaluation DTOs

    /// <summary>
    /// Evaluation pipeline result DTO
    /// </summary>
    public class EvaluationPipelineResultDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public PipelineStageResultDto ScreeningStage { get; set; } = new();
        public PipelineStageResultDto DetailedEvaluationStage { get; set; } = new();
        public PipelineStageResultDto FinalRankingStage { get; set; } = new();
        public int TotalProspectsProcessed { get; set; }
        public int ProspectsAdvanced { get; set; }
        public string PipelineStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pipeline stage result DTO
    /// </summary>
    public class PipelineStageResultDto
    {
        public string StageName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProspectsProcessed { get; set; }
        public int ProspectsPassed { get; set; }
        public int ProspectsFailed { get; set; }
        public string StageStatus { get; set; } = string.Empty;
        public List<string> StageIssues { get; set; } = new();
    }

    /// <summary>
    /// Rapid screening result DTO
    /// </summary>
    public class RapidScreeningResultDto
    {
        public string ScreeningId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ScreeningDate { get; set; }
        public List<ScreeningResultDto> Results { get; set; } = new();
        public int TotalProspects { get; set; }
        public int PassedScreening { get; set; }
        public int FailedScreening { get; set; }
        public string ScreeningCriteria { get; set; } = string.Empty;
    }

    /// <summary>
    /// Screening result DTO
    /// </summary>
    public class ScreeningResultDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public bool PassedScreening { get; set; }
        public decimal Score { get; set; }
        public List<string> PassedCriteria { get; set; } = new();
        public List<string> FailedCriteria { get; set; } = new();
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Detailed evaluation result DTO
    /// </summary>
    public class DetailedEvaluationResultDto
    {
        public string EvaluationId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public List<DetailedProspectEvaluationDto> Evaluations { get; set; } = new();
        public string EvaluationMethodology { get; set; } = string.Empty;
        public List<string> KeyFindings { get; set; } = new();
    }

    /// <summary>
    /// Detailed prospect evaluation DTO
    /// </summary>
    public class DetailedProspectEvaluationDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public VolumetricAnalysisDto VolumetricAnalysis { get; set; } = new();
        public RiskAssessmentDto RiskAssessment { get; set; } = new();
        public EconomicEvaluationDto EconomicEvaluation { get; set; } = new();
        public decimal OverallScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    #endregion

    #region Portfolio Management DTOs

    /// <summary>
    /// Prospect portfolio DTO
    /// </summary>
    public class ProspectPortfolioDto
    {
        public string PortfolioId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string PortfolioName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public List<PortfolioProspectDto> Prospects { get; set; } = new();
        public PortfolioSummaryDto Summary { get; set; } = new();
        public string OptimizationStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Portfolio prospect DTO
    /// </summary>
    public class PortfolioProspectDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public decimal ExpectedVolume { get; set; }
        public decimal SuccessProbability { get; set; }
        public decimal NPV { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string DrillingSequence { get; set; } = string.Empty;
    }



    /// <summary>
    /// Portfolio optimization DTO
    /// </summary>
    public class PortfolioOptimizationDto
    {
        public string OptimizationId { get; set; } = string.Empty;
        public string PortfolioId { get; set; } = string.Empty;
        public DateTime OptimizationDate { get; set; }
        public string OptimizationMethod { get; set; } = string.Empty;
        public List<OptimizationScenarioDto> Scenarios { get; set; } = new();
        public OptimizationScenarioDto RecommendedScenario { get; set; } = new();
        public string OptimizationSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimization scenario DTO
    /// </summary>
    public class OptimizationScenarioDto
    {
        public string ScenarioId { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public decimal ExpectedValue { get; set; }
        public decimal Risk { get; set; }
        public decimal Efficiency { get; set; }
        public List<string> SelectedProspects { get; set; } = new();
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Portfolio risk analysis DTO
    /// </summary>
    public class PortfolioRiskAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PortfolioId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal OverallRisk { get; set; }
        public decimal ValueAtRisk { get; set; }
        public decimal ExpectedShortfall { get; set; }
        public List<RiskFactorDto> KeyRiskFactors { get; set; } = new();
        public byte[] RiskChart { get; set; } = Array.Empty<byte>();
        public List<string> RiskMitigationStrategies { get; set; } = new();
    }

    /// <summary>
    /// Drilling schedule DTO
    /// </summary>
    public class DrillingScheduleDto
    {
        public string ScheduleId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<DrillingSlotDto> DrillingSlots { get; set; } = new();
        public decimal TotalCost { get; set; }
        public decimal TotalValue { get; set; }
        public string ScheduleOptimization { get; set; } = string.Empty;
    }

    /// <summary>
    /// Drilling slot DTO
    /// </summary>
    public class DrillingSlotDto
    {
        public string SlotId { get; set; } = string.Empty;
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal ExpectedValue { get; set; }
        public string RigType { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    #endregion

    #region Request DTOs

    /// <summary>
    /// Seismic data import request DTO
    /// </summary>
    public class SeismicDataImportRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> FilePaths { get; set; } = new();
        public string DataFormat { get; set; } = "SEG-Y";
        public bool ValidateData { get; set; } = true;
        public bool CreatePreStackGathers { get; set; } = false;
        public string ProcessingLevel { get; set; } = "Raw";
    }

    /// <summary>
    /// Well data import request DTO
    /// </summary>
    public class WellDataImportRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> WellFiles { get; set; } = new();
        public List<string> LogFiles { get; set; } = new();
        public string DataFormat { get; set; } = "LAS";
        public bool ValidateData { get; set; } = true;
        public bool CreateSyntheticSeismograms { get; set; } = false;
    }

    /// <summary>
    /// Data integration request DTO
    /// </summary>
    public class DataIntegrationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> DataSources { get; set; } = new();
        public string IntegrationMethod { get; set; } = "Automatic";
        public bool ResolveConflicts { get; set; } = true;
        public List<string> KeyFields { get; set; } = new();
    }

    /// <summary>
    /// Data quality request DTO
    /// </summary>
    public class DataQualityRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> DataTypes { get; set; } = new();
        public List<string> QualityChecks { get; set; } = new();
        public decimal QualityThreshold { get; set; } = 0.8m;
        public bool GenerateReport { get; set; } = true;
    }

    /// <summary>
    /// Seismic prospect generation request DTO
    /// </summary>
    public class SeismicProspectGenerationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> SurveyIds { get; set; } = new();
        public string DetectionMethod { get; set; } = "AttributeAnalysis";
        public decimal MinConfidence { get; set; } = 0.6m;
        public List<string> TargetTrapTypes { get; set; } = new();
        public decimal MinAmplitude { get; set; }
    }

    /// <summary>
    /// Geological prospect generation request DTO
    /// </summary>
    public class GeologicalProspectGenerationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> PlayIds { get; set; } = new();
        public string GenerationMethod { get; set; } = "PlayBased";
        public List<string> ReservoirTypes { get; set; } = new();
        public List<string> TrapTypes { get; set; } = new();
        public decimal MinVolume { get; set; }
    }

    /// <summary>
    /// Play-based prospect generation request DTO
    /// </summary>
    public class PlayBasedProspectGenerationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string PlayId { get; set; } = string.Empty;
        public string GenerationStrategy { get; set; } = "AnalogBased";
        public int MaxProspects { get; set; } = 50;
        public List<string> Constraints { get; set; } = new();
    }

    /// <summary>
    /// Prospect merge request DTO
    /// </summary>
    public class ProspectMergeRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
        public decimal MergeDistance { get; set; } = 1000; // meters
        public string MergeStrategy { get; set; } = "BestConfidence";
        public bool PreserveHighConfidence { get; set; } = true;
    }

    /// <summary>
    /// Evaluation pipeline request DTO
    /// </summary>
    public class EvaluationPipelineRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
        public bool RunRapidScreening { get; set; } = true;
        public bool RunDetailedEvaluation { get; set; } = true;
        public bool RunRanking { get; set; } = true;
        public string EvaluationMethodology { get; set; } = "Standard";
        public decimal ScreeningThreshold { get; set; } = 0.5m;
    }

    /// <summary>
    /// Rapid screening request DTO
    /// </summary>
    public class RapidScreeningRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
        public List<string> ScreeningCriteria { get; set; } = new();
        public Dictionary<string, decimal> CriteriaWeights { get; set; } = new();
        public decimal PassThreshold { get; set; } = 0.6m;
    }

    /// <summary>
    /// Detailed evaluation request DTO
    /// </summary>
    public class DetailedEvaluationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
        public bool IncludeVolumetric { get; set; } = true;
        public bool IncludeRisk { get; set; } = true;
        public bool IncludeEconomic { get; set; } = true;
        public string EvaluationDepth { get; set; } = "Comprehensive";
    }

    /// <summary>
    /// Portfolio creation request DTO
    /// </summary>
    public class PortfolioCreationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
        public string SelectionCriteria { get; set; } = "TopRanked";
        public int MaxProspects { get; set; } = 20;
    }

    /// <summary>
    /// Portfolio optimization request DTO
    /// </summary>
    public class PortfolioOptimizationRequestDto
    {
        public string PortfolioId { get; set; } = string.Empty;
        public string OptimizationObjective { get; set; } = "MaximizeValue";
        public List<string> Constraints { get; set; } = new();
        public int MaxIterations { get; set; } = 1000;
        public string Algorithm { get; set; } = "GeneticAlgorithm";
    }

    /// <summary>
    /// Portfolio risk request DTO
    /// </summary>
    public class PortfolioRiskRequestDto
    {
        public string PortfolioId { get; set; } = string.Empty;
        public decimal ConfidenceLevel { get; set; } = 0.95m;
        public string RiskMeasure { get; set; } = "VaR";
        public int SimulationRuns { get; set; } = 10000;
    }

    /// <summary>
    /// Drilling schedule request DTO
    /// </summary>
    public class DrillingScheduleRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string PortfolioId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public List<string> AvailableRigs { get; set; } = new();
        public Dictionary<string, decimal> RigCosts { get; set; } = new();
        public string OptimizationCriteria { get; set; } = "NPV";
    }

    /// <summary>
    /// Prospect assignment request DTO
    /// </summary>
    public class ProspectAssignmentRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public Dictionary<string, List<string>> Assignments { get; set; } = new(); // Evaluator -> ProspectIds
        public DateTime Deadline { get; set; }
        public string AssignmentType { get; set; } = "Evaluation";
    }

    /// <summary>
    /// Evaluation progress DTO
    /// </summary>
    public class EvaluationProgressDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public int TotalProspects { get; set; }
        public int CompletedEvaluations { get; set; }
        public int InProgressEvaluations { get; set; }
        public int PendingEvaluations { get; set; }
        public decimal OverallProgress { get; set; }
        public List<EvaluatorProgressDto> EvaluatorProgress { get; set; } = new();
        public List<string> Bottlenecks { get; set; } = new();
    }

    /// <summary>
    /// Evaluator progress DTO
    /// </summary>
    public class EvaluatorProgressDto
    {
        public string EvaluatorId { get; set; } = string.Empty;
        public string EvaluatorName { get; set; } = string.Empty;
        public int AssignedProspects { get; set; }
        public int CompletedProspects { get; set; }
        public int InProgressProspects { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime? NextDeadline { get; set; }
    }

    /// <summary>
    /// Quality assurance DTO
    /// </summary>
    public class QualityAssuranceDto
    {
        public string QAId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public string Reviewer { get; set; } = string.Empty;
        public List<QAReviewDto> Reviews { get; set; } = new();
        public decimal OverallQualityScore { get; set; }
        public List<string> QARecommendations { get; set; } = new();
    }

    /// <summary>
    /// QA review DTO
    /// </summary>
    public class QAReviewDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public string ReviewStatus { get; set; } = string.Empty;
        public List<string> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public bool RequiresRevision { get; set; }
    }

    /// <summary>
    /// Prospect escalation request DTO
    /// </summary>
    public class ProspectEscalationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
        public string EscalationReason { get; set; } = string.Empty;
        public string EscalationLevel { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
    }

    /// <summary>
    /// Project report DTO
    /// </summary>
    public class ProjectReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public byte[] ReportContent { get; set; } = Array.Empty<byte>();
        public List<byte[]> Charts { get; set; } = new();
        public ProjectMetricsDto Metrics { get; set; } = new();
        public string ExecutiveSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Prospect inventory DTO
    /// </summary>
    public class ProspectInventoryDto
    {
        public string InventoryId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<ProspectInventoryItemDto> Prospects { get; set; } = new();
        public InventorySummaryDto Summary { get; set; } = new();
        public byte[] InventoryMap { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// Prospect inventory item DTO
    /// </summary>
    public class ProspectInventoryItemDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public EvaluationStatus Status { get; set; }
        public decimal ExpectedVolume { get; set; }
        public decimal SuccessProbability { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Inventory summary DTO
    /// </summary>
    public class InventorySummaryDto
    {
        public int TotalProspects { get; set; }
        public int ByStatus { get; set; } // This should be a dictionary
        public decimal TotalExpectedVolume { get; set; }
        public decimal AverageSuccessProbability { get; set; }
        public Dictionary<string, int> ProspectsByStatus { get; set; } = new();
        public Dictionary<string, decimal> VolumeByStatus { get; set; } = new();
    }

    /// <summary>
    /// Project analytics DTO
    /// </summary>
    public class ProjectAnalyticsDto
    {
        public string AnalyticsId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<TrendAnalysisDto> Trends { get; set; } = new();
        public List<PerformanceMetricDto> Metrics { get; set; } = new();
        public List<PredictiveInsightDto> Insights { get; set; } = new();
        public byte[] AnalyticsDashboard { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// Trend analysis DTO
    /// </summary>
    public class TrendAnalysisDto
    {
        public string TrendName { get; set; } = string.Empty;
        public string TrendType { get; set; } = string.Empty;
        public decimal TrendValue { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public string Analysis { get; set; } = string.Empty;
    }

    /// <summary>
    /// Performance metric DTO
    /// </summary>
    public class PerformanceMetricDto
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public decimal Variance { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Trend { get; set; } = string.Empty;
    }

    /// <summary>
    /// Predictive insight DTO
    /// </summary>
    public class PredictiveInsightDto
    {
        public string InsightType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public List<string> SupportingData { get; set; } = new();
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Export request DTO
    /// </summary>
    public class ExportRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> DataTypes { get; set; } = new();
        public string Format { get; set; } = "Excel";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Filters { get; set; } = new();
    }

    /// <summary>
    /// Decision recommendation DTO
    /// </summary>
    public class DecisionRecommendationDto
    {
        public string RecommendationId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string DecisionType { get; set; } = string.Empty;
        public List<RecommendationOptionDto> Options { get; set; } = new();
        public RecommendationOptionDto RecommendedOption { get; set; } = new();
        public string Rationale { get; set; } = string.Empty;
        public List<string> Assumptions { get; set; } = new();
        public List<string> Risks { get; set; } = new();
    }

    /// <summary>
    /// Recommendation option DTO
    /// </summary>
    public class RecommendationOptionDto
    {
        public string OptionId { get; set; } = string.Empty;
        public string OptionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ExpectedValue { get; set; }
        public decimal Risk { get; set; }
        public decimal ProbabilityOfSuccess { get; set; }
        public List<string> Pros { get; set; } = new();
        public List<string> Cons { get; set; } = new();
    }

    /// <summary>
    /// Scenario analysis DTO
    /// </summary>
    public class ScenarioAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<AnalysisScenarioDto> Scenarios { get; set; } = new();
        public AnalysisScenarioDto BaseCase { get; set; } = new();
        public List<ScenarioComparisonDto> Comparisons { get; set; } = new();
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Analysis scenario DTO
    /// </summary>
    public class AnalysisScenarioDto
    {
        public string ScenarioId { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public Dictionary<string, decimal> Parameters { get; set; } = new();
        public decimal NPV { get; set; }
        public decimal IRR { get; set; }
        public decimal Risk { get; set; }
        public string Feasibility { get; set; } = string.Empty;
    }

    /// <summary>
    /// Scenario comparison DTO
    /// </summary>
    public class ScenarioComparisonDto
    {
        public string ComparisonId { get; set; } = string.Empty;
        public List<string> ScenarioIds { get; set; } = new();
        public Dictionary<string, decimal> ValueDifferences { get; set; } = new();
        public Dictionary<string, decimal> RiskDifferences { get; set; } = new();
        public string BestScenario { get; set; } = string.Empty;
        public string ComparisonSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Value of information DTO
    /// </summary>
    public class ValueOfInformationDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<InformationValueDto> InformationValues { get; set; } = new();
        public InformationValueDto HighestValueInformation { get; set; } = new();
        public decimal TotalVOI { get; set; }
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Information value DTO
    /// </summary>
    public class InformationValueDto
    {
        public string InformationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal Value { get; set; }
        public decimal NetValue { get; set; }
        public decimal ProbabilityOfSuccess { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    /// <summary>
    /// Management presentation DTO
    /// </summary>
    public class ManagementPresentationDto
    {
        public string PresentationId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string PresentationType { get; set; } = string.Empty;
        public byte[] PresentationContent { get; set; } = Array.Empty<byte>();
        public List<byte[]> Charts { get; set; } = new();
        public List<byte[]> Maps { get; set; } = new();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public List<string> KeyMessages { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Presentation request DTO
    /// </summary>
    public class PresentationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string PresentationType { get; set; } = "ExecutiveSummary";
        public List<string> IncludeSections { get; set; } = new();
        public bool IncludeFinancials { get; set; } = true;
        public bool IncludeRiskAnalysis { get; set; } = true;
        public string Audience { get; set; } = "Management";
    }

    /// <summary>
    /// Quality assurance request DTO
    /// </summary>
    public class QualityAssuranceRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
        public string QAStage { get; set; } = "Evaluation";
        public List<string> QACriteria { get; set; } = new();
        public string Reviewer { get; set; } = string.Empty;
    }

    /// <summary>
    /// Project report request DTO
    /// </summary>
    public class ProjectReportRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Status";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> IncludeMetrics { get; set; } = new();
        public bool IncludeCharts { get; set; } = true;
    }

    /// <summary>
    /// Prospect inventory request DTO
    /// </summary>
    public class ProspectInventoryRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<EvaluationStatus> StatusFilter { get; set; } = new();
        public decimal? MinVolume { get; set; }
        public decimal? MaxVolume { get; set; }
        public bool IncludeMap { get; set; } = true;
        public string SortBy { get; set; } = "ExpectedVolume";
    }

    /// <summary>
    /// Project analytics request DTO
    /// </summary>
    public class ProjectAnalyticsRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> AnalyticsTypes { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Metrics { get; set; } = new();
        public bool GenerateDashboard { get; set; } = true;
    }

    /// <summary>
    /// Decision request DTO
    /// </summary>
    public class DecisionRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string DecisionType { get; set; } = "PortfolioSelection";
        public List<string> Options { get; set; } = new();
        public Dictionary<string, decimal> CriteriaWeights { get; set; } = new();
        public bool IncludeRiskAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Scenario analysis request DTO
    /// </summary>
    public class ScenarioAnalysisRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> ScenarioParameters { get; set; } = new();
        public Dictionary<string, Tuple<decimal, decimal>> ParameterRanges { get; set; } = new();
        public int NumberOfScenarios { get; set; } = 5;
        public string AnalysisType { get; set; } = "Sensitivity";
    }

    /// <summary>
    /// Value of information request DTO
    /// </summary>
    public class ValueOfInformationRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> InformationTypes { get; set; } = new();
        public Dictionary<string, decimal> InformationCosts { get; set; } = new();
        public string ValuationMethod { get; set; } = "ExpectedValue";
        public decimal DiscountRate { get; set; } = 0.1m;
    }

    #endregion
}