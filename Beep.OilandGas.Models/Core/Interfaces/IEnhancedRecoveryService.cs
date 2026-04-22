using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for enhanced oil recovery (EOR) operations.
    /// Provides EOR method selection, injection management, and recovery analysis.
    /// </summary>
    public interface IEnhancedRecoveryService
    {
        /// <summary>
        /// Analyzes EOR potential for a field or well.
        /// </summary>
        /// <param name="fieldId">Field identifier</param>
        /// <param name="eorMethod">EOR method to analyze</param>
        /// <returns>EOR analysis result</returns>
        Task<EnhancedRecoveryOperation> AnalyzeEORPotentialAsync(string fieldId, string eorMethod);

        /// <summary>
        /// Calculates recovery factor for EOR project.
        /// </summary>
        /// <param name="operationId">EOR operation identifier</param>
        /// <returns>Recovery factor calculation result</returns>
        Task<EnhancedRecoveryOperation> CalculateRecoveryFactorAsync(string operationId);

        /// <summary>
        /// Calculates pilot economics for an EOR implementation.
        /// </summary>
        /// <param name="fieldId">Field identifier</param>
        /// <param name="estimatedIncrementalOil">Estimated incremental oil in barrels</param>
        /// <param name="oilPrice">Oil price in USD per barrel</param>
        /// <param name="capitalCost">Capital cost in USD</param>
        /// <param name="operatingCostPerBarrel">Operating cost in USD per barrel</param>
        /// <param name="projectLifeYears">Project life in years</param>
        /// <param name="discountRate">Discount rate as a decimal fraction</param>
        /// <returns>EOR economic analysis result</returns>
        Task<EOREconomicAnalysis> AnalyzeEOReconomicsAsync(
            string fieldId,
            double estimatedIncrementalOil,
            double oilPrice,
            double capitalCost,
            double operatingCostPerBarrel,
            int projectLifeYears,
            double discountRate = 0.10);

        /// <summary>
        /// Gets stored injection operations, optionally scoped to a well.
        /// </summary>
        /// <param name="wellUWI">Optional injection well identifier</param>
        /// <returns>Stored injection operations</returns>
        Task<List<InjectionOperation>> GetInjectionOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Manages injection well operations.
        /// </summary>
        /// <param name="injectionWellId">Injection well identifier</param>
        /// <param name="injectionRate">Injection rate</param>
        /// <returns>Injection operation result</returns>
        Task<InjectionOperation> ManageInjectionAsync(string injectionWellId, decimal injectionRate);
    }
}





