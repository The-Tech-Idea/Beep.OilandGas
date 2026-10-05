using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Accounting.Constants;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// GL integration service — posts accounting transactions to the General Ledger through
    /// <see cref="IJournalEntryService"/>: every method creates a balanced journal entry, posts it, and answers the id of
    /// the posted entry. A posting that does not happen throws; nothing here answers an id for an entry that was not posted.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Four of these methods were stubs that logged a warning and answered a new GUID as though it were the
    /// posted journal's id, so the cost, amortization, successful-efforts and invoice endpoints reported a journal entry that
    /// did not exist. Each now posts. The financial accounting types are the ones the API posts; the accounts are this
    /// ledger's default chart (<see cref="DefaultGlAccounts"/>), as production and revenue posting already used.
    /// </remarks>
    public class GLIntegrationService
    {
        /// <summary>Capitalized amortization (DD&amp;A) of a property: Dr amortization expense, Cr accumulated amortization.</summary>
        public const string AmortizationExpense = "AmortizationExpense";

        /// <summary>Interest capitalized into development cost: Dr property, plant and equipment, Cr borrowing cost expense.</summary>
        public const string DevelopmentCost = "DevelopmentCost";

        /// <summary>Unproved property acquisition (IFRS 6 exploration and evaluation asset): Dr E&amp;E asset, Cr cash or payables.</summary>
        public const string UnprovedProperty = "UnprovedProperty";

        /// <summary>Proved property development: Dr property, plant and equipment, Cr cash or payables.</summary>
        public const string ProvedProperty = "ProvedProperty";

        /// <summary>Exploration expense (dry hole, G&amp;G): Dr exploration expense, Cr cash or payables.</summary>
        public const string ExplorationExpense = "ExplorationExpense";

        /// <summary>Impairment of an unproved property: Dr impairment loss, Cr the E&amp;E asset.</summary>
        public const string ImpairmentExpense = "ImpairmentExpense";

        private readonly IJournalEntryService _journalEntryService;
        private readonly GLAccountMappingService _accountMapping;
        private readonly ILogger<GLIntegrationService> _logger;

        public GLIntegrationService(
            IJournalEntryService journalEntryService,
            GLAccountMappingService accountMapping,
            ILogger<GLIntegrationService> logger)
        {
            _journalEntryService = journalEntryService;
            _accountMapping = accountMapping;
            _logger = logger;
        }

        /// <summary>
        /// Posts a traditional accounting document's lines (an invoice, a bill, an inventory movement) as one journal entry.
        /// The lines name GL accounts by account number; the journal service refuses an entry that is out of balance or that
        /// names an account the chart does not hold.
        /// </summary>
        public Task<string> PostTraditionalAccountingToGL(
            string entityId,
            string entityType,
            List<JournalEntryLineData> lines,
            DateTime? transactionDate,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
            ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
            ArgumentNullException.ThrowIfNull(lines);

            var journalLines = lines.Select(line => new JOURNAL_ENTRY_LINE
            {
                GL_ACCOUNT_ID = line.GlAccountId,
                DEBIT_AMOUNT = line.DebitAmount ?? 0m,
                CREDIT_AMOUNT = line.CreditAmount ?? 0m,
                DESCRIPTION = line.Description
            }).ToList();

            return PostEntryAsync(entityId, transactionDate ?? DateTime.UtcNow, userId, entityType,
                $"{entityType} {entityId}", journalLines);
        }

        /// <summary>Posts a royalty payment: Dr accrued royalties, Cr cash.</summary>
        public Task<string> PostRoyaltyToGL(
            string paymentId,
            decimal royaltyAmount,
            DateTime? transactionDate,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(paymentId);
            return PostTwoLineEntryAsync(paymentId, royaltyAmount, transactionDate ?? DateTime.UtcNow, userId, "ROYALTY",
                $"Royalty payment {paymentId}", DefaultGlAccounts.AccruedRoyalties, DefaultGlAccounts.Cash);
        }

        public Task<string> PostRevenueToGL(
            string transactionId,
            decimal amount,
            bool isCash,
            DateTime transactionDate,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
            return PostRevenueEntryAsync(transactionId, amount, isCash, transactionDate, userId,
                "REVENUE", $"Revenue for transaction {transactionId}");
        }

        public Task<string> PostProductionToGL(
            string ticketNumber,
            decimal amount,
            bool isCash,
            DateTime? transactionDate,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ticketNumber);
            return PostRevenueEntryAsync(ticketNumber, amount, isCash, transactionDate ?? DateTime.UtcNow,
                userId, "PRODUCTION", $"Production revenue for ticket {ticketNumber}");
        }

        private Task<string> PostRevenueEntryAsync(string referenceNumber, decimal amount, bool isCash,
            DateTime transactionDate, string userId, string sourceModule, string description) =>
            PostTwoLineEntryAsync(referenceNumber, amount, transactionDate, userId, sourceModule, description,
                isCash ? DefaultGlAccounts.Cash : DefaultGlAccounts.AccountsReceivable, DefaultGlAccounts.Revenue);

        /// <summary>
        /// Posts one of the financial accounting types this ledger knows (<see cref="AmortizationExpense"/>,
        /// <see cref="DevelopmentCost"/>, <see cref="UnprovedProperty"/>, <see cref="ProvedProperty"/>,
        /// <see cref="ExplorationExpense"/>, <see cref="ImpairmentExpense"/>) for a property.
        /// </summary>
        public Task<string> PostFinancialAccountingToGL(
            string entityId,
            string accountingType,
            decimal amount,
            bool isCash,
            DateTime transactionDate,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
            ArgumentException.ThrowIfNullOrWhiteSpace(accountingType);

            var settlement = isCash ? DefaultGlAccounts.Cash : DefaultGlAccounts.AccountsPayable;

            // The types are this class's constants, passed by the API's own code; any other value is the program's error.
            var (debit, credit) = accountingType switch
            {
                AmortizationExpense => (DefaultGlAccounts.AmortizationExpense, DefaultGlAccounts.AccumulatedAmortization),
                DevelopmentCost => (DefaultGlAccounts.FixedAssets, DefaultGlAccounts.BorrowingCostExpense),
                UnprovedProperty => (DefaultGlAccounts.ExplorationAsset, settlement),
                ProvedProperty => (DefaultGlAccounts.FixedAssets, settlement),
                ExplorationExpense => (DefaultGlAccounts.ExplorationExpense, settlement),
                ImpairmentExpense => (DefaultGlAccounts.ImpairmentLoss, DefaultGlAccounts.ExplorationAsset),
                _ => throw new InvalidOperationException($"'{accountingType}' is not a financial accounting type this ledger posts.")
            };

            return PostTwoLineEntryAsync(entityId, amount, transactionDate, userId, "FINANCIAL",
                $"{accountingType} for {entityId}", debit, credit);
        }

        /// <summary>
        /// Posts a cost: Dr property, plant and equipment when capitalized, else operating expense; Cr cash or payables.
        /// </summary>
        public Task<string> PostCostToGL(
            string propertyId,
            decimal amount,
            bool isCapitalized,
            bool isCash,
            DateTime transactionDate,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyId);
            return PostTwoLineEntryAsync(propertyId, amount, transactionDate, userId, "COST",
                $"{(isCapitalized ? "Capitalized" : "Expensed")} cost for {propertyId}",
                isCapitalized ? DefaultGlAccounts.FixedAssets : DefaultGlAccounts.OperatingExpense,
                isCash ? DefaultGlAccounts.Cash : DefaultGlAccounts.AccountsPayable);
        }

        private Task<string> PostTwoLineEntryAsync(string referenceNumber, decimal amount, DateTime transactionDate,
            string userId, string sourceModule, string description, string debitAccount, string creditAccount)
        {
            // The API posts after it has saved the record the entry is for, so an amount it did not check is its own
            // error, not the caller's: a guard, answered as a failure.
            if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

            var lines = new List<JOURNAL_ENTRY_LINE>
            {
                new() { GL_ACCOUNT_ID = debitAccount, DEBIT_AMOUNT = amount, CREDIT_AMOUNT = 0m, DESCRIPTION = description },
                new() { GL_ACCOUNT_ID = creditAccount, DEBIT_AMOUNT = 0m, CREDIT_AMOUNT = amount, DESCRIPTION = description }
            };
            return PostEntryAsync(referenceNumber, transactionDate, userId, sourceModule, description, lines);
        }

        private async Task<string> PostEntryAsync(string referenceNumber, DateTime transactionDate, string userId,
            string sourceModule, string description, List<JOURNAL_ENTRY_LINE> lines)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var entry = await _journalEntryService.CreateEntryAsync(transactionDate,
                description, lines, userId, referenceNumber, sourceModule, null);
            if (entry == null || string.IsNullOrWhiteSpace(entry.JOURNAL_ENTRY_ID))
                throw new InvalidOperationException("Journal creation did not return a persisted entry ID.");
            if (!await _journalEntryService.PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId))
                throw new InvalidOperationException($"Journal {entry.JOURNAL_ENTRY_ID} was not posted.");

            _logger.LogInformation("Posted journal {JournalEntryId} for {SourceModule} {Reference}",
                entry.JOURNAL_ENTRY_ID, sourceModule, referenceNumber);
            return entry.JOURNAL_ENTRY_ID;
        }
    }
}
