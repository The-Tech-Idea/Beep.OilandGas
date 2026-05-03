using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.EnhancedRecovery.Services
{
    /// <summary>
    /// Extended enhanced-recovery operations available from <see cref="EnhancedRecoveryService"/>:
    /// listings and CRUD-style helpers beyond <see cref="Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService"/>.
    /// </summary>
    public interface IEnhancedRecoveryOperationsService
    {
        /// <summary>
        /// Gets enhanced recovery operations (non-injection PDEN schemes), optionally scoped by field.
        /// </summary>
        Task<List<EnhancedRecoveryOperation>> GetEnhancedRecoveryOperationsAsync(string? fieldId = null);

        /// <summary>
        /// Gets an enhanced recovery operation by PDEN / operation id.
        /// </summary>
        Task<EnhancedRecoveryOperation?> GetEnhancedRecoveryOperationAsync(string operationId);

        /// <summary>
        /// Creates a new enhanced recovery operation (PDEN-backed).
        /// </summary>
        Task<EnhancedRecoveryOperation> CreateEnhancedRecoveryOperationAsync(CreateEnhancedRecoveryOperation createDto);

        /// <summary>
        /// Gets injection operations.
        /// </summary>
        Task<List<InjectionOperation>> GetInjectionOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Gets water flooding operations (<c>PDEN_SUBTYPE = WATER_FLOOD</c>).
        /// </summary>
        Task<List<WaterFlooding>> GetWaterFloodingOperationsAsync(string? fieldId = null);

        /// <summary>
        /// Gets gas injection operations (<c>PDEN_SUBTYPE = GAS_INJECTION</c>).
        /// </summary>
        Task<List<GasInjection>> GetGasInjectionOperationsAsync(string? fieldId = null);

        /// <summary>
        /// Calculates pilot economics for an EOR implementation.
        /// </summary>
        Task<EOREconomicAnalysis> AnalyzeEOReconomicsAsync(
            string fieldId,
            double estimatedIncrementalOil,
            double oilPrice,
            double capitalCost,
            double operatingCostPerBarrel,
            int projectLifeYears,
            double discountRate = 0.10);
    }
}
