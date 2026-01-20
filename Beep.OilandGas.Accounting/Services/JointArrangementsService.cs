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
    /// IFRS 11 joint arrangements (joint operations cost sharing).
    /// </summary>
    public class JointArrangementsService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<JointArrangementsService> _logger;
        private const string ConnectionName = "PPDM39";

        public JointArrangementsService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<JointArrangementsService> logger,
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

        public async Task<ACCOUNTING_COST> RecordJointOperationCostShareAsync(
            string arrangementName,
            DateTime costDate,
            decimal amount,
            string userId,
            string? arrangementId = null,
            string? expenseAccountId = null,
            string? offsetAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(arrangementName))
                throw new ArgumentNullException(nameof(arrangementName));
            if (amount <= 0m)
                throw new InvalidOperationException("Cost share amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = arrangementId ?? "JOINT_ARRANGEMENT",
                COST_TYPE = "JOINT_ARRANGEMENT",
                COST_CATEGORY = "COST_SHARE",
                AMOUNT = amount,
                COST_DATE = costDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = $"Joint arrangement cost share: {arrangementName}",
                SOURCE = "IFRS11",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var debitAccount = string.IsNullOrWhiteSpace(expenseAccountId)
                ? GetAccountId(AccountMappingKeys.OperatingExpense, DefaultGlAccounts.OperatingExpense)
                : expenseAccountId;

            var creditAccount = string.IsNullOrWhiteSpace(offsetAccountId)
                ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                : offsetAccountId;

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                amount,
                $"Joint arrangement cost share {arrangementName}",
                userId,
                cn);

            _logger?.LogInformation("Recorded IFRS 11 cost share {Arrangement} amount {Amount}",
                arrangementName, amount);

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



