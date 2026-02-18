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
    /// IFRS 17 insurance contracts: contract liability, CSM, and service results.
    /// </summary>
    public class InsuranceContractsService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<InsuranceContractsService> _logger;
        private const string ConnectionName = "PPDM39";

        public InsuranceContractsService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<InsuranceContractsService> logger,
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

        public async Task<ACCOUNTING_COST> RecordContractInceptionAsync(
            string contractName,
            DateTime inceptionDate,
            decimal initialLiability,
            string userId,
            decimal? contractualServiceMargin = null,
            string? contractId = null,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (initialLiability <= 0m)
                throw new InvalidOperationException("Initial liability must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = contractId ?? "INSURANCE_CONTRACT",
                COST_TYPE = "INSURANCE_CONTRACT",
                COST_CATEGORY = "INCEPTION",
                AMOUNT = initialLiability,
                COST_DATE = inceptionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"IFRS 17 contract inception: {contractName}",
                SOURCE = "IFRS17",
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
                cashAccount,
                GetAccountId(AccountMappingKeys.InsuranceContractLiability, DefaultGlAccounts.InsuranceContractLiability),
                initialLiability,
                $"Insurance contract inception {contractName}",
                userId,
                cn);

            if (contractualServiceMargin.HasValue && contractualServiceMargin.Value != 0m)
            {
                var amount = Math.Abs(contractualServiceMargin.Value);
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.InsuranceContractLiability, DefaultGlAccounts.InsuranceContractLiability),
                    GetAccountId(AccountMappingKeys.ContractualServiceMargin, DefaultGlAccounts.ContractualServiceMargin),
                    amount,
                    $"Contractual service margin set-up {contractName}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IFRS 17 inception {Contract} liability {Amount}",
                contractName, initialLiability);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordPremiumReceiptAsync(
            string contractName,
            DateTime receiptDate,
            decimal amount,
            string userId,
            string? contractId = null,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (amount <= 0m)
                throw new InvalidOperationException("Premium receipt amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = contractId ?? "INSURANCE_CONTRACT",
                COST_TYPE = "INSURANCE_CONTRACT",
                COST_CATEGORY = "PREMIUM",
                AMOUNT = amount,
                COST_DATE = receiptDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Premium receipt: {contractName}",
                SOURCE = "IFRS17",
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
                cashAccount,
                GetAccountId(AccountMappingKeys.InsuranceContractLiability, DefaultGlAccounts.InsuranceContractLiability),
                amount,
                $"Premium receipt {contractName}",
                userId,
                cn);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecognizeInsuranceRevenueAsync(
            string contractName,
            DateTime recognitionDate,
            decimal amount,
            string userId,
            string? contractId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (amount <= 0m)
                throw new InvalidOperationException("Insurance revenue amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = contractId ?? "INSURANCE_CONTRACT",
                COST_TYPE = "INSURANCE_CONTRACT",
                COST_CATEGORY = "REVENUE",
                AMOUNT = amount,
                COST_DATE = recognitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Insurance revenue recognition: {contractName}",
                SOURCE = "IFRS17",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.InsuranceContractLiability, DefaultGlAccounts.InsuranceContractLiability),
                GetAccountId(AccountMappingKeys.InsuranceRevenue, DefaultGlAccounts.InsuranceRevenue),
                amount,
                $"Insurance revenue {contractName}",
                userId,
                cn);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordClaimExpenseAsync(
            string contractName,
            DateTime claimDate,
            decimal amount,
            string userId,
            string? contractId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (amount <= 0m)
                throw new InvalidOperationException("Claim amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = contractId ?? "INSURANCE_CONTRACT",
                COST_TYPE = "INSURANCE_CONTRACT",
                COST_CATEGORY = "CLAIM",
                AMOUNT = amount,
                COST_DATE = claimDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = $"Claim expense: {contractName}",
                SOURCE = "IFRS17",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.InsuranceServiceExpense, DefaultGlAccounts.InsuranceServiceExpense),
                GetAccountId(AccountMappingKeys.InsuranceContractLiability, DefaultGlAccounts.InsuranceContractLiability),
                amount,
                $"Claim expense {contractName}",
                userId,
                cn);

            return cost;
        }

        public async Task<ACCOUNTING_COST> AdjustContractualServiceMarginAsync(
            string contractName,
            DateTime adjustmentDate,
            decimal amount,
            bool increaseCsm,
            string userId,
            string? contractId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (amount <= 0m)
                throw new InvalidOperationException("CSM adjustment must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = contractId ?? "INSURANCE_CONTRACT",
                COST_TYPE = "INSURANCE_CONTRACT",
                COST_CATEGORY = "CSM_ADJUSTMENT",
                AMOUNT = amount,
                COST_DATE = adjustmentDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"CSM adjustment: {contractName}",
                SOURCE = "IFRS17",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var csmAccount = GetAccountId(AccountMappingKeys.ContractualServiceMargin, DefaultGlAccounts.ContractualServiceMargin);
            var liabilityAccount = GetAccountId(AccountMappingKeys.InsuranceContractLiability, DefaultGlAccounts.InsuranceContractLiability);

            if (increaseCsm)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    liabilityAccount,
                    csmAccount,
                    amount,
                    $"Increase CSM {contractName}",
                    userId,
                    cn);
            }
            else
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    csmAccount,
                    liabilityAccount,
                    amount,
                    $"Decrease CSM {contractName}",
                    userId,
                    cn);
            }

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordInsuranceFinanceExpenseAsync(
            string contractName,
            DateTime expenseDate,
            decimal amount,
            string userId,
            string? contractId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (amount <= 0m)
                throw new InvalidOperationException("Finance expense amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = contractId ?? "INSURANCE_CONTRACT",
                COST_TYPE = "INSURANCE_CONTRACT",
                COST_CATEGORY = "FINANCE_EXPENSE",
                AMOUNT = amount,
                COST_DATE = expenseDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorYes(),
                DESCRIPTION = $"Insurance finance expense: {contractName}",
                SOURCE = "IFRS17",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.InsuranceFinanceExpense, DefaultGlAccounts.InsuranceFinanceExpense),
                GetAccountId(AccountMappingKeys.InsuranceContractLiability, DefaultGlAccounts.InsuranceContractLiability),
                amount,
                $"Insurance finance expense {contractName}",
                userId,
                cn);

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



