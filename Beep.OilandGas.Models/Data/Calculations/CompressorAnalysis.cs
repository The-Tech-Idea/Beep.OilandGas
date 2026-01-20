using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// DTO for compressor performance analysis
    /// </summary>
    public class CompressorPerformance : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string CompressorType { get; set; } = string.Empty; // Centrifugal, Reciprocating
        public decimal InletPressure { get; set; }
        public decimal DischargePressure { get; set; }
        public decimal GasFlowRate { get; set; }
        public decimal Temperature { get; set; }
        public decimal CompressionRatio { get; set; }
        public decimal IsentropicEfficiency { get; set; }
        public decimal ActualEfficiency { get; set; }
        public decimal PowerRequired { get; set; }
        public decimal PolyHeatCapacityRatio { get; set; }
    }

    /// <summary>
    /// DTO for compressor design analysis
    /// </summary>
    public class CompressorDesign : ModelEntityBase
    {
        public string DesignId { get; set; } = string.Empty;
        public DateTime DesignDate { get; set; }
        public string CompressorType { get; set; } = string.Empty;
        public decimal RequiredFlowRate { get; set; }
        public decimal RequiredDischargePressure { get; set; }
        public decimal RequiredInletPressure { get; set; }
        public decimal GasSpecificGravity { get; set; }
        public decimal DesignTemperature { get; set; }
        public decimal RecommendedStages { get; set; }
        public decimal EstimatedEfficiency { get; set; }
        public decimal EstimatedPower { get; set; }
        public List<CompressorStage> Stages { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual compressor stage
    /// </summary>
    public class CompressorStage : ModelEntityBase
    {
        public int StageNumber { get; set; }
        public decimal InletPressure { get; set; }
        public decimal DischargePressure { get; set; }
        public decimal StageCompressionRatio { get; set; }
        public decimal StagePower { get; set; }
        public decimal StageEfficiency { get; set; }
    }

    /// <summary>
    /// DTO for centrifugal compressor analysis
    /// </summary>
    public class CentrifugalCompressorAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal InletFlowRate { get; set; }
        public decimal InletPressure { get; set; }
        public decimal DischargePressure { get; set; }
        public decimal ImpellerDiameter { get; set; }
        public decimal ImpellerSpeed { get; set; }
        public decimal HeadDeveloped { get; set; }
        public decimal SurgeMargin { get; set; }
        public decimal PolyIsentropicHead { get; set; }
        public decimal ActualHead { get; set; }
        public decimal StallMargin { get; set; }
        public string OperatingRegion { get; set; } = string.Empty; // Normal, Stall, Surge
    }

    /// <summary>
    /// DTO for reciprocating compressor analysis
    /// </summary>
    public class ReciprocationCompressorAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal CylinderCount { get; set; }
        public decimal BoreSize { get; set; }
        public decimal StrokeLength { get; set; }
        public decimal RPM { get; set; }
        public decimal VolumetricFlowRate { get; set; }
        public decimal DisplacementVolume { get; set; }
        public decimal VolumetricEfficiency { get; set; }
        public decimal SuctionPressure { get; set; }
        public decimal DischargePressure { get; set; }
        public decimal RodLoad { get; set; }
    }

    /// <summary>
    /// DTO for compressor efficiency analysis
    /// </summary>
    public class CompressorEfficiencyAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal IsentropicEfficiency { get; set; }
        public decimal PolyIsentropicEfficiency { get; set; }
        public decimal VolumetricEfficiency { get; set; }
        public decimal MechanicalEfficiency { get; set; }
        public decimal OverallEfficiency { get; set; }
        public decimal EfficiencyTrend { get; set; } // -1 to +1
        public string EfficiencyStatus { get; set; } = string.Empty; // Good, Fair, Poor
    }

    /// <summary>
    /// DTO for compressor maintenance prediction
    /// </summary>
    public class CompressorMaintenancePrediction : ModelEntityBase
    {
        public string PredictionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
        public int HoursUntilMaintenance { get; set; }
        public string MaintenanceType { get; set; } = string.Empty; // Minor, Major, Overhaul
        public List<string> MaintenanceItems { get; set; } = new();
        public decimal MaintenancePriority { get; set; } // 0-100 scale
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High
    }

    /// <summary>
    /// DTO for compressor pressure-flow analysis
    /// </summary>
    public class CompressorPressureFlowAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<PressureFlowPoint> PerformancePoints { get; set; } = new();
        public decimal SurgeLimit { get; set; }
        public decimal ChokingLimit { get; set; }
        public decimal OptimalFlowRate { get; set; }
        public decimal OptimalPressure { get; set; }
    }

    /// <summary>
    /// DTO for individual pressure-flow point
    /// </summary>
    public class PressureFlowPoint : ModelEntityBase
    {
        public decimal FlowRate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Power { get; set; }
        public decimal Efficiency { get; set; }
    }

    /// <summary>
    /// DTO for compressor power consumption analysis
    /// </summary>
    public class CompressorPowerAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal InletPower { get; set; }
        public decimal FrictionLosses { get; set; }
        public decimal IsothermicPower { get; set; }
        public decimal PolyIsentropicPower { get; set; }
        public decimal IsentropicPower { get; set; }
        public decimal ActualPower { get; set; }
        public decimal PowerSavings { get; set; }
        public string OptimizationRecommendation { get; set; } = string.Empty;
    }
}

