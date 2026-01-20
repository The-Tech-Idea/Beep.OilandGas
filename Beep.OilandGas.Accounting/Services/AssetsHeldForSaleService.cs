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
    /// IFRS 5 non-current assets held for sale.
    /// </summary>
    public class AssetsHeldForSaleService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<AssetsHeldForSaleService> _logger;
        private const string ConnectionName = "PPDM39";

        public AssetsHeldForSaleService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<AssetsHeldForSaleService> logger,
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

        public async Task<ACCOUNTING_COST> ClassifyAssetHeldForSaleAsync(
            string assetAccountId,
            DateTime classificationDate,
            decimal carryingAmount,
            string userId,
            string? referenceId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(assetAccountId))
                throw new ArgumentNullException(nameof(assetAccountId));
            if (carryingAmount <= 0m)
                throw new InvalidOperationException("Carrying amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = referenceId ?? assetAccountId,
                COST_TYPE = "HELD_FOR_SALE",
                COST_CATEGORY = "CLASSIFICATION",
                AMOUNT = carryingAmount,
                COST_DATE = classificationDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorYes(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = "Asset classified as held for sale",
                SOURCE = "IFRS5",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.HeldForSaleAsset, DefaultGlAccounts.HeldForSaleAsset),
                assetAccountId,
                carryingAmount,
                "Classify asset held for sale",
                userId,
                cn);

            _logger?.LogInformation("Classified asset held for sale {Account} amount {Amount}",
                assetAccountId, carryingAmount);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordHeldForSaleImpairmentAsync(
            string assetAccountId,
            DateTime impairmentDate,
            decimal impairmentAmount,
            string userId,
            string? referenceId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(assetAccountId))
                throw new ArgumentNullException(nameof(assetAccountId));
            if (impairmentAmount <= 0m)
                throw new InvalidOperationException("Impairment must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = referenceId ?? assetAccountId,
                COST_TYPE = "HELD_FOR_SALE",
                COST_CATEGORY = "IMPAIRMENT",
                AMOUNT = impairmentAmount,
                COST_DATE = impairmentDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = "Held for sale impairment",
                SOURCE = "IFRS5",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.HeldForSaleImpairment, DefaultGlAccounts.HeldForSaleImpairment),
                GetAccountId(AccountMappingKeys.HeldForSaleAsset, DefaultGlAccounts.HeldForSaleAsset),
                impairmentAmount,
                "Held for sale impairment",
                userId,
                cn);

            _logger?.LogInformation("Recorded held for sale impairment {Amount}", impairmentAmount);

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



