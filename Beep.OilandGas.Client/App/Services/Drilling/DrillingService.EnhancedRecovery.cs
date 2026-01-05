using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Drilling
{
    internal partial class DrillingService
    {
        #region Enhanced Recovery

        public async Task<EORAnalysisResult> AnalyzeEORAsync(EORAnalysisRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<EORAnalysisRequest, EORAnalysisResult>("/api/drilling/eor/analyze", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<InjectionPlan> GetInjectionPlanAsync(string fieldId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fieldId)) throw new ArgumentNullException(nameof(fieldId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<InjectionPlan>($"/api/drilling/eor/injection/{Uri.EscapeDataString(fieldId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<RecoveryFactor> GetRecoveryFactorAsync(string reservoirId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(reservoirId)) throw new ArgumentNullException(nameof(reservoirId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<RecoveryFactor>($"/api/drilling/eor/recovery/{Uri.EscapeDataString(reservoirId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WaterfloodDesign> CreateWaterfloodDesignAsync(WaterfloodDesign request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WaterfloodDesign, WaterfloodDesign>("/api/drilling/eor/waterflood", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<GasInjectionDesign> CreateGasInjectionDesignAsync(GasInjectionDesign request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasInjectionDesign, GasInjectionDesign>("/api/drilling/eor/gasinjection", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<EORPerformance> GetEORPerformanceAsync(string projectId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(projectId)) throw new ArgumentNullException(nameof(projectId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<EORPerformance>($"/api/drilling/eor/performance/{Uri.EscapeDataString(projectId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<InjectionPattern> OptimizeInjectionPatternAsync(InjectionPattern request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<InjectionPattern, InjectionPattern>("/api/drilling/eor/optimize", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
