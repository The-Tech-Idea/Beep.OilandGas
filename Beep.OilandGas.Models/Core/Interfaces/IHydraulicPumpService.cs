using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for hydraulic pump operations.
    /// Provides pump design, performance analysis, and optimization.
    /// </summary>
    public interface IHydraulicPumpService
    {
        /// <summary>
        /// Designs a hydraulic pump system.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="pumpType">Pump type</param>
        /// <param name="wellDepth">Well depth</param>
        /// <param name="desiredFlowRate">Desired flow rate</param>
        /// <returns>Pump design result</returns>
        Task<HydraulicPumpDesign> DesignPumpSystemAsync(string wellUWI, string pumpType, decimal wellDepth, decimal desiredFlowRate);

        /// <summary>
        /// Analyzes pump performance.
        /// </summary>
        /// <param name="pumpId">Pump identifier</param>
        /// <returns>Performance analysis result</returns>
        Task<PumpPerformanceAnalysis> AnalyzePumpPerformanceAsync(string pumpId);

        /// <summary>
        /// Saves pump design to database.
        /// </summary>
        /// <param name="design">Pump design</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SavePumpDesignAsync(HydraulicPumpDesign design, string userId);

        /// <summary>
        /// Gets pump performance history.
        /// </summary>
        /// <param name="pumpId">Pump identifier</param>
        /// <returns>Performance history data</returns>
        Task<List<PumpPerformanceHistory>> GetPumpPerformanceHistoryAsync(string pumpId);
    }
}




