using System.Threading.Tasks;

namespace Beep.OilandGas.HydraulicPumps.Services
{
    public partial class HydraulicPumpService
    {
        // Explicit implementations of Models.Core.Interfaces.IHydraulicPumpService
        System.Threading.Tasks.Task<Beep.OilandGas.Models.Data.HydraulicPumpDesign>
            Beep.OilandGas.Models.Core.Interfaces.IHydraulicPumpService.DesignPumpSystemAsync(
                string wellUWI, string pumpType, decimal wellDepth, decimal desiredFlowRate)
        {
            return System.Threading.Tasks.Task.FromResult(
                new Beep.OilandGas.Models.Data.HydraulicPumpDesign());
        }

        System.Threading.Tasks.Task<Beep.OilandGas.Models.Data.PumpPerformanceAnalysis>
            Beep.OilandGas.Models.Core.Interfaces.IHydraulicPumpService.AnalyzePumpPerformanceAsync(
                string pumpId)
        {
            return System.Threading.Tasks.Task.FromResult(
                new Beep.OilandGas.Models.Data.PumpPerformanceAnalysis());
        }

        System.Threading.Tasks.Task
            Beep.OilandGas.Models.Core.Interfaces.IHydraulicPumpService.SavePumpDesignAsync(
                Beep.OilandGas.Models.Data.HydraulicPumpDesign design, string userId)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        System.Threading.Tasks.Task<System.Collections.Generic.List<Beep.OilandGas.Models.Data.PumpPerformanceHistory>>
            Beep.OilandGas.Models.Core.Interfaces.IHydraulicPumpService.GetPumpPerformanceHistoryAsync(
                string pumpId)
        {
            return System.Threading.Tasks.Task.FromResult(
                new System.Collections.Generic.List<Beep.OilandGas.Models.Data.PumpPerformanceHistory>());
        }
    }
}
