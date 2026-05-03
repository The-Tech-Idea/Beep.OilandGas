using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Validation;
using Microsoft.Extensions.Logging;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.Services
{
    /// <summary>
    /// Service for pump performance analysis and calculations.
    /// Implements H-Q curves, efficiency calculations, and performance optimization.
    /// </summary>
    public class PumpPerformanceService : IPumpPerformanceService
    {
        private readonly ILogger<PumpPerformanceService>? _logger;

        public PumpPerformanceService(ILogger<PumpPerformanceService>? logger = null)
        {
            _logger = logger;
        }

        public async Task<double[]> CalculateHQCurveAsync(double[] flowRates, double[] heads, double[] power, double specificGravity = 1.0)
        {
            _logger?.LogInformation("Calculating H-Q curve with {DataPoints} points", flowRates?.Length ?? 0);
            await Task.CompletedTask;
            return EfficiencyCalculations.CalculateOverallEfficiency(flowRates, heads, power, specificGravity);
        }

        public async Task<double> CalculateCFactorAsync(double motorInputPower, double flowRate, double head)
        {
            PumpDataValidator.ValidateStrictlyPositivePower(motorInputPower, nameof(motorInputPower));
            PumpDataValidator.ValidateStrictlyPositiveFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateStrictlyPositiveHead(head, nameof(head));

            _logger?.LogInformation("Calculating C-Factor for motor power {Power}, flow {Flow}, head {Head}", motorInputPower, flowRate, head);

            await Task.CompletedTask;
            return motorInputPower / Math.Pow(flowRate, 3);
        }

        public async Task<PumpPerformanceCurve> GeneratePerformanceCurveAsync(
            string pumpId,
            double baseFlowRate,
            double baseHead,
            double motorInputPower,
            double specificGravity = 1.0)
        {
            if (string.IsNullOrWhiteSpace(pumpId))
                throw new ArgumentException("Pump ID is required", nameof(pumpId));
            PumpDataValidator.ValidateStrictlyPositiveFlowRate(baseFlowRate, nameof(baseFlowRate));
            PumpDataValidator.ValidateStrictlyPositiveHead(baseHead, nameof(baseHead));
            PumpDataValidator.ValidateStrictlyPositivePower(motorInputPower, nameof(motorInputPower));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));

            _logger?.LogInformation("Generating performance curve for pump {PumpId}", pumpId);

            double[] flowRates = new double[9];
            for (int i = 0; i < 9; i++)
            {
                flowRates[i] = baseFlowRate * (0.8 + (i * 0.05));
            }

            double cFactor = await CalculateCFactorAsync(motorInputPower, baseFlowRate, baseHead);

            double[] heads = flowRates.Select(q => baseHead * Math.Pow(q / baseFlowRate, 2)).ToArray();

            double[] powers = flowRates.Select(q => cFactor * Math.Pow(q, 3)).ToArray();

            double[] efficiencies = await CalculateHQCurveAsync(flowRates, heads, powers, specificGravity);

            _logger?.LogInformation("Performance curve generated with {Points} data points", flowRates.Length);

            return new PumpPerformanceCurve
            {
                PumpId = pumpId,
                AnalysisDate = DateTime.UtcNow,
                FlowRates = flowRates,
                Heads = heads,
                Efficiencies = efficiencies,
                RequiredPower = powers,
                CFactor = cFactor,
                SpecificGravity = specificGravity,
                CurveType = "HQ"
            };
        }

        public async Task<PumpPerformanceAnalysis> AnalyzePerformanceAsync(PumpOperatingPoint operatingPoint)
        {
            if (operatingPoint == null)
                throw new ArgumentNullException(nameof(operatingPoint));
            if (string.IsNullOrWhiteSpace(operatingPoint.PumpId))
                throw new ArgumentException("Pump ID is required", nameof(operatingPoint));

            PumpDataValidator.ValidateStrictlyPositiveFlowRate(operatingPoint.FlowRate, nameof(operatingPoint.FlowRate));
            PumpDataValidator.ValidateStrictlyPositiveHead(operatingPoint.Head, nameof(operatingPoint.Head));
            PumpDataValidator.ValidateStrictlyPositivePower(operatingPoint.BrakeHorsepower, nameof(operatingPoint.BrakeHorsepower));
            PumpDataValidator.ValidateSpecificGravity(operatingPoint.SpecificGravity, nameof(operatingPoint.SpecificGravity));

            _logger?.LogInformation("Analyzing performance for pump {PumpId} at Q={Flow} GPM, H={Head} ft",
                operatingPoint.PumpId, operatingPoint.FlowRate, operatingPoint.Head);

            double actualEfficiency = (operatingPoint.FlowRate * operatingPoint.Head * operatingPoint.SpecificGravity)
                / (HorsepowerConversionFactor * operatingPoint.BrakeHorsepower);

            double theoreticalEfficiency = DefaultCentrifugalTheoreticalEfficiency;
            double deviation = theoreticalEfficiency - actualEfficiency;

            string status = "Normal";
            var warnings = new List<string>();
            var recommendations = new List<string>();

            if (actualEfficiency < 0.60)
            {
                status = "Warning";
                warnings.Add("Pump efficiency is below 60%");
                recommendations.Add("Check for cavitation or suction conditions");
            }

            if (deviation > 0.15)
            {
                status = "Warning";
                warnings.Add("Efficiency deviation from theoretical is significant");
                recommendations.Add("Inspect pump for wear or mechanical issues");
            }

            await Task.CompletedTask;

            return new PumpPerformanceAnalysis
            {
                AnalysisId = Guid.NewGuid().ToString(),
                PumpId = operatingPoint.PumpId,
                AnalysisDate = DateTime.UtcNow,
                ActualEfficiency = Math.Max(0, Math.Min(1, actualEfficiency)),
                TheoreticalEfficiency = theoreticalEfficiency,
                EfficiencyDeviation = deviation,
                Status = status,
                Warnings = warnings,
                Recommendations = recommendations
            };
        }

        public async Task<PumpOptimization> OptimizePerformanceAsync(PumpSystemRequirements requirements)
        {
            if (requirements == null)
                throw new ArgumentNullException(nameof(requirements));
            PumpDataValidator.ValidateStrictlyPositiveFlowRate(requirements.DesiredFlowRate, nameof(requirements.DesiredFlowRate));
            PumpDataValidator.ValidateStrictlyPositiveHead(requirements.SystemHead, nameof(requirements.SystemHead));
            PumpDataValidator.ValidateSpecificGravity(requirements.SpecificGravity, nameof(requirements.SpecificGravity));

            _logger?.LogInformation("Optimizing pump performance for well {WellId}, desired flow {Flow} GPM",
                requirements.WellId, requirements.DesiredFlowRate);

            double requiredPower = await CalculatePowerRequirementAsync(
                requirements.DesiredFlowRate,
                requirements.SystemHead,
                requirements.SpecificGravity,
                0.75);

            string recommendedPumpType = requirements.PumpType;
            double efficiencyImprovement = 0.05;
            double powerReduction = requiredPower * 0.10;

            if (requirements.AvailablePower < requiredPower)
            {
                recommendedPumpType = "Multistage Centrifugal";
                powerReduction = requiredPower * 0.15;
            }

            var optimizationActions = new List<string>
            {
                $"Install {recommendedPumpType} pump",
                "Optimize suction line design",
                "Minimize friction losses",
                "Check and optimize impeller clearances"
            };

            await Task.CompletedTask;

            return new PumpOptimization
            {
                OptimizationId = Guid.NewGuid().ToString(),
                OptimizationDate = DateTime.UtcNow,
                CurrentPumpType = requirements.PumpType,
                RecommendedPumpType = recommendedPumpType,
                ExpectedEfficiencyImprovement = efficiencyImprovement,
                ExpectedPowerReduction = powerReduction,
                OptimizationActions = optimizationActions,
                ConfidenceScore = 0.85
            };
        }

        public async Task<double> GetEfficiencyAsync(double flowRate, double head, double power, double specificGravity = 1.0)
        {
            PumpDataValidator.ValidateStrictlyPositiveFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateStrictlyPositiveHead(head, nameof(head));
            PumpDataValidator.ValidateStrictlyPositivePower(power, nameof(power));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));

            _logger?.LogInformation("Getting efficiency for Q={Flow}, H={Head}, BHP={Power}", flowRate, head, power);

            double efficiency = (flowRate * head * specificGravity) / (HorsepowerConversionFactor * power);

            await Task.CompletedTask;
            return Math.Max(0, Math.Min(1, efficiency));
        }

        public async Task<double> CalculatePowerRequirementAsync(double flowRate, double head, double specificGravity = 1.0, double efficiency = 0.75)
        {
            PumpDataValidator.ValidateStrictlyPositiveFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateStrictlyPositiveHead(head, nameof(head));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));
            PumpDataValidator.ValidateEfficiency(efficiency, nameof(efficiency));

            _logger?.LogInformation("Calculating power requirement for Q={Flow}, H={Head}, SG={SG}, η={Efficiency}",
                flowRate, head, specificGravity, efficiency);

            double bhp = (flowRate * head * specificGravity) / (HorsepowerConversionFactor * efficiency);

            await Task.CompletedTask;
            return bhp;
        }

        public async Task<PumpValidationResult> ValidatePerformanceDataAsync(double[] flowRates, double[] heads, double[] powers)
        {
            var result = new PumpValidationResult();

            if (flowRates == null || flowRates.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("Flow rates array is null or empty");
                return result;
            }

            if (heads == null || heads.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("Heads array is null or empty");
                return result;
            }

            if (powers == null || powers.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("Powers array is null or empty");
                return result;
            }

            if (flowRates.Length != heads.Length || flowRates.Length != powers.Length)
            {
                result.IsValid = false;
                result.Errors.Add("All arrays must have the same length");
                return result;
            }

            int validPoints = 0;
            for (int i = 0; i < flowRates.Length; i++)
            {
                if (flowRates[i] <= 0)
                    result.Warnings.Add($"Point {i}: Invalid flow rate {flowRates[i]}");
                else if (heads[i] <= 0)
                    result.Warnings.Add($"Point {i}: Invalid head {heads[i]}");
                else if (powers[i] <= 0)
                    result.Warnings.Add($"Point {i}: Invalid power {powers[i]}");
                else
                    validPoints++;
            }

            result.IsValid = validPoints > 0;
            result.ValidDataPoints = validPoints;
            result.TotalDataPoints = flowRates.Length;

            _logger?.LogInformation("Validation complete: {Valid}/{Total} valid data points", validPoints, flowRates.Length);

            await Task.CompletedTask;
            return result;
        }
    }
}
