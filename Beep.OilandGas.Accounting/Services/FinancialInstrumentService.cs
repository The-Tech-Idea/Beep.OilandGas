using System;
using System.Collections.Generic;
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
    /// IAS 32/IAS 39 financial instruments recognition and remeasurement.
    /// </summary>
    public class FinancialInstrumentService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<FinancialInstrumentService> _logger;
        private const string ConnectionName = "PPDM39";

        public FinancialInstrumentService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<FinancialInstrumentService> logger,
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

        public async Task<ACCOUNTING_COST> RecordInstrumentAsync(
            string instrumentId,
            DateTime tradeDate,
            decimal amount,
            bool isLiability,
            string userId,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(instrumentId))
                throw new ArgumentNullException(nameof(instrumentId));
            if (amount <= 0m)
                throw new InvalidOperationException("Instrument amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = instrumentId,
                COST_TYPE = "FINANCIAL_INSTRUMENT",
                COST_CATEGORY = isLiability ? "LIABILITY" : "ASSET",
                AMOUNT = amount,
                COST_DATE = tradeDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorYes(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Financial instrument {(isLiability ? "liability" : "asset")} {instrumentId}",
                SOURCE = "IAS32_39",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var cashAccount = string.IsNullOrWhiteSpace(cashAccountId)
                ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                : cashAccountId;

            if (isLiability)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    cashAccount,
                    GetAccountId(AccountMappingKeys.FinancialInstrumentLiability, DefaultGlAccounts.FinancialInstrumentLiability),
                    amount,
                    $"Financial instrument liability {instrumentId}",
                    userId,
                    cn);
            }
            else
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.FinancialInstrumentAsset, DefaultGlAccounts.FinancialInstrumentAsset),
                    cashAccount,
                    amount,
                    $"Financial instrument asset {instrumentId}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IAS 32/39 instrument {Instrument} amount {Amount}",
                instrumentId, amount);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordFairValueChangeAsync(
            string instrumentId,
            DateTime valuationDate,
            decimal changeAmount,
            bool isLiability,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(instrumentId))
                throw new ArgumentNullException(nameof(instrumentId));
            if (changeAmount == 0m)
                throw new InvalidOperationException("Fair value change cannot be zero");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = instrumentId,
                COST_TYPE = "FINANCIAL_INSTRUMENT",
                COST_CATEGORY = "FAIR_VALUE_CHANGE",
                AMOUNT = Math.Abs(changeAmount),
                COST_DATE = valuationDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Financial instrument fair value change {instrumentId}",
                SOURCE = "IAS32_39",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            if (!isLiability)
            {
                if (changeAmount > 0m)
                {
                    await _basisPosting.PostBalancedEntryByAccountAsync(
                        GetAccountId(AccountMappingKeys.FinancialInstrumentAsset, DefaultGlAccounts.FinancialInstrumentAsset),
                        GetAccountId(AccountMappingKeys.FinancialInstrumentGain, DefaultGlAccounts.FinancialInstrumentGain),
                        changeAmount,
                        $"Financial instrument gain {instrumentId}",
                        userId,
                        cn);
                }
                else
                {
                    var amount = Math.Abs(changeAmount);
                    await _basisPosting.PostBalancedEntryByAccountAsync(
                        GetAccountId(AccountMappingKeys.FinancialInstrumentLoss, DefaultGlAccounts.FinancialInstrumentLoss),
                        GetAccountId(AccountMappingKeys.FinancialInstrumentAsset, DefaultGlAccounts.FinancialInstrumentAsset),
                        amount,
                        $"Financial instrument loss {instrumentId}",
                        userId,
                        cn);
                }
            }
            else
            {
                if (changeAmount > 0m)
                {
                    await _basisPosting.PostBalancedEntryByAccountAsync(
                        GetAccountId(AccountMappingKeys.FinancialInstrumentLoss, DefaultGlAccounts.FinancialInstrumentLoss),
                        GetAccountId(AccountMappingKeys.FinancialInstrumentLiability, DefaultGlAccounts.FinancialInstrumentLiability),
                        changeAmount,
                        $"Financial instrument liability increase {instrumentId}",
                        userId,
                        cn);
                }
                else
                {
                    var amount = Math.Abs(changeAmount);
                    await _basisPosting.PostBalancedEntryByAccountAsync(
                        GetAccountId(AccountMappingKeys.FinancialInstrumentLiability, DefaultGlAccounts.FinancialInstrumentLiability),
                        GetAccountId(AccountMappingKeys.FinancialInstrumentGain, DefaultGlAccounts.FinancialInstrumentGain),
                        amount,
                        $"Financial instrument liability decrease {instrumentId}",
                        userId,
                        cn);
                }
            }

            _logger?.LogInformation("Recorded IAS 32/39 fair value change {Instrument} amount {Amount}",
                instrumentId, changeAmount);

            return cost;
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



