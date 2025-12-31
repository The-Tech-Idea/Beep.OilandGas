using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Rendering;
using Beep.OilandGas.Models.DTOs.Rendering;
using Beep.OilandGas.ProductionAccounting.Allocation;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Rendering
{
    /// <summary>
    /// Service for managing chart rendering operations.
    /// Wraps existing chart renderers and provides configuration persistence.
    /// </summary>
    public class RenderingService : IRenderingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<RenderingService>? _logger;
        private readonly string _connectionName;
        private const string CHART_CONFIGURATION_TABLE = "CHART_CONFIGURATION";

        public RenderingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<RenderingService>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Renders a chart to a SkiaSharp canvas.
        /// </summary>
        public async Task<RenderChartResult> RenderChartAsync(RenderChartRequest request, SKCanvas canvas, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

            var connName = connectionName ?? _connectionName;
            var chartId = Guid.NewGuid().ToString();

            try
            {
                ProductionChartRendererConfiguration? config = null;

                if (!string.IsNullOrEmpty(request.ConfigurationId))
                {
                    var configEntity = await GetChartConfigurationAsync(request.ConfigurationId, connName);
                    if (configEntity != null)
                    {
                        config = DeserializeConfiguration(configEntity.CONFIGURATION_DATA);
                    }
                }

                config ??= new ProductionChartRendererConfiguration();

                switch (request.ChartType.ToLower())
                {
                    case "production":
                    case "productiontrend":
                        var productionRenderer = new ProductionChartRenderer(config);
                        SetProductionData(productionRenderer, request.ChartData);
                        productionRenderer.Render(canvas, request.Width, request.Height);
                        break;

                    case "allocation":
                        var allocationRenderer = new AllocationChartRenderer(config);
                        SetAllocationData(allocationRenderer, request.ChartData);
                        allocationRenderer.Render(canvas, request.Width, request.Height);
                        break;

                    case "revenue":
                    case "profitability":
                        var revenueRenderer = new RevenueChartRenderer(config);
                        SetRevenueData(revenueRenderer, request.ChartData);
                        revenueRenderer.Render(canvas, request.Width, request.Height);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported chart type: {request.ChartType}", nameof(request.ChartType));
                }

                _logger?.LogDebug("Rendered {ChartType} chart successfully", request.ChartType);

                return new RenderChartResult
                {
                    ChartId = chartId,
                    ChartType = request.ChartType,
                    RenderDate = DateTime.UtcNow,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to render {ChartType} chart", request.ChartType);

                return new RenderChartResult
                {
                    ChartId = chartId,
                    ChartType = request.ChartType,
                    RenderDate = DateTime.UtcNow,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Exports a chart to an image file.
        /// </summary>
        public async Task<ExportChartResult> ExportChartAsync(ExportChartRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.FilePath))
                throw new ArgumentException("File path is required.", nameof(request.FilePath));

            var connName = connectionName ?? _connectionName;
            var chartId = Guid.NewGuid().ToString();

            try
            {
                ProductionChartRendererConfiguration? config = null;

                if (!string.IsNullOrEmpty(request.ConfigurationId))
                {
                    var configEntity = await GetChartConfigurationAsync(request.ConfigurationId, connName);
                    if (configEntity != null)
                    {
                        config = DeserializeConfiguration(configEntity.CONFIGURATION_DATA);
                    }
                }

                config ??= new ProductionChartRendererConfiguration();

                var imageInfo = new SKImageInfo((int)request.Width, (int)request.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

                using (var surface = SKSurface.Create(imageInfo))
                {
                    var canvas = surface.Canvas;

                    var renderRequest = new RenderChartRequest
                    {
                        ChartType = request.ChartType,
                        Width = request.Width,
                        Height = request.Height,
                        ChartData = request.ChartData,
                        ConfigurationId = request.ConfigurationId
                    };

                    var renderResult = await RenderChartAsync(renderRequest, canvas, userId, connName);

                    if (!renderResult.IsSuccess)
                    {
                        return new ExportChartResult
                        {
                            ChartId = chartId,
                            ChartType = request.ChartType,
                            ExportFormat = request.ExportFormat,
                            FilePath = request.FilePath,
                            ExportDate = DateTime.UtcNow,
                            IsSuccess = false,
                            ErrorMessage = renderResult.ErrorMessage
                        };
                    }

                    using (var image = surface.Snapshot())
                    {
                        SKEncodedImageFormat format = request.ExportFormat.ToLower() switch
                        {
                            "png" => SKEncodedImageFormat.Png,
                            "jpeg" or "jpg" => SKEncodedImageFormat.Jpeg,
                            "webp" => SKEncodedImageFormat.Webp,
                            _ => SKEncodedImageFormat.Png
                        };

                        using (var data = image.Encode(format, 100))
                        using (var stream = File.Create(request.FilePath))
                        {
                            data.SaveTo(stream);
                        }
                    }
                }

                _logger?.LogDebug("Exported {ChartType} chart to {FilePath} in {Format} format",
                    request.ChartType, request.FilePath, request.ExportFormat);

                return new ExportChartResult
                {
                    ChartId = chartId,
                    ChartType = request.ChartType,
                    ExportFormat = request.ExportFormat,
                    FilePath = request.FilePath,
                    ExportDate = DateTime.UtcNow,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to export {ChartType} chart", request.ChartType);

                return new ExportChartResult
                {
                    ChartId = chartId,
                    ChartType = request.ChartType,
                    ExportFormat = request.ExportFormat,
                    FilePath = request.FilePath,
                    ExportDate = DateTime.UtcNow,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Saves a chart configuration.
        /// </summary>
        public async Task<CHART_CONFIGURATION> SaveChartConfigurationAsync(SaveChartConfigurationRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetChartConfigurationRepositoryAsync(connName);

            var entity = new CHART_CONFIGURATION
            {
                CHART_CONFIGURATION_ID = Guid.NewGuid().ToString(),
                CHART_TYPE = request.ChartType,
                CONFIGURATION_NAME = request.ConfigurationName,
                CONFIGURATION_DATA = request.ConfigurationData,
                IS_DEFAULT = request.IsDefault ? "Y" : "N",
                CREATED_BY = userId,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(entity);

            // If this is set as default, unset other defaults for this chart type
            if (request.IsDefault)
            {
                await UnsetOtherDefaultsAsync(request.ChartType, entity.CHART_CONFIGURATION_ID, connName);
            }

            _logger?.LogDebug("Saved chart configuration {ConfigurationName} for {ChartType}",
                request.ConfigurationName, request.ChartType);

            return entity;
        }

        /// <summary>
        /// Gets a chart configuration by ID.
        /// </summary>
        public async Task<CHART_CONFIGURATION?> GetChartConfigurationAsync(string configurationId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(configurationId))
                throw new ArgumentException("Configuration ID is required.", nameof(configurationId));

            var connName = connectionName ?? _connectionName;
            var repo = await GetChartConfigurationRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CHART_CONFIGURATION_ID", Operator = "=", FilterValue = configurationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<CHART_CONFIGURATION>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all chart configurations for a chart type.
        /// </summary>
        public async Task<List<CHART_CONFIGURATION>> GetChartConfigurationsAsync(string? chartType, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetChartConfigurationRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(chartType))
            {
                filters.Add(new AppFilter { FieldName = "CHART_TYPE", Operator = "=", FilterValue = chartType });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<CHART_CONFIGURATION>().OrderBy(c => c.CONFIGURATION_NAME).ToList();
        }

        /// <summary>
        /// Gets the default chart configuration for a chart type.
        /// </summary>
        public async Task<CHART_CONFIGURATION?> GetDefaultChartConfigurationAsync(string chartType, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(chartType))
                throw new ArgumentException("Chart type is required.", nameof(chartType));

            var connName = connectionName ?? _connectionName;
            var repo = await GetChartConfigurationRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CHART_TYPE", Operator = "=", FilterValue = chartType },
                new AppFilter { FieldName = "IS_DEFAULT", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<CHART_CONFIGURATION>().FirstOrDefault();
        }

        /// <summary>
        /// Deletes a chart configuration.
        /// </summary>
        public async Task<bool> DeleteChartConfigurationAsync(string configurationId, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(configurationId))
                throw new ArgumentException("Configuration ID is required.", nameof(configurationId));

            var connName = connectionName ?? _connectionName;
            var config = await GetChartConfigurationAsync(configurationId, connName);

            if (config == null)
                return false;

            var repo = await GetChartConfigurationRepositoryAsync(connName);

            // Soft delete by setting ACTIVE_IND to 'N'
            config.ACTIVE_IND = "N";

            if (config is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, userId, connName);
            }

            await repo.UpdateAsync(config);

            _logger?.LogDebug("Deleted chart configuration {ConfigurationId}", configurationId);

            return true;
        }

        // Helper methods
        private void SetProductionData(ProductionChartRenderer renderer, Dictionary<string, object>? chartData)
        {
            if (chartData == null) return;

            if (chartData.ContainsKey("productionData") && chartData["productionData"] is List<(DateTime, decimal)> productionData)
            {
                renderer.SetProductionData(productionData);
            }

            if (chartData.ContainsKey("revenueData") && chartData["revenueData"] is List<(DateTime, decimal)> revenueData)
            {
                renderer.SetRevenueData(revenueData);
            }

            if (chartData.ContainsKey("costData") && chartData["costData"] is List<(DateTime, decimal)> costData)
            {
                renderer.SetCostData(costData);
            }

            if (chartData.ContainsKey("allocationData") && chartData["allocationData"] is List<AllocationResult> allocationData)
            {
                renderer.SetAllocationData(allocationData);
            }
        }

        private void SetAllocationData(AllocationChartRenderer renderer, Dictionary<string, object>? chartData)
        {
            if (chartData == null) return;

            if (chartData.ContainsKey("allocationData") && chartData["allocationData"] is List<AllocationResult> allocationData)
            {
                renderer.SetAllocationData(allocationData);
            }
        }

        private void SetRevenueData(RevenueChartRenderer renderer, Dictionary<string, object>? chartData)
        {
            if (chartData == null) return;

            if (chartData.ContainsKey("transactions") && chartData["transactions"] is List<SalesTransaction> transactions)
            {
                renderer.SetTransactions(transactions);
            }
        }

        private ProductionChartRendererConfiguration DeserializeConfiguration(string? jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
                return new ProductionChartRendererConfiguration();

            try
            {
                // In a full implementation, would deserialize JSON to ProductionChartRendererConfiguration
                // For now, return default configuration
                return new ProductionChartRendererConfiguration();
            }
            catch
            {
                return new ProductionChartRendererConfiguration();
            }
        }

        private async Task UnsetOtherDefaultsAsync(string chartType, string currentConfigId, string connectionName)
        {
            var repo = await GetChartConfigurationRepositoryAsync(connectionName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CHART_TYPE", Operator = "=", FilterValue = chartType },
                new AppFilter { FieldName = "IS_DEFAULT", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "CHART_CONFIGURATION_ID", Operator = "<>", FilterValue = currentConfigId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);

            foreach (var config in results.Cast<CHART_CONFIGURATION>())
            {
                config.IS_DEFAULT = "N";
                await repo.UpdateAsync(config);
            }
        }

        private async Task<PPDMGenericRepository> GetChartConfigurationRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(CHART_CONFIGURATION_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Rendering.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(CHART_CONFIGURATION);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, CHART_CONFIGURATION_TABLE,
                null);
        }
    }
}
