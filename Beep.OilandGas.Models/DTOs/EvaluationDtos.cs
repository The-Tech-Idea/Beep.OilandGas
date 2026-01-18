using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    public enum ProspectStatus
    {
        Unknown = 0,
        Identified,
        Evaluated,
        Approved,
        Rejected
    }

    public class ProspectEvaluationRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
    }

    public class VolumetricAnalysisRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal GrossRockVolume { get; set; }
        public decimal NetToGrossRatio { get; set; }
        public decimal Porosity { get; set; }
        public decimal WaterSaturation { get; set; }
        public decimal FormationVolumeFactor { get; set; }
        public decimal RecoveryFactor { get; set; }
        public bool IncludeUncertainty { get; set; } = true;
        public string AnalysisMethodology { get; set; } = "Deterministic";
    }

    public class VolumetricAnalysisDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
        public decimal EstimatedOilResources { get; set; }
        public decimal EstimatedGasResources { get; set; }
        public string ResourceUnit { get; set; } = "STB";
    }

    public class RiskAssessmentRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public List<string> RiskCategories { get; set; } = new();
        public string AssessmentMethodology { get; set; } = "Subjective";
        public bool IncludeMitigationAnalysis { get; set; } = true;
    }

    public class RiskAssessmentDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string AssessedBy { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
        public decimal RiskScore { get; set; }
        public string RiskLevel { get; set; } = "Medium";
        public List<RiskFactorDto> RiskFactors { get; set; } = new();
    }

    public class EconomicEvaluationRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal OilPrice { get; set; }
        public decimal GasPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal OperatingCost { get; set; }
        public decimal CapitalCost { get; set; }
        public string EconomicModel { get; set; } = "Standard";
    }

    public class EconomicEvaluationDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;
        public decimal NPV { get; set; }
        public decimal IRR { get; set; }
        public decimal PaybackYears { get; set; }
    }

    public class ProspectRankingRequestDto
    {
        public List<string> ProspectIds { get; set; } = new();
        public List<string> Criteria { get; set; } = new();
    }

    public class ProspectComparisonRequestDto
    {
        public List<string> ProspectIds { get; set; } = new();
        public List<string> ComparisonCriteria { get; set; } = new();
    }

    public class ProspectComparisonDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public Dictionary<string, decimal> CriteriaScores { get; set; } = new();
    }

    public class SensitivityAnalysisRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new();
        public int NumberOfIterations { get; set; } = 100;
    }

    public class SensitivityAnalysisDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
        public Dictionary<string, decimal[]> Results { get; set; } = new();
    }

    public class ResourceEstimateRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string EstimationMethodology { get; set; } = "Volumetric";
    }

    public class ResourceEstimateDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal OilEstimate { get; set; }
        public decimal GasEstimate { get; set; }
    }

    public class ProbabilisticAssessmentRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public int NumberOfSimulations { get; set; } = 10000;
    }

    public class ProbabilisticAssessmentDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal P10 { get; set; }
        public decimal P50 { get; set; }
        public decimal P90 { get; set; }
    }

    public class PlayAnalysisRequestDto
    {
        public string PlayId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PlayAnalysisDto
    {
        public string PlayId { get; set; } = string.Empty;
        public List<PlayStatisticsDto> Statistics { get; set; } = new();
    }

    public class PlayStatisticsDto
    {
        public string Metric { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class AnalogSearchRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public List<string> SearchCriteria { get; set; } = new();
        public decimal SimilarityThreshold { get; set; } = 0.7m;
    }

    public class AnalogProspectDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal SimilarityScore { get; set; }
    }

    public class ProspectReportRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Comprehensive";
    }

    public class ProspectReportDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class PortfolioReportRequestDto
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
    }

    public class PortfolioReportDto
    {
        public string PortfolioName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class ProspectValidationDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
    }

    public class PeerReviewRequestDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string Reviewer { get; set; } = string.Empty;
        public string ReviewType { get; set; } = "Technical";
    }

    public class PeerReviewDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string Reviewer { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class PortfolioSummaryDto
    {
        public string PortfolioName { get; set; } = string.Empty;
        public int TotalProspects { get; set; }
        public decimal TotalEstimatedResources { get; set; }
    }
}
