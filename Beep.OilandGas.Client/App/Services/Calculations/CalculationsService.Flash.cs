using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.Client.App.Services.Calculations
{
    internal partial class CalculationsService
    {
        #region Flash Calculation

        public async Task<FlashResult> PerformIsothermalFlashAsync(FlashConditions request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<FlashConditions, FlashResult>("/api/flashcalculation/isothermal", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<FlashResult>> PerformMultiStageFlashAsync(FlashConditions request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<FlashConditions, List<FlashResult>>("/api/flashcalculation/multistage", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FLASH_CALCULATION_RESULT> SaveFlashResultAsync(FLASH_CALCULATION_RESULT result, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/flashcalculation/result", queryParams);
                return await PostAsync<FLASH_CALCULATION_RESULT, FLASH_CALCULATION_RESULT>(endpoint, result, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<FLASH_CALCULATION_RESULT>> GetFlashHistoryAsync(string compositionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(compositionId)) throw new ArgumentException("Composition ID is required", nameof(compositionId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<FLASH_CALCULATION_RESULT>>($"/api/flashcalculation/history/{Uri.EscapeDataString(compositionId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
