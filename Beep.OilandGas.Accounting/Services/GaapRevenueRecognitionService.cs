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
    /// ASC 606 revenue recognition (contract assets/liabilities and revenue).
    /// </summary>
    public class GaapRevenueRecognitionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<GaapRevenueRecognitionService> _logger;
        private const string ConnectionName = "PPDM39";

        public GaapRevenueRecognitionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<GaapRevenueRecognitionService> logger,
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

        public async Task<ACCOUNTING_COST> RecognizeContractRevenueAsync(
            string contractName,
            DateTime recognitionDate,
            decimal amount,
            string userId,
            bool fromContractAsset,
            string? contractId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (amount <= 0m)
                throw new InvalidOperationException("Revenue amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = contractId ?? "ASC606_CONTRACT",
                COST_TYPE = "ASC606_CONTRACT",
                COST_CATEGORY = "REVENUE",
                AMOUNT = amount,
                COST_DATE = recognitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"ASC 606 revenue recognition: {contractName}",
                SOURCE = "ASC606",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var revenueAccount = GetAccountId(AccountMappingKeys.Revenue, DefaultGlAccounts.Revenue);
            var assetAccount = GetAccountId(AccountMappingKeys.GaapContractAsset, DefaultGlAccounts.GaapContractAsset);
            var liabilityAccount = GetAccountId(AccountMappingKeys.GaapContractLiability, DefaultGlAccounts.GaapContractLiability);

            if (fromContractAsset)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    revenueAccount,
                    assetAccount,
                    amount,
                    $"ASC 606 revenue from contract asset {contractName}",
                    userId,
                    cn,
                    basis: AccountingBasis.Gaap);
            }
            else
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    liabilityAccount,
                    revenueAccount,
                    amount,
                    $"ASC 606 revenue from contract liability {contractName}",
                    userId,
                    cn,
                    basis: AccountingBasis.Gaap);
            }

            _logger?.LogInformation("Recorded ASC 606 revenue {Contract} amount {Amount}",
                contractName, amount);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordContractBillingAsync(
            string contractName,
            DateTime billingDate,
            decimal amount,
            string userId,
            bool createContractAsset,
            string? contractId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (amount <= 0m)
                throw new InvalidOperationException("Billing amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = contractId ?? "ASC606_CONTRACT",
                COST_TYPE = "ASC606_CONTRACT",
                COST_CATEGORY = "BILLING",
                AMOUNT = amount,
                COST_DATE = billingDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"ASC 606 billing: {contractName}",
                SOURCE = "ASC606",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var debitAccount = createContractAsset
                ? GetAccountId(AccountMappingKeys.GaapContractAsset, DefaultGlAccounts.GaapContractAsset)
                : GetAccountId(AccountMappingKeys.AccountsReceivable, DefaultGlAccounts.AccountsReceivable);

            var creditAccount = GetAccountId(AccountMappingKeys.GaapContractLiability, DefaultGlAccounts.GaapContractLiability);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                amount,
                $"ASC 606 billing {contractName}",
                userId,
                cn,
                basis: AccountingBasis.Gaap);

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



