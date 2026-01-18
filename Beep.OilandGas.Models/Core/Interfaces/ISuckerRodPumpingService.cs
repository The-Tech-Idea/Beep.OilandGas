using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.Pumps;

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
        Task<SuckerRodPumpDesignDto> DesignPumpSystemAsync(string wellUWI, SuckerRodPumpWellPropertiesDto wellProperties);

        /// <summary>
        /// Analyzes sucker rod pump performance.
        /// </summary>
        /// <param name="pumpId">Pump identifier</param>
        /// <returns>Performance analysis result</returns>
        Task<SuckerRodPumpPerformanceDto> AnalyzePerformanceAsync(string pumpId);

        /// <summary>
        /// Saves sucker rod pump design to database.
        /// </summary>
        /// <param name="design">Pump design</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SavePumpDesignAsync(SuckerRodPumpDesignDto design, string userId);
    }
}




