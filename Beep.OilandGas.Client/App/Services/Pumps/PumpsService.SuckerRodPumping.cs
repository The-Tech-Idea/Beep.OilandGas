using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data.SuckerRodPumping;

namespace Beep.OilandGas.Client.App.Services.Pumps
{
    internal partial class PumpsService
    {
        #region Sucker Rod Pumping

        public async Task<SuckerRodFlowRatePowerResult> DesignSuckerRodPumpAsync(SuckerRodSystemProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<SuckerRodSystemProperties, SuckerRodFlowRatePowerResult>("/api/suckerrodpumping/design", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<SuckerRodLoadResult> AnalyzeSuckerRodPumpPerformanceAsync(SUCKER_ROD_SYSTEM_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<SUCKER_ROD_SYSTEM_PROPERTIES, SuckerRodLoadResult>("/api/suckerrodpumping/analyze-performance", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<SUCKER_ROD_FLOW_RATE_POWER_RESULT> SaveSuckerRodPumpDesignAsync(SUCKER_ROD_FLOW_RATE_POWER_RESULT design, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/suckerrodpumping/design/save", queryParams);
                return await PostAsync<SUCKER_ROD_FLOW_RATE_POWER_RESULT, SUCKER_ROD_FLOW_RATE_POWER_RESULT>(endpoint, design, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
