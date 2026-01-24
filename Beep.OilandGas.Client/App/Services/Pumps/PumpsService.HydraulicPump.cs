using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HydraulicPumps;

namespace Beep.OilandGas.Client.App.Services.Pumps
{
    internal partial class PumpsService
    {
        #region Hydraulic Pump

        public async Task<HYDRAULIC_JET_PUMP_RESULT> DesignHydraulicJetPumpAsync(HYDRAULIC_JET_PUMP_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<HYDRAULIC_JET_PUMP_PROPERTIES, HYDRAULIC_JET_PUMP_RESULT>("/api/hydraulicpump/design/jet", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<HYDRAULIC_PISTON_PUMP_RESULT> DesignHydraulicPistonPumpAsync(HYDRAULIC_PISTON_PUMP_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<HYDRAULIC_PISTON_PUMP_PROPERTIES, HYDRAULIC_PISTON_PUMP_RESULT>("/api/hydraulicpump/design/piston", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<HYDRAULIC_JET_PUMP_RESULT> AnalyzeHydraulicPumpPerformanceAsync(HYDRAULIC_PUMP_WELL_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<HYDRAULIC_PUMP_WELL_PROPERTIES, HYDRAULIC_JET_PUMP_RESULT>("/api/hydraulicpump/analyze-performance", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<HYDRAULIC_JET_PUMP_RESULT> SaveHydraulicPumpDesignAsync(HYDRAULIC_JET_PUMP_RESULT design, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/hydraulicpump/design/save", queryParams);
                return await PostAsync<HYDRAULIC_JET_PUMP_RESULT, HYDRAULIC_JET_PUMP_RESULT>(endpoint, design, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<HYDRAULIC_JET_PUMP_RESULT>> GetHydraulicPumpHistoryAsync(string pumpId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pumpId)) throw new ArgumentException("Pump ID is required", nameof(pumpId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<HYDRAULIC_JET_PUMP_RESULT>>($"/api/hydraulicpump/performance-history/{Uri.EscapeDataString(pumpId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
