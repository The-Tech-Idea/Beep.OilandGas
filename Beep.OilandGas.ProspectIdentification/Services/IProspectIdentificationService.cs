using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProspectIdentification;

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
        Task<ProspectIdentificationProject> CreateProjectAsync(CreateProject createDto, string userId);

        /// <summary>
        /// Gets project status and progress
        /// </summary>
        Task<ProspectIdentificationProject> GetProjectAsync(string projectId);

        /// <summary>
        /// Updates project status
        /// </summary>
        Task<ProspectIdentificationProject> UpdateProjectStatusAsync(string projectId, ProjectStatus newStatus, string userId);

        /// <summary>
        /// Gets all projects with filtering
        /// </summary>
        Task<List<ProspectIdentificationProject>> GetProjectsAsync(string? basinId = null, ProjectStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);

        #endregion

        #region Data Acquisition & Integration

        /// <summary>
        /// Imports seismic data into the project
        /// </summary>
        Task<SeismicDataImport> ImportSeismicDataAsync(string projectId, SeismicDataImportRequest request, string userId);

        /// <summary>
        /// Imports well data for calibration
        /// </summary>
        Task<WellDataImport> ImportWellDataAsync(string projectId, WellDataImportRequest request, string userId);

        /// <summary>
        /// Integrates multiple data sources
        /// </summary>
        Task<DataIntegration> IntegrateDataSourcesAsync(string projectId, DataIntegrationRequest request, string userId);

        /// <summary>
        /// Performs quality control on imported data
        /// </summary>
        Task<DataQuality> PerformDataQualityControlAsync(string projectId, DataQualityRequest request);

        #endregion

        #region Prospect Generation

        /// <summary>
        /// Generates prospects from seismic interpretation
        /// </summary>
        Task<List<GeneratedProspect>> GenerateProspectsFromSeismicAsync(string projectId, SeismicProspectGenerationRequest request, string userId);

        /// <summary>
        /// Generates prospects from geological analysis
        /// </summary>
        Task<List<GeneratedProspect>> GenerateProspectsFromGeologyAsync(string projectId, GeologicalProspectGenerationRequest request, string userId);

        /// <summary>
        /// Generates prospects from play-based analysis
        /// </summary>
        Task<List<GeneratedProspect>> GenerateProspectsFromPlayAsync(string projectId, PlayBasedProspectGenerationRequest request, string userId);

        /// <summary>
        /// Merges duplicate or overlapping prospects
        /// </summary>
        Task<List<GeneratedProspect>> MergeProspectsAsync(string projectId, ProspectMergeRequest request, string userId);

        #endregion

        #region Prospect Evaluation Pipeline

        /// <summary>
        /// Runs complete evaluation pipeline on prospects
        /// </summary>
        Task<EvaluationPipelineResult> RunEvaluationPipelineAsync(string projectId, EvaluationPipelineRequest request, string userId);

        /// <summary>
        /// Performs rapid screening of generated prospects
        /// </summary>
        Task<RapidScreeningResult> PerformRapidScreeningAsync(string projectId, RapidScreeningRequest request);

        /// <summary>
        /// Performs detailed evaluation of selected prospects
        /// </summary>
        Task<DetailedEvaluationResult> PerformDetailedEvaluationAsync(string projectId, DetailedEvaluationRequest request, string userId);

        /// <summary>
        /// Updates prospect evaluation status
        /// </summary>
        Task UpdateProspectEvaluationStatusAsync(string projectId, string prospectId, EvaluationStatus newStatus, string userId);

        #endregion

        #region Portfolio Management

        /// <summary>
        /// Creates prospect portfolio for the project
        /// </summary>
        Task<ProspectPortfolio> CreatePortfolioAsync(string projectId, PortfolioCreationRequest request, string userId);

        /// <summary>
        /// Optimizes prospect portfolio
        /// </summary>
        Task<PortfolioOptimization> OptimizePortfolioAsync(string projectId, PortfolioOptimizationRequest request, string userId);

        /// <summary>
        /// Performs portfolio risk analysis
        /// </summary>
        Task<PortfolioRiskAnalysis> AnalyzePortfolioRiskAsync(string projectId, PortfolioRiskRequest request);

        /// <summary>
        /// Generates drilling schedule recommendations
        /// </summary>
        Task<DrillingSchedule> GenerateDrillingScheduleAsync(string projectId, DrillingScheduleRequest request);

        #endregion

        #region Collaboration & Review

        /// <summary>
        /// Assigns prospects to evaluators
        /// </summary>
        Task AssignProspectsAsync(string projectId, ProspectAssignmentRequest request, string userId);

        /// <summary>
        /// Gets evaluation progress and status
        /// </summary>
        Task<EvaluationProgress> GetEvaluationProgressAsync(string projectId);

        /// <summary>
        /// Performs quality assurance review
        /// </summary>
        Task<QualityAssurance> PerformQualityAssuranceAsync(string projectId, QualityAssuranceRequest request, string userId);

        /// <summary>
        /// Escalates prospects requiring senior review
        /// </summary>
        Task EscalateProspectsAsync(string projectId, ProspectEscalationRequest request, string userId);

        #endregion

        #region Reporting & Analytics

        /// <summary>
        /// Generates project status report
        /// </summary>
        Task<ProjectReport> GenerateProjectReportAsync(string projectId, ProjectReportRequest request);

        /// <summary>
        /// Generates prospect inventory report
        /// </summary>
        Task<ProspectInventory> GenerateProspectInventoryAsync(string projectId, ProspectInventoryRequest request);

        /// <summary>
        /// Performs project analytics
        /// </summary>
        Task<ProjectAnalytics> PerformProjectAnalyticsAsync(string projectId, ProjectAnalyticsRequest request);

        /// <summary>
        /// Exports project data
        /// </summary>
        Task<byte[]> ExportProjectDataAsync(string projectId, ExportRequest request);

        #endregion

        #region Decision Support

        /// <summary>
        /// Provides decision recommendations
        /// </summary>
        Task<DecisionRecommendation> GetDecisionRecommendationsAsync(string projectId, DecisionRequest request);

        /// <summary>
        /// Performs scenario analysis
        /// </summary>
        Task<ScenarioAnalysis> PerformScenarioAnalysisAsync(string projectId, ScenarioAnalysisRequest request);

        /// <summary>
        /// Calculates value of information
        /// </summary>
        Task<ValueOfInformation> CalculateValueOfInformationAsync(string projectId, ValueOfInformationRequest request);

        /// <summary>
        /// Generates management presentation
        /// </summary>
        Task<ManagementPresentation> GenerateManagementPresentationAsync(string projectId, PresentationRequest request);

        #endregion
    }

    #region Project Management DTOs

    /// <summary>
    /// Prospect identification project DTO
    /// </summary>
    public class ProspectIdentificationProject
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
        public ProjectMetrics Metrics { get; set; } = new();
        public List<ProjectMilestone> Milestones { get; set; } = new();
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
    public class CreateProject
    {
        public string ProjectName { get; set; } = string.Empty;
        public string BasinId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProjectManager { get; set; } = string.Empty;
        public List<string> TeamMembers { get; set; } = new();
        public decimal Budget { get; set; }
        public string BudgetCurrency { get; set; } = string.Empty;
        public List<ProjectMilestone> Milestones { get; set; } = new();
    }

    /// <summary>
    /// Project metrics DTO
    /// </summary>
    public class ProjectMetrics
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
    public class ProjectMilestone
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
    public class SeismicDataImport
    {
        public string ImportId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public List<SeismicSurvey> ImportedSurveys { get; set; } = new();
        public int TotalFiles { get; set; }
        public long TotalSizeBytes { get; set; }
        public string ImportStatus { get; set; } = string.Empty;
        public List<string> ImportErrors { get; set; } = new();
    }

    /// <summary>
    /// Well data import DTO
    /// </summary>
    public class WellDataImport
    {
        public string ImportId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public List<Well> ImportedWells { get; set; } = new();
        public int TotalWells { get; set; }
        public int TotalLogs { get; set; }
        public string ImportStatus { get; set; } = string.Empty;
        public List<string> ImportErrors { get; set; } = new();
    }

    /// <summary>
    /// Well DTO
    /// </summary>
    public class Well
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
    public class DataIntegration
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
    public class DataQuality
    {
        public string QualityCheckId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime CheckDate { get; set; }
        public decimal OverallQualityScore { get; set; }
        public Dictionary<string, decimal> QualityByDataType { get; set; } = new();
        public List<DATA_QUALITY_ISSUE> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Data quality issue DTO
    /// </summary>
    public class DATA_QUALITY_ISSUE
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
    public class GeneratedProspect
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
    public class EvaluationPipelineResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public PipelineStageResult ScreeningStage { get; set; } = new();
        public PipelineStageResult DetailedEvaluationStage { get; set; } = new();
        public PipelineStageResult FinalRankingStage { get; set; } = new();
        public int TotalProspectsProcessed { get; set; }
        public int ProspectsAdvanced { get; set; }
        public string PipelineStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pipeline stage result DTO
    /// </summary>
    public class PipelineStageResult
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
    public class RapidScreeningResult
    {
        public string ScreeningId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ScreeningDate { get; set; }
        public List<ScreeningResult> Results { get; set; } = new();
        public int TotalProspects { get; set; }
        public int PassedScreening { get; set; }
        public int FailedScreening { get; set; }
        public string ScreeningCriteria { get; set; } = string.Empty;
    }

    /// <summary>
    /// Screening result DTO
    /// </summary>
    public class ScreeningResult
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
    public class DetailedEvaluationResult
    {
        public string EvaluationId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public List<DetailedProspectEvaluation> Evaluations { get; set; } = new();
        public string EvaluationMethodology { get; set; } = string.Empty;
        public List<string> KeyFindings { get; set; } = new();
    }

    /// <summary>
    /// Detailed prospect evaluation DTO
    /// </summary>
    public class DetailedProspectEvaluation
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public VolumetricAnalysis VolumetricAnalysis { get; set; } = new();
        public RiskAssessment RiskAssessment { get; set; } = new();
        public EconomicEvaluation EconomicEvaluation { get; set; } = new();
        public decimal OverallScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    #endregion

    #region Portfolio Management DTOs

    /// <summary>
    /// Prospect portfolio DTO
    /// </summary>
    public class ProspectPortfolio
    {
        public string PortfolioId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string PortfolioName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public List<PortfolioProspect> Prospects { get; set; } = new();
        public PortfolioSummary Summary { get; set; } = new();
        public string OptimizationStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Portfolio prospect DTO
    /// </summary>
    public class PortfolioProspect
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
    public class PortfolioOptimization
    {
        public string OptimizationId { get; set; } = string.Empty;
        public string PortfolioId { get; set; } = string.Empty;
        public DateTime OptimizationDate { get; set; }
        public string OptimizationMethod { get; set; } = string.Empty;
        public List<OptimizationScenario> Scenarios { get; set; } = new();
        public OptimizationScenario RecommendedScenario { get; set; } = new();
        public string OptimizationSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimization scenario DTO
    /// </summary>
    public class OptimizationScenario
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
    public class PortfolioRiskAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PortfolioId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal OverallRisk { get; set; }
        public decimal ValueAtRisk { get; set; }
        public decimal ExpectedShortfall { get; set; }
        public List<RiskFactor> KeyRiskFactors { get; set; } = new();
        public byte[] RiskChart { get; set; } = Array.Empty<byte>();
        public List<string> RiskMitigationStrategies { get; set; } = new();
    }

    /// <summary>
    /// Drilling schedule DTO
    /// </summary>
    public class DrillingSchedule
    {
        public string ScheduleId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<DrillingSlot> DrillingSlots { get; set; } = new();
        public decimal TotalCost { get; set; }
        public decimal TotalValue { get; set; }
        public string ScheduleOptimization { get; set; } = string.Empty;
    }

    /// <summary>
    /// Drilling slot DTO
    /// </summary>
    public class DrillingSlot
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
    public class SeismicDataImportRequest
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
    public class WellDataImportRequest
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
    public class DataIntegrationRequest
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
    public class DataQualityRequest
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
    public class SeismicProspectGenerationRequest
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
    public class GeologicalProspectGenerationRequest
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
    public class PlayBasedProspectGenerationRequest
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
    public class ProspectMergeRequest
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
    public class EvaluationPipelineRequest
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
    public class RapidScreeningRequest
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
    public class DetailedEvaluationRequest
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
    public class PortfolioCreationRequest
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
    public class PortfolioOptimizationRequest
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
    public class PortfolioRiskRequest
    {
        public string PortfolioId { get; set; } = string.Empty;
        public decimal ConfidenceLevel { get; set; } = 0.95m;
        public string RiskMeasure { get; set; } = "VaR";
        public int SimulationRuns { get; set; } = 10000;
    }

    /// <summary>
    /// Drilling schedule request DTO
    /// </summary>
    public class DrillingScheduleRequest
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
    public class ProspectAssignmentRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public Dictionary<string, List<string>> Assignments { get; set; } = new(); // Evaluator -> ProspectIds
        public DateTime Deadline { get; set; }
        public string AssignmentType { get; set; } = "Evaluation";
    }

    /// <summary>
    /// Evaluation progress DTO
    /// </summary>
    public class EvaluationProgress
    {
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public int TotalProspects { get; set; }
        public int CompletedEvaluations { get; set; }
        public int InProgressEvaluations { get; set; }
        public int PendingEvaluations { get; set; }
        public decimal OverallProgress { get; set; }
        public List<EvaluatorProgress> EvaluatorProgress { get; set; } = new();
        public List<string> Bottlenecks { get; set; } = new();
    }

    /// <summary>
    /// Evaluator progress DTO
    /// </summary>
    public class EvaluatorProgress
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
    public class QualityAssurance
    {
        public string QAId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public string Reviewer { get; set; } = string.Empty;
        public List<QAReview> Reviews { get; set; } = new();
        public decimal OverallQualityScore { get; set; }
        public List<string> QARecommendations { get; set; } = new();
    }

    /// <summary>
    /// QA review DTO
    /// </summary>
    public class QAReview
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
    public class ProspectEscalationRequest
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
    public class ProjectReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public byte[] ReportContent { get; set; } = Array.Empty<byte>();
        public List<byte[]> Charts { get; set; } = new();
        public ProjectMetrics Metrics { get; set; } = new();
        public string ExecutiveSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Prospect inventory DTO
    /// </summary>
    public class ProspectInventory
    {
        public string InventoryId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<ProspectInventoryItem> Prospects { get; set; } = new();
        public InventorySummary Summary { get; set; } = new();
        public byte[] InventoryMap { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// Prospect inventory item DTO
    /// </summary>
    public class ProspectInventoryItem
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
    public class InventorySummary
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
    public class ProjectAnalytics
    {
        public string AnalyticsId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<TrendAnalysis> Trends { get; set; } = new();
        public List<PerformanceMetric> Metrics { get; set; } = new();
        public List<PredictiveInsight> Insights { get; set; } = new();
        public byte[] AnalyticsDashboard { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// Trend analysis DTO
    /// </summary>
    public class TrendAnalysis
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
    public class PerformanceMetric
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
    public class PredictiveInsight
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
    public class ExportRequest
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
    public class DecisionRecommendation
    {
        public string RecommendationId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string DecisionType { get; set; } = string.Empty;
        public List<RecommendationOption> Options { get; set; } = new();
        public RecommendationOption RecommendedOption { get; set; } = new();
        public string Rationale { get; set; } = string.Empty;
        public List<string> Assumptions { get; set; } = new();
        public List<string> Risks { get; set; } = new();
    }

    /// <summary>
    /// Recommendation option DTO
    /// </summary>
    public class RecommendationOption
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
    public class ScenarioAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<AnalysisScenario> Scenarios { get; set; } = new();
        public AnalysisScenario BaseCase { get; set; } = new();
        public List<ScenarioComparison> Comparisons { get; set; } = new();
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Analysis scenario DTO
    /// </summary>
    public class AnalysisScenario
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
    public class ScenarioComparison
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
    public class ValueOfInformation
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<InformationValue> InformationValues { get; set; } = new();
        public InformationValue HighestValueInformation { get; set; } = new();
        public decimal TotalVOI { get; set; }
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Information value DTO
    /// </summary>
    public class InformationValue
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
    public class ManagementPresentation
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
    public class PresentationRequest
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
    public class QualityAssuranceRequest
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
    public class ProjectReportRequest
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
    public class ProspectInventoryRequest
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
    public class ProjectAnalyticsRequest
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
    public class DecisionRequest
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
    public class ScenarioAnalysisRequest
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
    public class ValueOfInformationRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<string> InformationTypes { get; set; } = new();
        public Dictionary<string, decimal> InformationCosts { get; set; } = new();
        public string ValuationMethod { get; set; } = "ExpectedValue";
        public decimal DiscountRate { get; set; } = 0.1m;
    }

    #endregion
}