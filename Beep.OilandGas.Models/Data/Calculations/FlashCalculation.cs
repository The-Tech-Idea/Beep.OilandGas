using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Request for multi-stage flash calculation
    /// </summary>
    public class MultiStageFlashRequest : ModelEntityBase
    {
        /// <summary>
        /// Flash conditions
        /// </summary>
        [Required(ErrorMessage = "Conditions are required")]
        public FlashConditions Conditions { get; set; } = null!;

        /// <summary>
        /// Number of flash stages
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "Stages must be between 1 and 100")]
        public int Stages { get; set; }
    }

    /// <summary>
    /// DTO for PVT envelope analysis
    /// </summary>
    public class PVTEnvelopeAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal MinPressure { get; set; }
        public decimal MaxPressure { get; set; }
        public decimal MinTemperature { get; set; }
        public decimal MaxTemperature { get; set; }
        public decimal BubblePointPressure { get; set; }
        public decimal BubblePointTemperature { get; set; }
        public decimal DewPointPressure { get; set; }
        public decimal DewPointTemperature { get; set; }
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public List<EnvelopePoint> EnvelopePoints { get; set; } = new();
        public string EnvelopeType { get; set; } = string.Empty; // Type I, Type II, Type III, Type IV
    }

    /// <summary>
    /// DTO for individual envelope point
    /// </summary>
    public class EnvelopePoint : ModelEntityBase
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public string PhaseRegion { get; set; } = string.Empty;
        public decimal VaporFraction { get; set; }
    }

    /// <summary>
    /// DTO for bubble point analysis
    /// </summary>
    public class BubblePointAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal BubblePointPressure { get; set; }
        public Dictionary<string, decimal> LiquidComposition { get; set; } = new();
        public Dictionary<string, decimal> KValues { get; set; } = new();
        public int Iterations { get; set; }
        public bool Converged { get; set; }
        public decimal ConvergenceError { get; set; }
    }

    /// <summary>
    /// DTO for dew point analysis
    /// </summary>
    public class DewPointAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal DewPointPressure { get; set; }
        public Dictionary<string, decimal> VaporComposition { get; set; } = new();
        public Dictionary<string, decimal> KValues { get; set; } = new();
        public int Iterations { get; set; }
        public bool Converged { get; set; }
        public decimal ConvergenceError { get; set; }
    }

    /// <summary>
    /// DTO for flash calculation result with extended analysis
    /// </summary>
    public class FlashCalculationPropertyResult : ModelEntityBase
    {
        public string CalculationId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal VaporFraction { get; set; }
        public decimal LiquidFraction { get; set; }
        public Dictionary<string, decimal> VaporComposition { get; set; } = new();
        public Dictionary<string, decimal> LiquidComposition { get; set; } = new();
        public Dictionary<string, decimal> KValues { get; set; } = new();
        public int Iterations { get; set; }
        public bool Converged { get; set; }
        public decimal ConvergenceError { get; set; }
    }

    /// <summary>
    /// DTO for separator design and simulation
    /// </summary>
    public class SeparatorSimulation : ModelEntityBase
    {
        public string SimulationId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime SimulationDate { get; set; }
        public decimal InletPressure { get; set; }
        public decimal InletTemperature { get; set; }
        public decimal SeparatorPressure { get; set; }
        public decimal SeparatorTemperature { get; set; }
        public decimal GasOilRatio { get; set; }
        public decimal LiquidRecovery { get; set; }
        public decimal GasRecovery { get; set; }
        public List<SeparatorStage> Stages { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual separator stage
    /// </summary>
    public class SeparatorStage : ModelEntityBase
    {
        public int StageNumber { get; set; }
        public decimal StagePressure { get; set; }
        public decimal StageTemperature { get; set; }
        public decimal VaporFraction { get; set; }
        public decimal LiquidRecoveryFraction { get; set; }
    }

    /// <summary>
    /// DTO for pressure-temperature phase diagram
    /// </summary>
    public class PhaseDiagram : ModelEntityBase
    {
        public string DiagramId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime GenerationDate { get; set; }
        public decimal MinPressure { get; set; }
        public decimal MaxPressure { get; set; }
        public decimal MinTemperature { get; set; }
        public decimal MaxTemperature { get; set; }
        public List<PhaseRegion> PhaseRegions { get; set; } = new();
    }

    /// <summary>
    /// DTO for phase region in diagram
    /// </summary>
    public class PhaseRegion : ModelEntityBase
    {
        public string RegionName { get; set; } = string.Empty; // Single Phase, Two-Phase, Three-Phase
        public List<decimal> Pressures { get; set; } = new();
        public List<decimal> Temperatures { get; set; } = new();
    }

    /// <summary>
    /// DTO for stability analysis result
    /// </summary>
    public class StabilityAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public bool IsStable { get; set; }
        public decimal TangentPlaneDistance { get; set; }
        public string StabilityStatus { get; set; } = string.Empty; // Stable, Unstable, Critical
        public Dictionary<string, decimal> CriticalComposition { get; set; } = new();
    }

    /// <summary>
    /// DTO for equilibrium constant analysis
    /// </summary>
    public class EquilibriumConstantAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public Dictionary<string, decimal> KValues { get; set; } = new();
        public string CorrelationMethod { get; set; } = string.Empty;
    }
}




