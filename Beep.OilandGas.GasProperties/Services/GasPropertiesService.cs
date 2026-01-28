using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data.GasProperties;

namespace Beep.OilandGas.GasProperties.Services
{
    /// <summary>
    /// Service for gas property calculations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class GasPropertiesService : IGasPropertiesService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<GasPropertiesService>? _logger;

        public GasPropertiesService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<GasPropertiesService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public decimal CalculateZFactor(decimal pressure, decimal temperature, decimal specificGravity, string correlation = "Standing-Katz")
        {
            _logger?.LogDebug("Calculating Z-factor: Pressure={Pressure}, Temperature={Temperature}, SpecificGravity={Gravity}, Correlation={Correlation}",
                pressure, temperature, specificGravity, correlation);
            
            // Use the existing calculator based on correlation method
            decimal zFactor;
            switch (correlation.ToLowerInvariant())
            {
                case "hall-yarborough":
                    zFactor = ZFactorCalculator.CalculateHallYarborough(pressure, temperature, specificGravity);
                    break;
                case "brill-beggs":
                    zFactor = ZFactorCalculator.CalculateBrillBeggs(pressure, temperature, specificGravity);
                    break;
                case "standing-katz":
                default:
                    zFactor = ZFactorCalculator.CalculateStandingKatz(pressure, temperature, specificGravity);
                    break;
            }
            
            _logger?.LogDebug("Z-factor calculated: {ZFactor}", zFactor);
            return zFactor;
        }

        public decimal CalculateGasDensity(decimal pressure, decimal temperature, decimal zFactor, decimal molecularWeight)
        {
            _logger?.LogDebug("Calculating gas density");
            // Gas density = (P * MW) / (Z * R * T)
            // R = 10.73 psia·ft³/(lbmol·°R)
            const decimal R = 10.73m;
            var density = (pressure * molecularWeight) / (zFactor * R * temperature);
            _logger?.LogDebug("Gas density calculated: {Density}", density);
            return density;
        }

        public decimal CalculateFormationVolumeFactor(decimal pressure, decimal temperature, decimal zFactor)
        {
            _logger?.LogDebug("Calculating formation volume factor");
            // Bg = 0.02827 * Z * T / P (ft³/scf)
            const decimal constant = 0.02827m;
            var fvf = constant * zFactor * temperature / pressure;
            _logger?.LogDebug("Formation volume factor calculated: {FVF}", fvf);
            return fvf;
        }

        public async Task SaveGasCompositionAsync(GasComposition composition, string userId)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving gas composition {CompositionId}", composition.CompositionId);

            if (string.IsNullOrWhiteSpace(composition.CompositionId))
            {
                composition.CompositionId = _defaults.FormatIdForTable("GAS_COMPOSITION", Guid.NewGuid().ToString());
            }

            // Create repository for composition
            var compositionRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GAS_COMPOSITION), _connectionName, "GAS_COMPOSITION", null);

            // Check if composition exists
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GAS_COMPOSITION_ID", Operator = "=", FilterValue = composition.CompositionId }
            };
            var existingEntities = await compositionRepo.GetAsync(filters);
            var existingEntity = existingEntities.Cast<GAS_COMPOSITION>().FirstOrDefault();

            if (existingEntity == null)
            {
                // Create new entity
                var newEntity = new GAS_COMPOSITION
                {
                    GAS_COMPOSITION_ID = composition.CompositionId,
                    COMPOSITION_NAME = composition.CompositionName ?? string.Empty,
                    COMPOSITION_DATE = composition.CompositionDate,
                    TOTAL_MOLE_FRACTION = composition.TotalMoleFraction,
                    MOLECULAR_WEIGHT = composition.MolecularWeight,
                    SPECIFIC_GRAVITY = composition.SpecificGravity,
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
                existingEntity.TOTAL_MOLE_FRACTION = composition.TotalMoleFraction;
                existingEntity.MOLECULAR_WEIGHT = composition.MolecularWeight;
                existingEntity.SPECIFIC_GRAVITY = composition.SpecificGravity;
                existingEntity.ACTIVE_IND = "Y";

                // Prepare for update (sets common columns)
                if (existingEntity is IPPDMEntity ppdmExistingEntity)
                {
                    _commonColumnHandler.PrepareForUpdate(ppdmExistingEntity, userId);
                }
                await compositionRepo.UpdateAsync(existingEntity, userId);
            }

            // Save components
            if (composition.Components != null && composition.Components.Count > 0)
            {
                // Create repository for components
                var componentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(GAS_COMPOSITION_COMPONENT), _connectionName, "GAS_COMPOSITION_COMPONENT", null);

                // Delete existing components
                var deleteFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "GAS_COMPOSITION_ID", Operator = "=", FilterValue = composition.CompositionId }
                };
                var existingComponents = await componentRepo.GetAsync(deleteFilters);
                foreach (var existingComponent in existingComponents.Cast<GAS_COMPOSITION_COMPONENT>())
                {
                    await componentRepo.DeleteAsync(existingComponent);
                }

                // Insert new components
                foreach (var component in composition.Components)
                {
                    var componentId = _defaults.FormatIdForTable("GAS_COMPONENT", Guid.NewGuid().ToString());
                    var newComponent = new GasComposition
                    {
                        ComponentId = componentId,
                        CompositionId = composition.CompositionId,
                        CompositionName = component.ComponentName ?? string.Empty,
                        MOLE_FRACTION = component.MoleFraction,
                        MOLECULAR_WEIGHT = component.MolecularWeight,
                        ACTIVE_IND = "Y"
                    };

                    // Prepare for insert (sets common columns)
                    if (newComponent is IPPDMEntity ppdmNewComponent)
                    {
                        _commonColumnHandler.PrepareForInsert(ppdmNewComponent, userId);
                    }
                    await componentRepo.InsertAsync(newComponent, userId);
                }
            }

            _logger?.LogInformation("Successfully saved gas composition {CompositionId}", composition.CompositionId);
        }

        public async Task<GasComposition?> GetGasCompositionAsync(string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
            {
                _logger?.LogWarning("GetGasCompositionAsync called with null or empty compositionId");
                return null;
            }

            _logger?.LogInformation("Getting gas composition {CompositionId}", compositionId);

            // Create repository for composition
            var compositionRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GAS_COMPOSITION), _connectionName, "GAS_COMPOSITION", null);

            // Get composition using filters
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GAS_COMPOSITION_ID", Operator = "=", FilterValue = compositionId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var compositionEntities = await compositionRepo.GetAsync(filters);
            var compositionEntity = compositionEntities.Cast<GAS_COMPOSITION>().FirstOrDefault();

            if (compositionEntity == null)
            {
                _logger?.LogWarning("Gas composition {CompositionId} not found", compositionId);
                return null;
            }

            // Map entity to DTO
            var composition = new GasComposition
            {
                CompositionId = compositionEntity.GAS_COMPOSITION_ID ?? string.Empty,
                CompositionName = compositionEntity.COMPOSITION_NAME ?? string.Empty,
                CompositionDate = compositionEntity.COMPOSITION_DATE ?? DateTime.UtcNow,
                TotalMoleFraction = compositionEntity.TOTAL_MOLE_FRACTION ?? 0,
                MolecularWeight = compositionEntity.MOLECULAR_WEIGHT ?? 0,
                SpecificGravity = compositionEntity.SPECIFIC_GRAVITY ?? 0
            };

            // Get components
            var componentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GAS_COMPOSITION_COMPONENT), _connectionName, "GAS_COMPOSITION_COMPONENT", null);

            var componentFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GAS_COMPOSITION_ID", Operator = "=", FilterValue = compositionId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var componentEntities = await componentRepo.GetAsync(componentFilters);

            if (componentEntities != null && componentEntities.Any())
            {
                composition.Components = new List<GasComponent>();
                foreach (var componentEntity in componentEntities.Cast<GAS_COMPOSITION_COMPONENT>())
                {
                    composition.Components.Add(new GasComponent
                    {
                        ComponentName = componentEntity.COMPONENT_NAME ?? string.Empty,
                        MoleFraction = componentEntity.MOLE_FRACTION ?? 0,
                        MolecularWeight = componentEntity.MOLECULAR_WEIGHT ?? 0
                    });
                }
            }

            _logger?.LogInformation("Successfully retrieved gas composition {CompositionId} with {ComponentCount} components", 
                compositionId, composition.Components?.Count ?? 0);
            
            return composition;
        }

        /// <summary>
        /// Analyzes gas viscosity using Lee-Gonzalez-Eakin correlation.
        /// </summary>
        public async Task<GasViscosityAnalysis> AnalyzeGasViscosityAsync(GasComposition composition, decimal pressure, decimal temperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Analyzing gas viscosity for composition {CompositionId} at P={Pressure}, T={Temperature}", composition.CompositionId, pressure, temperature);

            var zFactor = CalculateZFactor(pressure, temperature, composition.SpecificGravity);
            
            // Lee-Gonzalez-Eakin correlation for gas viscosity
            var molWeight = composition.MolecularWeight;
            var reducedTemp = temperature / (9.67m + 23.8m * molWeight);
            var viscosityAtSC = (0.0001m * (decimal)Math.Pow((double)molWeight, 0.5) * (decimal)Math.Pow(520.0, 1.67)) / (decimal)Math.Pow(38.4, 2.667);
            
            var viscosity = viscosityAtSC * (1m + 10.8m * (decimal)Math.Pow((double)(pressure / 1000m), 0.4));

            var result = new GasViscosityAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("GAS_VISCOSITY", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                Temperature = temperature,
                Viscosity = Math.Max(viscosity, 0.001m),
                ViscosityAtSC = viscosityAtSC,
                PressureCoefficient = 10.8m * (decimal)Math.Pow((double)(pressure / 1000m), 0.4),
                TemperatureCoefficient = 0.5m,
                CorrelationMethod = "Lee-Gonzalez-Eakin"
            };

            _logger?.LogInformation("Gas viscosity analysis complete: Viscosity={Viscosity}cp", result.Viscosity);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Analyzes gas compressibility across pressure and temperature ranges.
        /// </summary>
        public async Task<GasCompressibilityAnalysis> AnalyzeCompressibilityAsync(GasComposition composition, decimal pressure, decimal temperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Analyzing gas compressibility for composition {CompositionId}", composition.CompositionId);

            var zFactor = CalculateZFactor(pressure, temperature, composition.SpecificGravity);
            
            // Isothermal compressibility from Z-factor derivative
            var dP = 10m;
            var zFactorPlus = CalculateZFactor(pressure + dP, temperature, composition.SpecificGravity);
            var isothermalCompressibility = -((zFactorPlus - zFactor) / (zFactor * dP / pressure)) / pressure;

            // Adiabatic compressibility (approximately 1.4 * isothermal for ideal gas)
            var adiabaticCompressibility = 1.4m * Math.Abs(isothermalCompressibility);
            var compressibilityFactor = (pressure * composition.MolecularWeight) / (zFactor * 10.73m * temperature);

            var result = new GasCompressibilityAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("GAS_COMPRESSIBILITY", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                Temperature = temperature,
                IsothermalCompressibility = Math.Abs(isothermalCompressibility),
                AdiabaticCompressibility = adiabaticCompressibility,
                ZFactor = zFactor,
                CompressibilityFactor = compressibilityFactor
            };

            _logger?.LogInformation("Compressibility analysis complete: Isothermal={Isothermal}, Adiabatic={Adiabatic}", result.IsothermalCompressibility, result.AdiabaticCompressibility);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Calculates virial coefficients for the gas mixture.
        /// </summary>
        public async Task<VirialCoefficientAnalysis> CalculateVirialCoefficientsAsync(GasComposition composition, decimal pressure, decimal temperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Calculating virial coefficients for composition {CompositionId}", composition.CompositionId);

            // Estimate pseudocritical properties
            var pseudoCriticalTemp = 169.2m + (349.5m * composition.SpecificGravity) - (74m * (composition.SpecificGravity * composition.SpecificGravity));
            var pseudoCriticalPress = 756.8m - (131m * composition.SpecificGravity) - (3.6m * (composition.SpecificGravity * composition.SpecificGravity));

            var reducedTemp = temperature / pseudoCriticalTemp;
            var reducedPress = pressure / pseudoCriticalPress;

            // Second virial coefficient (dimensionless)
            var secondVirial = 0.083m - (0.422m / (decimal)Math.Pow((double)reducedTemp, 1.6));
            
            // Third virial coefficient (dimensionless)
            var thirdVirial = 0.139m - (0.172m / (decimal)Math.Pow((double)reducedTemp, 4.2));

            var result = new VirialCoefficientAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("VIRIAL_COEFF", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                Temperature = temperature,
                SecondVirialCoefficient = secondVirial,
                ThirdVirialCoefficient = thirdVirial,
                ReducedTemperature = reducedTemp,
                ReducedPressure = reducedPress
            };

            _logger?.LogInformation("Virial coefficients calculated: B={B}, C={C}", secondVirial, thirdVirial);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Analyzes gas mixture properties and pseudocritical conditions.
        /// </summary>
        public async Task<GasMixtureAnalysis> AnalyzeMixturePropertiesAsync(GasComposition composition)
        {
            if (composition == null || composition.Components == null || composition.Components.Count == 0)
                throw new ArgumentException("Composition must have components", nameof(composition));

            _logger?.LogInformation("Analyzing mixture properties for composition {CompositionId}", composition.CompositionId);

            var pseudoCriticalTemp = 0m;
            var pseudoCriticalPress = 0m;

            var result = new GasMixtureAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("MIXTURE_ANALYSIS", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                AverageMolecularWeight = composition.MolecularWeight,
                ComponentAnalysis = new List<MixtureComponentAnalysis>()
            };

            // Analyze each component
            foreach (var component in composition.Components)
            {
                var componentAnalysis = new MixtureComponentAnalysis
                {
                    ComponentName = component.ComponentName,
                    MoleFraction = component.MoleFraction,
                    CriticalTemperature = EstimateCriticalTemp(component.ComponentName),
                    CriticalPressure = EstimateCriticalPress(component.ComponentName),
                    AccentricityFactor = EstimateAccentricity(component.ComponentName)
                };

                pseudoCriticalTemp += component.MoleFraction * componentAnalysis.CriticalTemperature;
                pseudoCriticalPress += component.MoleFraction * componentAnalysis.CriticalPressure;

                result.ComponentAnalysis.Add(componentAnalysis);
            }

            result.PseudoCriticalTemperature = pseudoCriticalTemp;
            result.PseudoCriticalPressure = pseudoCriticalPress;

            _logger?.LogInformation("Mixture analysis complete: Pc={Pc}psia, Tc={Tc}°R", result.PseudoCriticalPressure, result.PseudoCriticalTemperature);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Analyzes thermal conductivity of the gas.
        /// </summary>
        public async Task<ThermalConductivityAnalysis> AnalyzeThermalConductivityAsync(GasComposition composition, decimal pressure, decimal temperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Analyzing thermal conductivity for composition {CompositionId}", composition.CompositionId);

            // Thermal conductivity correlation (simplified)
            var thermalConductivityAtSC = 0.0015m + (0.00003m * composition.MolecularWeight);
            var tempFactor = (decimal)Math.Sqrt((double)(temperature / 520m));
            var pressureFactor = 1m + (0.000001m * pressure);
            var thermalConductivity = thermalConductivityAtSC * tempFactor * pressureFactor;

            var result = new ThermalConductivityAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("THERMAL_COND", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                AnalysisDate = DateTime.UtcNow,
                Pressure = pressure,
                Temperature = temperature,
                ThermalConductivity = Math.Max(thermalConductivity, 0.001m),
                TemperatureDependence = 0.5m,
                PressureDependence = 0.000001m,
                CorrelationMethod = "Simplified Correlation"
            };

            _logger?.LogInformation("Thermal conductivity analysis complete: κ={TC} BTU/(hr·ft·°R)", result.ThermalConductivity);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Generates a property correlation matrix across pressure and temperature ranges.
        /// </summary>
        public async Task<GasPropertyMatrix> GeneratePropertyMatrixAsync(GasComposition composition, decimal minPressure, decimal maxPressure, decimal minTemperature, decimal maxTemperature)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            _logger?.LogInformation("Generating property matrix for composition {CompositionId}", composition.CompositionId);

            var result = new GasPropertyMatrix
            {
                MatrixId = _defaults.FormatIdForTable("GAS_PROPERTY_MATRIX", Guid.NewGuid().ToString()),
                CompositionId = composition.CompositionId,
                GenerationDate = DateTime.UtcNow,
                MinPressure = minPressure,
                MaxPressure = maxPressure,
                MinTemperature = minTemperature,
                MaxTemperature = maxTemperature,
                PropertyValues = new List<PropertyValue>()
            };

            var pressureStep = (maxPressure - minPressure) / 10m;
            var tempStep = (maxTemperature - minTemperature) / 10m;

            for (decimal p = minPressure; p <= maxPressure; p += pressureStep)
            {
                for (decimal t = minTemperature; t <= maxTemperature; t += tempStep)
                {
                    var zFactor = CalculateZFactor(p, t, composition.SpecificGravity);
                    var density = CalculateGasDensity(p, t, zFactor, composition.MolecularWeight);
                    var fvf = CalculateFormationVolumeFactor(p, t, zFactor);
                    
                    var viscosityAnalysis = await AnalyzeGasViscosityAsync(composition, p, t);
                    var conductivityAnalysis = await AnalyzeThermalConductivityAsync(composition, p, t);

                    result.PropertyValues.Add(new PropertyValue
                    {
                        Pressure = p,
                        Temperature = t,
                        ZFactor = zFactor,
                        Density = density,
                        Viscosity = viscosityAnalysis.Viscosity,
                        ThermalConductivity = conductivityAnalysis.ThermalConductivity,
                        CompressibilityFactor = 1m / zFactor
                    });
                }
            }

            _logger?.LogInformation("Property matrix generated: {PointCount} property combinations", result.PropertyValues.Count);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Helper method to estimate critical temperature by component name.
        /// </summary>
        private decimal EstimateCriticalTemp(string componentName)
        {
            return componentName.ToLower() switch
            {
                "ch4" or "methane" => 343.0m,
                "c2h6" or "ethane" => 549.8m,
                "c3h8" or "propane" => 369.8m,
                "n2" or "nitrogen" => 126.2m,
                "co2" or "carbon dioxide" => 304.1m,
                "h2s" or "hydrogen sulfide" => 373.5m,
                _ => 350m // Default estimate
            };
        }

        /// <summary>
        /// Helper method to estimate critical pressure by component name.
        /// </summary>
        private decimal EstimateCriticalPress(string componentName)
        {
            return componentName.ToLower() switch
            {
                "ch4" or "methane" => 667.8m,
                "c2h6" or "ethane" => 708.3m,
                "c3h8" or "propane" => 616.3m,
                "n2" or "nitrogen" => 492.5m,
                "co2" or "carbon dioxide" => 1070.6m,
                "h2s" or "hydrogen sulfide" => 1306.0m,
                _ => 700m // Default estimate
            };
        }

        /// <summary>
        /// Helper method to estimate accentricity factor by component name.
        /// </summary>
        private decimal EstimateAccentricity(string componentName)
        {
            return componentName.ToLower() switch
            {
                "ch4" or "methane" => 0.011m,
                "c2h6" or "ethane" => 0.099m,
                "c3h8" or "propane" => 0.152m,
                "n2" or "nitrogen" => 0.040m,
                "co2" or "carbon dioxide" => 0.225m,
                "h2s" or "hydrogen sulfide" => 0.100m,
                _ => 0.10m // Default estimate
            };
        }
    }
}

