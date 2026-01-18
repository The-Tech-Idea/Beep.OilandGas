using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.NodalAnalysis;
using Beep.OilandGas.Models.NodalAnalysis;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for nodal analysis result.
    /// </summary>
    public class NodalAnalysisResultDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public OperatingPoint OperatingPoint { get; set; } = new();
        public List<IPRPoint> IPRCurve { get; set; } = new();
        public List<VLPPoint> VLPCurve { get; set; } = new();
        public decimal OptimalFlowRate { get; set; }
        public decimal OptimalBottomholePressure { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for nodal analysis parameters.
    /// </summary>
    public class NodalAnalysisParametersDto
    {
        public ReservoirProperties ReservoirProperties { get; set; } = new();
        public WellboreProperties WellboreProperties { get; set; } = new();
        public decimal MinFlowRate { get; set; }
        public decimal MaxFlowRate { get; set; }
        public int NumberOfPoints { get; set; } = 50;
    }

    /// <summary>
    /// DTO for optimization goals.
    /// </summary>
    public class OptimizationGoalsDto
    {
        public string OptimizationType { get; set; } = "MaximizeProduction"; // MaximizeProduction, MinimizePressure, OptimizeEfficiency
        public decimal? TargetFlowRate { get; set; }
        public decimal? TargetBottomholePressure { get; set; }
        public Dictionary<string, object> Constraints { get; set; } = new();
    }

     /// <summary>
     /// DTO for optimization result.
     /// </summary>
     public class OptimizationResultDto
     {
         public string OptimizationId { get; set; } = string.Empty;
         public string WellUWI { get; set; } = string.Empty;
         public DateTime OptimizationDate { get; set; }
         public OperatingPoint RecommendedOperatingPoint { get; set; } = new();
         public decimal ImprovementPercentage { get; set; }
         public List<string> Recommendations { get; set; } = new();
     }

     /// <summary>
     /// DTO for performance matching analysis results.
     /// </summary>
     public class PerformanceMatchingAnalysisDto
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string WellUWI { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public decimal CurrentFlowRate { get; set; }
         public decimal CurrentBottomholePressure { get; set; }
         public decimal MarginToBubblePoint { get; set; }
         public string SurfaceBottleneck { get; set; } = string.Empty;
         public string ReservoirBottleneck { get; set; } = string.Empty;
         public decimal ForecastedDecline { get; set; }
     }

     /// <summary>
     /// DTO for sensitivity analysis results.
     /// </summary>
     public class SensitivityAnalysisResultDto
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string WellUWI { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public Dictionary<string, decimal> SensitivityFactors { get; set; } = new();
         public string MostSensitiveParameter { get; set; } = string.Empty;
     }

     /// <summary>
     /// DTO for artificial lift recommendation.
     /// </summary>
     public class ArtificialLiftRecommendationDto
     {
         public string RecommendationId { get; set; } = string.Empty;
         public string WellUWI { get; set; } = string.Empty;
         public DateTime RecommendationDate { get; set; }
         public string PrimaryRecommendation { get; set; } = string.Empty;
         public List<string> AlternativeRecommendations { get; set; } = new();
         public decimal RecommendedCapacity { get; set; }
         public decimal EstimatedNPV { get; set; }
         public List<string> RiskFactors { get; set; } = new();
     }

     /// <summary>
     /// DTO for well diagnostics results.
     /// </summary>
     public class WellDiagnosticsResultDto
     {
         public string DiagnosisId { get; set; } = string.Empty;
         public string WellUWI { get; set; } = string.Empty;
         public DateTime DiagnosisDate { get; set; }
         public decimal ProductionShortfall { get; set; }
         public decimal ProductionShortfallPercent { get; set; }
         public List<string> IdentifiedIssues { get; set; } = new();
         public List<string> RecommendedActions { get; set; } = new();
         public string DiagnosisStatus { get; set; } = "Analysis Required";
     }

     /// <summary>
     /// DTO for production forecast.
     /// </summary>
     public class ProductionForecastDto
     {
         public string ForecastId { get; set; } = string.Empty;
         public string WellUWI { get; set; } = string.Empty;
         public DateTime ForecastDate { get; set; }
         public int ForecastPeriodMonths { get; set; }
         public Dictionary<int, decimal> MonthlyProduction { get; set; } = new();
         public decimal TotalForecastedVolume { get; set; }
         public decimal FinalProduction { get; set; }
         public decimal AverageMonthlyProduction { get; set; }
         public int? EconomicLimitMonth { get; set; }
     }

     /// <summary>
     /// DTO for pressure maintenance strategy analysis.
     /// </summary>
     public class PressureMaintenanceStrategyDto
     {
         public string StrategyId { get; set; } = string.Empty;
         public string WellUWI { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public decimal CurrentReservoirPressure { get; set; }
         public decimal MarginToBubblePoint { get; set; }
         public string RecommendedStrategy { get; set; } = string.Empty;
         public decimal InjectionVolumeRequired { get; set; }
     }
}




