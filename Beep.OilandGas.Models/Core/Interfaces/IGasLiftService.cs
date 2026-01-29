using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.GasLift;


namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for gas lift operations.
    /// Provides gas lift design, optimization, and performance monitoring.
    /// </summary>
    public interface IGasLiftService
    {
        /// <summary>
        /// Analyzes gas lift potential for a well.
        /// </summary>
        /// <param name="wellProperties">Well properties for gas lift analysis</param>
        /// <param name="minGasInjectionRate">Minimum gas injection rate</param>
        /// <param name="maxGasInjectionRate">Maximum gas injection rate</param>
        /// <param name="numberOfPoints">Number of analysis points</param>
        /// <returns>Gas lift potential result</returns>
        GAS_LIFT_WELL_PROPERTIES AnalyzeGasLiftPotential(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal minGasInjectionRate,
            decimal maxGasInjectionRate,
            int numberOfPoints = 50);

        /// <summary>
        /// Designs gas lift valves for a well.
        /// </summary>
        /// <param name="wellProperties">Well properties</param>
        /// <param name="gasInjectionPressure">Gas injection pressure</param>
        /// <param name="numberOfValves">Number of valves</param>
        /// <param name="useSIUnits">Whether to use SI units (default false for US field units)</param>
        /// <returns>Gas lift valve design result</returns>
        GAS_LIFT_VALVE_DESIGN_RESULT DesignValves(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves,
            bool useSIUnits = false);

        /// <summary>
        /// Saves gas lift design to database.
        /// </summary>
        /// <param name="design">Gas lift design</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveGasLiftDesignAsync(GAS_LIFT_DESIGN design, string userId);

        /// <summary>
        /// Gets gas lift performance data.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <returns>Gas lift performance data</returns>
        Task<GAS_LIFT_PERFORMANCE> GetGasLiftPerformanceAsync(string wellUWI);
    }
}




