using System;
using System.Collections.Generic;
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

                var result = await PostAsync<AnalyzeGasLiftPotentialRequest, GAS_LIFT_WELL_PROPERTIES>("/api/gaslift/analyze-potential", mappedRequest, cancellationToken);
                return MapGasLiftPotentialResult(request, result);
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

        private static GAS_LIFT_POTENTIAL_RESULT MapGasLiftPotentialResult(
            GAS_LIFT_WELL_PROPERTIES request,
            GAS_LIFT_WELL_PROPERTIES result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            var potentialResultId = Guid.NewGuid().ToString("N");
            var performancePoints = new List<GAS_LIFT_PERFORMANCE_POINT>();

            if (result.PERFORMANCE_POINTS != null)
            {
                for (var index = 0; index < result.PERFORMANCE_POINTS.Count; index++)
                {
                    var point = result.PERFORMANCE_POINTS[index];
                    performancePoints.Add(new GAS_LIFT_PERFORMANCE_POINT
                    {
                        GAS_LIFT_PERFORMANCE_POINT_ID = Guid.NewGuid().ToString("N"),
                        GAS_LIFT_POTENTIAL_RESULT_ID = potentialResultId,
                        GAS_INJECTION_RATE = point.GasInjectionRate,
                        PRODUCTION_RATE = point.ProductionRate,
                        GAS_LIQUID_RATIO = point.GasLiquidRatio,
                        BOTTOM_HOLE_PRESSURE = point.BottomHolePressure,
                        POINT_ORDER = index + 1
                    });
                }
            }

            return new GAS_LIFT_POTENTIAL_RESULT
            {
                GAS_LIFT_POTENTIAL_RESULT_ID = potentialResultId,
                GAS_LIFT_WELL_PROPERTIES_ID = string.IsNullOrWhiteSpace(request.GAS_LIFT_WELL_PROPERTIES_ID)
                    ? result.GAS_LIFT_WELL_PROPERTIES_ID
                    : request.GAS_LIFT_WELL_PROPERTIES_ID,
                OPTIMAL_GAS_INJECTION_RATE = result.OPTIMAL_GAS_INJECTION_RATE,
                MAXIMUM_PRODUCTION_RATE = result.MAXIMUM_PRODUCTION_RATE,
                OPTIMAL_GAS_LIQUID_RATIO = result.OPTIMAL_GAS_LIQUID_RATIO,
                PERFORMANCE_POINTS = performancePoints
            };
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
