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
    /// IFRS 9 expected credit loss provisioning.
    /// </summary>
    public class ExpectedCreditLossService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<ExpectedCreditLossService> _logger;
        private const string ConnectionName = "PPDM39";

        public ExpectedCreditLossService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<ExpectedCreditLossService> logger,
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

        public async Task<ACCOUNTING_COST> RecordExpectedCreditLossAsync(
            DateTime asOfDate,
            decimal adjustmentAmount,
            string userId,
            string? referenceId = null,
            string? description = null,
            string? connectionName = null)
        {
            if (adjustmentAmount == 0m)
                throw new InvalidOperationException("ECL adjustment cannot be zero");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = referenceId ?? "IFRS9_ECL",
                COST_TYPE = "IFRS9_ECL",
                COST_CATEGORY = adjustmentAmount > 0m ? "INCREASE" : "DECREASE",
                AMOUNT = Math.Abs(adjustmentAmount),
                COST_DATE = asOfDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = description ?? "Expected credit loss adjustment",
                SOURCE = "IFRS9",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var expenseAccount = GetAccountId(AccountMappingKeys.ExpectedCreditLossExpense, DefaultGlAccounts.ExpectedCreditLossExpense);
            var allowanceAccount = GetAccountId(AccountMappingKeys.LossAllowance, DefaultGlAccounts.LossAllowance);

            if (adjustmentAmount > 0m)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    expenseAccount,
                    allowanceAccount,
                    adjustmentAmount,
                    $"ECL increase {referenceId ?? string.Empty}".Trim(),
                    userId,
                    cn);
            }
            else
            {
                var amount = Math.Abs(adjustmentAmount);
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    allowanceAccount,
                    expenseAccount,
                    amount,
                    $"ECL decrease {referenceId ?? string.Empty}".Trim(),
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IFRS 9 ECL adjustment {Amount}", adjustmentAmount);
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



