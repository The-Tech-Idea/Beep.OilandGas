using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.HeatMap.Configuration;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.HeatMap;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Services
{
    /// <summary>
    /// Service for heat map generation and management.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class HeatMapService : IHeatMapService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<HeatMapService>? _logger;

        public HeatMapService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<HeatMapService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public async Task<HeatMapResultDto> GenerateHeatMapAsync(List<Beep.OilandGas.Models.HeatMap.HeatMapDataPoint> dataPoints, HeatMapConfigurationDto configuration)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points cannot be null or empty", nameof(dataPoints));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _logger?.LogInformation("Generating heat map with {PointCount} data points", dataPoints.Count);

            // Convert from Models.HeatMap.HeatMapDataPoint to HeatMap.HeatMapDataPoint for the generator
            var generatorDataPoints = dataPoints.Select(p => new HeatMapDataPoint(p.OriginalX, p.OriginalY, p.Value, p.Label)
            {
                X = p.X,
                Y = p.Y
            }).ToList();

            // Use the existing HeatMapGenerator
            var generator = new HeatMapGenerator(generatorDataPoints, 800, 600, SKColors.Blue);
            
            // Generate the heat map
            var result = new HeatMapResultDto
            {
                HeatMapId = _defaults.FormatIdForTable("HEAT_MAP", Guid.NewGuid().ToString()),
                HeatMapName = $"HeatMap_{DateTime.UtcNow:yyyyMMddHHmmss}",
                GeneratedDate = DateTime.UtcNow,
                DataPoints = dataPoints,
                Configuration = configuration
            };

            _logger?.LogInformation("Heat map generated: {HeatMapId}", result.HeatMapId);

            await Task.CompletedTask;
            return result;
        }

        public async Task<string> SaveHeatMapConfigurationAsync(HeatMapConfigurationDto configuration, string userId)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving heat map configuration {ConfigurationId}", configuration.ConfigurationId);

            if (string.IsNullOrWhiteSpace(configuration.ConfigurationId))
            {
                configuration.ConfigurationId = _defaults.FormatIdForTable("HEAT_MAP", Guid.NewGuid().ToString());
            }

            // Create repository for HEAT_MAP_CONFIGURATION
            var configRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(HEAT_MAP_CONFIGURATION), _connectionName, "HEAT_MAP_CONFIGURATION", null);

            var newEntity = new HEAT_MAP_CONFIGURATION
            {
                HEAT_MAP_ID = configuration.ConfigurationId,
                CONFIGURATION_NAME = configuration.ConfigurationName ?? string.Empty,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await configRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully saved heat map configuration {ConfigurationId}", configuration.ConfigurationId);
            return configuration.ConfigurationId;
        }

        public async Task<HeatMapConfigurationDto?> GetHeatMapConfigurationAsync(string heatMapId)
        {
            if (string.IsNullOrWhiteSpace(heatMapId))
            {
                _logger?.LogWarning("GetHeatMapConfigurationAsync called with null or empty heatMapId");
                return null;
            }

            _logger?.LogInformation("Getting heat map configuration {HeatMapId}", heatMapId);

            // Create repository for HEAT_MAP_CONFIGURATION
            var configRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(HEAT_MAP_CONFIGURATION), _connectionName, "HEAT_MAP_CONFIGURATION", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "HEAT_MAP_ID", Operator = "=", FilterValue = heatMapId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await configRepo.GetAsync(filters);
            var entity = entities.Cast<HEAT_MAP_CONFIGURATION>().FirstOrDefault();

            if (entity == null)
            {
                _logger?.LogWarning("Heat map configuration {HeatMapId} not found", heatMapId);
                return null;
            }

            var config = new HeatMapConfigurationDto
            {
                ConfigurationId = entity.HEAT_MAP_ID ?? string.Empty,
                ConfigurationName = entity.CONFIGURATION_NAME ?? string.Empty,
                CreatedDate = entity.ROW_CREATED_DATE ?? DateTime.UtcNow
            };

            _logger?.LogInformation("Successfully retrieved heat map configuration {HeatMapId}", heatMapId);
            return config;
        }

        public async Task<HeatMapResultDto> GenerateProductionHeatMapAsync(string fieldId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));

            _logger?.LogInformation("Generating production heat map for field {FieldId} from {StartDate} to {EndDate}",
                fieldId, startDate, endDate);

            // TODO: Retrieve production data from database and generate heat map
            // For now, return empty result
            var result = new HeatMapResultDto
            {
                HeatMapId = _defaults.FormatIdForTable("HEAT_MAP", Guid.NewGuid().ToString()),
                HeatMapName = $"ProductionHeatMap_{fieldId}_{startDate:yyyyMMdd}",
                GeneratedDate = DateTime.UtcNow,
                DataPoints = new List<Beep.OilandGas.Models.HeatMap.HeatMapDataPoint>(),
                Configuration = new HeatMapConfigurationDto()
            };

            _logger?.LogWarning("GenerateProductionHeatMapAsync not fully implemented - requires production data integration");

            await Task.CompletedTask;
            return result;
        }
    }
}
