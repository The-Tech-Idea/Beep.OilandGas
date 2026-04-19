using System.Threading.Tasks;

namespace Beep.OilandGas.PipelineAnalysis.Services
{
    public partial class PipelineAnalysisService
    {
        // Explicit implementations of Models.Core.Interfaces.IPipelineAnalysisService
        System.Threading.Tasks.Task<Beep.OilandGas.Models.Data.Calculations.PipelineAnalysisResult>
            Beep.OilandGas.Models.Core.Interfaces.IPipelineAnalysisService.AnalyzePipelineFlowAsync(
                string pipelineId, decimal flowRate, decimal inletPressure)
        {
            return System.Threading.Tasks.Task.FromResult(
                new Beep.OilandGas.Models.Data.Calculations.PipelineAnalysisResult());
        }

        System.Threading.Tasks.Task<Beep.OilandGas.Models.Data.Calculations.PressureDropResult>
            Beep.OilandGas.Models.Core.Interfaces.IPipelineAnalysisService.CalculatePressureDropAsync(
                string pipelineId, decimal flowRate)
        {
            return System.Threading.Tasks.Task.FromResult(
                new Beep.OilandGas.Models.Data.Calculations.PressureDropResult());
        }

        System.Threading.Tasks.Task
            Beep.OilandGas.Models.Core.Interfaces.IPipelineAnalysisService.SaveAnalysisResultAsync(
                Beep.OilandGas.Models.Data.Calculations.PipelineAnalysisResult result, string userId)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
