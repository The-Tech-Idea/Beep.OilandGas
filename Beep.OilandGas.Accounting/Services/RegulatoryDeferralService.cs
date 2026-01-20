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
    /// IFRS 14 regulatory deferral accounts.
    /// </summary>
    public class RegulatoryDeferralService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<RegulatoryDeferralService> _logger;
        private const string ConnectionName = "PPDM39";

        public RegulatoryDeferralService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<RegulatoryDeferralService> logger,
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

        public async Task<ACCOUNTING_COST> RecordRegulatoryDeferralAsync(
            DateTime recognitionDate,
            decimal amount,
            bool isAsset,
            string userId,
            string? description = null,
            string? referenceId = null,
            string? connectionName = null)
        {
            if (amount <= 0m)
                throw new InvalidOperationException("Deferral amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = referenceId ?? "REGULATORY_DEFERRAL",
                COST_TYPE = "REGULATORY_DEFERRAL",
                COST_CATEGORY = isAsset ? "ASSET" : "LIABILITY",
                AMOUNT = amount,
                COST_DATE = recognitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorYes(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = description ?? "Regulatory deferral recognized",
                SOURCE = "IFRS14",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            if (isAsset)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.RegulatoryDeferralAsset, DefaultGlAccounts.RegulatoryDeferralAsset),
                    GetAccountId(AccountMappingKeys.RegulatoryIncome, DefaultGlAccounts.RegulatoryIncome),
                    amount,
                    "Regulatory deferral asset",
                    userId,
                    cn);
            }
            else
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.RegulatoryExpense, DefaultGlAccounts.RegulatoryExpense),
                    GetAccountId(AccountMappingKeys.RegulatoryDeferralLiability, DefaultGlAccounts.RegulatoryDeferralLiability),
                    amount,
                    "Regulatory deferral liability",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IFRS 14 deferral amount {Amount} asset={Asset}", amount, isAsset);
            return cost;
        }

        public async Task<ACCOUNTING_COST> AmortizeRegulatoryDeferralAsync(
            DateTime amortizationDate,
            decimal amount,
            bool isAsset,
            string userId,
            string? description = null,
            string? referenceId = null,
            string? connectionName = null)
        {
            if (amount <= 0m)
                throw new InvalidOperationException("Amortization amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = referenceId ?? "REGULATORY_DEFERRAL",
                COST_TYPE = "REGULATORY_DEFERRAL",
                COST_CATEGORY = "AMORTIZATION",
                AMOUNT = amount,
                COST_DATE = amortizationDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = description ?? "Regulatory deferral amortization",
                SOURCE = "IFRS14",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            if (isAsset)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.RegulatoryExpense, DefaultGlAccounts.RegulatoryExpense),
                    GetAccountId(AccountMappingKeys.RegulatoryDeferralAsset, DefaultGlAccounts.RegulatoryDeferralAsset),
                    amount,
                    "Regulatory deferral asset amortization",
                    userId,
                    cn);
            }
            else
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.RegulatoryDeferralLiability, DefaultGlAccounts.RegulatoryDeferralLiability),
                    GetAccountId(AccountMappingKeys.RegulatoryIncome, DefaultGlAccounts.RegulatoryIncome),
                    amount,
                    "Regulatory deferral liability amortization",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Amortized IFRS 14 deferral amount {Amount} asset={Asset}", amount, isAsset);
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



