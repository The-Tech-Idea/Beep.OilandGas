using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.PipelineAnalysis;


namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region Pipeline

        public async Task<PIPELINE_FLOW_ANALYSIS_RESULT> AnalyzePipelineAsync(Beep.OilandGas.Models.Data.Calculations.AnalyzePipelineFlowRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (AccessMode == ServiceAccessMode.Local)
            {
                var service = GetLocalService<IPipelineAnalysisService>();
                var result = await service.AnalyzePipelineFlowAsync(request.PipelineId, request.FlowRate, request.InletPressure);

                return new PIPELINE_FLOW_ANALYSIS_RESULT
                {
                    AnalysisId = result.CalculationId,
                    PipelineId = result.PipelineId ?? string.Empty,
                    AnalysisDate = result.CalculationDate,
                    FlowRate = (double)result.FlowRate,
                    InletPressure = (double)result.InletPressure,
                    OutletPressure = (double)result.OutletPressure,
                    PressureDrop = (double)result.PressureDrop,
                    Velocity = result.Velocity != null ? Convert.ToDouble(result.Velocity) : 0,
                    FlowRegime = result.FlowRegime ?? string.Empty,
                    Status = result.Status ?? string.Empty,
                    Recommendations = result.Recommendations?.ToString() ?? string.Empty
                };
            }

            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<AnalyzePipelineFlowRequest, PIPELINE_FLOW_ANALYSIS_RESULT>("/api/pipeline/analyze-flow", request, cancellationToken);
            
            throw new InvalidOperationException($"Untitled AccessMode: {AccessMode}");
        }

        public async Task<PIPELINE_FLOW_ANALYSIS_RESULT> CalculatePressureDropAsync(GAS_PIPELINE_FLOW_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<GAS_PIPELINE_FLOW_PROPERTIES, PIPELINE_FLOW_ANALYSIS_RESULT>("/api/pipeline/pressure-drop", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PIPELINE_CAPACITY_RESULT> GetFlowCapacityAsync(string pipelineId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pipelineId)) throw new ArgumentNullException(nameof(pipelineId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PIPELINE_CAPACITY_RESULT>($"/api/pipeline/{pipelineId}/capacity", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PIPELINE_DESIGN> DesignPipelineAsync(PIPELINE_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PIPELINE_PROPERTIES, PIPELINE_DESIGN>("/api/pipeline/design", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PIPELINE_RISK_ASSESSMENT> GetPipelineIntegrityAsync(string pipelineId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pipelineId)) throw new ArgumentNullException(nameof(pipelineId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PIPELINE_RISK_ASSESSMENT>($"/api/pipeline/{pipelineId}/integrity", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
