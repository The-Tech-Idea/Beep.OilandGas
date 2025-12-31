using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Rendering;
using Beep.OilandGas.Models.DTOs.Rendering;
using SkiaSharp;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for chart rendering operations.
    /// </summary>
    public interface IRenderingService
    {
        /// <summary>
        /// Renders a chart to a SkiaSharp canvas.
        /// </summary>
        Task<RenderChartResult> RenderChartAsync(RenderChartRequest request, SKCanvas canvas, string userId, string? connectionName = null);
        
        /// <summary>
        /// Exports a chart to an image file.
        /// </summary>
        Task<ExportChartResult> ExportChartAsync(ExportChartRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Saves a chart configuration.
        /// </summary>
        Task<CHART_CONFIGURATION> SaveChartConfigurationAsync(SaveChartConfigurationRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a chart configuration by ID.
        /// </summary>
        Task<CHART_CONFIGURATION?> GetChartConfigurationAsync(string configurationId, string? connectionName = null);
        
        /// <summary>
        /// Gets all chart configurations for a chart type.
        /// </summary>
        Task<List<CHART_CONFIGURATION>> GetChartConfigurationsAsync(string? chartType, string? connectionName = null);
        
        /// <summary>
        /// Gets the default chart configuration for a chart type.
        /// </summary>
        Task<CHART_CONFIGURATION?> GetDefaultChartConfigurationAsync(string chartType, string? connectionName = null);
        
        /// <summary>
        /// Deletes a chart configuration.
        /// </summary>
        Task<bool> DeleteChartConfigurationAsync(string configurationId, string userId, string? connectionName = null);
    }
}

