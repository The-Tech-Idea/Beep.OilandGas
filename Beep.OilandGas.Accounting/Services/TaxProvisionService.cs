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
    /// IAS 12 current and deferred tax provisioning.
    /// </summary>
    public class TaxProvisionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<TaxProvisionService> _logger;
        private const string ConnectionName = "PPDM39";

        public TaxProvisionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<TaxProvisionService> logger,
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

        public async Task<TAX_TRANSACTION> RecordCurrentTaxAsync(
            string taxType,
            DateTime taxDate,
            decimal taxAmount,
            string jurisdiction,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(taxType))
                throw new ArgumentNullException(nameof(taxType));
            if (taxAmount <= 0m)
                throw new InvalidOperationException("Tax amount must be positive");
            if (string.IsNullOrWhiteSpace(jurisdiction))
                throw new ArgumentNullException(nameof(jurisdiction));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var tax = new TAX_TRANSACTION
            {
                TAX_TRANSACTION_ID = Guid.NewGuid().ToString(),
                TAX_TYPE = taxType,
                TAX_DATE = taxDate,
                TAX_AMOUNT = taxAmount,
                TAX_JURISDICTION = jurisdiction,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<TAX_TRANSACTION>("TAX_TRANSACTION", cn);
            await repo.InsertAsync(tax, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.IncomeTaxExpense, DefaultGlAccounts.IncomeTaxExpense),
                GetAccountId(AccountMappingKeys.IncomeTaxPayable, DefaultGlAccounts.IncomeTaxPayable),
                taxAmount,
                $"Current tax provision {taxType} {jurisdiction}",
                userId,
                cn);

            return tax;
        }

        public async Task<DEFERRED_TAX_BALANCE> RecordDeferredTaxBalanceAsync(
            DateTime periodEnd,
            decimal deferredTaxAsset,
            decimal deferredTaxLiability,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var previous = await GetLatestDeferredTaxBalanceAsync(cn);

            var deferred = new DEFERRED_TAX_BALANCE
            {
                DEFERRED_TAX_BALANCE_ID = Guid.NewGuid().ToString(),
                PERIOD_END_DATE = periodEnd,
                DEFERRED_TAX_ASSET = deferredTaxAsset,
                DEFERRED_TAX_LIABILITY = deferredTaxLiability,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<DEFERRED_TAX_BALANCE>("DEFERRED_TAX_BALANCE", cn);
            await repo.InsertAsync(deferred, userId);

            var deltaAsset = deferredTaxAsset - (previous?.DEFERRED_TAX_ASSET ?? 0m);
            var deltaLiability = deferredTaxLiability - (previous?.DEFERRED_TAX_LIABILITY ?? 0m);

            var lines = new List<JOURNAL_ENTRY_LINE>();
            if (deltaAsset != 0m)
            {
                if (deltaAsset > 0m)
                {
                    lines.Add(NewLine(GetAccountId(AccountMappingKeys.DeferredTaxAsset, DefaultGlAccounts.DeferredTaxAsset), deltaAsset, 0m, "Deferred tax asset increase"));
                    lines.Add(NewLine(GetAccountId(AccountMappingKeys.IncomeTaxExpense, DefaultGlAccounts.IncomeTaxExpense), 0m, deltaAsset, "Deferred tax asset increase"));
                }
                else
                {
                    var amount = Math.Abs(deltaAsset);
                    lines.Add(NewLine(GetAccountId(AccountMappingKeys.IncomeTaxExpense, DefaultGlAccounts.IncomeTaxExpense), amount, 0m, "Deferred tax asset decrease"));
                    lines.Add(NewLine(GetAccountId(AccountMappingKeys.DeferredTaxAsset, DefaultGlAccounts.DeferredTaxAsset), 0m, amount, "Deferred tax asset decrease"));
                }
            }

            if (deltaLiability != 0m)
            {
                if (deltaLiability > 0m)
                {
                    lines.Add(NewLine(GetAccountId(AccountMappingKeys.IncomeTaxExpense, DefaultGlAccounts.IncomeTaxExpense), deltaLiability, 0m, "Deferred tax liability increase"));
                    lines.Add(NewLine(GetAccountId(AccountMappingKeys.DeferredTaxLiability, DefaultGlAccounts.DeferredTaxLiability), 0m, deltaLiability, "Deferred tax liability increase"));
                }
                else
                {
                    var amount = Math.Abs(deltaLiability);
                    lines.Add(NewLine(GetAccountId(AccountMappingKeys.DeferredTaxLiability, DefaultGlAccounts.DeferredTaxLiability), amount, 0m, "Deferred tax liability decrease"));
                    lines.Add(NewLine(GetAccountId(AccountMappingKeys.IncomeTaxExpense, DefaultGlAccounts.IncomeTaxExpense), 0m, amount, "Deferred tax liability decrease"));
                }
            }

            if (lines.Count > 0)
            {
                var result = await _basisPosting.PostEntryAsync(
                    periodEnd,
                    "Deferred tax movement",
                    lines,
                    userId,
                    referenceNumber: null,
                    sourceModule: "DEFERRED_TAX");
                _ = result.IfrsEntry;
            }

            return deferred;
        }

        public async Task<TAX_RETURN> CreateTaxReturnAsync(
            string taxType,
            DateTime periodStart,
            DateTime periodEnd,
            string jurisdiction,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(taxType))
                throw new ArgumentNullException(nameof(taxType));
            if (string.IsNullOrWhiteSpace(jurisdiction))
                throw new ArgumentNullException(nameof(jurisdiction));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<TAX_TRANSACTION>("TAX_TRANSACTION", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TAX_TYPE", Operator = "=", FilterValue = taxType },
                new AppFilter { FieldName = "TAX_JURISDICTION", Operator = "=", FilterValue = jurisdiction },
                new AppFilter { FieldName = "TAX_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "TAX_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var transactions = await repo.GetAsync(filters);
            var totalTax = transactions?.Cast<TAX_TRANSACTION>().Sum(t => t.TAX_AMOUNT ?? 0m) ?? 0m;

            var taxReturn = new TAX_RETURN
            {
                TAX_RETURN_ID = Guid.NewGuid().ToString(),
                TAX_TYPE = taxType,
                RETURN_PERIOD_START = periodStart,
                RETURN_PERIOD_END = periodEnd,
                TOTAL_TAX_LIABILITY = totalTax,
                TAX_DUE = totalTax,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var returnRepo = await GetRepoAsync<TAX_RETURN>("TAX_RETURN", cn);
            await returnRepo.InsertAsync(taxReturn, userId);
            return taxReturn;
        }

        public async Task<TAX_TRANSACTION> TrackTemporaryDifferenceAsync(
            string differenceType, // e.g., "Depreciation", "BadDebt"
            decimal bookCarryingAmount,
            decimal taxBase,
            decimal statutoryRate,
            string jurisdiction,
            string userId,
            string? connectionName = null)
        {
             if (string.IsNullOrWhiteSpace(differenceType))
                throw new ArgumentNullException(nameof(differenceType));
             if (string.IsNullOrWhiteSpace(jurisdiction))
                throw new ArgumentNullException(nameof(jurisdiction));
             if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var difference = bookCarryingAmount - taxBase;
            var deferredTaxObj = difference * statutoryRate;

            // Determine if DTA or DTL
            // Asset < Tax Base => Deductible => DTA
            // Asset > Tax Base => Taxable => DTL
            // Liability < Tax Base => Taxable => DTL
            // Liability > Tax Base => Deductible => DTA
            // Assuming simplified asset logic here: Positive diff means Book > Tax (e.g. slower tax depreciation) -> DTL? 
            // Actually: Carrying Amount > Tax Base (Taxable Temp Diff) -> DTL.
            
            // We will record this as a TAX_TRANSACTION with specific type for tracking
            var tax = new TAX_TRANSACTION
            {
                TAX_TRANSACTION_ID = Guid.NewGuid().ToString(),
                TAX_TYPE = $"TEMP_DIFF_{differenceType}",
                TAX_DATE = DateTime.UtcNow,
                TAX_AMOUNT = deferredTaxObj, 
                TAX_JURISDICTION = jurisdiction,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow,
                REMARKS = $"Book: {bookCarryingAmount}, Tax: {taxBase}, Diff: {difference}"
            };

            var repo = await GetRepoAsync<TAX_TRANSACTION>("TAX_TRANSACTION", cn);
            await repo.InsertAsync(tax, userId);

            _logger?.LogInformation("Recorded temporary difference {Type}: {Diff}. Deferred Tax Impact: {Impact}", differenceType, difference, deferredTaxObj);
            
            return tax;
        }

        public async Task<EtrReconciliationDto> ReconcileETRAsync(
            DateTime periodStart,
            DateTime periodEnd,
            decimal preTaxIncome,
            decimal statutoryRate,
            string jurisdiction,
            string? connectionName = null)
        {
            var cn = connectionName ?? ConnectionName;
            
            // Get Current Tax Provision
            var repo = await GetRepoAsync<TAX_TRANSACTION>("TAX_TRANSACTION", cn);
             var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TAX_JURISDICTION", Operator = "=", FilterValue = jurisdiction },
                new AppFilter { FieldName = "TAX_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "TAX_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var transactions = await repo.GetAsync(filters);
            var currentTax = transactions?.Cast<TAX_TRANSACTION>()
                .Where(t => !t.TAX_TYPE.StartsWith("TEMP_DIFF")) // Exclude temp diff tracking
                .Sum(t => t.TAX_AMOUNT ?? 0m) ?? 0m;

            var expectedTax = preTaxIncome * statutoryRate;
            var difference = currentTax - expectedTax;
            var effectiveRate = preTaxIncome == 0 ? 0 : currentTax / preTaxIncome;

            return new EtrReconciliationDto
            {
                PreTaxIncome = preTaxIncome,
                StatutoryRate = statutoryRate,
                ExpectedTax = expectedTax,
                ActualTaxProvision = currentTax,
                EffectiveTaxRate = effectiveRate,
                Difference = difference
            };
        }

        public async Task<JOURNAL_ENTRY> AdjustPriorYearProvisionAsync(
            string taxYear,
            decimal returnAmount, // Actual from Tax Return
            decimal provisionAmount, // Estimated in prior year
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var delta = returnAmount - provisionAmount; // e.g. Return 110, Prov 100 -> Underprovision 10 (Expense)

            // Post adjustment
            // If delta > 0 (Underprovision): Debit Tax Expense, Credit Tax Payable/Cash
            // If delta < 0 (Overprovision): Debit Tax Payable, Credit Tax Expense
            
            var debitAccount = delta > 0 
                ? GetAccountId(AccountMappingKeys.IncomeTaxExpense, DefaultGlAccounts.IncomeTaxExpense)
                : GetAccountId(AccountMappingKeys.IncomeTaxPayable, DefaultGlAccounts.IncomeTaxPayable);
            
            var creditAccount = delta > 0
                ? GetAccountId(AccountMappingKeys.IncomeTaxPayable, DefaultGlAccounts.IncomeTaxPayable)
                : GetAccountId(AccountMappingKeys.IncomeTaxExpense, DefaultGlAccounts.IncomeTaxExpense);

            var adjustment = Math.Abs(delta);

            var result = await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                adjustment,
                $"Return-to-Provision Adjustment {taxYear}",
                userId,
                cn);

            return result.IfrsEntry ?? throw new InvalidOperationException("IFRS entry not created");
        }

        public class EtrReconciliationDto
        {
            public decimal PreTaxIncome { get; set; }
            public decimal StatutoryRate { get; set; }
            public decimal ExpectedTax { get; set; }
            public decimal ActualTaxProvision { get; set; }
            public decimal EffectiveTaxRate { get; set; }
            public decimal Difference { get; set; }
        }

        private static JOURNAL_ENTRY_LINE NewLine(string accountId, decimal debit, decimal credit, string description)
        {
            return new JOURNAL_ENTRY_LINE
            {
                GL_ACCOUNT_ID = accountId,
                DEBIT_AMOUNT = debit == 0m ? null : debit,
                CREDIT_AMOUNT = credit == 0m ? null : credit,
                DESCRIPTION = description
            };
        }

        private async Task<DEFERRED_TAX_BALANCE?> GetLatestDeferredTaxBalanceAsync(string cn)
        {
            var repo = await GetRepoAsync<DEFERRED_TAX_BALANCE>("DEFERRED_TAX_BALANCE", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<DEFERRED_TAX_BALANCE>()
                .OrderByDescending(x => x.PERIOD_END_DATE)
                .FirstOrDefault();
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



