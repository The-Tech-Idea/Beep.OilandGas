using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace Beep.OilandGas.Client.App.Services.LifeCycle
{
    internal partial class LifeCycleService
    {
        #region Development

        public async Task<DEVELOPMENT_COSTS> CreateDevelopmentPlanAsync(DEVELOPMENT_COSTS request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<DEVELOPMENT_COSTS, DEVELOPMENT_COSTS>("/api/lifecycle/development/create", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> GetDrillingScheduleAsync(string planId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(planId)) throw new ArgumentNullException(nameof(planId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FIELD_PHASE>($"/api/lifecycle/development/{Uri.EscapeDataString(planId)}/drilling-schedule", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> GetCompletionPlanAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FIELD_PHASE>($"/api/lifecycle/development/completion/{Uri.EscapeDataString(wellId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DEVELOPMENT_COSTS> UpdateDevelopmentPlanAsync(string planId, DEVELOPMENT_COSTS request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(planId)) throw new ArgumentNullException(nameof(planId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<DEVELOPMENT_COSTS, DEVELOPMENT_COSTS>($"/api/lifecycle/development/{Uri.EscapeDataString(planId)}", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> GetDevelopmentStatusAsync(string planId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(planId)) throw new ArgumentNullException(nameof(planId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FIELD_PHASE>($"/api/lifecycle/development/{Uri.EscapeDataString(planId)}/status", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
