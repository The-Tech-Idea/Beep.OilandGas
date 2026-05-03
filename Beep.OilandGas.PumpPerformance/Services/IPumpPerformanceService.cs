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
        /// Calculates pump head-flow (H-Q) curve efficiencies (η per point).
        /// Delegates to <see cref="Beep.OilandGas.PumpPerformance.Calculations.EfficiencyCalculations.CalculateOverallEfficiency"/>; throws
        /// <see cref="Beep.OilandGas.PumpPerformance.Exceptions.InvalidInputException"/> or <see cref="ArgumentNullException"/> when arrays fail validation.
        /// </summary>
        /// <param name="flowRates">Array of flow rates (GPM)</param>
        /// <param name="heads">Array of heads (feet)</param>
        /// <param name="power">Array of brake horsepower values</param>
        /// <param name="specificGravity">Fluid specific gravity (default 1.0)</param>
        /// <returns>Array of efficiency values</returns>
        Task<double[]> CalculateHQCurveAsync(double[] flowRates, double[] heads, double[] power, double specificGravity = 1.0);

        /// <summary>
        /// Calculates the empirical C-factor used with the bundled power-law pump model:
        /// C = P_motor / Q₀³ (motor input power in HP, Q₀ in GPM). Then P(Q) = C·Q³ and H(Q) = H₀·(Q/Q₀)²
        /// along a generated curve (see <see cref="Beep.OilandGas.PumpPerformance.PumpPerformanceCalc.CFactorCalc"/>).
        /// This is not an IPR productivity index or a vendor-specific "C" from pump datasheets.
        /// </summary>
        /// <param name="motorInputPower">Motor input power (HP).</param>
        /// <param name="flowRate">Reference flow rate Q₀ (GPM).</param>
        /// <param name="head">Reference head H₀ (ft); unused by this formula but validated for API symmetry.</param>
        /// <returns>C-factor (HP / GPM³).</returns>
        /// <exception cref="Beep.OilandGas.PumpPerformance.Exceptions.InvalidInputException">When motor power, flow, or head fail strict-positive validation.</exception>
        Task<double> CalculateCFactorAsync(double motorInputPower, double flowRate, double head);

        /// <summary>
        /// Generates synthetic H–Q–power points using the C-factor power law (see <see cref="Beep.OilandGas.PumpPerformance.PumpPerformanceCalc"/>).
        /// </summary>
        /// <param name="pumpId">Pump identifier</param>
        /// <param name="baseFlowRate">Base flow rate (GPM)</param>
        /// <param name="baseHead">Base head (ft)</param>
        /// <param name="motorInputPower">Motor input power (HP)</param>
        /// <param name="specificGravity">Fluid SG for efficiency points (default 1.0)</param>
        /// <returns>Performance curve data</returns>
        /// <exception cref="Beep.OilandGas.PumpPerformance.Exceptions.InvalidInputException">When numeric inputs fail validation.</exception>
        /// <exception cref="ArgumentException">When <paramref name="pumpId"/> is null or whitespace.</exception>
        Task<PumpPerformanceCurve> GeneratePerformanceCurveAsync(
            string pumpId,
            double baseFlowRate,
            double baseHead,
            double motorInputPower,
            double specificGravity = 1.0);

        /// <summary>
        /// Analyzes pump performance at a specific operating point.
        /// </summary>
        /// <param name="operatingPoint">Operating point data</param>
        /// <returns>Performance analysis result</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="operatingPoint"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="operatingPoint"/>.PumpId is null or whitespace.</exception>
        /// <exception cref="Beep.OilandGas.PumpPerformance.Exceptions.InvalidInputException">When flow, head, BHP, or SG fail validation.</exception>
        Task<PumpPerformanceAnalysis> AnalyzePerformanceAsync(PumpOperatingPoint operatingPoint);

        /// <summary>
        /// Optimizes pump performance based on system requirements.
        /// </summary>
        /// <param name="requirements">System requirements</param>
        /// <returns>Optimization recommendations</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="requirements"/> is null.</exception>
        /// <exception cref="Beep.OilandGas.PumpPerformance.Exceptions.InvalidInputException">When flow, head, or SG fail validation.</exception>
        Task<PumpOptimization> OptimizePerformanceAsync(PumpSystemRequirements requirements);

        /// <summary>
        /// Gets pump efficiency at a specific point: η = Q·H·SG / (k·BHP) with k = <c>PumpConstants.HorsepowerConversionFactor</c> (US field units).
        /// </summary>
        /// <param name="flowRate">Flow rate (GPM).</param>
        /// <param name="head">Head (feet).</param>
        /// <param name="power">Brake horsepower.</param>
        /// <param name="specificGravity">Fluid specific gravity (default 1.0).</param>
        /// <returns>Efficiency (0–1).</returns>
        /// <exception cref="Beep.OilandGas.PumpPerformance.Exceptions.InvalidInputException">When inputs fail validation.</exception>
        Task<double> GetEfficiencyAsync(double flowRate, double head, double power, double specificGravity = 1.0);

        /// <summary>
        /// Calculates power requirement for a given flow and head.
        /// </summary>
        /// <param name="flowRate">Flow rate (GPM)</param>
        /// <param name="head">Head (feet)</param>
        /// <param name="specificGravity">Specific gravity</param>
        /// <param name="efficiency">Pump efficiency</param>
        /// <returns>Required brake horsepower</returns>
        /// <exception cref="Beep.OilandGas.PumpPerformance.Exceptions.InvalidInputException">When inputs fail validation.</exception>
        Task<double> CalculatePowerRequirementAsync(double flowRate, double head, double specificGravity = 1.0, double efficiency = 0.75);

        /// <summary>
        /// Validates pump performance data without throwing for common issues; inspect <see cref="PumpValidationResult"/> flags and messages.
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
        /// <summary>Fluid specific gravity used when computing per-point efficiencies.</summary>
        public double SpecificGravity { get; set; } = 1.0;
        public string CurveType { get; set; } = "HQ";
    }

    /// <summary>
    /// DTO for a single pump operating point used with <see cref="IPumpPerformanceService.AnalyzePerformanceAsync"/> (BHP and SG).
    /// </summary>
    /// <remarks>
    /// Not the same type as <see cref="Beep.OilandGas.PumpPerformance.SystemAnalysis.PumpOperatingPoint"/> used in multi-pump / system-curve analysis (different shape and namespace).
    /// </remarks>
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
