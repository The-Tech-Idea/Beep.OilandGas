using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.FlashCalculations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.Calculations;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.FlashCalculations.Services
{
    /// <summary>
    /// Service for flash calculation operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class FlashCalculationService : IFlashCalculationService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<FlashCalculationService>? _logger;

        public FlashCalculationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<FlashCalculationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public FlashResult PerformIsothermalFlash(FlashConditions conditions)
        {
            _logger?.LogInformation("Performing isothermal flash calculation");
            var result = FlashCalculator.PerformIsothermalFlash(conditions);
            _logger?.LogInformation("Flash calculation completed: VaporFraction={VaporFraction}, Converged={Converged}", 
                result.VaporFraction, result.Converged);
            return result;
        }

        public List<FlashResult> PerformMultiStageFlash(FlashConditions conditions, int stages)
        {
            _logger?.LogInformation("Performing multi-stage flash calculation with {Stages} stages", stages);
            var results = new List<FlashResult>();
            
            // Perform first stage
            var firstStageResult = FlashCalculator.PerformIsothermalFlash(conditions);
            results.Add(firstStageResult);

            // For subsequent stages, use liquid from previous stage as feed
            var currentFeed = conditions;
            for (int i = 1; i < stages; i++)
            {
                // Create new conditions with liquid composition from previous stage
                var nextStageConditions = new FlashConditions
                {
                    Pressure = currentFeed.Pressure,
                    Temperature = currentFeed.Temperature,
                    FeedComposition = firstStageResult.LiquidComposition.Select(kvp => 
                        new FlashComponent
                        {
                            Name = kvp.Key,
                            MoleFraction = kvp.Value
                        }).ToList()
                };

                var stageResult = FlashCalculator.PerformIsothermalFlash(nextStageConditions);
                results.Add(stageResult);
                currentFeed = nextStageConditions;
            }

            _logger?.LogInformation("Multi-stage flash calculation completed with {Stages} stages", stages);
            return results;
        }

        public async Task SaveFlashResultAsync(FlashResult result, string userId)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving flash calculation result");

            // Create repository for FLASH_CALCULATION_RESULT
            var flashRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(FLASH_CALCULATION_RESULT), _connectionName, "FLASH_CALCULATION_RESULT", null);

            var calculationId = _defaults.FormatIdForTable("FLASH_CALCULATION", Guid.NewGuid().ToString());
            
            var newEntity = new FLASH_CALCULATION_RESULT
            {
                CALCULATION_ID = calculationId,
                CALCULATION_DATE = DateTime.UtcNow,
                VAPOR_FRACTION = result.VaporFraction,
                LIQUID_FRACTION = result.LiquidFraction,
                ITERATIONS = result.Iterations,
                CONVERGED = result.Converged ? "Y" : "N",
                CONVERGENCE_ERROR = result.ConvergenceError,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await flashRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully saved flash calculation result {CalculationId}", calculationId);
        }

        public async Task<List<FlashResult>> GetFlashHistoryAsync(string? componentId = null)
        {
            _logger?.LogInformation("Getting flash calculation history for component: {ComponentId}", componentId ?? "all");

            // Create repository for FLASH_CALCULATION_RESULT
            var flashRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(FLASH_CALCULATION_RESULT), _connectionName, "FLASH_CALCULATION_RESULT", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            if (!string.IsNullOrWhiteSpace(componentId))
            {
                filters.Add(new AppFilter { FieldName = "COMPONENT_ID", Operator = "=", FilterValue = componentId });
            }

            var entities = await flashRepo.GetAsync(filters);
            var results = entities.Cast<FLASH_CALCULATION_RESULT>().Select(entity => new FlashResult
            {
                VaporFraction = entity.VAPOR_FRACTION ?? 0,
                LiquidFraction = entity.LIQUID_FRACTION ?? 0,
                Iterations = entity.ITERATIONS ?? 0,
                Converged = entity.CONVERGED == "Y",
                ConvergenceError = entity.CONVERGENCE_ERROR ?? 0,
                VaporComposition = new Dictionary<string, decimal>(),
                LiquidComposition = new Dictionary<string, decimal>(),
                KValues = new Dictionary<string, decimal>()
            }).ToList();

            _logger?.LogInformation("Retrieved {Count} flash calculation results", results.Count);
            return results;
        }

        /// <summary>
        /// Analyzes the PVT envelope for a given composition across a range of pressures and temperatures.
        /// </summary>
        public async Task<PVTEnvelopeAnalysisDto> AnalyzePVTEnvelopeAsync(List<FlashComponent> composition, decimal minPressure, decimal maxPressure, decimal minTemperature, decimal maxTemperature)
        {
            if (composition == null || composition.Count == 0)
                throw new ArgumentException("Composition cannot be null or empty", nameof(composition));

            _logger?.LogInformation("Analyzing PVT envelope for composition with {ComponentCount} components", composition.Count);

            var result = new PVTEnvelopeAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("PVT_ENVELOPE", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                MinPressure = minPressure,
                MaxPressure = maxPressure,
                MinTemperature = minTemperature,
                MaxTemperature = maxTemperature,
                EnvelopePoints = new List<EnvelopePointDto>()
            };

            // Estimate critical properties from composition
            var avgMolWeight = composition.Sum(c => c.MoleFraction * 30m) / composition.Sum(c => c.MoleFraction);
            result.CriticalPressure = 5000m - (avgMolWeight * 10m);
            result.CriticalTemperature = 400m + (avgMolWeight * 2m);

            // Generate envelope points
            var pressureStep = (maxPressure - minPressure) / 15m;
            var tempStep = (maxTemperature - minTemperature) / 15m;

            for (decimal p = minPressure; p <= maxPressure; p += pressureStep)
            {
                for (decimal t = minTemperature; t <= maxTemperature; t += tempStep)
                {
                    var conditions = new FlashConditions
                    {
                        Pressure = p,
                        Temperature = t,
                        FeedComposition = composition
                    };

                    var flashResult = PerformIsothermalFlash(conditions);
                    var phaseRegion = DeterminePhaseRegion(flashResult.VaporFraction);

                    result.EnvelopePoints.Add(new EnvelopePointDto
                    {
                        Pressure = p,
                        Temperature = t,
                        PhaseRegion = phaseRegion,
                        VaporFraction = flashResult.VaporFraction
                    });
                }
            }

            result.EnvelopeType = DetermineEnvelopeType(composition);
            _logger?.LogInformation("PVT envelope analysis complete: {PointCount} points generated, Type={Type}", result.EnvelopePoints.Count, result.EnvelopeType);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Performs bubble point calculation for the given composition.
        /// </summary>
        public async Task<BubblePointAnalysisDto> CalculateBubblePointAsync(List<FlashComponent> composition, decimal pressure)
        {
            if (composition == null || composition.Count == 0)
                throw new ArgumentException("Composition cannot be null or empty", nameof(composition));

            _logger?.LogInformation("Calculating bubble point at pressure {Pressure} psia", pressure);

            var result = new BubblePointAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("BUBBLE_POINT", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                LiquidComposition = new Dictionary<string, decimal>(),
                KValues = new Dictionary<string, decimal>()
            };

            // Iterative bubble point calculation
            var temperature = 100m; // Initial guess in Rankine
            var tolerance = 0.001m;
            int iterations = 0;
            const int maxIterations = 50;

            while (iterations < maxIterations)
            {
                var conditions = new FlashConditions
                {
                    Pressure = pressure,
                    Temperature = temperature,
                    FeedComposition = composition
                };

                var flashResult = PerformIsothermalFlash(conditions);
                var sumKx = composition.Sum(c => c.MoleFraction * (flashResult.KValues.ContainsKey(c.Name) ? flashResult.KValues[c.Name] : 1m));

                var convergenceError = Math.Abs(sumKx - 1m);
                if (convergenceError < tolerance)
                {
                    result.Converged = true;
                    result.ConvergenceError = convergenceError;
                    result.Temperature = temperature;
                    result.BubblePointPressure = pressure;
                    result.KValues = flashResult.KValues;
                    result.LiquidComposition = composition.ToDictionary(c => c.Name, c => c.MoleFraction);
                    break;
                }

                temperature -= convergenceError * 50m;
                iterations++;
            }

            result.Iterations = iterations;
            _logger?.LogInformation("Bubble point calculation complete: T={Temperature}°R, Converged={Converged}", result.Temperature, result.Converged);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Performs dew point calculation for the given composition.
        /// </summary>
        public async Task<DewPointAnalysisDto> CalculateDewPointAsync(List<FlashComponent> composition, decimal pressure)
        {
            if (composition == null || composition.Count == 0)
                throw new ArgumentException("Composition cannot be null or empty", nameof(composition));

            _logger?.LogInformation("Calculating dew point at pressure {Pressure} psia", pressure);

            var result = new DewPointAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("DEW_POINT", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                VaporComposition = new Dictionary<string, decimal>(),
                KValues = new Dictionary<string, decimal>()
            };

            // Iterative dew point calculation
            var temperature = 200m; // Initial guess in Rankine
            var tolerance = 0.001m;
            int iterations = 0;
            const int maxIterations = 50;

            while (iterations < maxIterations)
            {
                var conditions = new FlashConditions
                {
                    Pressure = pressure,
                    Temperature = temperature,
                    FeedComposition = composition
                };

                var flashResult = PerformIsothermalFlash(conditions);
                var sumYoverK = composition.Sum(c => c.MoleFraction / (flashResult.KValues.ContainsKey(c.Name) ? flashResult.KValues[c.Name] : 1m));

                var convergenceError = Math.Abs(sumYoverK - 1m);
                if (convergenceError < tolerance)
                {
                    result.Converged = true;
                    result.ConvergenceError = convergenceError;
                    result.Temperature = temperature;
                    result.DewPointPressure = pressure;
                    result.KValues = flashResult.KValues;
                    result.VaporComposition = composition.ToDictionary(c => c.Name, c => c.MoleFraction);
                    break;
                }

                temperature += convergenceError * 50m;
                iterations++;
            }

            result.Iterations = iterations;
            _logger?.LogInformation("Dew point calculation complete: T={Temperature}°R, Converged={Converged}", result.Temperature, result.Converged);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Analyzes separator design and performance for multi-stage separation.
        /// </summary>
        public async Task<SeparatorSimulationDto> SimulateSeparatorAsync(List<FlashComponent> composition, decimal inletPressure, decimal inletTemperature, int stages)
        {
            if (composition == null || composition.Count == 0)
                throw new ArgumentException("Composition cannot be null or empty", nameof(composition));

            _logger?.LogInformation("Simulating separator with {Stages} stages at inlet P={Pressure}, T={Temperature}", stages, inletPressure, inletTemperature);

            var result = new SeparatorSimulationDto
            {
                SimulationId = _defaults.FormatIdForTable("SEPARATOR_SIM", Guid.NewGuid().ToString()),
                SimulationDate = DateTime.UtcNow,
                InletPressure = inletPressure,
                InletTemperature = inletTemperature,
                Stages = new List<SeparatorStageDto>()
            };

            var stagePressures = GenerateStagePressures(inletPressure, stages);
            var stageTemperatures = GenerateStageTemperatures(inletTemperature, stages);

            decimal totalLiquidRecovery = 0m;
            decimal totalGasRecovery = 0m;

            for (int i = 0; i < stages; i++)
            {
                var stagePressure = stagePressures[i];
                var stageTemperature = stageTemperatures[i];

                var conditions = new FlashConditions
                {
                    Pressure = stagePressure,
                    Temperature = stageTemperature,
                    FeedComposition = composition
                };

                var flashResult = PerformIsothermalFlash(conditions);
                
                result.Stages.Add(new SeparatorStageDto
                {
                    StageNumber = i + 1,
                    StagePressure = stagePressure,
                    StageTemperature = stageTemperature,
                    VaporFraction = flashResult.VaporFraction,
                    LiquidRecoveryFraction = 1m - flashResult.VaporFraction
                });

                totalLiquidRecovery += (1m - flashResult.VaporFraction) / stages;
                totalGasRecovery += flashResult.VaporFraction / stages;
            }

            result.LiquidRecovery = totalLiquidRecovery;
            result.GasRecovery = totalGasRecovery;
            result.GasOilRatio = totalGasRecovery / Math.Max(totalLiquidRecovery, 0.001m);

            _logger?.LogInformation("Separator simulation complete: LiquidRecovery={Liquid}%, GasRecovery={Gas}%, GOR={GOR}", totalLiquidRecovery * 100m, totalGasRecovery * 100m, result.GasOilRatio);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Generates pressure-temperature phase diagram for the composition.
        /// </summary>
        public async Task<PhaseDiagramDto> GeneratePhaseDiagramAsync(List<FlashComponent> composition, decimal minPressure, decimal maxPressure, decimal minTemperature, decimal maxTemperature)
        {
            if (composition == null || composition.Count == 0)
                throw new ArgumentException("Composition cannot be null or empty", nameof(composition));

            _logger?.LogInformation("Generating phase diagram for composition");

            var result = new PhaseDiagramDto
            {
                DiagramId = _defaults.FormatIdForTable("PHASE_DIAGRAM", Guid.NewGuid().ToString()),
                GenerationDate = DateTime.UtcNow,
                MinPressure = minPressure,
                MaxPressure = maxPressure,
                MinTemperature = minTemperature,
                MaxTemperature = maxTemperature,
                PhaseRegions = new List<PhaseRegionDto>()
            };

            // Identify single-phase and two-phase regions
            var singlePhaseGas = new PhaseRegionDto { RegionName = "Single Phase Gas", Pressures = new List<decimal>(), Temperatures = new List<decimal>() };
            var singlePhaseOil = new PhaseRegionDto { RegionName = "Single Phase Oil", Pressures = new List<decimal>(), Temperatures = new List<decimal>() };
            var twoPhase = new PhaseRegionDto { RegionName = "Two-Phase", Pressures = new List<decimal>(), Temperatures = new List<decimal>() };

            var pressureStep = (maxPressure - minPressure) / 10m;
            var tempStep = (maxTemperature - minTemperature) / 10m;

            for (decimal p = minPressure; p <= maxPressure; p += pressureStep)
            {
                for (decimal t = minTemperature; t <= maxTemperature; t += tempStep)
                {
                    var conditions = new FlashConditions { Pressure = p, Temperature = t, FeedComposition = composition };
                    var flashResult = PerformIsothermalFlash(conditions);

                    if (flashResult.VaporFraction < 0.01m)
                    {
                        singlePhaseOil.Pressures.Add(p);
                        singlePhaseOil.Temperatures.Add(t);
                    }
                    else if (flashResult.VaporFraction > 0.99m)
                    {
                        singlePhaseGas.Pressures.Add(p);
                        singlePhaseGas.Temperatures.Add(t);
                    }
                    else
                    {
                        twoPhase.Pressures.Add(p);
                        twoPhase.Temperatures.Add(t);
                    }
                }
            }

            if (singlePhaseGas.Pressures.Count > 0) result.PhaseRegions.Add(singlePhaseGas);
            if (singlePhaseOil.Pressures.Count > 0) result.PhaseRegions.Add(singlePhaseOil);
            if (twoPhase.Pressures.Count > 0) result.PhaseRegions.Add(twoPhase);

            _logger?.LogInformation("Phase diagram generated: {RegionCount} regions identified", result.PhaseRegions.Count);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Performs stability analysis using tangent plane distance criterion.
        /// </summary>
        public async Task<StabilityAnalysisDto> AnalyzeStabilityAsync(List<FlashComponent> composition, decimal pressure, decimal temperature)
        {
            if (composition == null || composition.Count == 0)
                throw new ArgumentException("Composition cannot be null or empty", nameof(composition));

            _logger?.LogInformation("Performing stability analysis at P={Pressure}, T={Temperature}", pressure, temperature);

            var result = new StabilityAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("STABILITY_ANALYSIS", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                Temperature = temperature,
                CriticalComposition = new Dictionary<string, decimal>()
            };

            // Perform flash to get K-values
            var conditions = new FlashConditions { Pressure = pressure, Temperature = temperature, FeedComposition = composition };
            var flashResult = PerformIsothermalFlash(conditions);

            // Calculate tangent plane distance
            var tpd = composition.Sum(c => (decimal)Math.Log((double)(c.MoleFraction / Math.Max(0.0001m, 1m))) * c.MoleFraction);
            result.TangentPlaneDistance = tpd;

            // Determine stability
            const decimal stabilityCriteria = 0.001m;
            result.IsStable = Math.Abs(result.TangentPlaneDistance) < stabilityCriteria;
            result.StabilityStatus = result.IsStable ? "Stable" : (Math.Abs(result.TangentPlaneDistance) < 0.1m ? "Critical" : "Unstable");

            // Store critical composition
            foreach (var component in composition)
            {
                result.CriticalComposition[component.Name] = component.MoleFraction;
            }

            _logger?.LogInformation("Stability analysis complete: Status={Status}, TPD={TPD}", result.StabilityStatus, result.TangentPlaneDistance);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Analyzes equilibrium constants (K-values) across pressure and temperature ranges.
        /// </summary>
        public async Task<EquilibriumConstantAnalysisDto> AnalyzeEquilibriumConstantsAsync(List<FlashComponent> composition, decimal pressure, decimal temperature)
        {
            if (composition == null || composition.Count == 0)
                throw new ArgumentException("Composition cannot be null or empty", nameof(composition));

            _logger?.LogInformation("Analyzing equilibrium constants at P={Pressure}, T={Temperature}", pressure, temperature);

            var conditions = new FlashConditions { Pressure = pressure, Temperature = temperature, FeedComposition = composition };
            var flashResult = PerformIsothermalFlash(conditions);

            var result = new EquilibriumConstantAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("EQUILIB_CONST", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                Temperature = temperature,
                KValues = flashResult.KValues,
                CorrelationMethod = "Generalized"
            };

            _logger?.LogInformation("Equilibrium constants analyzed: {ComponentCount} components", result.KValues.Count);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Helper method to determine phase region based on vapor fraction.
        /// </summary>
        private string DeterminePhaseRegion(decimal vaporFraction)
        {
            if (vaporFraction < 0.01m)
                return "Single Phase Oil";
            if (vaporFraction > 0.99m)
                return "Single Phase Gas";
            return "Two-Phase";
        }

        /// <summary>
        /// Helper method to determine envelope type based on composition.
        /// </summary>
        private string DetermineEnvelopeType(List<FlashComponent> composition)
        {
            var heavyComponents = composition.Where(c => !c.Name.StartsWith("C1") && !c.Name.StartsWith("C2")).Count();
            var heavyFraction = composition.Where(c => !c.Name.StartsWith("C1") && !c.Name.StartsWith("C2")).Sum(c => c.MoleFraction);

            if (heavyFraction < 0.05m)
                return "Type I";
            if (heavyFraction < 0.2m)
                return "Type II";
            if (heavyFraction < 0.5m)
                return "Type III";
            return "Type IV";
        }

        /// <summary>
        /// Helper method to generate stage pressures for separator simulation.
        /// </summary>
        private List<decimal> GenerateStagePressures(decimal inletPressure, int stages)
        {
            var stagePressures = new List<decimal>();
            decimal pressureStep = inletPressure / (stages + 1);

            for (int i = 0; i < stages; i++)
            {
                stagePressures.Add(inletPressure - (pressureStep * (i + 1)));
            }

            return stagePressures;
        }

        /// <summary>
        /// Helper method to generate stage temperatures for separator simulation.
        /// </summary>
        private List<decimal> GenerateStageTemperatures(decimal inletTemperature, int stages)
        {
            var stageTemperatures = new List<decimal>();
            decimal tempStep = (inletTemperature - 520m) / (stages + 1);  // 520°R = 60°F surface condition

            for (int i = 0; i < stages; i++)
            {
                stageTemperatures.Add(inletTemperature - (tempStep * (i + 1)));
            }

            return stageTemperatures;
        }
    }
}
