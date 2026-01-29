using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PumpPerformance.Services
{
    /// <summary>
    /// Service interface for pump performance analysis and calculations.
    /// Provides methods for H-Q curves, efficiency calculations, and performance optimization.
    /// </summary>
    public interface IPumpPerformanceService
    {
        /// <summary>
        /// Calculates pump head-flow (H-Q) curve.
        /// </summary>
        /// <param name="flowRates">Array of flow rates (GPM)</param>
        /// <param name="heads">Array of heads (feet)</param>
        /// <param name="power">Array of brake horsepower values</param>
        /// <param name="specificGravity">Fluid specific gravity (default 1.0)</param>
        /// <returns>Array of efficiency values</returns>
        Task<double[]> CalculateHQCurveAsync(double[] flowRates, double[] heads, double[] power, double specificGravity = 1.0);

        /// <summary>
        /// Calculates C-Factor from motor input power.
        /// </summary>
        /// <param name="motorInputPower">Motor input power</param>
        /// <param name="flowRate">Base flow rate (Q₀)</param>
        /// <param name="head">Base head (H₀)</param>
        /// <returns>C-Factor value</returns>
        Task<double> CalculateCFactorAsync(double motorInputPower, double flowRate, double head);

        /// <summary>
        /// Generates pump performance curve data.
        /// </summary>
        /// <param name="pumpId">Pump identifier</param>
        /// <param name="baseFlowRate">Base flow rate</param>
        /// <param name="baseHead">Base head</param>
        /// <param name="motorInputPower">Motor input power</param>
        /// <returns>Performance curve data</returns>
        Task<PumpPerformanceCurve> GeneratePerformanceCurveAsync(
            string pumpId,
            double baseFlowRate,
            double baseHead,
            double motorInputPower);

        /// <summary>
        /// Analyzes pump performance at a specific operating point.
        /// </summary>
        /// <param name="operatingPoint">Operating point data</param>
        /// <returns>Performance analysis result</returns>
        Task<PumpPerformanceAnalysis> AnalyzePerformanceAsync(PumpOperatingPoint operatingPoint);

        /// <summary>
        /// Optimizes pump performance based on system requirements.
        /// </summary>
        /// <param name="requirements">System requirements</param>
        /// <returns>Optimization recommendations</returns>
        Task<PumpOptimization> OptimizePerformanceAsync(PumpSystemRequirements requirements);

        /// <summary>
        /// Gets pump efficiency at a specific point.
        /// </summary>
        /// <param name="flowRate">Flow rate (GPM)</param>
        /// <param name="head">Head (feet)</param>
        /// <param name="power">Brake horsepower</param>
        /// <returns> EFFICIENCY value (0-1)</returns>
        Task<double> GetEfficiencyAsync(double flowRate, double head, double power);

        /// <summary>
        /// Calculates power requirement for a given flow and head.
        /// </summary>
        /// <param name="flowRate">Flow rate (GPM)</param>
        /// <param name="head">Head (feet)</param>
        /// <param name="specificGravity">Specific gravity</param>
        /// <param name="efficiency">Pump efficiency</param>
        /// <returns>Required brake horsepower</returns>
        Task<double> CalculatePowerRequirementAsync(double flowRate, double head, double specificGravity = 1.0, double efficiency = 0.75);

        /// <summary>
        /// Validates pump performance data.
        /// </summary>
        /// <param name="flowRates">Array of flow rates</param>
        /// <param name="heads">Array of heads</param>
        /// <param name="powers">Array of powers</param>
        /// <returns>Validation result</returns>
        Task<PumpValidationResult> ValidatePerformanceDataAsync(double[] flowRates, double[] heads, double[] powers);
    }

    /// <summary>
    /// DTO for pump H-Q curve performance analysis.
    /// </summary>
    public class PumpPerformanceCurve
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public double[] FlowRates { get; set; } = Array.Empty<double>();
        public double[] Heads { get; set; } = Array.Empty<double>();
        public double[] Efficiencies { get; set; } = Array.Empty<double>();
        public double[] RequiredPower { get; set; } = Array.Empty<double>();
        public double CFactor { get; set; }
        public string CurveType { get; set; } = "HQ";
    }

    /// <summary>
    /// DTO for pump operating point.
    /// </summary>
    public class PumpOperatingPoint
    {
        public string PumpId { get; set; } = string.Empty;
        public double FlowRate { get; set; }
        public double Head { get; set; }
        public double BrakeHorsepower { get; set; }
        public double SpecificGravity { get; set; } = 1.0;
        public DateTime OperatingDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for pump performance analysis result.
    /// </summary>
    public class PumpPerformanceAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public double ActualEfficiency { get; set; }
        public double TheoreticalEfficiency { get; set; }
        public double EfficiencyDeviation { get; set; }
        public string Status { get; set; } = "Normal";
        public List<string> Warnings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// DTO for pump system requirements.
    /// </summary>
    public class PumpSystemRequirements
    {
        public string WellId { get; set; } = string.Empty;
        public double DesiredFlowRate { get; set; }
        public double SystemHead { get; set; }
        public double AvailablePower { get; set; }
        public double SpecificGravity { get; set; } = 1.0;
        public string FluidType { get; set; } = "Oil";
        public string PumpType { get; set; } = "Centrifugal";
    }

    /// <summary>
    /// DTO for pump optimization recommendations.
    /// </summary>
    public class PumpOptimization
    {
        public string OptimizationId { get; set; } = string.Empty;
        public DateTime OptimizationDate { get; set; }
        public string CurrentPumpType { get; set; } = string.Empty;
        public string RecommendedPumpType { get; set; } = string.Empty;
        public double ExpectedEfficiencyImprovement { get; set; }
        public double ExpectedPowerReduction { get; set; }
        public List<string> OptimizationActions { get; set; } = new();
        public double ConfidenceScore { get; set; }
    }

    /// <summary>
    /// DTO for pump data validation result.
    /// </summary>
    public class PumpValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int ValidDataPoints { get; set; }
        public int TotalDataPoints { get; set; }
    }
}
