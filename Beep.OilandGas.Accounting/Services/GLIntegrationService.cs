using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Accounting.Constants;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// GL integration service — posts accounting transactions to the General Ledger.
    /// Production and revenue posting use IJournalEntryService; other posting methods still return placeholders.
    /// Remaining implementations should:
    ///   1. Resolve GL accounts via _accountMapping
    ///   2. Create journal entries via _journalEntryService
    ///   3. Return the actual journal entry ID
    /// </summary>
    public class GLIntegrationService
    {
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

        public Task<string> PostTraditionalAccountingToGL(
            string entityId,
            string entityType,
            List<JournalEntryLineData> lines,
            DateTime? transactionDate,
            string userId)
        {
            _logger.LogWarning("GLIntegrationService.PostTraditionalAccountingToGL is a stub — returning placeholder ID. Entity: {EntityType}/{EntityId}", entityType, entityId);
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> PostRoyaltyToGL(
            string paymentId,
            decimal royaltyAmount,
            DateTime? transactionDate,
            string userId)
        {
            _logger.LogWarning("GLIntegrationService.PostRoyaltyToGL is a stub — returning placeholder ID. Payment: {PaymentId}, Amount: {Amount}", paymentId, royaltyAmount);
            return Task.FromResult(Guid.NewGuid().ToString());
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

        private async Task<string> PostRevenueEntryAsync(string referenceNumber, decimal amount, bool isCash,
            DateTime transactionDate, string userId, string sourceModule, string description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

            var lines = new List<JOURNAL_ENTRY_LINE>
            {
                new() { GL_ACCOUNT_ID = isCash ? DefaultGlAccounts.Cash : DefaultGlAccounts.AccountsReceivable,
                    DEBIT_AMOUNT = amount, CREDIT_AMOUNT = 0m, DESCRIPTION = description },
                new() { GL_ACCOUNT_ID = DefaultGlAccounts.Revenue,
                    DEBIT_AMOUNT = 0m, CREDIT_AMOUNT = amount, DESCRIPTION = description }
            };
            var entry = await _journalEntryService.CreateEntryAsync(transactionDate,
                description, lines, userId, referenceNumber, sourceModule, null);
            if (entry == null || string.IsNullOrWhiteSpace(entry.JOURNAL_ENTRY_ID))
                throw new InvalidOperationException("Journal creation did not return a persisted entry ID.");
            if (!await _journalEntryService.PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId))
                throw new InvalidOperationException($"Journal {entry.JOURNAL_ENTRY_ID} was not posted.");
            return entry.JOURNAL_ENTRY_ID;
        }

        public Task<string> PostFinancialAccountingToGL(
            string entityId,
            string accountingType,
            decimal amount,
            bool isCash,
            DateTime transactionDate,
            string userId)
        {
            _logger.LogWarning("GLIntegrationService.PostFinancialAccountingToGL is a stub — returning placeholder ID. Entity: {EntityId}, Type: {AccountingType}", entityId, accountingType);
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> PostCostToGL(
            string propertyId,
            decimal amount,
            bool isCapitalized,
            bool isCash,
            DateTime transactionDate,
            string userId)
        {
            _logger.LogWarning("GLIntegrationService.PostCostToGL is a stub — returning placeholder ID. Property: {PropertyId}, Amount: {Amount}", propertyId, amount);
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}
