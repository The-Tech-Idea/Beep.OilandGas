using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using AccountingModels = Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Reconciliation helpers for subledger vs GL control accounts.
    /// </summary>
    public class ReconciliationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<ReconciliationService> _logger;
        private const string ConnectionName = "PPDM39";

        public ReconciliationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<ReconciliationService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        public async Task<AccountingModels.ReconciliationSummary> ReconcileAccountsReceivableAsync(DateTime? asOfDate = null)
        {
            var accountId = GetAccountId(AccountMappingKeys.AccountsReceivable, DefaultGlAccounts.AccountsReceivable);
            var subledgerTotal = await GetArBalanceAsync(asOfDate);
            var glBalance = await _glAccountService.GetAccountBalanceAsync(accountId, asOfDate);

            return new AccountingModels.ReconciliationSummary
            {
                AccountKey = AccountMappingKeys.AccountsReceivable,
                AccountId = accountId,
                AsOfDate = asOfDate,
                SubledgerTotal = subledgerTotal,
                GlBalance = glBalance,
                Difference = subledgerTotal - glBalance
            };
        }

        public async Task<AccountingModels.ReconciliationSummary> ReconcileAccountsPayableAsync(DateTime? asOfDate = null)
        {
            var accountId = GetAccountId(AccountMappingKeys.AccountsPayable, DefaultGlAccounts.AccountsPayable);
            var subledgerTotal = await GetApBalanceAsync(asOfDate);
            var glBalance = await _glAccountService.GetAccountBalanceAsync(accountId, asOfDate);

            return new AccountingModels.ReconciliationSummary
            {
                AccountKey = AccountMappingKeys.AccountsPayable,
                AccountId = accountId,
                AsOfDate = asOfDate,
                SubledgerTotal = subledgerTotal,
                GlBalance = glBalance,
                Difference = subledgerTotal - glBalance
            };
        }

        public async Task<AccountingModels.ReconciliationSummary> ReconcileInventoryAsync(DateTime? asOfDate = null)
        {
            var accountId = GetAccountId(AccountMappingKeys.Inventory, DefaultGlAccounts.Inventory);
            var subledgerTotal = await GetInventoryValuationAsync();
            var glBalance = await _glAccountService.GetAccountBalanceAsync(accountId, asOfDate);

            return new AccountingModels.ReconciliationSummary
            {
                AccountKey = AccountMappingKeys.Inventory,
                AccountId = accountId,
                AsOfDate = asOfDate,
                SubledgerTotal = subledgerTotal,
                GlBalance = glBalance,
                Difference = subledgerTotal - glBalance
            };
        }

        private async Task<decimal> GetArBalanceAsync(DateTime? asOfDate)
        {
            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            if (asOfDate.HasValue)
                filters.Add(new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = asOfDate.Value.ToString("yyyy-MM-dd") });

            var results = await repo.GetAsync(filters);
            var invoices = results?.Cast<AR_INVOICE>().ToList() ?? new List<AR_INVOICE>();

            return invoices
                .Where(i => i.STATUS == InvoiceStatuses.Issued || i.STATUS == InvoiceStatuses.PartiallyPaid)
                .Sum(i => i.BALANCE_DUE ?? 0m);
        }

        private async Task<decimal> GetApBalanceAsync(DateTime? asOfDate)
        {
            var repo = await GetRepoAsync<AP_INVOICE>("AP_INVOICE");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            if (asOfDate.HasValue)
                filters.Add(new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = asOfDate.Value.ToString("yyyy-MM-dd") });

            var results = await repo.GetAsync(filters);
            var bills = results?.Cast<AP_INVOICE>().ToList() ?? new List<AP_INVOICE>();

            return bills
                .Where(b => b.STATUS == "RECEIVED" || b.STATUS == InvoiceStatuses.PartiallyPaid)
                .Sum(b => b.BALANCE_DUE ?? 0m);
        }

        private async Task<decimal> GetInventoryValuationAsync()
        {
            var repo = await GetRepoAsync<INVENTORY_ITEM>("INVENTORY_ITEM");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            var items = results?.Cast<INVENTORY_ITEM>().ToList() ?? new List<INVENTORY_ITEM>();

            return items.Sum(i => (i.QUANTITY_ON_HAND ?? 0m) * (i.UNIT_COST ?? 0m));
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, ConnectionName, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }

}
