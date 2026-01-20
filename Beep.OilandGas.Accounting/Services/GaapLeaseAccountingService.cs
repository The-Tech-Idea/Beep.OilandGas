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
    /// ASC 842 lease accounting (ROU asset and lease liability).
    /// </summary>
    public class GaapLeaseAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<GaapLeaseAccountingService> _logger;
        private const string ConnectionName = "PPDM39";

        public GaapLeaseAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<GaapLeaseAccountingService> logger,
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

        public async Task<ACCOUNTING_COST> RecordLeaseCommencementAsync(
            string leaseName,
            DateTime commencementDate,
            decimal leaseLiability,
            string userId,
            string? leaseId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(leaseName))
                throw new ArgumentNullException(nameof(leaseName));
            if (leaseLiability <= 0m)
                throw new InvalidOperationException("Lease liability must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = leaseId ?? "ASC842_LEASE",
                COST_TYPE = "ASC842_LEASE",
                COST_CATEGORY = "COMMENCEMENT",
                AMOUNT = leaseLiability,
                COST_DATE = commencementDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorYes(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"ASC 842 lease commencement: {leaseName}",
                SOURCE = "ASC842",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.GaapRightOfUseAsset, DefaultGlAccounts.GaapRightOfUseAsset),
                GetAccountId(AccountMappingKeys.GaapLeaseLiability, DefaultGlAccounts.GaapLeaseLiability),
                leaseLiability,
                $"ASC 842 ROU and lease liability {leaseName}",
                userId,
                cn,
                basis: AccountingBasis.Gaap);

            _logger?.LogInformation("Recorded ASC 842 lease commencement {Lease} liability {Amount}",
                leaseName, leaseLiability);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordLeasePaymentAsync(
            string leaseName,
            DateTime paymentDate,
            decimal paymentAmount,
            string userId,
            string? leaseId = null,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(leaseName))
                throw new ArgumentNullException(nameof(leaseName));
            if (paymentAmount <= 0m)
                throw new InvalidOperationException("Lease payment must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = leaseId ?? "ASC842_LEASE",
                COST_TYPE = "ASC842_LEASE",
                COST_CATEGORY = "PAYMENT",
                AMOUNT = paymentAmount,
                COST_DATE = paymentDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"ASC 842 lease payment: {leaseName}",
                SOURCE = "ASC842",
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
                GetAccountId(AccountMappingKeys.GaapLeaseLiability, DefaultGlAccounts.GaapLeaseLiability),
                cashAccount,
                paymentAmount,
                $"ASC 842 lease payment {leaseName}",
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



