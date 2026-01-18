using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for product pricing and revenue calculation.
    /// </summary>
    public interface IPricingService
    {
        Task<decimal> GetPriceAsync(string productId, DateTime date, string cn = "PPDM39");
        Task<List<PRICE_INDEX>> GetHistoryAsync(string productId, DateTime start, DateTime end, string cn = "PPDM39");
        Task<decimal> CalculateRevenueAsync(string productId, decimal volume, DateTime date, string cn = "PPDM39");
        Task<decimal> GetAveragePriceAsync(string productId, DateTime start, DateTime end, string cn = "PPDM39");
    }
}
