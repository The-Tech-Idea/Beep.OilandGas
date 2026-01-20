using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.EnhancedRecovery.Services
{
    /// <summary>
    /// Service for managing enhanced recovery operations.
    /// </summary>
    public interface IEnhancedRecoveryService
    {
        /// <summary>
        /// Gets all enhanced recovery operations.
        /// </summary>
        Task<List<EnhancedRecoveryOperation>> GetEnhancedRecoveryOperationsAsync(string? fieldId = null);

        /// <summary>
        /// Gets an enhanced recovery operation by ID.
        /// </summary>
        Task<EnhancedRecoveryOperation?> GetEnhancedRecoveryOperationAsync(string operationId);

        /// <summary>
        /// Creates a new enhanced recovery operation.
        /// </summary>
        Task<EnhancedRecoveryOperation> CreateEnhancedRecoveryOperationAsync(CreateEnhancedRecoveryOperation createDto);

        /// <summary>
        /// Gets injection operations.
        /// </summary>
        Task<List<InjectionOperation>> GetInjectionOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Gets water flooding operations.
        /// </summary>
        Task<List<WaterFlooding>> GetWaterFloodingOperationsAsync(string? fieldId = null);

        /// <summary>
        /// Gets gas injection operations.
        /// </summary>
        Task<List<GasInjection>> GetGasInjectionOperationsAsync(string? fieldId = null);
    }
}

