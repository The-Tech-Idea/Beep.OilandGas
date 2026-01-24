using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ChokeAnalysis;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region Choke

        public async Task<ChokeFlowResult> CalculateDownholeChokeFlowAsync(GasChokeProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasChokeProperties, ChokeFlowResult>("/api/choke/downhole/flow", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ChokeFlowResult> CalculateUpholeChokeFlowAsync(GasChokeProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasChokeProperties, ChokeFlowResult>("/api/choke/uphole/flow", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ChokeFlowResult> CalculateDownstreamPressureAsync(GasChokeProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasChokeProperties, ChokeFlowResult>("/api/choke/downstream-pressure", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ChokeFlowResult> CalculateRequiredChokeSizeAsync(GasChokeProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GasChokeProperties, ChokeFlowResult>("/api/choke/size", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<CHOKE_FLOW_RESULT> AnalyzeChokePerformanceAsync(CHOKE_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<CHOKE_PROPERTIES, CHOKE_FLOW_RESULT>("/api/choke/performance", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
