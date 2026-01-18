using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
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
    public class JournalEntryService
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
            string? sourceModule = null)
        {
            if (lineItems == null || lineItems.Count == 0)
                throw new ArgumentException("Journal entry must have at least one line item", nameof(lineItems));

            _logger?.LogInformation("Creating journal entry with {LineCount} items", lineItems.Count);

            try
            {
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
                    TOTAL_DEBIT = totalDebit,
                    TOTAL_CREDIT = totalCredit,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                // Insert entry header
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "JOURNAL_ENTRY");

                await repo.InsertAsync(entry, userId);

                // Insert line items
                await InsertLineItemsAsync(entry.JOURNAL_ENTRY_ID, lineItems, userId);

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

                // Update status to POSTED
                entry.STATUS = "POSTED";
                entry.ROW_CHANGED_BY = userId;
                entry.ROW_CHANGED_DATE = DateTime.UtcNow;

                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "JOURNAL_ENTRY");

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

                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "JOURNAL_ENTRY");

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
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "JOURNAL_ENTRY");

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
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY_LINE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY_LINE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "JOURNAL_ENTRY_LINE");

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
        private async Task InsertLineItemsAsync(string journalEntryId, List<JOURNAL_ENTRY_LINE> lineItems, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY_LINE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY_LINE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "JOURNAL_ENTRY_LINE");

                int lineNumber = 1;
                foreach (var item in lineItems)
                {
                    item.JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString();
                    item.JOURNAL_ENTRY_ID = journalEntryId;
                    item.LINE_NUMBER = lineNumber;
                    item.ACTIVE_IND = "Y";
                    item.PPDM_GUID = Guid.NewGuid().ToString();
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
    }
}
