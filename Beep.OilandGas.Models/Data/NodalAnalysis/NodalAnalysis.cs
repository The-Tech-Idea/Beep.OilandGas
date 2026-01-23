using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for nodal analysis result.
    /// </summary>
    public class NodalAnalysisRunResult : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private OperatingPoint OperatingPointValue = new();

        public OperatingPoint OperatingPoint

        {

            get { return this.OperatingPointValue; }

            set { SetProperty(ref OperatingPointValue, value); }

        }
        private List<IPRPoint> IPRCurveValue = new();

        public List<IPRPoint> IPRCurve

        {

            get { return this.IPRCurveValue; }

            set { SetProperty(ref IPRCurveValue, value); }

        }
        private List<VLPPoint> VLPCurveValue = new();

        public List<VLPPoint> VLPCurve

        {

            get { return this.VLPCurveValue; }

            set { SetProperty(ref VLPCurveValue, value); }

        }
        private decimal OptimalFlowRateValue;

        public decimal OptimalFlowRate

        {

            get { return this.OptimalFlowRateValue; }

            set { SetProperty(ref OptimalFlowRateValue, value); }

        }
        private decimal OptimalBottomholePressureValue;

        public decimal OptimalBottomholePressure

        {

            get { return this.OptimalBottomholePressureValue; }

            set { SetProperty(ref OptimalBottomholePressureValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// DTO for nodal analysis parameters.
    /// </summary>
    public class NodalAnalysisParameters : ModelEntityBase
    {
        private ReservoirProperties ReservoirPropertiesValue = new();

        public ReservoirProperties ReservoirProperties

        {

            get { return this.ReservoirPropertiesValue; }

            set { SetProperty(ref ReservoirPropertiesValue, value); }

        }
        private WellboreProperties WellborePropertiesValue = new();

        public WellboreProperties WellboreProperties

        {

            get { return this.WellborePropertiesValue; }

            set { SetProperty(ref WellborePropertiesValue, value); }

        }
        private decimal MinFlowRateValue;

        public decimal MinFlowRate

        {

            get { return this.MinFlowRateValue; }

            set { SetProperty(ref MinFlowRateValue, value); }

        }
        private decimal MaxFlowRateValue;

        public decimal MaxFlowRate

        {

            get { return this.MaxFlowRateValue; }

            set { SetProperty(ref MaxFlowRateValue, value); }

        }
        private int NumberOfPointsValue = 50;

        public int NumberOfPoints

        {

            get { return this.NumberOfPointsValue; }

            set { SetProperty(ref NumberOfPointsValue, value); }

        }
    }

    /// <summary>
    /// DTO for optimization goals.
    /// </summary>
    public class OptimizationGoals : ModelEntityBase
    {
        private string OptimizationTypeValue = "MaximizeProduction";

        public string OptimizationType

        {

            get { return this.OptimizationTypeValue; }

            set { SetProperty(ref OptimizationTypeValue, value); }

        } // MaximizeProduction, MinimizePressure, OptimizeEfficiency
        private decimal? TargetFlowRateValue;

        public decimal? TargetFlowRate

        {

            get { return this.TargetFlowRateValue; }

            set { SetProperty(ref TargetFlowRateValue, value); }

        }
        private decimal? TargetBottomholePressureValue;

        public decimal? TargetBottomholePressure

        {

            get { return this.TargetBottomholePressureValue; }

            set { SetProperty(ref TargetBottomholePressureValue, value); }

        }
        public Dictionary<string, object> Constraints { get; set; } = new();
    }

     /// <summary>
     /// DTO for optimization result.
     /// </summary>
     public class OptimizationResult : ModelEntityBase
     {
         private string OptimizationIdValue = string.Empty;

         public string OptimizationId

         {

             get { return this.OptimizationIdValue; }

             set { SetProperty(ref OptimizationIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime OptimizationDateValue;

         public DateTime OptimizationDate

         {

             get { return this.OptimizationDateValue; }

             set { SetProperty(ref OptimizationDateValue, value); }

         }
         private OperatingPoint RecommendedOperatingPointValue = new();

         public OperatingPoint RecommendedOperatingPoint

         {

             get { return this.RecommendedOperatingPointValue; }

             set { SetProperty(ref RecommendedOperatingPointValue, value); }

         }
         private decimal ImprovementPercentageValue;

         public decimal ImprovementPercentage

         {

             get { return this.ImprovementPercentageValue; }

             set { SetProperty(ref ImprovementPercentageValue, value); }

         }
         private List<string> RecommendationsValue = new();

         public List<string> Recommendations

         {

             get { return this.RecommendationsValue; }

             set { SetProperty(ref RecommendationsValue, value); }

         }
     }

     /// <summary>
     /// DTO for performance matching analysis results.
     /// </summary>
     public class PerformanceMatchingAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private decimal CurrentFlowRateValue;

         public decimal CurrentFlowRate

         {

             get { return this.CurrentFlowRateValue; }

             set { SetProperty(ref CurrentFlowRateValue, value); }

         }
         private decimal CurrentBottomholePressureValue;

         public decimal CurrentBottomholePressure

         {

             get { return this.CurrentBottomholePressureValue; }

             set { SetProperty(ref CurrentBottomholePressureValue, value); }

         }
         private decimal MarginToBubblePointValue;

         public decimal MarginToBubblePoint

         {

             get { return this.MarginToBubblePointValue; }

             set { SetProperty(ref MarginToBubblePointValue, value); }

         }
         private string SurfaceBottleneckValue = string.Empty;

         public string SurfaceBottleneck

         {

             get { return this.SurfaceBottleneckValue; }

             set { SetProperty(ref SurfaceBottleneckValue, value); }

         }
         private string ReservoirBottleneckValue = string.Empty;

         public string ReservoirBottleneck

         {

             get { return this.ReservoirBottleneckValue; }

             set { SetProperty(ref ReservoirBottleneckValue, value); }

         }
         private decimal ForecastedDeclineValue;

         public decimal ForecastedDecline

         {

             get { return this.ForecastedDeclineValue; }

             set { SetProperty(ref ForecastedDeclineValue, value); }

         }
     }

     /// <summary>
     /// DTO for sensitivity analysis results.
     /// </summary>
     public class SensitivityAnalysisResult : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         public Dictionary<string, decimal> SensitivityFactors { get; set; } = new();
         private string MostSensitiveParameterValue = string.Empty;

         public string MostSensitiveParameter

         {

             get { return this.MostSensitiveParameterValue; }

             set { SetProperty(ref MostSensitiveParameterValue, value); }

         }
     }

     /// <summary>
     /// DTO for artificial lift recommendation.
     /// </summary>
     public class ArtificialLiftRecommendation : ModelEntityBase
     {
         private string RecommendationIdValue = string.Empty;

         public string RecommendationId

         {

             get { return this.RecommendationIdValue; }

             set { SetProperty(ref RecommendationIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime RecommendationDateValue;

         public DateTime RecommendationDate

         {

             get { return this.RecommendationDateValue; }

             set { SetProperty(ref RecommendationDateValue, value); }

         }
         private string PrimaryRecommendationValue = string.Empty;

         public string PrimaryRecommendation

         {

             get { return this.PrimaryRecommendationValue; }

             set { SetProperty(ref PrimaryRecommendationValue, value); }

         }
         private List<string> AlternativeRecommendationsValue = new();

         public List<string> AlternativeRecommendations

         {

             get { return this.AlternativeRecommendationsValue; }

             set { SetProperty(ref AlternativeRecommendationsValue, value); }

         }
         private decimal RecommendedCapacityValue;

         public decimal RecommendedCapacity

         {

             get { return this.RecommendedCapacityValue; }

             set { SetProperty(ref RecommendedCapacityValue, value); }

         }
         private decimal EstimatedNPVValue;

         public decimal EstimatedNPV

         {

             get { return this.EstimatedNPVValue; }

             set { SetProperty(ref EstimatedNPVValue, value); }

         }
         private List<string> RiskFactorsValue = new();

         public List<string> RiskFactors

         {

             get { return this.RiskFactorsValue; }

             set { SetProperty(ref RiskFactorsValue, value); }

         }
     }

     /// <summary>
     /// DTO for well diagnostics results.
     /// </summary>
     public class WellDiagnosticsResult : ModelEntityBase
     {
         private string DiagnosisIdValue = string.Empty;

         public string DiagnosisId

         {

             get { return this.DiagnosisIdValue; }

             set { SetProperty(ref DiagnosisIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime DiagnosisDateValue;

         public DateTime DiagnosisDate

         {

             get { return this.DiagnosisDateValue; }

             set { SetProperty(ref DiagnosisDateValue, value); }

         }
         private decimal ProductionShortfallValue;

         public decimal ProductionShortfall

         {

             get { return this.ProductionShortfallValue; }

             set { SetProperty(ref ProductionShortfallValue, value); }

         }
         private decimal ProductionShortfallPercentValue;

         public decimal ProductionShortfallPercent

         {

             get { return this.ProductionShortfallPercentValue; }

             set { SetProperty(ref ProductionShortfallPercentValue, value); }

         }
         private List<string> IdentifiedIssuesValue = new();

         public List<string> IdentifiedIssues

         {

             get { return this.IdentifiedIssuesValue; }

             set { SetProperty(ref IdentifiedIssuesValue, value); }

         }
         private List<string> RecommendedActionsValue = new();

         public List<string> RecommendedActions

         {

             get { return this.RecommendedActionsValue; }

             set { SetProperty(ref RecommendedActionsValue, value); }

         }
         private string DiagnosisStatusValue = "Analysis Required";

         public string DiagnosisStatus

         {

             get { return this.DiagnosisStatusValue; }

             set { SetProperty(ref DiagnosisStatusValue, value); }

         }
     }

     /// <summary>
     /// DTO for production forecast.
     /// </summary>
     public class ProductionForecast : ModelEntityBase
     {
         private string ForecastIdValue = string.Empty;

         public string ForecastId

         {

             get { return this.ForecastIdValue; }

             set { SetProperty(ref ForecastIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime ForecastDateValue;

         public DateTime ForecastDate

         {

             get { return this.ForecastDateValue; }

             set { SetProperty(ref ForecastDateValue, value); }

         }
         private int ForecastPeriodMonthsValue;

         public int ForecastPeriodMonths

         {

             get { return this.ForecastPeriodMonthsValue; }

             set { SetProperty(ref ForecastPeriodMonthsValue, value); }

         }
         public Dictionary<int, decimal> MonthlyProduction { get; set; } = new();
         private decimal TotalForecastedVolumeValue;

         public decimal TotalForecastedVolume

         {

             get { return this.TotalForecastedVolumeValue; }

             set { SetProperty(ref TotalForecastedVolumeValue, value); }

         }
         private decimal FinalProductionValue;

         public decimal FinalProduction

         {

             get { return this.FinalProductionValue; }

             set { SetProperty(ref FinalProductionValue, value); }

         }
         private decimal AverageMonthlyProductionValue;

         public decimal AverageMonthlyProduction

         {

             get { return this.AverageMonthlyProductionValue; }

             set { SetProperty(ref AverageMonthlyProductionValue, value); }

         }
         private int? EconomicLimitMonthValue;

         public int? EconomicLimitMonth

         {

             get { return this.EconomicLimitMonthValue; }

             set { SetProperty(ref EconomicLimitMonthValue, value); }

         }
     }

     /// <summary>
     /// DTO for pressure maintenance strategy analysis.
     /// </summary>
     public class PressureMaintenanceStrategy : ModelEntityBase
     {
         private string StrategyIdValue = string.Empty;

         public string StrategyId

         {

             get { return this.StrategyIdValue; }

             set { SetProperty(ref StrategyIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private decimal CurrentReservoirPressureValue;

         public decimal CurrentReservoirPressure

         {

             get { return this.CurrentReservoirPressureValue; }

             set { SetProperty(ref CurrentReservoirPressureValue, value); }

         }
         private decimal MarginToBubblePointValue;

         public decimal MarginToBubblePoint

         {

             get { return this.MarginToBubblePointValue; }

             set { SetProperty(ref MarginToBubblePointValue, value); }

         }
         private string RecommendedStrategyValue = string.Empty;

         public string RecommendedStrategy

         {

             get { return this.RecommendedStrategyValue; }

             set { SetProperty(ref RecommendedStrategyValue, value); }

         }
         private decimal InjectionVolumeRequiredValue;

         public decimal InjectionVolumeRequired

         {

             get { return this.InjectionVolumeRequiredValue; }

             set { SetProperty(ref InjectionVolumeRequiredValue, value); }

         }
     }
}







