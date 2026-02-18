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
    /// IAS 29 hyperinflationary restatement support.
    /// </summary>
    public class HyperinflationRestatementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<HyperinflationRestatementService> _logger;
        private const string ConnectionName = "PPDM39";

        public HyperinflationRestatementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<HyperinflationRestatementService> logger,
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

        public async Task<ACCOUNTING_COST> RestateNonMonetaryBalanceAsync(
            string accountId,
            decimal originalAmount,
            decimal indexAtAcquisition,
            decimal indexAtReporting,
            DateTime restatementDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new ArgumentNullException(nameof(accountId));
            if (indexAtAcquisition <= 0m || indexAtReporting <= 0m)
                throw new InvalidOperationException("Price indices must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var restatedAmount = originalAmount * (indexAtReporting / indexAtAcquisition);
            var delta = restatedAmount - originalAmount;
            if (delta == 0m)
                throw new InvalidOperationException("Restatement delta is zero");

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "HYPERINFLATION",
                COST_TYPE = "HYPERINFLATION_RESTATEMENT",
                COST_CATEGORY = "NON_MONETARY",
                AMOUNT = Math.Abs(delta),
                COST_DATE = restatementDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"IAS 29 restatement for {accountId}",
                REMARK = $"INDEX_START={indexAtAcquisition};INDEX_END={indexAtReporting}",
                SOURCE = "IAS29",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var reserveAccount = GetAccountId(AccountMappingKeys.RestatementReserve, DefaultGlAccounts.RestatementReserve);

            if (delta > 0m)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    accountId,
                    reserveAccount,
                    delta,
                    $"IAS 29 restatement increase {accountId}",
                    userId,
                    cn);
            }
            else
            {
                var amount = Math.Abs(delta);
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    reserveAccount,
                    accountId,
                    amount,
                    $"IAS 29 restatement decrease {accountId}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IAS 29 restatement for {Account} delta {Delta}",
                accountId, delta);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordMonetaryGainLossAsync(
            DateTime restatementDate,
            decimal amount,
            bool isGain,
            string userId,
            string? connectionName = null)
        {
            if (amount <= 0m)
                throw new InvalidOperationException("Amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "HYPERINFLATION",
                COST_TYPE = "HYPERINFLATION_RESTATEMENT",
                COST_CATEGORY = "MONETARY_GAIN_LOSS",
                AMOUNT = amount,
                COST_DATE = restatementDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = isGain ? "IAS 29 monetary gain" : "IAS 29 monetary loss",
                SOURCE = "IAS29",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var reserveAccount = GetAccountId(AccountMappingKeys.RestatementReserve, DefaultGlAccounts.RestatementReserve);
            if (isGain)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    reserveAccount,
                    GetAccountId(AccountMappingKeys.InflationGain, DefaultGlAccounts.InflationGain),
                    amount,
                    "IAS 29 monetary gain",
                    userId,
                    cn);
            }
            else
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.InflationLoss, DefaultGlAccounts.InflationLoss),
                    reserveAccount,
                    amount,
                    "IAS 29 monetary loss",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IAS 29 monetary gain/loss amount {Amount} gain={Gain}",
                amount, isGain);

            return cost;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), cn, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}



