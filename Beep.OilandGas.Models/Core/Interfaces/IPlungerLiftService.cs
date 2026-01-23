using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for plunger lift operations.
    /// Provides plunger lift design, optimization, and performance monitoring.
    /// </summary>
    public interface IPlungerLiftService
    {
        /// <summary>
        /// Designs a plunger lift system for a well.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="wellProperties">Well properties</param>
        /// <returns>Plunger lift design result</returns>
        Task<PlungerLiftDesign> DesignPlungerLiftSystemAsync(string wellUWI, PlungerLiftWellProperties wellProperties);

        /// <summary>
        /// Analyzes plunger lift performance.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <returns>Performance analysis result</returns>
        Task<PlungerLiftPerformance> AnalyzePerformanceAsync(string wellUWI);

        /// <summary>
        /// Saves plunger lift design to database.
        /// </summary>
        /// <param name="design">Plunger lift design</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SavePlungerLiftDesignAsync(PlungerLiftDesign design, string userId);
    }

    /// <summary>
    /// DTO for plunger lift design.
    /// </summary>
    



    /// <summary>
    /// DTO for plunger lift performance.
    /// </summary>
    
}





