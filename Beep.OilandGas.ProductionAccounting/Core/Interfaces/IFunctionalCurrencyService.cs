using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for IAS 21 functional currency and FX translation.
    /// </summary>
    public interface IFunctionalCurrencyService
    {
        Task<decimal> TranslateAmountAsync(
            decimal amount,
            string fromCurrency,
            string toCurrency,
            DateTime rateDate,
            string cn = "PPDM39");

        Task<CURRENCY_TRANSLATION_RESULT> TranslateBalancesAsync(
            string entityId,
            DateTime periodEnd,
            string reportingCurrency,
            string cn = "PPDM39");
    }
}
