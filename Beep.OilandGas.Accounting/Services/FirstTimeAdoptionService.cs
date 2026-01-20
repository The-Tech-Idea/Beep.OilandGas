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
    /// IFRS 1 first-time adoption transition adjustments.
    /// </summary>
    public class FirstTimeAdoptionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<FirstTimeAdoptionService> _logger;
        private const string ConnectionName = "PPDM39";

        public FirstTimeAdoptionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<FirstTimeAdoptionService> logger,
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

        public async Task<ACCOUNTING_COST> RecordTransitionAdjustmentAsync(
            DateTime transitionDate,
            decimal adjustmentAmount,
            string description,
            string userId,
            string? affectedAccountId = null,
            string? equityAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (adjustmentAmount == 0m)
                throw new InvalidOperationException("Adjustment amount cannot be zero");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "IFRS1_TRANSITION",
                COST_TYPE = "IFRS1_TRANSITION",
                COST_CATEGORY = adjustmentAmount > 0m ? "INCREASE" : "DECREASE",
                AMOUNT = Math.Abs(adjustmentAmount),
                COST_DATE = transitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = description,
                SOURCE = "IFRS1",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var accountId = string.IsNullOrWhiteSpace(affectedAccountId)
                ? GetAccountId(AccountMappingKeys.RetainedEarnings, DefaultGlAccounts.RetainedEarnings)
                : affectedAccountId;

            var equityAccount = string.IsNullOrWhiteSpace(equityAccountId)
                ? GetAccountId(AccountMappingKeys.RestatementReserve, DefaultGlAccounts.RestatementReserve)
                : equityAccountId;

            if (adjustmentAmount > 0m)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    accountId,
                    equityAccount,
                    adjustmentAmount,
                    $"IFRS 1 transition increase: {description}",
                    userId,
                    cn);
            }
            else
            {
                var amount = Math.Abs(adjustmentAmount);
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    equityAccount,
                    accountId,
                    amount,
                    $"IFRS 1 transition decrease: {description}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IFRS 1 transition adjustment {Amount}", adjustmentAmount);
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



