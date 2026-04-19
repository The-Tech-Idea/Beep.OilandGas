using System.Threading.Tasks;

namespace Beep.OilandGas.PlungerLift.Services
{
    public partial class PlungerLiftService
    {
        // Explicit implementations of Models.Core.Interfaces.IPlungerLiftService
        System.Threading.Tasks.Task<Beep.OilandGas.Models.Data.PlungerLift.PlungerLiftDesign>
            Beep.OilandGas.Models.Core.Interfaces.IPlungerLiftService.DesignPlungerLiftSystemAsync(
                string wellUWI, Beep.OilandGas.Models.Data.PlungerLift.PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            return System.Threading.Tasks.Task.FromResult(
                new Beep.OilandGas.Models.Data.PlungerLift.PlungerLiftDesign());
        }

        System.Threading.Tasks.Task<Beep.OilandGas.Models.Data.PlungerLift.PlungerLiftPerformance>
            Beep.OilandGas.Models.Core.Interfaces.IPlungerLiftService.AnalyzePerformanceAsync(
                string wellUWI)
        {
            return System.Threading.Tasks.Task.FromResult(
                new Beep.OilandGas.Models.Data.PlungerLift.PlungerLiftPerformance());
        }

        System.Threading.Tasks.Task
            Beep.OilandGas.Models.Core.Interfaces.IPlungerLiftService.SavePlungerLiftDesignAsync(
                Beep.OilandGas.Models.Data.PlungerLift.PlungerLiftDesign design, string userId)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
