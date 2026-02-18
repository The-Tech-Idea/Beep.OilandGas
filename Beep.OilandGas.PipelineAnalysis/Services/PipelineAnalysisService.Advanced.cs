using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.PipelineAnalysis.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PipelineAnalysis.Services
{
    public partial class PipelineAnalysisService
    {
        public async Task<PIPELINE_FLOW_ANALYSIS_RESULT> AnalyzeGasPipelineFlowAsync(
            string pipelineId, 
            decimal p1, 
            decimal p2, 
            decimal lengthMiles, 
            decimal diameterInches)
        {
            _logger?.LogInformation("Analyzing Gas Pipeline Flow for Pipeline {Id}", pipelineId);

            decimal flow = PipelineCalculator.CalculateGasFlow_PanhandleB(p1, p2, lengthMiles, diameterInches);

            var result = new PIPELINE_FLOW_ANALYSIS_RESULT
            {
                 PIPELINE_FLOW_ANALYSIS_RESULT_ID = Guid.NewGuid().ToString(),
                 PIPELINE_PROPERTIES_ID = pipelineId,
                 AnalysisDate = DateTime.UtcNow,
                 FLOW_RATE = flow,
                 INLET_PRESSURE = p1,
                 OUTLET_PRESSURE = p2,
                 FLOW_REGIME = "GAS_TRANSMISSION",
                 Status = "SUCCESS"
            };

            return await Task.FromResult(result);
        }

        public async Task<PIPELINE_FLOW_ANALYSIS_RESULT> AnalyzeLiquidPipelineFlowAsync(
            string pipelineId,
            decimal flowBpd,
            decimal lengthFt,
            decimal diameterInches,
            decimal densityLbFt3,
            decimal viscosityCp,
            decimal roughnessInches = 0.0018m)
        {
             _logger?.LogInformation("Analyzing Liquid Pipeline Flow for Pipeline {Id}", pipelineId);

             // 1. Velocity
             decimal v = PipelineCalculator.CalculateLiquidVelocity(flowBpd, diameterInches);
             
             // 2. Reynolds
             decimal re = PipelineCalculator.CalculateReynoldsNumber(densityLbFt3, v, diameterInches, viscosityCp);
             
             // 3. Friction Factor
             decimal f = PipelineCalculator.CalculateFrictionFactor(re, roughnessInches, diameterInches);
             
             // 4. Pressure Drop
             decimal dP = PipelineCalculator.CalculatePressureDrop_Darcy(f, lengthFt, densityLbFt3, v, diameterInches);

             var result = new PIPELINE_FLOW_ANALYSIS_RESULT
             {
                  PIPELINE_FLOW_ANALYSIS_RESULT_ID = Guid.NewGuid().ToString(),
                  PIPELINE_PROPERTIES_ID = pipelineId,
                  AnalysisDate = DateTime.UtcNow,
                  FLOW_RATE = flowBpd,
                  FLOW_VELOCITY = v,
                  REYNOLDS_NUMBER = re,
                  FRICTION_FACTOR = f,
                  PRESSURE_DROP = dP,
                  FLOW_REGIME = re > 4000 ? "TURBULENT" : "LAMINAR",
                  Status = "SUCCESS"
             };

             return await Task.FromResult(result);
        }
    }
}
