using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.GasProperties.Calculations;
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

        public async Task SaveGasCompositionAsync(GasCompositionDto composition, string userId)
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
                    var newComponent = new GAS_COMPOSITION_COMPONENT
                    {
                        GAS_COMPONENT_ID = componentId,
                        GAS_COMPOSITION_ID = composition.CompositionId,
                        COMPONENT_NAME = component.ComponentName ?? string.Empty,
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

        public async Task<GasCompositionDto?> GetGasCompositionAsync(string compositionId)
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
            var composition = new GasCompositionDto
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
                composition.Components = new List<GasComponentDto>();
                foreach (var componentEntity in componentEntities.Cast<GAS_COMPOSITION_COMPONENT>())
                {
                    composition.Components.Add(new GasComponentDto
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
    }
}

