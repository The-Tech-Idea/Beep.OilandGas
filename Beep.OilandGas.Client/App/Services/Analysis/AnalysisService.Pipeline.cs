using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.PipelineAnalysis;
using Beep.OilandGas.Models.Data.PipelineAnalysis;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region Pipeline

        public async Task<PipelineFlowAnalysisResult> AnalyzePipelineAsync(PipelineProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PipelineProperties, PipelineFlowAnalysisResult>("/api/pipeline/analyze", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PIPELINE_FLOW_ANALYSIS_RESULT> CalculatePressureDropAsync(GAS_PIPELINE_FLOW_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_PIPELINE_FLOW_PROPERTIES, PIPELINE_FLOW_ANALYSIS_RESULT>("/api/pipeline/pressure-drop", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PIPELINE_CAPACITY_RESULT> GetFlowCapacityAsync(string pipelineId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pipelineId)) throw new ArgumentNullException(nameof(pipelineId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PIPELINE_CAPACITY_RESULT>($"/api/pipeline/{pipelineId}/capacity", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PIPELINE_DESIGN> DesignPipelineAsync(PIPELINE_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PIPELINE_PROPERTIES, PIPELINE_DESIGN>("/api/pipeline/design", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PIPELINE_RISK_ASSESSMENT> GetPipelineIntegrityAsync(string pipelineId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pipelineId)) throw new ArgumentNullException(nameof(pipelineId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PIPELINE_RISK_ASSESSMENT>($"/api/pipeline/{pipelineId}/integrity", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
