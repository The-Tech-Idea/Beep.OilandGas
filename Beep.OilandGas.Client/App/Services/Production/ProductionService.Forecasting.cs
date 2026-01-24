using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.DCA;

namespace Beep.OilandGas.Client.App.Services.Production
{
    internal partial class ProductionService
    {
        #region Forecasting

        public async Task<ProductionForecast> CreateForecastAsync(ReservoirForecastProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ReservoirForecastProperties, ProductionForecast>("/api/production/forecast/create", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DCA_FIT_RESULT> GetDeclineCurveAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<DCA_FIT_RESULT>($"/api/production/decline-curve/{Uri.EscapeDataString(wellId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PRODUCTION_FORECAST> AnalyzeProductionAsync(RESERVOIR_FORECAST_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<RESERVOIR_FORECAST_PROPERTIES, PRODUCTION_FORECAST>("/api/production/analyze", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<PRODUCTION_FORECAST>> GetForecastHistoryAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<PRODUCTION_FORECAST>>($"/api/production/forecast/history/{Uri.EscapeDataString(wellId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PRODUCTION_FORECAST> GetTypeWellAsync(string fieldId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fieldId)) throw new ArgumentNullException(nameof(fieldId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PRODUCTION_FORECAST>($"/api/production/type-well/{Uri.EscapeDataString(fieldId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DCA_FIT_RESULT> RunDeclineCurveAnalysisAsync(RESERVOIR_FORECAST_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<RESERVOIR_FORECAST_PROPERTIES, DCA_FIT_RESULT>("/api/production/dca/run", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
