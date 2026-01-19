using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for amortization/depletion calculations.
    /// Supports Unit of Production and other methods.
    /// </summary>
    public interface IAmortizationService
    {
        Task<AMORTIZATION_RECORD> CalculateAsync(string assetId, DateTime period, string userId, string cn = "PPDM39");
        Task<AMORTIZATION_RECORD> CalculateFieldwideAsync(string fieldId, DateTime period, string userId, string cn = "PPDM39");
        Task<AMORTIZATION_SPLIT> CalculateSplitAsync(string propertyId, DateTime periodEnd, string userId, string cn = "PPDM39");
        Task<decimal> GetAccumulatedDepletionAsync(string assetId, string cn = "PPDM39");
        Task<DEPLETION_ROLLFORWARD> GenerateRollforwardAsync(string propertyId, DateTime periodStart, DateTime periodEnd, string userId, string cn = "PPDM39");
        Task<bool> ValidateAsync(AMORTIZATION_RECORD record, string cn = "PPDM39");
    }
}
