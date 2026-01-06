using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.DTOs.Calculations;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for calculation operations (PPDM39 version).
    /// Wraps static calculation methods and optionally tracks calculation history.
    /// Note: This is the PPDM39-specific calculation service. For lifecycle calculations, see ICalculationService.
    /// </summary>
    public interface ICalculationService_PPDM
    {
        /// <summary>
        /// Calculates decline rate.
        /// </summary>
        Task<CalculationResultResponse> CalculateDeclineRateAsync(CalculateDeclineRateRequest request, string userId, bool trackHistory = false, string? connectionName = null);
        
        /// <summary>
        /// Calculates volume (net or gross).
        /// </summary>
        Task<CalculationResultResponse> CalculateVolumeAsync(CalculateVolumeRequest request, string userId, bool trackHistory = false, string? connectionName = null);
        
        /// <summary>
        /// Calculates API gravity or specific gravity.
        /// </summary>
        Task<CalculationResultResponse> CalculateApiGravityAsync(CalculateApiGravityRequest request, string userId, bool trackHistory = false, string? connectionName = null);
        
        /// <summary>
        /// Saves calculation result to history.
        /// </summary>
        Task<CALCULATION_RESULT> SaveCalculationResultAsync(CalculationResultResponse result, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets calculation history.
        /// </summary>
        Task<List<CALCULATION_RESULT>> GetCalculationHistoryAsync(string? calculationType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    }
}




