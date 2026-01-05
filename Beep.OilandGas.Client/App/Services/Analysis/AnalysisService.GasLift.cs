using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.GasLift;
using Beep.OilandGas.Models.Data.GasLift;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region GasLift

        public async Task<GasLiftValveDesignResult> DesignGasLiftAsync(GasLiftWellProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasLiftWellProperties, GasLiftValveDesignResult>("/api/gaslift/design", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GasLiftPotentialResult> OptimizeInjectionAsync(GasLiftWellProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasLiftWellProperties, GasLiftPotentialResult>("/api/gaslift/optimize", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_LIFT_VALVE_DESIGN_RESULT> GetValveSpacingAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_LIFT_WELL_PROPERTIES, GAS_LIFT_VALVE_DESIGN_RESULT>("/api/gaslift/valve-spacing", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_LIFT_PERFORMANCE> GetGasLiftPerformanceAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<GAS_LIFT_PERFORMANCE>($"/api/gaslift/{wellId}/performance", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_LIFT_DESIGN> TroubleshootGasLiftAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_LIFT_WELL_PROPERTIES, GAS_LIFT_DESIGN>("/api/gaslift/troubleshoot", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
