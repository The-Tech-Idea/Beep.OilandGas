using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Journal Entry Service - Double-entry posting with GL validation
    /// Enforces: Debits = Credits (golden rule of accounting)
    /// </summary>
    public class JournalEntryService : IJournalEntryService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly ILogger<JournalEntryService> _logger;
        private const string ConnectionName = "PPDM39";

        // Tolerance for debit = credit check (0.01%)
        private const decimal BalanceTolerance = 0.0001m;

        public JournalEntryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<JournalEntryService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a journal entry with line items
        /// Status: DRAFT (not yet posted)
        /// </summary>
        public async Task<JOURNAL_ENTRY> CreateEntryAsync(
            DateTime entryDate,
            string description,
            List<JOURNAL_ENTRY_LINE> lineItems,
            string userId,
            string? referenceNumber = null,
            string? sourceModule = null,
            string? bookId = null)
        {
            if (lineItems == null || lineItems.Count == 0)
                throw new ArgumentException("Journal entry must have at least one line item", nameof(lineItems));

            _logger?.LogInformation("Creating journal entry with {LineCount} items", lineItems.Count);

            try
            {
                var resolvedBookId = string.IsNullOrWhiteSpace(bookId) ? AccountingBooks.Ifrs : bookId;

                // Validate all accounts exist and are active
                foreach (var item in lineItems)
                {
                    if (!await _glAccountService.ValidateAccountAsync(item.GL_ACCOUNT_ID ?? string.Empty))
                        throw new InvalidOperationException($"GL account {item.GL_ACCOUNT_ID} is invalid or inactive");
                }

                // Calculate totals
                decimal totalDebit = lineItems.Sum(x => x.DEBIT_AMOUNT ?? 0m);
                decimal totalCredit = lineItems.Sum(x => x.CREDIT_AMOUNT ?? 0m);

                // ENFORCE THE GOLDEN RULE: Debits must equal Credits
                if (Math.Abs(totalDebit - totalCredit) > BalanceTolerance)
                {
                    _logger?.LogError("Journal entry out of balance: Debits {Debits}, Credits {Credits}",
                        totalDebit, totalCredit);
                    throw new InvalidOperationException(
                        $"Journal entry out of balance: Debits {totalDebit:C} != Credits {totalCredit:C}");
                }

                // Create journal entry header
                var entry = new JOURNAL_ENTRY
                {
                    JOURNAL_ENTRY_ID = Guid.NewGuid().ToString(),
                    ENTRY_NUMBER = await GenerateEntryNumberAsync(),
                    ENTRY_DATE = entryDate,
                    ENTRY_TYPE = "GENERAL",
                    STATUS = "DRAFT",
                    DESCRIPTION = description,
                    REFERENCE_NUMBER = referenceNumber,
                    SOURCE_MODULE = sourceModule ?? "MANUAL",
                    SOURCE = resolvedBookId,
                    REMARK = $"BOOK={resolvedBookId}",
                    TOTAL_DEBIT = totalDebit,
                    TOTAL_CREDIT = totalCredit,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                // Insert entry header
                var repo = await GetRepoAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY");

                await repo.InsertAsync(entry, userId);

                // Insert line items
                await InsertLineItemsAsync(entry.JOURNAL_ENTRY_ID, lineItems, userId, resolvedBookId);

                _logger?.LogInformation("Journal entry {EntryNumber} created with status DRAFT", entry.ENTRY_NUMBER);
                return entry;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating journal entry: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Create and post a journal entry for a single account, with AR as the offset.
        /// </summary>
        public async Task<JOURNAL_ENTRY> CreateEntryAsync(
            string glAccount,
            decimal amount,
            string description,
            string userId,
            string cn = "PPDM39",
            string? bookId = null)
        {
            if (string.IsNullOrWhiteSpace(glAccount))
                throw new ArgumentNullException(nameof(glAccount));
            if (amount == 0)
                throw new InvalidOperationException("Journal entry amount cannot be zero");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation("Creating journal entry for GL account {Account}, amount: {Amount}",
                glAccount, amount);

            var arAccountId = DefaultGlAccounts.AccountsReceivable;
            if (!await _glAccountService.ValidateAccountAsync(glAccount))
                throw new InvalidOperationException($"GL account not found or inactive: {glAccount}");
            if (!await _glAccountService.ValidateAccountAsync(arAccountId))
                throw new InvalidOperationException($"AR account not found or inactive: {arAccountId}");

            var absAmount = Math.Abs(amount);

            var lines = amount > 0
                ? new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = arAccountId, DEBIT_AMOUNT = absAmount, CREDIT_AMOUNT = 0m, DESCRIPTION = description },
                    new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = glAccount, DEBIT_AMOUNT = 0m, CREDIT_AMOUNT = absAmount, DESCRIPTION = description }
                }
                : new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = glAccount, DEBIT_AMOUNT = absAmount, CREDIT_AMOUNT = 0m, DESCRIPTION = description },
                    new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = arAccountId, DEBIT_AMOUNT = 0m, CREDIT_AMOUNT = absAmount, DESCRIPTION = description }
                };

            var entry = await CreateEntryAsync(
                DateTime.UtcNow,
                description,
                lines,
                userId,
                referenceNumber: null,
                sourceModule: "PRODUCTION_ACCOUNTING",
                bookId: bookId);

            await PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId);
            entry.STATUS = "POSTED";
            return entry;
        }

        /// <summary>
        /// Creates a balanced journal entry using explicit debit and credit accounts.
        /// </summary>
        public async Task<JOURNAL_ENTRY> CreateBalancedEntryAsync(
            string debitAccount,
            string creditAccount,
            decimal amount,
            string description,
            string userId,
            string cn = "PPDM39",
            string? bookId = null)
        {
            if (string.IsNullOrWhiteSpace(debitAccount))
                throw new ArgumentNullException(nameof(debitAccount));
            if (string.IsNullOrWhiteSpace(creditAccount))
                throw new ArgumentNullException(nameof(creditAccount));
            if (amount <= 0)
                throw new InvalidOperationException("Journal entry amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            if (!await _glAccountService.ValidateAccountAsync(debitAccount))
                throw new InvalidOperationException($"GL account not found or inactive: {debitAccount}");
            if (!await _glAccountService.ValidateAccountAsync(creditAccount))
                throw new InvalidOperationException($"GL account not found or inactive: {creditAccount}");

            var lines = new List<JOURNAL_ENTRY_LINE>
            {
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = debitAccount,
                    DEBIT_AMOUNT = amount,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = description
                },
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = creditAccount,
                    DEBIT_AMOUNT = 0m,
                    CREDIT_AMOUNT = amount,
                    DESCRIPTION = description
                }
            };

            var entry = await CreateEntryAsync(
                DateTime.UtcNow,
                description,
                lines,
                userId,
                referenceNumber: null,
                sourceModule: "PRODUCTION_ACCOUNTING",
                bookId: bookId);

            await PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId);
            entry.STATUS = "POSTED";
            return entry;
        }

        public async Task<(JOURNAL_ENTRY IfrsEntry, JOURNAL_ENTRY GaapEntry)> CreateDualBalancedEntryFromKeysAsync(
            string debitKey,
            string creditKey,
            decimal amount,
            string description,
            string userId,
            IAccountMappingService ifrsMapping,
            IAccountMappingService gaapMapping,
            string ifrsBookId = AccountingBooks.Ifrs,
            string gaapBookId = AccountingBooks.Gaap,
            string cn = "PPDM39")
        {
            if (ifrsMapping == null)
                throw new ArgumentNullException(nameof(ifrsMapping));
            if (gaapMapping == null)
                throw new ArgumentNullException(nameof(gaapMapping));

            var ifrsDebit = ifrsMapping.GetAccountId(debitKey);
            var ifrsCredit = ifrsMapping.GetAccountId(creditKey);
            var gaapDebit = gaapMapping.GetAccountId(debitKey);
            var gaapCredit = gaapMapping.GetAccountId(creditKey);

            var ifrsEntry = await CreateBalancedEntryAsync(
                ifrsDebit,
                ifrsCredit,
                amount,
                $"{description} (IFRS)",
                userId,
                cn,
                ifrsBookId);

            var gaapEntry = await CreateBalancedEntryAsync(
                gaapDebit,
                gaapCredit,
                amount,
                $"{description} (GAAP)",
                userId,
                cn,
                gaapBookId);

            return (ifrsEntry, gaapEntry);
        }

        /// <summary>
        /// Gets all GL entries for an account in a date range.
        /// </summary>
        public async Task<List<GL_ENTRY>> GetEntriesByAccountAsync(
            string glAccount,
            DateTime start,
            DateTime end,
            string cn = "PPDM39",
            string? bookId = null)
        {
            if (string.IsNullOrWhiteSpace(glAccount))
                throw new ArgumentNullException(nameof(glAccount));
            if (start > end)
                throw new ArgumentException("start must be <= end", nameof(start));

            _logger?.LogInformation("Getting GL entries for account {Account} from {StartDate} to {EndDate}",
                glAccount, start.ToShortDateString(), end.ToShortDateString());

            var repo = await GetRepoAsync<GL_ENTRY>("GL_ENTRY", cn);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GL_ACCOUNT_ID", Operator = "=", FilterValue = glAccount },
                new AppFilter { FieldName = "ENTRY_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            if (!string.IsNullOrWhiteSpace(bookId))
            {
                filters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = bookId });
            }

            var entries = await repo.GetAsync(filters);
            return entries?.Cast<GL_ENTRY>().OrderBy(e => e.ENTRY_DATE).ToList() ?? new List<GL_ENTRY>();
        }

        /// <summary>
        /// Gets the balance of a GL account as of a date.
        /// </summary>
        public async Task<decimal> GetAccountBalanceAsync(
            string glAccount,
            DateTime asOfDate,
            string cn = "PPDM39",
            string? bookId = null)
        {
            if (string.IsNullOrWhiteSpace(glAccount))
                throw new ArgumentNullException(nameof(glAccount));

            _logger?.LogInformation("Getting balance for GL account {Account} as of {Date}",
                glAccount, asOfDate.ToShortDateString());

            var repo = await GetRepoAsync<GL_ENTRY>("GL_ENTRY", cn);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GL_ACCOUNT_ID", Operator = "=", FilterValue = glAccount },
                new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            if (!string.IsNullOrWhiteSpace(bookId))
            {
                filters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = bookId });
            }

            var entries = await repo.GetAsync(filters);
            var entryList = entries?.Cast<GL_ENTRY>().ToList() ?? new List<GL_ENTRY>();

            return entryList.Sum(e => (e.DEBIT_AMOUNT ?? 0m) - (e.CREDIT_AMOUNT ?? 0m));
        }

        /// <summary>
        /// Validates a journal entry.
        /// </summary>
        public Task<bool> ValidateAsync(JOURNAL_ENTRY entry, string cn = "PPDM39")
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            // Use pattern matching to handle both nullable and non-nullable representations of TOTAL_DEBIT/TOTAL_CREDIT
            decimal debits = entry.TOTAL_DEBIT is decimal td ? td : 0m;
            decimal credits = entry.TOTAL_CREDIT is decimal tc ? tc : 0m;

            if (Math.Abs(debits - credits) > BalanceTolerance)
                throw new InvalidOperationException($"Journal entry not balanced: Debits {debits} != Credits {credits}");

            if (entry.ENTRY_DATE.HasValue && entry.ENTRY_DATE > DateTime.UtcNow)
                throw new InvalidOperationException("Entry date cannot be in the future");

            if (string.IsNullOrWhiteSpace(entry.DESCRIPTION))
                throw new InvalidOperationException("Entry description is required");

            return Task.FromResult(true);
        }

        /// <summary>
        /// Post a journal entry (DRAFT -> POSTED)
        /// Only POSTED entries affect GL balances
        /// </summary>
        public async Task<bool> PostEntryAsync(string journalEntryId, string userId)
        {
            if (string.IsNullOrWhiteSpace(journalEntryId))
                throw new ArgumentNullException(nameof(journalEntryId));

            _logger?.LogInformation("Posting journal entry: {EntryId}", journalEntryId);

            try
            {
                var entry = await GetEntryByIdAsync(journalEntryId);
                if (entry == null)
                    throw new InvalidOperationException($"Journal entry {journalEntryId} not found");

                if (entry.STATUS != "DRAFT")
                    throw new InvalidOperationException($"Journal entry must be in DRAFT status to post (current: {entry.STATUS})");

                var lineItems = await GetEntryLineItemsAsync(journalEntryId);
                if (lineItems.Count == 0)
                    throw new InvalidOperationException($"Journal entry {journalEntryId} has no line items");

                // Materialize GL_ENTRY rows so account-based queries and GL reporting work off posted entries.
                await InsertGlEntriesAsync(entry, lineItems, userId);

                // Update status to POSTED
                entry.STATUS = "POSTED";
                entry.ROW_CHANGED_BY = userId;
                entry.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = await GetRepoAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY");

                await repo.UpdateAsync(entry, userId);

                _logger?.LogInformation("Journal entry {EntryId} posted successfully", journalEntryId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error posting journal entry {EntryId}: {Message}", journalEntryId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Reverse a journal entry (creates reversing entries with flipped debits/credits)
        /// </summary>
        public async Task<JOURNAL_ENTRY> ReverseEntryAsync(string journalEntryId, string reverseReason, string userId)
        {
            if (string.IsNullOrWhiteSpace(journalEntryId))
                throw new ArgumentNullException(nameof(journalEntryId));

            _logger?.LogInformation("Reversing journal entry: {EntryId}", journalEntryId);

            try
            {
                var originalEntry = await GetEntryByIdAsync(journalEntryId);
                if (originalEntry == null)
                    throw new InvalidOperationException($"Journal entry {journalEntryId} not found");

                // Get original line items
                var originalLines = await GetEntryLineItemsAsync(journalEntryId);

                // Create reversing entries (flip debits and credits)
                var reversingLines = new List<JOURNAL_ENTRY_LINE>();
                foreach (var line in originalLines)
                {
                    reversingLines.Add(new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = line.GL_ACCOUNT_ID,
                        DEBIT_AMOUNT = line.CREDIT_AMOUNT,   // Flip
                        CREDIT_AMOUNT = line.DEBIT_AMOUNT,   // Flip
                        DESCRIPTION = $"Reversal: {line.DESCRIPTION}"
                    });
                }

                // Create reversing entry
                var reversingEntry = await CreateEntryAsync(
                    DateTime.UtcNow,
                    $"Reversal of entry {originalEntry.ENTRY_NUMBER}: {reverseReason}",
                    reversingLines,
                    userId,
                    $"REVERSE-{originalEntry.ENTRY_NUMBER}",
                    "REVERSAL");

                // Update original entry status to REVERSED
                originalEntry.STATUS = "REVERSED";
                originalEntry.ROW_CHANGED_BY = userId;
                originalEntry.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = await GetRepoAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY");

                await repo.UpdateAsync(originalEntry, userId);

                _logger?.LogInformation("Journal entry {EntryId} reversed with new entry {ReversalEntry}", 
                    journalEntryId, reversingEntry.ENTRY_NUMBER);
                return reversingEntry;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reversing journal entry {EntryId}: {Message}", journalEntryId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get journal entry by ID
        /// </summary>
        public async Task<JOURNAL_ENTRY?> GetEntryByIdAsync(string journalEntryId)
        {
            if (string.IsNullOrWhiteSpace(journalEntryId))
                return null;

            try
            {
                var repo = await GetRepoAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY");

                var entry = await repo.GetByIdAsync(journalEntryId);
                return entry as JOURNAL_ENTRY;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting journal entry {EntryId}", journalEntryId);
                return null;
            }
        }

        /// <summary>
        /// Get journal entry line items
        /// </summary>
        private async Task<List<JOURNAL_ENTRY_LINE>> GetEntryLineItemsAsync(string journalEntryId)
        {
            try
            {
                var repo = await GetRepoAsync<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "=", FilterValue = journalEntryId }
                };

                var lines = await repo.GetAsync(filters);
                return lines?.Cast<JOURNAL_ENTRY_LINE>().ToList() ?? new List<JOURNAL_ENTRY_LINE>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting line items for entry {EntryId}", journalEntryId);
                throw;
            }
        }

        /// <summary>
        /// Insert journal entry line items
        /// </summary>
        private async Task InsertLineItemsAsync(string journalEntryId, List<JOURNAL_ENTRY_LINE> lineItems, string userId, string? bookId)
        {
            try
            {
                var repo = await GetRepoAsync<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE");

                int lineNumber = 1;
                foreach (var item in lineItems)
                {
                    item.JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString();
                    item.JOURNAL_ENTRY_ID = journalEntryId;
                    item.LINE_NUMBER = lineNumber;
                    item.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
                    item.PPDM_GUID = Guid.NewGuid().ToString();
                    item.SOURCE = bookId;
                    if (string.IsNullOrWhiteSpace(item.REMARK) && !string.IsNullOrWhiteSpace(bookId))
                        item.REMARK = $"BOOK={bookId}";
                    item.ROW_CREATED_BY = userId;
                    item.ROW_CREATED_DATE = DateTime.UtcNow;

                    await repo.InsertAsync(item, userId);
                    lineNumber++;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inserting line items for entry {EntryId}", journalEntryId);
                throw;
            }
        }

        private async Task InsertGlEntriesAsync(JOURNAL_ENTRY header, List<JOURNAL_ENTRY_LINE> lineItems, string userId)
        {
            try
            {
                var repo = await GetRepoAsync<GL_ENTRY>("GL_ENTRY");

                foreach (var item in lineItems)
                {
                    var debit = item.DEBIT_AMOUNT ?? 0m;
                    var credit = item.CREDIT_AMOUNT ?? 0m;
                    if (debit == 0m && credit == 0m)
                        continue;

                    var glEntry = new GL_ENTRY
                    {
                        GL_ENTRY_ID = Guid.NewGuid().ToString(),
                        JOURNAL_ENTRY_ID = header.JOURNAL_ENTRY_ID,
                        GL_ACCOUNT_ID = item.GL_ACCOUNT_ID,
                        ENTRY_DATE = header.ENTRY_DATE ?? DateTime.UtcNow,
                        DEBIT_AMOUNT = debit == 0m ? null : debit,
                        CREDIT_AMOUNT = credit == 0m ? null : credit,
                        DESCRIPTION = string.IsNullOrWhiteSpace(item.DESCRIPTION) ? header.DESCRIPTION : item.DESCRIPTION,
                        REFERENCE_NUMBER = header.ENTRY_NUMBER,
                        SOURCE_MODULE = header.SOURCE_MODULE,
                        SOURCE = header.SOURCE,
                        REMARK = string.IsNullOrWhiteSpace(header.REMARK) ? item.REMARK : header.REMARK,
                        ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                        PPDM_GUID = Guid.NewGuid().ToString(),
                        ROW_CREATED_BY = userId,
                        ROW_CREATED_DATE = DateTime.UtcNow
                    };

                    await repo.InsertAsync(glEntry, userId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inserting GL entries for journal entry {EntryId}", header.JOURNAL_ENTRY_ID);
                throw;
            }
        }

        /// <summary>
        /// Generate next journal entry number
        /// </summary>
        private async Task<string> GenerateEntryNumberAsync()
        {
            try
            {
                // For now, use timestamp-based generation
                // In production, this would use a sequence or counter
                return $"JE-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating entry number");
                throw;
            }
        }
        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName = null)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connectionName ?? ConnectionName, tableName);
        }
    }
}
