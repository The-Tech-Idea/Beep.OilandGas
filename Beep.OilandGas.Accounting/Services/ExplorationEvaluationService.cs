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
    /// IFRS 6 exploration and evaluation costs.
    /// </summary>
    public class ExplorationEvaluationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<ExplorationEvaluationService> _logger;
        private const string ConnectionName = "PPDM39";

        public ExplorationEvaluationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<ExplorationEvaluationService> logger,
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

        public async Task<ACCOUNTING_COST> CapitalizeExplorationCostAsync(
            string projectName,
            DateTime costDate,
            decimal amount,
            string userId,
            string? projectId = null,
            string? offsetAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentNullException(nameof(projectName));
            if (amount <= 0m)
                throw new InvalidOperationException("Exploration cost must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = projectId ?? "EXPLORATION",
                COST_TYPE = "EXPLORATION_EVALUATION",
                COST_CATEGORY = "CAPITALIZED",
                AMOUNT = amount,
                COST_DATE = costDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorYes(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Exploration cost capitalized: {projectName}",
                SOURCE = "IFRS6",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var creditAccount = string.IsNullOrWhiteSpace(offsetAccountId)
                ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                : offsetAccountId;

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.ExplorationAsset, DefaultGlAccounts.ExplorationAsset),
                creditAccount,
                amount,
                $"Exploration capitalization {projectName}",
                userId,
                cn);

            _logger?.LogInformation("Capitalized IFRS 6 exploration cost {Project} amount {Amount}",
                projectName, amount);

            return cost;
        }

        public async Task<ACCOUNTING_COST> ExpenseExplorationCostAsync(
            string projectName,
            DateTime costDate,
            decimal amount,
            string userId,
            string? projectId = null,
            string? offsetAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentNullException(nameof(projectName));
            if (amount <= 0m)
                throw new InvalidOperationException("Exploration cost must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = projectId ?? "EXPLORATION",
                COST_TYPE = "EXPLORATION_EVALUATION",
                COST_CATEGORY = "EXPENSED",
                AMOUNT = amount,
                COST_DATE = costDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = $"Exploration cost expensed: {projectName}",
                SOURCE = "IFRS6",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var creditAccount = string.IsNullOrWhiteSpace(offsetAccountId)
                ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                : offsetAccountId;

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.ExplorationExpense, DefaultGlAccounts.ExplorationExpense),
                creditAccount,
                amount,
                $"Exploration expensed {projectName}",
                userId,
                cn);

            _logger?.LogInformation("Expensed IFRS 6 exploration cost {Project} amount {Amount}",
                projectName, amount);

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



