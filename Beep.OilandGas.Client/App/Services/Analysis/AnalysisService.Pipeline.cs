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

                return MapPipelineAnalysisResult(result);
            }

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var result = await PostAsync<AnalyzePipelineFlowRequest, PipelineAnalysisResult>("/api/pipelineanalysis/analyze-flow", request, cancellationToken);
                return MapPipelineAnalysisResult(result);
            }
            
            throw new InvalidOperationException($"Untitled AccessMode: {AccessMode}");
        }

        public async Task<PIPELINE_FLOW_ANALYSIS_RESULT> CalculatePressureDropAsync(GAS_PIPELINE_FLOW_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var mappedRequest = new CalculatePressureDropRequest
                {
                    PipelineId = ResolvePipelineId(request),
                    FlowRate = request.GAS_FLOW_RATE
                };

                var result = await PostAsync<CalculatePressureDropRequest, PressureDropResult>("/api/pipelineanalysis/pressure-drop", mappedRequest, cancellationToken);
                return MapPressureDropResult(request, result);
            }
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

        private static PIPELINE_FLOW_ANALYSIS_RESULT MapPipelineAnalysisResult(PipelineAnalysisResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

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

        private static PIPELINE_FLOW_ANALYSIS_RESULT MapPressureDropResult(GAS_PIPELINE_FLOW_PROPERTIES request, PressureDropResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return new PIPELINE_FLOW_ANALYSIS_RESULT
            {
                PIPELINE_FLOW_ANALYSIS_RESULT_ID = string.IsNullOrWhiteSpace(request.GAS_PIPELINE_FLOW_PROPERTIES_ID)
                    ? Guid.NewGuid().ToString("N")
                    : request.GAS_PIPELINE_FLOW_PROPERTIES_ID,
                PIPELINE_PROPERTIES_ID = ResolvePipelineId(request),
                FLOW_RATE = request.GAS_FLOW_RATE,
                PRESSURE_DROP = result.PressureDrop,
                FRICTION_FACTOR = result.FrictionFactor,
                REYNOLDS_NUMBER = result.ReynoldsNumber,
                FLOW_REGIME = result.FlowRegime ?? string.Empty
            };
        }

        private static string ResolvePipelineId(GAS_PIPELINE_FLOW_PROPERTIES request)
        {
            if (!string.IsNullOrWhiteSpace(request.PIPELINE_PROPERTIES_ID))
                return request.PIPELINE_PROPERTIES_ID;

            if (!string.IsNullOrWhiteSpace(request.PIPELINE_PROPERTIES?.PIPELINE_PROPERTIES_ID))
                return request.PIPELINE_PROPERTIES.PIPELINE_PROPERTIES_ID;

            if (!string.IsNullOrWhiteSpace(request.PIPELINE?.PIPELINE_PROPERTIES_ID))
                return request.PIPELINE.PIPELINE_PROPERTIES_ID;

            throw new ArgumentException("Pipeline properties id is required.", nameof(request));
        }
    }
}
