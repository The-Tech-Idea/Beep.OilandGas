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
    /// IAS 20 government grants recognition and income release.
    /// </summary>
    public class GovernmentGrantService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<GovernmentGrantService> _logger;
        private const string ConnectionName = "PPDM39";

        public GovernmentGrantService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<GovernmentGrantService> logger,
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

        public async Task<ACCOUNTING_COST> RecordGrantAwardAsync(
            string grantName,
            DateTime awardDate,
            decimal amount,
            string userId,
            bool receivedInCash = false,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(grantName))
                throw new ArgumentNullException(nameof(grantName));
            if (amount <= 0m)
                throw new InvalidOperationException("Grant award amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "GOVERNMENT_GRANT",
                COST_TYPE = "GOVERNMENT_GRANT",
                COST_CATEGORY = "GRANT_AWARD",
                AMOUNT = amount,
                COST_DATE = awardDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Grant award: {grantName}",
                SOURCE = "IAS20",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var debitAccount = receivedInCash
                ? (string.IsNullOrWhiteSpace(cashAccountId)
                    ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                    : cashAccountId)
                : GetAccountId(AccountMappingKeys.GrantReceivable, DefaultGlAccounts.GrantReceivable);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                GetAccountId(AccountMappingKeys.DeferredGrantLiability, DefaultGlAccounts.DeferredGrantLiability),
                amount,
                $"Grant award {grantName}",
                userId,
                cn);

            _logger?.LogInformation("Recorded IAS 20 grant award {Grant} amount {Amount}",
                grantName, amount);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordGrantReceivableSettlementAsync(
            string grantName,
            DateTime receiptDate,
            decimal amount,
            string userId,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(grantName))
                throw new ArgumentNullException(nameof(grantName));
            if (amount <= 0m)
                throw new InvalidOperationException("Grant receipt amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "GOVERNMENT_GRANT",
                COST_TYPE = "GOVERNMENT_GRANT",
                COST_CATEGORY = "GRANT_RECEIPT",
                AMOUNT = amount,
                COST_DATE = receiptDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Grant cash receipt: {grantName}",
                SOURCE = "IAS20",
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

            await _basisPosting.PostBalancedEntryByAccountAsync(
                cashAccount,
                GetAccountId(AccountMappingKeys.GrantReceivable, DefaultGlAccounts.GrantReceivable),
                amount,
                $"Grant receivable settlement {grantName}",
                userId,
                cn);

            _logger?.LogInformation("Recorded IAS 20 grant receipt {Grant} amount {Amount}",
                grantName, amount);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecognizeGrantIncomeAsync(
            string grantName,
            DateTime recognitionDate,
            decimal amount,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(grantName))
                throw new ArgumentNullException(nameof(grantName));
            if (amount <= 0m)
                throw new InvalidOperationException("Grant income amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "GOVERNMENT_GRANT",
                COST_TYPE = "GOVERNMENT_GRANT",
                COST_CATEGORY = "GRANT_INCOME",
                AMOUNT = amount,
                COST_DATE = recognitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Grant income recognition: {grantName}",
                SOURCE = "IAS20",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.DeferredGrantLiability, DefaultGlAccounts.DeferredGrantLiability),
                GetAccountId(AccountMappingKeys.GrantIncome, DefaultGlAccounts.GrantIncome),
                amount,
                $"Grant income recognition {grantName}",
                userId,
                cn);

            _logger?.LogInformation("Recognized IAS 20 grant income {Grant} amount {Amount}",
                grantName, amount);

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



