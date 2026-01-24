using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data.OilProperties;

namespace Beep.OilandGas.Client.App.Services.Properties
{
    internal partial class PropertiesService
    {
        #region Oil Properties

        public async Task<decimal> CalculateOilFormationVolumeFactorAsync(OilPropertyConditions request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<OilPropertyConditions, decimal>("/api/oilproperties/fvf", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<decimal> CalculateOilDensityAsync(OilPropertyConditions request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<OilPropertyConditions, decimal>("/api/oilproperties/density", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<decimal> CalculateOilViscosityAsync(OilPropertyConditions request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<OilPropertyConditions, decimal>("/api/oilproperties/viscosity", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<OilPropertyResult> CalculateOilPropertiesAsync(OilPropertyConditions request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<OilPropertyConditions, OilPropertyResult>("/api/oilproperties/calculate", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<OIL_COMPOSITION> SaveOilCompositionAsync(OIL_COMPOSITION composition, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (composition == null) throw new ArgumentNullException(nameof(composition));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/oilproperties/composition/save", queryParams);
                return await PostAsync<OIL_COMPOSITION, OIL_COMPOSITION>(endpoint, composition, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<OIL_COMPOSITION> GetOilCompositionAsync(string compositionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(compositionId)) throw new ArgumentNullException(nameof(compositionId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<OIL_COMPOSITION>($"/api/oilproperties/composition/{Uri.EscapeDataString(compositionId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<OIL_PROPERTY_RESULT>> GetOilPropertyHistoryAsync(string compositionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(compositionId)) throw new ArgumentNullException(nameof(compositionId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<OIL_PROPERTY_RESULT>>($"/api/oilproperties/history/{Uri.EscapeDataString(compositionId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<OIL_PROPERTY_RESULT> SaveOilResultAsync(OIL_PROPERTY_RESULT result, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/oilproperties/result/save", queryParams);
                return await PostAsync<OIL_PROPERTY_RESULT, OIL_PROPERTY_RESULT>(endpoint, result, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
