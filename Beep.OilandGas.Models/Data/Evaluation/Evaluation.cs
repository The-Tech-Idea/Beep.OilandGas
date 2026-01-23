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
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
    }

    public class VolumetricAnalysisRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal GrossRockVolumeValue;

        public decimal GrossRockVolume

        {

            get { return this.GrossRockVolumeValue; }

            set { SetProperty(ref GrossRockVolumeValue, value); }

        }
        private decimal NetToGrossRatioValue;

        public decimal NetToGrossRatio

        {

            get { return this.NetToGrossRatioValue; }

            set { SetProperty(ref NetToGrossRatioValue, value); }

        }
        private decimal PorosityValue;

        public decimal Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }
        private decimal WaterSaturationValue;

        public decimal WaterSaturation

        {

            get { return this.WaterSaturationValue; }

            set { SetProperty(ref WaterSaturationValue, value); }

        }
        private decimal FormationVolumeFactorValue;

        public decimal FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

        }
        private decimal RecoveryFactorValue;

        public decimal RecoveryFactor

        {

            get { return this.RecoveryFactorValue; }

            set { SetProperty(ref RecoveryFactorValue, value); }

        }
        private bool IncludeUncertaintyValue = true;

        public bool IncludeUncertainty

        {

            get { return this.IncludeUncertaintyValue; }

            set { SetProperty(ref IncludeUncertaintyValue, value); }

        }
        private string AnalysisMethodologyValue = "Deterministic";

        public string AnalysisMethodology

        {

            get { return this.AnalysisMethodologyValue; }

            set { SetProperty(ref AnalysisMethodologyValue, value); }

        }
    }

    public class VolumetricAnalysis : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private DateTime AnalysisDateValue = DateTime.UtcNow;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal EstimatedOilResourcesValue;

        public decimal EstimatedOilResources

        {

            get { return this.EstimatedOilResourcesValue; }

            set { SetProperty(ref EstimatedOilResourcesValue, value); }

        }
        private decimal EstimatedGasResourcesValue;

        public decimal EstimatedGasResources

        {

            get { return this.EstimatedGasResourcesValue; }

            set { SetProperty(ref EstimatedGasResourcesValue, value); }

        }
        private string ResourceUnitValue = "STB";

        public string ResourceUnit

        {

            get { return this.ResourceUnitValue; }

            set { SetProperty(ref ResourceUnitValue, value); }

        }
    }

    public class ProspectRiskAssessmentRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private List<string> RiskCategoriesValue = new();

        public List<string> RiskCategories

        {

            get { return this.RiskCategoriesValue; }

            set { SetProperty(ref RiskCategoriesValue, value); }

        }
        private string AssessmentMethodologyValue = "Subjective";

        public string AssessmentMethodology

        {

            get { return this.AssessmentMethodologyValue; }

            set { SetProperty(ref AssessmentMethodologyValue, value); }

        }
        private bool IncludeMitigationAnalysisValue = true;

        public bool IncludeMitigationAnalysis

        {

            get { return this.IncludeMitigationAnalysisValue; }

            set { SetProperty(ref IncludeMitigationAnalysisValue, value); }

        }
    }

    public class RiskAssessment : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string AssessedByValue = string.Empty;

        public string AssessedBy

        {

            get { return this.AssessedByValue; }

            set { SetProperty(ref AssessedByValue, value); }

        }
        private DateTime AssessmentDateValue = DateTime.UtcNow;

        public DateTime AssessmentDate

        {

            get { return this.AssessmentDateValue; }

            set { SetProperty(ref AssessmentDateValue, value); }

        }
        private decimal RiskScoreValue;

        public decimal RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
        private string RiskLevelValue = "Medium";

        public string RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private List<RiskFactor> RiskFactorsValue = new();

        public List<RiskFactor> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        }
    }

    public class EconomicEvaluationRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal OilPriceValue;

        public decimal OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal GasPriceValue;

        public decimal GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private decimal DiscountRateValue;

        public decimal DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }
        private decimal OperatingCostValue;

        public decimal OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        }
        private decimal CapitalCostValue;

        public decimal CapitalCost

        {

            get { return this.CapitalCostValue; }

            set { SetProperty(ref CapitalCostValue, value); }

        }
        private string EconomicModelValue = "Standard";

        public string EconomicModel

        {

            get { return this.EconomicModelValue; }

            set { SetProperty(ref EconomicModelValue, value); }

        }
    }

    public class EconomicEvaluation : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private DateTime EvaluationDateValue = DateTime.UtcNow;

        public DateTime EvaluationDate

        {

            get { return this.EvaluationDateValue; }

            set { SetProperty(ref EvaluationDateValue, value); }

        }
        private decimal NPVValue;

        public decimal NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }
        private decimal IRRValue;

        public decimal IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }
        private decimal PaybackYearsValue;

        public decimal PaybackYears

        {

            get { return this.PaybackYearsValue; }

            set { SetProperty(ref PaybackYearsValue, value); }

        }
    }

    public class ProspectRankingRequest : ModelEntityBase
    {
        private List<string> ProspectIdsValue = new();

        public List<string> ProspectIds

        {

            get { return this.ProspectIdsValue; }

            set { SetProperty(ref ProspectIdsValue, value); }

        }
        private List<string> CriteriaValue = new();

        public List<string> Criteria

        {

            get { return this.CriteriaValue; }

            set { SetProperty(ref CriteriaValue, value); }

        }
    }

    public class ProspectComparisonRequest : ModelEntityBase
    {
        private List<string> ProspectIdsValue = new();

        public List<string> ProspectIds

        {

            get { return this.ProspectIdsValue; }

            set { SetProperty(ref ProspectIdsValue, value); }

        }
        private List<string> ComparisonCriteriaValue = new();

        public List<string> ComparisonCriteria

        {

            get { return this.ComparisonCriteriaValue; }

            set { SetProperty(ref ComparisonCriteriaValue, value); }

        }
    }

    public class ProspectComparison : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal ScoreValue;

        public decimal Score

        {

            get { return this.ScoreValue; }

            set { SetProperty(ref ScoreValue, value); }

        }
        public Dictionary<string, decimal> CriteriaScores { get; set; } = new();
    }

    public class SensitivityAnalysisRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private List<string> ParametersValue = new();

        public List<string> Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
        private int NumberOfIterationsValue = 100;

        public int NumberOfIterations

        {

            get { return this.NumberOfIterationsValue; }

            set { SetProperty(ref NumberOfIterationsValue, value); }

        }
    }

    public class SensitivityAnalysis : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private DateTime AnalysisDateValue = DateTime.UtcNow;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        public Dictionary<string, decimal[]> Results { get; set; } = new();
    }

    public class ResourceEstimateRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string EstimationMethodologyValue = "Volumetric";

        public string EstimationMethodology

        {

            get { return this.EstimationMethodologyValue; }

            set { SetProperty(ref EstimationMethodologyValue, value); }

        }
    }

    public class ResourceEstimate : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal OilEstimateValue;

        public decimal OilEstimate

        {

            get { return this.OilEstimateValue; }

            set { SetProperty(ref OilEstimateValue, value); }

        }
        private decimal GasEstimateValue;

        public decimal GasEstimate

        {

            get { return this.GasEstimateValue; }

            set { SetProperty(ref GasEstimateValue, value); }

        }
    }

    public class ProbabilisticAssessmentRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private int NumberOfSimulationsValue = 10000;

        public int NumberOfSimulations

        {

            get { return this.NumberOfSimulationsValue; }

            set { SetProperty(ref NumberOfSimulationsValue, value); }

        }
    }

    public class ProbabilisticAssessment : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal P10Value;

        public decimal P10

        {

            get { return this.P10Value; }

            set { SetProperty(ref P10Value, value); }

        }
        private decimal P50Value;

        public decimal P50

        {

            get { return this.P50Value; }

            set { SetProperty(ref P50Value, value); }

        }
        private decimal P90Value;

        public decimal P90

        {

            get { return this.P90Value; }

            set { SetProperty(ref P90Value, value); }

        }
    }

    public class PlayAnalysisRequest : ModelEntityBase
    {
        private string PlayIdValue = string.Empty;

        public string PlayId

        {

            get { return this.PlayIdValue; }

            set { SetProperty(ref PlayIdValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }

    public class PlayAnalysis : ModelEntityBase
    {
        private string PlayIdValue = string.Empty;

        public string PlayId

        {

            get { return this.PlayIdValue; }

            set { SetProperty(ref PlayIdValue, value); }

        }
        private List<PlayStatistics> StatisticsValue = new();

        public List<PlayStatistics> Statistics

        {

            get { return this.StatisticsValue; }

            set { SetProperty(ref StatisticsValue, value); }

        }
    }

    public class PlayStatistics : ModelEntityBase
    {
        private string MetricValue = string.Empty;

        public string Metric

        {

            get { return this.MetricValue; }

            set { SetProperty(ref MetricValue, value); }

        }
        private decimal ValueValue;

        public decimal Value

        {

            get { return this.ValueValue; }

            set { SetProperty(ref ValueValue, value); }

        }
    }

    public class AnalogSearchRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private List<string> SearchCriteriaValue = new();

        public List<string> SearchCriteria

        {

            get { return this.SearchCriteriaValue; }

            set { SetProperty(ref SearchCriteriaValue, value); }

        }
        private decimal SimilarityThresholdValue = 0.7m;

        public decimal SimilarityThreshold

        {

            get { return this.SimilarityThresholdValue; }

            set { SetProperty(ref SimilarityThresholdValue, value); }

        }
    }

    public class AnalogProspect : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal SimilarityScoreValue;

        public decimal SimilarityScore

        {

            get { return this.SimilarityScoreValue; }

            set { SetProperty(ref SimilarityScoreValue, value); }

        }
    }

    public class ProspectReportRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string ReportTypeValue = "Comprehensive";

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
    }

    public class ProspectReport : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string UrlValue = string.Empty;

        public string Url

        {

            get { return this.UrlValue; }

            set { SetProperty(ref UrlValue, value); }

        }
    }

    public class PortfolioReportRequest : ModelEntityBase
    {
        private string PortfolioNameValue = string.Empty;

        public string PortfolioName

        {

            get { return this.PortfolioNameValue; }

            set { SetProperty(ref PortfolioNameValue, value); }

        }
        private List<string> ProspectIdsValue = new();

        public List<string> ProspectIds

        {

            get { return this.ProspectIdsValue; }

            set { SetProperty(ref ProspectIdsValue, value); }

        }
    }

    public class PortfolioReport : ModelEntityBase
    {
        private string PortfolioNameValue = string.Empty;

        public string PortfolioName

        {

            get { return this.PortfolioNameValue; }

            set { SetProperty(ref PortfolioNameValue, value); }

        }
        private string UrlValue = string.Empty;

        public string Url

        {

            get { return this.UrlValue; }

            set { SetProperty(ref UrlValue, value); }

        }
    }

    public class ProspectValidation : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private bool IsValidValue = true;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }
        private List<string> ErrorsValue = new();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }

    public class PeerReviewRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string ReviewerValue = string.Empty;

        public string Reviewer

        {

            get { return this.ReviewerValue; }

            set { SetProperty(ref ReviewerValue, value); }

        }
        private string ReviewTypeValue = "Technical";

        public string ReviewType

        {

            get { return this.ReviewTypeValue; }

            set { SetProperty(ref ReviewTypeValue, value); }

        }
    }

    public class PeerReview : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string ReviewerValue = string.Empty;

        public string Reviewer

        {

            get { return this.ReviewerValue; }

            set { SetProperty(ref ReviewerValue, value); }

        }
        private string SummaryValue = string.Empty;

        public string Summary

        {

            get { return this.SummaryValue; }

            set { SetProperty(ref SummaryValue, value); }

        }
    }

    public class PortfolioSummary : ModelEntityBase
    {
        private string PortfolioNameValue = string.Empty;

        public string PortfolioName

        {

            get { return this.PortfolioNameValue; }

            set { SetProperty(ref PortfolioNameValue, value); }

        }
        private int TotalProspectsValue;

        public int TotalProspects

        {

            get { return this.TotalProspectsValue; }

            set { SetProperty(ref TotalProspectsValue, value); }

        }
        private decimal TotalEstimatedResourcesValue;

        public decimal TotalEstimatedResources

        {

            get { return this.TotalEstimatedResourcesValue; }

            set { SetProperty(ref TotalEstimatedResourcesValue, value); }

        }
    }
}



