using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.GeneralLedger
{
    /// <summary>
    /// Service for managing journal entry operations.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class JournalEntryService : IJournalEntryService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<JournalEntryService>? _logger;
        private readonly string _connectionName;
        private const string JOURNAL_ENTRY_TABLE = "JOURNAL_ENTRY";
        private const string JOURNAL_ENTRY_LINE_TABLE = "JOURNAL_ENTRY_LINE";
        private const string GL_ENTRY_TABLE = "GL_ENTRY";

        public JournalEntryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<JournalEntryService>? logger = null,
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
        public async Task<JOURNAL_ENTRY> CreateJournalEntryAsync(CreateJournalEntryRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.Lines == null || request.Lines.Count == 0)
                throw new ArgumentException("Journal entry must have at least one line.", nameof(request));

            // Validate double-entry: total debits must equal total credits
            decimal totalDebits = request.Lines.Sum(l => l.DebitAmount ?? 0m);
            decimal totalCredits = request.Lines.Sum(l => l.CreditAmount ?? 0m);

            if (Math.Abs(totalDebits - totalCredits) > 0.01m)
                throw new InvalidOperationException($"Journal entry is not balanced. Debits: {totalDebits}, Credits: {totalCredits}");

            var connName = connectionName ?? _connectionName;
            var entryRepo = await GetEntryRepositoryAsync(connName);
            var lineRepo = await GetLineRepositoryAsync(connName);

            var entry = new JOURNAL_ENTRY
            {
                JOURNAL_ENTRY_ID = Guid.NewGuid().ToString(),
                ENTRY_NUMBER = request.EntryNumber,
                ENTRY_DATE = request.EntryDate,
                ENTRY_TYPE = request.EntryType,
                STATUS = "Draft",
                DESCRIPTION = request.Description,
                REFERENCE_NUMBER = request.ReferenceNumber,
                SOURCE_MODULE = request.SourceModule,
                TOTAL_DEBIT = totalDebits,
                TOTAL_CREDIT = totalCredits,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            // Set common columns
            if (entry is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await entryRepo.InsertAsync(entry);

            // Create journal entry lines
            for (int i = 0; i < request.Lines.Count; i++)
            {
                var lineData = request.Lines[i];

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

                if (line is IPPDMEntity linePpdmEntity)
                {
                    await _commonColumnHandler.SetCommonColumnsForCreateAsync(linePpdmEntity, userId, connName);
                }

                await lineRepo.InsertAsync(line);
            }

            _logger?.LogDebug("Created journal entry {EntryNumber} with {LineCount} lines", request.EntryNumber, request.Lines.Count);
            return entry;
        }

        /// <summary>
        /// Gets a journal entry by ID.
        /// </summary>
        public async Task<JOURNAL_ENTRY?> GetJournalEntryAsync(string entryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(entryId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetEntryRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "=", FilterValue = entryId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<JOURNAL_ENTRY>().FirstOrDefault();
        }

        /// <summary>
        /// Gets journal entries by date range.
        /// </summary>
        public async Task<List<JOURNAL_ENTRY>> GetJournalEntriesByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetEntryRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ENTRY_DATE", Operator = ">=", FilterValue = startDate },
                new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = endDate },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<JOURNAL_ENTRY>().OrderBy(e => e.ENTRY_DATE).ToList();
        }

        /// <summary>
        /// Gets journal entries by account.
        /// </summary>
        public async Task<List<JOURNAL_ENTRY>> GetJournalEntriesByAccountAsync(string accountId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountId))
                return new List<JOURNAL_ENTRY>();

            var connName = connectionName ?? _connectionName;
            var lineRepo = await GetLineRepositoryAsync(connName);

            // Get journal entry IDs from lines
            var lineFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GL_ACCOUNT_ID", Operator = "=", FilterValue = accountId }
            };

            var lines = await lineRepo.GetAsync(lineFilters);
            var entryIds = lines.Cast<JOURNAL_ENTRY_LINE>().Select(l => l.JOURNAL_ENTRY_ID).Distinct().ToList();

            if (!entryIds.Any())
                return new List<JOURNAL_ENTRY>();

            var entryRepo = await GetEntryRepositoryAsync(connName);
            var entryFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "IN", FilterValue = string.Join(",", entryIds) }
            };

            if (startDate.HasValue)
                entryFilters.Add(new AppFilter { FieldName = "ENTRY_DATE", Operator = ">=", FilterValue = startDate.Value });
            if (endDate.HasValue)
                entryFilters.Add(new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = endDate.Value });

            var results = await entryRepo.GetAsync(entryFilters);
            return results.Cast<JOURNAL_ENTRY>().OrderBy(e => e.ENTRY_DATE).ToList();
        }

        /// <summary>
        /// Posts a journal entry (changes status to Posted and creates GL entries).
        /// </summary>
        public async Task<JOURNAL_ENTRY> PostJournalEntryAsync(string entryId, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(entryId))
                throw new ArgumentException("Entry ID is required.", nameof(entryId));

            var connName = connectionName ?? _connectionName;
            var entry = await GetJournalEntryAsync(entryId, connName);

            if (entry == null)
                throw new InvalidOperationException($"Journal entry {entryId} not found.");
            if (entry.STATUS == "Posted")
                throw new InvalidOperationException($"Journal entry {entryId} is already posted.");

            var entryRepo = await GetEntryRepositoryAsync(connName);
            var lineRepo = await GetLineRepositoryAsync(connName);
            var glEntryRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ENTRY), connName, GL_ENTRY_TABLE,
                null);

            // Get journal entry lines
            var lineFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "=", FilterValue = entryId }
            };

            var lines = await lineRepo.GetAsync(lineFilters);
            var journalLines = lines.Cast<JOURNAL_ENTRY_LINE>().ToList();

            // Create GL entries for each line
            foreach (var line in journalLines)
            {
                var glEntry = new GL_ENTRY
                {
                    GL_ENTRY_ID = Guid.NewGuid().ToString(),
                    JOURNAL_ENTRY_ID = entryId,
                    GL_ACCOUNT_ID = line.GL_ACCOUNT_ID,
                    ENTRY_DATE = entry.ENTRY_DATE,
                    DEBIT_AMOUNT = line.DEBIT_AMOUNT,
                    CREDIT_AMOUNT = line.CREDIT_AMOUNT,
                    DESCRIPTION = line.DESCRIPTION,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                if (glEntry is IPPDMEntity glPpdmEntity)
                {
                    await _commonColumnHandler.SetCommonColumnsForCreateAsync(glPpdmEntity, userId, connName);
                }

                await glEntryRepo.InsertAsync(glEntry);
            }

            // Update journal entry status
            entry.STATUS = "Posted";

            if (entry is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, userId, connName);
            }

            await entryRepo.UpdateAsync(entry);

            _logger?.LogDebug("Posted journal entry {EntryId}", entryId);
            return entry;
        }

        /// <summary>
        /// Reverses a journal entry.
        /// </summary>
        public async Task<JOURNAL_ENTRY> ReverseJournalEntryAsync(string entryId, string reason, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(entryId))
                throw new ArgumentException("Entry ID is required.", nameof(entryId));

            var connName = connectionName ?? _connectionName;
            var originalEntry = await GetJournalEntryAsync(entryId, connName);

            if (originalEntry == null)
                throw new InvalidOperationException($"Journal entry {entryId} not found.");

            // Get original lines
            var lines = await GetJournalEntryLinesAsync(entryId, connName);

            // Create reversal entry
            var reversalRequest = new CreateJournalEntryRequest
            {
                EntryNumber = $"{originalEntry.ENTRY_NUMBER}-REV",
                EntryDate = DateTime.UtcNow,
                EntryType = "Reversal",
                Description = $"Reversal of {originalEntry.ENTRY_NUMBER}: {reason}",
                ReferenceNumber = originalEntry.ENTRY_NUMBER,
                SourceModule = originalEntry.SOURCE_MODULE,
                Lines = lines.Select(l => new JournalEntryLineData
                {
                    GlAccountId = l.GL_ACCOUNT_ID,
                    DebitAmount = l.CREDIT_AMOUNT,
                    CreditAmount = l.DEBIT_AMOUNT,
                    Description = $"Reversal: {l.DESCRIPTION}"
                }).ToList()
            };

            var reversalEntry = await CreateJournalEntryAsync(reversalRequest, userId, connName);

            // Post the reversal immediately
            await PostJournalEntryAsync(reversalEntry.JOURNAL_ENTRY_ID, userId, connName);

            // Mark original entry as reversed
            originalEntry.STATUS = "Reversed";

            if (originalEntry is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, userId, connName);
            }

            var entryRepo = await GetEntryRepositoryAsync(connName);
            await entryRepo.UpdateAsync(originalEntry);

            _logger?.LogDebug("Reversed journal entry {EntryId}", entryId);
            return reversalEntry;
        }

        /// <summary>
        /// Gets journal entry lines for an entry.
        /// </summary>
        public async Task<List<JOURNAL_ENTRY_LINE>> GetJournalEntryLinesAsync(string entryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(entryId))
                return new List<JOURNAL_ENTRY_LINE>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetLineRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "JOURNAL_ENTRY_ID", Operator = "=", FilterValue = entryId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<JOURNAL_ENTRY_LINE>().OrderBy(l => l.LINE_NUMBER).ToList();
        }

        /// <summary>
        /// Closes an accounting period.
        /// </summary>
        public async Task<PeriodClosingResult> ClosePeriodAsync(DateTime periodEndDate, string userId, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var entryRepo = await GetEntryRepositoryAsync(connName);

            // Get all unposted entries for the period
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = periodEndDate },
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = "Draft" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var unpostedEntries = await entryRepo.GetAsync(filters);
            var entries = unpostedEntries.Cast<JOURNAL_ENTRY>().ToList();

            int postedCount = 0;
            foreach (var entry in entries)
            {
                try
                {
                    await PostJournalEntryAsync(entry.JOURNAL_ENTRY_ID, userId, connName);
                    postedCount++;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to post journal entry {EntryId} during period close", entry.JOURNAL_ENTRY_ID);
                }
            }

            return new PeriodClosingResult
            {
                ClosingId = Guid.NewGuid().ToString(),
                PeriodEndDate = periodEndDate,
                IsClosed = true,
                Status = "Closed",
                JournalEntriesCreated = postedCount,
                UserId = userId,
                ClosingDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Gets unposted journal entries.
        /// </summary>
        public async Task<List<JOURNAL_ENTRY>> GetUnpostedEntriesAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetEntryRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "!=", FilterValue = "Posted" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<JOURNAL_ENTRY>().OrderBy(e => e.ENTRY_DATE).ToList();
        }

        /// <summary>
        /// Approves a journal entry.
        /// </summary>
        public async Task<JournalEntryApprovalResult> ApproveJournalEntryAsync(string entryId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(entryId))
                throw new ArgumentException("Entry ID is required.", nameof(entryId));

            var connName = connectionName ?? _connectionName;
            var entry = await GetJournalEntryAsync(entryId, connName);

            if (entry == null)
                throw new InvalidOperationException($"Journal entry {entryId} not found.");

            entry.STATUS = "Approved";

            if (entry is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, approverId, connName);
            }

            var entryRepo = await GetEntryRepositoryAsync(connName);
            await entryRepo.UpdateAsync(entry);

            return new JournalEntryApprovalResult
            {
                JournalEntryId = entryId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        /// <summary>
        /// Gets repository for JOURNAL_ENTRY table.
        /// </summary>
        private async Task<PPDMGenericRepository> GetEntryRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(JOURNAL_ENTRY_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(JOURNAL_ENTRY);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, JOURNAL_ENTRY_TABLE,
                null);
        }

        /// <summary>
        /// Gets repository for JOURNAL_ENTRY_LINE table.
        /// </summary>
        private async Task<PPDMGenericRepository> GetLineRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(JOURNAL_ENTRY_LINE_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(JOURNAL_ENTRY_LINE);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, JOURNAL_ENTRY_LINE_TABLE,
                null);
        }
    }
}
