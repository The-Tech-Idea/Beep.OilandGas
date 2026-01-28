using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;

namespace Beep.OilandGas.Client.App.Services.Pumps
{
    /// <summary>
    /// Service interface for all Pump operations
    /// </summary>
    public interface IPumpsService
    {
        #region Hydraulic Pump

        Task<HYDRAULIC_JET_PUMP_RESULT> DesignHydraulicJetPumpAsync(HYDRAULIC_JET_PUMP_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<HYDRAULIC_PISTON_PUMP_RESULT> DesignHydraulicPistonPumpAsync(HYDRAULIC_PISTON_PUMP_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<HYDRAULIC_JET_PUMP_RESULT> AnalyzeHydraulicPumpPerformanceAsync(HYDRAULIC_PUMP_WELL_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<HYDRAULIC_JET_PUMP_RESULT> SaveHydraulicPumpDesignAsync(HYDRAULIC_JET_PUMP_RESULT design, string? userId = null, CancellationToken cancellationToken = default);
        Task<List<HYDRAULIC_JET_PUMP_RESULT>> GetHydraulicPumpHistoryAsync(string pumpId, CancellationToken cancellationToken = default);

        #endregion

        #region Plunger Lift

        Task<PlungerLiftPerformanceResult> DesignPlungerLiftAsync(PLUNGER_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<PLUNGER_LIFT_CYCLE_RESULT> AnalyzePlungerLiftPerformanceAsync(PLUNGER_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<PLUNGER_LIFT_CYCLE_RESULT> SavePlungerLiftDesignAsync(PLUNGER_LIFT_CYCLE_RESULT design, string? userId = null, CancellationToken cancellationToken = default);

        #endregion

        #region Sucker Rod Pumping

        Task<SUCKER_ROD_FLOW_RATE_POWER_RESULT> DesignSuckerRodPumpAsync(SUCKER_ROD_SYSTEM_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<SUCKER_ROD_LOAD_RESULT> AnalyzeSuckerRodPumpPerformanceAsync(SUCKER_ROD_SYSTEM_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<SUCKER_ROD_FLOW_RATE_POWER_RESULT> SaveSuckerRodPumpDesignAsync(SUCKER_ROD_FLOW_RATE_POWER_RESULT design, string? userId = null, CancellationToken cancellationToken = default);

        #endregion
    }
}
