using System.Collections.Generic;
using System.Threading.Tasks;
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
        GasLiftPotentialResult AnalyzeGasLiftPotential(
            GasLiftWellProperties wellProperties,
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
        GasLiftValveDesignResult DesignValves(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves,
            bool useSIUnits = false);

        /// <summary>
        /// Saves gas lift design to database.
        /// </summary>
        /// <param name="design">Gas lift design</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveGasLiftDesignAsync(Beep.OilandGas.Models.Data.GasLiftDesign design, string userId);

        /// <summary>
        /// Gets gas lift performance data.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <returns>Gas lift performance data</returns>
        Task<Beep.OilandGas.Models.Data.GasLiftPerformance> GetGasLiftPerformanceAsync(string wellUWI);
    }
}




