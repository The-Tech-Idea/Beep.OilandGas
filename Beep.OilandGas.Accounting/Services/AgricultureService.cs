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
    /// IAS 41 biological assets and fair value adjustments.
    /// </summary>
    public class AgricultureService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<AgricultureService> _logger;
        private const string ConnectionName = "PPDM39";

        public AgricultureService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<AgricultureService> logger,
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

        public async Task<ACCOUNTING_COST> RecordBiologicalAssetAsync(
            string assetName,
            DateTime acquisitionDate,
            decimal acquisitionCost,
            string userId,
            string? assetId = null,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                throw new ArgumentNullException(nameof(assetName));
            if (acquisitionCost <= 0m)
                throw new InvalidOperationException("Acquisition cost must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = assetId ?? "BIOLOGICAL_ASSET",
                COST_TYPE = "BIOLOGICAL_ASSET",
                COST_CATEGORY = "ACQUISITION",
                AMOUNT = acquisitionCost,
                COST_DATE = acquisitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorYes(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Biological asset acquisition: {assetName}",
                SOURCE = "IAS41",
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
                GetAccountId(AccountMappingKeys.BiologicalAssets, DefaultGlAccounts.BiologicalAssets),
                cashAccount,
                acquisitionCost,
                $"Biological asset acquisition {assetName}",
                userId,
                cn);

            _logger?.LogInformation("Recorded IAS 41 biological asset {Asset} cost {Cost}",
                assetName, acquisitionCost);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordFairValueChangeAsync(
            string assetName,
            DateTime valuationDate,
            decimal changeAmount,
            string userId,
            string? assetId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                throw new ArgumentNullException(nameof(assetName));
            if (changeAmount == 0m)
                throw new InvalidOperationException("Fair value change cannot be zero");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = assetId ?? "BIOLOGICAL_ASSET",
                COST_TYPE = "BIOLOGICAL_ASSET",
                COST_CATEGORY = "FAIR_VALUE_CHANGE",
                AMOUNT = Math.Abs(changeAmount),
                COST_DATE = valuationDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Biological asset fair value change: {assetName}",
                SOURCE = "IAS41",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            if (changeAmount > 0m)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.BiologicalAssets, DefaultGlAccounts.BiologicalAssets),
                    GetAccountId(AccountMappingKeys.BiologicalAssetGain, DefaultGlAccounts.BiologicalAssetGain),
                    changeAmount,
                    $"Biological asset fair value gain {assetName}",
                    userId,
                    cn);
            }
            else
            {
                var amount = Math.Abs(changeAmount);
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.BiologicalAssetLoss, DefaultGlAccounts.BiologicalAssetLoss),
                    GetAccountId(AccountMappingKeys.BiologicalAssets, DefaultGlAccounts.BiologicalAssets),
                    amount,
                    $"Biological asset fair value loss {assetName}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IAS 41 fair value change {Asset} amount {Amount}",
                assetName, changeAmount);

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



