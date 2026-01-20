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
    /// IFRS 2 share-based payment expense recognition.
    /// </summary>
    public class ShareBasedPaymentService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<ShareBasedPaymentService> _logger;
        private const string ConnectionName = "PPDM39";

        public ShareBasedPaymentService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<ShareBasedPaymentService> logger,
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

        public async Task<ACCOUNTING_COST> RecordShareBasedExpenseAsync(
            string awardName,
            DateTime expenseDate,
            decimal amount,
            string userId,
            string? awardId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(awardName))
                throw new ArgumentNullException(nameof(awardName));
            if (amount <= 0m)
                throw new InvalidOperationException("Share-based expense must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = awardId ?? "SHARE_BASED_PAYMENT",
                COST_TYPE = "SHARE_BASED_PAYMENT",
                COST_CATEGORY = "EXPENSE",
                AMOUNT = amount,
                COST_DATE = expenseDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = $"Share-based payment: {awardName}",
                SOURCE = "IFRS2",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.ShareBasedCompExpense, DefaultGlAccounts.ShareBasedCompExpense),
                GetAccountId(AccountMappingKeys.ShareBasedCompEquity, DefaultGlAccounts.ShareBasedCompEquity),
                amount,
                $"Share-based compensation {awardName}",
                userId,
                cn);

            _logger?.LogInformation("Recorded IFRS 2 share-based expense {Award} amount {Amount}",
                awardName, amount);

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



