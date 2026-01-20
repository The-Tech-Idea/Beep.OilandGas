using System;
using System.Collections.Generic;
using System.Linq;
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
    /// IFRS 15 performance obligation tracking and revenue recognition.
    /// </summary>
    public class PerformanceObligationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<PerformanceObligationService> _logger;
        private const string ConnectionName = "PPDM39";

        public PerformanceObligationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<PerformanceObligationService> logger,
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

        public async Task<CONTRACT_PERFORMANCE_OBLIGATION> CreateObligationAsync(
            string salesContractId,
            string obligationType,
            string description,
            decimal? allocatedPrice,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(salesContractId))
                throw new ArgumentNullException(nameof(salesContractId));
            if (string.IsNullOrWhiteSpace(obligationType))
                throw new ArgumentNullException(nameof(obligationType));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var obligation = new CONTRACT_PERFORMANCE_OBLIGATION
            {
                CONTRACT_PERFORMANCE_OBLIGATION_ID = Guid.NewGuid().ToString(),
                SALES_CONTRACT_ID = salesContractId,
                OBLIGATION_TYPE = obligationType,
                OBLIGATION_DESCRIPTION = description,
                ALLOCATED_PRICE = allocatedPrice,
                STATUS = "OPEN",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<CONTRACT_PERFORMANCE_OBLIGATION>("CONTRACT_PERFORMANCE_OBLIGATION", connectionName);
            await repo.InsertAsync(obligation, userId);
            return obligation;
        }

        public async Task<List<CONTRACT_PERFORMANCE_OBLIGATION>> AllocateTransactionPriceAsync(
            string salesContractId,
            decimal totalPrice,
            Dictionary<string, decimal> allocationWeights,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(salesContractId))
                throw new ArgumentNullException(nameof(salesContractId));
            if (totalPrice <= 0m)
                throw new InvalidOperationException("Total price must be positive");
            if (allocationWeights == null || allocationWeights.Count == 0)
                throw new ArgumentException("Allocation weights are required", nameof(allocationWeights));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<CONTRACT_PERFORMANCE_OBLIGATION>("CONTRACT_PERFORMANCE_OBLIGATION", cn);

            var totalWeight = allocationWeights.Values.Sum();
            if (totalWeight <= 0m)
                throw new InvalidOperationException("Allocation weights must total more than zero");

            var updated = new List<CONTRACT_PERFORMANCE_OBLIGATION>();

            foreach (var kvp in allocationWeights)
            {
                var obligation = await repo.GetByIdAsync(kvp.Key) as CONTRACT_PERFORMANCE_OBLIGATION;
                if (obligation == null || !string.Equals(obligation.SALES_CONTRACT_ID, salesContractId, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Performance obligation not found for contract: {kvp.Key}");

                obligation.ALLOCATED_PRICE = Math.Round(totalPrice * (kvp.Value / totalWeight), 2);
                obligation.ROW_CHANGED_BY = userId;
                obligation.ROW_CHANGED_DATE = DateTime.UtcNow;

                await repo.UpdateAsync(obligation, userId);
                updated.Add(obligation);
            }

            return updated;
        }

        public async Task<CONTRACT_PERFORMANCE_OBLIGATION> SatisfyObligationAsync(
            string obligationId,
            DateTime satisfiedDate,
            string userId,
            bool billCustomer,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(obligationId))
                throw new ArgumentNullException(nameof(obligationId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<CONTRACT_PERFORMANCE_OBLIGATION>("CONTRACT_PERFORMANCE_OBLIGATION", cn);
            var obligation = await repo.GetByIdAsync(obligationId) as CONTRACT_PERFORMANCE_OBLIGATION;
            if (obligation == null)
                throw new InvalidOperationException($"Performance obligation not found: {obligationId}");
            if (!obligation.ALLOCATED_PRICE.HasValue || obligation.ALLOCATED_PRICE.Value <= 0m)
                throw new InvalidOperationException("Allocated price is required to satisfy obligation");

            var amount = obligation.ALLOCATED_PRICE.Value;
            var debitAccount = billCustomer
                ? GetAccountId(AccountMappingKeys.AccountsReceivable, DefaultGlAccounts.AccountsReceivable)
                : GetAccountId(AccountMappingKeys.ContractAsset, DefaultGlAccounts.ContractAsset);
            var creditAccount = GetAccountId(AccountMappingKeys.Revenue, DefaultGlAccounts.Revenue);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                amount,
                $"Performance obligation satisfied {obligation.OBLIGATION_DESCRIPTION}",
                userId,
                cn);

            obligation.STATUS = "SATISFIED";
            obligation.SATISFIED_DATE = satisfiedDate;
            obligation.ROW_CHANGED_BY = userId;
            obligation.ROW_CHANGED_DATE = DateTime.UtcNow;
            await repo.UpdateAsync(obligation, userId);

            return obligation;
        }

        public async Task<JOURNAL_ENTRY> RecordContractLiabilityAsync(
            string salesContractId,
            decimal amount,
            DateTime receiptDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(salesContractId))
                throw new ArgumentNullException(nameof(salesContractId));
            if (amount <= 0m)
                throw new InvalidOperationException("Amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var debitAccount = GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash);
            var creditAccount = GetAccountId(AccountMappingKeys.ContractLiability, DefaultGlAccounts.ContractLiability);

            var result = await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                amount,
                $"Contract liability recorded for {salesContractId}",
                userId,
                cn);
            return result.IfrsEntry ?? throw new InvalidOperationException("IFRS entry was not created.");
        }

        public async Task<JOURNAL_ENTRY> RecognizeRevenueFromLiabilityAsync(
            string salesContractId,
            decimal amount,
            DateTime recognitionDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(salesContractId))
                throw new ArgumentNullException(nameof(salesContractId));
            if (amount <= 0m)
                throw new InvalidOperationException("Amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var debitAccount = GetAccountId(AccountMappingKeys.ContractLiability, DefaultGlAccounts.ContractLiability);
            var creditAccount = GetAccountId(AccountMappingKeys.Revenue, DefaultGlAccounts.Revenue);

            var result = await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                amount,
                $"Contract liability recognized for {salesContractId}",
                userId,
                cn);
            return result.IfrsEntry ?? throw new InvalidOperationException("IFRS entry was not created.");
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName)
        {
            var cn = connectionName ?? ConnectionName;
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


