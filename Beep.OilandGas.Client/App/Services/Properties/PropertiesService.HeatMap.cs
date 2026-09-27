using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Client.App.Services.Properties
{
    internal partial class PropertiesService
    {
        #region Heat Map

        public async Task<List<HEAT_MAP_DATA_POINT>> GenerateHeatMapAsync(HEAT_MAP_CONFIGURATION request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<HEAT_MAP_CONFIGURATION, List<HEAT_MAP_DATA_POINT>>("/api/heatmap/generate", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<HEAT_MAP_CONFIGURATION> SaveHeatMapConfigurationAsync(HEAT_MAP_CONFIGURATION configuration, CancellationToken cancellationToken = default)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var endpoint = "/api/heatmap/configuration/save";
                return await PostAsync<HEAT_MAP_CONFIGURATION, HEAT_MAP_CONFIGURATION>(endpoint, configuration, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<HEAT_MAP_CONFIGURATION> GetHeatMapConfigurationAsync(string heatMapId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(heatMapId)) throw new ArgumentNullException(nameof(heatMapId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<HEAT_MAP_CONFIGURATION>($"/api/heatmap/configuration/{Uri.EscapeDataString(heatMapId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<HEAT_MAP_DATA_POINT>> GenerateProductionHeatMapAsync(HEAT_MAP_CONFIGURATION request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<HEAT_MAP_CONFIGURATION, List<HEAT_MAP_DATA_POINT>>("/api/heatmap/production", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
