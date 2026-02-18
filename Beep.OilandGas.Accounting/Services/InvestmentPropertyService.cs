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
    /// IAS 40 investment property recognition and fair value adjustments.
    /// </summary>
    public class InvestmentPropertyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<InvestmentPropertyService> _logger;
        private const string ConnectionName = "PPDM39";

        public InvestmentPropertyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<InvestmentPropertyService> logger,
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

        public async Task<ACCOUNTING_COST> RegisterInvestmentPropertyAsync(
            string propertyName,
            DateTime acquisitionDate,
            decimal acquisitionCost,
            string userId,
            string? propertyId = null,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            if (acquisitionCost <= 0m)
                throw new InvalidOperationException("Acquisition cost must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = propertyId ?? "INVESTMENT_PROPERTY",
                COST_TYPE = "INVESTMENT_PROPERTY",
                COST_CATEGORY = "ACQUISITION",
                AMOUNT = acquisitionCost,
                COST_DATE = acquisitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorYes(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Investment property acquisition: {propertyName}",
                SOURCE = "IAS40",
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
                GetAccountId(AccountMappingKeys.InvestmentProperty, DefaultGlAccounts.InvestmentProperty),
                cashAccount,
                acquisitionCost,
                $"Investment property acquisition {propertyName}",
                userId,
                cn);

            _logger?.LogInformation("Registered IAS 40 investment property {Property} cost {Cost}",
                propertyName, acquisitionCost);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordFairValueChangeAsync(
            string propertyName,
            DateTime valuationDate,
            decimal changeAmount,
            string userId,
            string? propertyId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            if (changeAmount == 0m)
                throw new InvalidOperationException("Fair value change cannot be zero");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = propertyId ?? "INVESTMENT_PROPERTY",
                COST_TYPE = "INVESTMENT_PROPERTY",
                COST_CATEGORY = "FAIR_VALUE_CHANGE",
                AMOUNT = Math.Abs(changeAmount),
                COST_DATE = valuationDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Investment property fair value change: {propertyName}",
                SOURCE = "IAS40",
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
                    GetAccountId(AccountMappingKeys.InvestmentProperty, DefaultGlAccounts.InvestmentProperty),
                    GetAccountId(AccountMappingKeys.InvestmentPropertyFairValueGain, DefaultGlAccounts.InvestmentPropertyFairValueGain),
                    changeAmount,
                    $"Investment property fair value gain {propertyName}",
                    userId,
                    cn);
            }
            else
            {
                var amount = Math.Abs(changeAmount);
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.InvestmentPropertyFairValueLoss, DefaultGlAccounts.InvestmentPropertyFairValueLoss),
                    GetAccountId(AccountMappingKeys.InvestmentProperty, DefaultGlAccounts.InvestmentProperty),
                    amount,
                    $"Investment property fair value loss {propertyName}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IAS 40 fair value change {Property} amount {Amount}",
                propertyName, changeAmount);

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



