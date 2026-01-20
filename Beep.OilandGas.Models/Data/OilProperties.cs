using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for oil composition.
    /// </summary>
    public class OilComposition : ModelEntityBase
    {
        public string CompositionId { get; set; } = string.Empty;
        public string CompositionName { get; set; } = string.Empty;
        public DateTime CompositionDate { get; set; }
        public decimal OilGravity { get; set; } // API gravity
        public decimal GasOilRatio { get; set; } // scf/stb
        public decimal WaterCut { get; set; } // fraction
        public decimal BubblePointPressure { get; set; } // psia
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for oil property calculation result.
    /// </summary>
    public class OilPropertyResult : ModelEntityBase
    {
        public string CalculationId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal FormationVolumeFactor { get; set; }
        public decimal Density { get; set; }
        public decimal Viscosity { get; set; }
        public decimal Compressibility { get; set; }
        public DateTime CalculationDate { get; set; }
        public string CorrelationMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request for calculating Formation Volume Factor (FVF)
    /// </summary>
    public class CalculateFVFRequest : ModelEntityBase
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity { get; set; }

        public string Correlation { get; set; } = "Standing";
    }

    /// <summary>
    /// Request for calculating oil density
    /// </summary>
    public class CalculateDensityRequest : ModelEntityBase
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio { get; set; }
    }

    /// <summary>
    /// Request for calculating oil viscosity
    /// </summary>
    public class CalculateViscosityRequest : ModelEntityBase
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio { get; set; }
    }

    /// <summary>
    /// Request for calculating comprehensive oil properties
    /// </summary>
    public class CalculateOilPropertiesRequest : ModelEntityBase
    {
        [Required(ErrorMessage = "Composition is required")]
        public OilComposition Composition { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
         public decimal Temperature { get; set; }
     }

     /// <summary>
     /// DTO for phase diagram analysis
     /// </summary>
     public class PhaseDiagramAnalysis : ModelEntityBase
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string CompositionId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public decimal CriticalTemperature { get; set; }
         public decimal CriticalPressure { get; set; }
         public decimal CriticalDensity { get; set; }
         public decimal TriplePointTemperature { get; set; }
         public decimal TriplePointPressure { get; set; }
         public List<PhasePoint> PhasePoints { get; set; } = new();
         public string Phase { get; set; } = string.Empty; // Single Phase, Two-Phase, Three-Phase
     }

     /// <summary>
     /// DTO for phase envelope point
     /// </summary>
     public class PhasePoint : ModelEntityBase
     {
         public decimal Pressure { get; set; }
         public decimal Temperature { get; set; }
         public string Phase { get; set; } = string.Empty; // Gas, Oil, Two-Phase
         public decimal Density { get; set; }
     }

     /// <summary>
     /// DTO for compressibility factor analysis
     /// </summary>
     public class CompressibilityFactorAnalysis : ModelEntityBase
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string CompositionId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public decimal Pressure { get; set; }
         public decimal Temperature { get; set; }
         public decimal CompressibilityFactor { get; set; }
         public decimal ReducedPressure { get; set; }
         public decimal ReducedTemperature { get; set; }
         public string CorrelationMethod { get; set; } = string.Empty;
         public decimal DeviationFromIdeal { get; set; }
     }

     /// <summary>
     /// DTO for interfacial tension analysis
     /// </summary>
     public class InterfacialTensionAnalysis : ModelEntityBase
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string CompositionId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public decimal Pressure { get; set; }
         public decimal Temperature { get; set; }
         public decimal InterfacialTension { get; set; } // dyne/cm
         public string Phase1 { get; set; } = string.Empty;
         public string Phase2 { get; set; } = string.Empty;
         public decimal TemperatureDependence { get; set; }
     }

     /// <summary>
     /// DTO for fluid behavior analysis
     /// </summary>
     public class FluidBehaviorAnalysis : ModelEntityBase
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string CompositionId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public string FluidType { get; set; } = string.Empty; // Black Oil, Volatile Oil, Condensate, etc.
         public decimal BubblePointPressure { get; set; }
         public decimal DewPointPressure { get; set; }
         public decimal CriticalSolveGOR { get; set; }
         public decimal DissolvedGOR { get; set; }
         public string Characteristics { get; set; } = string.Empty;
         public List<string> BehaviorClassifications { get; set; } = new();
     }

     /// <summary>
     /// DTO for property correlation matrix
     /// </summary>
     public class PropertyCorrelationMatrix : ModelEntityBase
     {
         public string MatrixId { get; set; } = string.Empty;
         public string CompositionId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public List<PressureRangeProperty> PropertyByPressure { get; set; } = new();
         public List<TemperatureRangeProperty> PropertyByTemperature { get; set; } = new();
         public Dictionary<string, decimal> CorrelationCoefficients { get; set; } = new();
     }

     /// <summary>
     /// DTO for property variation with pressure
     /// </summary>
     public class PressureRangeProperty : ModelEntityBase
     {
         public decimal Pressure { get; set; }
         public decimal Temperature { get; set; }
         public decimal Viscosity { get; set; }
         public decimal Density { get; set; }
         public decimal FormationVolumeFactor { get; set; }
         public decimal Compressibility { get; set; }
     }

     /// <summary>
     /// DTO for property variation with temperature
     /// </summary>
     public class TemperatureRangeProperty : ModelEntityBase
     {
         public decimal Temperature { get; set; }
         public decimal Pressure { get; set; }
         public decimal Viscosity { get; set; }
         public decimal Density { get; set; }
         public decimal FormationVolumeFactor { get; set; }
         public decimal Compressibility { get; set; }
     }

     /// <summary>
     /// DTO for PVT surface property prediction
     /// </summary>
     public class PVTSurfaceProperty : ModelEntityBase
     {
         public string PropertyId { get; set; } = string.Empty;
         public string CompositionId { get; set; } = string.Empty;
         public DateTime PredictionDate { get; set; }
         public decimal StockTankOilGravity { get; set; }
         public decimal StockTankOilDensity { get; set; }
         public decimal ResidualGasGravity { get; set; }
         public decimal SeparationRatio { get; set; }
         public decimal SolubilityAtSurfaceConditions { get; set; }
         public string AnalysisMethod { get; set; } = string.Empty;
     }

     /// <summary>
     /// DTO for property trend analysis
     /// </summary>
     public class PropertyTrendAnalysis : ModelEntityBase
     {
         public string TrendId { get; set; } = string.Empty;
         public string CompositionId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public string PropertyName { get; set; } = string.Empty;
         public List<decimal> PropertyValues { get; set; } = new();
         public List<decimal> PressureRange { get; set; } = new();
         public decimal TrendSlope { get; set; }
         public string TrendDirection { get; set; } = string.Empty; // Increasing, Decreasing, Linear
         public decimal RSquared { get; set; } // Fit quality
     }
}





