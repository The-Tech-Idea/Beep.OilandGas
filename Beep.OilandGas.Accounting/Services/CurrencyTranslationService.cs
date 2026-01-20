using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IAS 21 currency translation and FX gain/loss posting.
    /// </summary>
    public class CurrencyTranslationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<CurrencyTranslationService> _logger;
        private const string ConnectionName = "PPDM39";

        public CurrencyTranslationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<CurrencyTranslationService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        public async Task<decimal> TranslateAmountAsync(
            decimal amount,
            string fromCurrency,
            string toCurrency,
            DateTime rateDate,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency))
                throw new ArgumentNullException(nameof(fromCurrency));
            if (string.IsNullOrWhiteSpace(toCurrency))
                throw new ArgumentNullException(nameof(toCurrency));

            if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
                return amount;

            var rate = await GetFxRateAsync(fromCurrency, toCurrency, rateDate, connectionName);
            if (rate == null || rate.RATE == null || rate.RATE <= 0m)
                throw new InvalidOperationException($"FX rate missing for {fromCurrency}->{toCurrency} on {rateDate:yyyy-MM-dd}");

            return Math.Round(amount * rate.RATE.Value, 2);
        }

        public async Task<CURRENCY_TRANSLATION_RESULT> RecordTranslationResultAsync(
            string entityId,
            DateTime periodEnd,
            string originalCurrency,
            string reportingCurrency,
            decimal originalAmount,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentNullException(nameof(entityId));
            if (string.IsNullOrWhiteSpace(originalCurrency))
                throw new ArgumentNullException(nameof(originalCurrency));
            if (string.IsNullOrWhiteSpace(reportingCurrency))
                throw new ArgumentNullException(nameof(reportingCurrency));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var rate = await GetFxRateAsync(originalCurrency, reportingCurrency, periodEnd, cn);
            if (rate == null || rate.RATE == null || rate.RATE <= 0m)
                throw new InvalidOperationException($"FX rate missing for {originalCurrency}->{reportingCurrency} on {periodEnd:yyyy-MM-dd}");

            var translated = Math.Round(originalAmount * rate.RATE.Value, 2);

            var result = new CURRENCY_TRANSLATION_RESULT
            {
                TRANSLATION_RESULT_ID = Guid.NewGuid().ToString(),
                ENTITY_ID = entityId,
                PERIOD_END = periodEnd,
                REPORTING_CURRENCY = reportingCurrency,
                ORIGINAL_CURRENCY = originalCurrency,
                TRANSLATED_AMOUNT = translated,
                RATE_USED = rate.RATE,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<CURRENCY_TRANSLATION_RESULT>("CURRENCY_TRANSLATION_RESULT", cn);
            await repo.InsertAsync(result, userId);
            return result;
        }

        public async Task<JOURNAL_ENTRY> RecordFxGainLossAsync(
            string adjustmentAccount,
            decimal amountDifference,
            DateTime asOfDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(adjustmentAccount))
                throw new ArgumentNullException(nameof(adjustmentAccount));
            if (amountDifference == 0m)
                throw new InvalidOperationException("FX difference must be non-zero");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var gainAccount = GetAccountId(AccountMappingKeys.ForeignExchangeGain, DefaultGlAccounts.ForeignExchangeGain);
            var lossAccount = GetAccountId(AccountMappingKeys.ForeignExchangeLoss, DefaultGlAccounts.ForeignExchangeLoss);

            var lines = new List<JOURNAL_ENTRY_LINE>();
            if (amountDifference > 0m)
            {
                lines.Add(NewLine(adjustmentAccount, amountDifference, 0m, "FX revaluation"));
                lines.Add(NewLine(gainAccount, 0m, amountDifference, "FX gain"));
            }
            else
            {
                var amount = Math.Abs(amountDifference);
                lines.Add(NewLine(lossAccount, amount, 0m, "FX loss"));
                lines.Add(NewLine(adjustmentAccount, 0m, amount, "FX revaluation"));
            }

            var result = await _basisPosting.PostEntryAsync(
                asOfDate,
                "FX revaluation",
                lines,
                userId,
                referenceNumber: null,
                sourceModule: "FX");
            var entry = result.IfrsEntry ?? throw new InvalidOperationException("IFRS entry was not created.");
            return entry;
        }

        private async Task<FX_RATE?> GetFxRateAsync(string fromCurrency, string toCurrency, DateTime rateDate, string? connectionName)
        {
            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<FX_RATE>("FX_RATE", cn);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FROM_CURRENCY", Operator = "=", FilterValue = fromCurrency },
                new AppFilter { FieldName = "TO_CURRENCY", Operator = "=", FilterValue = toCurrency },
                new AppFilter { FieldName = "RATE_DATE", Operator = "<=", FilterValue = rateDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<FX_RATE>()
                .OrderByDescending(r => r.RATE_DATE)
                .FirstOrDefault();
        }

        private static JOURNAL_ENTRY_LINE NewLine(string accountId, decimal debit, decimal credit, string description)
        {
            return new JOURNAL_ENTRY_LINE
            {
                GL_ACCOUNT_ID = accountId,
                DEBIT_AMOUNT = debit == 0m ? null : debit,
                CREDIT_AMOUNT = credit == 0m ? null : credit,
                DESCRIPTION = description
            };
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}


