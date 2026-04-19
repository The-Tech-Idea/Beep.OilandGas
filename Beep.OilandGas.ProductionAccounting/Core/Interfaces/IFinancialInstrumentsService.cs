using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for IFRS 9 financial instruments and hedge accounting.
    /// </summary>
    public interface IFinancialInstrumentsService
    {
        Task<FINANCIAL_INSTRUMENT> UpdateFairValueAsync(
            FINANCIAL_INSTRUMENT instrument,
            decimal fairValue,
            DateTime valuationDate,
            string userId,
            string cn = "PPDM39");

        Task<HEDGE_MEASUREMENT> MeasureHedgeAsync(
            HEDGE_RELATIONSHIP hedge,
            decimal hedgedItemChange,
            decimal hedgingInstrumentChange,
            DateTime measurementDate,
            string userId,
            string cn = "PPDM39");
    }
}
