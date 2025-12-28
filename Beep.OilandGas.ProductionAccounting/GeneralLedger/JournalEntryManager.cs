using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.GeneralLedger
{
    /// <summary>
    /// Manages journal entries and ensures double-entry bookkeeping.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
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
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = loggerFactory?.CreateLogger<JournalEntryManager>();
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
                TOTAL_CREDIT = totalCredits,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            // Save journal entry to database
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
                    DESCRIPTION = lineData.Description,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

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
            entry.ROW_CHANGED_BY = userId;
            entry.ROW_CHANGED_DATE = DateTime.UtcNow;

            // Save to database
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

        /// <summary>
        /// Converts JOURNAL_ENTRY entity to dictionary for database storage.
        /// </summary>
        private Dictionary<string, object> ConvertJournalEntryToDictionary(JOURNAL_ENTRY entry)
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(entry.JOURNAL_ENTRY_ID)) dict["JOURNAL_ENTRY_ID"] = entry.JOURNAL_ENTRY_ID;
            if (!string.IsNullOrEmpty(entry.ENTRY_NUMBER)) dict["ENTRY_NUMBER"] = entry.ENTRY_NUMBER;
            if (entry.ENTRY_DATE.HasValue) dict["ENTRY_DATE"] = entry.ENTRY_DATE.Value;
            if (!string.IsNullOrEmpty(entry.ENTRY_TYPE)) dict["ENTRY_TYPE"] = entry.ENTRY_TYPE;
            if (!string.IsNullOrEmpty(entry.STATUS)) dict["STATUS"] = entry.STATUS;
            if (!string.IsNullOrEmpty(entry.DESCRIPTION)) dict["DESCRIPTION"] = entry.DESCRIPTION;
            if (!string.IsNullOrEmpty(entry.REFERENCE_NUMBER)) dict["REFERENCE_NUMBER"] = entry.REFERENCE_NUMBER;
            if (!string.IsNullOrEmpty(entry.SOURCE_MODULE)) dict["SOURCE_MODULE"] = entry.SOURCE_MODULE;
            if (entry.TOTAL_DEBIT.HasValue) dict["TOTAL_DEBIT"] = entry.TOTAL_DEBIT.Value;
            if (entry.TOTAL_CREDIT.HasValue) dict["TOTAL_CREDIT"] = entry.TOTAL_CREDIT.Value;
            if (!string.IsNullOrEmpty(entry.ACTIVE_IND)) dict["ACTIVE_IND"] = entry.ACTIVE_IND;
            if (!string.IsNullOrEmpty(entry.ROW_CREATED_BY)) dict["ROW_CREATED_BY"] = entry.ROW_CREATED_BY;
            if (entry.ROW_CREATED_DATE.HasValue) dict["ROW_CREATED_DATE"] = entry.ROW_CREATED_DATE.Value;
            if (!string.IsNullOrEmpty(entry.ROW_CHANGED_BY)) dict["ROW_CHANGED_BY"] = entry.ROW_CHANGED_BY;
            if (entry.ROW_CHANGED_DATE.HasValue) dict["ROW_CHANGED_DATE"] = entry.ROW_CHANGED_DATE.Value;
            return dict;
        }

        /// <summary>
        /// Converts dictionary to JOURNAL_ENTRY entity.
        /// </summary>
        private JOURNAL_ENTRY ConvertDictionaryToJournalEntry(Dictionary<string, object> dict)
        {
            var entry = new JOURNAL_ENTRY();
            if (dict.TryGetValue("JOURNAL_ENTRY_ID", out var entryId)) entry.JOURNAL_ENTRY_ID = entryId?.ToString();
            if (dict.TryGetValue("ENTRY_NUMBER", out var entryNumber)) entry.ENTRY_NUMBER = entryNumber?.ToString();
            if (dict.TryGetValue("ENTRY_DATE", out var entryDate)) entry.ENTRY_DATE = entryDate != null ? Convert.ToDateTime(entryDate) : (DateTime?)null;
            if (dict.TryGetValue("ENTRY_TYPE", out var entryType)) entry.ENTRY_TYPE = entryType?.ToString();
            if (dict.TryGetValue("STATUS", out var status)) entry.STATUS = status?.ToString();
            if (dict.TryGetValue("DESCRIPTION", out var description)) entry.DESCRIPTION = description?.ToString();
            if (dict.TryGetValue("REFERENCE_NUMBER", out var refNumber)) entry.REFERENCE_NUMBER = refNumber?.ToString();
            if (dict.TryGetValue("SOURCE_MODULE", out var sourceModule)) entry.SOURCE_MODULE = sourceModule?.ToString();
            if (dict.TryGetValue("TOTAL_DEBIT", out var totalDebit)) entry.TOTAL_DEBIT = totalDebit != null ? Convert.ToDecimal(totalDebit) : (decimal?)null;
            if (dict.TryGetValue("TOTAL_CREDIT", out var totalCredit)) entry.TOTAL_CREDIT = totalCredit != null ? Convert.ToDecimal(totalCredit) : (decimal?)null;
            if (dict.TryGetValue("ACTIVE_IND", out var activeInd)) entry.ACTIVE_IND = activeInd?.ToString();
            if (dict.TryGetValue("ROW_CREATED_BY", out var createdBy)) entry.ROW_CREATED_BY = createdBy?.ToString();
            if (dict.TryGetValue("ROW_CREATED_DATE", out var createdDate)) entry.ROW_CREATED_DATE = createdDate != null ? Convert.ToDateTime(createdDate) : (DateTime?)null;
            if (dict.TryGetValue("ROW_CHANGED_BY", out var changedBy)) entry.ROW_CHANGED_BY = changedBy?.ToString();
            if (dict.TryGetValue("ROW_CHANGED_DATE", out var changedDate)) entry.ROW_CHANGED_DATE = changedDate != null ? Convert.ToDateTime(changedDate) : (DateTime?)null;
            return entry;
        }

        /// <summary>
        /// Converts JOURNAL_ENTRY_LINE entity to dictionary for database storage.
        /// </summary>
        private Dictionary<string, object> ConvertJournalEntryLineToDictionary(JOURNAL_ENTRY_LINE line)
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(line.JOURNAL_ENTRY_LINE_ID)) dict["JOURNAL_ENTRY_LINE_ID"] = line.JOURNAL_ENTRY_LINE_ID;
            if (!string.IsNullOrEmpty(line.JOURNAL_ENTRY_ID)) dict["JOURNAL_ENTRY_ID"] = line.JOURNAL_ENTRY_ID;
            if (!string.IsNullOrEmpty(line.GL_ACCOUNT_ID)) dict["GL_ACCOUNT_ID"] = line.GL_ACCOUNT_ID;
            if (line.LINE_NUMBER.HasValue) dict["LINE_NUMBER"] = line.LINE_NUMBER.Value;
            if (line.DEBIT_AMOUNT.HasValue) dict["DEBIT_AMOUNT"] = line.DEBIT_AMOUNT.Value;
            if (line.CREDIT_AMOUNT.HasValue) dict["CREDIT_AMOUNT"] = line.CREDIT_AMOUNT.Value;
            if (!string.IsNullOrEmpty(line.DESCRIPTION)) dict["DESCRIPTION"] = line.DESCRIPTION;
            if (!string.IsNullOrEmpty(line.ACTIVE_IND)) dict["ACTIVE_IND"] = line.ACTIVE_IND;
            if (!string.IsNullOrEmpty(line.ROW_CREATED_BY)) dict["ROW_CREATED_BY"] = line.ROW_CREATED_BY;
            if (line.ROW_CREATED_DATE.HasValue) dict["ROW_CREATED_DATE"] = line.ROW_CREATED_DATE.Value;
            if (!string.IsNullOrEmpty(line.ROW_CHANGED_BY)) dict["ROW_CHANGED_BY"] = line.ROW_CHANGED_BY;
            if (line.ROW_CHANGED_DATE.HasValue) dict["ROW_CHANGED_DATE"] = line.ROW_CHANGED_DATE.Value;
            return dict;
        }

        /// <summary>
        /// Converts dictionary to JOURNAL_ENTRY_LINE entity.
        /// </summary>
        private JOURNAL_ENTRY_LINE ConvertDictionaryToJournalEntryLine(Dictionary<string, object> dict)
        {
            var line = new JOURNAL_ENTRY_LINE();
            if (dict.TryGetValue("JOURNAL_ENTRY_LINE_ID", out var lineId)) line.JOURNAL_ENTRY_LINE_ID = lineId?.ToString();
            if (dict.TryGetValue("JOURNAL_ENTRY_ID", out var entryId)) line.JOURNAL_ENTRY_ID = entryId?.ToString();
            if (dict.TryGetValue("GL_ACCOUNT_ID", out var glAccountId)) line.GL_ACCOUNT_ID = glAccountId?.ToString();
            if (dict.TryGetValue("LINE_NUMBER", out var lineNumber)) line.LINE_NUMBER = lineNumber != null ? Convert.ToInt32(lineNumber) : (int?)null;
            if (dict.TryGetValue("DEBIT_AMOUNT", out var debitAmount)) line.DEBIT_AMOUNT = debitAmount != null ? Convert.ToDecimal(debitAmount) : (decimal?)null;
            if (dict.TryGetValue("CREDIT_AMOUNT", out var creditAmount)) line.CREDIT_AMOUNT = creditAmount != null ? Convert.ToDecimal(creditAmount) : (decimal?)null;
            if (dict.TryGetValue("DESCRIPTION", out var description)) line.DESCRIPTION = description?.ToString();
            if (dict.TryGetValue("ACTIVE_IND", out var activeInd)) line.ACTIVE_IND = activeInd?.ToString();
            if (dict.TryGetValue("ROW_CREATED_BY", out var createdBy)) line.ROW_CREATED_BY = createdBy?.ToString();
            if (dict.TryGetValue("ROW_CREATED_DATE", out var createdDate)) line.ROW_CREATED_DATE = createdDate != null ? Convert.ToDateTime(createdDate) : (DateTime?)null;
            if (dict.TryGetValue("ROW_CHANGED_BY", out var changedBy)) line.ROW_CHANGED_BY = changedBy?.ToString();
            if (dict.TryGetValue("ROW_CHANGED_DATE", out var changedDate)) line.ROW_CHANGED_DATE = changedDate != null ? Convert.ToDateTime(changedDate) : (DateTime?)null;
            return line;
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

