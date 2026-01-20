using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.PlungerLift;

namespace Beep.OilandGas.Client.App.Services.Pumps
{
    internal partial class PumpsService
    {
        #region Plunger Lift

        public async Task<PlungerLiftPerformanceResult> DesignPlungerLiftAsync(PlungerLiftWellProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PlungerLiftWellProperties, PlungerLiftPerformanceResult>("/api/plungerlift/design", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PlungerLiftCycleResult> AnalyzePlungerLiftPerformanceAsync(PLUNGER_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PLUNGER_LIFT_WELL_PROPERTIES, PlungerLiftCycleResult>("/api/plungerlift/analyze-performance", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PLUNGER_LIFT_CYCLE_RESULT> SavePlungerLiftDesignAsync(PLUNGER_LIFT_CYCLE_RESULT design, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/plungerlift/design/save", queryParams);
                return await PostAsync<PLUNGER_LIFT_CYCLE_RESULT, PLUNGER_LIFT_CYCLE_RESULT>(endpoint, design, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
