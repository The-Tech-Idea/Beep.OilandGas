using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ChokeAnalysis;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region Choke

        public async Task<CHOKE_FLOW_RESULT> CalculateDownholeChokeFlowAsync(GAS_CHOKE_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_CHOKE_PROPERTIES, CHOKE_FLOW_RESULT>("/api/choke/downhole/flow", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<CHOKE_FLOW_RESULT> CalculateUpholeChokeFlowAsync(GAS_CHOKE_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_CHOKE_PROPERTIES, CHOKE_FLOW_RESULT>("/api/choke/uphole/flow", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<CHOKE_FLOW_RESULT> CalculateDownstreamPressureAsync(GAS_CHOKE_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_CHOKE_PROPERTIES, CHOKE_FLOW_RESULT>("/api/choke/downstream-pressure", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<CHOKE_FLOW_RESULT> CalculateRequiredChokeSizeAsync(GAS_CHOKE_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_CHOKE_PROPERTIES, CHOKE_FLOW_RESULT>("/api/choke/size", request, cancellationToken);
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
