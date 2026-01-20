using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.GasProperties;
using Beep.OilandGas.Models.Data.Common;

namespace Beep.OilandGas.Client.App.Services.Properties
{
    internal partial class PropertiesService
    {
        #region Gas Properties

        public async Task<decimal> CalculateGasZFactorAsync(GasComposition request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasComposition, decimal>("/api/gasproperties/zfactor", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<decimal> CalculateGasDensityAsync(GasComposition request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasComposition, decimal>("/api/gasproperties/density", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<decimal> CalculateGasFormationVolumeFactorAsync(GasComposition request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasComposition, decimal>("/api/gasproperties/fvf", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_COMPOSITION> SaveGasCompositionAsync(GAS_COMPOSITION composition, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (composition == null) throw new ArgumentNullException(nameof(composition));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/gasproperties/composition/save", queryParams);
                return await PostAsync<GAS_COMPOSITION, GAS_COMPOSITION>(endpoint, composition, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_COMPOSITION> GetGasCompositionAsync(string compositionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(compositionId)) throw new ArgumentNullException(nameof(compositionId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<GAS_COMPOSITION>($"/api/gasproperties/composition/{Uri.EscapeDataString(compositionId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
