using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public enum ProspectStatus
    {
        Unknown = 0,
        Identified,
        Evaluated,
        Approved,
        Rejected
    }

    public class ProspectEvaluationRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
    }

    public class VolumetricAnalysisRequest : ModelEntityBase
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

    public class VolumetricAnalysis : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
        public decimal EstimatedOilResources { get; set; }
        public decimal EstimatedGasResources { get; set; }
        public string ResourceUnit { get; set; } = "STB";
    }

    public class ProspectRiskAssessmentRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public List<string> RiskCategories { get; set; } = new();
        public string AssessmentMethodology { get; set; } = "Subjective";
        public bool IncludeMitigationAnalysis { get; set; } = true;
    }

    public class RiskAssessment : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public string AssessedBy { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
        public decimal RiskScore { get; set; }
        public string RiskLevel { get; set; } = "Medium";
        public List<RiskFactor> RiskFactors { get; set; } = new();
    }

    public class EconomicEvaluationRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal OilPrice { get; set; }
        public decimal GasPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal OperatingCost { get; set; }
        public decimal CapitalCost { get; set; }
        public string EconomicModel { get; set; } = "Standard";
    }

    public class EconomicEvaluation : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;
        public decimal NPV { get; set; }
        public decimal IRR { get; set; }
        public decimal PaybackYears { get; set; }
    }

    public class ProspectRankingRequest : ModelEntityBase
    {
        public List<string> ProspectIds { get; set; } = new();
        public List<string> Criteria { get; set; } = new();
    }

    public class ProspectComparisonRequest : ModelEntityBase
    {
        public List<string> ProspectIds { get; set; } = new();
        public List<string> ComparisonCriteria { get; set; } = new();
    }

    public class ProspectComparison : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public Dictionary<string, decimal> CriteriaScores { get; set; } = new();
    }

    public class SensitivityAnalysisRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new();
        public int NumberOfIterations { get; set; } = 100;
    }

    public class SensitivityAnalysis : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
        public Dictionary<string, decimal[]> Results { get; set; } = new();
    }

    public class ResourceEstimateRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public string EstimationMethodology { get; set; } = "Volumetric";
    }

    public class ResourceEstimate : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal OilEstimate { get; set; }
        public decimal GasEstimate { get; set; }
    }

    public class ProbabilisticAssessmentRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public int NumberOfSimulations { get; set; } = 10000;
    }

    public class ProbabilisticAssessment : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal P10 { get; set; }
        public decimal P50 { get; set; }
        public decimal P90 { get; set; }
    }

    public class PlayAnalysisRequest : ModelEntityBase
    {
        public string PlayId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PlayAnalysis : ModelEntityBase
    {
        public string PlayId { get; set; } = string.Empty;
        public List<PlayStatistics> Statistics { get; set; } = new();
    }

    public class PlayStatistics : ModelEntityBase
    {
        public string Metric { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class AnalogSearchRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public List<string> SearchCriteria { get; set; } = new();
        public decimal SimilarityThreshold { get; set; } = 0.7m;
    }

    public class AnalogProspect : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal SimilarityScore { get; set; }
    }

    public class ProspectReportRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Comprehensive";
    }

    public class ProspectReport : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class PortfolioReportRequest : ModelEntityBase
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> ProspectIds { get; set; } = new();
    }

    public class PortfolioReport : ModelEntityBase
    {
        public string PortfolioName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class ProspectValidation : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
    }

    public class PeerReviewRequest : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public string Reviewer { get; set; } = string.Empty;
        public string ReviewType { get; set; } = "Technical";
    }

    public class PeerReview : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public string Reviewer { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class PortfolioSummary : ModelEntityBase
    {
        public string PortfolioName { get; set; } = string.Empty;
        public int TotalProspects { get; set; }
        public decimal TotalEstimatedResources { get; set; }
    }
}

