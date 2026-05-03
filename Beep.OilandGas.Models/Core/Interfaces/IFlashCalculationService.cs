using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for flash calculation operations.
    /// Provides phase equilibrium calculations, multi-stage flash, and component management.
    /// </summary>
    public interface IFlashCalculationService
    {
        /// <summary>
        /// Performs isothermal flash calculation.
        /// </summary>
        /// <param name="conditions">Flash calculation conditions</param>
        /// <returns>Flash calculation result</returns>
        FlashResult PerformIsothermalFlash(FLASH_CONDITIONS conditions);

        /// <summary>
        /// Performs multi-stage flash calculation.
        /// </summary>
        /// <param name="conditions">Flash calculation conditions</param>
        /// <param name="stages">Number of stages</param>
        /// <returns>Multi-stage flash results</returns>
        List<FlashResult> PerformMultiStageFlash(FLASH_CONDITIONS conditions, int stages);

        /// <summary>
        /// Saves flash calculation result to database.
        /// </summary>
        /// <param name="result">Flash calculation result</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveFlashResultAsync(FlashResult result, string userId);

        /// <summary>
        /// Gets flash calculation history.
        /// </summary>
        /// <param name="componentId">Component identifier (optional)</param>
        /// <returns>List of flash calculation results</returns>
        Task<List<FlashResult>> GetFlashHistoryAsync(string? componentId = null);

        /// <summary>
        /// Runs PT flash using the same Wilson + Rachford–Rice path as the calculator, returning a persisted-style
        /// <see cref="FlashCalculationResult"/> envelope.
        /// </summary>
        /// <param name="request">Pressure, temperature, and feed composition.</param>
        /// <param name="cancellationToken">Cancellation token (honored before heavy steps).</param>
        Task<FlashCalculationResult> RunRigorousFlashAsync(
            FlashCalculationRequest request,
            CancellationToken cancellationToken = default);
    }
}




