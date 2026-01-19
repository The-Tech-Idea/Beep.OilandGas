using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// IAS 21 functional currency and FX translation service.
    /// </summary>
    public class FunctionalCurrencyService : IFunctionalCurrencyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<FunctionalCurrencyService> _logger;

        public FunctionalCurrencyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<FunctionalCurrencyService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<decimal> TranslateAmountAsync(
            decimal amount,
            string fromCurrency,
            string toCurrency,
            DateTime rateDate,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fromCurrency))
                throw new ArgumentNullException(nameof(fromCurrency));
            if (string.IsNullOrWhiteSpace(toCurrency))
                throw new ArgumentNullException(nameof(toCurrency));

            var rate = await GetFxRateAsync(fromCurrency, toCurrency, rateDate, cn);
            return amount * rate;
        }

        public async Task<CURRENCY_TRANSLATION_RESULT> TranslateBalancesAsync(
            string entityId,
            DateTime periodEnd,
            string reportingCurrency,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentNullException(nameof(entityId));
            if (string.IsNullOrWhiteSpace(reportingCurrency))
                throw new ArgumentNullException(nameof(reportingCurrency));

            // Placeholder: in a full implementation, fetch trial balance by entity.
            const decimal placeholderBalance = 0m;
            const string baseCurrency = "USD";

            var translatedAmount = await TranslateAmountAsync(
                placeholderBalance,
                baseCurrency,
                reportingCurrency,
                periodEnd,
                cn);

            var rate = await GetFxRateAsync(baseCurrency, reportingCurrency, periodEnd, cn);

            var result = new CURRENCY_TRANSLATION_RESULT
            {
                TRANSLATION_RESULT_ID = Guid.NewGuid().ToString(),
                ENTITY_ID = entityId,
                PERIOD_END = periodEnd,
                REPORTING_CURRENCY = reportingCurrency,
                ORIGINAL_CURRENCY = baseCurrency,
                TRANSLATED_AMOUNT = translatedAmount,
                RATE_USED = rate,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = "system",
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var metadata = await _metadata.GetTableMetadataAsync("CURRENCY_TRANSLATION_RESULT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(CURRENCY_TRANSLATION_RESULT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "CURRENCY_TRANSLATION_RESULT");

            await repo.InsertAsync(result, result.ROW_CREATED_BY);

            _logger?.LogInformation(
                "Translated balances for entity {EntityId} to {Currency} at rate {Rate}",
                entityId, reportingCurrency, rate);

            return result;
        }

        private async Task<decimal> GetFxRateAsync(
            string fromCurrency,
            string toCurrency,
            DateTime rateDate,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("FX_RATE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(FX_RATE);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "FX_RATE");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FROM_CURRENCY", Operator = "=", FilterValue = fromCurrency },
                new AppFilter { FieldName = "TO_CURRENCY", Operator = "=", FilterValue = toCurrency },
                new AppFilter { FieldName = "RATE_DATE", Operator = "<=", FilterValue = rateDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var rates = results?.Cast<FX_RATE>().OrderByDescending(r => r.RATE_DATE).ToList()
                ?? new List<FX_RATE>();

            var rateValue = rates.FirstOrDefault()?.RATE ?? 1m;
            if (rateValue <= 0m)
                rateValue = 1m;

            return rateValue;
        }
    }
}
