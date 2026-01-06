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
}




