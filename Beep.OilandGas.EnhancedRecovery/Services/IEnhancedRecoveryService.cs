using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

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
        Task<List<EnhancedRecoveryOperationDto>> GetEnhancedRecoveryOperationsAsync(string? fieldId = null);

        /// <summary>
        /// Gets an enhanced recovery operation by ID.
        /// </summary>
        Task<EnhancedRecoveryOperationDto?> GetEnhancedRecoveryOperationAsync(string operationId);

        /// <summary>
        /// Creates a new enhanced recovery operation.
        /// </summary>
        Task<EnhancedRecoveryOperationDto> CreateEnhancedRecoveryOperationAsync(CreateEnhancedRecoveryOperationDto createDto);

        /// <summary>
        /// Gets injection operations.
        /// </summary>
        Task<List<InjectionOperationDto>> GetInjectionOperationsAsync(string? wellUWI = null);

        /// <summary>
        /// Gets water flooding operations.
        /// </summary>
        Task<List<WaterFloodingDto>> GetWaterFloodingOperationsAsync(string? fieldId = null);

        /// <summary>
        /// Gets gas injection operations.
        /// </summary>
        Task<List<GasInjectionDto>> GetGasInjectionOperationsAsync(string? fieldId = null);
    }
}

