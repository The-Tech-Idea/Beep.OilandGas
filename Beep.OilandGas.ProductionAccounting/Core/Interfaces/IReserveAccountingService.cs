using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for reserve accounting inputs used in depletion and impairment testing.
    /// </summary>
    public interface IReserveAccountingService
    {
        Task<PROVED_RESERVES> RecordReservesAsync(PROVED_RESERVES reserves, string userId, string cn = "PPDM39");
        Task<PROVED_RESERVES> GetLatestReservesAsync(string propertyId, DateTime? asOfDate = null, string cn = "PPDM39");
        Task<decimal> CalculateDepletionRateAsync(string propertyId, decimal netCapitalizedCosts, DateTime? asOfDate = null, string cn = "PPDM39");
        Task<bool> ValidateReservesAsync(PROVED_RESERVES reserves, string cn = "PPDM39");
        Task<RESERVE_CASHFLOW> RecordCashflowAsync(RESERVE_CASHFLOW cashflow, string userId, string cn = "PPDM39");
        Task<List<RESERVE_CASHFLOW>> GetCashflowsAsync(string propertyId, DateTime? asOfDate = null, string cn = "PPDM39");
        Task<decimal> CalculatePresentValueAsync(string propertyId, DateTime? asOfDate = null, string cn = "PPDM39");
    }
}
