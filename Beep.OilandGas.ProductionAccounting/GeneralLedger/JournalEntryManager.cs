using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.GeneralLedger
{
    /// <summary>
    /// Manages journal entries and ensures double-entry bookkeeping.
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
    /// </summary>
    public class JournalEntryManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<JournalEntryManager>? _logger;
        private readonly string _connectionName;
        private const string JOURNAL_ENTRY_TABLE = "JOURNAL_ENTRY";
        private const string JOURNAL_ENTRY_LINE_TABLE = "JOURNAL_ENTRY_LINE";

        public JournalEntryManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<JournalEntryManager>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a journal entry with lines.
        /// </summary>
        public JOURNAL_ENTRY CreateJournalEntry(
            string entryNumber,
            DateTime entryDate,
            string entryType,
            string description,
            List<JournalEntryLineData> lines,
            string userId,
            string? connectionName = null)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Journal entry must have at least one line.", nameof(lines));

            // Validate double-entry: total debits must equal total credits
            decimal totalDebits = lines.Sum(l => l.DebitAmount ?? 0m);
            decimal totalCredits = lines.Sum(l => l.CreditAmount ?? 0m);

            if (Math.Abs(totalDebits - totalCredits) > 0.01m)
                throw new InvalidOperationException($"Journal entry is not balanced. Debits: {totalDebits}, Credits: {totalCredits}");

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var entry = new JOURNAL_ENTRY
            {
                JOURNAL_ENTRY_ID = Guid.NewGuid().ToString(),
                ENTRY_NUMBER = entryNumber,
                ENTRY_DATE = entryDate,
                ENTRY_TYPE = entryType,
                STATUS = "Draft",
                DESCRIPTION = description,
                TOTAL_DEBIT = totalDebits,
                TOTAL_CREDIT = totalCredits
            };

            // Prepare for insert and save journal entry to database
            _commonColumnHandler.PrepareForInsert(entry, userId);
            var result = dataSource.InsertEntity(JOURNAL_ENTRY_TABLE, entry);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create journal entry {EntryNumber}: {Error}", entryNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save journal entry: {errorMessage}");
            }

            // Save journal entry lines to database
            for (int i = 0; i < lines.Count; i++)
            {
                var lineData = lines[i];
                var line = new JOURNAL_ENTRY_LINE
                {
                    JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString(),
                    JOURNAL_ENTRY_ID = entry.JOURNAL_ENTRY_ID,
                    GL_ACCOUNT_ID = lineData.GlAccountId,
                    LINE_NUMBER = i + 1,
                    DEBIT_AMOUNT = lineData.DebitAmount,
                    CREDIT_AMOUNT = lineData.CreditAmount,
                    DESCRIPTION = lineData.Description
                };

                _commonColumnHandler.PrepareForInsert(line, userId);
                var lineResult = dataSource.InsertEntity(JOURNAL_ENTRY_LINE_TABLE, line);
                
                if (lineResult != null && lineResult.Errors != null && lineResult.Errors.Count > 0)
                {
                    var errorMessage = string.Join("; ", lineResult.Errors.Select(e => e.Message));
                    _logger?.LogError("Failed to create journal entry line {LineNumber} for entry {EntryNumber}: {Error}", i + 1, entryNumber, errorMessage);
                    throw new InvalidOperationException($"Failed to save journal entry line: {errorMessage}");
                }
            }

            _logger?.LogDebug("Created journal entry {EntryNumber} with {LineCount} lines in database", entryNumber, lines.Count);
            return entry;
        }

        /// <summary>
        /// Posts a journal entry (changes status to Posted).
        /// </summary>
        public void PostJournalEntry(string journalEntryId, string userId, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Get the entry first
            var entry = GetJournalEntry(journalEntryId, connName);
            if (entry == null)
                throw new KeyNotFoundException($"Journal entry not found: {journalEntryId}");

            // Update status
            entry.STATUS = "Posted";

            // Prepare for update and save to database
            _commonColumnHandler.PrepareForUpdate(entry, userId);
            var result = dataSource.UpdateEntity(JOURNAL_ENTRY_TABLE, entry);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to post journal entry {JournalEntryId}: {Error}", journalEntryId, errorMessage);
                throw new InvalidOperationException($"Failed to post journal entry: {errorMessage}");
            }

            _logger?.LogDebug("Posted journal entry {JournalEntryId}", journalEntryId);
        }

        /// <summary>
        /// Gets a journal entry by ID.
        /// </summary>
        public JOURNAL_ENTRY? GetJournalEntry(string journalEntryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(journalEntryId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "=", FilterValue = journalEntryId }
            };

            var results = dataSource.GetEntityAsync(JOURNAL_ENTRY_TABLE, filters).GetAwaiter().GetResult();
            var entryData = results?.FirstOrDefault();
            
            if (entryData == null)
                return null;

            return entryData as JOURNAL_ENTRY;
        }

        /// <summary>
        /// Gets all lines for a journal entry.
        /// </summary>
        public IEnumerable<JOURNAL_ENTRY_LINE> GetJournalEntryLines(string journalEntryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(journalEntryId))
                return Enumerable.Empty<JOURNAL_ENTRY_LINE>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "=", FilterValue = journalEntryId }
            };

            var results = dataSource.GetEntityAsync(JOURNAL_ENTRY_LINE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<JOURNAL_ENTRY_LINE>();

            return results.Cast<JOURNAL_ENTRY_LINE>().Where(l => l != null)!;
        }
    }

    /// <summary>
    /// Data class for creating journal entry lines.
    /// </summary>
    public class JournalEntryLineData
    {
        public string GlAccountId { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public string Description { get; set; }
    }
}
