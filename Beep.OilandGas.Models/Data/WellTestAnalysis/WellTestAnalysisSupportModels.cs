using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>DTO for multi-rate test data.</summary>
    public class MultiRateTestData : ModelEntityBase
    {
        private List<RateChange> RateChangesValue = new();

        public List<RateChange> RateChanges

        {

            get { return this.RateChangesValue; }

            set { SetProperty(ref RateChangesValue, value); }

        }
        private List<PressureTimePoint> PressureDataValue = new();

        public List<PressureTimePoint> PressureData

        {

            get { return this.PressureDataValue; }

            set { SetProperty(ref PressureDataValue, value); }

        }
    }

    /// <summary>DTO for rate change event.</summary>
    public class RateChange : ModelEntityBase
    {
        private DateTime ChangeTimeValue;

        public DateTime ChangeTime

        {

            get { return this.ChangeTimeValue; }

            set { SetProperty(ref ChangeTimeValue, value); }

        }
        private double NewFlowRateValue;

        public double NewFlowRate

        {

            get { return this.NewFlowRateValue; }

            set { SetProperty(ref NewFlowRateValue, value); }

        }
        private string? ReasonValue;

        public string? Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }

    /// <summary>DTO for variable rate data.</summary>
    public class VariableRateData : ModelEntityBase
    {
        private List<ProductionHistory> ProductionHistoryValue = new();

        public List<ProductionHistory> ProductionHistory

        {

            get { return this.ProductionHistoryValue; }

            set { SetProperty(ref ProductionHistoryValue, value); }

        }
        private List<PressureTimePoint> PressureDataValue = new();

        public List<PressureTimePoint> PressureData

        {

            get { return this.PressureDataValue; }

            set { SetProperty(ref PressureDataValue, value); }

        }
    }

    /// <summary>DTO for production history event.</summary>
    public class ProductionHistory : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private double FlowRateValue;

        public double FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
    }

    /// <summary>DTO for type curve match result.</summary>
    public class TypeCurveMatchResult : ModelEntityBase
    {
        private string MatchIdValue = string.Empty;

        public string MatchId

        {

            get { return this.MatchIdValue; }

            set { SetProperty(ref MatchIdValue, value); }

        }
        private string TypeCurveNameValue = string.Empty;

        public string TypeCurveName

        {

            get { return this.TypeCurveNameValue; }

            set { SetProperty(ref TypeCurveNameValue, value); }

        }
        private ReservoirModel ReservoirModelValue = new();

        public ReservoirModel ReservoirModel

        {

            get { return this.ReservoirModelValue; }

            set { SetProperty(ref ReservoirModelValue, value); }

        }
        private double MatchQualityValue;

        public double MatchQuality

        {

            get { return this.MatchQualityValue; }

            set { SetProperty(ref MatchQualityValue, value); }

        }
        private double PermeabilityValue;

        public double Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private double SkinFactorValue;

        public double SkinFactor

        {

            get { return this.SkinFactorValue; }

            set { SetProperty(ref SkinFactorValue, value); }

        }
        private double InitialPressureValue;

        public double InitialPressure

        {

            get { return this.InitialPressureValue; }

            set { SetProperty(ref InitialPressureValue, value); }

        }
        private List<string> ConfidenceIndicatorsValue = new();

        public List<string> ConfidenceIndicators

        {

            get { return this.ConfidenceIndicatorsValue; }

            set { SetProperty(ref ConfidenceIndicatorsValue, value); }

        }
    }

    /// <summary>DTO for well test analysis report.</summary>
    public class WellTestAnalysisReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private string GeneratedByValue = string.Empty;

        public string GeneratedBy

        {

            get { return this.GeneratedByValue; }

            set { SetProperty(ref GeneratedByValue, value); }

        }
        private List<WellTestAnalysisResult> AnalysisResultsValue = new();

        public List<WellTestAnalysisResult> AnalysisResults

        {

            get { return this.AnalysisResultsValue; }

            set { SetProperty(ref AnalysisResultsValue, value); }

        }
        private string ExecutiveSummaryValue = string.Empty;

        public string ExecutiveSummary

        {

            get { return this.ExecutiveSummaryValue; }

            set { SetProperty(ref ExecutiveSummaryValue, value); }

        }
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }

    /// <summary>DTO for test data validation result.</summary>
    public class TestDataValidationResult : ModelEntityBase
    {
        private bool IsValidValue;

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
        private List<string> WarningsValue = new();

        public List<string> Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }
        private double DataQualityScoreValue;

        public double DataQualityScore

        {

            get { return this.DataQualityScoreValue; }

            set { SetProperty(ref DataQualityScoreValue, value); }

        }
        private string DataQualityRatingValue = string.Empty;

        public string DataQualityRating

        {

            get { return this.DataQualityRatingValue; }

            set { SetProperty(ref DataQualityRatingValue, value); }

        }
    }

    /// <summary>DTO for analysis comparison result.</summary>
    public class AnalysisComparisonResult : ModelEntityBase
    {
        private string ComparisonIdValue = string.Empty;

        public string ComparisonId

        {

            get { return this.ComparisonIdValue; }

            set { SetProperty(ref ComparisonIdValue, value); }

        }
        private List<ComparisonEntry> ComparisonsValue = new();

        public List<ComparisonEntry> Comparisons

        {

            get { return this.ComparisonsValue; }

            set { SetProperty(ref ComparisonsValue, value); }

        }
        private string ConclusionValue = string.Empty;

        public string Conclusion

        {

            get { return this.ConclusionValue; }

            set { SetProperty(ref ConclusionValue, value); }

        }
    }

    /// <summary>DTO for individual comparison entry.</summary>
    public class ComparisonEntry : ModelEntityBase
    {
        private string Method1Value = string.Empty;

        public string Method1

        {

            get { return this.Method1Value; }

            set { SetProperty(ref Method1Value, value); }

        }
        private string Method2Value = string.Empty;

        public string Method2

        {

            get { return this.Method2Value; }

            set { SetProperty(ref Method2Value, value); }

        }
        private double PermeabilityDifferenceValue;

        public double PermeabilityDifference

        {

            get { return this.PermeabilityDifferenceValue; }

            set { SetProperty(ref PermeabilityDifferenceValue, value); }

        }
        private double SkinFactorDifferenceValue;

        public double SkinFactorDifference

        {

            get { return this.SkinFactorDifferenceValue; }

            set { SetProperty(ref SkinFactorDifferenceValue, value); }

        }
        private double ConfidenceLevelValue;

        public double ConfidenceLevel

        {

            get { return this.ConfidenceLevelValue; }

            set { SetProperty(ref ConfidenceLevelValue, value); }

        }
    }
}



