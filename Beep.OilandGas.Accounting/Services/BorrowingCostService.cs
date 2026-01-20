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
    /// IAS 23 borrowing costs: capitalization or expensing of interest.
    /// </summary>
    public class BorrowingCostService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<BorrowingCostService> _logger;
        private const string ConnectionName = "PPDM39";

        public BorrowingCostService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<BorrowingCostService> logger,
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

        public async Task<ACCOUNTING_COST> RecordBorrowingCostAsync(
            string borrowingId,
            DateTime costDate,
            decimal amount,
            bool capitalize,
            string userId,
            string? assetAccountId = null,
            string? offsetAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(borrowingId))
                throw new ArgumentNullException(nameof(borrowingId));
            if (amount <= 0m)
                throw new InvalidOperationException("Borrowing cost amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = borrowingId,
                COST_TYPE = "BORROWING_COST",
                COST_CATEGORY = capitalize ? "CAPITALIZED" : "EXPENSED",
                AMOUNT = amount,
                COST_DATE = costDate,
                IS_CAPITALIZED = capitalize ? _defaults.GetActiveIndicatorYes() : _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = capitalize ? _defaults.GetActiveIndicatorNo() : _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = capitalize
                    ? $"Capitalized borrowing cost {borrowingId}"
                    : $"Borrowing cost expense {borrowingId}",
                SOURCE = "IAS23",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var creditAccount = string.IsNullOrWhiteSpace(offsetAccountId)
                ? GetAccountId(AccountMappingKeys.InterestPayable, DefaultGlAccounts.InterestPayable)
                : offsetAccountId;

            if (capitalize)
            {
                var debitAccount = string.IsNullOrWhiteSpace(assetAccountId)
                    ? GetAccountId(AccountMappingKeys.FixedAssets, DefaultGlAccounts.FixedAssets)
                    : assetAccountId;

                await _basisPosting.PostBalancedEntryByAccountAsync(
                    debitAccount,
                    creditAccount,
                    amount,
                    $"Capitalized borrowing cost {borrowingId}",
                    userId,
                    cn);
            }
            else
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.BorrowingCostExpense, DefaultGlAccounts.BorrowingCostExpense),
                    creditAccount,
                    amount,
                    $"Borrowing cost expense {borrowingId}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IAS 23 borrowing cost {BorrowingId} amount {Amount} capitalize={Capitalize}",
                borrowingId, amount, capitalize);

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



