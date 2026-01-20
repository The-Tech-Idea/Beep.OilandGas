using System;
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
    /// IAS 38 intangible asset capitalization and amortization.
    /// </summary>
    public class IntangibleAssetService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<IntangibleAssetService> _logger;
        private const string ConnectionName = "PPDM39";

        public IntangibleAssetService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<IntangibleAssetService> logger,
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

        public async Task<ACCOUNTING_COST> CapitalizeIntangibleAsync(
            string intangibleId,
            string description,
            decimal amount,
            DateTime acquisitionDate,
            string userId,
            string? propertyId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(intangibleId))
                throw new ArgumentNullException(nameof(intangibleId));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (amount <= 0m)
                throw new InvalidOperationException("Capitalization amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = intangibleId,
                PROPERTY_ID = propertyId,
                COST_TYPE = "INTANGIBLE_CAPITALIZATION",
                COST_CATEGORY = "INTANGIBLE",
                AMOUNT = amount,
                COST_DATE = acquisitionDate,
                IS_CAPITALIZED = "Y",
                IS_EXPENSED = "N",
                DESCRIPTION = description,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.IntangibleAssets, DefaultGlAccounts.IntangibleAssets),
                GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash),
                amount,
                $"Intangible capitalization {description}",
                userId,
                cn);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordAmortizationAsync(
            string intangibleId,
            decimal amortizationAmount,
            DateTime amortizationDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(intangibleId))
                throw new ArgumentNullException(nameof(intangibleId));
            if (amortizationAmount <= 0m)
                throw new InvalidOperationException("Amortization amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = intangibleId,
                COST_TYPE = "INTANGIBLE_AMORTIZATION",
                COST_CATEGORY = "INTANGIBLE",
                AMOUNT = amortizationAmount,
                COST_DATE = amortizationDate,
                IS_CAPITALIZED = "N",
                IS_EXPENSED = "Y",
                DESCRIPTION = $"Intangible amortization {intangibleId}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.AmortizationExpense, DefaultGlAccounts.AmortizationExpense),
                GetAccountId(AccountMappingKeys.AccumulatedAmortization, DefaultGlAccounts.AccumulatedAmortization),
                amortizationAmount,
                $"Intangible amortization {intangibleId}",
                userId,
                cn);

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



