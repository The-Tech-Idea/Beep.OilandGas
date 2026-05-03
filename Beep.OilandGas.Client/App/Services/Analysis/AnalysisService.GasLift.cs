using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.GasLift;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region GasLift

        public async Task<GAS_LIFT_VALVE_DESIGN_RESULT> DesignGasLiftAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var mappedRequest = new DesignValvesRequest
                {
                    WellProperties = request,
                    GasInjectionPressure = ResolveGasInjectionPressure(request),
                    NumberOfValves = 5,
                    UseSIUnits = false
                };

                return await PostAsync<DesignValvesRequest, GAS_LIFT_VALVE_DESIGN_RESULT>("/api/gaslift/design-valves", mappedRequest, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_LIFT_POTENTIAL_RESULT> OptimizeInjectionAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var mappedRequest = new AnalyzeGasLiftPotentialRequest
                {
                    WellProperties = request,
                    MinGasInjectionRate = 100m,
                    MaxGasInjectionRate = 5000m,
                    NumberOfPoints = 50
                };

                var result = await PostAsync<AnalyzeGasLiftPotentialRequest, GAS_LIFT_POTENTIAL_RESULT>(
                    "/api/gaslift/analyze-potential", mappedRequest, cancellationToken);
                return result ?? throw new InvalidOperationException("Gas lift potential analysis returned no result.");
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_LIFT_VALVE_DESIGN_RESULT> GetValveSpacingAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_LIFT_WELL_PROPERTIES, GAS_LIFT_VALVE_DESIGN_RESULT>("/api/gaslift/valve-spacing", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_LIFT_PERFORMANCE> GetGasLiftPerformanceAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<GAS_LIFT_PERFORMANCE>($"/api/gaslift/performance/{wellId}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GAS_LIFT_DESIGN> TroubleshootGasLiftAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_LIFT_WELL_PROPERTIES, GAS_LIFT_DESIGN>("/api/gaslift/troubleshoot", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        private static decimal ResolveGasInjectionPressure(GAS_LIFT_WELL_PROPERTIES request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var gasInjectionPressure = request.CASING_PRESSURE_RATING * 0.8m;
            if (gasInjectionPressure <= request.WELLHEAD_PRESSURE)
            {
                throw new ArgumentException("Gas injection pressure cannot be inferred from casing pressure rating.", nameof(request));
            }

            return gasInjectionPressure;
        }

        #endregion
    }
}
