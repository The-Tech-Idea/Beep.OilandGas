using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PumpPerformance;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region PumpPerformance

        public async Task<ESP_DESIGN_RESULT> AnalyzePumpPerformanceAsync(ESP_DESIGN_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ESP_DESIGN_PROPERTIES, ESP_DESIGN_RESULT>("/api/pump/analyze", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ESP_DESIGN_RESULT> GetSystemCurveAsync(ESP_DESIGN_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ESP_DESIGN_PROPERTIES, ESP_DESIGN_RESULT>("/api/pump/system-curve", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ESP_DESIGN_RESULT> OptimizePumpAsync(ESP_DESIGN_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ESP_DESIGN_PROPERTIES, ESP_DESIGN_RESULT>("/api/pump/optimize", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ESP_DESIGN_RESULT> SelectPumpAsync(ESP_DESIGN_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ESP_DESIGN_PROPERTIES, ESP_DESIGN_RESULT>("/api/pump/select", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ESP_PUMP_POINT> GetPumpEfficiencyAsync(string pumpId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pumpId)) throw new ArgumentNullException(nameof(pumpId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<ESP_PUMP_POINT>($"/api/pump/{pumpId}/efficiency", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
