using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Pumps;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for sucker rod pumping operations.
    /// Provides pump design, performance analysis, and optimization.
    /// </summary>
    public interface ISuckerRodPumpingService
    {
        /// <summary>
        /// Designs a sucker rod pumping system.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="wellProperties">Well properties</param>
        /// <returns>Sucker rod pump design result</returns>
        Task<SuckerRodPumpDesign> DesignPumpSystemAsync(string wellUWI, SuckerRodPumpWellProperties wellProperties);

        /// <summary>
        /// Analyzes sucker rod pump performance.
        /// </summary>
        /// <param name="pumpId">Pump identifier</param>
        /// <returns>Performance analysis result</returns>
        Task<SuckerRodPumpPerformance> AnalyzePerformanceAsync(string pumpId);

        /// <summary>
        /// Saves sucker rod pump design to database.
        /// </summary>
        /// <param name="design">Pump design</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SavePumpDesignAsync(SuckerRodPumpDesign design, string userId);
    }
}




