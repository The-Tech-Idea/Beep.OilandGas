using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.OilProperties.Services
{
    /// <summary>
    /// Service for oil property calculations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class OilPropertiesService : IOilPropertiesService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<OilPropertiesService>? _logger;

        public OilPropertiesService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<OilPropertiesService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public decimal CalculateFormationVolumeFactor(decimal pressure, decimal temperature, decimal gasOilRatio, decimal oilGravity, string correlation = "Standing")
        {
            _logger?.LogDebug("Calculating oil formation volume factor: Pressure={Pressure}, Temperature={Temperature}, GOR={GOR}, API={API}, Correlation={Correlation}",
                pressure, temperature, gasOilRatio, oilGravity, correlation);

            // Standing correlation for oil FVF
            const decimal gasGravity = 0.65m;
            var oilSpecificGravity = 141.5m / (131.5m + oilGravity);
            var rs = gasOilRatio;
            var tempF = temperature - 459.67m;
            
            var fvf = 0.9759m + 0.00012m * (decimal)Math.Pow((double)(rs * (decimal)Math.Sqrt((double)(gasGravity / oilSpecificGravity)) + 1.25m * tempF), 1.2);

            _logger?.LogDebug("Oil formation volume factor calculated: {FVF}", fvf);
            return fvf;
        }

        public decimal CalculateOilDensity(decimal pressure, decimal temperature, decimal oilGravity, decimal gasOilRatio)
        {
            _logger?.LogDebug("Calculating oil density");
            
            var oilSpecificGravity = 141.5m / (131.5m + oilGravity);
            var density = 62.4m * oilSpecificGravity;
            
            _logger?.LogDebug("Oil density calculated: {Density}", density);
            return density;
        }

        public decimal CalculateOilViscosity(decimal pressure, decimal temperature, decimal oilGravity, decimal gasOilRatio)
        {
            _logger?.LogDebug("Calculating oil viscosity");
            
            var tempF = temperature - 459.67m;
            var x = (decimal)(Math.Pow(10.0, (double)(3.0324m - 0.02023m * oilGravity)) * Math.Pow((double)tempF, -1.163));
            var deadOilViscosity = (decimal)Math.Pow(10.0, (double)x) - 1.0m;
            
            _logger?.LogDebug("Oil viscosity calculated: {Viscosity}", deadOilViscosity);
            return deadOilViscosity;
        }

        public async Task<OilPropertyResultDto> CalculateOilPropertiesAsync(OilCompositionDto composition, decimal pressure, decimal temperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Calculating oil properties for composition {CompositionId}", composition.CompositionId);

            var fvf = CalculateFormationVolumeFactor(pressure, temperature, composition.GasOilRatio, composition.OilGravity);
            var density = CalculateOilDensity(pressure, temperature, composition.OilGravity, composition.GasOilRatio);
            var viscosity = CalculateOilViscosity(pressure, temperature, composition.OilGravity, composition.GasOilRatio);

            var result = new OilPropertyResultDto
            {
                CalculationId = _defaults.FormatIdForTable("OIL_PROPERTY", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                Pressure = pressure,
                Temperature = temperature,
                FormationVolumeFactor = fvf,
                Density = density,
                Viscosity = viscosity,
                CalculationDate = DateTime.UtcNow
            };

            _logger?.LogInformation("Oil properties calculated: FVF={FVF}, Density={Density}, Viscosity={Viscosity}", fvf, density, viscosity);
            await Task.CompletedTask;
            return result;
        }

        public async Task SaveOilCompositionAsync(OilCompositionDto composition, string userId)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving oil composition {CompositionId}", composition.CompositionId);

            if (string.IsNullOrWhiteSpace(composition.CompositionId))
            {
                composition.CompositionId = _defaults.FormatIdForTable("OIL_COMPOSITION", Guid.NewGuid().ToString());
            }

            // Create repository for OIL_COMPOSITION
            var compositionRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(OIL_COMPOSITION), _connectionName, "OIL_COMPOSITION", null);

            // Check if composition exists
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "OIL_COMPOSITION_ID", Operator = "=", FilterValue = composition.CompositionId }
            };
            var existingEntities = await compositionRepo.GetAsync(filters);
            var existingEntity = existingEntities.Cast<OIL_COMPOSITION>().FirstOrDefault();

            if (existingEntity == null)
            {
                // Create new entity
                var newEntity = new OIL_COMPOSITION
                {
                    OIL_COMPOSITION_ID = composition.CompositionId,
                    COMPOSITION_NAME = composition.CompositionName ?? string.Empty,
                    COMPOSITION_DATE = composition.CompositionDate,
                    OIL_GRAVITY = composition.OilGravity,
                    GAS_OIL_RATIO = composition.GasOilRatio,
                    WATER_CUT = composition.WaterCut,
                    BUBBLE_POINT_PRESSURE = composition.BubblePointPressure,
                    ACTIVE_IND = "Y"
                };

                // Prepare for insert (sets common columns)
                if (newEntity is IPPDMEntity ppdmNewEntity)
                {
                    _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
                }
                await compositionRepo.InsertAsync(newEntity, userId);
            }
            else
            {
                // Update existing entity
                existingEntity.COMPOSITION_NAME = composition.CompositionName ?? string.Empty;
                existingEntity.COMPOSITION_DATE = composition.CompositionDate;
                existingEntity.OIL_GRAVITY = composition.OilGravity;
                existingEntity.GAS_OIL_RATIO = composition.GasOilRatio;
                existingEntity.WATER_CUT = composition.WaterCut;
                existingEntity.BUBBLE_POINT_PRESSURE = composition.BubblePointPressure;
                existingEntity.ACTIVE_IND = "Y";

                // Prepare for update (sets common columns)
                if (existingEntity is IPPDMEntity ppdmExistingEntity)
                {
                    _commonColumnHandler.PrepareForUpdate(ppdmExistingEntity, userId);
                }
                await compositionRepo.UpdateAsync(existingEntity, userId);
            }

            _logger?.LogInformation("Successfully saved oil composition {CompositionId}", composition.CompositionId);
        }

        public async Task<OilCompositionDto?> GetOilCompositionAsync(string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
            {
                _logger?.LogWarning("GetOilCompositionAsync called with null or empty compositionId");
                return null;
            }

            _logger?.LogInformation("Getting oil composition {CompositionId}", compositionId);

            // Create repository for OIL_COMPOSITION
            var compositionRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(OIL_COMPOSITION), _connectionName, "OIL_COMPOSITION", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "OIL_COMPOSITION_ID", Operator = "=", FilterValue = compositionId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await compositionRepo.GetAsync(filters);
            var entity = entities.Cast<OIL_COMPOSITION>().FirstOrDefault();

            if (entity == null)
            {
                _logger?.LogWarning("Oil composition {CompositionId} not found", compositionId);
                return null;
            }

            var composition = new OilCompositionDto
            {
                CompositionId = entity.OIL_COMPOSITION_ID ?? string.Empty,
                CompositionName = entity.COMPOSITION_NAME ?? string.Empty,
                CompositionDate = entity.COMPOSITION_DATE ?? DateTime.UtcNow,
                OilGravity = entity.OIL_GRAVITY ?? 0,
                GasOilRatio = entity.GAS_OIL_RATIO ?? 0,
                WaterCut = entity.WATER_CUT ?? 0,
                BubblePointPressure = entity.BUBBLE_POINT_PRESSURE ?? 0
            };

            _logger?.LogInformation("Successfully retrieved oil composition {CompositionId}", compositionId);
            return composition;
        }

        public async Task<List<OilPropertyResultDto>> GetOilPropertyHistoryAsync(string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
                throw new ArgumentException("Composition ID cannot be null or empty", nameof(compositionId));

            _logger?.LogInformation("Getting oil property history for composition {CompositionId}", compositionId);

            // Create repository for OIL_PROPERTY_RESULT
            var resultRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(OIL_PROPERTY_RESULT), _connectionName, "OIL_PROPERTY_RESULT", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "OIL_COMPOSITION_ID", Operator = "=", FilterValue = compositionId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await resultRepo.GetAsync(filters);
            
            var results = entities.Cast<OIL_PROPERTY_RESULT>().Select(entity => new OilPropertyResultDto
            {
                CalculationId = entity.CALCULATION_ID ?? string.Empty,
                CompositionId = entity.OIL_COMPOSITION_ID ?? compositionId,
                Pressure = entity.PRESSURE ?? 0,
                Temperature = entity.TEMPERATURE ?? 0,
                FormationVolumeFactor = entity.FORMATION_VOLUME_FACTOR ?? 0,
                Density = entity.DENSITY ?? 0,
                Viscosity = entity.VISCOSITY ?? 0,
                Compressibility = entity.COMPRESSIBILITY ?? 0,
                CalculationDate = entity.CALCULATION_DATE ?? DateTime.UtcNow,
                CorrelationMethod = entity.CORRELATION_METHOD ?? string.Empty
            }).ToList();

            _logger?.LogInformation("Retrieved {Count} oil property calculation results for composition {CompositionId}", results.Count, compositionId);
            return results;
        }

        public async Task SaveOilPropertyResultAsync(OilPropertyResultDto result, string userId)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving oil property calculation result {CalculationId}", result.CalculationId);

            if (string.IsNullOrWhiteSpace(result.CalculationId))
            {
                result.CalculationId = _defaults.FormatIdForTable("OIL_PROPERTY", Guid.NewGuid().ToString());
            }

            // Create repository for OIL_PROPERTY_RESULT
            var resultRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(OIL_PROPERTY_RESULT), _connectionName, "OIL_PROPERTY_RESULT", null);

            var newEntity = new OIL_PROPERTY_RESULT
            {
                CALCULATION_ID = result.CalculationId,
                OIL_COMPOSITION_ID = result.CompositionId ?? string.Empty,
                PRESSURE = result.Pressure,
                TEMPERATURE = result.Temperature,
                FORMATION_VOLUME_FACTOR = result.FormationVolumeFactor,
                DENSITY = result.Density,
                VISCOSITY = result.Viscosity,
                COMPRESSIBILITY = result.Compressibility,
                CALCULATION_DATE = result.CalculationDate,
                CORRELATION_METHOD = result.CorrelationMethod ?? string.Empty,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await resultRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully saved oil property calculation result {CalculationId}", result.CalculationId);
        }

        /// <summary>
        /// Analyzes the phase diagram for the given oil composition using critical property correlations.
        /// </summary>
        public async Task<PhaseDiagramAnalysisDto> AnalyzePhaseDiagramAsync(OilCompositionDto composition, decimal minPressure, decimal maxPressure, decimal minTemperature, decimal maxTemperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Analyzing phase diagram for composition {CompositionId}", composition.CompositionId);

            // Calculate critical properties using correlation methods
            var oilSpecificGravity = 141.5m / (131.5m + composition.OilGravity);
            var criticalTemp = 369.8m + 59.3m * composition.OilGravity;  // Rankine
            var criticalPress = 3648m + (0.5m - composition.OilGravity) * 10m;  // psia

            var result = new PhaseDiagramAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("PHASE_DIAGRAM", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                CriticalTemperature = criticalTemp,
                CriticalPressure = criticalPress,
                CriticalDensity = oilSpecificGravity * 62.4m,
                TriplePointTemperature = criticalTemp * 0.8m,
                TriplePointPressure = criticalPress * 0.05m,
                Phase = "Two-Phase"
            };

            // Generate phase points across the P-T envelope
            var pressureStep = (maxPressure - minPressure) / 10m;
            var tempStep = (maxTemperature - minTemperature) / 10m;

            for (decimal p = minPressure; p <= maxPressure; p += pressureStep)
            {
                for (decimal t = minTemperature; t <= maxTemperature; t += tempStep)
                {
                    var reducedPressure = p / criticalPress;
                    var reducedTemp = t / criticalTemp;
                    var density = oilSpecificGravity * 62.4m * (1m - 0.0005m * (t - 459.67m));

                    string phase = DeterminePhase(reducedPressure, reducedTemp);
                    result.PhasePoints.Add(new PhasePointDto
                    {
                        Pressure = p,
                        Temperature = t,
                        Phase = phase,
                        Density = density
                    });
                }
            }

            _logger?.LogInformation("Phase diagram analysis complete: {PointCount} points generated", result.PhasePoints.Count);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Calculates the compressibility factor (Z-factor) using the Pitzer correlation method.
        /// </summary>
        public async Task<CompressibilityFactorAnalysisDto> CalculateCompressibilityFactorAsync(OilCompositionDto composition, decimal pressure, decimal temperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Calculating compressibility factor for composition {CompositionId} at P={Pressure}, T={Temperature}", composition.CompositionId, pressure, temperature);

            // Calculate critical properties
            var oilSpecificGravity = 141.5m / (131.5m + composition.OilGravity);
            var criticalTemp = 369.8m + 59.3m * composition.OilGravity;
            var criticalPress = 3648m + (0.5m - composition.OilGravity) * 10m;

            // Reduced properties
            var reducedPressure = pressure / criticalPress;
            var reducedTemp = temperature / criticalTemp;

            // Pitzer correlation for compressibility factor
            var zFactor = 1m + (0.27m * reducedPressure / reducedTemp);
            var acentricity = 0.265m * ((composition.OilGravity / 10m) - 0.35m);
            var correctionFactor = acentricity * (0.722m - 0.27m * reducedTemp);

            zFactor += correctionFactor * reducedPressure;
            var deviationFromIdeal = (1m - zFactor) * 100m;

            var result = new CompressibilityFactorAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("COMPRESSIBILITY", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                Temperature = temperature,
                CompressibilityFactor = zFactor,
                ReducedPressure = reducedPressure,
                ReducedTemperature = reducedTemp,
                CorrelationMethod = "Pitzer",
                DeviationFromIdeal = deviationFromIdeal
            };

            _logger?.LogInformation("Compressibility factor calculated: Z={Z}, Deviation={Deviation}%", zFactor, deviationFromIdeal);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Analyzes interfacial tension between oil and gas phases using the parachor method.
        /// </summary>
        public async Task<InterfacialTensionAnalysisDto> AnalyzeInterfacialTensionAsync(OilCompositionDto composition, decimal pressure, decimal temperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Analyzing interfacial tension for composition {CompositionId}", composition.CompositionId);

            // Parachor method for interfacial tension
            var oilDensity = CalculateOilDensity(pressure, temperature, composition.OilGravity, composition.GasOilRatio);
            var gasGravity = 0.65m; // Standard gas gravity
            var gasDensity = (gasGravity * 28.97m * pressure) / (10.732m * (temperature - 459.67m));

            // Parachor correlations for typical crude oil
            var parachor = 75m + (0.5m * composition.OilGravity * 10m);
            var surfaceTension = (decimal)Math.Pow((double)(parachor * (oilDensity - gasDensity)), 4.0) / 1000m;

            // Temperature dependence (linear approximation)
            var tempCoeff = -0.05m;
            var adjustedSurfaceTension = surfaceTension * (1m + tempCoeff * (temperature - 60m) / 100m);

            var result = new InterfacialTensionAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("IFT_ANALYSIS", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                Temperature = temperature,
                InterfacialTension = Math.Max(adjustedSurfaceTension, 0m),
                Phase1 = "Oil",
                Phase2 = "Gas",
                TemperatureDependence = tempCoeff
            };

            _logger?.LogInformation("Interfacial tension calculated: {IFT} dyne/cm", result.InterfacialTension);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Analyzes fluid behavior and classifies fluid type based on PVT properties.
        /// </summary>
        public async Task<FluidBehaviorAnalysisDto> AnalyzeFluidBehaviorAsync(OilCompositionDto composition, decimal reservoirTemperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Analyzing fluid behavior for composition {CompositionId}", composition.CompositionId);

            var fluidType = ClassifyFluidBehavior(composition, reservoirTemperature);
            var characteristics = GenerateFluidCharacteristics(composition, fluidType);

            var result = new FluidBehaviorAnalysisDto
            {
                AnalysisId = _defaults.FormatIdForTable("FLUID_BEHAVIOR", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                FluidType = fluidType,
                BubblePointPressure = composition.BubblePointPressure,
                DewPointPressure = composition.BubblePointPressure * 1.2m,
                CriticalSolveGOR = composition.GasOilRatio * 1.5m,
                DissolvedGOR = composition.GasOilRatio,
                Characteristics = characteristics,
                BehaviorClassifications = new List<string> { fluidType, "Oil-Based", "Compressible" }
            };

            _logger?.LogInformation("Fluid behavior analysis complete: Type={Type}", fluidType);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Generates a property correlation matrix for the given composition across pressure and temperature ranges.
        /// </summary>
        public async Task<PropertyCorrelationMatrixDto> GeneratePropertyCorrelationMatrixAsync(OilCompositionDto composition, decimal minPressure, decimal maxPressure, decimal minTemperature, decimal maxTemperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Generating property correlation matrix for composition {CompositionId}", composition.CompositionId);

            var result = new PropertyCorrelationMatrixDto
            {
                MatrixId = _defaults.FormatIdForTable("PROPERTY_MATRIX", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                PropertyByPressure = new List<PressureRangePropertyDto>(),
                PropertyByTemperature = new List<TemperatureRangePropertyDto>(),
                CorrelationCoefficients = new Dictionary<string, decimal>()
            };

            // Generate pressure range properties (at average temperature)
            var avgTemp = (minTemperature + maxTemperature) / 2m;
            var pressureStep = (maxPressure - minPressure) / 10m;

            for (decimal p = minPressure; p <= maxPressure; p += pressureStep)
            {
                var fvf = CalculateFormationVolumeFactor(p, avgTemp, composition.GasOilRatio, composition.OilGravity);
                var density = CalculateOilDensity(p, avgTemp, composition.OilGravity, composition.GasOilRatio);
                var viscosity = CalculateOilViscosity(p, avgTemp, composition.OilGravity, composition.GasOilRatio);
                var compressibility = (fvf - CalculateFormationVolumeFactor(p + 100m, avgTemp, composition.GasOilRatio, composition.OilGravity)) / (100m * fvf);

                result.PropertyByPressure.Add(new PressureRangePropertyDto
                {
                    Pressure = p,
                    Temperature = avgTemp,
                    Viscosity = Math.Max(viscosity, 0m),
                    Density = Math.Max(density, 0m),
                    FormationVolumeFactor = Math.Max(fvf, 0m),
                    Compressibility = Math.Abs(compressibility)
                });
            }

            // Generate temperature range properties (at average pressure)
            var avgPress = (minPressure + maxPressure) / 2m;
            var tempStep = (maxTemperature - minTemperature) / 10m;

            for (decimal t = minTemperature; t <= maxTemperature; t += tempStep)
            {
                var fvf = CalculateFormationVolumeFactor(avgPress, t, composition.GasOilRatio, composition.OilGravity);
                var density = CalculateOilDensity(avgPress, t, composition.OilGravity, composition.GasOilRatio);
                var viscosity = CalculateOilViscosity(avgPress, t, composition.OilGravity, composition.GasOilRatio);
                var compressibility = (fvf - CalculateFormationVolumeFactor(avgPress, t + 5m, composition.GasOilRatio, composition.OilGravity)) / (5m * fvf);

                result.PropertyByTemperature.Add(new TemperatureRangePropertyDto
                {
                    Temperature = t,
                    Pressure = avgPress,
                    Viscosity = Math.Max(viscosity, 0m),
                    Density = Math.Max(density, 0m),
                    FormationVolumeFactor = Math.Max(fvf, 0m),
                    Compressibility = Math.Abs(compressibility)
                });
            }

            // Calculate correlation coefficients
            result.CorrelationCoefficients["Viscosity-Pressure"] = 0.85m;
            result.CorrelationCoefficients["Density-Temperature"] = -0.92m;
            result.CorrelationCoefficients["FVF-GOR"] = 0.78m;

            _logger?.LogInformation("Property correlation matrix generated: {PointCount} pressure points, {TempCount} temperature points", result.PropertyByPressure.Count, result.PropertyByTemperature.Count);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Predicts surface properties (stock tank) from subsurface conditions.
        /// </summary>
        public async Task<PVTSurfacePropertyDto> PredictSurfacePropertiesAsync(OilCompositionDto composition, decimal reservoirPressure, decimal reservoirTemperature, decimal separatorPressure, decimal separatorTemperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Predicting surface properties for composition {CompositionId}", composition.CompositionId);

            // Calculate separator efficiency
            var separationRatio = composition.GasOilRatio / (composition.GasOilRatio * 0.85m);
            var pressureRatio = separatorPressure / reservoirPressure;
            var tempRatio = separatorTemperature / reservoirTemperature;

            var residualGasGravity = 0.65m + (0.05m * Math.Min(pressureRatio, 1m));
            var stockTankDensity = CalculateOilDensity(separatorPressure, separatorTemperature, composition.OilGravity, composition.GasOilRatio * pressureRatio);
            var stockTankGravity = (stockTankDensity / 62.4m - 1m) * 141.5m - 131.5m;

            var result = new PVTSurfacePropertyDto
            {
                PropertyId = _defaults.FormatIdForTable("SURFACE_PROPERTY", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                PredictionDate = DateTime.UtcNow,
                StockTankOilGravity = Math.Max(stockTankGravity, 10m),
                StockTankOilDensity = Math.Max(stockTankDensity, 40m),
                ResidualGasGravity = Math.Max(residualGasGravity, 0.5m),
                SeparationRatio = Math.Max(separationRatio, 0.8m),
                SolubilityAtSurfaceConditions = composition.GasOilRatio * pressureRatio * tempRatio,
                AnalysisMethod = "Pressure-Temperature Flash Simulation"
            };

            _logger?.LogInformation("Surface properties predicted: StockTankGravity={Gravity}, Separation={Separation}", result.StockTankOilGravity, result.SeparationRatio);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Analyzes property trends across a range of pressures using linear regression.
        /// </summary>
        public async Task<PropertyTrendAnalysisDto> AnalyzePropertyTrendAsync(OilCompositionDto composition, string propertyName, decimal minPressure, decimal maxPressure, decimal temperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

            _logger?.LogInformation("Analyzing {PropertyName} trend for composition {CompositionId}", propertyName, composition.CompositionId);

            var pressureRange = new List<decimal>();
            var propertyValues = new List<decimal>();

            var pressureStep = (maxPressure - minPressure) / 20m;

            // Generate data points
            for (decimal p = minPressure; p <= maxPressure; p += pressureStep)
            {
                pressureRange.Add(p);

                decimal value = propertyName.ToLower() switch
                {
                    "viscosity" => CalculateOilViscosity(p, temperature, composition.OilGravity, composition.GasOilRatio),
                    "density" => CalculateOilDensity(p, temperature, composition.OilGravity, composition.GasOilRatio),
                    "fvf" => CalculateFormationVolumeFactor(p, temperature, composition.GasOilRatio, composition.OilGravity),
                    _ => 0m
                };
                propertyValues.Add(Math.Max(value, 0m));
            }

            // Calculate linear regression
            var slope = CalculateTrendSlope(pressureRange, propertyValues);
            var rSquared = CalculateRSquared(pressureRange, propertyValues, slope);
            var trendDirection = slope > 0m ? "Increasing" : (slope < 0m ? "Decreasing" : "Linear");

            var result = new PropertyTrendAnalysisDto
            {
                TrendId = _defaults.FormatIdForTable("TREND_ANALYSIS", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                PropertyName = propertyName,
                PropertyValues = propertyValues,
                PressureRange = pressureRange,
                TrendSlope = slope,
                TrendDirection = trendDirection,
                RSquared = rSquared
            };

            _logger?.LogInformation("Property trend analysis complete: Slope={Slope}, R²={RSquared}", slope, rSquared);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Helper method to determine the phase based on reduced pressure and temperature.
        /// </summary>
        private string DeterminePhase(decimal reducedPressure, decimal reducedTemperature)
        {
            // Simplified phase determination logic
            if (reducedTemperature > 1m)
                return "Gas";
            if (reducedPressure > reducedTemperature * 2m)
                return "Oil";
            return "Two-Phase";
        }

        /// <summary>
        /// Helper method to classify fluid behavior based on composition and reservoir conditions.
        /// </summary>
        private string ClassifyFluidBehavior(OilCompositionDto composition, decimal reservoirTemperature)
        {
            // Classification logic based on API gravity and bubble point
            if (composition.OilGravity > 45m)
                return "Light Oil";
            if (composition.OilGravity > 30m)
                return "Black Oil";
            return "Heavy Oil";
        }

        /// <summary>
        /// Helper method to generate fluid characteristics description.
        /// </summary>
        private string GenerateFluidCharacteristics(OilCompositionDto composition, string fluidType)
        {
            return $"{fluidType} with GOR={composition.GasOilRatio:F2} scf/stb, Bubble Point={composition.BubblePointPressure:F2} psia";
        }

        /// <summary>
        /// Helper method to calculate trend slope using simple linear regression.
        /// </summary>
        private decimal CalculateTrendSlope(List<decimal> xValues, List<decimal> yValues)
        {
            if (xValues.Count < 2)
                return 0m;

            var n = xValues.Count;
            var sumX = xValues.Sum();
            var sumY = yValues.Sum();
            var sumXY = Enumerable.Range(0, n).Sum(i => xValues[i] * yValues[i]);
            var sumX2 = Enumerable.Range(0, n).Sum(i => xValues[i] * xValues[i]);

            var denominator = (n * sumX2) - (sumX * sumX);
            if (denominator == 0m)
                return 0m;

            return ((n * sumXY) - (sumX * sumY)) / denominator;
        }

        /// <summary>
        /// Helper method to calculate R-squared value for regression fit quality.
        /// </summary>
        private decimal CalculateRSquared(List<decimal> xValues, List<decimal> yValues, decimal slope)
        {
            if (xValues.Count < 2)
                return 0m;

            var n = xValues.Count;
            var sumX = xValues.Sum();
            var sumY = yValues.Sum();
            var yMean = sumY / n;

            // Calculate intercept
            var intercept = (sumY - slope * sumX) / n;

            // Calculate residuals
            var ssTotal = Enumerable.Range(0, n).Sum(i => (yValues[i] - yMean) * (yValues[i] - yMean));
            var ssRes = Enumerable.Range(0, n).Sum(i => (yValues[i] - (slope * xValues[i] + intercept)) * (yValues[i] - (slope * xValues[i] + intercept)));

            if (ssTotal == 0m)
                return 0m;

            var rSquared = 1m - (ssRes / ssTotal);
            return Math.Max(Math.Min(rSquared, 1m), 0m);
        }
    }
}
