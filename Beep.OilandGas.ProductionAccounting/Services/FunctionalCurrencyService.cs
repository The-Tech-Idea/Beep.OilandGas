using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;

using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;


namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// IAS 21 functional currency and FX translation service.
    /// Company translation requires recorded book and original source evidence.
    /// Missing rates and unavailable balances are refused without substitute values.
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
            string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fromCurrency))
                throw new ArgumentNullException(nameof(fromCurrency));
            if (string.IsNullOrWhiteSpace(toCurrency))
                throw new ArgumentNullException(nameof(toCurrency));

            if(string.Equals(fromCurrency,toCurrency,StringComparison.Ordinal))return amount;
            var rate = await GetFxRateAsync(fromCurrency, toCurrency, rateDate, connectionName);
            return amount * rate;
        }

        public Task<CURRENCY_TRANSLATION_RESULT> TranslateBalancesAsync(string entityId,DateTime periodEnd,string reportingCurrency,string connectionName="PPDM39")
        {
            if(string.IsNullOrWhiteSpace(entityId))throw new ArgumentNullException(nameof(entityId));
            if(string.IsNullOrWhiteSpace(reportingCurrency))throw new ArgumentNullException(nameof(reportingCurrency));
            throw new InvalidOperationException("Recorded entity/book balances and explicit original currency are required. No placeholder balance or default USD currency is persisted.");
        }
        private async Task<decimal> GetFxRateAsync(
            string fromCurrency,
            string toCurrency,
            DateTime rateDate,
            string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync("FX_RATE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(FX_RATE);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, "FX_RATE");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FROM_CURRENCY", Operator = "=", FilterValue = fromCurrency },
                new AppFilter { FieldName = "TO_CURRENCY", Operator = "=", FilterValue = toCurrency },
                new AppFilter { FieldName = "RATE_DATE", Operator = "<=", FilterValue = rateDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            var rows=results?.ToArray() ??throw new InvalidOperationException("The complete recorded FX query is unavailable.");
            if(rows.Any(row=>row is not FX_RATE || TheTechIdea.Data.OilGas.CompanyFxRateProof.IsCompanyMarked((FX_RATE)row)))throw new InvalidOperationException("Company FX requires the recorded functional book, precision and original company source.");
            var rates=rows.Cast<FX_RATE>().OrderByDescending(row=>row.RATE_DATE).ToArray();
            var rateValue=rates.FirstOrDefault()?.RATE;
            if(rateValue is null || rateValue<=0)throw new InvalidOperationException("No positive recorded directed FX rate is available; no rate is substituted.");

            return rateValue.Value;
        }
    }
}
