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
    /// IFRS 16 lease accounting with ROU asset, lease liability, and periodic amortization.
    /// </summary>
    public class LeaseAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<LeaseAccountingService> _logger;
        private const string ConnectionName = "PPDM39";

        public LeaseAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<LeaseAccountingService> logger,
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

        public async Task<LEASE_CONTRACT> CreateLeaseContractAsync(
            string leaseId,
            string propertyId,
            string lessorBaId,
            DateTime commencementDate,
            int termMonths,
            decimal discountRate,
            string currencyCode,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (string.IsNullOrWhiteSpace(lessorBaId))
                throw new ArgumentNullException(nameof(lessorBaId));
            if (termMonths <= 0)
                throw new InvalidOperationException("Term months must be positive");
            if (discountRate <= 0m)
                throw new InvalidOperationException("Discount rate must be positive");
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ArgumentNullException(nameof(currencyCode));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var lease = new LEASE_CONTRACT
            {
                LEASE_ID = leaseId,
                PROPERTY_ID = propertyId,
                LESSOR_BA_ID = lessorBaId,
                COMMENCEMENT_DATE = commencementDate,
                TERM_MONTHS = termMonths,
                DISCOUNT_RATE = discountRate,
                CURRENCY_CODE = currencyCode,
                STATUS = "ACTIVE",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<LEASE_CONTRACT>("LEASE_CONTRACT", connectionName);
            await repo.InsertAsync(lease, userId);
            return lease;
        }

        public async Task<List<LEASE_PAYMENT>> AddLeasePaymentsAsync(
            string leaseId,
            IEnumerable<(DateTime PaymentDate, decimal Amount)> payments,
            string currencyCode,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (payments == null)
                throw new ArgumentNullException(nameof(payments));
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ArgumentNullException(nameof(currencyCode));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var repo = await GetRepoAsync<LEASE_PAYMENT>("LEASE_PAYMENT", connectionName);
            var created = new List<LEASE_PAYMENT>();

            foreach (var payment in payments)
            {
                if (payment.Amount <= 0m)
                    throw new InvalidOperationException("Payment amount must be positive");

                var leasePayment = new LEASE_PAYMENT
                {
                    LEASE_PAYMENT_ID = Guid.NewGuid().ToString(),
                    LEASE_ID = leaseId,
                    PAYMENT_DATE = payment.PaymentDate,
                    PAYMENT_AMOUNT = payment.Amount,
                    CURRENCY_CODE = currencyCode,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                await repo.InsertAsync(leasePayment, userId);
                created.Add(leasePayment);
            }

            return created;
        }

        public async Task<LEASE_ACCOUNTING_ENTRY> MeasureInitialLeaseAsync(
            string leaseId,
            DateTime measurementDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var lease = await GetLeaseAsync(leaseId, cn);
            if (lease == null)
                throw new InvalidOperationException($"Lease not found: {leaseId}");

            var payments = await GetLeasePaymentsAsync(leaseId, cn);
            if (payments.Count == 0)
                throw new InvalidOperationException("Lease payments are required to measure initial lease");

            var presentValue = CalculatePresentValue(
                payments.Where(p => p.PAYMENT_DATE >= measurementDate).ToList(),
                measurementDate,
                lease.DISCOUNT_RATE ?? 0m);

            var entry = await CreateLeaseAccountingEntryAsync(
                leaseId,
                measurementDate,
                presentValue,
                presentValue,
                lease.CURRENCY_CODE,
                userId,
                cn);

            var debitAccount = GetAccountId(AccountMappingKeys.RightOfUseAsset, DefaultGlAccounts.RightOfUseAsset);
            var creditAccount = GetAccountId(AccountMappingKeys.LeaseLiability, DefaultGlAccounts.LeaseLiability);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                presentValue,
                $"Initial lease measurement {leaseId}",
                userId,
                cn);

            return entry;
        }

        public async Task<LEASE_ACCOUNTING_ENTRY> RecordLeasePaymentAsync(
            string leaseId,
            DateTime paymentDate,
            decimal paymentAmount,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (paymentAmount <= 0m)
                throw new InvalidOperationException("Payment amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var lease = await GetLeaseAsync(leaseId, cn);
            if (lease == null)
                throw new InvalidOperationException($"Lease not found: {leaseId}");

            await AddLeasePaymentsAsync(
                leaseId,
                new[] { (paymentDate, paymentAmount) },
                lease.CURRENCY_CODE,
                userId,
                cn);

            var lastEntry = await GetLatestLeaseEntryAsync(leaseId, cn);
            var openingLiability = lastEntry?.LEASE_LIABILITY ?? 0m;
            if (openingLiability <= 0m)
                openingLiability = await CalculatePresentValueAsync(leaseId, paymentDate, cn);

            var lastDate = lastEntry?.MEASUREMENT_DATE ?? lease.COMMENCEMENT_DATE ?? paymentDate;
            var yearFraction = (decimal)(paymentDate - lastDate).TotalDays / 365m;
            var interest = openingLiability * (lease.DISCOUNT_RATE ?? 0m) * Math.Max(0m, yearFraction);
            var principal = Math.Max(0m, paymentAmount - interest);
            var remaining = Math.Max(0m, openingLiability + interest - paymentAmount);

            var entry = await CreateLeaseAccountingEntryAsync(
                leaseId,
                paymentDate,
                rouAsset: lastEntry?.ROU_ASSET,
                leaseLiability: remaining,
                lease.CURRENCY_CODE,
                userId,
                cn,
                interestExpense: interest);

            var lines = new List<JOURNAL_ENTRY_LINE>
            {
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.LeaseLiability, DefaultGlAccounts.LeaseLiability),
                    DEBIT_AMOUNT = principal,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = $"Lease principal {leaseId}"
                },
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.LeaseInterestExpense, DefaultGlAccounts.LeaseInterestExpense),
                    DEBIT_AMOUNT = interest,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = $"Lease interest {leaseId}"
                },
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash),
                    DEBIT_AMOUNT = 0m,
                    CREDIT_AMOUNT = paymentAmount,
                    DESCRIPTION = $"Lease payment {leaseId}"
                }
            };

            var paymentResult = await _basisPosting.PostEntryAsync(
                paymentDate,
                $"Lease payment {leaseId}",
                lines,
                userId,
                referenceNumber: null,
                sourceModule: "LEASE_PAYMENT");
            _ = paymentResult.IfrsEntry;

            return entry;
        }

        public async Task<LEASE_ACCOUNTING_ENTRY> RecordLeaseAmortizationAsync(
            string leaseId,
            DateTime amortizationDate,
            decimal amortizationAmount,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (amortizationAmount <= 0m)
                throw new InvalidOperationException("Amortization amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var lease = await GetLeaseAsync(leaseId, cn);
            if (lease == null)
                throw new InvalidOperationException($"Lease not found: {leaseId}");

            var lastEntry = await GetLatestLeaseEntryAsync(leaseId, cn);
            var openingRoi = lastEntry?.ROU_ASSET ?? 0m;
            var remainingRoi = Math.Max(0m, openingRoi - amortizationAmount);

            var entry = await CreateLeaseAccountingEntryAsync(
                leaseId,
                amortizationDate,
                remainingRoi,
                lastEntry?.LEASE_LIABILITY,
                lease.CURRENCY_CODE,
                userId,
                cn,
                amortizationExpense: amortizationAmount);

            var debitAccount = GetAccountId(AccountMappingKeys.LeaseAmortizationExpense, DefaultGlAccounts.LeaseAmortizationExpense);
            var creditAccount = GetAccountId(AccountMappingKeys.RightOfUseAsset, DefaultGlAccounts.RightOfUseAsset);

            var amortizationResult = await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                amortizationAmount,
                $"Lease amortization {leaseId}",
                userId,
                cn);
            var journal = amortizationResult.IfrsEntry;
            if (journal != null)
                journal.STATUS = "POSTED";

            return entry;
        }

        public async Task<LEASE_ACCOUNTING_ENTRY> RemeasureLeaseAsync(
            string leaseId,
            DateTime remeasurementDate,
            decimal newDiscountRate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (newDiscountRate <= 0m)
                throw new InvalidOperationException("New discount rate must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var lease = await GetLeaseAsync(leaseId, cn);
            if (lease == null)
                throw new InvalidOperationException($"Lease not found: {leaseId}");

            var lastEntry = await GetLatestLeaseEntryAsync(leaseId, cn);
            var currentLiability = lastEntry?.LEASE_LIABILITY ?? 0m;
            var currentRou = lastEntry?.ROU_ASSET ?? 0m;

            var allPayments = await GetLeasePaymentsAsync(leaseId, cn);
            var futurePayments = allPayments.Where(p => p.PAYMENT_DATE > remeasurementDate).ToList();
            
            var newLiability = CalculatePresentValue(futurePayments, remeasurementDate, newDiscountRate);
            var adjustment = newLiability - currentLiability;

            lease.DISCOUNT_RATE = newDiscountRate;
            lease.ROW_CHANGED_BY = userId;
            lease.ROW_CHANGED_DATE = DateTime.UtcNow;
            var leaseRepo = await GetRepoAsync<LEASE_CONTRACT>("LEASE_CONTRACT", cn);
            await leaseRepo.UpdateAsync(lease, userId);

            var entry = await CreateLeaseAccountingEntryAsync(
                leaseId,
                remeasurementDate,
                currentRou + adjustment,
                newLiability,
                lease.CURRENCY_CODE,
                userId,
                cn);

            var debitAccount = adjustment > 0 
                ? GetAccountId(AccountMappingKeys.RightOfUseAsset, DefaultGlAccounts.RightOfUseAsset)
                : GetAccountId(AccountMappingKeys.LeaseLiability, DefaultGlAccounts.LeaseLiability);
            
            var creditAccount = adjustment > 0
                ? GetAccountId(AccountMappingKeys.LeaseLiability, DefaultGlAccounts.LeaseLiability)
                : GetAccountId(AccountMappingKeys.RightOfUseAsset, DefaultGlAccounts.RightOfUseAsset);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                Math.Abs(adjustment),
                $"Lease remeasurement {leaseId} (CPI/Rate Change)",
                userId,
                cn);

            _logger?.LogInformation("Remeasured lease {LeaseId}. Adjustment: {Adjustment}", leaseId, adjustment);
            return entry;
        }

        public async Task<LEASE_ACCOUNTING_ENTRY> ModifyLeaseAsync(
            string leaseId,
            DateTime modificationDate,
            int newTermMonths,
            decimal newDiscountRate,
            string userId,
            string? connectionName = null)
        {
             if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (newTermMonths <= 0)
                throw new InvalidOperationException("New term must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var lease = await GetLeaseAsync(leaseId, cn);
            if (lease == null)
                throw new InvalidOperationException($"Lease not found: {leaseId}");

            var lastEntry = await GetLatestLeaseEntryAsync(leaseId, cn);
            var currentLiability = lastEntry?.LEASE_LIABILITY ?? 0m;
            var currentRou = lastEntry?.ROU_ASSET ?? 0m;

            lease.TERM_MONTHS = newTermMonths;
            lease.DISCOUNT_RATE = newDiscountRate;
            lease.ROW_CHANGED_BY = userId;
            lease.ROW_CHANGED_DATE = DateTime.UtcNow;
            
            var leaseRepo = await GetRepoAsync<LEASE_CONTRACT>("LEASE_CONTRACT", cn);
            await leaseRepo.UpdateAsync(lease, userId);

            var allPayments = await GetLeasePaymentsAsync(leaseId, cn);
            var futurePayments = allPayments.Where(p => p.PAYMENT_DATE > modificationDate).ToList();
            var newLiability = CalculatePresentValue(futurePayments, modificationDate, newDiscountRate);

            var adjustment = newLiability - currentLiability;

            var entry = await CreateLeaseAccountingEntryAsync(
                leaseId,
                modificationDate,
                currentRou + adjustment,
                newLiability,
                lease.CURRENCY_CODE,
                userId,
                cn);

             var debitAccount = adjustment > 0 
                ? GetAccountId(AccountMappingKeys.RightOfUseAsset, DefaultGlAccounts.RightOfUseAsset)
                : GetAccountId(AccountMappingKeys.LeaseLiability, DefaultGlAccounts.LeaseLiability);
            
            var creditAccount = adjustment > 0
                ? GetAccountId(AccountMappingKeys.LeaseLiability, DefaultGlAccounts.LeaseLiability)
                : GetAccountId(AccountMappingKeys.RightOfUseAsset, DefaultGlAccounts.RightOfUseAsset);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                Math.Abs(adjustment),
                $"Lease modification {leaseId}",
                userId,
                cn);

            _logger?.LogInformation("Modified lease {LeaseId}. Term: {Term}, Adjustment: {Adjustment}", leaseId, newTermMonths, adjustment);
            return entry;
        }

        public async Task<List<LeaseMaturityBucket>> GenerateMaturityAnalysisAsync(string? connectionName = null)
        {
            var cn = connectionName ?? ConnectionName;
            var leaseRepo = await GetRepoAsync<LEASE_CONTRACT>("LEASE_CONTRACT", cn);
            
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = "ACTIVE" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var leases = (await leaseRepo.GetAsync(filters)).Cast<LEASE_CONTRACT>().ToList();
            var buckets = new List<LeaseMaturityBucket>();

            var today = DateTime.Today;

            foreach (var lease in leases)
            {
                var payments = await GetLeasePaymentsAsync(lease.LEASE_ID, cn);
                var futurePayments = payments.Where(p => p.PAYMENT_DATE >= today).ToList();

                foreach (var p in futurePayments)
                {
                    if (!p.PAYMENT_DATE.HasValue) continue;

                    var yearDiff = (p.PAYMENT_DATE.Value - today).TotalDays / 365.0;
                    string bucket;

                    if (yearDiff <= 1) bucket = "Year 1";
                    else if (yearDiff <= 2) bucket = "Year 2";
                    else if (yearDiff <= 3) bucket = "Year 3";
                    else if (yearDiff <= 4) bucket = "Year 4";
                    else if (yearDiff <= 5) bucket = "Year 5";
                    else bucket = "Thereafter";

                    var existing = buckets.FirstOrDefault(b => b.Bucket == bucket);
                    if (existing == null)
                    {
                        existing = new LeaseMaturityBucket { Bucket = bucket, TotalUndiscountedCashFlows = 0m };
                        buckets.Add(existing);
                    }
                    existing.TotalUndiscountedCashFlows += p.PAYMENT_AMOUNT ?? 0m;
                    existing.LeaseCount++;
                }
            }

            return buckets.OrderBy(b => b.Bucket).ToList();
        }

        public class LeaseMaturityBucket
        {
            public string Bucket { get; set; } = string.Empty;
            public decimal TotalUndiscountedCashFlows { get; set; }
            public int LeaseCount { get; set; }
        }

        private async Task<LEASE_CONTRACT?> GetLeaseAsync(string leaseId, string cn)
        {
            var repo = await GetRepoAsync<LEASE_CONTRACT>("LEASE_CONTRACT", cn);
            var lease = await repo.GetByIdAsync(leaseId);
            return lease as LEASE_CONTRACT;
        }

        private async Task<List<LEASE_PAYMENT>> GetLeasePaymentsAsync(string leaseId, string cn)
        {
            var repo = await GetRepoAsync<LEASE_PAYMENT>("LEASE_PAYMENT", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<LEASE_PAYMENT>().OrderBy(p => p.PAYMENT_DATE).ToList() ?? new List<LEASE_PAYMENT>();
        }

        private async Task<LEASE_ACCOUNTING_ENTRY?> GetLatestLeaseEntryAsync(string leaseId, string cn)
        {
            var repo = await GetRepoAsync<LEASE_ACCOUNTING_ENTRY>("LEASE_ACCOUNTING_ENTRY", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<LEASE_ACCOUNTING_ENTRY>()
                .OrderByDescending(e => e.MEASUREMENT_DATE)
                .FirstOrDefault();
        }

        private async Task<decimal> CalculatePresentValueAsync(string leaseId, DateTime measurementDate, string cn)
        {
            var lease = await GetLeaseAsync(leaseId, cn);
            if (lease == null)
                return 0m;

            var payments = await GetLeasePaymentsAsync(leaseId, cn);
            return CalculatePresentValue(
                payments.Where(p => p.PAYMENT_DATE >= measurementDate).ToList(),
                measurementDate,
                lease.DISCOUNT_RATE ?? 0m);
        }

        private decimal CalculatePresentValue(
            List<LEASE_PAYMENT> payments,
            DateTime measurementDate,
            decimal discountRate)
        {
            if (discountRate <= 0m)
                return payments.Sum(p => p.PAYMENT_AMOUNT ?? 0m);

            decimal presentValue = 0m;
            foreach (var payment in payments)
            {
                var amount = payment.PAYMENT_AMOUNT ?? 0m;
                if (amount <= 0m || !payment.PAYMENT_DATE.HasValue)
                    continue;

                var years = (decimal)(payment.PAYMENT_DATE.Value.Date - measurementDate.Date).TotalDays / 365m;
                if (years <= 0m)
                {
                    presentValue += amount;
                    continue;
                }

                var factor = (decimal)Math.Pow((double)(1m + discountRate), (double)years);
                presentValue += amount / factor;
            }

            return Math.Round(presentValue, 2);
        }

        private async Task<LEASE_ACCOUNTING_ENTRY> CreateLeaseAccountingEntryAsync(
            string leaseId,
            DateTime measurementDate,
            decimal? rouAsset,
            decimal? leaseLiability,
            string currencyCode,
            string userId,
            string cn,
            decimal? interestExpense = null,
            decimal? amortizationExpense = null)
        {
            var entry = new LEASE_ACCOUNTING_ENTRY
            {
                LEASE_ACCOUNTING_ENTRY_ID = Guid.NewGuid().ToString(),
                LEASE_ID = leaseId,
                MEASUREMENT_DATE = measurementDate,
                ROU_ASSET = rouAsset,
                LEASE_LIABILITY = leaseLiability,
                INTEREST_EXPENSE = interestExpense,
                AMORTIZATION_EXPENSE = amortizationExpense,
                CURRENCY_CODE = currencyCode,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<LEASE_ACCOUNTING_ENTRY>("LEASE_ACCOUNTING_ENTRY", cn);
            await repo.InsertAsync(entry, userId);
            return entry;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName)
        {
            var cn = connectionName ?? ConnectionName;
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



