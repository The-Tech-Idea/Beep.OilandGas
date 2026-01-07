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
    }
}
