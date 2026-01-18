using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Comprehensive DTO for advanced choke analysis results.
    /// Includes bean choke, venturi, and specialized flow models.
    /// </summary>
    public class AdvancedChokeAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string ChokeType { get; set; } = string.Empty; // Bean, Venturi, Orifice
        public decimal ChokeDiameter { get; set; }
        public decimal UpstreamPressure { get; set; }
        public decimal DownstreamPressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal GasFlowRate { get; set; }
        public decimal CalculatedFlowRate { get; set; }
        public decimal DischargeCoefficient { get; set; }
        public decimal PressureRatio { get; set; }
        public decimal CriticalPressureRatio { get; set; }
        public string FlowRegime { get; set; } = string.Empty; // Sonic, Subsonic
        public decimal Efficiency { get; set; }
        public string ModelUsed { get; set; } = string.Empty; // API-43, ISO 6149, etc.
    }

    /// <summary>
    /// DTO for multiphase flow choke analysis.
    /// Handles oil, water, and gas simultaneously.
    /// </summary>
    public class MultiphaseChokeAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal OilFlowRate { get; set; } // STB/day
        public decimal WaterFlowRate { get; set; } // STB/day
        public decimal GasFlowRate { get; set; } // Mscf/day
        public decimal OilDensity { get; set; } // lb/ft³
        public decimal WaterDensity { get; set; } // lb/ft³
        public decimal GasDensity { get; set; } // lb/ft³
        public decimal OilViscosity { get; set; } // cp
        public decimal WaterViscosity { get; set; } // cp
        public decimal GasViscosity { get; set; } // cp
        public decimal SurfaceTension { get; set; } // dyne/cm
        public string FlowPattern { get; set; } = string.Empty; // Bubbly, Slug, Annular, Dispersed
        public decimal MixtureDensity { get; set; }
        public decimal MixtureViscosity { get; set; }
        public decimal HomogeneousVoidFraction { get; set; } // Quality
        public decimal TotalPressureDrop { get; set; }
        public decimal AccelerationPressureDrop { get; set; }
        public decimal FrictionalPressureDrop { get; set; }
        public decimal ElevationPressureDrop { get; set; }
        public decimal DownstreamPressure { get; set; }
    }

    /// <summary>
    /// DTO for choke erosion and wear prediction.
    /// Based on sand rate, particle size, and flow velocity.
    /// </summary>
    public class ChokeErosionPredictionDto
    {
        public string PredictionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal SandProductionRate { get; set; } // lb/day
        public decimal SandParticleSize { get; set; } // microns
        public decimal ParticleVelocity { get; set; } // ft/sec
        public decimal ChokeMaterial { get; set; } // Hardness rating
        public decimal ErosionRate { get; set; } // mils/year (0.001 inch/year)
        public decimal EstimatedChokeLife { get; set; } // years
        public int DaysUntilReplacement { get; set; }
        public decimal CumulativeWearDepth { get; set; } // mils
        public string WearStatus { get; set; } = string.Empty; // Good, Fair, Poor, Critical
        public decimal ErosionSeverity { get; set; } // 0-100 scale
        public List<string> RecommendedActions { get; set; } = new();
    }

    /// <summary>
    /// DTO for choke back-pressure optimization.
    /// Analyzes production vs. pressure drop trade-offs.
    /// </summary>
    public class ChokeBackPressureOptimizationDto
    {
        public string OptimizationId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal CurrentChokeDiameter { get; set; }
        public decimal CurrentBackPressure { get; set; }
        public decimal CurrentProductionRate { get; set; }
        public decimal OptimalChokeDiameter { get; set; }
        public decimal OptimalBackPressure { get; set; }
        public decimal OptimalProductionRate { get; set; }
        public decimal ProductionIncrease { get; set; } // percent
        public decimal PressureDropReduction { get; set; } // psi
        public decimal ReservoirPressure { get; set; }
        public decimal BubblePointPressure { get; set; }
        public string OptimizationStrategy { get; set; } = string.Empty; // Maximize Production, Minimize Backpressure, etc.
        public List<ChokeOpeningPoint> OpeningCurve { get; set; } = new();
    }

    /// <summary>
    /// Individual data point for choke opening vs. production curve.
    /// </summary>
    public class ChokeOpeningPoint
    {
        public decimal ChokeDiameter { get; set; }
        public decimal ProductionRate { get; set; }
        public decimal PressureDrop { get; set; }
        public decimal Efficiency { get; set; }
    }

    /// <summary>
    /// DTO for artificial lift choke interaction analysis.
    /// Shows how choke affects ESP, GasLift, or Sucker Rod systems.
    /// </summary>
    public class LiftSystemChokeInteractionDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string LiftSystemType { get; set; } = string.Empty; // ESP, GasLift, SuckerRod, Plunger
        public decimal CurrentChokeSize { get; set; }
        public decimal CurrentDischarge { get; set; }
        public decimal LiftSystemPower { get; set; } // HP or scf/day
        public decimal RequiredHeadOrPressure { get; set; }
        public decimal ChokeBackPressure { get; set; }
        public decimal SystemEfficiency { get; set; }
        public decimal OptimalChokeSize { get; set; }
        public decimal EfficiencyGain { get; set; }
        public decimal PowerSavings { get; set; } // HP or scf/day
        public string Recommendation { get; set; } = string.Empty;
        public List<string> OperatingConstraints { get; set; } = new();
    }

    /// <summary>
    /// DTO for well nodal analysis with choke effects.
    /// Integrates IPR, VLP (tubing), and choke into unified analysis.
    /// </summary>
    public class ChokeNodalAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string WellUWI { get; set; } = string.Empty;
        public string NodalPoint { get; set; } = string.Empty; // Reservoir, Downhole, Wellhead, Surface
        public decimal ReservoirPressure { get; set; }
        public decimal BubblePointPressure { get; set; }
        public decimal TubingHeadPressure { get; set; }
        public decimal SeparatorPressure { get; set; }
        public decimal CurrentProduction { get; set; }
        public decimal OptimalProduction { get; set; }
        public decimal ChokeBackPressure { get; set; }
        public decimal TubingFrictionLoss { get; set; }
        public decimal ElevationChange { get; set; }
        public decimal AccelerationLoss { get; set; }
        public string ConstrainedBy { get; set; } = string.Empty; // Reservoir, Choke, Tubing, Separator
        public string Recommendation { get; set; } = string.Empty;
        public List<NodalPoint> NodalPointData { get; set; } = new();
    }

    /// <summary>
    /// Individual nodal point data for performance analysis.
    /// </summary>
    public class NodalPoint
    {
        public string PointName { get; set; } = string.Empty;
        public decimal Pressure { get; set; }
        public decimal FlowRate { get; set; }
        public decimal RestrictionType { get; set; } // 0=Capacity, 1=Restriction
    }

    /// <summary>
    /// DTO for pressure and temperature transient effects on choke flow.
    /// Analyzes how transient conditions affect performance.
    /// </summary>
    public class ChokeTransientAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal InitialUpstreamPressure { get; set; }
        public decimal InitialTemperature { get; set; }
        public decimal FinalUpstreamPressure { get; set; }
        public decimal FinalTemperature { get; set; }
        public decimal ChangeRate { get; set; } // psi/hour or °R/hour
        public decimal TransientDuration { get; set; } // hours
        public decimal AverageFlowRate { get; set; }
        public decimal PeakFlowRate { get; set; }
        public decimal MinimumFlowRate { get; set; }
        public decimal TemperatureEffect { get; set; } // psi equivalent
        public decimal PressureEffect { get; set; } // psi
        public List<TransientPoint> TransientCurve { get; set; } = new();
        public string TransientType { get; set; } = string.Empty; // Pressure, Temperature, Both
    }

    /// <summary>
    /// Individual data point along transient curve.
    /// </summary>
    public class TransientPoint
    {
        public decimal TimeElapsed { get; set; } // hours
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal FlowRate { get; set; }
        public decimal DischargeCoefficient { get; set; }
    }

    /// <summary>
    /// DTO for bean choke design and optimization per API RP 43.
    /// </summary>
    public class BeanChokeDesignDto
    {
        public string DesignId { get; set; } = string.Empty;
        public DateTime DesignDate { get; set; }
        public decimal DesiredFlowRate { get; set; }
        public decimal UpstreamPressure { get; set; }
        public decimal DownstreamPressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal GasSpecificGravity { get; set; }
        public string TrimMaterial { get; set; } = string.Empty; // WC, Tungsten Carbide, Steel
        public decimal RecommendedChokeDiameter { get; set; }
        public decimal MinimumChokeDiameter { get; set; }
        public decimal MaximumChokeDiameter { get; set; }
        public decimal DischargeCoefficient { get; set; }
        public decimal SurfaceArea { get; set; }
        public string RecommendedSeries { get; set; } = string.Empty; // AX, BX, CX, DX
        public decimal EstimatedErosionRate { get; set; } // mils/year
        public decimal DesignLife { get; set; } // years
        public string ManufacturerRecommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for choke opening vs. production forecasting.
    /// Predicts production decline and required choke adjustments.
    /// </summary>
    public class ChokeProductionForecastDto
    {
        public string ForecastId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string WellUWI { get; set; } = string.Empty;
        public decimal CurrentProduction { get; set; }
        public decimal CurrentChokeDiameter { get; set; }
        public decimal ReservoirDeclineRate { get; set; } // decimal/month
        public int ForecastMonths { get; set; }
        public List<ChokeProductionPoint> ProductionScenarios { get; set; } = new();
        public List<ChokeOpeningAdjustment> RecommendedAdjustments { get; set; } = new();
        public decimal RequiredChokeOpeningByMonth12 { get; set; }
        public decimal CumulativeProductionGain { get; set; } // BOPD-months
        public string Strategy { get; set; } = string.Empty; // Conservative, Moderate, Aggressive
    }

    /// <summary>
    /// Production forecast point with choke size.
    /// </summary>
    public class ChokeProductionPoint
    {
        public int Month { get; set; }
        public decimal ReservoirPressure { get; set; }
        public decimal NaturalProduction { get; set; }
        public decimal ProductionWithCurrentChoke { get; set; }
        public decimal RecommendedChokeDiameter { get; set; }
        public decimal ProductionWithOptimalChoke { get; set; }
    }

    /// <summary>
    /// Recommended choke opening adjustment.
    /// </summary>
    public class ChokeOpeningAdjustment
    {
        public int Month { get; set; }
        public decimal CurrentChokeDiameter { get; set; }
        public decimal AdjustToChokeDiameter { get; set; }
        public decimal PercentChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal ExpectedProductionChange { get; set; }
    }

    /// <summary>
    /// DTO for venturi choke analysis.
    /// Specialized model for venturi-type chokes with recovery sections.
    /// </summary>
    public class VenturiChokeAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal ThroatDiameter { get; set; }
        public decimal UpstreamDiameter { get; set; }
        public decimal DownstreamDiameter { get; set; }
        public decimal RecoveryLength { get; set; }
        public decimal UpstreamPressure { get; set; }
        public decimal ThroatPressure { get; set; }
        public decimal DownstreamPressure { get; set; }
        public decimal RecoveryPressure { get; set; }
        public decimal GasFlowRate { get; set; }
        public decimal ThroatVelocity { get; set; }
        public decimal RecoveryFraction { get; set; }
        public decimal EffectivePressureDrop { get; set; }
        public decimal CoefficientOfRecovery { get; set; }
        public string Advantage { get; set; } = string.Empty; // Lower erosion, Higher recovery, etc.
    }

    /// <summary>
    /// DTO for trim and material selection recommendations.
    /// Based on well conditions and erosion predictions.
    /// </summary>
    public class ChokeTrimSelectionDto
    {
        public string SelectionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string ChokeType { get; set; } = string.Empty; // Bean, Venturi, Orifice
        public decimal DesiredFlowRate { get; set; }
        public decimal ExpectedSandRate { get; set; }
        public decimal PredictedErosionRate { get; set; }
        public List<TrimOption> TrimOptions { get; set; } = new();
        public string RecommendedTrim { get; set; } = string.Empty;
        public string RecommendedMaterial { get; set; } = string.Empty; // WC, Tungsten Carbide, 17-4 Steel
        public decimal EstimatedChokeLife { get; set; } // years
        public decimal CostPerChoke { get; set; }
        public decimal LifetimeCost { get; set; } // $/year
        public string Justification { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual trim option for selection analysis.
    /// </summary>
    public class TrimOption
    {
        public string TrimSize { get; set; } = string.Empty; // AX, BX, CX, DX
        public decimal ChokeDiameter { get; set; }
        public decimal FlowCapacity { get; set; }
        public string Material { get; set; } = string.Empty;
        public decimal EstimatedLife { get; set; }
        public decimal Cost { get; set; }
        public decimal ErosionRating { get; set; } // 0-100, 100=Best
        public string Pros { get; set; } = string.Empty;
        public string Cons { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for choke performance monitoring and diagnostics.
    /// Identifies operational issues and performance degradation.
    /// </summary>
    public class ChokePerformanceDiagnosticsDto
    {
        public string DiagnosticsId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal MeasuredFlowRate { get; set; }
        public decimal ExpectedFlowRate { get; set; }
        public decimal FlowDeviation { get; set; } // percent
        public decimal MeasuredDownstreamPressure { get; set; }
        public decimal ExpectedDownstreamPressure { get; set; }
        public decimal PressureDeviation { get; set; } // psi
        public string StatusCode { get; set; } = string.Empty; // Normal, Warning, Critical
        public List<string> IdentifiedIssues { get; set; } = new();
        public List<string> DiagnosticsDetails { get; set; } = new();
        public decimal DischargeCoefficientDegradation { get; set; } // percent
        public string ProbableCause { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for choke sand cut risk assessment.
    /// Predicts sand production impact and migration through choke.
    /// </summary>
    public class ChokeSandCutRiskAssessmentDto
    {
        public string AssessmentId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string WellUWI { get; set; } = string.Empty;
        public decimal EstimatedSandRate { get; set; } // lb/day
        public decimal SandGrainSize { get; set; } // microns
        public decimal WellProdDepth { get; set; }
        public decimal ChokeDepth { get; set; }
        public decimal SettlingVelocity { get; set; } // ft/sec
        public decimal FlowVelocity { get; set; } // ft/sec
        public decimal SandMigrationRisk { get; set; } // 0-100 scale
        public string SandStatus { get; set; } = string.Empty; // Low, Moderate, High, Severe
        public List<string> SandMigrationPoints { get; set; } = new();
        public decimal PredictedChokeDamageRate { get; set; } // mils/year
        public int DaysUntilChokeReplacement { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for temperature effect analysis on choke flow.
    /// Shows how temperature changes impact flow rates and efficiency.
    /// </summary>
    public class ChokeTemperatureEffectAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal BaselineTemperature { get; set; }
        public decimal BaselineFlowRate { get; set; }
        public decimal TemperatureChangeRange { get; set; } // °R
        public List<TemperatureFlowPoint> TemperatureEffectCurve { get; set; } = new();
        public decimal FlowSensitivity { get; set; } // %change/°R
        public decimal PressureDropSensitivity { get; set; } // psi/°R
        public decimal DischargeCoefficientTemperatureCoeff { get; set; } // 1/°R
        public string TemperatureControlRecommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual data point for temperature vs. flow analysis.
    /// </summary>
    public class TemperatureFlowPoint
    {
        public decimal Temperature { get; set; }
        public decimal FlowRate { get; set; }
        public decimal PressureDrop { get; set; }
        public decimal DischargeCoefficient { get; set; }
        public decimal Efficiency { get; set; }
    }

    /// <summary>
    /// DTO for comprehensive choke system analysis report.
    /// Combines multiple analyses into unified well overview.
    /// </summary>
    public class ChokeSystemComprehensiveReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public string WellUWI { get; set; } = string.Empty;
        public string OperatingStatus { get; set; } = string.Empty; // Active, Shut-in, Suspended
        
        // Current conditions
        public AdvancedChokeAnalysisDto CurrentChokeAnalysis { get; set; } = new();
        public ChokePerformanceDiagnosticsDto PerformanceDiagnostics { get; set; } = new();
        public ChokeErosionPredictionDto ErosionPrediction { get; set; } = new();
        public ChokeSandCutRiskAssessmentDto SandRiskAssessment { get; set; } = new();
        
        // Optimization
        public ChokeBackPressureOptimizationDto OptimizationAnalysis { get; set; } = new();
        public ChokeProductionForecastDto ProductionForecast { get; set; } = new();
        
        // Equipment
        public ChokeTrimSelectionDto EquipmentRecommendation { get; set; } = new();
        
        // Overall summary
        public decimal CurrentProduction { get; set; }
        public decimal OptimizedProduction { get; set; }
        public decimal ProductionPotential { get; set; } // percent increase
        public string OverallHealthStatus { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
        public List<string> KeyFindings { get; set; } = new();
        public List<string> PriorityActions { get; set; } = new();
        public decimal EstimatedRevenueImpact { get; set; } // $/day
    }
}
