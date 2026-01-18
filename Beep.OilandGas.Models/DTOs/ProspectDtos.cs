using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for prospect information.
    /// </summary>
    public class ProspectDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Country { get; set; }
        public string? StateProvince { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? EvaluationDate { get; set; }
        public string? EvaluatedBy { get; set; }
        public decimal? EstimatedResources { get; set; }
        public string? ResourceUnit { get; set; }
        public decimal? RiskScore { get; set; }
        public string? RiskLevel { get; set; }
        public List<SeismicSurveyDto> SeismicSurveys { get; set; } = new();
        public ProspectEvaluationDto? Evaluation { get; set; }
    }

    /// <summary>
    /// DTO for seismic survey information.
    /// </summary>
    public class SeismicSurveyDto
    {
        public string SurveyId { get; set; } = string.Empty;
        public string ProspectId { get; set; } = string.Empty;
        public string SurveyName { get; set; } = string.Empty;
        public string? SurveyType { get; set; }
        public DateTime? SurveyDate { get; set; }
        public string? Contractor { get; set; }
        public decimal? AreaCovered { get; set; }
        public string? AreaUnit { get; set; }
        public string? Status { get; set; }
        public string? InterpretationStatus { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for prospect evaluation results.
    /// </summary>
    public class ProspectEvaluationDto
    {
        public string EvaluationId { get; set; } = string.Empty;
        public string ProspectId { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public string EvaluatedBy { get; set; } = string.Empty;
        public decimal? EstimatedOilResources { get; set; }
        public decimal? EstimatedGasResources { get; set; }
        public string? ResourceUnit { get; set; }
        public decimal? ProbabilityOfSuccess { get; set; }
        public decimal? RiskScore { get; set; }
        public string? RiskLevel { get; set; }
        public string? Recommendation { get; set; }
        public string? Remarks { get; set; }
        public List<RiskFactorDto> RiskFactors { get; set; } = new();
    }

    /// <summary>
    /// DTO for risk factors in prospect evaluation.
    /// </summary>
    public class RiskFactorDto
    {
        public string RiskFactorId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? RiskScore { get; set; }
        public string? Mitigation { get; set; }
    }

    /// <summary>
    /// DTO for creating a new prospect.
    /// </summary>
    public class CreateProspectDto
    {
        public string ProspectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Country { get; set; }
        public string? StateProvince { get; set; }
        public string? FieldId { get; set; }
    }

    /// <summary>
    /// DTO for updating a prospect.
    /// </summary>
     public class UpdateProspectDto
     {
         public string? ProspectName { get; set; }
         public string? Description { get; set; }
         public string? Status { get; set; }
         public decimal? EstimatedResources { get; set; }
         public string? ResourceUnit { get; set; }
     }

     /// <summary>
     /// DTO for prospect ranking results
     /// </summary>
     public class ProspectRankingDto
     {
         public string ProspectId { get; set; } = string.Empty;
         public string ProspectName { get; set; } = string.Empty;
         public int Rank { get; set; }
         public decimal Score { get; set; }
         public decimal WeightedScore { get; set; }
         public List<CriteriaScoringDto> CriteriaScores { get; set; } = new();
     }

     /// <summary>
     /// DTO for seismic data interpretation analysis
     /// </summary>
     public class SeismicInterpretationAnalysisDto
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string ProspectId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public string SurveyId { get; set; } = string.Empty;
         public int HorizonCount { get; set; }
         public int FaultCount { get; set; }
         public List<HorizonDto> Horizons { get; set; } = new();
         public List<FaultDto> Faults { get; set; } = new();
         public decimal InterpretationConfidence { get; set; }
         public string InterpretationStatus { get; set; } = string.Empty;
     }

     /// <summary>
     /// DTO for seismic horizon
     /// </summary>
     public class HorizonDto
     {
         public string HorizonId { get; set; } = string.Empty;
         public string HorizonName { get; set; } = string.Empty;
         public string GeologicalAge { get; set; } = string.Empty;
         public decimal Depth { get; set; }
         public decimal Thickness { get; set; }
         public string LithologyType { get; set; } = string.Empty;
         public string ReservoirQuality { get; set; } = string.Empty;
     }

     /// <summary>
     /// DTO for seismic fault
     /// </summary>
     public class FaultDto
     {
         public string FaultId { get; set; } = string.Empty;
         public string FaultName { get; set; } = string.Empty;
         public decimal Throw { get; set; }
         public string FaultType { get; set; } = string.Empty;
         public string SealingPotential { get; set; } = string.Empty;
     }

     /// <summary>
     /// DTO for resource estimation
     /// </summary>
     public class ResourceEstimationResultDto
     {
         public string EstimationId { get; set; } = string.Empty;
         public string ProspectId { get; set; } = string.Empty;
         public DateTime EstimationDate { get; set; }
         public string EstimatedBy { get; set; } = string.Empty;
         public decimal GrossRockVolume { get; set; }
         public decimal NetRockVolume { get; set; }
         public decimal Porosity { get; set; }
         public decimal WaterSaturation { get; set; }
         public decimal OilRecoveryFactor { get; set; }
         public decimal GasRecoveryFactor { get; set; }
         public decimal EstimatedOilVolume { get; set; }
         public decimal EstimatedGasVolume { get; set; }
         public string VolumeUnit { get; set; } = string.Empty;
         public string EstimationMethod { get; set; } = string.Empty;
         public List<string> AssumptionsAndLimitations { get; set; } = new();
     }

     /// <summary>
     /// DTO for trap geometry analysis
     /// </summary>
     public class TrapGeometryAnalysisDto
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string ProspectId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public string TrapType { get; set; } = string.Empty; // Structural, Stratigraphic, Combination
         public decimal Closure { get; set; }
         public decimal CrestDepth { get; set; }
         public decimal SpillPointDepth { get; set; }
         public decimal Area { get; set; }
         public string AreaUnit { get; set; } = string.Empty;
         public decimal Volume { get; set; }
         public string VolumeUnit { get; set; } = string.Empty;
         public string TrapGeometry { get; set; } = string.Empty;
         public string SourceRockProximity { get; set; } = string.Empty;
     }

     /// <summary>
     /// DTO for migration path analysis
     /// </summary>
     public class MigrationPathAnalysisDto
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string ProspectId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public string SourceRockId { get; set; } = string.Empty;
         public decimal SourceRockMaturityLevel { get; set; }
         public string MigrationPathway { get; set; } = string.Empty;
         public decimal MigrationDistance { get; set; }
         public string DistanceUnit { get; set; } = string.Empty;
         public decimal MigrationEfficiency { get; set; }
         public string SealIntegrity { get; set; } = string.Empty;
         public string LateralMigrationRisk { get; set; } = string.Empty;
         public List<string> MigrationBarriers { get; set; } = new();
     }

     /// <summary>
     /// DTO for seal and source rock assessment
     /// </summary>
     public class SealSourceAssessmentDto
     {
         public string AssessmentId { get; set; } = string.Empty;
         public string ProspectId { get; set; } = string.Empty;
         public DateTime AssessmentDate { get; set; }
         public string SealRockType { get; set; } = string.Empty;
         public decimal SealRockThickness { get; set; }
         public string SealQuality { get; set; } = string.Empty;
         public decimal SealIntegrityScore { get; set; }
         public string SourceRockType { get; set; } = string.Empty;
         public decimal SourceRockMaturity { get; set; }
         public string GenerationStatus { get; set; } = string.Empty;
         public decimal SourceRockProductivity { get; set; }
         public string SystemStatus { get; set; } = string.Empty; // Active, Inactive, Marginal
     }

     /// <summary>
     /// DTO for risk category details
     /// </summary>
     public class RiskCategoryDto
     {
         public string CategoryName { get; set; } = string.Empty;
         public decimal RiskScore { get; set; }
         public string RiskLevel { get; set; } = string.Empty;
         public List<string> MitigationStrategies { get; set; } = new();
     }

     /// <summary>
     /// DTO for economic viability analysis
     /// </summary>
     public class EconomicViabilityAnalysisDto
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string ProspectId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public decimal EstimatedCapitalCost { get; set; }
         public decimal EstimatedOperatingCost { get; set; }
         public decimal OilPrice { get; set; }
         public decimal GasPrice { get; set; }
         public decimal DiscountRate { get; set; }
         public int ProjectLifeYears { get; set; }
         public decimal NetPresentValue { get; set; }
         public decimal InternalRateOfReturn { get; set; }
         public decimal PaybackPeriodYears { get; set; }
         public decimal ProfitabilityIndex { get; set; }
         public string ViabilityStatus { get; set; } = string.Empty; // Viable, Marginal, Non-Viable
     }

     /// <summary>
     /// DTO for criteria scoring in ranking
     /// </summary>
     public class CriteriaScoringDto
     {
         public string CriteriaName { get; set; } = string.Empty;
         public decimal Weight { get; set; }
         public decimal RawScore { get; set; }
         public decimal WeightedScore { get; set; }
         public string Category { get; set; } = string.Empty;
     }

     /// <summary>
     /// DTO for prospect risk analysis result
     /// </summary>
     public class ProspectRiskAnalysisResultDto
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string ProspectId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public string AssessedBy { get; set; } = string.Empty;
         public decimal TrapRisk { get; set; }
         public decimal SealRisk { get; set; }
         public decimal SourceRisk { get; set; }
         public decimal MigrationRisk { get; set; }
         public decimal CharacterizationRisk { get; set; }
         public decimal OverallRisk { get; set; }
         public decimal ProbabilityOfSuccess { get; set; }
         public string OverallRiskLevel { get; set; } = string.Empty;
         public List<RiskCategoryDto> RiskCategories { get; set; } = new();
     }

     /// <summary>
     /// DTO for portfolio optimization
     /// </summary>
     public class PortfolioOptimizationResultDto
     {
         public string OptimizationId { get; set; } = string.Empty;
         public DateTime OptimizationDate { get; set; }
         public List<string> RecommendedProspects { get; set; } = new();
         public List<string> MarginallProspects { get; set; } = new();
         public List<string> RejectedProspects { get; set; } = new();
         public decimal TotalPortfolioRisk { get; set; }
         public decimal TotalExpectedValue { get; set; }
         public decimal RiskAdjustedReturn { get; set; }
         public string OptimizationStrategy { get; set; } = string.Empty;
         public List<string> OptimizationRecommendations { get; set; } = new();
     }
}




