using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Journal Entry Service - Posts double-entry accounting transactions to general ledger.
    /// Per GAAP: All transactions must balance (debits = credits).
    /// </summary>
    public class JournalEntryService : IJournalEntryService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<JournalEntryService> _logger;
        private const string ConnectionName = "PPDM39";

        public JournalEntryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<JournalEntryService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Creates a journal entry for a GL account.
        /// Double-entry accounting: debit one account, credit another.
        /// </summary>
        public async Task<JOURNAL_ENTRY> CreateEntryAsync(
            string glAccount,
            decimal amount,
            string description,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(glAccount))
                throw new ArgumentNullException(nameof(glAccount));
            if (amount == 0)
                throw new AccountingException("Journal entry amount cannot be zero");

            _logger?.LogInformation("Creating journal entry for GL account {Account}, amount: {Amount}",
                glAccount, amount);

            var entry = new JOURNAL_ENTRY
            {
                JOURNAL_ENTRY_ID = Guid.NewGuid().ToString(),
                ENTRY_NUMBER = GenerateEntryNumber(),
                ENTRY_DATE = DateTime.UtcNow,
                ENTRY_TYPE = amount > 0 ? "DEBIT" : "CREDIT",
                STATUS = JournalEntryStatus.Draft,
                DESCRIPTION = description,
                TOTAL_DEBIT = amount > 0 ? amount : 0,
                TOTAL_CREDIT = amount < 0 ? Math.Abs(amount) : 0,
                SOURCE_MODULE = "PRODUCTION_ACCOUNTING",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(JOURNAL_ENTRY);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "JOURNAL_ENTRY");

            await repo.InsertAsync(entry, userId);

            _logger?.LogInformation("Journal entry created: {EntryId} for account {Account}",
                entry.JOURNAL_ENTRY_ID, glAccount);

            return entry;
        }

        /// <summary>
        /// Gets all GL entries for an account in a date range.
        /// </summary>
        public async Task<List<GL_ENTRY>> GetEntriesByAccountAsync(
            string glAccount,
            DateTime start,
            DateTime end,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(glAccount))
                throw new ArgumentNullException(nameof(glAccount));

            _logger?.LogInformation("Getting GL entries for account {Account} from {StartDate} to {EndDate}",
                glAccount, start.ToShortDateString(), end.ToShortDateString());

            // In real implementation, would query GL_ENTRY table
            // For now, return empty list
            return new List<GL_ENTRY>();
        }

        /// <summary>
        /// Gets the balance of a GL account as of a date.
        /// </summary>
        public async Task<decimal> GetAccountBalanceAsync(
            string glAccount,
            DateTime asOfDate,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(glAccount))
                throw new ArgumentNullException(nameof(glAccount));

            _logger?.LogInformation("Getting balance for GL account {Account} as of {Date}",
                glAccount, asOfDate.ToShortDateString());

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "JOURNAL_ENTRY");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = JournalEntryStatus.Posted },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var entries = await repo.GetAsync(filters);
                var entryList = entries?.Cast<JOURNAL_ENTRY>().ToList() ?? new List<JOURNAL_ENTRY>();

                decimal balance = entryList.Sum(e => (e.TOTAL_DEBIT ?? 0) - (e.TOTAL_CREDIT ?? 0));

                _logger?.LogInformation("Account {Account} balance as of {Date}: {Balance}",
                    glAccount, asOfDate, balance);

                return balance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting account balance");
                throw;
            }
        }

        /// <summary>
        /// Validates a journal entry.
        /// Checks: debits = credits, date is valid, description set, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(JOURNAL_ENTRY entry, string cn = "PPDM39")
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            _logger?.LogInformation("Validating journal entry {EntryId}", entry.JOURNAL_ENTRY_ID);

            try
            {
                // Validation 1: Debits must equal credits
                decimal debits = entry.TOTAL_DEBIT ?? 0;
                decimal credits = entry.TOTAL_CREDIT ?? 0;

                if (Math.Abs(debits - credits) > 0.01m)
                {
                    _logger?.LogWarning("Journal entry {EntryId}: Debits {Debits} != Credits {Credits}",
                        entry.JOURNAL_ENTRY_ID, debits, credits);
                    throw new AccountingException($"Journal entry not balanced: Debits {debits} != Credits {credits}");
                }

                // Validation 2: Entry date should not be in future
                if (entry.ENTRY_DATE.HasValue && entry.ENTRY_DATE > DateTime.UtcNow)
                {
                    _logger?.LogWarning("Journal entry {EntryId}: Date is in the future", entry.JOURNAL_ENTRY_ID);
                    throw new AccountingException("Entry date cannot be in the future");
                }

                // Validation 3: Description should be set
                if (string.IsNullOrWhiteSpace(entry.DESCRIPTION))
                {
                    _logger?.LogWarning("Journal entry {EntryId}: Description is required", entry.JOURNAL_ENTRY_ID);
                    throw new AccountingException("Entry description is required");
                }

                _logger?.LogInformation("Journal entry {EntryId} validation passed", entry.JOURNAL_ENTRY_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Journal entry validation failed");
                throw;
            }
        }

        private string GenerateEntryNumber()
        {
            // Simple implementation: timestamp-based entry number
            return DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        }
    }

    /// <summary>
    /// Journal entry status constants.
    /// </summary>
    public static class JournalEntryStatus
    {
        public const string Draft = "DRAFT";
        public const string Posted = "POSTED";
        public const string Reversed = "REVERSED";
        public const string Void = "VOID";
    }
}
